namespace Medhavi.SemanticModel

/// Lifecycle states for Reference Objects (Item, Customer, Supplier, Resources, etc.)
type ReferenceLifecycleState =
    | Active
    | Inactive
    | Retired

module ReferenceLifecycle =
    let validateTransition
        (fromState: ReferenceLifecycleState)
        (toState: ReferenceLifecycleState)
        : Result<unit, SemanticValidationError> =
        match fromState, toState with
        | ReferenceLifecycleState.Active, ReferenceLifecycleState.Inactive
        | ReferenceLifecycleState.Active, ReferenceLifecycleState.Retired
        | ReferenceLifecycleState.Inactive, ReferenceLifecycleState.Active
        | ReferenceLifecycleState.Inactive, ReferenceLifecycleState.Retired -> Ok()
        | _ -> Error(InvalidLifecycleTransition(sprintf "%A -> %A is not a permitted lifecycle transition." fromState toState))
