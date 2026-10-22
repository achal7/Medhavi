/// AB-S-060 — Create Production Schedule Aggregate Behavior
/// Traces to: Supply Intelligence Specification §4.3.1 (AB-S-060, EV-S-060)
module Medhavi.Supply.ScheduleProduction.ProductionSchedule.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model
open Policies
open Algorithms
open Rules
open Decisions

type CreateProductionScheduleContext =
    { Input: SchedulingInput
      ScheduleType: ScheduleType
      PlanningPeriodId: PlanningPeriodId
      SourceSupplyPlanVersion: int
      SchedulingPolicyVersion: int
      ExistingActiveSchedule: ProductionScheduleAggregate option
      Timestamp: Timestamp }

/// AB-S-060: Create Production Schedule
/// Evaluates algorithms, decisions, and rules, transitions state, and publishes EV-S-060
let createProductionSchedule
    (ctx: CreateProductionScheduleContext)
    : Result<ProductionScheduleAggregate * ProductionScheduleEvent list, DomainError> =

    result {
        // Step 1: Precondition validations (BR-S-070, BR-S-220)
        do! validateAggregateIdentity ctx.Input.ResourceId ctx.PlanningPeriodId
        do! validateCreationEligibility true true true true

        // Step 2: Algorithmic Construction (BA-S-040)
        let constructed = constructProductionSchedule ctx.Input

        // Step 3: Sequencing Decision (DE-S-061)
        let! sequenceOutcome =
            determineProductionSequence
                constructed.Activities
                constructed.Changeovers
                constructed.Feasibility
                ctx.Input.Policy

        // Step 4: Feasibility Assessment Decision (DE-S-062)
        let! feasibilityOutcome = assessScheduleFeasibility constructed.Feasibility ctx.Input.Policy

        match sequenceOutcome, feasibilityOutcome with
        | SequenceDecisionOutcome.NoFeasibleSequence reason, _ ->
            return! Error(ruleViolationWithDetail ArsIdentifiers.Rules.productionSequenceDeterminationGovernance $"No feasible sequence could be constructed: {reason}")

        | _, FeasibilityDecisionOutcome.Infeasible reasons ->
            let reasonStr = String.concat "; " reasons
            return! Error(ruleViolationWithDetail ArsIdentifiers.Rules.scheduleFeasibilityAssessmentGovernance $"Schedule is infeasible: {reasonStr}")

        | SequenceDecisionOutcome.OptimalSequenceSelected(activities, changeovers), _ ->
            // Step 5: Versioning and Atomic Activation
            let newVersion =
                match ctx.ExistingActiveSchedule with
                | Some prior -> prior.VersionNumber + 1
                | None -> 1

            let supersededVersionOpt = ctx.ExistingActiveSchedule |> Option.map (fun s -> s.VersionNumber)

            let activatedSchedule: ProductionScheduleAggregate =
                { ResourceId = ctx.Input.ResourceId
                  PlanningPeriodId = ctx.PlanningPeriodId
                  ScheduleType = ctx.ScheduleType
                  VersionNumber = newVersion
                  LifecycleState = ProductionScheduleLifecycleState.Active
                  Activities = activities
                  Changeovers = changeovers
                  TotalScheduledHours = constructed.TotalScheduledHours
                  TotalChangeoverHours = constructed.TotalChangeoverHours
                  OvertimeHours = constructed.OvertimeHours
                  Feasibility = constructed.Feasibility
                  Confidence = constructed.Confidence
                  Stability = constructed.Stability
                  SourceSupplyPlanVersion = ctx.SourceSupplyPlanVersion
                  SchedulingPolicyVersion = ctx.SchedulingPolicyVersion
                  DecisionRationale =
                      $"Production Schedule v{newVersion} activated for {PhysicalResourceId.value ctx.Input.ResourceId}. "
                      + $"Feasibility: {constructed.Feasibility.Overall}. Stability Score: {constructed.Stability.StabilityScore:F2}. "
                      + $"Activities: {activities.Length}, Changeovers: {changeovers.Length}."
                  EvidenceReferences =
                      [ $"SupplyPlan-v{ctx.SourceSupplyPlanVersion}"
                        $"SchedulingPolicy-v{ctx.SchedulingPolicyVersion}" ]
                  CreatedTimestamp = ctx.Timestamp
                  ActivatedTimestamp = Some ctx.Timestamp
                  SupersededTimestamp = None
                  SupersededByVersion = None }

            // Emit Enterprise Event EV-S-060
            let event = ProductionScheduleEvent.ProductionScheduleActivated(activatedSchedule, supersededVersionOpt)

            return (activatedSchedule, [ event ])
    }

let decide
    (reqs: PlannedProductionRequirementFact list)
    (avail: ResourceAvailabilityFact)
    (chgMatrix: ChangeoverMatrixFact)
    (priorOpt: ActiveScheduleFact option)
    (policy: SchedulingPolicyConfig)
    (cmd: CreateProductionScheduleCmd)
    (stateOpt: ProductionScheduleAggregate option)
    : Result<Decision<ProductionScheduleAggregate, ProductionScheduleEvent>, DomainError> =
    let input: Algorithms.SchedulingInput =
        { ResourceId = cmd.ResourceId
          PeriodStart = cmd.Timestamp
          PeriodEnd = Timestamp.addDays 14.0 cmd.Timestamp
          Requirements = reqs
          Availability = avail
          ChangeoverMatrix = chgMatrix
          PriorSchedule = priorOpt
          Policy = policy }

    let ctx: CreateProductionScheduleContext =
        { Input = input
          ScheduleType = cmd.ScheduleType
          PlanningPeriodId = cmd.PlanningPeriodId
          SourceSupplyPlanVersion = cmd.SourceSupplyPlanVersion
          SchedulingPolicyVersion = cmd.SchedulingPolicyVersion
          ExistingActiveSchedule = stateOpt
          Timestamp = cmd.Timestamp }

    match createProductionSchedule ctx with
    | Error err -> Error err
    | Ok(aggregate, events) ->
        let traceOutcome = { Outcome = "ScheduleCreated"; Evaluations = [] }
        let decision =
            buildDecisionWithTrace
                evolve
                stateOpt
                events
                ArsIdentifiers.Decisions.determineProductionSequence.Id
                []
                ArsIdentifiers.Capabilities.scheduleProduction.Id
                traceOutcome
                (Some ArsIdentifiers.Policies.schedulingPolicy.Id)
                (Some 1)
                [ ArsIdentifiers.SemanticObjects.productionSchedule.Id ]
                (Some $"Production schedule created for {PhysicalResourceId.value cmd.ResourceId}")
        Ok decision
