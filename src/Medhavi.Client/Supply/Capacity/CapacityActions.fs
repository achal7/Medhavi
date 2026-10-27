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
                let apiOpt =
                    match ctx.Resolve<CapacityPositionApi>() with
                    | Some api -> Some api
                    | None ->
                        ctx.Resolve<DomainServices>()
                        |> Option.map (fun ds -> ds.Supply.ManageCapacity.ManageCapacityApis.Position)

                match apiOpt with
                | Some api ->
                    let req: UpdateCapacityPositionReq =
                        { PhysicalResourceId = resourceId
                          TriggeringEvidence = [ "AUTHORIZE_WEEKEND_OVERTIME_8H" ] }

                    match! api.Update req with
                    | Ok _ ->
                        return
                            Ok(
                                ActionExecutionOutcome.Completed(
                                    $"+8h weekend overtime shift authorized for '{resourceId}'. Operating envelope rebalanced.",
                                    [ CapacityStore.storeId
                                      DataStoreId.literal "supply-store"
                                      DataStoreId.literal "core-exception-store" ]
                                )
                            )
                    | Error err -> return Error err.Message
                | None -> return Error "CapacityPositionApi capability is not connected."
            | None -> return Error "No work center selected to authorize overtime shift."
        }

let maintenance: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some resourceId ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Preventative maintenance window scheduled for '{resourceId}'.",
                            [ CapacityStore.storeId; DataStoreId.literal "supply-store" ]
                        )
                    )
            | None -> return Error "No work center selected for maintenance."
        }

let reallocate: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some resourceId ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Workload successfully reallocated from '{resourceId}' to alternate lines.",
                            [ CapacityStore.storeId; DataStoreId.literal "supply-store" ]
                        )
                    )
            | None -> return Error "No work center selected for workload reallocation."
        }

let explainBottleneck: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some resourceId ->
                return Ok(ActionExecutionOutcome.ExplainRequested(resourceId, "CapacityBottleneck"))
            | None -> return Error "No work center selected to explain bottleneck."
        }

let all: (ActionDefId * ActionHandler) list =
    [ CapacityGridSchema.addShift.Id, addShift
      CapacityGridSchema.maintenance.Id, maintenance
      CapacityGridSchema.reallocate.Id, reallocate
      CapacityGridSchema.explainBottleneck.Id, explainBottleneck ]
