namespace Medhavi.Contracts.MasterData.Routing

open System
open System.Threading.Tasks
open Medhavi.Contracts

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

type RoutingApi =
    { Define: RoutingDefineReq -> Task<Result<Routing, ApiError>>
      DefineBulk: RoutingDefineReq list -> Task<Result<Routing list, ApiError>>
      Activate: RoutingActivateReq -> Task<Result<Routing, ApiError>>
      Deactivate: RoutingDeactivateReq -> Task<Result<Routing, ApiError>> }

type RoutingQueryService = QueryService<Routing, string>
