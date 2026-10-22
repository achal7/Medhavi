/// SE-S-110 — Supply Exception Assessment Aggregate Behaviors
/// Traces to: AB-S-100 (Evaluate Supply Exception)
module Medhavi.Supply.DetectSupplyExceptions.SupplyExceptionAssessment.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Foundation.Failure
open Medhavi.Supply
open Model
open Policies
open Rules
open Decisions

/// AB-S-100: Evaluate Supply Exception
let decide
    (policy: ExceptionDetectionPolicy)
    : Decide<SupplyExceptionAssessment, SupplyExceptionCommand, SupplyExceptionEvent> =
    fun (cmd: SupplyExceptionCommand) (stateOpt: SupplyExceptionAssessment option) ->
        result {
            let input: EvaluateExceptionInput =
                { Command = cmd
                  CurrentState = stateOpt
                  Policy = policy
                  PrerequisitesSatisfied = true
                  ExistingActiveExceptions = [] }

            let! decisionOutcome = evaluateException evaluationRules input

            let events =
                match decisionOutcome.Outcome with
                | ExceptionEvaluationOutcome.Detected assessment ->
                    [ SupplyExceptionEvent.SupplyExceptionDetected assessment ]
                | ExceptionEvaluationOutcome.Confirmed(assessment, escalated) ->
                    [ SupplyExceptionEvent.SupplyExceptionConfirmed(assessment, escalated) ]
                | ExceptionEvaluationOutcome.EvidenceUpdated assessment ->
                    [ SupplyExceptionEvent.SupplyExceptionEvidenceUpdated assessment ]
                | ExceptionEvaluationOutcome.Resolved(assessment, wasEscalated) ->
                    [ SupplyExceptionEvent.SupplyExceptionResolved(assessment, wasEscalated) ]
                | ExceptionEvaluationOutcome.Archived assessment ->
                    [ SupplyExceptionEvent.SupplyExceptionArchived assessment ]

            let traceId = Guid.NewGuid().ToString()

            let summaryText =
                match decisionOutcome.Outcome with
                | ExceptionEvaluationOutcome.Detected s -> sprintf "Supply Exception detected for %s" s.GovernedPolicyReference
                | ExceptionEvaluationOutcome.Confirmed(s, esc) -> sprintf "Supply Exception confirmed (EscalatedToCore: %b)" esc
                | ExceptionEvaluationOutcome.EvidenceUpdated _ -> "Supply Exception evidence updated"
                | ExceptionEvaluationOutcome.Resolved(s, wasEsc) -> sprintf "Supply Exception resolved (WasEscalatedToCore: %b)" wasEsc
                | ExceptionEvaluationOutcome.Archived _ -> "Supply Exception archived"

            return
                buildDecisionWithTrace
                    evolve
                    stateOpt
                    events
                    traceId
                    []
                    ArsIdentifiers.Capabilities.detectSupplyExceptions.Id
                    decisionOutcome
                    (Some ArsIdentifiers.Policies.exceptionDetectionPolicy.Id)
                    (Some 1)
                    [ ArsIdentifiers.SemanticObjects.supplyExceptionAssessment.Id ]
                    (Some summaryText)
        }
