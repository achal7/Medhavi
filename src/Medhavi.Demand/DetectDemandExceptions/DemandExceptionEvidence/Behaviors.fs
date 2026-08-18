/// AB-D-015 — Detect & Resolve Demand Exception Evidence Behaviors
module Medhavi.Demand.DetectDemandExceptions.DemandExceptionEvidence.Behaviors

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.SemanticModel
open Medhavi.Demand
open Medhavi.Demand.ArsIdentifiers
open Policies
open Model
open Rules

let private buildDecisionTrace policyId policyVersion decisionId decisionOutcome state events summary =
    buildDecisionWithTrace
        evolve
        state
        events
        decisionId
        []
        ArsIdentifiers.Capabilities.detectDemandExceptions.Id
        decisionOutcome
        policyId
        policyVersion
        [ "SE-D-009" ]
        (Some summary)

/// AB-D-015: Evaluate Demand Exception Evidence Decider (DE-D-012)
let evaluateDemandException
    (policy: DemandExceptionEvidencePolicy)
    : Decide<DemandExceptionEvidenceAggregate, EvaluateDemandExceptionCmd, DemandExceptionEvent> =
    fun (cmd: EvaluateDemandExceptionCmd) (state: DemandExceptionEvidenceAggregate option) ->
        result {
            let isCurrentlyActive = state |> Option.map(fun a -> a.IsCurrentlyActive) |> Option.defaultValue false

            let input: ExceptionRuleInput =
                { ExceptionType = cmd.ExceptionType
                  MetricValue = cmd.MetricValue
                  HistoricalValues = cmd.HistoricalValues
                  IsCurrentlyActive = isCurrentlyActive
                  Policy = policy }

            let! decision = Decisions.evaluateEvidence Rules.exceptionRules input

            match decision.Outcome with
            | DetectionEvidenceExists(severity, metric, value, threshold, rationale) ->
                let record: DemandExceptionEvidenceRecord =
                    { EvidenceId = cmd.EvidenceId
                      ExceptionType = cmd.ExceptionType
                      PlanningEntityType = cmd.PlanningEntityType
                      PlanningEntityId = cmd.PlanningEntityId
                      Scope = cmd.Scope
                      Severity = Some severity
                      TriggeringMetric = metric
                      MetricValue = value
                      ThresholdValue = threshold
                      Rationale = rationale
                      IsResolution = false
                      Timestamp = cmd.EvaluationTime }

                let updatedAggregate: DemandExceptionEvidenceAggregate =
                    { Id = cmd.EvidenceId
                      ExceptionType = cmd.ExceptionType
                      PlanningEntityType = cmd.PlanningEntityType
                      PlanningEntityId = cmd.PlanningEntityId
                      Scope = cmd.Scope
                      ActiveSeverity = Some severity
                      LastTriggeringMetric = metric
                      LastMetricValue = value
                      LastThresholdValue = threshold
                      LastRationale = rationale
                      IsCurrentlyActive = true
                      History =
                        match state with
                        | Some ex -> record :: ex.History
                        | None -> [ record ]
                      LastUpdated = cmd.EvaluationTime }

                let events = [ DemandExceptionDetected(updatedAggregate, record) ]

                let summary =
                    sprintf
                        "Demand exception DETECTED: %s for %s '%s' (Scope: %s) with %s severity. %s"
                        cmd.ExceptionType.AsString
                        cmd.PlanningEntityType
                        cmd.PlanningEntityId
                        (PlanningScopeId.value cmd.Scope)
                        severity.AsString
                        rationale

                return
                    buildDecisionTrace
                        (Some policy.PolicyId)
                        (Some policy.Version)
                        ArsIdentifiers.Decisions.evaluateDemandExceptionEvidence.Id
                        decision
                        state
                        events
                        summary

            | ResolutionEvidenceExists(metric, value, threshold, rationale) ->
                let record: DemandExceptionEvidenceRecord =
                    { EvidenceId = cmd.EvidenceId
                      ExceptionType = cmd.ExceptionType
                      PlanningEntityType = cmd.PlanningEntityType
                      PlanningEntityId = cmd.PlanningEntityId
                      Scope = cmd.Scope
                      Severity = None
                      TriggeringMetric = metric
                      MetricValue = value
                      ThresholdValue = threshold
                      Rationale = rationale
                      IsResolution = true
                      Timestamp = cmd.EvaluationTime }

                let updatedAggregate: DemandExceptionEvidenceAggregate =
                    { Id = cmd.EvidenceId
                      ExceptionType = cmd.ExceptionType
                      PlanningEntityType = cmd.PlanningEntityType
                      PlanningEntityId = cmd.PlanningEntityId
                      Scope = cmd.Scope
                      ActiveSeverity = None
                      LastTriggeringMetric = metric
                      LastMetricValue = value
                      LastThresholdValue = threshold
                      LastRationale = rationale
                      IsCurrentlyActive = false
                      History =
                        match state with
                        | Some ex -> record :: ex.History
                        | None -> [ record ]
                      LastUpdated = cmd.EvaluationTime }

                let events = [ DemandExceptionResolved(updatedAggregate, record) ]

                let summary =
                    sprintf
                        "Demand exception RESOLVED: %s for %s '%s' (Scope: %s). %s"
                        cmd.ExceptionType.AsString
                        cmd.PlanningEntityType
                        cmd.PlanningEntityId
                        (PlanningScopeId.value cmd.Scope)
                        rationale

                return
                    buildDecisionTrace
                        (Some policy.PolicyId)
                        (Some policy.Version)
                        ArsIdentifiers.Decisions.evaluateDemandExceptionEvidence.Id
                        decision
                        state
                        events
                        summary

            | NoEvidence ->
                let currentAggregate =
                    match state with
                    | Some ex -> ex
                    | None ->
                        { Id = cmd.EvidenceId
                          ExceptionType = cmd.ExceptionType
                          PlanningEntityType = cmd.PlanningEntityType
                          PlanningEntityId = cmd.PlanningEntityId
                          Scope = cmd.Scope
                          ActiveSeverity = None
                          LastTriggeringMetric = cmd.TriggeringMetric
                          LastMetricValue = cmd.MetricValue
                          LastThresholdValue = 0.0m
                          LastRationale = "No exception evidence"
                          IsCurrentlyActive = false
                          History = []
                          LastUpdated = cmd.EvaluationTime }

                let events = [ DemandExceptionNoEvidence currentAggregate ]

                let summary =
                    sprintf
                        "Demand exception evaluation: No evidence for %s on %s '%s'"
                        cmd.ExceptionType.AsString
                        cmd.PlanningEntityType
                        cmd.PlanningEntityId

                return
                    buildDecisionTrace
                        (Some policy.PolicyId)
                        (Some policy.Version)
                        ArsIdentifiers.Decisions.evaluateDemandExceptionEvidence.Id
                        decision
                        state
                        events
                        summary
        }
