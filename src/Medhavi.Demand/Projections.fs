namespace Medhavi.Demand

open System
open System.Threading.Tasks

// =============================================================================
// Demand Query Service — BC-scoped read model
// =============================================================================

/// Read-model query service for the Demand BC.
type DemandQueryService =
    {
        GetAllDemandLines: unit -> Task<DemandLine list>
        /// Get all demand lines for a plant within a date range (by RequestedDeliveryDate)
        GetDemandLines: string -> DateOnly -> DateOnly -> Task<DemandLine list>
        /// Get all demand lines belonging to a specific order
        GetByOrderId: string -> Task<DemandLine list>
        /// Get all open (unfulfilled) demand for a SKU at a specific stocking point
        GetOpenDemand: string -> string -> Task<DemandLine list>
        /// Get demand lines filtered by fulfillment status within a plant
        GetByStatus: DemandStatus -> string -> Task<DemandLine list>
    }

module Projections =
    open System
    open System.Threading.Tasks
    open Medhavi.SharedKernel
    open Medhavi.Infrastructure.Projections
    open Medhavi.SharedKernel.Projections
    open Medhavi.Demand.Domain.DemandLineAgg

    let evolveProjection (state: Map<string, DemandLine>) (evt: DemandLineEvent) =
        match evt with
        | DemandLineCreated dl -> Map.add dl.DemandLineId dl state
        | DemandLineFulfilled e ->
            match Map.tryFind e.DemandLineId state with
            | Some dl ->
                let updated = applyFulfilled dl e
                Map.add dl.DemandLineId updated state
            | None -> state

    let createProjectionAgent () =
        ProjectionAgent<Map<string, DemandLine>, DemandLineEvent>(evolveProjection, Map.empty, "DemandLineReadModel")

    let createDemandQueryService
        (agent: ProjectionAgent<Map<string, DemandLine>, DemandLineEvent>)
        : DemandQueryService =
        { GetDemandLines =
            fun plantId startDate endDate ->
                task {
                    let! all = agent.GetStateAsync()

                    return
                        all.Values
                        |> Seq.filter (fun l ->
                            let reqDate = DateOnly.FromDateTime(l.RequestedDeliveryDate.DateTime)
                            reqDate >= startDate && reqDate <= endDate)
                        |> Seq.toList
                }
          GetAllDemandLines =
            fun () ->
                task {
                    let! all = agent.GetStateAsync()

                    return all.Values |> Seq.toList
                }
          GetByOrderId =
            fun orderId ->
                task {
                    let! all = agent.GetStateAsync()

                    return
                        all.Values
                        |> Seq.filter (fun l -> l.DemandOrderId = orderId)
                        |> Seq.toList
                }
          GetOpenDemand =
            fun skuId stockingPointId ->
                task {
                    let! all = agent.GetStateAsync()

                    return
                        all.Values
                        |> Seq.filter (fun l ->
                            SkuId.value l.SkuId = skuId
                            && StockingPointId.value l.StockingPointId = stockingPointId
                            && l.Status = DemandStatus.Open)
                        |> Seq.toList
                }
          GetByStatus =
            fun status plantId ->
                task {
                    let! all = agent.GetStateAsync()

                    return
                        all.Values
                        |> Seq.filter (fun l -> l.Status = status)
                        |> Seq.toList
                } }
