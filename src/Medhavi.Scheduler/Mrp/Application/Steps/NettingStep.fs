module Medhavi.Scheduler.Mrp.Steps.NettingStep

open System
open System.Threading.Tasks
open Medhavi.SharedKernel
open Medhavi.Scheduler.Mrp.Domain
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Mrp.Domain.Errors
open Medhavi.Scheduler.Mrp.Pipeline
open Medhavi.Scheduler.Mrp.Domain.Algorithms

// ============================================================================
// INJECTED QUERY SIGNATURES
// ============================================================================

type OnHandQuery = SkuId -> StockingPointId -> Task<Quantity>
type InboundQuery = SkuId -> StockingPointId -> Timestamp -> Timestamp -> Task<(Timestamp * Quantity * bool * string) list>
type ReservationsQuery = SkuId -> StockingPointId -> Timestamp -> Timestamp -> Task<(Timestamp * Quantity * string) list>
type SafetyStockQuery = SkuId -> StockingPointId -> Task<Quantity>

// ============================================================================
// HELPERS
// ============================================================================

let adjustForFirmedPegs
    (skuId: SkuId)
    (spId: StockingPointId)
    (demands: MrpDemand list)
    (inbound: (Timestamp * Quantity * bool * string) list)
    (reservations: (Timestamp * Quantity * string) list)
    (firmedPegs: PeggingLink list)
    =
    let skuPegs =
        firmedPegs
        |> List.filter (fun peg -> 
            peg.Demand.SkuId = skuId && 
            peg.Demand.StockingPointId = spId &&
            peg.Status = Active && 
            peg.IsLocked)

    let mutable demandMap = demands |> List.map (fun d -> d.DemandId, Quantity.value d.Quantity) |> Map.ofList
    let mutable inboundMap = inbound |> List.map (fun (t, q, f, id) -> id, Quantity.value q) |> Map.ofList
    let mutable resMap = reservations |> List.map (fun (t, q, id) -> id, Quantity.value q) |> Map.ofList

    for peg in skuPegs do
        let pegQty = Quantity.value peg.PeggedQty
        
        match Map.tryFind peg.Demand.DemandId demandMap with
        | Some dQty ->
            let newDQty = max 0m (dQty - pegQty)
            demandMap <- Map.add peg.Demand.DemandId newDQty demandMap
        | None -> ()

        match peg.Target with
        | Supply s ->
            match Map.tryFind s.SupplyId inboundMap with
            | Some sQty ->
                let newSQty = max 0m (sQty - pegQty)
                inboundMap <- Map.add s.SupplyId newSQty inboundMap
            | None -> ()
        | Reservation r ->
            let rId =
                match r with
                | Material id -> id
                | Capacity id -> CapacityReservationId.value id
                | Transport id -> id
            match Map.tryFind rId resMap with
            | Some rQty ->
                let newRQty = max 0m (rQty - pegQty)
                resMap <- Map.add rId newRQty resMap
            | None -> ()

    let adjustedDemands =
        demands
        |> List.map (fun d ->
            let qty = Map.tryFind d.DemandId demandMap |> Option.defaultValue (Quantity.value d.Quantity) |> Quantity.clampToZero
            { d with Quantity = qty })
        |> List.filter (fun d -> Quantity.isPositive d.Quantity)

    let adjustedInbound =
        inbound
        |> List.map (fun (t, q, f, id) ->
            let qty = Map.tryFind id inboundMap |> Option.defaultValue (Quantity.value q) |> Quantity.clampToZero
            (t, qty, f, id))
        |> List.filter (fun (_, q, _, _) -> Quantity.isPositive q)

    let adjustedReservations =
        reservations
        |> List.map (fun (t, q, id) ->
            let qty = Map.tryFind id resMap |> Option.defaultValue (Quantity.value q) |> Quantity.clampToZero
            (t, qty, id))
        |> List.filter (fun (_, q, _) -> Quantity.isPositive q)

    adjustedDemands, adjustedInbound, adjustedReservations

// ============================================================================
// STEP EXECUTION
// ============================================================================

/// Create netting step with injected queries
let createStep
    (onHandQuery: OnHandQuery)
    (inboundQuery: InboundQuery)
    (reservationsQuery: ReservationsQuery)
    (safetyStockQuery: SafetyStockQuery)
    : MrpStepAsync<ExplodedComponent list, NetRequirement list> =

    fun components ctx ->
        task {
            let startTime = DateTimeOffset.UtcNow

            // Filter out phantom items
            let activeComponents =
                components
                |> List.filter (fun c -> not c.IsPhantom)

            // Group components by SKU + Node + StockingPoint
            let grouped =
                activeComponents
                |> List.groupBy (fun c -> (c.SkuId, c.NodeId, c.StockingPointId))

            // Run netting for each group in parallel
            let nettingTasks =
                grouped
                |> List.map (fun ((skuId, nodeId, spId), groupComponents) ->
                    task {
                        try
                            // Load inventory snapshot data
                            let! onHand = onHandQuery skuId spId
                            let! inbound = inboundQuery skuId spId ctx.StartDate ctx.EndDate
                            let! reservations = reservationsQuery skuId spId ctx.StartDate ctx.EndDate
                            let! safetyStock = safetyStockQuery skuId spId

                            // Map components to MrpDemand inputs for netting
                            let demands =
                                groupComponents
                                |> List.mapi (fun idx c ->
                                    { MrpDemand.DemandId =
                                        $"comp-{MrpRunId.value ctx.RunId}-{SkuId.value c.SkuId}-{idx}"
                                      SkuId = c.SkuId
                                      NodeId = c.NodeId
                                      StockingPointId = c.StockingPointId
                                      Quantity = c.RequiredQuantity
                                      RequiredDate = c.RequiredDate
                                      Source =
                                        Dependent(
                                            c.ParentSkuId
                                            |> Option.map SkuId.value
                                            |> Option.defaultValue ""
                                        )
                                      Priority = None })

                            // Pre-net using firmed pegs
                            let adjustedDemands, adjustedInbound, adjustedReservations =
                                adjustForFirmedPegs skuId spId demands inbound reservations ctx.FirmedPegs

                            let nettingInbound = adjustedInbound |> List.map (fun (t, q, f, id) -> (t, q, f))
                            let nettingReservations = adjustedReservations |> List.map (fun (t, q, id) -> (t, q))

                            // Execute pure netting logic
                            let netReqs, _proposals =
                                Netting.netDemands
                                    skuId
                                    nodeId
                                    spId
                                    onHand
                                    nettingInbound
                                    nettingReservations
                                    safetyStock
                                    adjustedDemands
                                    ctx.Policy.NettingPolicy

                            return Ok netReqs
                        with ex ->
                            return Error(InventoryQueryFailed(SkuId.value skuId, ex.Message))
                    })
            let! results = Task.WhenAll(nettingTasks)

            // Collect netting requirements and errors
            let netRequirements =
                results
                |> Array.choose (function
                    | Ok nrs -> Some nrs
                    | _ -> None)
                |> Array.collect List.toArray
                |> List.ofArray

            let errors =
                results
                |> Array.choose (function
                    | Error e -> Some e
                    | _ -> None)
                |> List.ofArray

            // Filter to requirements with active shortages
            let activeRequirements =
                netRequirements
                |> List.filter (fun nr -> Quantity.isPositive nr.NetRequirement)

            if
                not (List.isEmpty errors)
                && List.isEmpty netRequirements
            then
                return Error(Netting errors)
            else
                let endTime = DateTimeOffset.UtcNow
                let duration = endTime - startTime

                let updatedCtx =
                    ctx
                    |> MrpContext.addEvent (NettingCompleted(List.length activeRequirements))
                    |> MrpContext.updateTelemetry (fun t -> { t with NettingDuration = duration })
                    |> (fun c ->
                        errors
                        |> List.fold
                            (fun acc err ->
                                match err with
                                | InventoryQueryFailed(sku, msg) ->
                                    MrpContext.addWarning $"Netting inventory lookup failed for SKU {sku}: {msg}" acc
                                | _ -> acc)
                            c)

                return Ok(activeRequirements, updatedCtx)
        }
