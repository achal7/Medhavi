/// DE-S-063 — Publish Production Schedule Decision
/// Traces to: Supply Intelligence Specification §6 (DE-S-063)
module Medhavi.Supply.ScheduleProduction.PublishedProductionSchedule.Decisions

open System
open Medhavi.Foundation.Failure
open Medhavi.Supply
open Model
open Policies

[<RequireQualifiedAccess>]
type PublicationDecisionOutcome =
    | Publish
    | PublishWithFlags of Flags: string list
    | DoNotPublish of Reasons: string list

/// DE-S-063 — Publish Production Schedule
/// Publication Decision: Final gate between schedule creation and authoritative baseline release
let evaluatePublicationDecision
    (feasibility: FeasibilitySummary)
    (stability: StabilitySummary)
    (activeCount: int)
    (trigger: PublicationTriggerType)
    (policy: PublicationPolicyConfig)
    : Result<PublicationDecisionOutcome, DomainError> =

    // Check 1: Completeness
    if activeCount = 0 then
        Ok(PublicationDecisionOutcome.DoNotPublish [ "No active production schedules exist in scope" ])

    // Check 2: Hard Infeasibility
    elif feasibility.InfeasibleCount > 0 && trigger <> PublicationTriggerType.GovernanceOverride then
        Ok(PublicationDecisionOutcome.DoNotPublish [ $"Schedule baseline contains {feasibility.InfeasibleCount} infeasible schedules" ])

    // Check 3: Governance Override or Emergency Replan
    elif trigger = PublicationTriggerType.GovernanceOverride then
        Ok(PublicationDecisionOutcome.PublishWithFlags [ "Published under Production Planning Manager Governance Override" ])

    elif trigger = PublicationTriggerType.EmergencyReplan then
        Ok(PublicationDecisionOutcome.PublishWithFlags [ "Published under Emergency Replan Trigger" ])

    // Check 4: Stability Threshold
    elif stability.AverageStabilityScore < policy.StabilityScoreThreshold then
        Ok(PublicationDecisionOutcome.DoNotPublish [
            $"Average schedule stability score ({stability.AverageStabilityScore:F2}) is below threshold ({policy.StabilityScoreThreshold:F2})"
        ])

    // Check 5: Conditional Feasibility
    elif feasibility.ConditionallyFeasibleCount > 0 then
        let flags = feasibility.TopConstraints
        Ok(PublicationDecisionOutcome.PublishWithFlags flags)

    else
        Ok PublicationDecisionOutcome.Publish
