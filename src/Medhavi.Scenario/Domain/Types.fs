namespace Medhavi.Scenario.Domain

open System
open Medhavi.Contracts.Scenario

type ScenarioConfigurationId = private ScenarioConfigurationId of Guid

module ScenarioConfigurationId =
    let create () = ScenarioConfigurationId(Guid.NewGuid())
    let value (ScenarioConfigurationId id) = id

type ScenarioOverlaySetId = private ScenarioOverlaySetId of Guid

module ScenarioOverlaySetId =
    let create () = ScenarioOverlaySetId(Guid.NewGuid())
    let value (ScenarioOverlaySetId id) = id

/// Tracks whether a structural change (BOM, routing) invalidates the current plan.
[<System.Text.Json.Serialization.JsonFSharpConverter>]
type StructuralChange =
    /// No structural change since last plan run.
    | Unchanged
    /// A structural change requires a full replan (e.g., BOM topology changed).
    | FullReplanRequired

// =============================================================================
// Scenario Policy — derives the narrowest valid planning mode from dirty state
// =============================================================================

module ScenarioPolicy =
    /// Determine the narrowest planning mode that is correct for the current
    /// dirty state. This prevents over-solving (full replanning when only one
    /// demand changed) while guaranteeing correctness.
    let determinePlanningMode (structural: StructuralChange) (dirtyReason: DirtyReason option) : PlanningMode =
        match structural with
        | FullReplanRequired -> PlanningMode.FullReplan
        | Unchanged ->
            match dirtyReason with
            | None -> PlanningMode.FullReplan
            | Some reason ->
                match reason with
                | DirtyReason.BomOrRoutingChanged _ -> PlanningMode.FullReplan
                | DirtyReason.PolicyChanged _ -> PlanningMode.FullReplan
                | DirtyReason.DemandDataChanged(_, _, changedIds) ->
                    if List.isEmpty changedIds then
                        PlanningMode.FullReplan
                    else
                        PlanningMode.ReactiveRepair changedIds
                | DirtyReason.CapacityDataChanged _ -> PlanningMode.ReactiveRepair []
                | DirtyReason.InventoryDataChanged _ -> PlanningMode.ReactiveRepair []
                | DirtyReason.OverlayChanged _ -> PlanningMode.ReactiveRepair []
                | DirtyReason.ManualPlannerChange _ -> PlanningMode.FullReplan
