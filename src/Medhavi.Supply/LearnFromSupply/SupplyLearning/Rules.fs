// Traceability: BR-S-130, BR-S-300 to BR-S-307, BR-S-310 to BR-S-316
module Medhavi.Supply.LearnFromSupply.SupplyLearning.Rules

open System
open Medhavi.Foundation.Contracts
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers
open Model
open Policies

/// Input context for validating RecordSupplyLearning command (AB-S-120)
type RecordLearningRuleInput =
    {
        LearningId: SupplyLearningId
        SubjectType: string
        SubjectId: string
        LearningType: SupplyLearningType
        RecurrenceCount: int
        RecurrenceFrequency: decimal
        PatternConfidence: ConfidenceLevel
        CrossCapabilitySourceCount: int
        ExistingLearning: SupplyLearning option
        Policy: LearningPolicy
    }

/// Input context for validating RecordLearningOutcome command (AB-S-121)
type RecordOutcomeRuleInput =
    {
        LearningId: SupplyLearningId
        ProposedOutcome: LearningOutcome
        Authority: string
        ExistingLearning: SupplyLearning option
        Policy: LearningPolicy
    }

/// BR-S-130 — Supply Learning Identity
let supplyLearningIdentity: Rule<RecordLearningRuleInput> =
    Rule.create
        Rules.supplyLearningIdentity.Id
        Rules.supplyLearningIdentity.Explanation
        (fun input -> not(String.IsNullOrWhiteSpace(SupplyLearningId.value input.LearningId)))
        (fun input ->
            if String.IsNullOrWhiteSpace(SupplyLearningId.value input.LearningId) then
                "Supply Learning Identifier is empty or whitespace"
            else
                $"Supply Learning '{SupplyLearningId.value input.LearningId}' has valid immutable identity")

/// BR-S-300 — Supply Learning Discovery Eligibility
let discoveryEligibility: Rule<RecordLearningRuleInput> =
    Rule.create
        Rules.supplyLearningDiscoveryEligibility.Id
        Rules.supplyLearningDiscoveryEligibility.Explanation
        (fun input -> input.RecurrenceCount >= input.Policy.MinRecurrencePeriods)
        (fun input ->
            $"Recurrence count: {input.RecurrenceCount}, Minimum required by policy: {input.Policy.MinRecurrencePeriods}")

/// BR-S-302 & BR-S-304 — Supply Learning Immutability on Recording
let learningCreationImmutability: Rule<RecordLearningRuleInput> =
    Rule.create
        Rules.supplyLearningImmutability.Id
        Rules.supplyLearningImmutability.Explanation
        (fun input ->
            match input.ExistingLearning with
            | None -> true
            | Some existing -> existing.LifecycleState = LearningLifecycleState.Superseded)
        (fun input ->
            match input.ExistingLearning with
            | Some existing when existing.LifecycleState <> LearningLifecycleState.Superseded ->
                $"Supply Learning '{SupplyLearningId.value input.LearningId}' already exists in active state '{existing.LifecycleState.AsString}' (immutable per BR-S-302)"
            | _ -> $"Supply Learning '{SupplyLearningId.value input.LearningId}' is newly established or superseding")

/// BR-S-306 — Candidate Learning Derivation Source (Multi-Source Corroboration)
let crossCapabilityCorroboration: Rule<RecordLearningRuleInput> =
    Rule.create
        Rules.candidateLearningDerivationSource.Id
        Rules.candidateLearningDerivationSource.Explanation
        (fun input -> input.CrossCapabilitySourceCount >= input.Policy.MinCrossCapabilitySources)
        (fun input ->
            $"Corroborating distinct capability sources: {input.CrossCapabilitySourceCount}, Required: {input.Policy.MinCrossCapabilitySources}")

/// BR-S-311 — Multi-Period Recurrence Frequency Gating
let recurrenceFrequencyGating: Rule<RecordLearningRuleInput> =
    Rule.create
        Rules.multiPeriodRecurrenceGating.Id
        Rules.multiPeriodRecurrenceGating.Explanation
        (fun input -> input.RecurrenceFrequency >= input.Policy.MinRecurrenceFrequency)
        (fun input ->
            $"Empirical recurrence ratio: {input.RecurrenceFrequency:P1}, Governed threshold: {input.Policy.MinRecurrenceFrequency:P1}")

/// Rule set for AB-S-120 Record Supply Learning
let recordLearningRules: Rule<RecordLearningRuleInput> list =
    [
        supplyLearningIdentity
        discoveryEligibility
        learningCreationImmutability
        crossCapabilityCorroboration
        recurrenceFrequencyGating
    ]

// =========================================================================
// Aggregate Invariant Rules for AB-S-121 Record Learning Outcome
// =========================================================================

/// BR-S-301 & BR-S-304 — Single Outcome Transition from Pending
let singleOutcomeTransition: Rule<RecordOutcomeRuleInput> =
    Rule.create
        Rules.singleLearningOutcomePerSupplyLearning.Id
        Rules.singleLearningOutcomePerSupplyLearning.Explanation
        (fun input ->
            match input.ExistingLearning with
            | Some existing ->
                existing.LifecycleState = LearningLifecycleState.PendingOutcome
                && existing.Outcome = LearningOutcome.Pending
            | None -> false)
        (fun input ->
            match input.ExistingLearning with
            | None -> $"Supply Learning '{SupplyLearningId.value input.LearningId}' not found"
            | Some existing when existing.LifecycleState <> LearningLifecycleState.PendingOutcome ->
                $"Supply Learning '{SupplyLearningId.value input.LearningId}' is already in state '{existing.LifecycleState.AsString}' with outcome '{existing.Outcome.AsString}' (terminal per BR-S-304)"
            | _ ->
                $"Supply Learning '{SupplyLearningId.value input.LearningId}' is in Pending Outcome state and ready for outcome assignment")

/// Outcome authority validation
let validOutcomeAuthority: Rule<RecordOutcomeRuleInput> =
    Rule.create
        Rules.supplyLearningEvaluationGovernance.Id
        Rules.supplyLearningEvaluationGovernance.Explanation
        (fun input -> not(String.IsNullOrWhiteSpace input.Authority))
        (fun input ->
            if String.IsNullOrWhiteSpace input.Authority then
                "Outcome authority is empty or missing"
            else
                $"Outcome authority '{input.Authority}' is declared")

/// Rule set for AB-S-121 Record Learning Outcome
let recordOutcomeRules: Rule<RecordOutcomeRuleInput> list = [ singleOutcomeTransition; validOutcomeAuthority ]
