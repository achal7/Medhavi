namespace Medhavi.Contracts.Integration

open System
open Medhavi.Contracts.Domain

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

type PlantRenameReq = { Id: string; NewName: string }

type PlantRetireReq = { Id: string }

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

type StockingPointRenameReq = { Id: string; NewName: string }

type StockingPointRetireReq = { Id: string }

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

type NodeRetireReq = { Id: string }

type SkuDefineReq =
    { Id: string
      Code: string
      Name: string
      Group: string
      Created: DateTimeOffset }

type SkuRenameReq = { Id: string; NewName: string }

type SkuRetireReq = { Id: string }

type BomItemReq =
    { ComponentSkuId: string
      Quantity: decimal
      UnitOfMeasureId: string
      Sequence: int }

type BomDefineReq =
    { Id: string
      SkuId: string
      Items: BomItemReq list }

type BomActivateReq = { Id: string }

type BomDeactivateReq = { Id: string }

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

type RoutingActivateReq = { Id: string }

type RoutingDeactivateReq = { Id: string }

type TransportLegDefineReq =
    { Id: string
      Origin: string
      Destination: string
      Mode: string
      Schedule: string
      LeadTimeMinutes: decimal
      Capacity: decimal option
      CapacityUnit: string option
      CutoffMinutes: decimal option
      Constraints: string list
      Reliability: decimal option
      CO2PerUnit: decimal option
      EffectiveStart: DateTimeOffset
      EffectiveEnd: DateTimeOffset option
      Created: DateTimeOffset }

type TransportLegUpdateReq =
    { Id: string
      Mode: string option
      Schedule: string option
      LeadTimeMinutes: decimal option
      Capacity: decimal option
      CapacityUnit: string option
      CutoffMinutes: decimal option
      Constraints: string list option
      Reliability: decimal option
      CO2PerUnit: decimal option
      EffectiveEnd: DateTimeOffset option
      Modified: DateTimeOffset }

type TransportLegDeactivateReq =
    { Id: string
      DeactivatedAt: DateTimeOffset }

type InventoryDefineReq =
    { Id: string
      SkuId: string
      StockingPointId: string
      Quantity: decimal
      UnitOfMeasure: string }

type InventoryTargetDefineReq =
    { SkuId: string
      StockingPointId: string
      ReplenishmentPolicy: ReplenishmentPolicy option
      SafetyStockQty: decimal option
      MinQty: decimal option
      MaxQty: decimal option
      TargetServiceLevel: decimal option
      CoverDays: decimal option
      SeasonalAdjustments: SeasonalAdjustment list
      EffectiveStart: DateTimeOffset option
      EffectiveEnd: DateTimeOffset option
      IsActive: bool }

type InventoryTargetUpdateReq =
    { Id: string
      SkuId: string
      StockingPointId: string
      ReplenishmentPolicy: ReplenishmentPolicy option
      SafetyStockQty: decimal option
      MinQty: decimal option
      MaxQty: decimal option
      TargetServiceLevel: decimal option
      CoverDays: decimal option
      SeasonalAdjustments: SeasonalAdjustment list option
      EffectiveStart: DateTimeOffset option
      EffectiveEnd: DateTimeOffset option }

type PriceTierReq =
    { TierNumber: int
      MinQuantity: decimal
      MaxQuantity: decimal option
      PricePerUnit: decimal
      Currency: string }

type CapacityWindowReq =
    { WindowId: string
      StartDate: DateTimeOffset
      EndDate: DateTimeOffset
      MaxQuantity: decimal
      AvailableQuantity: decimal }

type SupplierOfferDefineReq =
    { Id: string
      SupplierId: string
      SkuId: string
      StockingPointId: string option
      Moq: decimal option
      LotSize: decimal option
      LeadTimeP50Minutes: decimal option
      LeadTimeP95Minutes: decimal option
      PriceTiers: PriceTierReq list
      Reliability: decimal option
      Incoterm: string option
      CapacityWindows: CapacityWindowReq list
      CreatedDate: DateTimeOffset }

type SupplierOfferUpdateReq =
    { Id: string
      Moq: decimal option
      LotSize: decimal option
      LeadTimeP50Minutes: decimal option
      LeadTimeP95Minutes: decimal option
      PriceTiers: PriceTierReq list option
      Reliability: decimal option
      Incoterm: string option
      CapacityWindows: CapacityWindowReq list option
      ModifiedDate: DateTimeOffset }

type SupplierOfferChangeStatusReq =
    { Id: string
      IsActive: bool
      ModifiedDate: DateTimeOffset }

type SupplyOrderCreateReq =
    { Id: string
      OrderType: string
      SkuId: string
      StockingPointId: string
      Quantity: decimal
      UnitOfMeasure: string
      RoutingId: string option
      SupplierId: string option
      IsFirm: bool
      IsExpedited: bool
      IsLocked: bool
      UsesLeadTimeQuantity: bool
      RequiredDeliveryDate: DateTimeOffset option
      CreatedDate: DateTimeOffset }

type SupplyOrderStartReq =
    { Id: string
      StartedDate: DateTimeOffset }

type SupplyOrderPartialCompleteReq =
    { Id: string
      CompletedQuantity: decimal
      CompletedDate: DateTimeOffset }

type SupplyOrderCompleteReq =
    { Id: string
      CompletedDate: DateTimeOffset }

type SupplyOrderPlanReq =
    { Id: string
      PlannedDeliveryDate: DateTimeOffset }

type SupplyOrderConfirmReq =
    { Id: string
      ConfirmedDate: DateTimeOffset }

type SupplyOrderReleaseReq =
    { Id: string
      ReleasedDate: DateTimeOffset }

type SupplyOrderCancelReq =
    { Id: string
      CancelledDate: DateTimeOffset }

type SupplyOrderLockReq =
    { Id: string
      Locked: bool
      ModifiedDate: DateTimeOffset }

type MaterialReservationCreateReq =
    { Id: string
      IdempotencyKey: string
      SkuId: string
      StockingPointId: string
      Quantity: decimal
      RequiredDate: DateTimeOffset
      ExpiryTime: DateTimeOffset }

type MaterialReservationConfirmReq = { Id: string }
type MaterialReservationReleaseReq = { Id: string }
type MaterialReservationReduceReq = { Id: string; NewQuantity: decimal }
type MaterialReservationExpireReq = { Id: string }