module Medhavi.SemanticModel.Lifecycles

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

/// Scenario lifecycle.
let validateScenarioTransition
    (fromState: ScenarioLifecycleState)
    (toState: ScenarioLifecycleState)
    : Result<unit, SemanticValidationError> =
    match fromState, toState with
    | ScenarioLifecycleState.Draft, ScenarioLifecycleState.Active
    | ScenarioLifecycleState.Draft, ScenarioLifecycleState.Archived
    | ScenarioLifecycleState.Active, ScenarioLifecycleState.Archived -> Ok()
    | _ -> invalidTransition fromState toState

/// Plan lifecycle.
let validatePlanTransition
    (fromState: PlanLifecycleState)
    (toState: PlanLifecycleState)
    : Result<unit, SemanticValidationError> =
    match fromState, toState with
    | PlanLifecycleState.Draft, PlanLifecycleState.Approved
    | PlanLifecycleState.Draft, PlanLifecycleState.Archived
    | PlanLifecycleState.Approved, PlanLifecycleState.Superseded
    | PlanLifecycleState.Approved, PlanLifecycleState.Archived -> Ok()
    | _ -> invalidTransition fromState toState

/// BOM lifecycle.
let validateBomTransition
    (fromState: BomLifecycleState)
    (toState: BomLifecycleState)
    : Result<unit, SemanticValidationError> =
    match fromState, toState with
    | BomLifecycleState.Draft, BomLifecycleState.Active
    | BomLifecycleState.Draft, BomLifecycleState.Archived
    | BomLifecycleState.Active, BomLifecycleState.Superseded
    | BomLifecycleState.Active, BomLifecycleState.Archived -> Ok()
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
    | ExceptionLifecycleState.Active, Resolved -> Ok()
    | _ -> invalidTransition fromState toState

/// Enterprise Picture version lifecycle.
let validatePictureVersionTransition
    (fromState: PictureVersionLifecycleState)
    (toState: PictureVersionLifecycleState)
    : Result<unit, SemanticValidationError> =
    match fromState, toState with
    | Draft, Published
    | Published, PictureVersionLifecycleState.Superseded -> Ok()
    | _ -> invalidTransition fromState toState

/// Risk lifecycle.
let validateRiskTransition
    (fromState: RiskLifecycleState)
    (toState: RiskLifecycleState)
    : Result<unit, SemanticValidationError> =
    match fromState, toState with
    | RiskLifecycleState.Active, RiskLifecycleState.Retired -> Ok()
    | _ -> invalidTransition fromState toState

/// Calendar adoption lifecycle.
let validateCalendarTransition
    (fromState: CalendarAdoptionState)
    (toState: CalendarAdoptionState)
    : Result<unit, SemanticValidationError> =
    match fromState, toState with
    | CalendarAdoptionState.Active, CalendarAdoptionState.Superseded
    | CalendarAdoptionState.Active, CalendarAdoptionState.Retired
    | CalendarAdoptionState.Superseded, CalendarAdoptionState.Retired -> Ok()
    | _ -> invalidTransition fromState toState

/// Governed Catalog lifecycle.
let validateGovernedCatalogTransition
    (fromState: GovernedCatalogState)
    (toState: GovernedCatalogState)
    : Result<unit, SemanticValidationError> =
    match fromState, toState with
    | GovernedCatalogState.Active, GovernedCatalogState.Deprecated
    | GovernedCatalogState.Active, GovernedCatalogState.Retired
    | GovernedCatalogState.Deprecated, GovernedCatalogState.Retired -> Ok()
    | _ -> invalidTransition fromState toState

/// Adoption state lifecycle.
let validateAdoptionTransition
    (fromState: AdoptionState)
    (toState: AdoptionState)
    : Result<unit, SemanticValidationError> =
    match fromState, toState with
    | AdoptionState.Admitted, AdoptionState.Deprecated
    | AdoptionState.Admitted, AdoptionState.Retired
    | AdoptionState.Deprecated, AdoptionState.Retired -> Ok()
    | _ -> invalidTransition fromState toState

/// SE-C-040 Item Transition lifecycle transitions.
/// Enterprise-Governed Master Data: Active ↔ Inactive; Active → Retired; Inactive → Retired.
let validateItemTransitionTransition
    (fromState: ItemTransitionLifecycleState)
    (toState: ItemTransitionLifecycleState)
    : Result<unit, SemanticValidationError> =

    match fromState, toState with
    | Active, Inactive -> Ok()
    | Inactive, Active -> Ok()
    | Active, Retired -> Ok()
    | Inactive, Retired -> Ok()
    | _ -> invalidTransition fromState toState
