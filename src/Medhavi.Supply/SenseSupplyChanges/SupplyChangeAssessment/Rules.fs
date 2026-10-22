/// Rules governing SE-S-090 Supply Change Assessment
/// Traces to: BR-S-090, BR-S-250, BR-S-251, BR-S-252, BR-S-253, BR-S-254, BR-S-255, BR-S-256, BR-S-257, BR-S-258
module Medhavi.Supply.SenseSupplyChanges.SupplyChangeAssessment.Rules

open System
open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers
open Model
open Algorithms

type AssessmentRuleInput =
    {
        Command: AssessSupplyChangeCmd
        ExistingAggregate: SupplyChangeAssessmentAggregate option
        Result: SupplyChangeAssessmentResult
    }

/// BR-S-090 — Supply Change Assessment Identity
let assessmentIdentity: Rule<AssessmentRuleInput> =
    Rule.create
        Rules.supplyChangeAssessmentIdentity.Id
        Rules.supplyChangeAssessmentIdentity.Explanation
        (fun input ->
            not(String.IsNullOrWhiteSpace(SupplyChangeAssessmentId.value input.Command.AssessmentId))
            && not(String.IsNullOrWhiteSpace input.Command.MonitoredEntityId))
        (fun input ->
            sprintf
                "AssessmentId: %s; EntityId: %s"
                (SupplyChangeAssessmentId.value input.Command.AssessmentId)
                input.Command.MonitoredEntityId)

/// BR-S-250 — Supply Change Assessment Update Eligibility
let updateEligibility: Rule<AssessmentRuleInput> =
    Rule.create
        Rules.supplyChangeAssessmentUpdateEligibility.Id
        Rules.supplyChangeAssessmentUpdateEligibility.Explanation
        (fun input ->
            match input.ExistingAggregate with
            | None -> true
            | Some existing -> existing.LifecycleState = SupplyChangeLifecycleState.Active)
        (fun input ->
            let stateStr =
                input.ExistingAggregate
                |> Option.map(fun e -> sprintf "%A" e.LifecycleState)
                |> Option.defaultValue "None"

            sprintf "ExistingLifecycleState: %s" stateStr)

/// BR-S-251 — Single Current Supply Change Assessment per Entity-Change Type
let singleCurrentAssessment: Rule<AssessmentRuleInput> =
    Rule.create
        Rules.singleCurrentSupplyChangeAssessment.Id
        Rules.singleCurrentSupplyChangeAssessment.Explanation
        (fun input ->
            match input.ExistingAggregate with
            | None -> true
            | Some existing ->
                existing.MonitoredEntityType = input.Command.MonitoredEntityType
                && existing.MonitoredEntityId = input.Command.MonitoredEntityId
                && existing.ChangeType = input.Command.ChangeType)
        (fun input ->
            sprintf
                "EntityType: %A; EntityId: %s; ChangeType: %A"
                input.Command.MonitoredEntityType
                input.Command.MonitoredEntityId
                input.Command.ChangeType)

/// BR-S-252 — Supply Change Assessment State Change Immutability
let stateChangeImmutability: Rule<AssessmentRuleInput> =
    Rule.create
        Rules.supplyChangeAssessmentStateChangeImmutability.Id
        Rules.supplyChangeAssessmentStateChangeImmutability.Explanation
        (fun input ->
            match input.ExistingAggregate with
            | None -> true
            | Some existing -> existing.LifecycleState <> SupplyChangeLifecycleState.Archived)
        (fun input ->
            let stateStr =
                input.ExistingAggregate
                |> Option.map(fun e -> sprintf "%A" e.LifecycleState)
                |> Option.defaultValue "New"

            sprintf "LifecycleState: %s" stateStr)

/// BR-S-253 — Supply Change Assessment History Immutability
let historyImmutability: Rule<AssessmentRuleInput> =
    Rule.create
        Rules.supplyChangeAssessmentHistoryImmutability.Id
        Rules.supplyChangeAssessmentHistoryImmutability.Explanation
        (fun input ->
            match input.ExistingAggregate with
            | None -> true
            | Some existing -> existing.History |> List.forall(fun h -> not(String.IsNullOrWhiteSpace h.EventId)))
        (fun input ->
            let count = input.ExistingAggregate |> Option.map(fun e -> e.History.Length) |> Option.defaultValue 0
            sprintf "ExistingHistoryCount: %d" count)

/// BR-S-254 — Change Detection Governance
let changeDetectionGovernance: Rule<AssessmentRuleInput> =
    Rule.create
        Rules.changeDetectionGovernance.Id
        Rules.changeDetectionGovernance.Explanation
        (fun input ->
            input.Result.DaysVariance =
                Math.Round(
                    (Timestamp.value input.Command.Signal.ObservedDate
                     - Timestamp.value input.Command.Baseline.CommittedDeliveryDate)
                        .TotalDays
                    |> decimal,
                    2
                ))
        (fun input -> sprintf "DaysVariance: %.2f" input.Result.DaysVariance)

/// BR-S-255 — Change Materiality Governance
let changeMaterialityGovernance: Rule<AssessmentRuleInput> =
    Rule.create
        Rules.changeMaterialityGovernance.Id
        Rules.changeMaterialityGovernance.Explanation
        (fun input ->
            match input.Result.Materiality with
            | PlanningMateriality.MaterialToPlanning
            | PlanningMateriality.NonMaterial -> true
            | PlanningMateriality.Unassessed -> false)
        (fun input -> sprintf "Materiality: %A" input.Result.Materiality)

/// BR-S-256 — Change Lifecycle Determinism
let changeLifecycleDeterminism: Rule<AssessmentRuleInput> =
    Rule.create
        Rules.changeLifecycleDeterminism.Id
        Rules.changeLifecycleDeterminism.Explanation
        (fun input ->
            match input.ExistingAggregate with
            | None ->
                input.Result.SuggestedEpistemicState = ChangeEpistemicState.Observed
                || input.Result.SuggestedEpistemicState = ChangeEpistemicState.Corroborated
            | Some existing ->
                match existing.CurrentEpistemicState, input.Result.SuggestedEpistemicState with
                | ChangeEpistemicState.Observed, ChangeEpistemicState.Observed
                | ChangeEpistemicState.Observed, ChangeEpistemicState.Corroborated
                | ChangeEpistemicState.Observed, ChangeEpistemicState.Confirmed
                | ChangeEpistemicState.Observed, ChangeEpistemicState.Resolved
                | ChangeEpistemicState.Corroborated, ChangeEpistemicState.Corroborated
                | ChangeEpistemicState.Corroborated, ChangeEpistemicState.Confirmed
                | ChangeEpistemicState.Corroborated, ChangeEpistemicState.Resolved
                | ChangeEpistemicState.Confirmed, ChangeEpistemicState.Confirmed
                | ChangeEpistemicState.Confirmed, ChangeEpistemicState.Resolved
                | ChangeEpistemicState.Resolved, ChangeEpistemicState.Resolved
                | ChangeEpistemicState.NoChange, _ -> true
                | _ -> false)
        (fun input ->
            let prev = input.ExistingAggregate |> Option.map(fun e -> e.CurrentEpistemicState)
            sprintf "Previous: %A; NewSuggested: %A" prev input.Result.SuggestedEpistemicState)

/// BR-S-257 — Supply Change Assessment Derivation Source
let derivationSource: Rule<AssessmentRuleInput> =
    Rule.create
        Rules.supplyChangeAssessmentDerivationSource.Id
        Rules.supplyChangeAssessmentDerivationSource.Explanation
        (fun input -> input.Result.EntityReference = input.Command.Signal.EntityReference)
        (fun input -> sprintf "EntityRef: %s" input.Result.EntityReference)

/// BR-S-258 — Change Signal Normalization
let signalNormalization: Rule<AssessmentRuleInput> =
    Rule.create
        Rules.changeSignalNormalization.Id
        Rules.changeSignalNormalization.Explanation
        (fun input ->
            input.Command.Signal.SourceReliabilityPrior >= 0.0m
            && input.Command.Signal.SourceReliabilityPrior <= 1.0m
            && not(String.IsNullOrWhiteSpace input.Command.Signal.SignalId))
        (fun input ->
            sprintf
                "SignalId: %s; ReliabilityPrior: %.2f"
                input.Command.Signal.SignalId
                input.Command.Signal.SourceReliabilityPrior)

let defaultRules: Rule<AssessmentRuleInput> list =
    [
        assessmentIdentity
        updateEligibility
        singleCurrentAssessment
        stateChangeImmutability
        historyImmutability
        changeDetectionGovernance
        changeMaterialityGovernance
        changeLifecycleDeterminism
        derivationSource
        signalNormalization
    ]
