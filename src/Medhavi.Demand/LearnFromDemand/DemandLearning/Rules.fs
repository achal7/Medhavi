/// Learn From Demand Business Rules
/// Traces to: BR-D-125, BR-D-411, BR-D-412, BR-D-413 (Specification Chapter 7)
module Medhavi.Demand.LearnFromDemand.DemandLearning.Rules

open Medhavi.SemanticModel
open Medhavi.Foundation.Contracts
open Medhavi.Demand
open Medhavi.Demand.ArsIdentifiers
open Model
open Policies

/// Input context for Demand Learning approval rules (DE-D-015)
type DemandLearningRuleInput =
    { LearningId: DemandLearningId
      Scope: PlanningScopeId
      LearningType: LearningType
      PatternConfidence: decimal
      SupportingEvidenceCount: int
      ExistingLearning: DemandLearning option
      Policy: LearningAnalysisPolicy }

/// BR-D-125 — Learning Immutability (cannot overwrite existing learning)
let learningImmutability: Rule<DemandLearningRuleInput> =
    Rule.create
        Rules.learningImmutability.Id
        Rules.learningImmutability.Explanation
        (fun input -> input.ExistingLearning.IsNone)
        (fun input ->
            if input.ExistingLearning.IsSome then
                sprintf
                    "Demand Learning '%s' already exists (immutable per BR-D-125)"
                    (DemandLearningId.value input.LearningId)
            else
                sprintf
                    "Demand Learning '%s' is newly established and immutable"
                    (DemandLearningId.value input.LearningId))

/// BR-D-411 — Minimum Recurrence for Learning Derivation
let minimumRecurrence: Rule<DemandLearningRuleInput> =
    Rule.create
        Rules.learningMinimumRecurrence.Id
        Rules.learningMinimumRecurrence.Explanation
        (fun input -> input.SupportingEvidenceCount >= input.Policy.MinRecurrencePeriods)
        (fun input ->
            sprintf
                "Supporting evidence count: %d, Minimum required: %d"
                input.SupportingEvidenceCount
                input.Policy.MinRecurrencePeriods)

/// BR-D-413 — Pattern Confidence Criteria for Learning Derivation
let patternConfidenceCriteria: Rule<DemandLearningRuleInput> =
    Rule.create
        Rules.learningPatternConfidenceCriteria.Id
        Rules.learningPatternConfidenceCriteria.Explanation
        (fun input -> input.PatternConfidence >= input.Policy.PatternConfidenceMedium)
        (fun input ->
            let confStr = input.PatternConfidence.ToString("N1") + "%"
            let minStr = input.Policy.PatternConfidenceMedium.ToString("N1") + "%"
            sprintf "Pattern confidence: %s, Governed threshold: %s" confStr minStr)

/// Rule set for DE-D-015 Demand Learning approval
let learningRules: Rule<DemandLearningRuleInput> list =
    [ learningImmutability; minimumRecurrence; patternConfidenceCriteria ]
