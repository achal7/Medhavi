module Medhavi.Client.Supply.Capacity.CapacityActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Supply
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ActionExecution
open Medhavi.Client

let addShift: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some resourceId ->
                let api = ctx.Domain.Supply.ManageCapacity.ManageCapacityApis.Position

                let req: UpdateCapacityPositionReq =
                    {
                        PhysicalResourceId = resourceId
                        TriggeringEvidence = [ "AUTHORIZE_WEEKEND_OVERTIME_8H" ]
                    }

                match! api.Update req with
                | Ok _ ->
                    return
                        Ok(
                            ActionExecutionOutcome.Completed(
                                $"+8h weekend overtime shift authorized for '{resourceId}'. Operating envelope rebalanced.",
                                [
                                    CapacityGrid.storeId
                                    DataStoreId.literal "supply-store"
                                    DataStoreId.literal "core-exception-store"
                                ]
                            )
                        )
                | Error err -> return Error err.Message
            | None -> return Error "No work center selected to authorize overtime shift."
        }

let maintenance: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some _ -> return Error "Operation not supported by backend ManageCapacity capability."
            | None -> return Error "No work center selected for maintenance."
        }

let reallocate: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some _ -> return Error "Operation not supported by backend ManageCapacity capability."
            | None -> return Error "No work center selected for workload reallocation."
        }

let explainBottleneck: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some resourceId -> return Ok(ActionExecutionOutcome.ExplainRequested(resourceId, "CapacityBottleneck"))
            | None -> return Error "No work center selected to explain bottleneck."
        }

let all: (ActionDefId * ActionHandler) list =
    [
        CapacityGrid.CapacityActions.addShift.Id, addShift
        CapacityGrid.CapacityActions.maintenance.Id, maintenance
        CapacityGrid.CapacityActions.reallocate.Id, reallocate
        CapacityGrid.CapacityActions.explainBottleneck.Id, explainBottleneck
    ]
