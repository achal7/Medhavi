namespace Medhavi.SemanticModel

type DemandId = private DemandId of string

module DemandId =
    let create (id: string) = Invariants.createStringId DemandId "DemandId" id
    let value (DemandId id) = id

/// Lifecycle states for Demand
type DemandLifecycleState =
    | Active
    | Satisfied
    | Cancelled

module DemandLifecycleState =
    let validateTransition
        (fromState: DemandLifecycleState)
        (toState: DemandLifecycleState)
        : Result<unit, SemanticValidationError> =
        match fromState, toState with
        | DemandLifecycleState.Active, DemandLifecycleState.Satisfied
        | DemandLifecycleState.Active, DemandLifecycleState.Cancelled -> Ok()
        | _ -> Error(InvalidLifecycleTransition(sprintf "%A -> %A is not a permitted lifecycle transition." fromState toState))

type DemandOrigin =
    | CustomerOrder
    | Forecast
    | ProductionRequirement
    | Transfer
    | Other

/// SE-C-013 Demand
type Demand =
    { DemandIdentifier: DemandId
      Item: ItemId
      Location: LocationId
      Customer: CustomerId option
      Quantity: Quantity
      NeedWindow: NeedWindow
      DemandOrigin: DemandOrigin
      ParentDemand: DemandId option
      LifecycleState: DemandLifecycleState }

module Demand =
    let validate (demand: Demand) : Result<unit, SemanticValidationError> =
        Invariants.firstError
            [ Invariants.nonEmptyIdentifier "DemandId" (DemandId.value demand.DemandIdentifier)
              Quantity.positiveQuantity "Demand.Quantity" demand.Quantity
              NeedWindow.validateNeedWindow demand.NeedWindow ]
