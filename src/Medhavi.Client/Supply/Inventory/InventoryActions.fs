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
                let api = ctx.Domain.Supply.ManageInventory.ManageInventoryApis.Policy
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
                                    InventoryGrid.storeId
                                    DataStoreId.literal "supply-store"
                                    DataStoreId.literal "core-exception-store"
                                ]
                            )
                        )
                | Error err -> return Error err.Message
            | None -> return Error "No inventory item selected for safety stock adjustment."
        }

let expedite: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some itemId ->
                let api = ctx.Domain.Supply.ManageInventory.ManageInventoryApis.Position
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
                                    InventoryGrid.storeId
                                    DataStoreId.literal "supply-store"
                                    DataStoreId.literal "core-exception-store"
                                ]
                            )
                        )
                | Error err -> return Error err.Message
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
        InventoryGrid.InventoryActions.adjustSafetyStock.Id, adjustSafetyStock
        InventoryGrid.InventoryActions.expedite.Id, expedite
        InventoryGrid.InventoryActions.explainRisk.Id, explainRisk
    ]
