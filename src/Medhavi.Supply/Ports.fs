namespace Medhavi.Supply

open System
open System.Threading.Tasks
open Medhavi.SemanticModel

type ItemExistsPort = ItemId -> Task<bool>
type LocationExistsPort = LocationId -> Task<bool>
type SupplierExistsPort = SupplierId -> Task<bool>
type SourceReliabilityPort = string -> Task<decimal>
type DuplicateCheckPort = string -> Timestamp -> TimeSpan -> Task<bool>
type PlanningScopeExistsPort = PlanningScopeId -> Task<bool>
type ForecastHorizonExistsPort = ForecastHorizonId -> Task<bool>

/// Supply fact referenced by a Published Enterprise Picture version
type SupplyFact =
    { SupplyId: string
      Item: ItemId
      Location: LocationId
      Quantity: Quantity
      SupplyType: SupplyType
      AvailableDate: Timestamp option }

/// The supply facts of the latest Published Enterprise Picture for a Planning Scope
type PictureSupplyFacts =
    { PictureVersion: int64
      SupplyFacts: SupplyFact list }

type GetPictureSupplyFactsPort = PlanningScopeId -> Task<PictureSupplyFacts>
type GetSupplyChangeAssessmentsPort = PlanningScopeId -> Task<string list>

// ---------- BA-S-015 Input Facts ----------

type DemandForecastFact =
    { Item: ItemId
      Location: LocationId
      PeriodBucket: int
      Quantity: Quantity
      NeedDate: Timestamp }

type BomComponentFact =
    { ParentItem: ItemId
      ComponentItem: ItemId
      UsageQuantity: decimal
      ScrapFactor: decimal
      ProductionLeadTimeDays: int }

type SourcingAllocationFact =
    { Item: ItemId
      Supplier: SupplierId
      AllocationRatio: decimal }

type ResourceCapacityFact =
    { Resource: PhysicalResourceId
      PeriodBucket: int
      MaxPlannedCapacityHours: decimal }

type GetDemandForecastPort = PlanningScopeId -> ForecastHorizonId -> Task<DemandForecastFact list>
type GetBomComponentsPort = ItemId -> Task<BomComponentFact list>
type GetResourceCapacityPort = PhysicalResourceId -> int -> Task<decimal>
type GetSourcingAllocationsPort = ItemId -> Task<SourcingAllocationFact list>
type GetLeadTimeDaysPort = ItemId -> Task<int>
type GetSafetyStockBufferPort = ItemId -> LocationId -> Task<decimal>
type EvaluationPeriodExistsPort = EvaluationPeriodId -> Task<bool>

// ---------- Inventory Domain Facts & Ports ----------

type ItemLocationInventoryFacts =
    { OnHand: decimal
      Allocated: decimal
      InTransit: decimal }

type DemandStatisticsFact =
    { AverageDemand: decimal
      DemandStdDev: decimal
      ObservationCount: int }

type LeadTimeStatisticsFact =
    { LeadTimeDays: int
      LeadTimeStdDevDays: decimal
      IsSingleSource: bool
      SupplierReliability: decimal }

type ItemValuationFact =
    { UnitCost: decimal
      ShelfLifeDays: int option
      RemainingLifetimeDemand: decimal option }

type GetItemLocationInventoryPort = ItemId -> LocationId -> Task<ItemLocationInventoryFacts>
type GetDemandStatisticsPort = ItemId -> LocationId -> Task<DemandStatisticsFact>
type GetLeadTimeStatisticsPort = ItemId -> LocationId -> Task<LeadTimeStatisticsFact>
type GetItemValuationPort = ItemId -> Task<ItemValuationFact>
type GetScopeItemLocationsPort = PlanningScopeId -> Task<(ItemId * LocationId) list>

// ---------- Capacity Domain Facts & Ports ----------

type WorkShiftFact =
    { ShiftId: string
      StartTime: Timestamp
      EndTime: Timestamp }

type OutageFact =
    { OutageId: string
      StartTime: Timestamp
      EndTime: Timestamp
      Reason: string }

type ResourceCapacityFacts =
    { ResourceId: PhysicalResourceId
      RatePerHour: decimal
      Shifts: WorkShiftFact list
      Outages: OutageFact list
      HasAlternateResource: bool
      IsUniqueCapability: bool }

type ResourceLoadFact =
    { ResourceId: PhysicalResourceId
      CurrentCommittedHours: decimal
      ProjectedPlannedHours: decimal
      DownstreamDependentJobsCount: int }

type GetResourceCapacityFactsPort = PhysicalResourceId -> Timestamp -> Timestamp -> Task<ResourceCapacityFacts>
type GetResourceLoadFactsPort = PhysicalResourceId -> Timestamp -> Timestamp -> Task<ResourceLoadFact>
type GetScopeResourcesPort = PlanningScopeId -> Task<PhysicalResourceId list>

// ---------- CA-S-005 Collaborate with Suppliers Ports ----------

type SupplierTelemetryFact =
    { HistoricalOTIF: decimal option
      HistoricalLeadTimeStdDevDays: decimal option
      MeanPromiseDateSlippageDays: decimal option
      QualifiedAlternateSuppliersCount: int
      CategoryTotalSpend: decimal
      ContractedVolumeSurgeTolerance: decimal
      QualityAcceptanceRate: decimal option
      RevisionsInsideFrozenWindow: int
      AverageAcknowledgementHours: decimal
      AverageCommitmentHours: decimal
      DormancyDays: decimal }

type SupplierOpenPortfolioFact =
    { OpenCommitmentsCount: int
      OpenCommitmentsQuantity: decimal
      EstimatedPortfolioValue: decimal }

type GetSupplierTelemetryPort = SupplierId -> Task<SupplierTelemetryFact option>
type GetSupplierPortfolioPort = SupplierId -> Task<SupplierOpenPortfolioFact option>
type GetScopeSuppliersPort = PlanningScopeId -> Task<SupplierId list>

// ---------- Procurement Domain Facts & Ports ----------

type ItemPurchasingParametersFact =
    { MOQ: decimal
      PackagingMultiple: decimal
      PurchasingLeadTimeDays: decimal
      InboundTransitLeadTimeDays: decimal
      QualityInspectionBufferDays: decimal
      GracePeriodDays: decimal
      UnitStandardCost: decimal }

type SupplierCandidateFact =
    { SupplierId: SupplierId
      IsApproved: bool
      IsCompliant: bool
      AvailableCapacity: decimal
      PurchasingLeadTimeDays: decimal
      InboundTransitLeadTimeDays: decimal }

type GetItemPurchasingParametersPort = ItemId -> LocationId -> Task<ItemPurchasingParametersFact option>
type GetCandidateSuppliersPort = ItemId -> LocationId -> Task<SupplierCandidateFact list>
type GetActiveDisruptionSignalsPort = ItemId -> LocationId -> Task<string list>
type GetFutureNetGapsPort = ItemId -> LocationId -> Timestamp -> TimeSpan -> Task<decimal>

// ---------- Scheduling Domain Facts & Ports (CA-S-007) ----------

type PlannedProductionRequirementFact =
    { RequirementId: string
      ItemId: ItemId
      Quantity: decimal
      DueDate: Timestamp
      MaterialAvailableDate: Timestamp
      ProductionPriority: int }

type ResourceAvailabilityFact =
    { ResourceId: PhysicalResourceId
      AvailableWorkingShifts: (Timestamp * Timestamp) list
      MaintenanceWindows: (Timestamp * Timestamp) list
      NominalRunRateUnitsPerHour: decimal
      BaseCapacityHours: decimal
      MaxOvertimeHours: decimal }

type ChangeoverMatrixFact =
    { Matrix: Map<ItemId * ItemId, decimal>
      DefaultChangeoverHours: decimal }

/// Decoupled read-only fact representing an active production schedule for baseline composition
type ActiveScheduleFact =
    { ScheduleId: string
      ResourceId: PhysicalResourceId
      PlanningPeriodId: PlanningPeriodId
      VersionNumber: int
      Feasibility: ScheduleFeasibility
      StabilityScore: decimal
      TotalScheduledHours: decimal
      TotalChangeoverHours: decimal
      ResourceUtilization: decimal
      OvertimeHours: decimal
      IsOvertimeDependent: bool
      IsBottleneck: bool
      HasMaterialRisk: bool
      HasDelayRisk: bool
      ActivityCount: int }

type GetPlannedProductionRequirementsPort = PhysicalResourceId -> PlanningPeriodId -> Task<PlannedProductionRequirementFact list>
type GetResourceAvailabilityFactsPort = PhysicalResourceId -> PlanningPeriodId -> Task<ResourceAvailabilityFact option>
type GetChangeoverMatrixPort = PhysicalResourceId -> Task<ChangeoverMatrixFact>
type GetActiveSchedulesForScopePort = PlanningScopeId -> PlanningPeriodId -> Task<ActiveScheduleFact list>
type GetPriorActiveScheduleFactPort = PhysicalResourceId -> PlanningPeriodId -> ScheduleType -> Task<ActiveScheduleFact option>

// ---------- Distribution Domain Facts & Ports (CA-S-008) ----------

type CandidateLaneFact =
    { LaneId: TransportationLaneId
      SourceLocation: LocationId
      DestinationLocation: LocationId
      TransitTimeHours: decimal
      DailyCapacity: decimal
      AllocatedCapacity: decimal }

type TransferRequirementFact =
    { RequirementId: DistributionRequirementId
      Item: ItemId
      DestinationLocation: LocationId
      RequiredQuantity: Quantity
      RequiredArrivalDate: Timestamp
      DeploymentPriority: string
      IsExpedited: bool }

type DockCapacityFact =
    { Location: LocationId
      CapacityUnits: decimal
      AllocatedUnits: decimal }

type DistributionRequirementExistsPort = DistributionRequirementId -> Task<bool>
type GetTransferRequirementPort = DistributionRequirementId -> Task<TransferRequirementFact option>
type GetCandidateTransportationLanesPort = ItemId -> LocationId -> Task<CandidateLaneFact list>
type GetSourceInventoryPositionsPort = ItemId -> LocationId list -> Task<Map<LocationId, decimal>>
type GetDockCapacityPort = LocationId -> Timestamp -> Task<DockCapacityFact option>
type IsSupplyPlanPublishedPort = PlanningScopeId -> Task<bool>

/// Decoupled read-only fact representing an active distribution recommendation for baseline composition (AB-S-071)
type ActiveDistributionRecommendationFact =
    { RequirementId: DistributionRequirementId
      VersionNumber: int
      ItemId: ItemId
      DestinationLocation: LocationId
      RequiredQuantity: Quantity
      RequiredArrivalDate: Timestamp
      SourceLocations: (LocationId * Quantity * Timestamp) list
      LanesUsed: (TransportationLaneId * decimal * decimal) list
      DepartureTiming: Timestamp
      ExpectedArrivalTiming: Timestamp
      Strategy: DistributionStrategy
      AlternativeStrategy: DistributionStrategy option
      Feasibility: DistributionFeasibility
      Confidence: DistributionConfidence
      StabilityScore: decimal
      StabilityChangeCount: int
      HasFeasibilityViolations: bool
      HasFeasibilityConditions: bool }

type GetActiveDistributionRecommendationsForScopePort = PlanningScopeId -> Task<ActiveDistributionRecommendationFact list>

// ---------- Sense Supply Changes Facts & Ports (CA-S-009) ----------

type BaselineSupplyFact =
    { EntityReference: string
      CommittedDeliveryDate: Timestamp
      CommittedQuantity: decimal
      IsCriticalPath: bool
      BufferDays: decimal }

type GetBaselineSupplyFactPort = string -> Task<BaselineSupplyFact option>
type GetCorroboratingSignalsCountPort = string -> TimeSpan -> Task<int>

// ---------- Evaluate Supply Quality Facts & Ports (CA-S-010) ----------

type ObservedMetricFact =
    { MetricName: string
      ActualValue: decimal
      ObjectiveTarget: decimal
      StretchTarget: decimal option
      MinimumAcceptable: decimal option
      CriticalThreshold: decimal option
      IsEffectiveness: bool
      PriorPeriodValue: decimal option }

type DimensionalOutcomeFact =
    { DimensionName: string
      IsMandatory: bool
      Metrics: ObservedMetricFact list
      EvidenceCompleteness: decimal
      EvidenceTimeliness: decimal
      SourceConsistency: decimal
      SourceReliability: decimal
      EvidenceReferences: string list }

type QualityEvaluationContextFact =
    { IsForceMajeure: bool
      StrategicInvestmentAnnotation: string option
      ContextNotes: string option }

type ScopePrerequisitesFact =
    { HasPublishedSupplyPlan: bool
      HasInventoryHealthAssessment: bool
      HasCapacityHealthAssessment: bool
      HasSupplierCollaborationHealthAssessment: bool
      IsEvaluationPeriodComplete: bool }

type GetScopePrerequisitesPort = PlanningScopeId -> PlanningHorizon -> EvaluationPeriodId -> Task<ScopePrerequisitesFact>
type GetDimensionalOutcomesForScopePort = PlanningScopeId -> PlanningHorizon -> EvaluationPeriodId -> Task<DimensionalOutcomeFact list>
type GetPriorPublishedQualityAssessmentPort = PlanningScopeId -> PlanningHorizon -> EvaluationPeriodId -> Task<string option>
type GetQualityEvaluationContextPort = PlanningScopeId -> EvaluationPeriodId -> Task<QualityEvaluationContextFact option>

// ---------- Exception Domain Facts & Ports (CA-S-011) ----------

type SupplyChangeEvidenceFact =
    { AssessmentId: string
      EntityType: string
      EntityId: string
      ChangeType: string
      EpistemicState: string
      Materiality: string
      DaysVariance: decimal
      QuantityVariance: decimal
      Confidence: decimal
      OccurrenceTime: Timestamp
      StoppageConfirmed: bool }

type QualityEvidenceFact =
    { AssessmentId: string
      PlanningScopeId: PlanningScopeId
      OverallClassification: string
      OverallScore: decimal
      OverallConfidence: string
      ConsecutiveBreachCount: int
      LowestServiceLevel: decimal }

type OperationalPositionEvidenceFact =
    { EntityType: string
      EntityId: string
      OnHand: decimal
      SafetyStock: decimal
      DaysOfSupply: decimal
      ResourceUtilization: decimal
      MaxPlannedCapacity: decimal
      IsBottleneck: bool
      SupplierOtifPercent: decimal
      ConsecutiveUnderperformanceMonths: int
      IsCriticalEntity: bool }

type ActiveExceptionFact =
    { ExceptionId: string
      GovernedPolicyReference: string
      ViolatedRuleId: string
      ExceptionType: string
      ScopeEntityType: string
      ScopeEntityId: string
      LifecycleState: string
      BusinessImpact: string
      Urgency: string }

type GetScopeExceptionPrerequisitesPort = PlanningScopeId -> Task<bool>
type GetSupplyChangeEvidencePort = PlanningScopeId -> Task<SupplyChangeEvidenceFact list>
type GetQualityEvidencePort = PlanningScopeId -> Task<QualityEvidenceFact option>
type GetOperationalEvidencePort = PlanningScopeId -> Task<OperationalPositionEvidenceFact list>
type GetActiveExceptionsForScopePort = PlanningScopeId -> Task<ActiveExceptionFact list>

/// Clean Supply Domain Query Ports (Zero Notification Ports; all events dispatched via Envelope Store)
type SupplyPorts =
    { ItemExists: ItemExistsPort
      LocationExists: LocationExistsPort
      SupplierExists: SupplierExistsPort
      SourceReliability: SourceReliabilityPort
      IsDuplicate: DuplicateCheckPort
      PlanningScopeExists: PlanningScopeExistsPort
      ForecastHorizonExists: ForecastHorizonExistsPort
      EvaluationPeriodExists: EvaluationPeriodExistsPort
      GetPictureSupplyFacts: GetPictureSupplyFactsPort
      GetSupplyChangeAssessments: GetSupplyChangeAssessmentsPort
      GetDemandForecast: GetDemandForecastPort
      GetBomComponents: GetBomComponentsPort
      GetResourceCapacity: GetResourceCapacityPort
      GetSourcingAllocations: GetSourcingAllocationsPort
      GetLeadTimeDays: GetLeadTimeDaysPort
      GetSafetyStockBuffer: GetSafetyStockBufferPort
      GetItemLocationInventory: GetItemLocationInventoryPort
      GetDemandStatistics: GetDemandStatisticsPort
      GetLeadTimeStatistics: GetLeadTimeStatisticsPort
      GetItemValuation: GetItemValuationPort
      GetScopeItemLocations: GetScopeItemLocationsPort
      GetResourceCapacityFacts: GetResourceCapacityFactsPort
      GetResourceLoadFacts: GetResourceLoadFactsPort
      GetScopeResources: GetScopeResourcesPort
      GetSupplierTelemetry: GetSupplierTelemetryPort
      GetSupplierPortfolio: GetSupplierPortfolioPort
      GetScopeSuppliers: GetScopeSuppliersPort
      GetItemPurchasingParameters: GetItemPurchasingParametersPort
      GetCandidateSuppliers: GetCandidateSuppliersPort
      GetActiveDisruptionSignals: GetActiveDisruptionSignalsPort
      GetFutureNetGaps: GetFutureNetGapsPort
      GetPlannedProductionRequirements: GetPlannedProductionRequirementsPort
      GetResourceAvailabilityFacts: GetResourceAvailabilityFactsPort
      GetChangeoverMatrix: GetChangeoverMatrixPort
      GetActiveSchedulesForScope: GetActiveSchedulesForScopePort
      GetPriorActiveScheduleFact: GetPriorActiveScheduleFactPort
      DistributionRequirementExists: DistributionRequirementExistsPort
      GetTransferRequirement: GetTransferRequirementPort
      GetCandidateTransportationLanes: GetCandidateTransportationLanesPort
      GetSourceInventoryPositions: GetSourceInventoryPositionsPort
      GetDockCapacity: GetDockCapacityPort
      IsSupplyPlanPublished: IsSupplyPlanPublishedPort
      GetActiveDistributionRecommendationsForScope: GetActiveDistributionRecommendationsForScopePort
      GetBaselineSupplyFact: GetBaselineSupplyFactPort
      GetCorroboratingSignalsCount: GetCorroboratingSignalsCountPort
      GetScopePrerequisites: GetScopePrerequisitesPort
      GetDimensionalOutcomesForScope: GetDimensionalOutcomesForScopePort
      GetPriorPublishedQualityAssessment: GetPriorPublishedQualityAssessmentPort
      GetQualityEvaluationContext: GetQualityEvaluationContextPort
      GetScopeExceptionPrerequisites: GetScopeExceptionPrerequisitesPort
      GetSupplyChangeEvidence: GetSupplyChangeEvidencePort
      GetQualityEvidence: GetQualityEvidencePort
      GetOperationalEvidence: GetOperationalEvidencePort
      GetActiveExceptionsForScope: GetActiveExceptionsForScopePort }



