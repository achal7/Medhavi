namespace Medhavi.SemanticModel

type CalendarId = private CalendarId of string

module CalendarId =
    let create (id: string) = Invariants.createStringId CalendarId "CalendarId" id
    let value (CalendarId id) = id

type CalendarDefinitionId = private CalendarDefinitionId of string

module CalendarDefinitionId =
    let create (id: string) = Invariants.createStringId CalendarDefinitionId "CalendarDefinitionId" id
    let value (CalendarDefinitionId id) = id

/// Adoption states for Calendar
type CalendarAdoptionState =
    | Active
    | Superseded
    | Retired

module CalendarAdoptionState =
    let validateTransition
        (fromState: CalendarAdoptionState)
        (toState: CalendarAdoptionState)
        : Result<unit, SemanticValidationError> =
        match fromState, toState with
        | CalendarAdoptionState.Active, CalendarAdoptionState.Superseded
        | CalendarAdoptionState.Active, CalendarAdoptionState.Retired
        | CalendarAdoptionState.Superseded, CalendarAdoptionState.Retired -> Ok()
        | _ -> Error(InvalidLifecycleTransition(sprintf "%A -> %A is not a permitted lifecycle transition." fromState toState))

/// SE-C-033 Calendar
type Calendar =
    { CalendarIdentifier: CalendarId
      CalendarName: string
      TimeZone: TimeZoneId
      CalendarDefinitionReference: string
      VersionNumber: int
      AdoptionState: CalendarAdoptionState }

module Calendar =
    let validate (calendar: Calendar) : Result<unit, SemanticValidationError> =
        Invariants.firstError
            [ Invariants.nonEmptyIdentifier "CalendarId" (CalendarId.value calendar.CalendarIdentifier)
              Invariants.nonEmptyField "Calendar" "CalendarName" calendar.CalendarName
              Invariants.nonEmptyField "Calendar" "CalendarDefinitionReference" calendar.CalendarDefinitionReference
              Invariants.nonNegativeInt "Calendar" "VersionNumber" calendar.VersionNumber ]
