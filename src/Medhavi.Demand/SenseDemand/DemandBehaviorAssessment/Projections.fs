/// SE-D-004 — Demand Behavior Assessment Read Model Projections
/// Pure Functional Projection Fold (Layer E: Catamorphism)
module Medhavi.Demand.SenseDemand.DemandBehaviorAssessment.Projections

open Medhavi.SemanticModel
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Model

let private mapDirection (dir: DeviationDirection) : string =
    match dir with
    | Increase -> "Increase"
    | Decrease -> "Decrease"

let private mapConfidence (conf: AssessmentConfidence) : string =
    match conf with
    | High -> "High"
    | Medium -> "Medium"
    | Low -> "Low"

let private mapState (st: DemandBehaviorState) : string =
    match st with
    | Normal -> "Normal"
    | Elevated -> "Elevated"
    | Depressed -> "Depressed"
    | DemandBehaviorState.Critical -> "Critical"

let mapStateChangeEventToDto (evt: StateChangeEvent) : StateChangeEventDto =
    { Timestamp = Timestamp.value evt.Timestamp
      FromState = mapState evt.FromState
      ToState = mapState evt.ToState
      DeviationMagnitude = evt.DeviationMagnitude
      Direction = mapDirection evt.Direction
      Confidence = mapConfidence evt.Confidence
      CorroboratingSources = evt.CorroboratingSources
      BaselineReference = evt.BaselineReference
      DecisionTraceId = evt.DecisionTraceId }

/// Map domain aggregate state to DTO
let mapToDto (assessment: DemandBehaviorAssessment) : DemandBehaviorAssessmentDto =
    { AssessmentId = DemandBehaviorAssessmentId.value assessment.AssessmentId
      Item = ItemId.value assessment.Item
      Location = LocationId.value assessment.Location
      CurrentState = mapState assessment.CurrentState
      BaselineMean = assessment.BaselineMean
      BaselineStdDev = assessment.BaselineStdDev
      LastDeviationMagnitude = assessment.LastDeviationMagnitude
      LastDeviationDirection = assessment.Direction |> Option.map mapDirection
      CorroborationCount = assessment.CorroborationCount
      AssessmentConfidence = mapConfidence assessment.AssessmentConfidence
      StateChangeEvents = assessment.StateChangeEvents |> List.map mapStateChangeEventToDto }

/// Projection state: Map of DemandBehaviorAssessmentId to DTO
type State = Map<DemandBehaviorAssessmentId, DemandBehaviorAssessmentDto>

let initial: State = Map.empty

/// Pure projection fold (Layer E: Catamorphism)
let apply (state: State) (event: DemandBehaviorEvent) : State =
    match event with
    | BaselineInitialized assessment ->
        state |> Map.add assessment.AssessmentId (mapToDto assessment)
    | DemandBehaviorStateChanged(assessment, _, _) ->
        state |> Map.add assessment.AssessmentId (mapToDto assessment)
    | CriticalDemandBehaviorDetected(assessment, _) ->
        state |> Map.add assessment.AssessmentId (mapToDto assessment)

/// Seed projection from a collection of existing aggregates
let seedFromAggregates (aggregates: DemandBehaviorAssessment list) : State =
    aggregates
    |> List.fold
        (fun s agg -> Map.add agg.AssessmentId (mapToDto agg) s)
        initial
