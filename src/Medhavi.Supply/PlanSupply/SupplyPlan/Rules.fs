/// SE-S-020 — Supply Planning Business Rules
module Medhavi.Supply.PlanSupply.SupplyPlan.Rules

open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model
open Policies


/// BR-S-020: Supply Plan Aggregate Identity
let validateAggregateIdentity
    (scopeId: PlanningScopeId)
    (horizonId: ForecastHorizonId)
    : Result<unit, DomainError> =
    if System.String.IsNullOrWhiteSpace(PlanningScopeId.value scopeId) then
        Error(DomainError.validation "PlanningScopeId cannot be empty for Supply Plan")
    elif System.String.IsNullOrWhiteSpace(ForecastHorizonId.value horizonId) then
        Error(DomainError.validation "ForecastHorizonId cannot be empty for Supply Plan")
    else
        Ok ()

/// BR-S-021: Supply Plan Generation Eligibility
let validateGenerationEligibility
    (scopeExists: bool)
    (horizonExists: bool)
    : Result<unit, DomainError> =
    if not scopeExists then
        Error(ruleViolationWithDetail ArsIdentifiers.Rules.supplyPlanGenerationEligibility "PlanningScope does not exist or is inactive")
    elif not horizonExists then
        Error(ruleViolationWithDetail ArsIdentifiers.Rules.supplyPlanGenerationEligibility "ForecastHorizon does not exist or is inactive")
    else
        Ok ()

/// BR-S-022: Supply Plan Derivation Source
let validateDerivationSource
    (supplyUndVer: int)
    (forecastPubVer: int)
    (pictureVer: int64)
    : Result<unit, DomainError> =
    if supplyUndVer <= 0 then
        Error(ruleViolationWithDetail ArsIdentifiers.Rules.supplyPlanDerivationSource "Missing valid Published Supply Understanding version")
    elif forecastPubVer <= 0 then
        Error(ruleViolationWithDetail ArsIdentifiers.Rules.supplyPlanDerivationSource "Missing valid Published Forecast Publication version")
    elif pictureVer <= 0L then
        Error(ruleViolationWithDetail ArsIdentifiers.Rules.supplyPlanDerivationSource "Missing valid Published Enterprise Picture version")
    else
        Ok ()

/// BR-S-121: Supply Plan Evaluation Eligibility
let validateEvaluationEligibility
    (draftOpt: SupplyPlanVersion option)
    : Result<SupplyPlanVersion, DomainError> =
    match draftOpt with
    | None -> Error(ruleViolationWithDetail ArsIdentifiers.Rules.supplyPlanEvaluationEligibility "No Draft Supply Plan Version exists for evaluation")
    | Some v when v.State <> VersionState.Draft ->
        Error(ruleViolationWithDetail ArsIdentifiers.Rules.supplyPlanEvaluationEligibility $"Version %d{v.VersionNumber} is not in Draft state")
    | Some v -> Ok v

/// BR-S-125: Hard Constraint Satisfaction Before Publication
let validateConstraintSatisfaction
    (violations: ConstraintViolationRecord list)
    : Result<unit, DomainError> =
    let unapproved = violations |> List.filter (fun v -> not v.IsRelaxationApproved)
    if not unapproved.IsEmpty then
        Error(ruleViolationWithDetail ArsIdentifiers.Rules.hardConstraintSatisfactionBeforePublication $"Plan has %d{unapproved.Length} unapproved hard constraint violation(s)")
    else
        Ok ()

/// BR-S-126: Plan Version Monotonicity
let validateVersionMonotonicity
    (targetVersion: int)
    (existingVersions: SupplyPlanVersion list)
    : Result<unit, DomainError> =
    let maxExisting =
        existingVersions
        |> List.map (fun v -> v.VersionNumber)
        |> List.fold max 0

    if targetVersion <= maxExisting then
        Error(DomainError.conflict $"Version number %d{targetVersion} is not monotonically greater than max existing %d{maxExisting}")
    else
        Ok ()

/// BR-S-127: Supply Plan Quality Thresholds
let evaluateQualityThresholds
    (policy: SupplyPlanAcceptancePolicy)
    (version: SupplyPlanVersion)
    : Result<unit, string> =
    if version.ServiceLevelAchieved < (policy.MinServiceLevelTarget - policy.WarningTolerance) then
        Error $"Service level %M{version.ServiceLevelAchieved} breached mandatory minimum %M{policy.MinServiceLevelTarget - policy.WarningTolerance}"
    elif version.MaxCapacityUtilization > (policy.MaxCapacityUtilizationLimit + policy.WarningTolerance) then
        Error $"Max capacity utilization %M{version.MaxCapacityUtilization} breached ceiling %M{policy.MaxCapacityUtilizationLimit + policy.WarningTolerance}"
    else
        Ok ()

/// BR-S-128 / BR-S-131: Plan Stability & First Publication Acceptance
let evaluateStabilityThreshold
    (policy: SupplyPlanAcceptancePolicy)
    (isFirstPublication: bool)
    (version: SupplyPlanVersion)
    : Result<unit, string> =
    if isFirstPublication then
        Ok () // BR-S-131: Waived for initial baseline
    elif version.PlanStabilityScore < (policy.MinStabilityScoreTarget - policy.WarningTolerance) then
        Error $"Plan stability %M{version.PlanStabilityScore} breached threshold %M{policy.MinStabilityScoreTarget}"
    else
        Ok ()
