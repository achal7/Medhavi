namespace Medhavi.SemanticModel

/// SE-C-005 Resource Group
type ResourceGroup =
    { ResourceGroupIdentifier: ResourceGroupId
      ResourceGroupName: string
      LifecycleState: ReferenceLifecycleState }

/// SE-C-006 Standard Resource
type StandardResource =
    { StandardResourceIdentifier: StandardResourceId
      StandardResourceName: string
      ResourceGroup: ResourceGroupId
      DefaultCapacity: Capacity option
      LifecycleState: ReferenceLifecycleState }

/// SE-C-007 Physical Resource
type PhysicalResource =
    { PhysicalResourceIdentifier: PhysicalResourceId
      StandardResource: StandardResourceId
      Location: LocationId
      LifecycleState: ReferenceLifecycleState }

/// SE-C-008 Transportation Lane
type TransportationLane =
    { LaneIdentifier: TransportationLaneId
      Origin: LocationId
      Destination: LocationId
      TransitDuration: Duration
      LaneCapacity: Capacity option
      LifecycleState: ReferenceLifecycleState }

/// SE-C-009 Network
type Network =
    { NetworkIdentifier: NetworkId
      NetworkName: string
      TransportationLanes: TransportationLaneId list
      LifecycleState: ReferenceLifecycleState }
