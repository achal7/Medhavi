/// SE-S-110 — Supply Exception Assessment Decisions
/// Governed by: PO-S-120 Exception Detection Policy
module Medhavi.Supply.DetectSupplyExceptions.SupplyExceptionAssessment.Decisions

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Model
open Rules

/// Governed decision alternatives for supply exception lifecycle transitions
[<RequireQualifiedAccess>]
type ExceptionEvaluationOutcome =
    | Detected of Assessment: SupplyExceptionAssessment
    | Confirmed of Assessment: SupplyExceptionAssessment * EscalatedToCore: bool
    | EvidenceUpdated of Assessment: SupplyExceptionAssessment
    | Resolved of Assessment: SupplyExceptionAssessment * WasEscalatedToCore: bool
    | Archived of Assessment: SupplyExceptionAssessment

/// Evaluates whether a supply exception command conforms to governed rules and derives the updated assessment
let evaluateException
    (rules: Rule<EvaluateExceptionInput> list)
    (input: EvaluateExceptionInput)
    : Result<DecisionOutcome<ExceptionEvaluationOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        if not failed.IsEmpty then
            let failureMsg =
                failed
                |> List.map (fun e -> sprintf "[%s] %s" e.RuleId (e.Evidence |> String.concat "; "))
                |> String.concat " | "
            return! Error (DomainError.rule(failureMsg, rule = "PO-S-120"))
        else
            let! outcome =
                match input.Command with
                | SupplyExceptionCommand.DetectException d ->
                    let stateEvent: StateChangeEvent =
                        { EventId = Guid.NewGuid().ToString()
                          PreviousState = None
                          NewState = SupplyExceptionLifecycleState.Detected
                          TriggeringEvidence = d.Rationale
                          Rationale = d.Rationale
                          Timestamp = d.Timestamp }

                    let assessment: SupplyExceptionAssessment =
                        { Id = d.Id
                          GovernedPolicyReference = d.GovernedPolicyReference
                          ViolatedRuleId = d.ViolatedRuleId
                          ExceptionType = d.ExceptionType
                          ExceptionScope = d.ExceptionScope
                          PlanningScopeId = d.PlanningScopeId
                          LifecycleState = SupplyExceptionLifecycleState.Detected
                          CurrentSeverity = d.Severity
                          ExceptionHorizon = d.Horizon
                          DetectionTimestamp = d.Timestamp
                          ResolutionTimestamp = None
                          EvidenceReferences = d.Evidence
                          EvaluationConfidence = d.Confidence
                          ReasonCodes = d.ReasonCodes
                          CorrelatedExceptionIds = d.CorrelatedExceptionIds
                          StateChangeHistory = [ stateEvent ]
                          CreatedTimestamp = d.Timestamp
                          UpdatedTimestamp = d.Timestamp }
                    Ok (ExceptionEvaluationOutcome.Detected assessment)

                | SupplyExceptionCommand.CorroborateException d ->
                    match input.CurrentState with
                    | None -> Error (DomainError.notFound ("SupplyExceptionAssessment", "Current state not found"))
                    | Some current ->
                        let stateEvent: StateChangeEvent =
                            { EventId = Guid.NewGuid().ToString()
                              PreviousState = Some current.LifecycleState
                              NewState = SupplyExceptionLifecycleState.Confirmed
                              TriggeringEvidence = d.Rationale
                              Rationale = d.Rationale
                              Timestamp = d.Timestamp }

                        let escalate = shouldEscalateToCore input.Policy d.Severity

                        let assessment: SupplyExceptionAssessment =
                            { current with
                                LifecycleState = SupplyExceptionLifecycleState.Confirmed
                                CurrentSeverity = d.Severity
                                ExceptionHorizon = d.Horizon
                                EvaluationConfidence = d.Confidence
                                ReasonCodes = (current.ReasonCodes @ d.ReasonCodes) |> List.distinct
                                EvidenceReferences = (current.EvidenceReferences @ d.CorroboratingEvidence) |> List.distinct
                                StateChangeHistory = current.StateChangeHistory @ [ stateEvent ]
                                UpdatedTimestamp = d.Timestamp }
                        Ok (ExceptionEvaluationOutcome.Confirmed(assessment, escalate))

                | SupplyExceptionCommand.UpdateExceptionEvidence d ->
                    match input.CurrentState with
                    | None -> Error (DomainError.notFound ("SupplyExceptionAssessment", "Current state not found"))
                    | Some current ->
                        let stateEvent: StateChangeEvent =
                            { EventId = Guid.NewGuid().ToString()
                              PreviousState = Some current.LifecycleState
                              NewState = current.LifecycleState
                              TriggeringEvidence = d.Rationale
                              Rationale = d.Rationale
                              Timestamp = d.Timestamp }

                        let assessment: SupplyExceptionAssessment =
                            { current with
                                CurrentSeverity = d.Severity
                                ExceptionHorizon = d.Horizon
                                EvaluationConfidence = d.Confidence
                                ReasonCodes = (current.ReasonCodes @ d.ReasonCodes) |> List.distinct
                                EvidenceReferences = (current.EvidenceReferences @ d.UpdatedEvidence) |> List.distinct
                                StateChangeHistory = current.StateChangeHistory @ [ stateEvent ]
                                UpdatedTimestamp = d.Timestamp }
                        Ok (ExceptionEvaluationOutcome.EvidenceUpdated assessment)

                | SupplyExceptionCommand.ResolveException d ->
                    match input.CurrentState with
                    | None -> Error (DomainError.notFound ("SupplyExceptionAssessment", "Current state not found"))
                    | Some current ->
                        let stateEvent: StateChangeEvent =
                            { EventId = Guid.NewGuid().ToString()
                              PreviousState = Some current.LifecycleState
                              NewState = SupplyExceptionLifecycleState.Resolved
                              TriggeringEvidence = d.Rationale
                              Rationale = d.Rationale
                              Timestamp = d.Timestamp }

                        let wasEscalated =
                            current.LifecycleState = SupplyExceptionLifecycleState.Confirmed
                            && shouldEscalateToCore input.Policy current.CurrentSeverity

                        let assessment: SupplyExceptionAssessment =
                            { current with
                                LifecycleState = SupplyExceptionLifecycleState.Resolved
                                ResolutionTimestamp = Some d.Timestamp
                                EvidenceReferences = (current.EvidenceReferences @ d.ResolutionEvidence) |> List.distinct
                                StateChangeHistory = current.StateChangeHistory @ [ stateEvent ]
                                UpdatedTimestamp = d.Timestamp }
                        Ok (ExceptionEvaluationOutcome.Resolved(assessment, wasEscalated))

                | SupplyExceptionCommand.ArchiveException d ->
                    match input.CurrentState with
                    | None -> Error (DomainError.notFound ("SupplyExceptionAssessment", "Current state not found"))
                    | Some current ->
                        let stateEvent: StateChangeEvent =
                            { EventId = Guid.NewGuid().ToString()
                              PreviousState = Some current.LifecycleState
                              NewState = SupplyExceptionLifecycleState.Archived
                              TriggeringEvidence = d.Rationale
                              Rationale = d.Rationale
                              Timestamp = d.Timestamp }

                        let assessment: SupplyExceptionAssessment =
                            { current with
                                LifecycleState = SupplyExceptionLifecycleState.Archived
                                StateChangeHistory = current.StateChangeHistory @ [ stateEvent ]
                                UpdatedTimestamp = d.Timestamp }
                        Ok (ExceptionEvaluationOutcome.Archived assessment)

            return { Outcome = outcome; Evaluations = evaluations }
    }
