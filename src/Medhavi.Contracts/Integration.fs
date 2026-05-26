namespace Medhavi.Contracts.Integration

open System

type UomDefineReq =
    { Id: string
      Code: string
      Name: string
      IsBase: bool
      ToBaseFactor: decimal
      Created: DateTimeOffset }

type UomStatusChangeReq = { Id: string; NewStatus: bool }

type UomChangeConversionFactorReq =
    { Id: string
      NewFactor: decimal
      IsBase: bool }

type UnitConversionDefineReq =
    { SourceUom: string
      TargetUom: string
      ConversionFactor: decimal
      Created: DateTimeOffset }

type UnitConversionUpdateReq = { Id: string; Ratio: decimal }

type UnitConversionRetireReq = { Id: string }

type PlantDefineReq =
    { Id: string
      Code: string
      Name: string }

type PlantRenameReq =
    { Id: string
      NewName: string }

type PlantRetireReq =
    { Id: string }

type StockingPointDefineReq =
    { Id: string
      PlantId: string
      Code: string
      Name: string
      Type: string // Plant, DistributionCenter, Warehouse
      Location: string option
      Level: int option
      PlanningLevel: int option
      SupplyCanBeSplit: bool }

type StockingPointRenameReq =
    { Id: string
      NewName: string }

type StockingPointRetireReq =
    { Id: string }

type NodeAttributesReq =
    { LocationCode: string option
      PlanningLevel: int option
      StockingPointRef: string option }

type NodeDefineReq =
    { Id: string
      Code: string
      Name: string
      Type: string // Plant, DistributionCenter, etc.
      Attributes: NodeAttributesReq
      Created: DateTimeOffset }

type NodeRetireReq =
    { Id: string }

type SkuDefineReq =
    { Id: string
      Code: string
      Name: string
      Group: string
      Created: DateTimeOffset }

type SkuRenameReq =
    { Id: string
      NewName: string }

type SkuRetireReq =
    { Id: string }

type BomItemReq =
    { ComponentSkuId: string
      Quantity: decimal
      UnitOfMeasureId: string
      Sequence: int }

type BomDefineReq =
    { Id: string
      SkuId: string
      Items: BomItemReq list }

type BomActivateReq =
    { Id: string }

type BomDeactivateReq =
    { Id: string }

type RoutingStepReq =
    { StepId: string
      Sequence: int
      ResourceGroupId: string option
      Yield: decimal option }

type RoutingInputReq =
    { StepId: string
      SkuId: string
      NodeId: string
      ConversionRate: decimal option }

type RoutingOutputReq =
    { StepId: string
      SkuId: string
      NodeId: string
      ConversionRate: decimal option
      IsCoSku: bool }

type StepResourceReq =
    { StepId: string
      ResourceId: string
      IsAllowed: bool
      Sequence: int
      DurationPerUnitMinutes: decimal option }

type RoutingDefineReq =
    { Id: string
      Name: string
      Type: string // Work, Transport, Purchase
      EffectiveStart: DateTimeOffset
      EffectiveEnd: DateTimeOffset option
      Steps: RoutingStepReq list
      Inputs: RoutingInputReq list
      Outputs: RoutingOutputReq list
      StepResources: StepResourceReq list
      Created: DateTimeOffset }

type RoutingActivateReq =
    { Id: string }

type RoutingDeactivateReq =
    { Id: string }

type TransportLegDefineReq =
    { Id: string
      Origin: string
      Destination: string
      Mode: string
      Schedule: string
      LeadTimeMinutes: float
      Capacity: decimal option
      CapacityUnit: string option
      CutoffMinutes: float option
      Constraints: string list
      Reliability: float option
      CO2PerUnit: decimal option
      EffectiveStart: DateTimeOffset
      EffectiveEnd: DateTimeOffset option
      Created: DateTimeOffset }

type TransportLegUpdateReq =
    { Id: string
      Mode: string option
      Schedule: string option
      LeadTimeMinutes: float option
      Capacity: decimal option
      CapacityUnit: string option
      CutoffMinutes: float option
      Constraints: string list option
      Reliability: float option
      CO2PerUnit: decimal option
      EffectiveEnd: DateTimeOffset option
      Modified: DateTimeOffset }

type TransportLegDeactivateReq =
    { Id: string
      DeactivatedAt: DateTimeOffset }
