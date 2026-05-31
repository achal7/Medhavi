module Medhavi.Transport.Domain.TransportGraphAgg

open System
open Medhavi.Transport

// ─── Graph Representation ────────────────────────────────────────────────────
// The transport network is a directed weighted graph where:
//   - Nodes = StockingPoint / Node IDs (strings)
//   - Edges = TransportLegRef records (directed: Origin → Destination)

type TransportGraph =
    { Nodes: Set<string>
      // Adjacency list: from-node → list of outgoing legs
      Edges: Map<string, TransportLegRef list> }

let emptyGraph : TransportGraph =
    { Nodes = Set.empty
      Edges = Map.empty }

/// Add a transport leg to the graph
let addLeg (graph: TransportGraph) (leg: TransportLegRef) : TransportGraph =
    let newNodes = graph.Nodes |> Set.add leg.Origin |> Set.add leg.Destination
    let currentEdges =
        graph.Edges
        |> Map.tryFind leg.Origin
        |> Option.defaultValue []
    let newEdges = Map.add leg.Origin (leg :: currentEdges) graph.Edges
    { Nodes = newNodes; Edges = newEdges }

/// Build a graph from a list of transport leg refs
let buildGraph (legs: TransportLegRef list) : TransportGraph =
    legs |> List.fold addLeg emptyGraph

/// Get outgoing legs from a given node
let outgoingLegs (graph: TransportGraph) (node: string) : TransportLegRef list =
    graph.Edges |> Map.tryFind node |> Option.defaultValue []

// ─── Path Representation ─────────────────────────────────────────────────────

/// A raw path through the graph (list of legs in order)
type Path = { Legs: TransportLegRef list }

module Path =
    let empty = { Legs = [] }

    let lastNode (path: Path) : string option =
        path.Legs
        |> List.tryLast
        |> Option.map (fun l -> l.Destination)

    let firstNode (path: Path) : string option =
        path.Legs |> List.tryHead |> Option.map (fun l -> l.Origin)

    let totalLeadTimeMinutes (path: Path) : decimal =
        path.Legs |> List.sumBy (fun l -> l.LeadTimeMinutes)

    let totalFixedCost (path: Path) : decimal =
        path.Legs |> List.sumBy (fun l -> l.FixedCost)

    let totalReliability (path: Path) : decimal =
        path.Legs
        |> List.map (fun l -> l.Reliability |> Option.defaultValue 1.0m)
        |> List.fold (*) 1.0m

    let totalCO2 (path: Path) : decimal option =
        let co2s = path.Legs |> List.choose (fun l -> l.CO2PerUnit)
        if co2s.IsEmpty then None
        else Some(List.sum co2s)

    let hopCount (path: Path) : int = path.Legs.Length

    let containsNode (path: Path) (node: string) : bool =
        path.Legs |> List.exists (fun l -> l.Origin = node || l.Destination = node)

    let toItinerary (id: ItineraryId) (skuId: string option) (qty: decimal option) (path: Path) : Itinerary =
        let mutable offsetMinutes = 0.0m
        let hops =
            path.Legs
            |> List.map (fun leg ->
                let dep = offsetMinutes
                let arr = dep + leg.LeadTimeMinutes
                let hop =
                    { LegId              = leg.LegId
                      Origin             = leg.Origin
                      Destination        = leg.Destination
                      Mode               = leg.Mode
                      LeadTimeMinutes    = leg.LeadTimeMinutes
                      DepartureDateOffset = dep
                      ArrivalDateOffset   = arr
                      FixedCost          = leg.FixedCost
                      VariableCostPerUnit = leg.VariableCostPerUnit }
                offsetMinutes <- arr
                hop)

        let fromNode = path.Legs |> List.head |> fun l -> l.Origin
        let toNode   = path.Legs |> List.last |> fun l -> l.Destination

        let totalVarCost =
            match qty with
            | None -> None
            | Some q ->
                let varCosts =
                    path.Legs
                    |> List.choose (fun l -> l.VariableCostPerUnit |> Option.map (fun v -> v * q))
                if varCosts.IsEmpty then None else Some(List.sum varCosts)

        { Id                     = id
          SkuId                  = skuId
          FromNode               = fromNode
          ToNode                 = toNode
          Hops                   = hops
          TotalLeadTimeMinutes   = totalLeadTimeMinutes path
          TotalFixedCost         = totalFixedCost path
          TotalVariableCostPerUnit = path.Legs |> List.choose (fun l -> l.VariableCostPerUnit) |> (fun l -> if l.IsEmpty then None else Some(List.sum l))
          TotalCO2               = totalCO2 path
          TotalReliability       = totalReliability path
          HopCount               = hopCount path }

// ─── Yen's K-Shortest Paths ──────────────────────────────────────────────────
// Implements Yen's algorithm for finding K shortest (fastest) loopless paths.
// Sorting key: total lead time (minutes) ascending.

/// Dijkstra for a shortest path from source to sink in the graph,
/// avoiding certain nodes and edges (used internally by Yen's).
/// Returns None if no path exists.
let private dijkstra
    (graph: TransportGraph)
    (source: string)
    (sink: string)
    (removedNodes: Set<string>)
    (removedEdges: Set<string * string>)   // set of (legId, dummy) — actually (origin*dest) pairs per spur
    : Path option =

    // Priority queue as a sorted list: (cost, node, path)
    // We use a simple mutable list for clarity (adequate for small networks).
    let mutable frontier: (decimal * string * Path) list =
        [ (0.0m, source, Path.empty) ]

    let mutable visited: Set<string> = Set.empty
    let mutable result: Path option = None

    while not frontier.IsEmpty && result.IsNone do
        // Pick the element with the smallest cost
        let sorted = frontier |> List.sortBy (fun (c, _, _) -> c)
        let (cost, node, path) = sorted |> List.head
        frontier <- sorted |> List.tail

        if not (Set.contains node visited) then
            visited <- Set.add node visited

            if node = sink then
                result <- Some path
            elif not (Set.contains node removedNodes) then
                let outgoing =
                    outgoingLegs graph node
                    |> List.filter (fun leg ->
                        leg.Status
                        && not (Set.contains (leg.Origin, leg.Destination) removedEdges))

                for leg in outgoing do
                    if not (Set.contains leg.Destination visited) then
                        let newPath = { Legs = path.Legs @ [ leg ] }
                        let newCost = cost + leg.LeadTimeMinutes
                        frontier <- (newCost, leg.Destination, newPath) :: frontier

    result

/// Yen's K-Shortest Paths algorithm
/// Returns up to K loopless shortest paths from source to sink (sorted by lead time).
let kShortestPaths
    (graph: TransportGraph)
    (source: string)
    (sink: string)
    (k: int)
    (maxHops: int)
    : Path list =

    // A: list of found shortest paths
    let mutable a: Path list = []
    // B: candidate paths (potential k-th shortest paths)
    let mutable b: (decimal * Path) list = []

    // Find the 1st shortest path
    match dijkstra graph source sink Set.empty Set.empty with
    | None -> []
    | Some firstPath ->
        if firstPath.Legs.Length > maxHops then []
        else

        a <- [ firstPath ]

        for kIdx in 1 .. k - 1 do
            if a.Length >= k then ()
            else
                let prevPath = a |> List.last

                for i in 0 .. prevPath.Legs.Length - 1 do
                    let spurNode = prevPath.Legs.[i].Origin
                    let rootPath = { Legs = prevPath.Legs |> List.take i }

                    // Collect edges to remove: edges from spurNode used by root-equivalent paths in A
                    let removedEdgesSet =
                        a
                        |> List.choose (fun path ->
                            if path.Legs.Length > i
                               && path.Legs |> List.take i = rootPath.Legs then
                                let leg = path.Legs.[i]
                                Some(leg.Origin, leg.Destination)
                            else
                                None)
                        |> Set.ofList

                    // Remove all nodes in root (except spur node) from the graph
                    let removedNodes =
                        rootPath.Legs
                        |> List.map (fun l -> l.Origin)
                        |> Set.ofList
                        |> Set.remove spurNode

                    match dijkstra graph spurNode sink removedNodes removedEdgesSet with
                    | None -> ()
                    | Some spurPath ->
                        let totalPath = { Legs = rootPath.Legs @ spurPath.Legs }
                        if totalPath.Legs.Length <= maxHops then
                            let totalCost = Path.totalLeadTimeMinutes totalPath
                            // Only add if not already in A or B
                            let notInA = not (List.contains totalPath a)
                            let notInB = b |> List.forall (fun (_, p) -> p <> totalPath)
                            if notInA && notInB then
                                b <- (totalCost, totalPath) :: b

                match b with
                | [] -> ()
                | candidates ->
                    let (_, bestCandidate) = candidates |> List.minBy fst
                    b <- candidates |> List.filter (fun (_, p) -> p <> bestCandidate)
                    a <- a @ [ bestCandidate ]

        a
