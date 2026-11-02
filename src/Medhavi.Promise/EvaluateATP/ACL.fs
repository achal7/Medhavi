/// CA-P-004 Evaluate Multi-Option ATP ACL
module Medhavi.Promise.EvaluateATP.ACL

open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model

let toEvaluateCmd (req: EvaluateATPOptionsReq) : Validation<EvaluateATPOptionsCmd, DomainError> =
    let create lineId custId item loc qty reqDate =
        { OrderLineId = lineId
          CustomerId = custId
          Item = item
          DestinationLocation = loc
          RequestedQuantity = qty
          RequestedDeliveryDate = reqDate
          AllowSubstitutions = req.AllowSubstitutions
          AllowSplits = req.AllowSplits }

    create <!> validateOrderLineId req.OrderLineId
           <*> validateCustomerId req.CustomerId
           <*> validateItemId req.Item
           <*> validateLocationId req.DestinationLocation
           <*> validateQty req.RequestedQuantity
           <*> validateTimestamp req.RequestedDeliveryDate

let toArsEventId = function
    | ATPOptionsGenerated _ -> ArsIdentifiers.EnterpriseEvents.promiseOptionsGenerated.Id
    | ProductSubstitutionEvaluated _ -> ArsIdentifiers.EnterpriseEvents.productSubstitutionEvaluated.Id

