namespace Medhavi.SemanticModel

type ResourceGroupId = private ResourceGroupId of string

module ResourceGroupId =
    let create (id: string) = Invariants.createStringId ResourceGroupId "ResourceGroupId" id
    let value (ResourceGroupId id) = id

/// SE-C-005 Resource Group
type ResourceGroup =
    { ResourceGroupIdentifier: ResourceGroupId
      ResourceGroupName: string
      ResourceGroupType: VocabularyEntryId
      Calendar: CalendarId
      LifecycleState: ReferenceLifecycleState }

module ResourceGroup =
    let validate (resourceGroup: ResourceGroup) : Result<unit, SemanticValidationError> =
        Invariants.firstError
            [ Invariants.nonEmptyIdentifier "ResourceGroupId" (ResourceGroupId.value resourceGroup.ResourceGroupIdentifier)
              Invariants.nonEmptyField "ResourceGroup" "ResourceGroupName" resourceGroup.ResourceGroupName
              Invariants.nonEmptyIdentifier "ResourceGroup.Calendar" (CalendarId.value resourceGroup.Calendar) ]
