namespace Medhavi.SemanticModel

type PhysicalResourceId = private PhysicalResourceId of string

module PhysicalResourceId =
    let create (id: string) = Invariants.createStringId PhysicalResourceId "PhysicalResourceId" id
    let value (PhysicalResourceId id) = id

/// SE-C-007 Physical Resource
type PhysicalResource =
    { PhysicalResourceIdentifier: PhysicalResourceId
      PhysicalResourceName: string
      Location: LocationId
      ResourceGroup: ResourceGroupId option
      StandardResource: StandardResourceId option
      Calendar: CalendarId
      AssignedCapacity: Capacity
      LifecycleState: ReferenceLifecycleState }

module PhysicalResource =
    let validate (resource: PhysicalResource) : Result<unit, SemanticValidationError> =
        Invariants.firstError
            [ Invariants.nonEmptyIdentifier "PhysicalResourceId" (PhysicalResourceId.value resource.PhysicalResourceIdentifier)
              Invariants.nonEmptyField "PhysicalResource" "PhysicalResourceName" resource.PhysicalResourceName
              Invariants.nonEmptyIdentifier "PhysicalResource.Location" (LocationId.value resource.Location)
              Capacity.validateCapacity resource.AssignedCapacity
              Invariants.nonEmptyIdentifier "PhysicalResource.Calendar" (CalendarId.value resource.Calendar) ]
