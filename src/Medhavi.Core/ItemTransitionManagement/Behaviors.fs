/// CA-C-021 Aggregate Behaviors
module Medhavi.Core.ItemTransitionManagement.Behaviors

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Model
open Rules
open Decisions
open Policies

/// AB-C-005: Recognize Item Transition.
let recognize
    (policy: ItemTransitionPolicy)
    (cmd: RecognizeItemTransitionCmd)
    (state: ItemTransition option)
    : Result<Decision<ItemTransition, ItemTransitionEvent>, DomainError> =
    result {
        let input: RecognizeInput =
            { Cmd = cmd
              CurrentState = state
              Policy = policy }

        let! (decision: DecisionOutcome<RecognitionOutcome>) =
            Decisions.evaluateRecognition Rules.recognitionRules input

        match decision.Outcome with
        | RejectRecognition reasons ->
            return!
                Error(
                    DomainError.rule(
                        (String.concat "; " reasons),
                        Medhavi.Core.ArsIdentifiers.Decisions.validateItemTransitionRecognition.Id
                    )
                )

        | RecognizeTransition ->
            let newTransition: ItemTransition =
                { TransitionIdentifier = cmd.TransitionId
                  SupersededItem = cmd.SupersededItem
                  SupersedingItem = cmd.SupersedingItem
                  TransitionType = cmd.TransitionType
                  EffectiveDate = cmd.EffectiveDate
                  EndDate = cmd.EndDate
                  LifecycleState = ItemTransitionLifecycleState.Active }

            let events = [ ItemTransitionRecognized newTransition ]

            let trace: DecisionTrace =
                { DecisionId = System.Guid.NewGuid().ToString()
                  CapabilityId = Medhavi.Core.ArsIdentifiers.Capabilities.manageItemTransitions.Id
                  CausalDecisionIds = []
                  Outcome = "Recognized"
                  PolicyId = Some policy.PolicyId
                  PolicyVersion = Some policy.Version
                  Rationale =
                    { Summary = sprintf "Recognized Item Transition %A" (Identities.transitionIdValue cmd.TransitionId)
                      Evidence = decision.Evaluations |> List.collect(fun e -> e.Evidence)
                      Alternatives = [ ("RejectRecognition", "All recognition criteria satisfied") ] }
                  RulesEvaluated = decision.Evaluations
                  SemanticObjectIds = [ "ItemTransition" ] }

            let! newState =
                events
                |> List.fold evolve state
                |> Result.ofOption(DomainError.invariant "Item Transition state must exist after recognition")

            return
                { NewState = newState
                  Events = events
                  Trace = Some trace }
    }

/// AB-C-006: Suspend Item Transition.
let suspend
    (policy: ItemTransitionPolicy)
    (cmd: SuspendItemTransitionCmd)
    (state: ItemTransition option)
    : Result<Decision<ItemTransition, ItemTransitionEvent>, DomainError> =
    result {
        let input: SuspendInput = { Cmd = cmd; CurrentState = state }

        let! (decision: DecisionOutcome<SuspensionOutcome>) = Decisions.evaluateSuspension Rules.suspensionRules input

        match decision.Outcome with
        | RejectSuspension reasons ->
            return!
                Error(
                    DomainError.rule(
                        (String.concat "; " reasons),
                        Medhavi.Core.ArsIdentifiers.Capabilities.manageItemTransitions.Id
                    )
                )

        | SuspendTransition ->
            let events = [ ItemTransitionSuspended(cmd.TransitionId, cmd.SuspensionTime) ]

            let trace: DecisionTrace =
                { DecisionId = System.Guid.NewGuid().ToString()
                  CapabilityId = Medhavi.Core.ArsIdentifiers.Capabilities.manageItemTransitions.Id
                  CausalDecisionIds = []
                  Outcome = "Suspended"
                  PolicyId = Some policy.PolicyId
                  PolicyVersion = Some policy.Version
                  Rationale =
                    { Summary = sprintf "Suspended Item Transition %A" (Identities.transitionIdValue cmd.TransitionId)
                      Evidence = decision.Evaluations |> List.collect(fun e -> e.Evidence)
                      Alternatives = [] }
                  RulesEvaluated = decision.Evaluations
                  SemanticObjectIds = [ "ItemTransition" ] }

            let! newState =
                events
                |> List.fold evolve state
                |> Result.ofOption(DomainError.invariant "Item Transition state must exist after suspension")

            return
                { NewState = newState
                  Events = events
                  Trace = Some trace }
    }

/// AB-C-007: Reinstate Item Transition.
let reinstate
    (policy: ItemTransitionPolicy)
    (cmd: ReinstateItemTransitionCmd)
    (state: ItemTransition option)
    : Result<Decision<ItemTransition, ItemTransitionEvent>, DomainError> =
    result {
        let input: ReinstateInput = { Cmd = cmd; CurrentState = state }

        let! (decision: DecisionOutcome<ReinstatementOutcome>) =
            Decisions.evaluateReinstatement Rules.reinstatementRules input

        match decision.Outcome with
        | RejectReinstatement reasons ->
            return!
                Error(
                    DomainError.rule(
                        (String.concat "; " reasons),
                        Medhavi.Core.ArsIdentifiers.Capabilities.manageItemTransitions.Id
                    )
                )

        | ReinstateTransition ->
            let events = [ ItemTransitionReinstated(cmd.TransitionId, cmd.ReinstatementTime) ]

            let trace: DecisionTrace =
                { DecisionId = System.Guid.NewGuid().ToString()
                  CapabilityId = Medhavi.Core.ArsIdentifiers.Capabilities.manageItemTransitions.Id
                  CausalDecisionIds = []
                  Outcome = "Reinstated"
                  PolicyId = Some policy.PolicyId
                  PolicyVersion = Some policy.Version
                  Rationale =
                    { Summary = sprintf "Reinstated Item Transition %A" (Identities.transitionIdValue cmd.TransitionId)
                      Evidence = decision.Evaluations |> List.collect(fun e -> e.Evidence)
                      Alternatives = [] }
                  RulesEvaluated = decision.Evaluations
                  SemanticObjectIds = [ "ItemTransition" ] }

            let! newState =
                events
                |> List.fold evolve state
                |> Result.ofOption(DomainError.invariant "Item Transition state must exist after reinstatement")

            return
                { NewState = newState
                  Events = events
                  Trace = Some trace }
    }

/// AB-C-008: Retire Item Transition.
let retire
    (policy: ItemTransitionPolicy)
    (cmd: RetireItemTransitionCmd)
    (state: ItemTransition option)
    : Result<Decision<ItemTransition, ItemTransitionEvent>, DomainError> =
    result {
        let input: RetireInput = { Cmd = cmd; CurrentState = state }

        let! (decision: DecisionOutcome<RetirementOutcome>) = Decisions.evaluateRetirement Rules.retirementRules input

        match decision.Outcome with
        | RejectRetirement reasons ->
            return!
                Error(
                    DomainError.rule(
                        (String.concat "; " reasons),
                        Medhavi.Core.ArsIdentifiers.Capabilities.manageItemTransitions.Id
                    )
                )

        | RetireTransition ->
            let events = [ ItemTransitionRetired(cmd.TransitionId, cmd.RetirementTime) ]

            let trace: DecisionTrace =
                { DecisionId = System.Guid.NewGuid().ToString()
                  CapabilityId = Medhavi.Core.ArsIdentifiers.Capabilities.manageItemTransitions.Id
                  CausalDecisionIds = []
                  Outcome = "Retired"
                  PolicyId = Some policy.PolicyId
                  PolicyVersion = Some policy.Version
                  Rationale =
                    { Summary = sprintf "Retired Item Transition %A" (Identities.transitionIdValue cmd.TransitionId)
                      Evidence = decision.Evaluations |> List.collect(fun e -> e.Evidence)
                      Alternatives = [] }
                  RulesEvaluated = decision.Evaluations
                  SemanticObjectIds = [ "ItemTransition" ] }

            let! newState =
                events
                |> List.fold evolve state
                |> Result.ofOption(DomainError.invariant "Item Transition state must exist after retirement")

            return
                { NewState = newState
                  Events = events
                  Trace = Some trace }
    }

/// Unified decide.
let decide
    (policy: ItemTransitionPolicy)
    (cmd: ItemTransitionCmd)
    (state: ItemTransition option)
    : Result<Decision<ItemTransition, ItemTransitionEvent>, DomainError> =
    match cmd with
    | Recognize c -> recognize policy c state
    | Suspend c -> suspend policy c state
    | Reinstate c -> reinstate policy c state
    | Retire c -> retire policy c state
