/// CA-P-001 Customer Promise Profile ACL
module Medhavi.Promise.UnderstandOrders.CustomerPromiseProfile.ACL

open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model

let private validateOptionalTimestamp (tOpt: System.DateTimeOffset option) : Validation<Timestamp option, DomainError> =
    match tOpt with
    | Some t -> Some <!> validateTimestamp t
    | None -> Valid None

let toConfigureCmd (req: ConfigureCustomerPromiseProfileReq) : Validation<ConfigureCustomerPromiseProfileCmd, DomainError> =
    let create custId effFrom effTo =
        { CustomerId = custId
          CustomerTier = req.CustomerTier
          CreditLimit = req.CreditLimit
          CreditHoldActive = req.CreditHoldActive
          DefaultSlaDays = req.DefaultSlaDays
          AllowSplitShipments = req.AllowSplitShipments
          MaxSplitShipmentPackages = req.MaxSplitShipmentPackages
          AllowProductSubstitution = req.AllowProductSubstitution
          SubstitutionMarginFloorPct = req.SubstitutionMarginFloorPct
          DailyPenaltyRatePct = req.DailyPenaltyRatePct
          MaxPenaltyCapPct = req.MaxPenaltyCapPct
          EffectiveFrom = effFrom
          EffectiveTo = effTo }

    create <!> validateCustomerId req.CustomerId
           <*> validateTimestamp req.EffectiveFrom
           <*> validateOptionalTimestamp req.EffectiveTo

let toUpdateTermsCmd (req: UpdateCustomerServiceTermsReq) : Validation<UpdateCustomerServiceTermsCmd, DomainError> =
    let create custId updateTime =
        { CustomerId = custId
          DefaultSlaDays = req.DefaultSlaDays
          AllowSplitShipments = req.AllowSplitShipments
          AllowProductSubstitution = req.AllowProductSubstitution
          DailyPenaltyRatePct = req.DailyPenaltyRatePct
          CreditHoldActive = req.CreditHoldActive
          UpdateTime = updateTime }

    create <!> validateCustomerId req.CustomerId
           <*> validateTimestamp req.UpdateTime

let toArsEventId = function
    | CustomerPromiseProfileConfigured _ -> ArsIdentifiers.EnterpriseEvents.customerPromiseProfileConfigured.Id
    | CustomerServiceTermsUpdated _ -> ArsIdentifiers.EnterpriseEvents.customerServiceTermsUpdated.Id

