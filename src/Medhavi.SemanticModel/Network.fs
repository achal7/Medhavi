namespace Medhavi.SemanticModel

type NetworkId = private NetworkId of string

module NetworkId =
    let create (id: string) = Invariants.createStringId NetworkId "NetworkId" id
    let value (NetworkId id) = id

/// SE-C-009 Network
type Network =
    { NetworkIdentifier: NetworkId
      NetworkName: string
      ParticipatingLocations: LocationId list
      TransportationLanes: TransportationLaneId list
      LifecycleState: ReferenceLifecycleState }

module Network =
    let validate (network: Network) : Result<unit, SemanticValidationError> =
        let duplicateLaneCheck =
            if Invariants.hasDuplicatesBy id network.TransportationLanes then
                Error(DuplicateValue("Network", "TransportationLanes"))
            else
                Ok()

        Invariants.firstError
            [ Invariants.nonEmptyIdentifier "NetworkId" (NetworkId.value network.NetworkIdentifier)
              Invariants.nonEmptyField "Network" "NetworkName" network.NetworkName
              duplicateLaneCheck ]
