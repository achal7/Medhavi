module Medhavi.Core.DemandManagement.ACL

open Medhavi.SemanticModel
open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.Contracts.Core.Demand
open Medhavi.Core
open Model

/// Translates RecordDemandReq into domain command.
let toRecordCmd (req: RecordDemandReq) : Validation<RecordDemandCmd, DomainError> =

    let validateNeedWindow =
        NeedWindow.create req.NeedWindowLatest req.NeedWindowEarliest req.NeedWindowPreferred
        |> Result.mapError mapSemanticValidationToDomainError
        |> fromResult

    let parseOrigin (origin: string) : DemandOrigin =
        match origin with
        | "CustomerOrder" -> CustomerOrder
        | "Forecast" -> Forecast
        | "ProductionRequirement" -> ProductionRequirement
        | "Transfer" -> Transfer
        | _ -> DemandOrigin.Other

    let validateParentDemand =
        match req.ParentDemand with
        | None -> Valid None
        | Some pd ->
            DemandId.create pd |> Result.mapError mapSemanticValidationToDomainError |> Result.map Some |> fromResult

    let create demandId item loc customer qty needWindow parentDemand =
        { DemandId = demandId
          Item = item
          Location = loc
          Customer = customer
          Quantity = qty
          NeedWindow = needWindow
          DemandOrigin = parseOrigin req.DemandOrigin
          ParentDemand = parentDemand }

    create <!> validateDemandId req.DemandId
    <*> validateItemId req.Item
    <*> validateLocationId req.Location
    <*> validateCustomerId req.Customer
    <*> validateQuantity req.Quantity
    <*> validateNeedWindow
    <*> validateParentDemand

/// Translates SatisfyDemandReq into domain command.
let toSatisfyCmd (req: SatisfyDemandReq) : Validation<SatisfyDemandCmd, DomainError> =
    let create demandId satTime =
        { DemandId = demandId
          SatisfactionTime = satTime }

    create <!> validateDemandId req.DemandId <*> validateTimestamp req.SatisfactionTime

/// Translates CancelDemandReq into domain command.
let toCancelCmd (req: CancelDemandReq) : Validation<CancelDemandCmd, DomainError> =

    let create demandId cancelTime =
        { DemandId = demandId
          CancellationTime = cancelTime
          Reason = req.Reason }

    create <!> validateDemandId req.DemandId <*> validateTimestamp req.CancellationTime
