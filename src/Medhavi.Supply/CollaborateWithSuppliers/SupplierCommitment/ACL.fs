/// Anti-Corruption Layer (ACL) for Supplier Commitment Commands
/// Translates Medhavi.Contracts.Supply DTOs into pure domain commands using applicative validation
module Medhavi.Supply.CollaborateWithSuppliers.SupplierCommitment.ACL

open Medhavi
open Medhavi.Common.Validation
open Medhavi.Contracts.Supply
open Medhavi.Foundation.Failure
open Medhavi.Supply
open Model

let mapEventType (t: Contracts.Supply.CollaborationEventType) : Model.CollaborationEventType =
    match t with
    | Contracts.Supply.CollaborationEventType.EnterpriseRequest -> Model.CollaborationEventType.EnterpriseRequest
    | Contracts.Supply.CollaborationEventType.SupplierAcknowledgement ->
        Model.CollaborationEventType.SupplierAcknowledgement
    | Contracts.Supply.CollaborationEventType.SupplierCommitment -> Model.CollaborationEventType.SupplierCommitment
    | Contracts.Supply.CollaborationEventType.SupplierRejection -> Model.CollaborationEventType.SupplierRejection
    | Contracts.Supply.CollaborationEventType.SupplierAlternative -> Model.CollaborationEventType.SupplierAlternative
    | Contracts.Supply.CollaborationEventType.EnterpriseConfirmation ->
        Model.CollaborationEventType.EnterpriseConfirmation

let toRecordRequestCmd (req: RecordEnterpriseRequestReq) : Validation<RecordEnterpriseRequestCmd, DomainError> =
    let supplierV = validateSupplierId req.SupplierId
    let itemV = validateItemId req.ItemId
    let reqDateV = validateTimestamp req.RequiredDate
    let reqQtyV = validateQty req.RequestedQuantity
    let deliverDateV = validateTimestamp req.RequestedDate
    let reqTimestampV = validateTimestamp req.RequestTimestamp

    (fun s i rd rq dd rt ->
        {
            SupplierId = s
            ItemId = i
            RequiredDate = rd
            RequestedQuantity = rq
            RequestedDate = dd
            RequestTimestamp = rt
        })
    <!> supplierV
    <*> itemV
    <*> reqDateV
    <*> reqQtyV
    <*> deliverDateV
    <*> reqTimestampV

let toRecordResponseCmd (req: RecordSupplierResponseReq) : Validation<RecordSupplierResponseCmd, DomainError> =
    let supplierV = validateSupplierId req.SupplierId
    let itemV = validateItemId req.ItemId
    let reqDateV = validateTimestamp req.RequiredDate
    let respTimestampV = validateTimestamp req.ResponseTimestamp

    let committedQtyV =
        match req.CommittedQuantity with
        | Some q -> Some <!> validateQty q
        | None -> return' None

    let committedDateV =
        match req.CommittedDate with
        | Some d -> Some <!> validateTimestamp d
        | None -> return' None

    let altQtyV =
        match req.AlternativeQuantity with
        | Some q -> Some <!> validateQty q
        | None -> return' None

    let altDateV =
        match req.AlternativeDate with
        | Some d -> Some <!> validateTimestamp d
        | None -> return' None

    (fun s i rd rt cq cd aq ad ->
        {
            SupplierId = s
            ItemId = i
            RequiredDate = rd
            ResponseEventType = mapEventType req.ResponseEventType
            CommittedQuantity = cq
            CommittedDate = cd
            AlternativeDescription = req.AlternativeDescription
            AlternativeQuantity = aq
            AlternativeDate = ad
            SupplierAssumptions = req.SupplierAssumptions
            ResponseTimestamp = rt
        })
    <!> supplierV
    <*> itemV
    <*> reqDateV
    <*> respTimestampV
    <*> committedQtyV
    <*> committedDateV
    <*> altQtyV
    <*> altDateV
