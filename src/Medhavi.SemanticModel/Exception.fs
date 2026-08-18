namespace Medhavi.SemanticModel

type ExceptionId = private ExceptionId of string

module ExceptionId =
    let create (id: string) = Invariants.createStringId ExceptionId "ExceptionId" id
    let value (ExceptionId id) = id

/// Lifecycle states for Exception
type ExceptionLifecycleState =
    | Active
    | Resolved

module ExceptionLifecycleState =
    let validateTransition
        (fromState: ExceptionLifecycleState)
        (toState: ExceptionLifecycleState)
        : Result<unit, SemanticValidationError> =
        match fromState, toState with
        | ExceptionLifecycleState.Active, Resolved -> Ok()
        | _ -> Error(InvalidLifecycleTransition(sprintf "%A -> %A is not a permitted lifecycle transition." fromState toState))

/// SE-C-019 Exception
type Exception =
    { ExceptionIdentifier: ExceptionId
      ConstraintReference: string
      Classification: VocabularyEntryId
      AffectedScopeType: VocabularyEntryId
      AffectedScopeIdentifier: string
      EvidenceReference: string option
      LifecycleState: ExceptionLifecycleState }

module Exception =
    let validate (exceptionObject: Exception) : Result<unit, SemanticValidationError> =
        Invariants.firstError
            [ Invariants.nonEmptyIdentifier "ExceptionId" (ExceptionId.value exceptionObject.ExceptionIdentifier)
              Invariants.nonEmptyField "Exception" "ConstraintReference" exceptionObject.ConstraintReference
              Invariants.nonEmptyIdentifier "Exception.Classification" (VocabularyEntryId.value exceptionObject.Classification)
              Invariants.nonEmptyIdentifier "Exception.AffectedScopeType" (VocabularyEntryId.value exceptionObject.AffectedScopeType)
              Invariants.nonEmptyField "Exception" "AffectedScopeIdentifier" exceptionObject.AffectedScopeIdentifier ]
