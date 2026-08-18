namespace Medhavi.SemanticModel

type RiskId = private RiskId of string

module RiskId =
    let create (id: string) = Invariants.createStringId RiskId "RiskId" id
    let value (RiskId id) = id

/// Lifecycle states for Risk objects.
type RiskLifecycleState =
    | Active
    | Retired

module RiskLifecycleState =
    let validateTransition
        (fromState: RiskLifecycleState)
        (toState: RiskLifecycleState)
        : Result<unit, SemanticValidationError> =
        match fromState, toState with
        | RiskLifecycleState.Active, RiskLifecycleState.Retired -> Ok()
        | _ -> Error(InvalidLifecycleTransition(sprintf "%A -> %A is not a permitted lifecycle transition." fromState toState))

/// SE-C-020 Risk
type Risk =
    { RiskIdentifier: RiskId
      RiskType: VocabularyEntryId
      RiskSubjectType: VocabularyEntryId
      RiskSubjectIdentifier: string
      Assessments: RiskAssessment list
      LifecycleState: RiskLifecycleState }

module Risk =
    let validate (risk: Risk) : Result<unit, SemanticValidationError> =
        Invariants.firstError
            [ Invariants.nonEmptyIdentifier "RiskId" (RiskId.value risk.RiskIdentifier)
              Invariants.nonEmptyIdentifier "Risk.RiskType" (VocabularyEntryId.value risk.RiskType)
              Invariants.nonEmptyIdentifier "Risk.RiskSubjectType" (VocabularyEntryId.value risk.RiskSubjectType)
              Invariants.nonEmptyField "Risk" "RiskSubjectIdentifier" risk.RiskSubjectIdentifier ]
