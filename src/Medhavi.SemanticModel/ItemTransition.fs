namespace Medhavi.SemanticModel

open System

type TransitionId = private TransitionId of string

module TransitionId =
    let create (id: string) = Invariants.createStringId TransitionId "TransitionId" id
    let value (TransitionId id) = id

/// SE-C-040 Item Transition Lifecycle State
type ItemTransitionLifecycleState =
    | Active
    | Inactive
    | Retired

module ItemTransitionLifecycleState =
    let validateTransition
        (fromState: ItemTransitionLifecycleState)
        (toState: ItemTransitionLifecycleState)
        : Result<unit, SemanticValidationError> =
        match fromState, toState with
        | Active, Inactive
        | Inactive, Active
        | Active, Retired
        | Inactive, Retired -> Ok()
        | _ -> Error(InvalidLifecycleTransition(sprintf "%A -> %A is not a permitted lifecycle transition." fromState toState))

/// SE-C-040 Item Transition
type ItemTransition =
    { TransitionIdentifier: TransitionId
      SupersededItem: ItemId
      SupersedingItem: ItemId
      TransitionType: VocabularyEntryId
      EffectiveDate: Timestamp
      EndDate: Timestamp option
      LifecycleState: ItemTransitionLifecycleState }

module ItemTransition =
    let validate (transition: ItemTransition) : Result<unit, SemanticValidationError> =
        let distinctItemsCheck =
            if transition.SupersededItem = transition.SupersedingItem then
                Error(
                    InvariantViolation(
                        "ItemTransition",
                        "Superseded Item and Superseding Item must reference distinct items."
                    )
                )
            else
                Ok()

        let effectiveDateCheck =
            if Timestamp.value transition.EffectiveDate = DateTimeOffset.MinValue then
                Error(InvariantViolation("ItemTransition", "Effective Date must be a valid UTC timestamp."))
            else
                Ok()

        let endDateCheck =
            match transition.EndDate with
            | Some endDate ->
                if Timestamp.isBefore endDate transition.EffectiveDate then
                    Error(InvariantViolation("ItemTransition", "End Date must be after Effective Date."))
                else
                    Ok()
            | None -> Ok()

        Invariants.firstError [ distinctItemsCheck; effectiveDateCheck; endDateCheck ]
