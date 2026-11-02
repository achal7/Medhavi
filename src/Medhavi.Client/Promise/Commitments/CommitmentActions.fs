module Medhavi.Client.Promise.Commitments.CommitmentActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Promise
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ActionExecution
open Medhavi.Client

let establishCommitment: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Promise.EstablishCommitments.Commitments
            let id = $"CMT-AUTO-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 10000L}"
            let req: EstablishCommitmentReq =
                { CommitmentId = id
                  CustomerOrderIdentifier = $"ORD-AUTO-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 1000L}"
                  OrderLineId = $"LINE-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 1000L}-01"
                  CustomerId = "CUST-TITAN-CLOUD"
                  Item = "SKU-DRIVE-1TB"
                  DestinationLocation = "LOC-EAST-HUB"
                  CommittedDeliveryDate = DateTimeOffset.UtcNow.AddDays(7.0)
                  PromisedQuantity = 100.0m
                  ProjectedLandedCost = 142.50m
                  GrossMarginPct = 0.28m
                  DecisionConfidence = 0.95m
                  SourcingLegs =
                    [ { LegId = $"{id}-LEG-1"
                        OriginLocation = "LOC-CENTRAL-DC"
                        Item = "SKU-DRIVE-1TB"
                        SourcedQuantity = 100.0m
                        FulfillmentMode = "ExpressFreight"
                        EstimatedShipDate = DateTimeOffset.UtcNow.AddDays(5.0)
                        EstimatedDeliveryDate = DateTimeOffset.UtcNow.AddDays(7.0)
                        FreightCost = 250.0m
                        CarbonEmissionsKg = 42.0m } ]
                  EstablishmentTime = DateTimeOffset.UtcNow }

            match! api.Establish req with
            | Ok dto ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Order commitment '{dto.CommitmentId}' established. Revision: {dto.CurrentRevisionIndex}.",
                            [ CommitmentGrid.storeId
                              DataStoreId.literal "order-book-store" ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let reviseCommitment: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some cmtId ->
                let api = ctx.Domain.Promise.EstablishCommitments.Commitments
                let req: ReviseCommitmentReq =
                    { CommitmentId = cmtId
                      NewDeliveryDate = DateTimeOffset.UtcNow.AddDays(9.0)
                      PromisedQuantity = 100.0m
                      ProjectedLandedCost = 148.00m
                      GrossMarginPct = 0.25m
                      DecisionConfidence = 0.92m
                      SourcingLegs =
                        [ { LegId = $"{cmtId}-LEG-REV"
                            OriginLocation = "LOC-CENTRAL-DC"
                            Item = "SKU-DRIVE-1TB"
                            SourcedQuantity = 100.0m
                            FulfillmentMode = "StandardGround"
                            EstimatedShipDate = DateTimeOffset.UtcNow.AddDays(6.0)
                            EstimatedDeliveryDate = DateTimeOffset.UtcNow.AddDays(9.0)
                            FreightCost = 180.0m
                            CarbonEmissionsKg = 35.0m } ]
                      RevisionReason = "Customer agreed to delayed schedule to save freight cost"
                      RevisionTime = DateTimeOffset.UtcNow }

                match! api.Revise req with
                | Ok dto ->
                    return
                        Ok(
                            ActionExecutionOutcome.Completed(
                                $"Commitment '{dto.CommitmentId}' revised to Rev {dto.CurrentRevisionIndex}.",
                                [ CommitmentGrid.storeId ]
                            )
                        )
                | Error err -> return Error err.Message
            | None -> return Error "No commitment selected for revision."
        }

let confirmFulfillment: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some cmtId ->
                let api = ctx.Domain.Promise.EstablishCommitments.Commitments
                let req: ConfirmFulfillmentReq =
                    { CommitmentId = cmtId
                      ProofOfDeliveryTime = DateTimeOffset.UtcNow
                      DeliveredQuantity = 100.0m }

                match! api.ConfirmFulfillment req with
                | Ok dto ->
                    return
                        Ok(
                            ActionExecutionOutcome.Completed(
                                $"Proof of delivery recorded for commitment '{dto.CommitmentId}'. State: {dto.LifecycleState}.",
                                [ CommitmentGrid.storeId ]
                            )
                        )
                | Error err -> return Error err.Message
            | None -> return Error "No commitment selected."
        }

let cancelCommitment: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some cmtId ->
                let api = ctx.Domain.Promise.EstablishCommitments.Commitments
                let req: CancelCommitmentReq =
                    { CommitmentId = cmtId
                      CancellationReason = "Customer cancelled order before dispatch"
                      CancellationTime = DateTimeOffset.UtcNow }

                match! api.Cancel req with
                | Ok dto ->
                    return
                        Ok(
                            ActionExecutionOutcome.Completed(
                                $"Commitment '{dto.CommitmentId}' cancelled. Quota released.",
                                [ CommitmentGrid.storeId
                                  DataStoreId.literal "allocation-pool-store" ]
                            )
                        )
                | Error err -> return Error err.Message
            | None -> return Error "No commitment selected."
        }

let all: (ActionDefId * ActionHandler) list =
    [ CommitmentGrid.CommitmentActions.establishCommitment.Id, establishCommitment
      CommitmentGrid.CommitmentActions.reviseCommitment.Id, reviseCommitment
      CommitmentGrid.CommitmentActions.confirmFulfillment.Id, confirmFulfillment
      CommitmentGrid.CommitmentActions.cancelCommitment.Id, cancelCommitment ]

