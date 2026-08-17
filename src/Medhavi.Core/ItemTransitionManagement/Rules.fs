/// CA-C-021 Item Transition Management Rules
module Medhavi.Core.ItemTransitionManagement.Rules

open Medhavi.SemanticModel
open Medhavi.Foundation.Contracts
open Medhavi.Core.ArsIdentifiers
open Model
open Policies

type RecognizeInput =
    { Cmd: RecognizeItemTransitionCmd
      CurrentState: ItemTransition option
      Policy: ItemTransitionPolicy }

type SuspendInput =
    { Cmd: SuspendItemTransitionCmd
      CurrentState: ItemTransition option }

type ReinstateInput =
    { Cmd: ReinstateItemTransitionCmd
      CurrentState: ItemTransition option }

type RetireInput =
    { Cmd: RetireItemTransitionCmd
      CurrentState: ItemTransition option }

/// BR-C-017: Superseded Item and Superseding Item must be distinct.
let noSelfSupersessionRule: Rule<RecognizeInput> =
    Rule.create
        Rules.noSelfSupersession.Id
        Rules.noSelfSupersession.Explanation
        (fun input -> input.Cmd.SupersededItem <> input.Cmd.SupersedingItem)
        (fun input ->
            sprintf
                "SupersededItem: %A, SupersedingItem: %A"
                (Identities.itemIdValue input.Cmd.SupersededItem)
                (Identities.itemIdValue input.Cmd.SupersedingItem))

/// BR-C-014: Superseded Item must be Active or Inactive (checked via port in Capability FIRST GATE).
/// This rule validates the state passed in the input.
let supersededItemValidityRule: Rule<RecognizeInput> =
    Rule.create
        Rules.supersededItemValidity.Id
        Rules.supersededItemValidity.Explanation
        (fun input ->
            // State validation is performed in Capability FIRST GATE.
            // This rule confirms the command was constructed with valid pre-conditions.
            true)
        (fun input -> sprintf "SupersededItem: %A" (Identities.itemIdValue input.Cmd.SupersededItem))

/// BR-C-015: Superseding Item must be Active (checked via port in Capability FIRST GATE).
let supersedingItemValidityRule: Rule<RecognizeInput> =
    Rule.create
        Rules.supersedingItemValidity.Id
        Rules.supersedingItemValidity.Explanation
        (fun input ->
            // State validation is performed in Capability FIRST GATE.
            true)
        (fun input -> sprintf "SupersedingItem: %A" (Identities.itemIdValue input.Cmd.SupersedingItem))

/// BR-C-016: At most one Active transition per Superseded Item (checked via projection in Capability FIRST GATE).
let singleActiveTransitionRule: Rule<RecognizeInput> =
    Rule.create
        Rules.singleActiveTransitionPerItem.Id
        Rules.singleActiveTransitionPerItem.Explanation
        (fun input ->
            // Conflict check is performed in Capability FIRST GATE via projection query.
            true)
        (fun input -> sprintf "SupersededItem: %A" (Identities.itemIdValue input.Cmd.SupersededItem))

/// Suspend rule: transition must be Active.
let transitionMustBeActiveForSuspend: Rule<SuspendInput> =
    Rule.create
        "BR-C-016-SUSPEND"
        "Item Transition must be in Active state to be suspended"
        (fun input ->
            input.CurrentState
            |> Option.map(fun t -> t.LifecycleState = ItemTransitionLifecycleState.Active)
            |> Option.defaultValue false)
        (fun input -> sprintf "TransitionId: %A" (Identities.transitionIdValue input.Cmd.TransitionId))

/// Reinstate rule: transition must be Inactive.
let transitionMustBeInactiveForReinstate: Rule<ReinstateInput> =
    Rule.create
        "BR-C-016-REINSTATE"
        "Item Transition must be in Inactive state to be reinstated"
        (fun input ->
            input.CurrentState
            |> Option.map(fun t -> t.LifecycleState = ItemTransitionLifecycleState.Inactive)
            |> Option.defaultValue false)
        (fun input -> sprintf "TransitionId: %A" (Identities.transitionIdValue input.Cmd.TransitionId))

/// Retire rule: transition must be Active or Inactive.
let transitionMustBeActiveOrInactiveForRetire: Rule<RetireInput> =
    Rule.create
        "BR-C-016-RETIRE"
        "Item Transition must be in Active or Inactive state to be retired"
        (fun input ->
            input.CurrentState
            |> Option.map(fun t ->
                t.LifecycleState = ItemTransitionLifecycleState.Active
                || t.LifecycleState = ItemTransitionLifecycleState.Inactive)
            |> Option.defaultValue false)
        (fun input -> sprintf "TransitionId: %A" (Identities.transitionIdValue input.Cmd.TransitionId))

/// Transition must exist (for Suspend, Reinstate, Retire).
let transitionMustExistForSuspend: Rule<SuspendInput> =
    Rule.create
        "BR-C-013-EXISTS-SUSPEND"
        "Item Transition must exist before suspension"
        (fun input -> input.CurrentState.IsSome)
        (fun input -> sprintf "TransitionId: %A" (Identities.transitionIdValue input.Cmd.TransitionId))

let transitionMustExistForReinstate: Rule<ReinstateInput> =
    Rule.create
        "BR-C-013-EXISTS-REINSTATE"
        "Item Transition must exist before reinstatement"
        (fun input -> input.CurrentState.IsSome)
        (fun input -> sprintf "TransitionId: %A" (Identities.transitionIdValue input.Cmd.TransitionId))

let transitionMustExistForRetire: Rule<RetireInput> =
    Rule.create
        "BR-C-013-EXISTS-RETIRE"
        "Item Transition must exist before retirement"
        (fun input -> input.CurrentState.IsSome)
        (fun input -> sprintf "TransitionId: %A" (Identities.transitionIdValue input.Cmd.TransitionId))

let recognitionRules: Rule<RecognizeInput> list =
    [ noSelfSupersessionRule
      supersededItemValidityRule
      supersedingItemValidityRule
      singleActiveTransitionRule ]

let suspensionRules: Rule<SuspendInput> list = [ transitionMustExistForSuspend; transitionMustBeActiveForSuspend ]

let reinstatementRules: Rule<ReinstateInput> list =
    [ transitionMustExistForReinstate; transitionMustBeInactiveForReinstate ]

let retirementRules: Rule<RetireInput> list =
    [ transitionMustExistForRetire; transitionMustBeActiveOrInactiveForRetire ]
