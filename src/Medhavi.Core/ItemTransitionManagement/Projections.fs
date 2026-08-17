/// CA-C-021 Item Transition Management Projections
module Medhavi.Core.ItemTransitionManagement.Projections

open Medhavi.SemanticModel
open Medhavi.Contracts.Core.ItemTransition
open Model

/// Map aggregate state to DTO.
let mapToDto (transition: ItemTransition) : ItemTransitionDto =
    { TransitionId = Identities.transitionIdValue transition.TransitionIdentifier
      SupersededItem = Identities.itemIdValue transition.SupersededItem
      SupersedingItem = Identities.itemIdValue transition.SupersedingItem
      TransitionType = Identities.vocabularyEntryIdValue transition.TransitionType
      EffectiveDate = Timestamp.value transition.EffectiveDate
      EndDate = transition.EndDate |> Option.map Timestamp.value
      LifecycleState =
        match transition.LifecycleState with
        | ItemTransitionLifecycleState.Active -> "Active"
        | ItemTransitionLifecycleState.Inactive -> "Inactive"
        | ItemTransitionLifecycleState.Retired -> "Retired" }

/// Projection state: Map of TransitionId to DTO.
type State = Map<TransitionId, ItemTransitionDto>

let initial: State = Map.empty

/// Pure projection fold.
let apply (state: State) (event: ItemTransitionEvent) : State =
    match event with
    | ItemTransitionRecognized transition -> state |> Map.add transition.TransitionIdentifier (mapToDto transition)
    | ItemTransitionSuspended(transitionId, _) ->
        state |> Map.change transitionId (Option.map(fun dto -> { dto with LifecycleState = "Inactive" }))
    | ItemTransitionReinstated(transitionId, _) ->
        state |> Map.change transitionId (Option.map(fun dto -> { dto with LifecycleState = "Active" }))
    | ItemTransitionRetired(transitionId, _) ->
        state |> Map.change transitionId (Option.map(fun dto -> { dto with LifecycleState = "Retired" }))

/// Seed projection from existing aggregates.
let seedFromAggregates (aggregates: ItemTransition list) : State =
    aggregates
    |> List.fold
        (fun state agg ->
            let dto = mapToDto agg
            Map.add agg.TransitionIdentifier dto state)
        initial
