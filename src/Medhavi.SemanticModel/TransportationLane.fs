namespace Medhavi.SemanticModel

type TransportationLaneId = private TransportationLaneId of string

module TransportationLaneId =
    let create (id: string) = Invariants.createStringId TransportationLaneId "TransportationLaneId" id
    let value (TransportationLaneId id) = id

/// SE-C-008 Transportation Lane
type TransportationLane =
    { LaneIdentifier: TransportationLaneId
      LaneName: string option
      Origin: LocationId
      Destination: LocationId
      LifecycleState: ReferenceLifecycleState }

module TransportationLane =
    let validate (lane: TransportationLane) : Result<unit, SemanticValidationError> =
        let originDestCheck =
            if lane.Origin = lane.Destination then
                Error(InvariantViolation("TransportationLane", "Origin and Destination cannot be the same location."))
            else
                Ok()

        Invariants.firstError
            [ Invariants.nonEmptyIdentifier "TransportationLaneId" (TransportationLaneId.value lane.LaneIdentifier)
              Invariants.nonEmptyIdentifier "TransportationLane.Origin" (LocationId.value lane.Origin)
              Invariants.nonEmptyIdentifier "TransportationLane.Destination" (LocationId.value lane.Destination)
              originDestCheck ]
