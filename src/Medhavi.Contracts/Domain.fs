namespace Medhavi.Contracts.Domain

open System

type UnitOfMeasure =
    { Id: string
      Code: string
      Name: string
      Status: bool
      ConversionFactor: decimal
      IsBase: bool }

type UnitConversion =
    { Id: string
      ProductId: string option
      FromUnitCode: string
      ToUnitCode: string
      Ratio: decimal
      Status: bool }

type Plant =
    { Id: string
      Code: string
      Name: string
      Status: bool }

type StockingPoint =
    { Id: string
      PlantId: string
      Code: string
      Name: string
      Type: string
      Status: bool }

type Sku =
    { Id: string
      Code: string
      Name: string
      Group: string
      Status: bool }

type BomItem =
    { ComponentSkuId: string
      Quantity: decimal
      Sequence: int }

type Bom =
    { Id: string
      SkuId: string
      Items: BomItem list
      Status: bool }

type ResourceGroup =
    { Id: string
      PlantId: string option
      Name: string
      Description: string option
      DefaultCalendarId: string option
      IsActive: bool
      Created: DateTimeOffset
      Modified: DateTimeOffset }

type StandardResource =
    { Id: string
      ResourceGroupId: string
      Name: string
      Description: string option
      DefaultEfficiency: decimal
      DefaultCostRateAmount: decimal option
      DefaultCostRateCurrency: string option
      IsActive: bool
      Created: DateTimeOffset
      Modified: DateTimeOffset }

type PhysicalResource =
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
      Created: DateTimeOffset
      Modified: DateTimeOffset }

type PromiseRequest =
    { OrderId: string
      SkuId: string
      NodeId: string
      Quantity: decimal
      RequestedDate: DateTimeOffset }

type PromiseResponse =
    { OrderId: string
      SkuId: string
      PromiseDate: DateTimeOffset
      IsFeasible: bool
      LimiterReason: string }

type RoutingPreference = { Priority: int; IsPreferred: bool }

type RoutingQuantityRule =
    { MinQuantity: decimal option
      MaxQuantity: decimal option
      LotSize: decimal option
      OrderMultiple: decimal option }

type RoutingCostPolicy =
    | NoRoutingCost
    | FixedCost of decimal
    | CostPerUnit of decimal
    | EstimatedCost of decimal

type RoutingApplicability =
    { StockingPointId: string option
      EffectiveStart: DateTimeOffset
      EffectiveEnd: DateTimeOffset option }

type RoutingStepKind =
    | Standard
    | Alternate
    | Parallel
    | Rework
    | External

type StepOverlapPolicy =
    | NoOverlap
    | OverlapAfterQuantity of decimal
    | OverlapAfterDuration of decimal

type StepInputTiming =
    | AtStepStart
    | AtStepEnd
    | OffsetBeforeStepStart of decimal
    | OffsetAfterStepStart of decimal
    | OffsetBeforeStepEnd of decimal
    | OffsetAfterStepEnd of decimal

type StepOutputTiming =
    | AtStepStart
    | AtStepEnd
    | OffsetAfterStepStart of decimal
    | OffsetAfterStepEnd of decimal

type RoutingOutputRole =
    | PrimaryOutput
    | CoProduct
    | ByProduct
    | Scrap
    | Waste

type RoutingStepInput =
    { SkuId: string
      FromNodeId: string option
      QuantityPerBaseOutput: decimal option
      Timing: StepInputTiming
      IsConsumed: bool
      IsOptional: bool }

type RoutingStepOutput =
    { SkuId: string
      ToNodeId: string option
      QuantityRatioToPrimaryOutput: decimal option
      Role: RoutingOutputRole
      Timing: StepOutputTiming }

type RoutingResourceKind =
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

type ResourceLoadBasis =
    | PerOrder
    | PerUnit
    | PerBatch
    | PerTonne
    | PerPallet
    | PerContainer

type ResourceUsage =
    | Primary
    | Alternate
    | Optional
    | Parallel
    | Rework

type ResourceSelectionRule =
    | AnyAllowed
    | PreferPrimary
    | PreferLowestCost
    | PreferFastest
    | PreferPriorityOrder
    | FixedResourceGroup of string

type SetupPolicy =
    | NoSetup
    | FixedSetup of decimal

type CoolingPolicy =
    | NoCooling
    | FixedCooling of decimal

type ResourceEfficiencyPolicy =
    | StandardEfficiency
    | EfficiencyFactor of decimal

type ResourceTimingProfile =
    { SetupTime: decimal option
      RunTimePerBaseQuantity: decimal option
      TeardownTime: decimal option
      CoolingTime: decimal option
      MinLeadTime: decimal option }

type RoutingResourceOption =
    { OptionId: string
      ResourceGroupId: string
      WorkCenterId: string option
      Usage: ResourceUsage
      Priority: int option
      TimingProfile: ResourceTimingProfile
      SetupPolicy: SetupPolicy
      CoolingPolicy: CoolingPolicy
      CostPerMinute: decimal option
      EfficiencyPolicy: ResourceEfficiencyPolicy
      EffectivePeriodStart: DateTimeOffset option
      EffectivePeriodEnd: DateTimeOffset option }

type RoutingStepResourceRequirement =
    { RequirementId: string
      ResourceKind: RoutingResourceKind
      LoadBasis: ResourceLoadBasis
      RequiredUnits: decimal
      SelectionRule: ResourceSelectionRule
      Options: RoutingResourceOption list }

type StepYieldPolicy =
    | NoYieldLoss
    | ExpectedYield of decimal

type ReworkPolicy =
    | NoRework
    | ReworkToStep of stepId: string * reworkRate: decimal

type StepTimingProfile =
    { FixedLeadTime: decimal option
      QueueTime: decimal option
      WaitTime: decimal option
      MoveTime: decimal option }

type RoutingStep =
    { StepId: string
      Sequence: int
      OperationCode: string
      Name: string
      Description: string option
      Kind: RoutingStepKind
      Inputs: RoutingStepInput list
      Outputs: RoutingStepOutput list
      ResourceRequirements: RoutingStepResourceRequirement list
      TimingProfile: StepTimingProfile
      YieldPolicy: StepYieldPolicy
      ReworkPolicy: ReworkPolicy
      OverlapPolicy: StepOverlapPolicy
      EffectivePeriodStart: DateTimeOffset option
      EffectivePeriodEnd: DateTimeOffset option }

type WorkRoutingDetails =
    { ProductId: string
      PrimaryOutputSkuId: string
      BaseOutputQuantity: decimal
      Steps: RoutingStep list }

type TransportMode =
    | Road
    | Rail
    | Sea
    | Air
    | Pipeline
    | Conveyor
    | InternalTransfer

type TransportResourceOption =
    { OptionId: string
      ResourceGroupId: string option
      CarrierId: string option
      Usage: ResourceUsage
      Priority: int option
      TransitTime: decimal
      LoadingTime: decimal option
      UnloadingTime: decimal option
      CostPerUnit: decimal option
      CostPerTrip: decimal option
      EffectivePeriodStart: DateTimeOffset option
      EffectivePeriodEnd: DateTimeOffset option }

type TransportRoutingDetails =
    { SkuId: string
      FromNodeId: string
      ToNodeId: string
      Mode: TransportMode
      TransitLeadTime: decimal
      LossFactor: decimal option
      ResourceSelectionRule: ResourceSelectionRule
      TransportResourceOptions: TransportResourceOption list }

type SupplierPreference = { Priority: int; IsPreferred: bool }

type PurchasePricingPolicy =
    | NoPurchaseCost
    | PurchaseCostPerUnit of decimal
    | ContractPriceReference of contractId: string

type PurchaseRoutingDetails =
    { SkuId: string
      SupplierId: string
      ReceivingNodeId: string
      SupplierShipFromNodeId: string option
      SupplierLeadTime: decimal
      InspectionLeadTime: decimal option
      PutawayLeadTime: decimal option
      SupplierSkuCode: string option
      SupplierPreference: SupplierPreference
      PurchasePricingPolicy: PurchasePricingPolicy }

type RoutingDetails =
    | Work of WorkRoutingDetails
    | Transport of TransportRoutingDetails
    | Purchase of PurchaseRoutingDetails

type Routing =
    { Id: string
      Name: string
      Description: string option
      Applicability: RoutingApplicability
      Preference: RoutingPreference
      QuantityRule: RoutingQuantityRule
      CostPolicy: RoutingCostPolicy
      Details: RoutingDetails
      Status: bool
      CreatedAt: DateTimeOffset
      ModifiedAt: DateTimeOffset }

type TransportLeg =
    { Id: string
      Origin: string
      Destination: string
      Mode: string
      LeadTimeMinutes: decimal
      Capacity: decimal option
      CapacityUnit: string option
      Status: bool }

type SupplyProposal =
    { Id: string
      ProposalType: string // "PlannedPurchaseOrder" | "PlannedWorkOrder" | "PlannedTransferOrder"
      SkuId: string
      NodeId: string
      StockingPointId: string
      Quantity: decimal
      DueDate: DateTimeOffset
      StartDate: DateTimeOffset option
      RoutingId: string option
      SupplierId: string option
      Priority: int
      IsExpedite: bool
      Status: string // "Planned" | "Firmed" | "Released" | "Cancelled"
      PeggingRefs: string list
      CapacityCheckedDate: DateTimeOffset option
      CreatedAt: DateTimeOffset }
