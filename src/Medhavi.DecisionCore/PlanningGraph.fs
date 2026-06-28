namespace Medhavi.DecisionCore

open System

type PlanningNode =
    | MaterialNode of id: string * skuId: string * quantity: decimal
    | InventoryNode of id: string * locationId: string * onHand: decimal
    | DemandNode of id: string * demandId: string * quantity: decimal
    | SupplyNode of id: string * orderId: string * quantity: decimal
    | CapacityNode of id: string * resourceId: string * hours: decimal
    | TransportNode of id: string * laneId: string * cost: decimal
    | OperationNode of id: string * routingStep: int * duration: TimeSpan

type PlanningEdge =
    | Consumes of sourceId: string * targetId: string * quantity: decimal
    | Produces of sourceId: string * targetId: string * quantity: decimal
    | Requires of sourceId: string * targetId: string
    | Constrains of sourceId: string * targetId: string * limit: decimal

type PlanningGraph = {
    Nodes: Map<string, PlanningNode>
    Edges: PlanningEdge list
}

type GraphDelta = {
    AddedNodes: PlanningNode list
    RemovedNodeIds: string list
    AddedEdges: PlanningEdge list
    RemovedEdges: PlanningEdge list
}

module PlanningGraph =

    let empty = { Nodes = Map.empty; Edges = [] }

    let addNode (graph: PlanningGraph) (node: PlanningNode) =
        let id =
            match node with
            | MaterialNode (id, _, _) -> id
            | InventoryNode (id, _, _) -> id
            | DemandNode (id, _, _) -> id
            | SupplyNode (id, _, _) -> id
            | CapacityNode (id, _, _) -> id
            | TransportNode (id, _, _) -> id
            | OperationNode (id, _, _) -> id
        { graph with Nodes = Map.add id node graph.Nodes }

    let addEdge (graph: PlanningGraph) (edge: PlanningEdge) =
        { graph with Edges = edge :: graph.Edges }

    let applyDelta (graph: PlanningGraph) (delta: GraphDelta) =
        let graph' =
            (graph, delta.RemovedNodeIds)
            ||> List.fold (fun g id -> { g with Nodes = Map.remove id g.Nodes })
        let graph' =
            (graph', delta.AddedNodes)
            ||> List.fold addNode
        let graph' =
            (graph', delta.RemovedEdges)
            ||> List.fold (fun g edge -> { g with Edges = List.filter ((<>) edge) g.Edges }) // simplistic removal
        (graph', delta.AddedEdges)
        ||> List.fold addEdge

    let indexByNode (graph: PlanningGraph) = graph.Nodes

    let indexByEdge (graph: PlanningGraph) =
        graph.Edges
        |> List.groupBy (fun e ->
            match e with
            | Consumes (src, _, _) -> src
            | Produces (src, _, _) -> src
            | Requires (src, _) -> src
            | Constrains (src, _, _) -> src)
        |> Map.ofList
