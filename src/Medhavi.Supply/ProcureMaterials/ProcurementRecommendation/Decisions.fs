/// DE-S-051: Determine Enterprise Sourcing Determination
/// Traces to: Supply Intelligence Specification §6 (DE-S-051)
module Medhavi.Supply.ProcureMaterials.ProcurementRecommendation.Decisions

open System
open Medhavi.Common
open Medhavi.Foundation.Failure
open Model
open Algorithms
open Policies
open Medhavi.Supply

[<RequireQualifiedAccess>]
type SourcingDecisionOutcome =
    | Approve
    | Reject of Reason: string

/// DE-S-051 — Determine Enterprise Sourcing Determination
/// Evaluates the completeness, consistency, and governance alignment of the computed determination
let evaluateSourcingDetermination
    (determination: EnterpriseSourcingDetermination)
    (situation: SourcingSituationAssessment)
    (evaluatedReq: EvaluatedProcurementRequirement)
    (policy: ProcurementPolicyConfig)
    : Result<SourcingDecisionOutcome, DomainError> =

    let isInfeasible =
        match situation.Feasibility with
        | ProcurementFeasibility.Infeasible _ -> true
        | _ -> false

    // Check 1: Infeasible cannot have directive Execute
    if isInfeasible && determination.Directive = SourcingDirective.ExecuteProcurementRecommendation then
        Ok(SourcingDecisionOutcome.Reject "DirectiveIntentMismatch: Infeasible situation cannot execute recommendation")

    // Check 2: Low confidence requires escalation
    elif
        situation.OverallConfidence = RecommendationConfidence.Low
        && determination.Governance = GovernancePosture.AutonomousExecutionPermitted
    then
        Ok(
            SourcingDecisionOutcome.Reject
                "LowConfidenceRequiresEscalation: Autonomous execution prohibited under low confidence"
        )

    // Check 3: Directive conflict with HaltAndEscalate
    elif
        determination.Governance = GovernancePosture.ExecutiveEscalationRequired
        && determination.Directive = SourcingDirective.ExecuteProcurementRecommendation
    then
        Ok(
            SourcingDecisionOutcome.Reject
                "DirectiveGovernanceMismatch: Executive escalation required cannot execute without approval"
        )

    else
        Ok SourcingDecisionOutcome.Approve
