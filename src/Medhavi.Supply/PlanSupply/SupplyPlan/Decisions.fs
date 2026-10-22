/// SE-S-020 — Supply Planning Decisions
module Medhavi.Supply.PlanSupply.SupplyPlan.Decisions

open Medhavi.Foundation.Failure
open Medhavi.Supply
open Model
open Policies
open Rules

/// DE-S-011: Accept Supply Plan
/// Assesses draft supply plan against quality KPIs and stability rules.
let acceptSupplyPlan
    (policy: SupplyPlanAcceptancePolicy)
    (isFirstPublication: bool)
    (draft: SupplyPlanVersion)
    : PlanEvaluationOutcome =

    let qualityRes = evaluateQualityThresholds policy draft
    let stabilityRes = evaluateStabilityThreshold policy isFirstPublication draft

    match qualityRes, stabilityRes with
    | Error _, _
    | _, Error _ ->
        PlanEvaluationOutcome.Rejected
    | Ok (), Ok () ->
        let isStrictQuality =
            draft.ServiceLevelAchieved >= policy.MinServiceLevelTarget
            && draft.MaxCapacityUtilization <= policy.MaxCapacityUtilizationLimit

        let isStrictStability =
            isFirstPublication || draft.PlanStabilityScore >= policy.MinStabilityScoreTarget

        if isStrictQuality && isStrictStability then
            PlanEvaluationOutcome.Accepted
        else
            PlanEvaluationOutcome.AcceptedWithWarnings

/// DE-S-012: Publish Supply Plan
/// Final gate before plan becomes authoritative. Authorizes state transition to Published.
let publishSupplyPlan
    (policy: SupplyPlanPublicationCadencePolicy)
    (plan: SupplyPlan)
    (versionNumber: int)
    : Result<int * int option, DomainError> =

    let targetVersionOpt =
        plan.Versions |> List.tryFind (fun v -> v.VersionNumber = versionNumber)

    match targetVersionOpt with
    | None ->
        Error(DomainError.notFound ("SupplyPlanVersion", string versionNumber))
    | Some version ->
        // 1. Must be in Draft state
        if version.State <> VersionState.Draft then
            Error(ruleViolationWithDetail ArsIdentifiers.Rules.publishedSupplyPlanImmutability $"Version %d{versionNumber} is already in state %A{version.State}")
        // 2. Must have been evaluated and accepted per BR-S-122
        elif version.EvaluationOutcome.IsNone then
            Error(ruleViolationWithDetail ArsIdentifiers.Rules.supplyPlanPublicationEligibility $"Version %d{versionNumber} has not been evaluated by DE-S-011")
        elif version.EvaluationOutcome = Some PlanEvaluationOutcome.Rejected then
            Error(ruleViolationWithDetail ArsIdentifiers.Rules.supplyPlanPublicationEligibility $"Version %d{versionNumber} was Rejected during evaluation and cannot be published")
        // 3. Hard constraints must be satisfied or approved per BR-S-125
        else
            match validateConstraintSatisfaction version.ConstraintViolations with
            | Error err -> Error err
            | Ok () ->
                let currentPublishedOpt =
                    plan.Versions |> List.tryFind (fun v -> v.State = VersionState.Published)

                let supersededOpt =
                    currentPublishedOpt |> Option.map (fun v -> v.VersionNumber)

                Ok (versionNumber, supersededOpt)
