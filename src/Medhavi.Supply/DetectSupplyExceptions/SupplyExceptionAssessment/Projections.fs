/// SE-S-110 — Supply Exception Assessment Projections
/// Maps pure domain aggregate states and events to Medhavi.Contracts.Supply DTOs
module Medhavi.Supply.DetectSupplyExceptions.SupplyExceptionAssessment.Projections

open Medhavi.Contracts.Supply
open Medhavi.SemanticModel
open Medhavi.Supply
open Model

let mapSeverityImpactToDto (imp: SeverityBusinessImpact) : string =
    match imp with
    | SeverityBusinessImpact.Negligible -> "Negligible"
    | SeverityBusinessImpact.Minor -> "Minor"
    | SeverityBusinessImpact.Moderate -> "Moderate"
    | SeverityBusinessImpact.Major -> "Major"
    | SeverityBusinessImpact.Severe -> "Severe"

let mapSeverityUrgencyToDto (urg: SeverityUrgency) : string =
    match urg with
    | SeverityUrgency.Immediate -> "Immediate"
    | SeverityUrgency.ShortTerm -> "ShortTerm"
    | SeverityUrgency.MediumTerm -> "MediumTerm"
    | SeverityUrgency.Low -> "Low"

let mapSeverityScopeToDto (scp: SeverityScope) : string =
    match scp with
    | SeverityScope.SingleItem -> "SingleItem"
    | SeverityScope.ProductFamily -> "ProductFamily"
    | SeverityScope.Supplier -> "Supplier"
    | SeverityScope.Resource -> "Resource"
    | SeverityScope.Location -> "Location"
    | SeverityScope.BusinessUnit -> "BusinessUnit"
    | SeverityScope.Enterprise -> "Enterprise"

let mapPotentialImpactToDto (pot: SeverityPotentialImpact) : string =
    match pot with
    | SeverityPotentialImpact.Negligible -> "Negligible"
    | SeverityPotentialImpact.Minor -> "Minor"
    | SeverityPotentialImpact.Moderate -> "Moderate"
    | SeverityPotentialImpact.Major -> "Major"
    | SeverityPotentialImpact.Severe -> "Severe"

let mapCurrentImpactToDto (cur: SeverityCurrentImpact) : string =
    match cur with
    | SeverityCurrentImpact.None -> "None"
    | SeverityCurrentImpact.Buffered -> "Buffered"
    | SeverityCurrentImpact.PartiallyMaterialized -> "PartiallyMaterialized"
    | SeverityCurrentImpact.Materialized -> "Materialized"

let mapHorizonToDto (hor: ExceptionHorizon) : string =
    match hor with
    | ExceptionHorizon.Immediate -> "Immediate"
    | ExceptionHorizon.ShortTerm -> "ShortTerm"
    | ExceptionHorizon.MediumTerm -> "MediumTerm"
    | ExceptionHorizon.Future -> "Future"
    | ExceptionHorizon.Uncertain -> "Uncertain"

let mapConfidenceToDto (conf: ExceptionConfidence) : string =
    match conf with
    | ExceptionConfidence.High -> "High"
    | ExceptionConfidence.Medium -> "Medium"
    | ExceptionConfidence.Low -> "Low"

let mapLifecycleStateToDto (state: SupplyExceptionLifecycleState) : string =
    match state with
    | SupplyExceptionLifecycleState.Detected -> "Detected"
    | SupplyExceptionLifecycleState.Confirmed -> "Confirmed"
    | SupplyExceptionLifecycleState.Resolved -> "Resolved"
    | SupplyExceptionLifecycleState.Archived -> "Archived"

let mapStateChangeEventToDto (sc: StateChangeEvent) : StateChangeEventDto =
    { EventId = sc.EventId
      PreviousState = sc.PreviousState |> Option.map mapLifecycleStateToDto
      NewState = mapLifecycleStateToDto sc.NewState
      TriggeringEvidence = sc.TriggeringEvidence
      Rationale = sc.Rationale
      Timestamp = Timestamp.value sc.Timestamp }

/// Map pure domain aggregate state to external DTO
let mapToDto (sea: SupplyExceptionAssessment) : SupplyExceptionAssessmentDto =
    let (entityType, entityId) = ExceptionScope.entityTypeAndId sea.ExceptionScope
    { AssessmentId = SupplyExceptionId.value sea.Id
      GovernedPolicyReference = sea.GovernedPolicyReference
      ViolatedRuleId = sea.ViolatedRuleId
      ExceptionType = sea.ExceptionType.ToString()
      ScopeEntityType = entityType
      ScopeEntityId = entityId
      PlanningScopeId = PlanningScopeId.value sea.PlanningScopeId
      LifecycleState = mapLifecycleStateToDto sea.LifecycleState
      BusinessImpact = mapSeverityImpactToDto sea.CurrentSeverity.BusinessImpact
      Urgency = mapSeverityUrgencyToDto sea.CurrentSeverity.Urgency
      Scope = mapSeverityScopeToDto sea.CurrentSeverity.Scope
      PotentialImpact = mapPotentialImpactToDto sea.CurrentSeverity.PotentialImpact
      CurrentImpact = mapCurrentImpactToDto sea.CurrentSeverity.CurrentImpact
      ExceptionHorizon = mapHorizonToDto sea.ExceptionHorizon
      DetectionTimestamp = Timestamp.value sea.DetectionTimestamp
      ResolutionTimestamp = sea.ResolutionTimestamp |> Option.map Timestamp.value
      EvidenceReferences = sea.EvidenceReferences |> List.map (fun e -> sprintf "[%s] %s (%.2f)" e.AssessmentType e.Description e.Confidence)
      EvaluationConfidence = mapConfidenceToDto sea.EvaluationConfidence
      ReasonCodes = sea.ReasonCodes
      CorrelatedExceptionIds = sea.CorrelatedExceptionIds |> List.map SupplyExceptionId.value
      History = sea.StateChangeHistory |> List.map mapStateChangeEventToDto
      CreatedTimestamp = Timestamp.value sea.CreatedTimestamp
      UpdatedTimestamp = Timestamp.value sea.UpdatedTimestamp }

/// Projection state: Map of SupplyExceptionId to DTO
type State = Map<SupplyExceptionId, SupplyExceptionAssessmentDto>

let initial: State = Map.empty

/// Pure projection fold (catamorphism)
let apply (state: State) (event: SupplyExceptionEvent) : State =
    let snapshot = SupplyExceptionEvent.snapshot event
    state |> Map.add snapshot.Id (mapToDto snapshot)

/// Seed projection from existing aggregates
let seedFromAggregates (aggregates: SupplyExceptionAssessment list) : State =
    aggregates |> List.fold (fun st agg -> Map.add agg.Id (mapToDto agg) st) initial
