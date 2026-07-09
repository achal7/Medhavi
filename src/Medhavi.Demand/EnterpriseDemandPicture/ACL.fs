module Medhavi.Demand.EnterpriseDemandPicture.ACL

open Medhavi.Common.Validation
open Medhavi.Contracts.Demand.Edp
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Failure
open Medhavi.Demand
open Medhavi.Demand.EnterpriseDemandPicture.Model

let toReviseCmd (req: ReviseEnterpriseDemandPictureReq) : Validation<ReviseEdpCmd, DomainError> =
    let make scopeId qty obsId =
        { PlanningScopeId = scopeId
          Period = req.Period
          Quantity = qty
          ObservationId = obsId }

    make <!> (PlanningScopeId.fromString req.PlanningScopeId |> fromResult)
    <*> (Quantity.create req.Quantity |> fromResult)
    <*> (DemandObservationId.create req.ObservationId |> fromResult)

let toCalculateCmd (req: CalculateEnterpriseDemandPictureReq) : Validation<CalculateEdpCmd, DomainError> =
    let make scopeId =
        { PlanningScopeId = scopeId
          Adjustments = Map.empty
          Overrides = Map.empty }
    make <!> (PlanningScopeId.fromString req.PlanningScopeId |> fromResult)

let toPublishCmd (req: PublishEnterpriseDemandPictureReq) : Validation<PublishEdpCmd, DomainError> =
    let make scopeId = { PlanningScopeId = scopeId }: PublishEdpCmd
    make <!> (PlanningScopeId.fromString req.PlanningScopeId |> fromResult)
