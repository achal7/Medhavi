/// CA-P-007 Assess Promise Risk and Resilience — Behaviors
/// Traces to: AB-P-019 Assess Commitment Risk, AB-P-020 Escalate Risk Breach
module Medhavi.Promise.AssessPromiseRisk.PromiseRiskAssessment.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Model
open Rules
open Decisions
open Policies

let buildTrace policy decisionId decisionOutcome state events summary =
    let policyId = policy |> Option.map (fun (p: PromiseRiskAssessmentPolicy) -> p.PolicyId)
    let policyVersion = policy |> Option.map (fun (p: PromiseRiskAssessmentPolicy) -> p.Version)

    buildDecisionWithTrace
        evolve
        state
        events
        decisionId
        []
        Capabilities.assessPromiseRiskAndResilience.Id
        decisionOutcome
        policyId
        policyVersion
        [ SemanticObjects.promiseRiskAssessment.Id ]
        (Some summary)

/// AB-P-019: Assess Commitment Risk
let assess
    (policy: PromiseRiskAssessmentPolicy)
    (commitmentExists: bool)
    (itemExists: bool)
    (committedDeliveryDate: Timestamp)
    (disruptionSignals: SupplyDisruptionSignalFact list)
    : Decide<PromiseRiskAssessment, AssessCommitmentRiskCmd, PromiseRiskEvent> =
    fun (cmd: AssessCommitmentRiskCmd) (state: PromiseRiskAssessment option) ->
        result {
            let input: AssessRiskInput =
                { Cmd = cmd
                  CommitmentExists = commitmentExists
                  ItemExists = itemExists
                  Policy = policy }

            let! decision = evaluateAssessRisk assessRules input
            let traceId = Guid.NewGuid().ToString()

            match decision.Outcome with
            | RiskAssessmentAuthorized ->
                let prob, lateDays, bufferRatio, severity, drivers, causalFactors =
                    Algorithms.evaluateRiskMetrics
                        policy
                        committedDeliveryDate
                        cmd.AssessmentTime
                        disruptionSignals

                let autoEscalate = prob >= policy.BreachProbabilityEscalationThresholdPct
                let epochSeconds = (Timestamp.value cmd.AssessmentTime).ToUnixTimeSeconds()

                let status =
                    if autoEscalate then
                        let excId = PromiseExceptionId.ofCommitment cmd.CommitmentId epochSeconds
                        EscalatedToException(cmd.AssessmentTime, excId)
                    else
                        MonitoredActive

                let assessment: PromiseRiskAssessment =
                    { AssessmentId = cmd.AssessmentId
                      CommitmentId = cmd.CommitmentId
                      OrderLineId = cmd.OrderLineId
                      Item = cmd.Item
                      AssessmentTimestamp = cmd.AssessmentTime
                      AssessmentStatus = status
                      SeverityTier = severity
                      BreachProbabilityPct = prob
                      ProjectedLateDays = lateDays
                      BufferConsumptionRatio = bufferRatio
                      RiskDrivers = drivers
                      CausalFactors = causalFactors
                      TelemetryHistory = []
                      AutoEscalationTriggered = autoEscalate
                      TargetExceptionId =
                        if autoEscalate then
                            Some (PromiseExceptionId.ofCommitment cmd.CommitmentId epochSeconds)
                        else None
                      DecisionTraceability = Some traceId }

                let events = [ CommitmentRiskAssessed assessment ]
                return buildTrace (Some policy) traceId decision state events $"Commitment risk assessed with {prob}%% breach probability"

            | AssessmentRejected reason ->
                return! Error(DomainError.rule (reason, rule = Rules.promiseRiskAssessmentIdentity.Id))
        }

/// AB-P-020: Escalate Risk Breach
let escalate
    (policy: PromiseRiskAssessmentPolicy)
    : Decide<PromiseRiskAssessment, EscalateRiskBreachCmd, PromiseRiskEvent> =
    fun (cmd: EscalateRiskBreachCmd) (state: PromiseRiskAssessment option) ->
        result {
            match state with
            | None ->
                return! Error(DomainError.notFound (SemanticObjects.promiseRiskAssessment.Id, PromiseRiskAssessmentId.value cmd.AssessmentId))
            | Some assessment ->
                let input: EscalateRiskInput =
                    { Cmd = cmd
                      CurrentAssessment = state
                      Policy = policy }

                let! decision = evaluateEscalateRisk escalateRules input
                let traceId = Guid.NewGuid().ToString()

                match decision.Outcome with
                | EscalationAuthorized targetExceptionId ->
                    let events =
                        [ CommitmentRiskEscalated(
                            cmd.AssessmentId,
                            assessment.CommitmentId,
                            targetExceptionId,
                            cmd.EscalationTime,
                            traceId) ]
                    return buildTrace (Some policy) traceId decision state events $"Commitment risk escalated to exception: {cmd.EscalationReason}"

                | EscalationRejected reason ->
                    return! Error(DomainError.rule (reason, rule = Rules.commitmentBreachEscalationRule.Id))
        }
