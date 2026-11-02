module Medhavi.Client.Promise.Backorders.BackorderActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Promise
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ActionExecution
open Medhavi.Client

let enqueue: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Promise.ManageBackorders.Backorder
            let id = $"BO-AUTO-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 10000L}"
            let req: EnqueueBackorderReq =
                { BackorderId = id
                  OrderLineId = "LINE-NORDIC-8002-01"
                  CustomerId = "CUST-NORDIC-MFG"
                  Item = "SKU-BATTERY-PACK"
                  Location = "LOC-WEST-HUB"
                  Quantity = 40.0m
                  EnqueuedTime = DateTimeOffset.UtcNow }

            match! api.Enqueue req with
            | Ok dto ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Line '{dto.OrderLineId}' enqueued into backorder recovery with ID '{dto.BackorderId}'.",
                            [ BackorderGrid.storeId ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let resolve: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some boId ->
                let api = ctx.Domain.Promise.ManageBackorders.Backorder
                let req: ResolveBackorderReq =
                    { BackorderId = boId
                      MatchedSupplyIdentifier = "SUP-REC-ARRIVED-W46"
                      ResolvedDeliveryDate = DateTimeOffset.UtcNow.AddDays(4.0)
                      ResolutionTime = DateTimeOffset.UtcNow }

                match! api.Resolve req with
                | Ok dto ->
                    let etaStr =
                        match dto.EstimatedReplenishmentDate with
                        | Some dt -> dt.ToString("yyyy-MM-dd")
                        | None -> "N/A"
                    let msg = sprintf "Backorder '%s' matched with incoming supply. Promised: %s." dto.BackorderId etaStr
                    return
                        Ok(
                            ActionExecutionOutcome.Completed(
                                msg,
                                [ BackorderGrid.storeId
                                  DataStoreId.literal "order-commitment-store" ]
                            )
                        )
                | Error err -> return Error err.Message
            | None -> return Error "No backorder record selected."
        }

let repromise: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some boId ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Autonomous re-promising calculation scheduled for backorder '{boId}'.",
                            [ BackorderGrid.storeId ]
                        )
                    )
            | None -> return Error "No backorder record selected."
        }

let all: (ActionDefId * ActionHandler) list =
    [ BackorderGrid.BackorderActions.enqueue.Id, enqueue
      BackorderGrid.BackorderActions.resolve.Id, resolve
      BackorderGrid.BackorderActions.repromise.Id, repromise ]

