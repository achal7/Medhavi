// Traceability: DE-S-121 Evaluate Supply Learning
module Medhavi.Supply.LearnFromSupply.SupplyLearning.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers
open Model
open Policies
open Rules

/// DE-S-121 Decision Result Payload
type EvaluateLearningDecision =
    {
        SelectedOutcome: LearningOutcome
        LearningId: SupplyLearningId
        Rationale: string
        ReasonCode: string
    }

/// Decision Result for recording validation
type RecordLearningValidationDecision =
    {
        LearningId: SupplyLearningId
        Passed: bool
        Rationale: string
    }

/// Evaluates rule eligibility for recording a new Supply Learning (AB-S-120)
let evaluateRecordEligibility
    (rules: Rule<RecordLearningRuleInput> list)
    (input: RecordLearningRuleInput)
    : Result<DecisionOutcome<RecordLearningValidationDecision>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let passed = evaluations |> List.forall(fun e -> e.Passed)

        let rationale =
            if passed then
                $"Supply Learning '{SupplyLearningId.value input.LearningId}' ({input.LearningType.AsString}) satisfies all discovery recurrence and evidence rules per PO-S-140."
            else
                let failures =
                    evaluations
                    |> List.filter(fun e -> not e.Passed)
                    |> List.map(fun e -> e.Evidence |> String.concat "; ")
                    |> String.concat " | "

                $"Supply Learning '{SupplyLearningId.value input.LearningId}' recording rejected: {failures}"

        if not passed then
            return!
                Error(DomainError.rule(rationale, rule = ArsIdentifiers.Rules.supplyLearningDiscoveryEligibility.Id))
        else
            let outcome =
                {
                    LearningId = input.LearningId
                    Passed = true
                    Rationale = rationale
                }

            return
                {
                    Outcome = outcome
                    Evaluations = evaluations
                }
    }

/// Evaluates rule eligibility for recording a governance outcome (AB-S-121)
let evaluateOutcomeEligibility
    (rules: Rule<RecordOutcomeRuleInput> list)
    (input: RecordOutcomeRuleInput)
    : Result<DecisionOutcome<RecordLearningValidationDecision>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let passed = evaluations |> List.forall(fun e -> e.Passed)

        let rationale =
            if passed then
                $"Governance outcome '{input.ProposedOutcome.AsString}' for Supply Learning '{SupplyLearningId.value input.LearningId}' authorized by '{input.Authority}'."
            else
                let failures =
                    evaluations
                    |> List.filter(fun e -> not e.Passed)
                    |> List.map(fun e -> e.Evidence |> String.concat "; ")
                    |> String.concat " | "

                $"Outcome recording for '{SupplyLearningId.value input.LearningId}' rejected: {failures}"

        if not passed then
            return!
                Error(
                    DomainError.rule(rationale, rule = ArsIdentifiers.Rules.singleLearningOutcomePerSupplyLearning.Id)
                )
        else
            let outcome =
                {
                    LearningId = input.LearningId
                    Passed = true
                    Rationale = rationale
                }

            return
                {
                    Outcome = outcome
                    Evaluations = evaluations
                }
    }

/// Evaluates candidate learning for governance determination (DE-S-121)
let evaluateLearningAdoption (learning: SupplyLearning) (policy: LearningPolicy) : EvaluateLearningDecision =

    let distinctSources =
        learning.SupportingEvidence |> List.map(fun e -> e.CapabilityId) |> List.distinct |> List.length

    if
        learning.RecurrenceCount < policy.MinRecurrencePeriods
        || learning.RecurrenceFrequency < policy.MinRecurrenceFrequency
    then
        {
            SelectedOutcome = LearningOutcome.Rejected
            LearningId = learning.Id
            Rationale =
                $"Empirical recurrence ratio {learning.RecurrenceFrequency:P1} across {learning.RecurrenceCount} periods is below minimum threshold."
            ReasonCode = "CandidatePatternBelowRecurrenceThreshold"
        }

    elif distinctSources < policy.MinCrossCapabilitySources then
        {
            SelectedOutcome = LearningOutcome.Rejected
            LearningId = learning.Id
            Rationale =
                $"Evidence is corroborated by {distinctSources} capability source(s); minimum {policy.MinCrossCapabilitySources} required for enterprise perspective."
            ReasonCode = "SingleSourceInsightRejected"
        }

    elif learning.PatternConfidence = ConfidenceLevel.Low then
        {
            SelectedOutcome = LearningOutcome.Rejected
            LearningId = learning.Id
            Rationale = "Pattern confidence score is Low; evidence does not support genuine enterprise phenomenon."
            ReasonCode = "LowPatternConfidence"
        }

    else
        let allInterventionsActionable =
            learning.ImprovementOpportunities
            |> List.forall(fun opp -> opp.InterventionConfidence <> ConfidenceLevel.Low)

        if allInterventionsActionable && not learning.ImprovementOpportunities.IsEmpty then
            {
                SelectedOutcome = LearningOutcome.Accepted
                LearningId = learning.Id
                Rationale =
                    "Pattern is robust, cross-corroborated, and improvement interventions demonstrate high feasibility and historical analogue support."
                ReasonCode = "RecurringPatternValidatedAndActionable"
            }
        else
            {
                SelectedOutcome = LearningOutcome.Deferred
                LearningId = learning.Id
                Rationale =
                    "Pattern is genuine and statistically robust, but improvement interventions carry low analogue confidence or require further longitudinal monitoring."
                ReasonCode = "PatternRobustInterventionRequiresMonitoring"
            }
