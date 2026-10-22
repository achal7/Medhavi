/// DE-S-052: Publish Procurement Plan
/// Traces to: Supply Intelligence Specification §6 (DE-S-052)
module Medhavi.Supply.ProcureMaterials.ProcurementPlan.Decisions

open Medhavi.Foundation.Failure

[<RequireQualifiedAccess>]
type PlanPublicationDecisionOutcome =
    | Publish
    | PublishWithFlags of Flags: string list
    | Hold of Reason: string

/// DE-S-052 — Publish Procurement Plan Decision
let evaluatePublication
        (isComplete: bool)
        (confidenceScore: decimal)
        (unresolvedFeasibilityCount: int)
        (policy: PlanPublicationPolicyConfig)
        : Result<PlanPublicationDecisionOutcome, DomainError> =

        if not isComplete then
            Ok(PlanPublicationDecisionOutcome.Hold "Completeness threshold violated: active requirements are uncovered (BR-S-111)")
        elif confidenceScore < policy.MinimumConfidenceThreshold then
            Ok(PlanPublicationDecisionOutcome.Hold $"Average confidence score {confidenceScore} is below minimum threshold {policy.MinimumConfidenceThreshold} (BR-S-112)")
        elif unresolvedFeasibilityCount > 0 && confidenceScore < policy.GovernedConfidenceThreshold then
            let flags =
                [ $"Confidence score ({confidenceScore:F2}) below governed threshold ({policy.GovernedConfidenceThreshold:F2})"
                  $"{unresolvedFeasibilityCount} unresolved feasibility issues present" ]
            Ok(PlanPublicationDecisionOutcome.PublishWithFlags flags)
        elif confidenceScore < policy.GovernedConfidenceThreshold then
            let flags = [ $"Confidence score ({confidenceScore:F2}) below governed threshold ({policy.GovernedConfidenceThreshold:F2})" ]
            Ok(PlanPublicationDecisionOutcome.PublishWithFlags flags)
        elif unresolvedFeasibilityCount > 0 then
            let flags = [ $"{unresolvedFeasibilityCount} recommendations with documented feasibility constraints" ]
            Ok(PlanPublicationDecisionOutcome.PublishWithFlags flags)
        else
            Ok PlanPublicationDecisionOutcome.Publish
