/// SE-S-100 — Supply Quality Assessment Business Rules
/// Traces to: BR-S-100, BR-S-260 through BR-S-268
module Medhavi.Supply.EvaluateSupplyQuality.SupplyQualityAssessment.Rules

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers
open Model
open Policies
open Algorithms

type PublicationRuleInput =
    { OverallQuality: OverallQualityState
      DimensionalEvaluations: DimensionalEvaluation list
      Policy: SupplyQualityPolicy }

/// BR-S-100 — Supply Quality Assessment Identity
let validateIdentity
    (scopeId: PlanningScopeId)
    (horizon: PlanningHorizon)
    (periodId: EvaluationPeriodId) : Result<SupplyQualityAssessmentId, DomainError> =
    if String.IsNullOrWhiteSpace (PlanningScopeId.value scopeId) then
        Error (DomainError.validation "BR-S-100: PlanningScopeId cannot be empty.")
    elif String.IsNullOrWhiteSpace (EvaluationPeriodId.value periodId) then
        Error (DomainError.validation "BR-S-100: EvaluationPeriodId cannot be empty.")
    else
        Ok (SupplyQualityAssessmentId.ofScopeHorizonPeriod scopeId horizon periodId)

/// BR-S-260 — Supply Quality Assessment Eligibility
let validateEligibility
    (hasSupplyPlan: bool)
    (hasInventoryHealth: bool)
    (hasCapacityHealth: bool)
    (hasSupplierHealth: bool)
    (isPeriodComplete: bool) : Result<unit, DomainError> =
    if not isPeriodComplete then
        Error (DomainError.rule("BR-S-260: Evaluation period is not yet complete.", rule = "BR-S-260"))
    elif not hasSupplyPlan then
        Error (DomainError.rule("BR-S-260: Published Supply Plan is missing for the target scope and horizon.", rule = "BR-S-260"))
    elif not hasInventoryHealth then
        Error (DomainError.rule("BR-S-260: Inventory health assessment is missing for the target scope.", rule = "BR-S-260"))
    elif not hasCapacityHealth then
        Error (DomainError.rule("BR-S-260: Capacity health assessment is missing for the target scope.", rule = "BR-S-260"))
    elif not hasSupplierHealth then
        Error (DomainError.rule("BR-S-260: Supplier collaboration health assessment is missing for the target scope.", rule = "BR-S-260"))
    else
        Ok ()

/// BR-S-261 — Single Published Supply Quality Assessment
let ensureSinglePublished
    (currentPublishedOpt: SupplyQualityAssessmentVersion option)
    (newVersion: int) : Result<unit, DomainError> =
    match currentPublishedOpt with
    | Some existing when existing.VersionNumber >= newVersion ->
        Error (DomainError.conflict $"BR-S-261: An authoritative version ({existing.VersionNumber}) already exists with greater or equal version number.")
    | _ -> Ok ()

/// BR-S-262 — Published Supply Quality Assessment Immutability
let ensureMutable (version: SupplyQualityAssessmentVersion) : Result<unit, DomainError> =
    match version.LifecycleState with
    | SupplyQualityLifecycleState.Draft -> Ok ()
    | SupplyQualityLifecycleState.Published ->
        Error (DomainError.invariant "BR-S-262: Published Supply Quality Assessment is immutable and cannot be modified.")
    | SupplyQualityLifecycleState.Superseded ->
        Error (DomainError.invariant "BR-S-262: Superseded Supply Quality Assessment is immutable and terminal.")

/// BR-S-263 — Permanent Retention of Supply Quality Assessments
let forbidDeletion () : Result<unit, DomainError> =
    Error (DomainError.invariant "BR-S-263: Permanent Retention: Supply Quality Assessments cannot be deleted.")

/// BR-S-264 — Supply Quality Assessment Version Monotonicity
let validateNextVersion (highestExistingVersion: int) (proposedVersion: int) : Result<unit, DomainError> =
    if proposedVersion <= highestExistingVersion then
        Error (DomainError.invariant $"BR-S-264: Proposed version number {proposedVersion} must be strictly greater than existing version {highestExistingVersion}.")
    else
        Ok ()

/// BR-S-265 — Dimensional Quality Evaluation Governance
let validatePolicyActive (policy: SupplyQualityPolicy) : Result<unit, DomainError> =
    if String.IsNullOrWhiteSpace policy.PolicyVersion then
        Error (DomainError.rule("BR-S-265: Missing Supply Quality Policy (PO-S-110).", rule = "BR-S-265"))
    elif policy.Dimensions.IsEmpty then
        Error (DomainError.rule("BR-S-265: PO-S-110 must configure at least one governed quality dimension.", rule = "BR-S-265"))
    else
        Ok ()

/// BR-S-266 — Publication Rule: Mandatory dimensions must all be evaluated
let mandatoryDimensionsEvaluated: Rule<PublicationRuleInput> =
    Rule.create
        "BR-S-266-MANDATORY"
        "All mandatory quality dimensions must be evaluated with conclusive assessments"
        (fun input ->
            let mandatory = input.DimensionalEvaluations |> List.filter (fun d -> d.IsMandatory)
            not mandatory.IsEmpty &&
            mandatory |> List.forall (fun d -> d.AuthoritativeClassification <> QualityClassification.Unassessed) &&
            input.OverallQuality.OverallClassification <> QualityClassification.Unassessed)
        (fun input ->
            let unassessed =
                input.DimensionalEvaluations
                |> List.filter (fun d -> d.IsMandatory && d.AuthoritativeClassification = QualityClassification.Unassessed)
                |> List.map (fun d -> d.DimensionName)
            if unassessed.IsEmpty then
                "All mandatory dimensions evaluated"
            else
                let names = String.concat ", " unassessed
                $"Unassessed mandatory dimensions: {names}")

/// BR-S-266 — Publication Rule: Overall confidence meets minimum threshold
let confidenceThresholdMet: Rule<PublicationRuleInput> =
    Rule.create
        "BR-S-266-CONFIDENCE"
        "Overall confidence must meet or exceed minimum policy threshold"
        (fun input ->
            let currentRank = QualityConfidenceLevel.rank input.OverallQuality.OverallConfidence
            let minRank = QualityConfidenceLevel.rank input.Policy.MinimumPublicationConfidence
            currentRank >= minRank)
        (fun input ->
            $"Overall confidence: {input.OverallQuality.OverallConfidence}; Minimum required: {input.Policy.MinimumPublicationConfidence}")

/// Publication rule set
let publicationRules: Rule<PublicationRuleInput> list =
    [ mandatoryDimensionsEvaluated; confidenceThresholdMet ]
