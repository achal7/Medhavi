/// CA-P-007 Assess Promise Risk and Resilience — Model
/// Traces to: SE-P-060, CR-P-023, CR-P-024
module Medhavi.Promise.AssessPromiseRisk.PromiseRiskAssessment.Model

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers

type RiskAssessmentState =
    | Evaluated
    | MonitoredActive
    | EscalatedToException of EscalationTime: Timestamp * TargetExceptionId: PromiseExceptionId
    | NormalizedResolved of ResolutionTime: Timestamp
    | Closed of CloseTime: Timestamp

type RiskCausalFactorRecord =
    { FactorRecordId: string
      FactorType: string
      ContributionWeightPct: decimal
      EvidenceDescription: string
      DetectedTimestamp: Timestamp }

type RiskTelemetryObservation =
    { ObservationId: string
      SourceSystem: string
      SignalType: string
      RecordedValue: string
      IngestedTimestamp: Timestamp }

/// SE-P-060 Promise Risk Assessment Aggregate Root
type PromiseRiskAssessment =
    { AssessmentId: PromiseRiskAssessmentId
      CommitmentId: OrderCommitmentId
      OrderLineId: OrderLineId
      Item: ItemId
      AssessmentTimestamp: Timestamp
      AssessmentStatus: RiskAssessmentState
      SeverityTier: PromiseRiskTier
      BreachProbabilityPct: decimal
      ProjectedLateDays: decimal
      BufferConsumptionRatio: decimal
      RiskDrivers: string list
      CausalFactors: RiskCausalFactorRecord list
      TelemetryHistory: RiskTelemetryObservation list
      AutoEscalationTriggered: bool
      TargetExceptionId: PromiseExceptionId option
      DecisionTraceability: string option }

type AssessCommitmentRiskCmd =
    { AssessmentId: PromiseRiskAssessmentId
      CommitmentId: OrderCommitmentId
      OrderLineId: OrderLineId
      Item: ItemId
      AssessmentTime: Timestamp }

type EscalateRiskBreachCmd =
    { AssessmentId: PromiseRiskAssessmentId
      EscalationReason: string
      EscalationTime: Timestamp
      TargetExceptionId: PromiseExceptionId }

type NormalizeRiskCmd =
    { AssessmentId: PromiseRiskAssessmentId
      NormalizationTime: Timestamp }

type PromiseRiskCmd =
    | Assess of AssessCommitmentRiskCmd
    | Escalate of EscalateRiskBreachCmd
    | Normalize of NormalizeRiskCmd

    static member GetId(cmd: PromiseRiskCmd) =
        match cmd with
        | Assess c -> c.AssessmentId
        | Escalate c -> c.AssessmentId
        | Normalize c -> c.AssessmentId

type PromiseRiskEvent =
    | CommitmentRiskAssessed of Assessment: PromiseRiskAssessment
    | CommitmentRiskEscalated of AssessmentId: PromiseRiskAssessmentId * CommitmentId: OrderCommitmentId * ExceptionId: PromiseExceptionId * EscalationTime: Timestamp * TraceId: string
    | RiskFlagCleared of AssessmentId: PromiseRiskAssessmentId * CommitmentId: OrderCommitmentId * NormalizedTime: Timestamp * TraceId: string

let evolve: Evolve<PromiseRiskAssessment, PromiseRiskEvent> =
    fun (state: PromiseRiskAssessment option) (event: PromiseRiskEvent) ->
        match event with
        | CommitmentRiskAssessed assessment ->
            Some assessment

        | CommitmentRiskEscalated(_, _, exceptionId, escTime, traceId) ->
            state
            |> Option.map (fun a ->
                { a with
                    AssessmentStatus = EscalatedToException(escTime, exceptionId)
                    AutoEscalationTriggered = true
                    TargetExceptionId = Some exceptionId
                    DecisionTraceability = Some traceId })

        | RiskFlagCleared(_, _, normTime, traceId) ->
            state
            |> Option.map (fun a ->
                { a with
                    AssessmentStatus = NormalizedResolved normTime
                    SeverityTier = PromiseRiskTier.Low
                    BreachProbabilityPct = 0.0m
                    ProjectedLateDays = 0.0m
                    DecisionTraceability = Some traceId })

