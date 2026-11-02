/// CA-P-006 Establish Commitments ACL
module Medhavi.Promise.EstablishCommitments.OrderCommitment.ACL

open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Medhavi.Promise.EvaluateATP.Model
open Model

let private toSourcingLeg (dto: SourcingLegReq) : Validation<SourcingLeg, DomainError> =
    let create origin item qty shipDate delivDate =
        { LegId = dto.LegId
          OriginLocation = origin
          Item = item
          SourcedQuantity = qty
          FulfillmentMode = FulfillmentMode.OfString dto.FulfillmentMode
          EstimatedShipDate = shipDate
          EstimatedDeliveryDate = delivDate
          FreightCost = dto.FreightCost
          CarbonEmissionsKg = dto.CarbonEmissionsKg }

    create <!> validateLocationId dto.OriginLocation
           <*> validateItemId dto.Item
           <*> validateQty dto.SourcedQuantity
           <*> validateTimestamp dto.EstimatedShipDate
           <*> validateTimestamp dto.EstimatedDeliveryDate

let toEstablishCmd (req: EstablishCommitmentReq) : Validation<EstablishCommitmentCmd, DomainError> =
    let validatedLegs = req.SourcingLegs |> List.map toSourcingLeg

    let create commId lineId custId item dest delivDate qty time legs =
        { CommitmentId = commId
          CustomerOrderIdentifier = req.CustomerOrderIdentifier
          OrderLineId = lineId
          CustomerId = custId
          Item = item
          DestinationLocation = dest
          CommittedDeliveryDate = delivDate
          PromisedQuantity = qty
          ProjectedLandedCost = req.ProjectedLandedCost
          GrossMarginPct = req.GrossMarginPct
          DecisionConfidence = req.DecisionConfidence
          SourcingLegs = legs
          EstablishmentTime = time }

    create <!> validateOrderCommitmentId req.CommitmentId
           <*> validateOrderLineId req.OrderLineId
           <*> validateCustomerId req.CustomerId
           <*> validateItemId req.Item
           <*> validateLocationId req.DestinationLocation
           <*> validateTimestamp req.CommittedDeliveryDate
           <*> validateQty req.PromisedQuantity
           <*> validateTimestamp req.EstablishmentTime
           <*> sequenceValidations validatedLegs

let toReviseCmd (req: ReviseCommitmentReq) : Validation<ReviseCommitmentCmd, DomainError> =
    let validatedLegs = req.SourcingLegs |> List.map toSourcingLeg

    let create commId newDate qty time legs =
        { CommitmentId = commId
          NewDeliveryDate = newDate
          PromisedQuantity = qty
          ProjectedLandedCost = req.ProjectedLandedCost
          GrossMarginPct = req.GrossMarginPct
          DecisionConfidence = req.DecisionConfidence
          SourcingLegs = legs
          RevisionReason = req.RevisionReason
          RevisionTime = time }

    create <!> validateOrderCommitmentId req.CommitmentId
           <*> validateTimestamp req.NewDeliveryDate
           <*> validateQty req.PromisedQuantity
           <*> validateTimestamp req.RevisionTime
           <*> sequenceValidations validatedLegs

let toConfirmFulfillmentCmd (req: ConfirmFulfillmentReq) : Validation<ConfirmFulfillmentCmd, DomainError> =
    let create commId podTime qty =
        { CommitmentId = commId
          ProofOfDeliveryTime = podTime
          DeliveredQuantity = qty }

    create <!> validateOrderCommitmentId req.CommitmentId
           <*> validateTimestamp req.ProofOfDeliveryTime
           <*> validateQty req.DeliveredQuantity

let toCancelCmd (req: CancelCommitmentReq) : Validation<CancelCommitmentCmd, DomainError> =
    let create commId cancelTime =
        { CommitmentId = commId
          CancellationReason = req.CancellationReason
          CancellationTime = cancelTime }

    create <!> validateOrderCommitmentId req.CommitmentId
           <*> validateTimestamp req.CancellationTime

let toArsEventId = function
    | CommitmentEstablished _ -> ArsIdentifiers.EnterpriseEvents.commitmentEstablished.Id
    | CommitmentRevised _ -> ArsIdentifiers.EnterpriseEvents.commitmentRevised.Id
    | CommitmentFulfilled _ -> ArsIdentifiers.EnterpriseEvents.commitmentFulfilled.Id
    | CommitmentCancelled _ -> ArsIdentifiers.EnterpriseEvents.commitmentCancelled.Id

