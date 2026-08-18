/// Sense Demand Aggregate Behaviors
/// Traces to: AB-D-010 Maintain Demand Behavior Understanding (Specification Chapter 4.3.1)
module Medhavi.Demand.SenseDemand.DemandBehaviorAssessment.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Foundation.Failure
open Medhavi.Demand
open Medhavi.Demand.ArsIdentifiers
open Model
open Policies
open Rules
open Decisions

let private buildDecisionTrace policyId policyVersion decisionId decisionOutcome state events summary =
    buildDecisionWithTrace
        evolve
        state
        events
        decisionId
        []
        ArsIdentifiers.Capabilities.senseDemand.Id
        decisionOutcome
        policyId
        policyVersion
        [ ArsIdentifiers.SemanticObjects.demandBehaviorAssessment.Id ]
        (Some summary)

/// AB-D-010: Initialize Baseline for Item-Location
let initializeBaseline: Decide<DemandBehaviorAssessment, InitializeBaselineCmd, DemandBehaviorEvent> =
    fun (cmd: InitializeBaselineCmd) (state: DemandBehaviorAssessment option) ->
        result {
            match state with
            | Some _ ->
                return!
                    Error(
                        DomainError.conflict(
                            "Demand Behavior Assessment baseline already exists for this Item-Location",
                            Capabilities.senseDemand.Id
                        )
                    )
            | None ->
                let initialAssessment: DemandBehaviorAssessment =
                    { AssessmentId = cmd.AssessmentId
                      Item = cmd.Item
                      Location = cmd.Location
                      CurrentState = Normal
                      BaselineMean = cmd.BaselineMean
                      BaselineStdDev = cmd.BaselineStdDev
                      LastDeviationMagnitude = None
                      Direction = None
                      CorroborationCount = 0
                      AssessmentConfidence = High
                      StateChangeEvents = [] }

                let events = [ BaselineInitialized initialAssessment ]

                let decision: DecisionOutcome<string> =
                    { Outcome = "BaselineInitialized"
                      Evaluations = [] }

                return
                    buildDecisionTrace
                        None
                        None
                        Capabilities.senseDemand.Id
                        decision
                        None
                        events
                        "Demand Behavior Assessment baseline initialized"
        }

/// AB-D-010: Maintain Demand Behavior Understanding (Evaluate Signal)
let evaluateSignal
    (sensingPolicy: DemandSensingPolicy)
    : Decide<DemandBehaviorAssessment, EvaluateSignalCmd, DemandBehaviorEvent> =
    fun (cmd: EvaluateSignalCmd) (state: DemandBehaviorAssessment option) ->
        result {
            match state with
            | None ->
                return!
                    Error(
                        DomainError.notFound(
                            "DemandBehaviorAssessment",
                            DemandBehaviorAssessmentId.value cmd.AssessmentId
                        )
                    )
            | Some current ->
                let input: SignalEvaluationInput =
                    { SignalQuantity = cmd.Quantity
                      BaselineMean = current.BaselineMean
                      BaselineStdDev = current.BaselineStdDev
                      CorroborationCount = cmd.CorroboratingSources.Length
                      IsHighPriority = cmd.IsHighPriority
                      Policy = sensingPolicy }

                let! decision =
                    Decisions.evaluateSignalStateChange
                        Rules.signalEvaluationRules
                        input
                        current.CurrentState
                        cmd.SignalTimestamp

                let traceId = Guid.NewGuid().ToString()

                if decision.Outcome.Determination.StateTransitionOccurred then
                    let stateChangeEvent: StateChangeEvent =
                        { Timestamp = cmd.SignalTimestamp
                          FromState = current.CurrentState
                          ToState = decision.Outcome.DeterminedState
                          DeviationMagnitude = abs decision.Outcome.Deviation.MagnitudeSigma
                          Direction = decision.Outcome.Deviation.Direction
                          Confidence = decision.Outcome.Deviation.AssessmentConfidence
                          CorroboratingSources = cmd.CorroboratingSources
                          BaselineReference =
                            sprintf "Mean: %.2f, StdDev: %.2f" current.BaselineMean current.BaselineStdDev
                          DecisionTraceId = traceId }

                    let updatedAssessment =
                        { current with
                            CurrentState = decision.Outcome.DeterminedState
                            LastDeviationMagnitude = Some(abs decision.Outcome.Deviation.MagnitudeSigma)
                            Direction = Some decision.Outcome.Deviation.Direction
                            CorroborationCount = cmd.CorroboratingSources.Length
                            AssessmentConfidence = decision.Outcome.Deviation.AssessmentConfidence
                            StateChangeEvents = stateChangeEvent :: current.StateChangeEvents }

                    let events =
                        if updatedAssessment.CurrentState = DemandBehaviorState.Critical then
                            [ DemandBehaviorStateChanged(updatedAssessment, current.CurrentState, stateChangeEvent)
                              CriticalDemandBehaviorDetected(updatedAssessment, stateChangeEvent) ]
                        else
                            [ DemandBehaviorStateChanged(updatedAssessment, current.CurrentState, stateChangeEvent) ]

                    return
                        buildDecisionTrace
                            (Some sensingPolicy.PolicyId)
                            (Some sensingPolicy.Version)
                            ArsIdentifiers.Decisions.evaluateDemandSignalForStateChange.Id
                            decision
                            (Some current)
                            events
                            decision.Outcome.Determination.Rationale
                else
                    // No state transition; assessment reflects evaluated deviation magnitude and direction
                    let updatedAssessment =
                        { current with
                            LastDeviationMagnitude = Some(abs decision.Outcome.Deviation.MagnitudeSigma)
                            Direction = Some decision.Outcome.Deviation.Direction
                            CorroborationCount = cmd.CorroboratingSources.Length
                            AssessmentConfidence = decision.Outcome.Deviation.AssessmentConfidence }

                    let events = []

                    return
                        buildDecisionTrace
                            (Some sensingPolicy.PolicyId)
                            (Some sensingPolicy.Version)
                            ArsIdentifiers.Decisions.evaluateDemandSignalForStateChange.Id
                            decision
                            (Some current)
                            events
                            decision.Outcome.Determination.Rationale
        }

/// AB-D-010: Evaluate Forecast Refresh Trigger on Critical State (DE-D-007)
let evaluateForecastRefresh
    (triggerPolicy: ForecastRefreshTriggerPolicy)
    : Decide<DemandBehaviorAssessment, EvaluateForecastRefreshCmd, DemandBehaviorEvent> =
    fun (cmd: EvaluateForecastRefreshCmd) (state: DemandBehaviorAssessment option) ->
        result {
            match state with
            | None ->
                return!
                    Error(
                        DomainError.notFound(
                            "DemandBehaviorAssessment",
                            DemandBehaviorAssessmentId.value cmd.AssessmentId
                        )
                    )
            | Some current ->
                let input: Rules.RefreshEvaluationInput =
                    { CurrentState = current.CurrentState
                      ForecastAgeHours = cmd.ForecastAgeHours
                      ExpectedAccuracyImprovementWape = cmd.ExpectedAccuracyImprovementWape
                      Policy = triggerPolicy }

                let! decision = Decisions.evaluateForecastRefresh Rules.refreshEvaluationRules input

                let events = []

                return
                    buildDecisionTrace
                        (Some triggerPolicy.PolicyId)
                        (Some triggerPolicy.Version)
                        ArsIdentifiers.Decisions.triggerForecastRefreshOnCriticalState.Id
                        decision
                        (Some current)
                        events
                        decision.Outcome.Rationale
        }
