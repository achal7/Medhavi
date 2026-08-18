/// AB-D-013 — Prioritize Planning Entity Behaviors & Deciders
module Medhavi.Demand.PrioritizeDemand.PlanningPriorityAssignment.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Demand
open Medhavi.Demand.ArsIdentifiers
open Policies
open Model
open Decisions
open Rules

let private buildDecisionTrace policyId policyVersion decisionId decisionOutcome state events summary =
    buildDecisionWithTrace
        evolve
        state
        events
        decisionId
        []
        ArsIdentifiers.Capabilities.prioritizeDemand.Id
        decisionOutcome
        policyId
        policyVersion
        [ ArsIdentifiers.SemanticObjects.planningPriorityAssignment.Id ]
        (Some summary)

/// AB-D-013: Prioritize Planning Entity Decider
let prioritizePlanningEntity
    (policy: PrioritizationPolicy)
    : Decide<PlanningPriorityAssignment, PrioritizePlanningEntityCmd, PlanningPriorityEvent> =
    fun (cmd: PrioritizePlanningEntityCmd) (state: PlanningPriorityAssignment option) ->
        result {
            let input: PrioritizationRuleInput =
                { EntityType = cmd.EntityType
                  EntityId = cmd.EntityId
                  RevenueContribution = cmd.RevenueContribution
                  StrategicImportance = cmd.StrategicImportance
                  RiskExposure = cmd.RiskExposure
                  ContractualObligation = cmd.ContractualObligation
                  Policy = policy }

            let! decision = Decisions.evaluatePlanningPriority Rules.prioritizationRules input
            let comp = decision.Outcome

            let previousPriority = state |> Option.map (fun a -> a.CurrentPriority)
            let traceId = Guid.NewGuid().ToString()

            let changeEvent: PriorityChangeEvent =
                { Timestamp = cmd.PrioritizationTime
                  FromPriority = previousPriority
                  ToPriority = comp.Priority
                  PriorityScore = comp.Score
                  DimensionBreakdown = comp.Breakdown
                  Rationale = comp.Rationale
                  BusinessValidity = comp.BusinessValidity
                  PolicyVersion = policy.PolicyVersion
                  DecisionTraceId = traceId }

            let updatedAssignment: PlanningPriorityAssignment =
                { AssignmentId = cmd.AssignmentId
                  EntityType = cmd.EntityType
                  EntityId = cmd.EntityId
                  CurrentPriority = comp.Priority
                  PriorityScore = comp.Score
                  DimensionBreakdown = comp.Breakdown
                  DecisionRationale = comp.Rationale
                  BusinessValidity = comp.BusinessValidity
                  PolicyVersion = policy.PolicyVersion
                  ChangeEvents =
                    match state with
                    | Some ex -> changeEvent :: ex.ChangeEvents
                    | None -> [ changeEvent ]
                  LastUpdated = cmd.PrioritizationTime }

            let events =
                [ PlanningPriorityAssigned(updatedAssignment, previousPriority, changeEvent) ]

            let summary =
                sprintf
                    "Planning priority for %s '%s' determined as %s (Score: %.2f/100). %s"
                    cmd.EntityType.AsString
                    cmd.EntityId
                    comp.Priority.AsString
                    comp.Score
                    comp.Rationale

            return
                buildDecisionTrace
                    (Some policy.PolicyId)
                    (Some policy.Version)
                    ArsIdentifiers.Decisions.determinePlanningPriority.Id
                    decision
                    state
                    events
                    summary
        }

/// AB-D-013: Manual Planner Override Decider (PO-D-040)
let overridePlanningPriority
    (overridePolicy: PrioritizationOverridePolicy)
    : Decide<PlanningPriorityAssignment, OverridePlanningPriorityCmd, PlanningPriorityEvent> =
    fun (cmd: OverridePlanningPriorityCmd) (state: PlanningPriorityAssignment option) ->
        result {
            let input: OverrideRuleInput =
                { PlannerId = cmd.PlannerId
                  Justification = cmd.Justification
                  Policy = overridePolicy }

            let! decision = Decisions.evaluateOverride Rules.overrideRules input cmd.NewPriority

            let previousPriority = state |> Option.map (fun a -> a.CurrentPriority)
            let existingScore = state |> Option.map (fun a -> a.PriorityScore) |> Option.defaultValue 0.0m
            let existingBreakdown =
                state
                |> Option.map (fun a -> a.DimensionBreakdown)
                |> Option.defaultValue
                    { RevenueScore = 0.0m
                      StrategyScore = 0.0m
                      RiskScore = 0.0m
                      ContractualScore = 0.0m }

            let traceId = Guid.NewGuid().ToString()

            let changeEvent: PriorityChangeEvent =
                { Timestamp = cmd.OverrideTime
                  FromPriority = previousPriority
                  ToPriority = cmd.NewPriority
                  PriorityScore = existingScore
                  DimensionBreakdown = existingBreakdown
                  Rationale = $"Manual planner override by {cmd.PlannerId}: {cmd.Justification}"
                  BusinessValidity = "Valid per authorized planner override"
                  PolicyVersion = overridePolicy.PolicyVersion
                  DecisionTraceId = traceId }

            let updatedAssignment: PlanningPriorityAssignment =
                { AssignmentId = cmd.AssignmentId
                  EntityType = cmd.EntityType
                  EntityId = cmd.EntityId
                  CurrentPriority = cmd.NewPriority
                  PriorityScore = existingScore
                  DimensionBreakdown = existingBreakdown
                  DecisionRationale = changeEvent.Rationale
                  BusinessValidity = changeEvent.BusinessValidity
                  PolicyVersion = overridePolicy.PolicyVersion
                  ChangeEvents =
                    match state with
                    | Some ex -> changeEvent :: ex.ChangeEvents
                    | None -> [ changeEvent ]
                  LastUpdated = cmd.OverrideTime }

            let events =
                [ PlanningPriorityOverridden(updatedAssignment, previousPriority, changeEvent) ]

            let summary =
                sprintf
                    "Manual priority override by planner %s for %s '%s': new priority %s. Justification: '%s'"
                    cmd.PlannerId
                    cmd.EntityType.AsString
                    cmd.EntityId
                    cmd.NewPriority.AsString
                    cmd.Justification

            return
                buildDecisionTrace
                    (Some overridePolicy.PolicyId)
                    (Some overridePolicy.Version)
                    ArsIdentifiers.Decisions.determinePlanningPriority.Id
                    decision
                    state
                    events
                    summary
        }
