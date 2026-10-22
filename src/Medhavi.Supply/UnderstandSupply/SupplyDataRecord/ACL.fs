module Medhavi.Supply.UnderstandSupply.SupplyDataRecord.ACL

open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Model

let parseSupplyType (s: string) : Validation<SupplyType, DomainError> =
    match s.Trim().ToLowerInvariant() with
    | "inventory" -> Valid SupplyType.Inventory
    | "purchaseorder" -> Valid SupplyType.PurchaseOrder
    | "productionorder" -> Valid SupplyType.ProductionOrder
    | "transfer" -> Valid SupplyType.Transfer
    | "suppliercommitment" -> Valid SupplyType.SupplierCommitment
    | other -> Invalid [ DomainError.validation $"Invalid SupplyType: '{other}'" ]

let private validateOptionalSupplier (sOpt: string option) : Validation<SupplierId option, DomainError> =
    match sOpt with
    | Some s when not (System.String.IsNullOrWhiteSpace s) ->
        Some <!> validateSupplierId s
    | _ -> Valid None

let private validateOptionalTimestamp (tOpt: System.DateTimeOffset option) : Validation<Timestamp option, DomainError> =
    match tOpt with
    | Some t -> Some <!> validateTimestamp t
    | None -> Valid None

/// Translates ReceiveSupplyDataReq into domain command using Applicative validation.
let toReceiveCmd (req: ReceiveSupplyDataReq) : Validation<ReceiveRecordCmd, DomainError> =
    let validateQty =
        UnitOfMeasureId.create req.UnitOfMeasure
        |> Result.bind(fun uom -> Quantity.createWithUoM req.Quantity uom)
        |> Result.mapError(fun e -> DomainError.validation $"Invalid Quantity: {e}")
        |> fromResult

    let validateUom =
        UnitOfMeasureId.create req.UnitOfMeasure
        |> Result.mapError(fun e -> DomainError.validation $"Invalid UnitOfMeasure: {e}")
        |> fromResult

    let create recId item loc stype qty uom batch order comm supp expected bizTime obsTime source =
        { RecordId = recId
          Item = item
          Location = loc
          SupplyType = stype
          Quantity = qty
          UnitOfMeasure = uom
          BatchIdentifier = batch
          OrderIdentifier = order
          CommitmentIdentifier = comm
          SupplierIdentifier = supp
          ExpectedDeliveryDate = expected
          BusinessTime = bizTime
          ObservationTime = obsTime
          SourceSystemProvenance = source }

    create <!> validateRecordId req.RecordId
    <*> validateItemId req.Item
    <*> validateLocationId req.Location
    <*> parseSupplyType req.SupplyType
    <*> validateQty
    <*> validateUom
    <*> (Valid req.BatchIdentifier)
    <*> (Valid req.OrderIdentifier)
    <*> (Valid req.CommitmentIdentifier)
    <*> validateOptionalSupplier req.SupplierIdentifier
    <*> validateOptionalTimestamp req.ExpectedDeliveryDate
    <*> validateTimestamp req.BusinessTime
    <*> validateTimestamp req.ObservationTime
    <*> (Valid req.SourceSystemProvenance)

/// Translates EvaluateSupplyDataReq into domain command.
let toEvaluateCmd (req: EvaluateSupplyDataReq) : Validation<EvaluateRecordCmd, DomainError> =
    let create recId evalTime =
        { RecordId = recId
          EvaluationTime = evalTime }

    create <!> validateRecordId req.RecordId <*> validateTimestamp req.EvaluationTime

/// Outbound ACL: maps domain record events to canonical ARS Enterprise Events (EV-S-001, EV-S-002).
let toArsEventId = function
    | RecordReceived _ -> ArsIdentifiers.EnterpriseEvents.supplyDataRecordReceived.Id
    | RecordAccepted _
    | RecordQuarantined _
    | RecordRejected _ -> ArsIdentifiers.EnterpriseEvents.supplyDataRecordEvaluated.Id
