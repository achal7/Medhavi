namespace Medhavi.SemanticModel

type LocationId = private LocationId of string

module LocationId =
    let create (id: string) = Invariants.createStringId LocationId "LocationId" id
    let value (LocationId id) = id

/// Lifecycle states for Location (Closed instead of Retired per ESM)
type LocationLifecycleState =
    | Active
    | Inactive
    | Closed

module LocationLifecycleState =
    let validateTransition
        (fromState: LocationLifecycleState)
        (toState: LocationLifecycleState)
        : Result<unit, SemanticValidationError> =
        match fromState, toState with
        | LocationLifecycleState.Active, LocationLifecycleState.Inactive
        | LocationLifecycleState.Active, LocationLifecycleState.Closed
        | LocationLifecycleState.Inactive, LocationLifecycleState.Active
        | LocationLifecycleState.Inactive, LocationLifecycleState.Closed -> Ok()
        | _ -> Error(InvalidLifecycleTransition(sprintf "%A -> %A is not a permitted lifecycle transition." fromState toState))

/// Structural Classifications for Location
type LocationType =
    | Plant
    | DistributionCenter
    | Warehouse
    | Store
    | CustomerSite
    | SupplierSite
    | Port
    | Depot
    | Terminal
    | Other

/// SE-C-002 Location
type Location =
    { LocationIdentifier: LocationId
      LocationName: string
      LocationType: LocationType
      TimeZone: TimeZoneId
      LifecycleState: LocationLifecycleState }

module Location =
    let validate (location: Location) : Result<unit, SemanticValidationError> =
        Invariants.firstError
            [ Invariants.nonEmptyIdentifier "LocationId" (LocationId.value location.LocationIdentifier)
              Invariants.nonEmptyField "Location" "LocationName" location.LocationName ]
