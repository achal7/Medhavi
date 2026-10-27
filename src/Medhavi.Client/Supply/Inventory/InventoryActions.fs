module Medhavi.Client.Supply.Inventory.InventoryActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Supply
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ActionExecution
open Medhavi.Client

let adjustSafetyStock: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some itemId ->
                let apiOpt =
                    match ctx.Resolve<InventoryPolicyApi>() with
                    | Some api -> Some api
                    | None ->
                        ctx.Resolve<DomainServices>()
                        |> Option.map(fun ds -> ds.Supply.ManageInventory.ManageInventoryApis.Policy)

                match apiOpt with
                | Some api ->
                    let req: UpdateInventoryPolicyReq =
                        {
                            ItemId = itemId
                            LocationId = "LOC-CENTRAL-WH"
                            TargetServiceLevel = Some 0.98m
                            ManualOverrideParameters = None
                        }

                    match! api.Update req with
                    | Ok _ ->
                        return
                            Ok(
                                ActionExecutionOutcome.Completed(
                                    $"Safety stock policy updated for item '{itemId}'. Buffer targets recalculated.",
                                    [
                                        InventoryStore.storeId
                                        DataStoreId.literal "supply-store"
                                        DataStoreId.literal "core-exception-store"
                                    ]
                                )
                            )
                    | Error err -> return Error err.Message
                | None -> return Error "InventoryPolicyApi capability is not connected."
            | None -> return Error "No inventory item selected for safety stock adjustment."
        }

let expedite: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some itemId ->
                let apiOpt =
                    match ctx.Resolve<InventoryPositionApi>() with
                    | Some api -> Some api
                    | None ->
                        ctx.Resolve<DomainServices>()
                        |> Option.map(fun ds -> ds.Supply.ManageInventory.ManageInventoryApis.Position)

                match apiOpt with
                | Some api ->
                    let req: UpdateInventoryPositionReq =
                        {
                            ItemId = itemId
                            LocationId = "LOC-CENTRAL-WH"
                            TriggeringEvidence = [ "EXPEDITE_REPLENISHMENT_WORKBENCH" ]
                        }

                    match! api.Update req with
                    | Ok _ ->
                        return
                            Ok(
                                ActionExecutionOutcome.Completed(
                                    $"Emergency replenishment requisition triggered for item '{itemId}'.",
                                    [
                                        InventoryStore.storeId
                                        DataStoreId.literal "supply-store"
                                        DataStoreId.literal "core-exception-store"
                                    ]
                                )
                            )
                    | Error err -> return Error err.Message
                | None -> return Error "InventoryPositionApi capability is not connected."
            | None -> return Error "No inventory item selected to expedite replenishment."
        }

let explainRisk: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some itemId -> return Ok(ActionExecutionOutcome.ExplainRequested(itemId, "InventoryRisk"))
            | None -> return Error "No inventory item selected to explain risk."
        }

let all: (ActionDefId * ActionHandler) list =
    [
        InventoryGridSchema.adjustSafetyStock.Id, adjustSafetyStock
        InventoryGridSchema.expedite.Id, expedite
        InventoryGridSchema.explainRisk.Id, explainRisk
    ]
