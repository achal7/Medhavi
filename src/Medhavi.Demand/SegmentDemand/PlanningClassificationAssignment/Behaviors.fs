/// Segment Demand Aggregate Behaviors
/// Traces to: AB-D-011 Classify Planning Entity (Specification Chapter 4.3.1)
module Medhavi.Demand.SegmentDemand.PlanningClassificationAssignment.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Demand
open Medhavi.Demand.ArsIdentifiers
open Model
open Policies
open Rules
open Decisions
open Algorithms

let private buildDecisionTrace policyId policyVersion decisionId decisionOutcome state events summary =
    buildDecisionWithTrace
        evolve
        state
        events
        decisionId
        []
        ArsIdentifiers.Capabilities.segmentDemand.Id
        decisionOutcome
        policyId
        policyVersion
        [ ArsIdentifiers.SemanticObjects.planningClassificationAssignment.Id ]
        (Some summary)

/// AB-D-011: Classify Planning Entity
let classifyPlanningEntity
    (policy: SegmentationPolicy)
    : Decide<PlanningClassificationAssignment, ClassifyPlanningEntityCmd, PlanningClassificationEvent> =
    fun (cmd: ClassifyPlanningEntityCmd) (state: PlanningClassificationAssignment option) ->
        result {
            let input: ClassificationRuleInput =
                { EntityId = cmd.EntityId
                  ClassificationType = cmd.ClassificationType
                  VolumeOrRevenuePercentage = cmd.VolumeOrRevenuePercentage
                  HistoricalDemandValues = cmd.HistoricalDemandValues
                  AnalogItemId = cmd.AnalogItemId
                  Policy = policy }

            let! decision = Decisions.evaluatePlanningClassification Rules.classificationRules input

            let comp = decision.Outcome.Computation
            let previousClassification = state |> Option.map(fun a -> a.CurrentClassification)

            let traceId = Guid.NewGuid().ToString()

            let changeEvent: AssignmentChangeEvent =
                { Timestamp = cmd.ClassificationTime
                  FromClassification = previousClassification
                  ToClassification = comp.Classification
                  ClassificationScore = comp.Score
                  ClassificationConfidence = comp.Confidence
                  Rationale = comp.Rationale
                  PolicyVersion = policy.PolicyVersion
                  DecisionTraceId = traceId }

            let updatedAssignment: PlanningClassificationAssignment =
                { AssignmentId = cmd.AssignmentId
                  EntityType = cmd.EntityType
                  EntityId = cmd.EntityId
                  ClassificationType = cmd.ClassificationType
                  CurrentClassification = comp.Classification
                  AnalogItemReference = cmd.AnalogItemId
                  ClassificationScore = comp.Score
                  ClassificationConfidence = comp.Confidence
                  AssignmentRationale = comp.Rationale
                  PolicyVersion = policy.PolicyVersion
                  ChangeEvents =
                    match state with
                    | Some ex -> changeEvent :: ex.ChangeEvents
                    | None -> [ changeEvent ]
                  LastUpdated = cmd.ClassificationTime }

            let events = [ PlanningClassificationAssigned(updatedAssignment, previousClassification, changeEvent) ]

            let summary =
                sprintf
                    "Planning classification %s evaluated for %s '%s' under scheme %s: %s (Score: %.2f, Confidence: %s)"
                    comp.Classification.AsString
                    cmd.EntityType.AsString
                    cmd.EntityId
                    cmd.ClassificationType.AsString
                    comp.Rationale
                    comp.Score
                    comp.Confidence

            return
                buildDecisionTrace
                    (Some policy.PolicyId)
                    (Some policy.Version)
                    ArsIdentifiers.Decisions.determinePlanningClassification.Id
                    decision
                    state
                    events
                    summary
        }

/// AB-D-011: Manual Planner Override (PO-D-036)
let overrideClassification
    (overridePolicy: SegmentationOverridePolicy)
    : Decide<PlanningClassificationAssignment, OverridePlanningClassificationCmd, PlanningClassificationEvent> =
    fun (cmd: OverridePlanningClassificationCmd) (state: PlanningClassificationAssignment option) ->
        result {
            let input: OverrideRuleInput =
                { PlannerId = cmd.PlannerId
                  Justification = cmd.Justification
                  Policy = overridePolicy }

            let! decision = Decisions.evaluateOverride Rules.overrideRules input cmd.NewClassification

            let previousClassification = state |> Option.map(fun a -> a.CurrentClassification)

            let score = state |> Option.map(fun a -> a.ClassificationScore) |> Option.defaultValue 0.0m

            let traceId = Guid.NewGuid().ToString()

            let changeEvent: AssignmentChangeEvent =
                { Timestamp = cmd.OverrideTime
                  FromClassification = previousClassification
                  ToClassification = cmd.NewClassification
                  ClassificationScore = score
                  ClassificationConfidence = "High"
                  Rationale = $"Manual override by {cmd.PlannerId}: {cmd.Justification}"
                  PolicyVersion = overridePolicy.PolicyVersion
                  DecisionTraceId = traceId }

            let updatedAssignment: PlanningClassificationAssignment =
                { AssignmentId = cmd.AssignmentId
                  EntityType = cmd.EntityType
                  EntityId = cmd.EntityId
                  ClassificationType = cmd.ClassificationType
                  CurrentClassification = cmd.NewClassification
                  AnalogItemReference = state |> Option.bind(fun a -> a.AnalogItemReference)
                  ClassificationScore = score
                  ClassificationConfidence = "High"
                  AssignmentRationale = changeEvent.Rationale
                  PolicyVersion = overridePolicy.PolicyVersion
                  ChangeEvents =
                    match state with
                    | Some ex -> changeEvent :: ex.ChangeEvents
                    | None -> [ changeEvent ]
                  LastUpdated = cmd.OverrideTime }

            let events = [ PlanningClassificationOverridden(updatedAssignment, previousClassification, changeEvent) ]

            let summary =
                sprintf
                    "Manual override applied by %s for %s '%s' (%s): new classification %s. Justification: '%s'"
                    cmd.PlannerId
                    cmd.EntityType.AsString
                    cmd.EntityId
                    cmd.ClassificationType.AsString
                    cmd.NewClassification.AsString
                    cmd.Justification

            return
                buildDecisionTrace
                    (Some overridePolicy.PolicyId)
                    (Some overridePolicy.Version)
                    ArsIdentifiers.Decisions.determinePlanningClassification.Id
                    decision
                    state
                    events
                    summary
        }
