module Medhavi.Client.Core.Demand.DemandActions

open System
open Medhavi.Contracts.Core.Demand
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ActionExecution
open Medhavi.Client
open Medhavi.Client.Core.Demand

/// Authoritative Application Command Handlers for Enterprise Demand Orders.
let satisfyDemand: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Core.DemandManagement.Commands
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
                                [ DemandOrdersGrid.storeId; DataStoreId.literal "core-exception-store" ]
                            )
                        )
                | Error err -> return Error err.Message
            | None -> return Error "No demand order selected to satisfy."
        }

let cancelDemand: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Core.DemandManagement.Commands
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
                                [ DemandOrdersGrid.storeId; DataStoreId.literal "core-exception-store" ]
                            )
                        )
                | Error err -> return Error err.Message
            | None -> return Error "No demand order selected to cancel."
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
        DemandOrdersGrid.DemandGridActions.satisfy.Id, satisfyDemand
        DemandOrdersGrid.DemandGridActions.cancel.Id, cancelDemand
        DemandOrdersGrid.DemandGridActions.explainShortage.Id, explainShortage
    ]
