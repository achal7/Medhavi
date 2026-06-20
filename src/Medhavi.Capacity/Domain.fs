namespace Medhavi.Capacity

type CapacityResourceKind =
    | Machine
    | WorkCenter
    | LaborPool
    | Tool
    | Utility
    | Berth
    | Conveyor
    | RailTrack
    | TruckFleet
    | VesselClass

type LoadTarget =
    | Resource of string * CapacityResourceKind
    | WorkCenter of string * CapacityResourceKind

type CapacityLoadBasis =
    | PerOrder
    | PerUnit
    | PerBatch
    | PerTonne
    | PerPallet
    | PerContainer

type CapacityRoutingLoad =
    { Target : LoadTarget
      LoadBasis : CapacityLoadBasis
      UnitsRequired : decimal
      SetupLoadMinutes : decimal option
      RunLoadPerBaseQuantityMinutes : decimal
      TeardownLoadMinutes : decimal option
      CostPerMinute : decimal option }

type RoutingStepLoadProfile =
    { RoutingStepId : string
      OperationCode : string
      SequenceNumber : int
      Loads : CapacityRoutingLoad list
      Yield : decimal option
      ReworkStepId : string option
      ReworkRate : decimal option }

type RoutingLoadProfile =
    { RoutingId : string
      ProductId : string
      PreferencePriority : int
      BaseQuantity : decimal
      StepLoads : RoutingStepLoadProfile list }
