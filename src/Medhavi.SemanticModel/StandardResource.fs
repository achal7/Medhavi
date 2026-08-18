namespace Medhavi.SemanticModel

type StandardResourceId = private StandardResourceId of string

module StandardResourceId =
    let create (id: string) = Invariants.createStringId StandardResourceId "StandardResourceId" id
    let value (StandardResourceId id) = id

/// SE-C-006 Standard Resource
type StandardResource =
    { StandardResourceIdentifier: StandardResourceId
      StandardResourceName: string
      CapabilityDescription: string
      ResourceType: VocabularyEntryId
      ReferenceCapacity: Capacity
      Calendar: CalendarId
      LifecycleState: ReferenceLifecycleState }

module StandardResource =
    let validate (resource: StandardResource) : Result<unit, SemanticValidationError> =
        Invariants.firstError
            [ Invariants.nonEmptyIdentifier "StandardResourceId" (StandardResourceId.value resource.StandardResourceIdentifier)
              Invariants.nonEmptyField "StandardResource" "StandardResourceName" resource.StandardResourceName
              Capacity.validateCapacity resource.ReferenceCapacity
              Invariants.nonEmptyIdentifier "StandardResource.Calendar" (CalendarId.value resource.Calendar) ]
