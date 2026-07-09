module Medhavi.Demand.ForecastQualityAssessment.Decisions

open Medhavi.Common
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Contracts.DecisionTrace
open Medhavi.SharedKernel.Failure
open Medhavi.Demand
open Medhavi.Demand.ForecastQualityAssessment.Model
open Medhavi.Demand.ForecastQualityAssessment.Rules

/// DE‑D‑035 — Determine if assessment can be published.
let evaluate (cmd: EvaluateForecastQualityCmd) : Result<ForecastQualityAssessmentEvent list, DomainError> =
    let assessmentIdRes =
        ForecastQualityAssessmentId.create
            $"{PlanningScopeId.value cmd.PlanningScopeId}-{Timestamp.value cmd.EvaluationPeriodStart:yyyyMMdd}-{Timestamp.value cmd.EvaluationPeriodEnd:yyyyMMdd}"

    match assessmentIdRes with
    | Error e -> Error(DomainError.validation "Assessment ID required")
    | Ok assessmentId ->
        result {
            let! _ = dataCompleteness cmd.ActualDataCount cmd.ExpectedDataCount cmd.CompletenessThreshold
            let! _ = minimumEvaluationPeriod cmd.EvaluationPeriodStart cmd.EvaluationPeriodEnd cmd.MinEvaluationPeriodDays

            // Build the assessment

            let published =
                { Id = assessmentId
                  PlanningScopeId = cmd.PlanningScopeId
                  EvaluationPeriodStart = cmd.EvaluationPeriodStart
                  EvaluationPeriodEnd = cmd.EvaluationPeriodEnd
                  Status = Published
                  Version = 1
                  CoreMetrics = cmd.CoreMetrics
                  OptionalMetrics = cmd.OptionalMetrics
                  OverallQualityScore = cmd.OverallQualityScore
                  SourceForecastPublicationRefs = cmd.SourceForecastPublicationRefs
                  SourceDemandHistoryRefs = cmd.SourceDemandHistoryRefs
                  ForecastMeasurementPolicyVersionRef = cmd.ForecastMeasurementPolicyVersionRef
                  TransactionTime = cmd.PublicationTime
                  PublicationTime = Some cmd.PublicationTime
                  SupersededAssessmentId = None }

            return [ ForecastQualityAssessed published ]
        }
        |> function
            | Ok events -> Ok events
            | Error err ->
                // If rules fail, create a Draft assessment
                let draft =
                    { Id = assessmentId
                      PlanningScopeId = cmd.PlanningScopeId
                      EvaluationPeriodStart = cmd.EvaluationPeriodStart
                      EvaluationPeriodEnd = cmd.EvaluationPeriodEnd
                      Status = Draft
                      Version = 1
                      CoreMetrics = cmd.CoreMetrics
                      OptionalMetrics = cmd.OptionalMetrics
                      OverallQualityScore = cmd.OverallQualityScore
                      SourceForecastPublicationRefs = cmd.SourceForecastPublicationRefs
                      SourceDemandHistoryRefs = cmd.SourceDemandHistoryRefs
                      ForecastMeasurementPolicyVersionRef = cmd.ForecastMeasurementPolicyVersionRef
                      TransactionTime = cmd.PublicationTime
                      PublicationTime = None
                      SupersededAssessmentId = None }

                Ok [ ForecastQualityAssessed draft ]

let decide
    (cmd: ForecastQualityAssessmentCommand)
    (stateOpt: ForecastQualityAssessment option)
    : Result<Decision<ForecastQualityAssessment, ForecastQualityAssessmentEvent>, DomainError> =
    match cmd, stateOpt with
    | Evaluate cmd, None ->
        evaluate cmd
        |> Result.map(fun events ->
            let derivedState = events |> List.fold (fun acc e -> evolve e acc) stateOpt
            let rationale =
                match derivedState with
                | Some s when s.Status = Published ->
                    { Summary = $"Forecast quality successfully evaluated and published for scope {PlanningScopeId.value s.PlanningScopeId}."
                      Evidence = [ $"WAPE: {s.CoreMetrics.WAPE:P2}"; $"Accuracy: {s.CoreMetrics.ForecastAccuracy:P2}" ]
                      Alternatives = [] }
                | Some s when s.Status = Draft ->
                    { Summary = $"Forecast quality saved as Draft because validation checks did not meet publication threshold."
                      Evidence = [ $"WAPE: {s.CoreMetrics.WAPE:P2}"; $"Accuracy: {s.CoreMetrics.ForecastAccuracy:P2}" ]
                      Alternatives = [] }
                | _ ->
                    { Summary = "Forecast quality evaluation completed"
                      Evidence = []
                      Alternatives = [] }

            let trace =
                { DecisionId = ArsIdentifiers.Demand.Decisions.publishForecastQualityAssessment
                  CapabilityId = ArsIdentifiers.Demand.Capabilities.evaluateDemandQuality
                  RulesEvaluated =
                    [ (ArsIdentifiers.Demand.Rules.qualityAssessmentCompleteness, 1)
                      (ArsIdentifiers.Demand.Rules.minEvaluationPeriod, 1) ]
                  PolicyId = Some ArsIdentifiers.Demand.Policies.qualityAssessmentPublication
                  PolicyVersion = Some 1
                  SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.forecastQualityAssessment ]
                  Rationale = rationale }

            buildDecision evolve stateOpt events (Some trace))
    | _ -> Error(DomainError.validation "Command invalid for current state")
