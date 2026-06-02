/// Netting Step — Computes net requirements from gross requirements using inventory projections
/// FP Pattern: Railway-Oriented Programming (ROP) with async pipelines
module Medhavi.Planning.Mrp.Steps.NettingStep

open System
open Medhavi.SharedKernel
open Medhavi.Planning.Mrp.Domain.Types
open Medhavi.Planning.Mrp.Domain.Errors
open Medhavi.Planning.Mrp.Pipeline.PipelineTypes
open Medhavi.Planning.Mrp.Domain.Algorithms

// ============================================================================
// INJECTED QUERY SIGNATURES
// ============================================================================

type OnHandQuery = SkuId -> StockingPointId -> Async<Quantity>
type InboundQuery = SkuId -> StockingPointId -> Timestamp -> Timestamp -> Async<(Timestamp * Quantity * bool) list>
type ReservationsQuery = SkuId -> StockingPointId -> Timestamp -> Timestamp -> Async<(Timestamp * Quantity) list>
type SafetyStockQuery = SkuId -> StockingPointId -> Async<Quantity>

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
        async {
            let startTime = DateTimeOffset.UtcNow

            // Filter out phantom items
            let activeComponents = components |> List.filter (fun c -> not c.IsPhantom)

            // Group components by SKU + Node + StockingPoint
            let grouped = activeComponents |> List.groupBy (fun c -> (c.SkuId, c.NodeId, c.StockingPointId))

            // Run netting for each group in parallel
            let! results =
                grouped
                |> List.map (fun ((skuId, nodeId, spId), groupComponents) ->
                    async {
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
                                    { MrpDemand.DemandId = $"comp-{MrpRunId.value ctx.RunId}-{SkuId.value c.SkuId}-{idx}"
                                      SkuId = c.SkuId
                                      NodeId = c.NodeId
                                      StockingPointId = c.StockingPointId
                                      Quantity = c.RequiredQuantity
                                      RequiredDate = c.RequiredDate
                                      Source = Dependent (c.ParentSkuId |> Option.map SkuId.value |> Option.defaultValue "")
                                      Priority = None })

                            // Execute pure netting logic
                            let netReqs, _proposals =
                                Netting.netDemands
                                    skuId
                                    nodeId
                                    spId
                                    onHand
                                    inbound
                                    reservations
                                    safetyStock
                                    demands
                                    ctx.Policy.NettingPolicy

                            return Ok netReqs
                        with ex ->
                            return Error (InventoryQueryFailed (SkuId.value skuId, ex.Message))
                    })
                |> Async.Parallel

            // Collect netting requirements and errors
            let netRequirements =
                results
                |> Array.choose (function Ok nrs -> Some nrs | _ -> None)
                |> Array.collect List.toArray
                |> List.ofArray

            let errors =
                results
                |> Array.choose (function Error e -> Some e | _ -> None)
                |> List.ofArray

            // Filter to requirements with active shortages
            let activeRequirements =
                netRequirements
                |> List.filter (fun nr -> Quantity.isPositive nr.NetRequirement)

            if not (List.isEmpty errors) && List.isEmpty netRequirements then
                return Error (Netting errors)
            else
                let endTime = DateTimeOffset.UtcNow
                let duration = endTime - startTime

                let updatedCtx =
                    ctx
                    |> MrpContext.addEvent (NettingCompleted (List.length activeRequirements))
                    |> MrpContext.updateTelemetry (fun t ->
                        { t with NettingDuration = duration })
                    |> (fun c ->
                        errors
                        |> List.fold (fun acc err ->
                            match err with
                            | InventoryQueryFailed (sku, msg) ->
                                MrpContext.addWarning $"Netting inventory lookup failed for SKU {sku}: {msg}" acc
                            | _ -> acc)
                            c)

                return Ok (activeRequirements, updatedCtx)
        }
