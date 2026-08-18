/// SE-D-004 — Demand Behavior Assessment Aggregate Model
/// Traces to: Demand Intelligence Specification (SE-D-004, AB-D-010, Chapter 4.3.1)
module Medhavi.Demand.SenseDemand.DemandBehaviorAssessment.Model

open Medhavi.SemanticModel
open Medhavi.Demand

/// SE-D-004 Demand Behavior State (Information Model)
type DemandBehaviorState =
    | Normal
    | Elevated
    | Depressed
    | Critical

/// Deviation direction relative to expected baseline
type DeviationDirection =
    | Increase
    | Decrease

/// Confidence in the assessment
type AssessmentConfidence =
    | High
    | Medium
    | Low

/// BR-D-111 — State Change Events recorded in Demand Behavior Assessment are immutable.
type StateChangeEvent =
    { Timestamp: Timestamp
      FromState: DemandBehaviorState
      ToState: DemandBehaviorState
      DeviationMagnitude: decimal
      Direction: DeviationDirection
      Confidence: AssessmentConfidence
      CorroboratingSources: string list
      BaselineReference: string
      DecisionTraceId: string }

/// SE-D-004 — Demand Behavior Assessment Aggregate Root
/// Identity: Item (SE-C-001) + Location (SE-C-002) per BR-D-004
type DemandBehaviorAssessment =
    { AssessmentId: DemandBehaviorAssessmentId
      Item: ItemId
      Location: LocationId
      CurrentState: DemandBehaviorState
      BaselineMean: decimal
      BaselineStdDev: decimal
      LastDeviationMagnitude: decimal option
      Direction: DeviationDirection option
      CorroborationCount: int
      AssessmentConfidence: AssessmentConfidence
      StateChangeEvents: StateChangeEvent list }

    /// Static helper to construct the identity from item and location.
    static member GetId (itemId: ItemId) (locationId: LocationId) =
        $"{ItemId.value itemId}-{LocationId.value locationId}"

/// AB-D-010 Command: Initialize Baseline
type InitializeBaselineCmd =
    { AssessmentId: DemandBehaviorAssessmentId
      Item: ItemId
      Location: LocationId
      BaselineMean: decimal
      BaselineStdDev: decimal }

/// AB-D-010 Command: Evaluate Signal
type EvaluateSignalCmd =
    { AssessmentId: DemandBehaviorAssessmentId
      Item: ItemId
      Location: LocationId
      Quantity: Quantity
      SignalTimestamp: Timestamp
      CorroboratingSources: string list
      IsHighPriority: bool }

/// AB-D-010 Command: Evaluate Forecast Refresh Trigger on Critical State (DE-D-007)
type EvaluateForecastRefreshCmd =
    { AssessmentId: DemandBehaviorAssessmentId
      Item: ItemId
      Location: LocationId
      ForecastAgeHours: int
      ExpectedAccuracyImprovementWape: decimal }

/// Enterprise Events emitted by Demand Behavior Assessment aggregate
type DemandBehaviorEvent =
    | BaselineInitialized of DemandBehaviorAssessment
    | DemandBehaviorStateChanged of
        Assessment: DemandBehaviorAssessment *
        PreviousState: DemandBehaviorState *
        Event: StateChangeEvent
    | CriticalDemandBehaviorDetected of Assessment: DemandBehaviorAssessment * Event: StateChangeEvent

/// Pure evolution (Layer E: Catamorphism).
let evolve: Medhavi.Foundation.Contracts.Evolve<DemandBehaviorAssessment, DemandBehaviorEvent> =
    fun (_: DemandBehaviorAssessment option) (event: DemandBehaviorEvent) ->
        match event with
        | BaselineInitialized assessment -> Some assessment
        | DemandBehaviorStateChanged(assessment, _, _) -> Some assessment
        | CriticalDemandBehaviorDetected(assessment, _) -> Some assessment

/// Replay event sequence to rehydrate aggregate state
let replay (events: DemandBehaviorEvent seq) : DemandBehaviorAssessment option = Seq.fold evolve None events
