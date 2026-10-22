module Medhavi.Supply.PlanSupply.SupplyPlan.ACL

open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Model

/// Translates GenerateSupplyPlanReq into domain command using Applicative validation.
let toGenerateCmd (req: GenerateSupplyPlanReq) : Validation<GeneratePlanCmd, DomainError> =
    let validateVer (v: int) (name: string) : Validation<int, DomainError> =
        if v > 0 then Valid v
        else Invalid [ DomainError.validation $"{name} must be greater than zero" ]

    let validatePicVer (v: int64) : Validation<int64, DomainError> =
        if v > 0L then Valid v
        else Invalid [ DomainError.validation "EnterprisePictureVersion must be greater than zero" ]

    let create scope horizon suVer fpVer epVer =
        { PlanningScopeId = scope
          ForecastHorizonId = horizon
          SupplyUnderstandingVersion = suVer
          ForecastPublicationVersion = fpVer
          EnterprisePictureVersion = epVer
          Timestamp = Timestamp.now() }

    create <!> validatePlanningScopeId req.PlanningScopeId
    <*> validateForecastHorizonId req.ForecastHorizonId
    <*> validateVer req.SupplyUnderstandingVersion "SupplyUnderstandingVersion"
    <*> validateVer req.ForecastPublicationVersion "ForecastPublicationVersion"
    <*> validatePicVer req.EnterprisePictureVersion

/// Translates EvaluateSupplyPlanReq into domain command.
let toEvaluateCmd (req: EvaluateSupplyPlanReq) : Validation<EvaluatePlanCmd, DomainError> =
    let validateVer (v: int) : Validation<int, DomainError> =
        if v > 0 then Valid v
        else Invalid [ DomainError.validation "VersionNumber must be greater than zero" ]

    let create scope horizon ver =
        { PlanningScopeId = scope
          ForecastHorizonId = horizon
          VersionNumber = ver
          Timestamp = Timestamp.now() }

    create <!> validatePlanningScopeId req.PlanningScopeId
    <*> validateForecastHorizonId req.ForecastHorizonId
    <*> validateVer req.VersionNumber

/// Translates PublishSupplyPlanReq into domain command.
let toPublishCmd (req: PublishSupplyPlanReq) : Validation<PublishPlanCmd, DomainError> =
    let validateVer (v: int) : Validation<int, DomainError> =
        if v > 0 then Valid v
        else Invalid [ DomainError.validation "VersionNumber must be greater than zero" ]

    let create scope horizon ver =
        { PlanningScopeId = scope
          ForecastHorizonId = horizon
          VersionNumber = ver
          IsOutCycle = req.IsOutCycle
          Timestamp = Timestamp.now() }

    create <!> validatePlanningScopeId req.PlanningScopeId
    <*> validateForecastHorizonId req.ForecastHorizonId
    <*> validateVer req.VersionNumber

/// Outbound ACL: maps domain plan events to canonical ARS Enterprise Events (EV-S-010, EV-S-011, EV-S-012).
let toArsEventId = function
    | SupplyPlanGenerated _ -> ArsIdentifiers.EnterpriseEvents.supplyPlanGenerated.Id
    | SupplyPlanEvaluated _ -> ArsIdentifiers.EnterpriseEvents.supplyPlanEvaluated.Id
    | SupplyPlanPublished _ -> ArsIdentifiers.EnterpriseEvents.supplyPlanPublished.Id
