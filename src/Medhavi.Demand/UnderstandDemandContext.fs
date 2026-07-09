module Medhavi.Demand.UnderstandDemandContext

open System
open Medhavi.Contracts
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Observation
open Medhavi.Demand
open Medhavi.Demand.Application.UnderstandDemandWorkflow
open Medhavi.Demand.DemandObservation.Context
open Medhavi.Demand.PlanningScope.Context
open Medhavi.Demand.EnterpriseDemandPicture.Context
open Medhavi.Demand.DemandObservation.Model
open Medhavi.Demand.PlanningScope.Model
open Medhavi.Demand.EnterpriseDemandPicture.Model

type UnderstandDemandContext =
    { Workflow: UnderstandDemandWorkflow
      ObservationContext: DemandObservation.Context.ObservationContext
      PlanningScopeContext: PlanningScope.Context.PlanningScopeContext
      EdpContext: EnterpriseDemandPicture.Context.EdpContext
      Dispose: unit -> unit }

let create
    (obsRepo: Repository<DemandObservation, string, ObservationEvent>)
    (scopeRepo: Repository<PlanningScope, string, PlanningScopeEvent>)
    (edpRepo: Repository<EnterpriseDemandPicture, string, EdpEvent>)
    (forecastQueries: Medhavi.Contracts.Demand.ForecastPublication.ForecastPublicationQueries)
    (publishKnowledge: ArchitecturalKnowledge -> unit)
    =

    let getAdjustments (scopeId: PlanningScopeId) =
        task {
            let! res = obsRepo.GetAll()
            match res with
            | Error _ -> return Map.empty
            | Ok observations ->
                let scopeIdStr = PlanningScopeId.value scopeId
                let parts = scopeIdStr.Split('-')
                let periodOpt =
                    if parts.Length >= 6 then
                        let bucket = parts.[parts.Length - 3]
                        let year = int parts.[parts.Length - 2]
                        let num = int parts.[parts.Length - 1]
                        match bucket with
                        | "W" -> Some (PlanningPeriod.PlanningWeek(year, num))
                        | "D" -> Some (PlanningPeriod.PlanningDay (DateOnly(year, 1, 1).AddDays(num - 1)))
                        | "M" -> Some (PlanningPeriod.PlanningMonth(year, num))
                        | "Q" -> Some (PlanningPeriod.PlanningQuarter(year, num))
                        | _ -> None
                    else None
                
                match periodOpt with
                | None -> return Map.empty
                | Some period ->
                    let totalQtyVal =
                        observations
                        |> List.filter (fun o ->
                            o.PlanningScopeId = Some scopeId
                            && (Option.isSome o.PromotionRef || Option.isSome o.CampaignRef))
                        |> List.sumBy (fun o -> Quantity.value o.Quantity)
                    if totalQtyVal > 0m then
                        let qty =
                            match Quantity.create totalQtyVal with
                            | Ok q -> q
                            | Error err -> failwith err.Message
                        return Map.ofList [ period, qty ]
                    else
                        return Map.empty
        }

    let getOverrides (scopeId: PlanningScopeId) =
        task {
            let scopeIdStr = PlanningScopeId.value scopeId
            let! pubs = forecastQueries.Filter(fun p -> p.Status = "Published" && List.contains scopeIdStr p.PlanningScopeIds)
            match pubs |> List.sortByDescending (fun p -> p.Version) |> List.tryHead with
            | None -> return Map.empty
            | Some pub ->
                let overridesMap =
                    pub.Overrides
                    |> List.choose (fun o ->
                        pub.Forecasts
                        |> List.tryFind (fun f -> f.ForecastId = o.ForecastId)
                        |> Option.map (fun f ->
                            let qty =
                                match Quantity.create o.OverrideValue with
                                | Ok q -> q
                                | Error err -> failwith err.Message
                            f.PlanningPeriod, qty
                        )
                    )
                    |> Map.ofList
                return overridesMap
        }

    let obsCtx = DemandObservation.Context.create obsRepo publishKnowledge
    let scopeCtx = PlanningScope.Context.create scopeRepo publishKnowledge
    let edpCtx = EnterpriseDemandPicture.Context.create edpRepo getAdjustments getOverrides publishKnowledge
    let workflow = createUnderstandDemandWorkflow obsCtx.Commands scopeCtx.Commands edpCtx.Commands forecastQueries

    let dispose () =
        obsCtx.Dispose()
        scopeCtx.Dispose()
        edpCtx.Dispose()

    { Workflow = workflow
      ObservationContext = obsCtx
      PlanningScopeContext = scopeCtx
      EdpContext = edpCtx
      Dispose = dispose }
