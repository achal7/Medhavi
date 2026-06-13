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

type StepInputTimingReq =
    | AtStepStart
    | AtStepEnd
    | OffsetBeforeStepStart of decimal
    | OffsetAfterStepStart of decimal
    | OffsetBeforeStepEnd of decimal
    | OffsetAfterStepEnd of decimal

type StepOutputTimingReq =
    | AtStepStart
    | AtStepEnd
    | OffsetAfterStepStart of decimal
    | OffsetAfterStepEnd of decimal

type RoutingOutputRoleReq =
    | PrimaryOutput
    | CoProduct
    | ByProduct
    | Scrap
    | Waste

type RoutingStepInputReq =
    { SkuId: string
      FromNodeId: string option
      QuantityPerBaseOutput: decimal option
      Timing: StepInputTimingReq
      IsConsumed: bool
      IsOptional: bool }

type RoutingStepOutputReq =
    { SkuId: string
      ToNodeId: string option
      QuantityRatioToPrimaryOutput: decimal option
      Role: RoutingOutputRoleReq
      Timing: StepOutputTimingReq }

type RoutingResourceOptionReq =
    { OptionId: string
      ResourceGroupId: string
      ResourceId: string option
      WorkCenterId: string option
      Usage: string // "Primary", "Alternate", "Optional", "Parallel", "Rework"
      Priority: int option
      SetupTimeMinutes: decimal option
      RunTimePerBaseQuantityMinutes: decimal option
      TeardownTimeMinutes: decimal option
      CoolingTimeMinutes: decimal option
      MinLeadTimeMinutes: decimal option
      CostPerMinute: decimal option
      EfficiencyFactor: decimal option
      SetupTimeFixed: decimal option
      CoolingTimeFixed: decimal option
      EffectiveStart: DateTimeOffset option
      EffectiveEnd: DateTimeOffset option }

type RoutingStepResourceRequirementReq =
    { RequirementId: string
      ResourceKind: string // "Machine", "WorkCenter", etc.
      LoadBasis: string // "PerOrder", "PerUnit", etc.
      RequiredUnits: decimal
      SelectionRule: string // "AnyAllowed", "PreferPrimary", etc.
      SelectionRuleGroupId: string option
      Options: RoutingResourceOptionReq list }

type StepTimingProfileReq =
    { FixedLeadTime: decimal option
      QueueTime: decimal option
      WaitTime: decimal option
      MoveTime: decimal option }

type RoutingStepReq =
    { StepId: string
      Sequence: int
      OperationCode: string
      Name: string
      Description: string option
      Kind: string // "Standard", "Alternate", etc.
      Inputs: RoutingStepInputReq list
      Outputs: RoutingStepOutputReq list
      ResourceRequirements: RoutingStepResourceRequirementReq list
      TimingProfile: StepTimingProfileReq
      YieldPercentage: decimal option
      ReworkStepId: string option
      ReworkRate: decimal option
      OverlapPolicyType: string // "NoOverlap", "OverlapAfterQuantity", "OverlapAfterDuration"
      OverlapPolicyValue: decimal option
      EffectiveStart: DateTimeOffset option
      EffectiveEnd: DateTimeOffset option }

type WorkRoutingDetailsReq =
    { ProductId: string
      PrimaryOutputSkuId: string
      BaseOutputQuantity: decimal
      Steps: RoutingStepReq list }

type TransportResourceOptionReq =
    { OptionId: string
      ResourceGroupId: string option
      ResourceId: string option
      CarrierId: string option
      Usage: string
      Priority: int option
      TransitTime: decimal
      LoadingTime: decimal option
      UnloadingTime: decimal option
      CostPerUnit: decimal option
      CostPerTrip: decimal option
      EffectiveStart: DateTimeOffset option
      EffectiveEnd: DateTimeOffset option }

type TransportRoutingDetailsReq =
    { SkuId: string
      FromNodeId: string
      ToNodeId: string
      Mode: string // "Road", "Rail", etc.
      TransitLeadTime: decimal
      LossFactor: decimal option
      ResourceSelectionRule: string
      TransportResourceOptions: TransportResourceOptionReq list }

type PurchaseRoutingDetailsReq =
    { SkuId: string
      SupplierId: string
      ReceivingNodeId: string
      SupplierShipFromNodeId: string option
      SupplierLeadTime: decimal
      InspectionLeadTime: decimal option
      PutawayLeadTime: decimal option
      SupplierSkuCode: string option
      SupplierPriority: int
      SupplierIsPreferred: bool
      PricingPolicyType: string // "NoPurchaseCost", "PurchaseCostPerUnit", "ContractPriceReference"
      PricingPolicyValue: string option }

type RoutingDetailsReq =
    | WorkDetails of WorkRoutingDetailsReq
    | TransportDetails of TransportRoutingDetailsReq
    | PurchaseDetails of PurchaseRoutingDetailsReq

type RoutingDefineReq =
    { Id: string
      Name: string
      Description: string option
      Type: string // "Work", "Transport", "Purchase"
      StockingPointId: string option
      EffectiveStart: DateTimeOffset
      EffectiveEnd: DateTimeOffset option
      PreferencePriority: int
      IsPreferred: bool
      MinQuantity: decimal option
      MaxQuantity: decimal option
      LotSize: decimal option
      OrderMultiple: decimal option
      CostPolicyType: string // "NoRoutingCost", "FixedCost", "CostPerUnit", "EstimatedCost"
      CostPolicyValue: decimal option
      Details: RoutingDetailsReq
      Created: DateTimeOffset }

type RoutingActivateReq = { Id: string }

type RoutingDeactivateReq = { Id: string }

type ResourceGroupDefineReq =
    { Id: string
      PlantId: string option
      Name: string
      Description: string option
      DefaultCalendarId: string option
      IsActive: bool
      Created: DateTimeOffset }

type StandardResourceDefineReq =
    { Id: string
      ResourceGroupId: string
      Name: string
      Description: string option
      DefaultEfficiency: decimal
      DefaultCostRateAmount: decimal option
      DefaultCostRateCurrency: string option
      IsActive: bool
      Created: DateTimeOffset }

type PhysicalResourceDefineReq =
    { Id: string
      StandardResourceId: string
      Name: string
      SerialNumber: string option
      Location: string option
      EfficiencyOverride: decimal option
      CostRateOverrideAmount: decimal option
      CostRateOverrideCurrency: string option
      CalendarId: string option
      IsActive: bool
      Created: DateTimeOffset }

type ResourceGroupRenameReq = { Id: string; NewName: string }
type ResourceGroupRetireReq = { Id: string }

type StandardResourceRenameReq = { Id: string; NewName: string }
type StandardResourceRetireReq = { Id: string }

type PhysicalResourceRenameReq = { Id: string; NewName: string }
type PhysicalResourceRetireReq = { Id: string }

type KpiInvalidationEvent =
    | MrpRunCompleted of plantId: string * stockingPointId: string * runId: string * periodsCovered: DateOnly list
    | OptimizerRunCompleted of plantId: string * scenarioId: string option * periodsCovered: DateOnly list
    | CapacityCalendarChanged of resourceGroupId: string * plantId: string * affectedDates: DateOnly list
    | InventoryAdjusted of stockingPointId: string * skuId: string * asOfDate: DateOnly
    | ShipmentStatusChanged of legId: string * shipmentId: string * date: DateOnly
    | SafetyStockPolicyChanged of skuId: string * stockingPointId: string
    | WorkOrderStatusChanged of workOrderId: string * plantId: string * date: DateOnly
    | DemandFulfilled of demandLineId: string * plantId: string * date: DateOnly
