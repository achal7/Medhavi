/// CA-C-021 Item Transition Management Model
module Medhavi.Core.ItemTransitionManagement.Model

open Medhavi.SemanticModel

/// AB-C-005 input. TransitionId is DERIVED by the ACL from business identity (BR-C-013).
type RecognizeItemTransitionCmd =
    { TransitionId: TransitionId
      SupersededItem: ItemId
      SupersedingItem: ItemId
      TransitionType: VocabularyEntryId
      EffectiveDate: Timestamp
      EndDate: Timestamp option }

/// AB-C-006 input
type SuspendItemTransitionCmd =
    { TransitionId: TransitionId
      SuspensionTime: Timestamp }

/// AB-C-007 input
type ReinstateItemTransitionCmd =
    { TransitionId: TransitionId
      ReinstatementTime: Timestamp }

/// AB-C-008 input
type RetireItemTransitionCmd =
    { TransitionId: TransitionId
      RetirementTime: Timestamp }

type ItemTransitionCmd =
    | Recognize of RecognizeItemTransitionCmd
    | Suspend of SuspendItemTransitionCmd
    | Reinstate of ReinstateItemTransitionCmd
    | Retire of RetireItemTransitionCmd

    static member GetId(cmd: ItemTransitionCmd) =
        match cmd with
        | Recognize c -> c.TransitionId
        | Suspend c -> c.TransitionId
        | Reinstate c -> c.TransitionId
        | Retire c -> c.TransitionId

/// EV-C-006 (recognized), EV-C-007 (suspended), EV-C-008 (reinstated), EV-C-009 (retired).
type ItemTransitionEvent =
    | ItemTransitionRecognized of ItemTransition
    | ItemTransitionSuspended of TransitionId * SuspensionTime: Timestamp
    | ItemTransitionReinstated of TransitionId * ReinstatementTime: Timestamp
    | ItemTransitionRetired of TransitionId * RetirementTime: Timestamp

/// Pure evolution. No validation in evolve (Rule 6.1).
let evolve (state: ItemTransition option) (event: ItemTransitionEvent) : ItemTransition option =
    match event with
    | ItemTransitionRecognized transition -> Some transition
    | ItemTransitionSuspended(transitionId, _) ->
        state
        |> Option.map(fun t ->
            if t.TransitionIdentifier = transitionId then
                { t with
                    LifecycleState = ItemTransitionLifecycleState.Inactive }
            else
                t)
    | ItemTransitionReinstated(transitionId, _) ->
        state
        |> Option.map(fun t ->
            if t.TransitionIdentifier = transitionId then
                { t with
                    LifecycleState = ItemTransitionLifecycleState.Active }
            else
                t)
    | ItemTransitionRetired(transitionId, _) ->
        state
        |> Option.map(fun t ->
            if t.TransitionIdentifier = transitionId then
                { t with
                    LifecycleState = ItemTransitionLifecycleState.Retired }
            else
                t)

let replay (events: ItemTransitionEvent seq) : ItemTransition option = Seq.fold evolve None events
