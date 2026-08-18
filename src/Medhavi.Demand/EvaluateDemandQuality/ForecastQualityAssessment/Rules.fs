/// BR-D-212 & BR-D-213 — Forecast Quality Assessment Business Rules
module Medhavi.Demand.EvaluateDemandQuality.ForecastQualityAssessment.Rules

open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Demand
open Medhavi.Demand.ArsIdentifiers
open Policies
open Model

/// Typed input context for evaluating forecast quality
type EvaluationRuleInput =
    { Scope: PlanningScopeId
      EvaluationPeriodStart: Timestamp
      EvaluationPeriodEnd: Timestamp
      CompletenessScore: decimal
      Policy: ForecastMeasurementPolicy }

/// Typed input context for publishing a forecast quality assessment version
type PublicationRuleInput =
    { Assessment: ForecastQualityAssessment
      TargetVersionNumber: int
      Policy: ForecastMeasurementPolicy }

/// BR-D-212: Completeness requirement for publication
let completenessRule: Rule<PublicationRuleInput> =
    Rule.create
        ArsIdentifiers.Rules.qualityAssessmentCompletenessRequirement.Id
        ArsIdentifiers.Rules.qualityAssessmentCompletenessRequirement.Explanation
        (fun input ->
            match input.Assessment.Versions |> List.tryFind(fun v -> v.VersionNumber = input.TargetVersionNumber) with
            | Some v -> v.Metrics.CompletenessScore >= input.Policy.MinCompletenessThreshold
            | None -> false)
        (fun input ->
            match input.Assessment.Versions |> List.tryFind(fun v -> v.VersionNumber = input.TargetVersionNumber) with
            | Some v ->
                if v.Metrics.CompletenessScore >= input.Policy.MinCompletenessThreshold then
                    $"Data completeness {v.Metrics.CompletenessScore:F1}%% meets or exceeds governed threshold {input.Policy.MinCompletenessThreshold:F1}%%"
                else
                    $"Data completeness {v.Metrics.CompletenessScore:F1}%% is below minimum publication threshold {input.Policy.MinCompletenessThreshold:F1}%%"
            | None -> $"Target version v{input.TargetVersionNumber} does not exist")

/// BR-D-213: Evaluation period duration minimum length
let evaluationPeriodDurationRule: Rule<PublicationRuleInput> =
    Rule.create
        ArsIdentifiers.Rules.qualityAssessmentEvaluationPeriodMinimum.Id
        ArsIdentifiers.Rules.qualityAssessmentEvaluationPeriodMinimum.Explanation
        (fun input ->
            let durationDays =
                (Timestamp.value input.Assessment.EvaluationPeriodEnd
                 - Timestamp.value input.Assessment.EvaluationPeriodStart)
                    .TotalDays

            decimal durationDays >= input.Policy.MinEvaluationPeriodDays)
        (fun input ->
            let durationDays =
                (Timestamp.value input.Assessment.EvaluationPeriodEnd
                 - Timestamp.value input.Assessment.EvaluationPeriodStart)
                    .TotalDays

            if decimal durationDays >= input.Policy.MinEvaluationPeriodDays then
                $"Evaluation period duration of {durationDays:F1} days satisfies minimum requirement of {input.Policy.MinEvaluationPeriodDays:F1} days"
            else
                $"Evaluation period duration of {durationDays:F1} days is shorter than required minimum of {input.Policy.MinEvaluationPeriodDays:F1} days")

/// Publication governance rules pipeline
let publicationRules: Rule<PublicationRuleInput> list = [ completenessRule; evaluationPeriodDurationRule ]
