module Medhavi.Transport.Application.TransportAtpApp

open System
open Medhavi.Transport
open Medhavi.Transport.Domain.TransportGraphAgg

type TransportAtpConfig =
    { DefaultMaxHops: int
      DefaultMaxItineraries: int
      CacheTtlSeconds: int }

let defaultConfig : TransportAtpConfig =
    { DefaultMaxHops        = 4
      DefaultMaxItineraries = 5
      CacheTtlSeconds       = 300 } // 5-minute cache

// ─── Cache ───────────────────────────────────────────────────────────────────

type private CacheEntry =
    { Itineraries: Itinerary list
      CachedAt: DateTimeOffset }

/// Simple in-memory TTL cache for itineraries
type TransportItineraryCache(ttlSeconds: int) =
    let mutable cache: Map<string, CacheEntry> = Map.empty

    let key (fromNode: string) (toNode: string) (skuId: string option) =
        sprintf "%s->%s|%s" fromNode toNode (skuId |> Option.defaultValue "*")

    member _.TryGet(fromNode, toNode, skuId) =
        let k = key fromNode toNode skuId
        match Map.tryFind k cache with
        | None -> None
        | Some entry ->
            let age = (DateTimeOffset.UtcNow - entry.CachedAt).TotalSeconds
            if age > float ttlSeconds then
                None
            else
                Some entry.Itineraries

    member _.Set(fromNode, toNode, skuId, itineraries) =
        let k = key fromNode toNode skuId
        cache <- Map.add k { Itineraries = itineraries; CachedAt = DateTimeOffset.UtcNow } cache

    /// Invalidate all cache entries (called on leg schedule or capacity changes)
    member _.InvalidateAll() =
        cache <- Map.empty

    /// Invalidate entries that pass through a specific leg (origin/dest pair)
    member _.InvalidateForNodes(origin: string, dest: string) =
        // Simple approach: invalidate all entries that route through these nodes
        cache <-
            cache
            |> Map.filter (fun k _ ->
                not (k.Contains(origin) || k.Contains(dest)))

// ─── Leg Loader (provider function) ─────────────────────────────────────────

/// Function type for fetching active transport legs (injected from MasterData)
type GetActiveLegs = unit -> Async<TransportLegRef list>

// ─── Hazmat / Constraint Filter ──────────────────────────────────────────────

/// Regulatory / hazmat constraint codes considered hazardous
let private hazmatConstraints =
    Set.ofList [ "Hazmat"; "TemperatureControlled"; "Refrigerated" ]

/// Filter out paths that violate regulatory constraints for a given SKU
/// (stub: in Phase 6.7 this can be enriched with per-SKU constraint lookups)
let private filterByConstraints
    (allowHazmat: bool)
    (paths: Path list)
    : Path list =
    if allowHazmat then paths
    else
        paths
        |> List.filter (fun path ->
            path.Legs |> List.forall (fun _ ->
                // In a real system we'd cross-check the leg's constraint list
                // For MVP: allow all paths (constraints enforced at leg ingestion)
                true))

// ─── Cost Modeling ───────────────────────────────────────────────────────────

/// Compute total estimated cost for a path given quantity
let computeCost (path: Path) (quantity: decimal) : decimal =
    path.Legs
    |> List.sumBy (fun leg ->
        let fixed' = leg.FixedCost
        let variable = leg.VariableCostPerUnit |> Option.map ((*) quantity) |> Option.defaultValue 0.0m
        fixed' + variable)

/// Compute a composite score for ranking itineraries:
///   Score = w_time * normalizedTime + w_cost * normalizedCost - w_reliability * reliability
/// Lower score = better. Weights can be made configurable later.
let private scoreItinerary (itin: Itinerary) (qty: decimal) =
    let timeFactor     = float itin.TotalLeadTimeMinutes
    let costFactor     = float (itin.TotalFixedCost + (itin.TotalVariableCostPerUnit |> Option.map ((*) qty) |> Option.defaultValue 0.0m))
    let reliabFactor   = float itin.TotalReliability
    // Balanced: weight time 50%, cost 30%, subtract reliability 20%
    0.50 * timeFactor + 0.30 * costFactor - 0.20 * reliabFactor * 10000.0


let private rankItineraries (qty: decimal) (itineraries: Itinerary list) : Itinerary list =
    itineraries |> List.sortBy (fun itin -> scoreItinerary itin qty)


// ─── Main ATP Service ────────────────────────────────────────────────────────

type TransportAtpCapabilities =
    { /// Find K-shortest feasible itineraries between two nodes
      GetOptions: GetTransportOptionsReq -> Async<Result<TransportOption list, string>>
      /// Get the full itinerary graph (for display / debugging)
      GetGraph: unit -> Async<TransportGraph>
      /// Invalidate the itinerary cache (call on leg data changes)
      InvalidateCache: unit -> unit }

/// Build a TransportLegRef from the Contracts DTO
let private legRefFromDto (dto: Medhavi.Contracts.MasterData.Transport.TransportLeg) : TransportLegRef =
    { LegId              = dto.Id
      Origin             = dto.Origin
      Destination        = dto.Destination
      Mode               = dto.Mode
      LeadTimeMinutes    = dto.LeadTimeMinutes
      Capacity           = dto.Capacity
      CapacityUnit       = dto.CapacityUnit
      Reliability        = None  // Not in slim DTO; will be enriched if needed
      CO2PerUnit         = None
      FixedCost          = 0.0m  // Default; enrich from full leg data if available
      VariableCostPerUnit = None
      Status             = dto.Status }

let createCapabilities
    (getLegs: GetActiveLegs)
    (config: TransportAtpConfig)
    : TransportAtpCapabilities =

    let cache = TransportItineraryCache(config.CacheTtlSeconds)
    let mutable graphOpt: TransportGraph option = None

    let getOrBuildGraph () = async {
        match graphOpt with
        | Some g -> return g
        | None ->
            let! legs = getLegs ()
            let g = buildGraph legs
            graphOpt <- Some g
            return g
    }

    { GetOptions = fun req -> async {
        let fromNode = req.FromNode
        let toNode   = req.ToNode
        let skuId    = req.SkuId
        let qty      = req.RequiredQuantity |> Option.defaultValue 1.0m
        let maxHops  = req.MaxHops |> Option.defaultValue config.DefaultMaxHops
        let maxK     = req.MaxItineraries |> Option.defaultValue config.DefaultMaxItineraries

        // 1. Check cache
        match cache.TryGet(fromNode, toNode, skuId) with
        | Some cachedItins ->
            let options =
                cachedItins
                |> List.map (fun itin ->
                    let cost = itin.TotalFixedCost + (itin.TotalVariableCostPerUnit |> Option.map ((*) qty) |> Option.defaultValue 0.0m)
                    { Itinerary       = itin
                      EarliestDeparture = req.NeedByDate.AddMinutes(-(float itin.TotalLeadTimeMinutes))
                      EarliestArrival   = req.NeedByDate
                      EstimatedCost    = cost
                      CostBreakdown   = sprintf "Fixed: %M + Var: %M" itin.TotalFixedCost (cost - itin.TotalFixedCost)
                      ReliabilityScore = itin.TotalReliability
                      CO2Estimate      = itin.TotalCO2 |> Option.map ((*) qty)
                      IsPreferred      = false })
            return Ok options

        | None ->
            // 2. Build or use cached graph
            let! graph = getOrBuildGraph ()

            if fromNode = toNode then
                return Error (sprintf "Origin and destination cannot be the same: %s" fromNode)
            elif not (graph.Nodes.Contains fromNode) then
                return Error (sprintf "Origin node '%s' not found in transport network" fromNode)
            elif not (graph.Nodes.Contains toNode) then
                return Error (sprintf "Destination node '%s' not found in transport network" toNode)
            else
                // 3. Find K-shortest paths using Yen's algorithm
                let paths = kShortestPaths graph fromNode toNode maxK maxHops

                if paths.IsEmpty then
                    return Error (sprintf "No feasible transport route found from '%s' to '%s' within %d hops" fromNode toNode maxHops)
                else
                    // 4. Convert to itineraries
                    let itineraries =
                        paths
                        |> List.mapi (fun _ path ->
                            let id = ItineraryId.generate ()
                            Path.toItinerary id skuId (Some qty) path)

                    // 5. Cache the itineraries
                    cache.Set(fromNode, toNode, skuId, itineraries)

                    // 6. Build TransportOption list
                    let options =
                        itineraries
                        |> rankItineraries qty
                        |> List.mapi (fun i itin ->
                            let cost = itin.TotalFixedCost + (itin.TotalVariableCostPerUnit |> Option.map ((*) qty) |> Option.defaultValue 0.0m)
                            let departure = req.NeedByDate.AddMinutes(-(float itin.TotalLeadTimeMinutes))
                            { Itinerary        = itin
                              EarliestDeparture = departure
                              EarliestArrival   = req.NeedByDate
                              EstimatedCost     = cost
                              CostBreakdown     = sprintf "Fixed: %M + Var: %M | Hops: %d" itin.TotalFixedCost (cost - itin.TotalFixedCost) itin.HopCount
                              ReliabilityScore  = itin.TotalReliability
                              CO2Estimate       = itin.TotalCO2 |> Option.map ((*) qty)
                              IsPreferred       = (i = 0) })

                    return Ok options
      }

      GetGraph = fun () -> async {
          return! getOrBuildGraph ()
      }

      InvalidateCache = fun () ->
          cache.InvalidateAll()
          graphOpt <- None  // also rebuild the graph next time
    }
