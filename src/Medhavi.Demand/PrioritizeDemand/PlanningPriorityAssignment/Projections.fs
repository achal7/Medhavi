/// SE-D-007 — Planning Priority Assignment Read Model Projections
/// Pure Functional Projection Fold (Layer E: Catamorphism)
module Medhavi.Demand.PrioritizeDemand.PlanningPriorityAssignment.Projections

open Medhavi.SemanticModel
open Medhavi.Demand
open Medhavi.Contracts.Demand
open Model

let mapDimensionBreakdownToDto (breakdown: DimensionScoreBreakdown) : DimensionScoreBreakdownDto =
    { RevenueScore = breakdown.RevenueScore
      StrategyScore = breakdown.StrategyScore
      RiskScore = breakdown.RiskScore
      ContractualScore = breakdown.ContractualScore }

let mapChangeEventToDto (event: PriorityChangeEvent) : PriorityChangeEventDto =
    { PreviousPriority = event.FromPriority |> Option.map (fun p -> p.AsString)
      NewPriority = event.ToPriority.AsString
      PriorityScore = event.PriorityScore
      DimensionBreakdown = mapDimensionBreakdownToDto event.DimensionBreakdown
      Rationale = event.Rationale
      BusinessValidity = event.BusinessValidity
      PolicyVersion = event.PolicyVersion
      Timestamp = Timestamp.value event.Timestamp }

let mapToDto (assignment: PlanningPriorityAssignment) : PlanningPriorityDto =
    { AssignmentId = PlanningPriorityAssignmentId.value assignment.AssignmentId
      EntityType = assignment.EntityType.AsString
      EntityId = assignment.EntityId
      CurrentPriority = assignment.CurrentPriority.AsString
      PriorityScore = assignment.PriorityScore
      DimensionBreakdown = mapDimensionBreakdownToDto assignment.DimensionBreakdown
      DecisionRationale = assignment.DecisionRationale
      BusinessValidity = assignment.BusinessValidity
      PolicyVersion = assignment.PolicyVersion
      ChangeEvents = assignment.ChangeEvents |> List.map mapChangeEventToDto
      LastUpdated = Timestamp.value assignment.LastUpdated }

/// Projection state: Map of PlanningPriorityAssignmentId to DTO
type State = Map<PlanningPriorityAssignmentId, PlanningPriorityDto>

let initial: State = Map.empty

/// Pure projection fold (Layer E: Catamorphism)
let apply (state: State) (event: PlanningPriorityEvent) : State =
    match event with
    | PlanningPriorityAssigned(assignment, _, _)
    | PlanningPriorityOverridden(assignment, _, _) ->
        let dto = mapToDto assignment
        Map.add assignment.AssignmentId dto state

/// Seed projection from existing aggregates
let seedFromAggregates (aggregates: PlanningPriorityAssignment list) : State =
    aggregates
    |> List.fold
        (fun state assessment ->
            let dto = mapToDto assessment
            Map.add assessment.AssignmentId dto state)
        initial
