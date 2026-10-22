/// SE-S-100 — Supply Quality Decisions
/// Traces to: DE-S-091 (Evaluate Supply Performance), DE-S-092 (Publish Supply Quality Assessment)
module Medhavi.Supply.EvaluateSupplyQuality.SupplyQualityAssessment.Decisions

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Model
open Policies
open Rules

// =============================================================================
// DE-S-091 — Evaluate Supply Performance
// =========================================================================

module EvaluateSupplyPerformance =

    type DecisionContext =
        { ForceMajeureDeclared: bool
          StrategicContextNote: string option }

    type DimensionalDetermination =
        { Dimension: QualityDimensionType
          AuthoritativeClassification: QualityClassification
          AuthoritativeTrend: TrendTrajectory
          DataQualityFlag: bool
          Rationale: string }

    let decide
        (evaluation: DimensionalEvaluation)
        (context: DecisionContext)
        (policy: SupplyQualityPolicy) : DimensionalDetermination =

        let baseClass = evaluation.RecommendedClassification
        let isLowConfidence = evaluation.Confidence = QualityConfidenceLevel.Low

        let finalClass, contextNote =
            if context.ForceMajeureDeclared && baseClass <> QualityClassification.Unassessed && baseClass <> QualityClassification.Critical then
                match baseClass with
                | QualityClassification.Critical -> QualityClassification.Poor, " Mitigated 1 level due to Force Majeure declaration."
                | QualityClassification.Poor -> QualityClassification.Adequate, " Mitigated 1 level due to Force Majeure declaration."
                | other -> other, ""
            else
                baseClass, ""

        let rationale =
            $"Dimension '{evaluation.DimensionName}' classified as {finalClass} (trend: {evaluation.Trend}, confidence: {evaluation.Confidence}).{contextNote}"

        { Dimension = evaluation.Dimension
          AuthoritativeClassification = finalClass
          AuthoritativeTrend = evaluation.Trend
          DataQualityFlag = isLowConfidence
          Rationale = rationale }

// =============================================================================
// DE-S-092 — Publish Supply Quality Assessment
// =========================================================================

module PublishSupplyQualityAssessment =

    let evaluatePublication
        (rules: Rule<PublicationRuleInput> list)
        (input: PublicationRuleInput)
        : Result<DecisionOutcome<PublicationDecisionOutcome>, DomainError> =
        result {
            let! evaluations = Rule.evaluateAll rules input
            let mandatoryRulePassed =
                evaluations
                |> List.tryFind (fun e -> e.RuleId = "BR-S-266-MANDATORY")
                |> Option.map (fun e -> e.Passed)
                |> Option.defaultValue false

            let confidenceRulePassed =
                evaluations
                |> List.tryFind (fun e -> e.RuleId = "BR-S-266-CONFIDENCE")
                |> Option.map (fun e -> e.Passed)
                |> Option.defaultValue false

            let outcome =
                if not mandatoryRulePassed then
                    PublicationDecisionOutcome.Hold
                elif not confidenceRulePassed then
                    if input.Policy.AllowPublicationWithFlags then
                        PublicationDecisionOutcome.PublishWithFlags
                    else
                        PublicationDecisionOutcome.Hold
                else
                    let hasPoorOrCritical =
                        input.DimensionalEvaluations
                        |> List.exists (fun d -> d.IsMandatory && (d.AuthoritativeClassification = QualityClassification.Poor || d.AuthoritativeClassification = QualityClassification.Critical))

                    if hasPoorOrCritical then
                        PublicationDecisionOutcome.PublishWithFlags
                    else
                        PublicationDecisionOutcome.Publish

            return { Outcome = outcome; Evaluations = evaluations }
        }
