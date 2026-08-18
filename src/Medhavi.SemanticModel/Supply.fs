namespace Medhavi.SemanticModel

type SupplyId = private SupplyId of string

module SupplyId =
    let create (id: string) = Invariants.createStringId SupplyId "SupplyId" id
    let value (SupplyId id) = id

/// Lifecycle states for Supply
type SupplyLifecycleState =
    | Available
    | Consumed
    | Withdrawn
    | Expired

module SupplyLifecycleState =
    let validateTransition
        (fromState: SupplyLifecycleState)
        (toState: SupplyLifecycleState)
        : Result<unit, SemanticValidationError> =
        match fromState, toState with
        | Available, Consumed
        | Available, Withdrawn
        | Available, Expired -> Ok()
        | _ -> Error(InvalidLifecycleTransition(sprintf "%A -> %A is not a permitted lifecycle transition." fromState toState))

/// SE-C-014 Supply
type Supply =
    { SupplyIdentifier: SupplyId
      Item: ItemId
      Location: LocationId
      Quantity: Quantity
      AvailabilityWindow: TemporalWindow
      Provenance: VocabularyEntryId
      LifecycleState: SupplyLifecycleState }

module Supply =
    let validate (supply: Supply) : Result<unit, SemanticValidationError> =
        Invariants.firstError
            [ Invariants.nonEmptyIdentifier "SupplyId" (SupplyId.value supply.SupplyIdentifier)
              Quantity.nonNegativeQuantity "Supply.Quantity" supply.Quantity
              TemporalWindow.validateTemporalWindow supply.AvailabilityWindow
              Invariants.nonEmptyIdentifier "Supply.Provenance" (VocabularyEntryId.value supply.Provenance) ]
