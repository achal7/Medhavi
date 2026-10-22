/// DE-S-061: Determine Production Sequence
/// DE-S-062: Assess Schedule Feasibility
/// Traces to: Supply Intelligence Specification §6 (DE-S-061, DE-S-062)
module Medhavi.Supply.ScheduleProduction.ProductionSchedule.Decisions

open System
open Medhavi.Foundation.Failure
open Medhavi.Supply
open Model
open Policies

[<RequireQualifiedAccess>]
type SequenceDecisionOutcome =
    | OptimalSequenceSelected of activities: ProductionActivity list * changeovers: ChangeoverDetail list
    | NoFeasibleSequence of Reason: string

[<RequireQualifiedAccess>]
type FeasibilityDecisionOutcome =
    | Feasible
    | ConditionallyFeasible of Conditions: string list
    | Infeasible of Reasons: string list

/// DE-S-061 — Determine Production Sequence
/// Selection Decision: Evaluates candidate production sequence against PO-S-080 criteria and hard constraints
let determineProductionSequence
    (activities: ProductionActivity list)
    (changeovers: ChangeoverDetail list)
    (feasibility: FeasibilityEvaluation)
    (policy: SchedulingPolicyConfig)
    : Result<SequenceDecisionOutcome, DomainError> =
    if activities.IsEmpty then
        Ok(SequenceDecisionOutcome.NoFeasibleSequence "No production activities provided for sequencing")
    else
        match feasibility.Overall with
        | ScheduleFeasibility.Infeasible reasons ->
            Ok(SequenceDecisionOutcome.NoFeasibleSequence (String.concat "; " reasons))
        | _ ->
            Ok(SequenceDecisionOutcome.OptimalSequenceSelected(activities, changeovers))

/// DE-S-062 — Assess Schedule Feasibility
/// Assessment Decision: Multi-dimensional gate across capacity, material, tool, labor, maintenance, and frozen horizon
let assessScheduleFeasibility
    (evaluation: FeasibilityEvaluation)
    (policy: SchedulingPolicyConfig)
    : Result<FeasibilityDecisionOutcome, DomainError> =
    let violations =
        [ match evaluation.FiniteCapacity with
          | DimensionFeasibility.Violated r -> Some $"Capacity: {r}"
          | _ -> None
          match evaluation.MaterialAvailability with
          | DimensionFeasibility.Violated r -> Some $"Material: {r}"
          | _ -> None
          match evaluation.MaintenanceAndCalendar with
          | DimensionFeasibility.Violated r -> Some $"Calendar: {r}"
          | _ -> None
          match evaluation.FrozenHorizon with
          | DimensionFeasibility.Violated r -> Some $"Frozen Horizon: {r}"
          | _ -> None ]
        |> List.choose id

    if not violations.IsEmpty then
        Ok(FeasibilityDecisionOutcome.Infeasible violations)
    else
        let conditions =
            [ match evaluation.FiniteCapacity with
              | DimensionFeasibility.Conditional c -> Some c
              | _ -> None
              match evaluation.MaterialAvailability with
              | DimensionFeasibility.Conditional c -> Some c
              | _ -> None
              match evaluation.Overall with
              | ScheduleFeasibility.ConditionallyFeasible conds -> Some (String.concat "; " conds)
              | _ -> None ]
            |> List.choose id

        if not conditions.IsEmpty then
            Ok(FeasibilityDecisionOutcome.ConditionallyFeasible conditions)
        else
            Ok FeasibilityDecisionOutcome.Feasible
