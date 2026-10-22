/// Business Rules for SE-S-071 Published Production Schedule
/// Traces to: Supply Intelligence Specification §7 (BR-S-071, BR-S-221, BR-S-224, BR-S-225, BR-S-226, BR-S-227, BR-S-244, BR-S-132, BR-S-133, BR-S-134)
module Medhavi.Supply.ScheduleProduction.PublishedProductionSchedule.Rules

open System
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers
open Model
open Policies

/// BR-S-071: Published Production Schedule Identity
let validateAggregateIdentity
    (scopeId: PlanningScopeId)
    (periodId: PlanningPeriodId)
    : Result<unit, DomainError> =
    if String.IsNullOrWhiteSpace(PlanningScopeId.value scopeId) then
        Error(ruleViolationWithDetail Rules.publishedProductionScheduleIdentity "Planning Scope Identifier cannot be empty")
    elif String.IsNullOrWhiteSpace(PlanningPeriodId.value periodId) then
        Error(ruleViolationWithDetail Rules.publishedProductionScheduleIdentity "Planning Period Identifier cannot be empty")
    else
        Ok()

/// BR-S-221: Production Schedule Publication Eligibility
let validatePublicationEligibility
    (hasActiveSchedules: bool)
    (isPolicyCurrent: bool)
    : Result<unit, DomainError> =
    if not hasActiveSchedules then
        Error(ruleViolationWithDetail Rules.productionSchedulePublicationEligibility "Active Production Schedules (SE-S-070) must exist before baseline publication")
    elif not isPolicyCurrent then
        Error(ruleViolationWithDetail Rules.productionSchedulePublicationEligibility "PO-S-080 Scheduling Policy must be current")
    else
        Ok()

/// BR-S-225 & BR-S-132: Published Production Schedule Immutability
let validateBaselineImmutability
    (currentState: PublishedScheduleLifecycleState)
    (targetState: PublishedScheduleLifecycleState)
    : Result<unit, DomainError> =
    match currentState, targetState with
    | PublishedScheduleLifecycleState.Draft, PublishedScheduleLifecycleState.Published -> Ok()
    | PublishedScheduleLifecycleState.Published, PublishedScheduleLifecycleState.Superseded -> Ok()
    | PublishedScheduleLifecycleState.Published, _ ->
        Error(ruleViolationWithDetail Rules.publishedProductionScheduleImmutability "Published Production Schedule is immutable; cannot be modified or regressed")
    | PublishedScheduleLifecycleState.Superseded, _ ->
        Error(ruleViolationWithDetail Rules.publishedProductionScheduleImmutability "Superseded baseline is terminal and permanently immutable")
    | _ -> Ok()

/// BR-S-227: Production Schedule Version Monotonicity
let validateVersionMonotonicity (newVersion: int) (priorVersionOpt: int option) : Result<unit, DomainError> =
    match priorVersionOpt with
    | Some prior when newVersion <= prior ->
        Error(ruleViolationWithDetail Rules.productionScheduleVersionMonotonicity $"New version ({newVersion}) must be strictly greater than prior version ({prior})")
    | _ -> Ok()

/// BR-S-133: Feasibility Threshold for Publication
let validateFeasibilityThreshold (summary: FeasibilitySummary) (policy: PublicationPolicyConfig) : Result<unit, DomainError> =
    if summary.InfeasibleCount > policy.MaxInfeasibleAllowed && not policy.AllowInfeasibleWithFlags then
        Error(ruleViolationWithDetail Rules.schedulePublicationFeasibilityThreshold $"Baseline contains {summary.InfeasibleCount} infeasible schedules; publication blocked")
    else
        Ok()

/// BR-S-134: Stability Threshold for Publication
let validateStabilityThreshold (summary: StabilitySummary) (policy: PublicationPolicyConfig) : Result<unit, DomainError> =
    if summary.AverageStabilityScore < policy.StabilityScoreThreshold then
        Error(ruleViolationWithDetail Rules.schedulePublicationStabilityThreshold $"Average stability score ({summary.AverageStabilityScore:F2}) is below governed threshold ({policy.StabilityScoreThreshold:F2})")
    else
        Ok()
