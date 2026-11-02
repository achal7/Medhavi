/// CA-P-003 Reserve Inventory and Capacity Temporarily ACL
module Medhavi.Promise.ReserveInventory.TemporaryReservation.ACL

open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model

let toCreateCmd (req: CreateTemporaryHoldReq) : Validation<CreateTemporaryHoldCmd, DomainError> =
    let create token lineId custId item loc qty time =
        { ReservationToken = token
          OrderLineId = lineId
          CustomerId = custId
          Item = item
          Location = loc
          Quantity = qty
          LeaseDurationMinutes = req.LeaseDurationMinutes
          CreationTime = time }

    create <!> validateTemporaryReservationId req.ReservationToken
           <*> validateOrderLineId req.OrderLineId
           <*> validateCustomerId req.CustomerId
           <*> validateItemId req.Item
           <*> validateLocationId req.Location
           <*> validateQty req.Quantity
           <*> validateTimestamp req.CreationTime

let toConfirmCmd (req: ConfirmHoldToCommitmentReq) : Validation<ConfirmHoldToCommitmentCmd, DomainError> =
    let create token time =
        { ReservationToken = token
          ConfirmationTime = time }

    create <!> validateTemporaryReservationId req.ReservationToken
           <*> validateTimestamp req.ConfirmationTime

let toReleaseCmd (req: ReleaseTemporaryHoldReq) : Validation<ReleaseTemporaryHoldCmd, DomainError> =
    let create token time =
        { ReservationToken = token
          ReleaseReason = req.ReleaseReason
          ReleaseTime = time }

    create <!> validateTemporaryReservationId req.ReservationToken
           <*> validateTimestamp req.ReleaseTime

let toExpireCmd (req: ExpireTemporaryHoldReq) : Validation<ExpireTemporaryHoldCmd, DomainError> =
    let create token time =
        { ReservationToken = token
          HeartbeatTime = time }

    create <!> validateTemporaryReservationId req.ReservationToken
           <*> validateTimestamp req.HeartbeatTime

let toArsEventId = function
    | TemporaryHoldCreated _ -> ArsIdentifiers.EnterpriseEvents.temporaryHoldCreated.Id
    | TemporaryHoldConfirmed _ -> ArsIdentifiers.EnterpriseEvents.temporaryHoldConfirmed.Id
    | TemporaryHoldReleased _ -> ArsIdentifiers.EnterpriseEvents.temporaryHoldReleased.Id
    | TemporaryHoldExpired _ -> ArsIdentifiers.EnterpriseEvents.temporaryHoldExpired.Id

