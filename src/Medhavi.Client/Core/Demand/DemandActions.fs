module Medhavi.Client.Core.Demand.DemandActions

open System
open Medhavi.Contracts.Core.Demand
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ActionExecution
open Medhavi.Client

/// Authoritative Application Command Handlers for Enterprise Demand Orders.
let satisfyDemand: ActionHandler =
    fun ctx ->
        task {
            let apiOpt =
                match ctx.Resolve<DemandApi>() with
                | Some api -> Some api
                | None -> ctx.Resolve<DomainServices>() |> Option.map(fun ds -> ds.Core.DemandManagement.Commands)

            match apiOpt with
            | Some api ->
                match ctx.TargetKey with
                | Some demandId ->
                    let req: SatisfyDemandReq =
                        {
                            DemandId = demandId
                            SatisfactionTime = DateTimeOffset.Now
                        }

                    match! api.Satisfy req with
                    | Ok dto ->
                        return
                            Ok(
                                ActionExecutionOutcome.Completed(
                                    $"Demand '{dto.Id}' fulfilled and marked satisfied.",
                                    [ DemandStore.storeId; DataStoreId.literal "core-exception-store" ]
                                )
                            )
                    | Error err -> return Error err.Message
                | None -> return Error "No demand order selected to satisfy."
            | None -> return Error "DemandApi capability is not connected."
        }

let cancelDemand: ActionHandler =
    fun ctx ->
        task {
            let apiOpt =
                match ctx.Resolve<DemandApi>() with
                | Some api -> Some api
                | None -> ctx.Resolve<DomainServices>() |> Option.map(fun ds -> ds.Core.DemandManagement.Commands)

            match apiOpt with
            | Some api ->
                match ctx.TargetKey with
                | Some demandId ->
                    let req: CancelDemandReq =
                        {
                            DemandId = demandId
                            CancellationTime = DateTimeOffset.Now
                            Reason = "Cancelled by planner"
                        }

                    match! api.Cancel req with
                    | Ok dto ->
                        return
                            Ok(
                                ActionExecutionOutcome.Completed(
                                    $"Demand '{dto.Id}' cancelled in enterprise ledger.",
                                    [ DemandStore.storeId; DataStoreId.literal "core-exception-store" ]
                                )
                            )
                    | Error err -> return Error err.Message
                | None -> return Error "No demand order selected to cancel."
            | None -> return Error "DemandApi capability is not connected."
        }

let explainShortage: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some demandId -> return Ok(ActionExecutionOutcome.ExplainRequested(demandId, "Demand"))
            | None -> return Error "No demand order selected for shortage explanation."
        }

let all: (ActionDefId * ActionHandler) list =
    [
        DemandGridActions.satisfy.Id, satisfyDemand
        DemandGridActions.cancel.Id, cancelDemand
        DemandGridActions.explainShortage.Id, explainShortage
    ]
