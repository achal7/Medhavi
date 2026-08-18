/// SE-D-018 — Demand Intervention Impact Read Model Projections
/// Pure Functional Projection Fold (Layer E: Catamorphism)
module Medhavi.Demand.ModelDemandInterventions.DemandInterventionImpact.Projections

open Medhavi.Contracts.Demand
open Medhavi.Demand
open Medhavi.SemanticModel
open Model

let mapToDto (impact: DemandInterventionImpact) : DemandInterventionImpactDto =
    { ImpactId = DemandInterventionImpactId.value impact.ImpactId
      InterventionReference = ScenarioAdjustmentId.value impact.InterventionReference
      Item = ItemId.value impact.Item
      Location = LocationId.value impact.Location
      AssessedDemandLift = Quantity.value impact.AssessedDemandLift
      LiftConfidence = impact.LiftConfidence
      TemporalValidityStart = Timestamp.value impact.TemporalValidity.Start
      TemporalValidityEnd = Timestamp.value impact.TemporalValidity.End
      ModelProvenance = impact.ModelProvenance.AsString
      LifecycleState = impact.LifecycleState.AsString
      Version = impact.Version
      CreatedAt = Timestamp.value impact.CreatedAt
      PublishedAt = impact.PublishedAt |> Option.map Timestamp.value }

/// Projection state: Map of DemandInterventionImpactId to DemandInterventionImpactDto
type State = Map<DemandInterventionImpactId, DemandInterventionImpactDto>

let initial: State = Map.empty

/// Pure projection fold (Layer E: Catamorphism)
let apply (state: State) (event: DemandInterventionImpactEvent) : State =
    match event with
    | InterventionImpactAssessed impact ->
        let dto = mapToDto impact
        Map.add impact.ImpactId dto state
    | InterventionImpactPublished(impact, previousImpactIdOpt) ->
        let dto = mapToDto impact
        let stateWithPublished = Map.add impact.ImpactId dto state
        match previousImpactIdOpt with
        | Some prevId ->
            match Map.tryFind prevId stateWithPublished with
            | Some prevDto ->
                let supersededDto = { prevDto with LifecycleState = "Superseded" }
                Map.add prevId supersededDto stateWithPublished
            | None -> stateWithPublished
        | None -> stateWithPublished

/// Seed projection from existing aggregates
let seedFromAggregates (aggregates: DemandInterventionImpact list) : State =
    aggregates
    |> List.fold
        (fun state agg ->
            let dto = mapToDto agg
            Map.add agg.ImpactId dto state)
        initial
