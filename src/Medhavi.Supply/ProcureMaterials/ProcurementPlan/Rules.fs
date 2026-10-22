/// BR-S-061, BR-S-201, BR-S-204, BR-S-205, BR-S-206, BR-S-209, BR-S-111, BR-S-112, BR-S-113
/// Traces to: Supply Intelligence Specification §7
module Medhavi.Supply.ProcureMaterials.ProcurementPlan.Rules

open System
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply

/// BR-S-061: Procurement Plan Aggregate Identity Invariant
let validateIdentity (scopeId: PlanningScopeId) (cycleId: string) : Result<unit, DomainError> =
    if String.IsNullOrWhiteSpace(PlanningScopeId.value scopeId) then
        Error(DomainError.validation "PlanningScopeId cannot be empty (BR-S-061)")
    elif String.IsNullOrWhiteSpace cycleId then
        Error(DomainError.validation "PlanningCycleId cannot be empty (BR-S-061)")
    else
        Ok()

/// BR-S-201: Procurement Plan Publication Eligibility
let validatePublicationEligibility
    (recommendationsCount: int)
    (authority: string)
    (timestamp: Timestamp)
    : Result<unit, DomainError> =

    if recommendationsCount = 0 then
        Error(
            ruleViolation ArsIdentifiers.Rules.procurementPlanPublicationEligibility
        )
    elif String.IsNullOrWhiteSpace authority then
        Error(ruleViolation ArsIdentifiers.Rules.procurementPlanPublicationAuthorization)
    elif Timestamp.value timestamp = DateTimeOffset.MinValue then
        Error(ruleViolationWithDetail ArsIdentifiers.Rules.procurementPlanPublicationEligibility "Publication timestamp must be a valid UTC timestamp")
    else
        Ok()

/// BR-S-111: Procurement Plan Completeness
let validateCompleteness
    (totalActiveRequirements: int)
    (coveredRequirements: int)
    (policy: PlanPublicationPolicyConfig)
    : Result<bool, DomainError> =

    if policy.RequireCompleteCoverage && coveredRequirements < totalActiveRequirements then
        Ok false
    else
        Ok true

/// BR-S-112: Procurement Plan Confidence Threshold
let evaluateConfidenceScore (confidenceDistribution: (RecommendationConfidence * int) list) : decimal =

    let total = confidenceDistribution |> List.sumBy snd

    if total = 0 then
        0.0m
    else
        let weightedSum =
            confidenceDistribution
            |> List.sumBy(fun (c, count) ->
                let weight =
                    match c with
                    | RecommendationConfidence.High -> 1.0m
                    | RecommendationConfidence.Medium -> 0.70m
                    | RecommendationConfidence.Low -> 0.30m

                weight * decimal count)

        weightedSum / decimal total
