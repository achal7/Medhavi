namespace Medhavi.SemanticModel

type UnitOfMeasureId = private UnitOfMeasureId of string

module UnitOfMeasureId =
    let create (id: string) = Invariants.createStringId UnitOfMeasureId "UnitOfMeasureId" id
    let value (UnitOfMeasureId id) = id
    let defaultUoM () = UnitOfMeasureId("EA")

/// Adoption states for Unit of Measure and Planning Period
type AdoptionState =
    | Admitted
    | Deprecated
    | Retired

module AdoptionState =
    let validateTransition (fromState: AdoptionState) (toState: AdoptionState) : Result<unit, SemanticValidationError> =
        match fromState, toState with
        | AdoptionState.Admitted, AdoptionState.Deprecated
        | AdoptionState.Admitted, AdoptionState.Retired
        | AdoptionState.Deprecated, AdoptionState.Retired -> Ok()
        | _ ->
            Error(
                InvalidLifecycleTransition(
                    sprintf "%A -> %A is not a permitted lifecycle transition." fromState toState
                )
            )

/// SE-C-032 Unit of Measure
type UnitOfMeasure =
    { UnitIdentifier: UnitOfMeasureId
      UnitName: string
      UnitClassification: string
      AdoptionState: AdoptionState }

module UnitOfMeasure =
    let validate (unit: UnitOfMeasure) : Result<unit, SemanticValidationError> =
        Invariants.firstError
            [ Invariants.nonEmptyIdentifier "UnitOfMeasureId" (UnitOfMeasureId.value unit.UnitIdentifier)
              Invariants.nonEmptyField "UnitOfMeasure" "UnitName" unit.UnitName
              Invariants.nonEmptyField "UnitOfMeasure" "UnitClassification" unit.UnitClassification ]
