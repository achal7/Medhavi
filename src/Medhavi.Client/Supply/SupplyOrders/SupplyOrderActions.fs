module Medhavi.Client.Supply.SupplyOrders.SupplyOrderActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Supply
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ActionExecution
open Medhavi.Client

type SupplyPlanningApi = SupplyPlanApi

let expedite: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some orderId ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Planned order '{orderId}' marked for expedited release and priority fulfillment.",
                            [ SupplyOrderStore.storeId
                              DataStoreId.literal "inventory-store"
                              DataStoreId.literal "core-exception-store" ]
                        )
                    )
            | None -> return Error "No planned order selected to expedite."
        }

let deExpedite: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some orderId ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Planned order '{orderId}' de-expedited and restored to baseline schedule.",
                            [ SupplyOrderStore.storeId
                              DataStoreId.literal "inventory-store" ]
                        )
                    )
            | None -> return Error "No planned order selected to de-expedite."
        }

let split: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some orderId ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Planned order '{orderId}' split into staggered production batches.",
                            [ SupplyOrderStore.storeId ]
                        )
                    )
            | None -> return Error "No planned order selected to split."
        }

let changeVendor: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some orderId ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Alternate supplier assigned to planned order '{orderId}'. Lead time re-benchmarked.",
                            [ SupplyOrderStore.storeId
                              DataStoreId.literal "supplier-collaboration-store" ]
                        )
                    )
            | None -> return Error "No planned order selected for supplier reassignment."
        }

let explainDelay: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some orderId ->
                return Ok(ActionExecutionOutcome.ExplainRequested(orderId, "SupplyOrderFeasibility"))
            | None -> return Error "No planned order selected to explain."
        }

let all: (ActionDefId * ActionHandler) list =
    [ SupplyOrderGridSchema.expedite.Id, expedite
      SupplyOrderGridSchema.deExpedite.Id, deExpedite
      SupplyOrderGridSchema.split.Id, split
      SupplyOrderGridSchema.changeVendor.Id, changeVendor
      SupplyOrderGridSchema.explainDelay.Id, explainDelay ]
