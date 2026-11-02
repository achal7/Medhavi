/// CA-P-001 Ingest and Understand Orders Anti-Corruption Layer
module Medhavi.Promise.UnderstandOrders.OrderBookSnapshot.ACL

open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model

let parsePriorityClass (s: string) : Validation<OrderPriorityClass, DomainError> =
    match s.Trim().ToLowerInvariant() with
    | "critical" -> Valid OrderPriorityClass.Critical
    | "expedited" -> Valid OrderPriorityClass.Expedited
    | "standard" -> Valid OrderPriorityClass.Standard
    | "promotional" -> Valid OrderPriorityClass.Promotional
    | other -> Invalid [ DomainError.validation $"Invalid OrderPriorityClass: '{other}'" ]

let private validateOptionalTimestamp (tOpt: System.DateTimeOffset option) : Validation<Timestamp option, DomainError> =
    match tOpt with
    | Some t -> Some <!> validateTimestamp t
    | None -> Valid None

let toOrderLine (req: IngestOrderLineReq) : Validation<OrderLine, DomainError> =
    let validateQty =
        UnitOfMeasureId.create req.UnitOfMeasure
        |> Result.bind (fun uom -> Quantity.createWithUoM req.RequestedQuantity uom)
        |> Result.mapError (fun e -> DomainError.validation $"Invalid Quantity: {e}")
        |> fromResult

    let validateUom =
        UnitOfMeasureId.create req.UnitOfMeasure
        |> Result.mapError (fun e -> DomainError.validation $"Invalid UnitOfMeasure: {e}")
        |> fromResult

    let create lineId item qty uom reqDate earliest latest dest ref =
        { OrderLineId = lineId
          Item = item
          RequestedQuantity = qty
          UnitOfMeasure = uom
          RequestedDeliveryDate = reqDate
          EarliestAcceptableDate = earliest
          LatestAcceptableDate = latest
          DestinationLocation = dest
          ExternalLineReference = ref
          ValidationStatus = LineValidationStatus.PendingValidation
          RejectionReason = None }

    create <!> validateOrderLineId req.OrderLineId
           <*> validateItemId req.Item
           <*> validateQty
           <*> validateUom
           <*> validateTimestamp req.RequestedDeliveryDate
           <*> validateOptionalTimestamp req.EarliestAcceptableDate
           <*> validateOptionalTimestamp req.LatestAcceptableDate
           <*> validateLocationId req.DestinationLocation
           <*> (Valid req.ExternalLineReference)

let toIngestCmd (req: IngestOrderRequest) : Validation<IngestOrderRequestCmd, DomainError> =
    let validatedLines = req.Lines |> List.map toOrderLine

    let sequenceValidations (validations: Validation<'a, DomainError> list) : Validation<'a list, DomainError> =
        let folder acc v =
            match acc, v with
            | Valid xs, Valid x -> Valid (x :: xs)
            | Invalid e1, Invalid e2 -> Invalid (e1 @ e2)
            | Invalid e1, Valid _ -> Invalid e1
            | Valid _, Invalid e2 -> Invalid e2
        validations
        |> List.fold folder (Valid [])
        |> function
            | Valid xs -> Valid (List.rev xs)
            | Invalid errs -> Invalid errs

    let create snapId scopeId custId priority obsTime lines =
        { SnapshotId = snapId
          PlanningScopeId = scopeId
          CustomerOrderIdentifier = req.CustomerOrderIdentifier
          CustomerId = custId
          OrderChannel = req.OrderChannel
          OrderPriorityClass = priority
          ObservationTimestamp = obsTime
          Lines = lines }

    create <!> validateOrderBookSnapshotId req.SnapshotId
           <*> validatePlanningScopeId req.PlanningScopeId
           <*> validateCustomerId req.CustomerId
           <*> parsePriorityClass req.OrderPriorityClass
           <*> validateTimestamp req.ObservationTimestamp
           <*> sequenceValidations validatedLines

let toValidateCmd (req: ValidateOrderLineReq) : Validation<ValidateOrderTermsCmd, DomainError> =
    let create snapId lineId evalTime =
        { SnapshotId = snapId
          OrderLineId = lineId
          EvaluationTime = evalTime }

    create <!> validateOrderBookSnapshotId req.SnapshotId
           <*> validateOrderLineId req.OrderLineId
           <*> validateTimestamp req.EvaluationTime

let toCancelCmd (req: CancelOrderLineReq) : Validation<CancelOrderLineCmd, DomainError> =
    let create snapId lineId cancelTime =
        { SnapshotId = snapId
          OrderLineId = lineId
          CancellationReason = req.CancellationReason
          CancellationTime = cancelTime }

    create <!> validateOrderBookSnapshotId req.SnapshotId
           <*> validateOrderLineId req.OrderLineId
           <*> validateTimestamp req.CancellationTime

/// Outbound ACL: maps domain record events to canonical ARS Enterprise Events
let toArsEventId = function
    | OrderBookIngested _ -> ArsIdentifiers.EnterpriseEvents.orderBookIngested.Id
    | OrderValidationPassed _ -> ArsIdentifiers.EnterpriseEvents.orderValidationPassed.Id
    | OrderValidationFailed _ -> ArsIdentifiers.EnterpriseEvents.orderValidationFailed.Id
    | OrderLineCancelled _ -> ArsIdentifiers.EnterpriseEvents.orderLineCancelled.Id

