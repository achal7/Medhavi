namespace Medhavi.SemanticModel

/// Pure lifecycle transition validators.
/// These validators define permitted semantic state transitions.
/// They do not execute transitions. Execution belongs to domain Aggregate Behaviors.
module Lifecycles =

    let private invalidTransition (fromState: obj) (toState: obj) : Result<unit, SemanticValidationError> =
        Error(InvalidLifecycleTransition(sprintf "%A -> %A is not a permitted lifecycle transition." fromState toState))

    /// Reference object lifecycle: Item, Customer, Supplier.
    let validateReferenceTransition
        (fromState: ReferenceLifecycleState)
        (toState: ReferenceLifecycleState)
        : Result<unit, SemanticValidationError> =

        match fromState, toState with
        | ReferenceLifecycleState.Active, ReferenceLifecycleState.Inactive
        | ReferenceLifecycleState.Active, ReferenceLifecycleState.Retired
        | ReferenceLifecycleState.Inactive, ReferenceLifecycleState.Active
        | ReferenceLifecycleState.Inactive, ReferenceLifecycleState.Retired -> Ok()
        | _ -> invalidTransition fromState toState

    /// Location lifecycle.
    let validateLocationTransition
        (fromState: LocationLifecycleState)
        (toState: LocationLifecycleState)
        : Result<unit, SemanticValidationError> =

        match fromState, toState with
        | LocationLifecycleState.Active, LocationLifecycleState.Inactive
        | LocationLifecycleState.Active, LocationLifecycleState.Closed
        | LocationLifecycleState.Inactive, LocationLifecycleState.Active
        | LocationLifecycleState.Inactive, LocationLifecycleState.Closed -> Ok()
        | _ -> invalidTransition fromState toState

    /// Planning object lifecycle: Scenario, Plan.
    let validatePlanningTransition
        (fromState: PlanningLifecycleState)
        (toState: PlanningLifecycleState)
        : Result<unit, SemanticValidationError> =

        match fromState, toState with
        | PlanningLifecycleState.Draft, PlanningLifecycleState.Active
        | PlanningLifecycleState.Draft, PlanningLifecycleState.Archived
        | PlanningLifecycleState.Active, PlanningLifecycleState.Archived
        | PlanningLifecycleState.Active, PlanningLifecycleState.Superseded -> Ok()
        | _ -> invalidTransition fromState toState

    /// Demand lifecycle.
    let validateDemandTransition
        (fromState: DemandLifecycleState)
        (toState: DemandLifecycleState)
        : Result<unit, SemanticValidationError> =

        match fromState, toState with
        | DemandLifecycleState.Active, DemandLifecycleState.Satisfied
        | DemandLifecycleState.Active, DemandLifecycleState.Cancelled -> Ok()
        | _ -> invalidTransition fromState toState

    /// Supply lifecycle.
    let validateSupplyTransition
        (fromState: SupplyLifecycleState)
        (toState: SupplyLifecycleState)
        : Result<unit, SemanticValidationError> =

        match fromState, toState with
        | Available, Consumed
        | Available, Withdrawn
        | Available, Expired -> Ok()
        | _ -> invalidTransition fromState toState

    /// Commitment lifecycle.
    let validateCommitmentTransition
        (fromState: CommitmentLifecycleState)
        (toState: CommitmentLifecycleState)
        : Result<unit, SemanticValidationError> =

        match fromState, toState with
        | Committed, Fulfilled
        | Committed, Cancelled -> Ok()
        | _ -> invalidTransition fromState toState

    /// Exception lifecycle.
    let validateExceptionTransition
        (fromState: ExceptionLifecycleState)
        (toState: ExceptionLifecycleState)
        : Result<unit, SemanticValidationError> =

        match fromState, toState with
        | Active, Resolved -> Ok()
        | _ -> invalidTransition fromState toState

    /// Enterprise Picture version lifecycle.
    let validatePictureVersionTransition
        (fromState: PictureVersionLifecycleState)
        (toState: PictureVersionLifecycleState)
        : Result<unit, SemanticValidationError> =

        match fromState, toState with
        | Draft, Published
        | Published, Superseded -> Ok()
        | _ -> invalidTransition fromState toState

    /// Risk lifecycle.
    let validateRiskTransition
        (fromState: RiskLifecycleState)
        (toState: RiskLifecycleState)
        : Result<unit, SemanticValidationError> =

        match fromState, toState with
        | Identified, Assessed
        | Identified, Closed
        | Identified, Retired
        | Assessed, Mitigating
        | Assessed, Closed
        | Assessed, Retired
        | Mitigating, Closed
        | Mitigating, Retired -> Ok()
        | _ -> invalidTransition fromState toState

    /// Governed vocabulary adoption lifecycle.
    let validateAdoptionTransition
        (fromState: AdoptionState)
        (toState: AdoptionState)
        : Result<unit, SemanticValidationError> =

        match fromState, toState with
        | AdoptionState.Admitted, AdoptionState.Deprecated
        | AdoptionState.Admitted, AdoptionState.Retired
        | AdoptionState.Deprecated, AdoptionState.Retired -> Ok()
        | _ -> invalidTransition fromState toState
