/// SE-D-005 — Planning Classification Assignment Read Model Projections
/// Pure Functional Projection Fold (Layer E: Catamorphism)
module Medhavi.Demand.SegmentDemand.PlanningClassificationAssignment.Projections

open Medhavi.SemanticModel
open Medhavi.Demand
open Medhavi.Contracts.Demand
open Model

let mapChangeEventToDto (event: AssignmentChangeEvent) : AssignmentChangeEventDto =
    { PreviousClassification = event.FromClassification |> Option.map (fun c -> c.AsString)
      NewClassification = event.ToClassification.AsString
      ClassificationScore = event.ClassificationScore
      ClassificationConfidence = event.ClassificationConfidence
      Rationale = event.Rationale
      PolicyVersion = event.PolicyVersion
      Timestamp = Timestamp.value event.Timestamp }

let mapToDto (assignment: PlanningClassificationAssignment) : PlanningClassificationDto =
    { AssignmentId = PlanningClassificationAssignmentId.value assignment.AssignmentId
      EntityType = assignment.EntityType.AsString
      EntityId = assignment.EntityId
      ClassificationType = assignment.ClassificationType.AsString
      CurrentClassification = assignment.CurrentClassification.AsString
      AnalogItemReference = assignment.AnalogItemReference |> Option.map ItemId.value
      ClassificationScore = assignment.ClassificationScore
      ClassificationConfidence = assignment.ClassificationConfidence
      AssignmentRationale = assignment.AssignmentRationale
      PolicyVersion = assignment.PolicyVersion
      ChangeEvents = assignment.ChangeEvents |> List.map mapChangeEventToDto
      LastUpdated = Timestamp.value assignment.LastUpdated }

/// Projection state: Map of PlanningClassificationAssignmentId to DTO
type State = Map<PlanningClassificationAssignmentId, PlanningClassificationDto>

let initial: State = Map.empty

/// Pure projection fold (Layer E: Catamorphism)
let apply (state: State) (event: PlanningClassificationEvent) : State =
    match event with
    | PlanningClassificationAssigned(assignment, _, _)
    | PlanningClassificationOverridden(assignment, _, _) ->
        let dto = mapToDto assignment
        Map.add assignment.AssignmentId dto state

/// Seed projection from existing aggregates
let seedFromAggregates (aggregates: PlanningClassificationAssignment list) : State =
    aggregates
    |> List.fold
        (fun state assessment ->
            let dto = mapToDto assessment
            Map.add assessment.AssignmentId dto state)
        initial
