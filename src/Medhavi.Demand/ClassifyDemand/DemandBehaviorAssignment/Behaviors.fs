/// AB-D-012 — Classify Demand Behavior Behaviors & Deciders
module Medhavi.Demand.ClassifyDemand.DemandBehaviorAssignment.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
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
        ArsIdentifiers.Capabilities.classifyDemand.Id
        decisionOutcome
        policyId
        policyVersion
        [ ArsIdentifiers.SemanticObjects.demandBehaviorAssignment.Id ]
        (Some summary)

/// AB-D-012: Classify Demand Behavior Decider
let classifyDemandBehavior
    (policy: ClassificationPolicy)
    : Decide<DemandBehaviorAssignment, ClassifyDemandBehaviorCmd, DemandBehaviorEvent> =
    fun (cmd: ClassifyDemandBehaviorCmd) (state: DemandBehaviorAssignment option) ->
        result {
            let input: ClassificationRuleInput =
                { Item = cmd.Item
                  Location = cmd.Location
                  Dimension = cmd.Dimension
                  DemandQuantities = cmd.DemandQuantities
                  Policy = policy }

            let! decision = Decisions.evaluateBehaviorClassification Rules.classificationRules input
            let comp = decision.Outcome

            let previousClassification = state |> Option.map(fun a -> a.CurrentClassification)
            let traceId = Guid.NewGuid().ToString()

            let changeEvent: BehaviorChangeEvent =
                { Timestamp = cmd.ClassificationTime
                  FromClassification = previousClassification
                  ToClassification = comp.Classification
                  StatisticalFeatures = comp.Features
                  Confidence = comp.Confidence
                  Rationale = comp.Rationale
                  PolicyVersion = policy.PolicyVersion
                  DecisionTraceId = traceId }

            let updatedAssignment: DemandBehaviorAssignment =
                { AssignmentId = cmd.AssignmentId
                  Item = cmd.Item
                  Location = cmd.Location
                  Dimension = cmd.Dimension
                  CurrentClassification = comp.Classification
                  StatisticalFeatures = comp.Features
                  ClassificationConfidence = comp.Confidence
                  AssignmentRationale = comp.Rationale
                  PolicyVersion = policy.PolicyVersion
                  ChangeEvents =
                    match state with
                    | Some ex -> changeEvent :: ex.ChangeEvents
                    | None -> [ changeEvent ]
                  LastUpdated = cmd.ClassificationTime }

            let events = [ DemandBehaviorClassified(updatedAssignment, previousClassification, changeEvent) ]

            let summary =
                sprintf
                    "Demand behavior for SKU %s at Location %s classified as %s (ADI: %.2f, CV²: %.2f, Confidence: %s)"
                    (ItemId.value cmd.Item)
                    (LocationId.value cmd.Location)
                    comp.Classification.AsString
                    (comp.Features |> Option.map(fun f -> f.AverageDemandInterval) |> Option.defaultValue 0.0m)
                    (comp.Features |> Option.map(fun f -> f.SquaredCoefficientOfVariation) |> Option.defaultValue 0.0m)
                    comp.Confidence

            return
                buildDecisionTrace
                    (Some policy.PolicyId)
                    (Some policy.Version)
                    ArsIdentifiers.Decisions.determineBehaviorClassification.Id
                    decision
                    state
                    events
                    summary
        }

/// AB-D-012: Manual Planner Override Decider (PO-D-038)
let overrideDemandBehavior
    (overridePolicy: ClassificationOverridePolicy)
    : Decide<DemandBehaviorAssignment, OverrideDemandBehaviorCmd, DemandBehaviorEvent> =
    fun (cmd: OverrideDemandBehaviorCmd) (state: DemandBehaviorAssignment option) ->
        result {
            let input: OverrideRuleInput =
                { PlannerId = cmd.PlannerId
                  Justification = cmd.Justification
                  Policy = overridePolicy }

            let! decision = Decisions.evaluateOverride Rules.overrideRules input cmd.NewClassification

            let previousClassification = state |> Option.map(fun a -> a.CurrentClassification)
            let existingFeatures = state |> Option.bind(fun a -> a.StatisticalFeatures)
            let traceId = Guid.NewGuid().ToString()

            let changeEvent: BehaviorChangeEvent =
                { Timestamp = cmd.OverrideTime
                  FromClassification = previousClassification
                  ToClassification = cmd.NewClassification
                  StatisticalFeatures = existingFeatures
                  Confidence = "High"
                  Rationale = $"Manual planner override by {cmd.PlannerId}: {cmd.Justification}"
                  PolicyVersion = overridePolicy.PolicyVersion
                  DecisionTraceId = traceId }

            let updatedAssignment: DemandBehaviorAssignment =
                { AssignmentId = cmd.AssignmentId
                  Item = cmd.Item
                  Location = cmd.Location
                  Dimension = cmd.Dimension
                  CurrentClassification = cmd.NewClassification
                  StatisticalFeatures = existingFeatures
                  ClassificationConfidence = "High"
                  AssignmentRationale = changeEvent.Rationale
                  PolicyVersion = overridePolicy.PolicyVersion
                  ChangeEvents =
                    match state with
                    | Some ex -> changeEvent :: ex.ChangeEvents
                    | None -> [ changeEvent ]
                  LastUpdated = cmd.OverrideTime }

            let events = [ DemandBehaviorOverridden(updatedAssignment, previousClassification, changeEvent) ]

            let summary =
                sprintf
                    "Manual override by planner %s for SKU %s at Location %s: new classification %s. Justification: '%s'"
                    cmd.PlannerId
                    (ItemId.value cmd.Item)
                    (LocationId.value cmd.Location)
                    cmd.NewClassification.AsString
                    cmd.Justification

            return
                buildDecisionTrace
                    (Some overridePolicy.PolicyId)
                    (Some overridePolicy.Version)
                    ArsIdentifiers.Decisions.determineBehaviorClassification.Id
                    decision
                    state
                    events
                    summary
        }
