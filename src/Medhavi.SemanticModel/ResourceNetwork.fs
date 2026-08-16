namespace Medhavi.SemanticModel

/// SE-C-005 Resource Group
type ResourceGroup =
    { ResourceGroupIdentifier: ResourceGroupId
      ResourceGroupName: string
      ResourceGroupType: VocabularyEntryId
      Calendar: CalendarId
      LifecycleState: ReferenceLifecycleState }

/// SE-C-006 Standard Resource
type StandardResource =
    { StandardResourceIdentifier: StandardResourceId
      StandardResourceName: string
      CapabilityDescription: string
      ResourceType: VocabularyEntryId
      ReferenceCapacity: Capacity
      Calendar: CalendarId
      LifecycleState: ReferenceLifecycleState }

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

/// SE-C-008 Transportation Lane
type TransportationLane =
    { LaneIdentifier: TransportationLaneId
      LaneName: string option
      Origin: LocationId
      Destination: LocationId
      LifecycleState: ReferenceLifecycleState }

/// SE-C-009 Network
type Network =
    { NetworkIdentifier: NetworkId
      NetworkName: string
      ParticipatingLocations: LocationId list
      TransportationLanes: TransportationLaneId list
      LifecycleState: ReferenceLifecycleState }
