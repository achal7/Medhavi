/// Business Rules for SE-S-070 Production Schedule
/// Traces to: Supply Intelligence Specification §7 (BR-S-070, BR-S-220, BR-S-222, BR-S-223, BR-S-228, BR-S-229, BR-S-246, BR-S-248, BR-S-120..128, 130, 135)
module Medhavi.Supply.ScheduleProduction.ProductionSchedule.Rules

open System
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers
open Model

/// BR-S-070: Production Schedule Aggregate Identity
let validateAggregateIdentity
    (resourceId: PhysicalResourceId)
    (periodId: PlanningPeriodId)
    : Result<unit, DomainError> =
    if String.IsNullOrWhiteSpace(PhysicalResourceId.value resourceId) then
        Error(ruleViolationWithDetail Rules.productionScheduleAggregateIdentity "Physical Resource Identifier cannot be empty")
    elif String.IsNullOrWhiteSpace(PlanningPeriodId.value periodId) then
        Error(ruleViolationWithDetail Rules.productionScheduleAggregateIdentity "Planning Period Identifier cannot be empty")
    else
        Ok()

/// BR-S-220: Production Schedule Creation Eligibility
let validateCreationEligibility
    (hasPublishedSupplyPlan: bool)
    (isPolicyCurrent: bool)
    (isResourceActive: bool)
    (isCalendarAvailable: bool)
    : Result<unit, DomainError> =
    if not hasPublishedSupplyPlan then
        Error(ruleViolationWithDetail Rules.productionScheduleCreationEligibility "A Published Supply Plan (SE-S-020) is required before schedule creation")
    elif not isPolicyCurrent then
        Error(ruleViolationWithDetail Rules.productionScheduleCreationEligibility "PO-S-080 Scheduling Policy must be current")
    elif not isResourceActive then
        Error(ruleViolationWithDetail Rules.productionScheduleCreationEligibility "The target Physical Resource must be active and recognized")
    elif not isCalendarAvailable then
        Error(ruleViolationWithDetail Rules.productionScheduleCreationEligibility "The resource calendar must be available")
    else
        Ok()

/// BR-S-222: Single Active Production Schedule per Resource and Period
let validateSingleActiveSchedule
    (existingActiveOpt: ProductionScheduleAggregate option)
    (targetVersion: int)
    : Result<unit, DomainError> =
    match existingActiveOpt with
    | Some existing when existing.VersionNumber >= targetVersion ->
        Error(ruleViolationWithDetail Rules.singleActiveProductionSchedulePerResource $"An Active schedule version ({existing.VersionNumber}) already exists for this resource and period")
    | _ -> Ok()

/// BR-S-223: Production Schedule State Change Immutability
let validateStateChangeImmutability
    (currentState: ProductionScheduleLifecycleState)
    (targetState: ProductionScheduleLifecycleState)
    : Result<unit, DomainError> =
    match currentState, targetState with
    | ProductionScheduleLifecycleState.Draft, ProductionScheduleLifecycleState.Active -> Ok()
    | ProductionScheduleLifecycleState.Active, ProductionScheduleLifecycleState.Superseded -> Ok()
    | ProductionScheduleLifecycleState.Active, ProductionScheduleLifecycleState.Archived -> Ok()
    | ProductionScheduleLifecycleState.Superseded, ProductionScheduleLifecycleState.Archived -> Ok()
    | ProductionScheduleLifecycleState.Archived, _ ->
        Error(ruleViolationWithDetail Rules.productionScheduleStateChangeImmutability "Archived Production Schedule is terminal and immutable")
    | ProductionScheduleLifecycleState.Superseded, _ ->
        Error(ruleViolationWithDetail Rules.productionScheduleStateChangeImmutability "Superseded Production Schedule cannot be transitioned except to Archived")
    | ProductionScheduleLifecycleState.Active, ProductionScheduleLifecycleState.Draft ->
        Error(ruleViolationWithDetail Rules.productionScheduleStateChangeImmutability "Active Production Schedule cannot regress to Draft")
    | _ -> Ok()

/// BR-S-135: Completeness of Production Schedule
let validateScheduleCompleteness (schedule: ProductionScheduleAggregate) : Result<unit, DomainError> =
    if schedule.Activities.IsEmpty then
        Error(ruleViolationWithDetail Rules.scheduleCompletenessRequirement "Production Schedule cannot have empty activities")
    else
        let rec checkSequence (lastEndOpt: Timestamp option) (acts: ProductionActivity list) =
            match acts with
            | [] -> Ok()
            | act :: tail ->
                let (ActivityId idStr) = act.ActivityId
                if Timestamp.isBefore act.EndTime act.StartTime || Timestamp.isEqual act.EndTime act.StartTime then
                    Error(ruleViolationWithDetail Rules.scheduleCompletenessRequirement $"Activity {idStr} has EndTime <= StartTime")
                elif Quantity.value act.Quantity <= 0.0M then
                    Error(ruleViolationWithDetail Rules.scheduleCompletenessRequirement $"Activity {idStr} has non-positive quantity")
                else
                    checkSequence (Some act.EndTime) tail

        checkSequence None schedule.Activities

/// BR-S-124 & BR-S-130: Material Feasibility Check
let validateMaterialFeasibility (feasibility: FeasibilityEvaluation) : Result<unit, DomainError> =
    match feasibility.MaterialAvailability with
    | DimensionFeasibility.Violated reason ->
        Error(ruleViolationWithDetail Rules.materialAvailabilityConfirmation $"Hard constraint violated: material unavailable ({reason})")
    | _ -> Ok()

/// BR-S-125 & BR-S-130: Finite Capacity Feasibility Check
let validateCapacityFeasibility (feasibility: FeasibilityEvaluation) : Result<unit, DomainError> =
    match feasibility.FiniteCapacity with
    | DimensionFeasibility.Violated reason ->
        Error(ruleViolationWithDetail Rules.capacityAvailabilityVerification $"Hard constraint violated: capacity exceeded ({reason})")
    | _ -> Ok()
