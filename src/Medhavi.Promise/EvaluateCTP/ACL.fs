/// CA-P-005 Evaluate Capable-to-Promise ACL
module Medhavi.Promise.EvaluateCTP.ACL

open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model

let toEvaluateCmd (req: EvaluateCTPOptionsReq) : Validation<EvaluateCTPOptionsCmd, DomainError> =
    let create lineId custId item dest plant qty reqDate =
        { OrderLineId = lineId
          CustomerId = custId
          Item = item
          DestinationLocation = dest
          PlantLocation = plant
          RequestedQuantity = qty
          RequestedDeliveryDate = reqDate }

    create <!> validateOrderLineId req.OrderLineId
           <*> validateCustomerId req.CustomerId
           <*> validateItemId req.Item
           <*> validateLocationId req.DestinationLocation
           <*> validateLocationId req.PlantLocation
           <*> validateQty req.RequestedQuantity
           <*> validateTimestamp req.RequestedDeliveryDate

let toArsEventId = function
    | CTPProductionSlotEvaluated _ -> ArsIdentifiers.EnterpriseEvents.ctpProductionSlotEvaluated.Id

