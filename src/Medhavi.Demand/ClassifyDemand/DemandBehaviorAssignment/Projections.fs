/// SE-D-006 — Demand Behavior Assignment Read Model Projections
module Medhavi.Demand.ClassifyDemand.DemandBehaviorAssignment.Projections

open Medhavi.SemanticModel
open Medhavi.Demand
open Medhavi.Contracts.Demand
open Model

let mapStatisticalFeaturesToDto (features: StatisticalFeatures) : StatisticalFeaturesDto =
    { CoefficientOfVariation = features.CoefficientOfVariation
      SquaredCoefficientOfVariation = features.SquaredCoefficientOfVariation
      AverageDemandInterval = features.AverageDemandInterval
      AutocorrelationAtSeasonalLag = features.AutocorrelationAtSeasonalLag
      TrendPValue = features.TrendPValue
      ZeroDemandRatio = features.ZeroDemandRatio
      SamplePeriodCount = features.SamplePeriodCount }

let mapChangeEventToDto (event: BehaviorChangeEvent) : BehaviorChangeEventDto =
    { PreviousClassification = event.FromClassification |> Option.map(fun c -> c.AsString)
      NewClassification = event.ToClassification.AsString
      StatisticalFeatures = event.StatisticalFeatures |> Option.map mapStatisticalFeaturesToDto
      Confidence = event.Confidence
      Rationale = event.Rationale
      PolicyVersion = event.PolicyVersion
      Timestamp = Timestamp.value event.Timestamp }

let mapToDto (assignment: DemandBehaviorAssignment) : DemandBehaviorAssignmentDto =
    { AssignmentId = DemandBehaviorAssignmentId.value assignment.AssignmentId
      ItemId = ItemId.value assignment.Item
      LocationId = LocationId.value assignment.Location
      Dimension = assignment.Dimension.AsString
      CurrentClassification = assignment.CurrentClassification.AsString
      StatisticalFeatures = assignment.StatisticalFeatures |> Option.map mapStatisticalFeaturesToDto
      ClassificationConfidence = assignment.ClassificationConfidence
      AssignmentRationale = assignment.AssignmentRationale
      PolicyVersion = assignment.PolicyVersion
      ChangeEvents = assignment.ChangeEvents |> List.map mapChangeEventToDto
      LastUpdated = Timestamp.value assignment.LastUpdated }

/// Projection state: Map of DemandBehaviorAssignmentId to DTO
type State = Map<DemandBehaviorAssignmentId, DemandBehaviorAssignmentDto>

let initial: State = Map.empty

/// Pure projection fold (Layer E: Catamorphism)
let apply (state: State) (event: DemandBehaviorEvent) : State =
    match event with
    | DemandBehaviorClassified(assignment, _, _)
    | DemandBehaviorOverridden(assignment, _, _) ->
        let dto = mapToDto assignment
        Map.add assignment.AssignmentId dto state

/// Seed projection from existing aggregates
let seedFromAggregates (aggregates: DemandBehaviorAssignment list) : State =
    aggregates
    |> List.fold
        (fun state assessment ->
            let dto = mapToDto assessment
            Map.add assessment.AssignmentId dto state)
        initial
