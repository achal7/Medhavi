/// CA-P-008 Manage Backorders — ACL
module Medhavi.Promise.ManageBackorders.BackorderRecord.ACL

open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model

let toEnqueueCmd (req: EnqueueBackorderReq) : Validation<EnqueueBackorderCmd, DomainError> =
    let create boId lineId custId item loc qty time =
        { BackorderId = boId
          OrderLineId = lineId
          CustomerId = custId
          Item = item
          Location = loc
          Quantity = qty
          EnqueuedTime = time }

    create <!> validateBackorderId req.BackorderId
           <*> validateOrderLineId req.OrderLineId
           <*> validateCustomerId req.CustomerId
           <*> validateItemId req.Item
           <*> validateLocationId req.Location
           <*> validateQty req.Quantity
           <*> validateTimestamp req.EnqueuedTime

let toResolveCmd (req: ResolveBackorderReq) : Validation<ResolveBackorderCmd, DomainError> =
    let create boId resDate time =
        { BackorderId = boId
          MatchedSupplyIdentifier = req.MatchedSupplyIdentifier
          ResolvedDeliveryDate = resDate
          ResolutionTime = time }

    create <!> validateBackorderId req.BackorderId
           <*> validateTimestamp req.ResolvedDeliveryDate
           <*> validateTimestamp req.ResolutionTime

let toArsEventId = function
    | BackorderQueued _ -> ArsIdentifiers.EnterpriseEvents.backorderQueued.Id
    | BackorderResolved _ -> ArsIdentifiers.EnterpriseEvents.backorderResolved.Id

