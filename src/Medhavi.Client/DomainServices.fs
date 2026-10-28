namespace Medhavi.Client

open System
open Medhavi.Contracts
open Medhavi.Contracts.Core
open Medhavi.Contracts.Core.Demand
open Medhavi.Contracts.Core.Exception
open Medhavi.Contracts.Core.ItemTransition
open Medhavi.Contracts.Demand
open Medhavi.Contracts.Supply
open Medhavi.Contracts.Traceability

// ============================================================================
// Core Bounded Context Capabilities
// ============================================================================

type DemandManagementCapability =
    { Commands: DemandApi
      Queries: DemandQueries }

type ExceptionManagementCapability =
    { Commands: ExceptionApi
      Queries: ExceptionQueries }

type ItemTransitionManagementCapability =
    { Commands: ItemTransitionApi
      Queries: ItemTransitionQueries }

type EnterprisePictureCapability =
    { Commands: EnterprisePictureApi
      Queries: EnterprisePictureQueries }

type TraceabilityCapability =
    { Queries: TraceabilityQueryService }

type MasterDataCapability =
    { ItemApi: ItemApi option
      LocationApi: LocationApi option
      ItemQueries: ItemQueries option
      LocationQueries: LocationQueries option }

type CoreDomainServices =
    { DemandManagement: DemandManagementCapability
      ExceptionManagement: ExceptionManagementCapability
      ItemTransitionManagement: ItemTransitionManagementCapability
      EnterprisePicture: EnterprisePictureCapability
      Traceability: TraceabilityCapability
      MasterData: MasterDataCapability option }

// ============================================================================
// Demand Bounded Context Capabilities
// ============================================================================

type DemandObservationContext =
    { Commands: DemandObservationApi
      Queries: DemandObservationQueries }

type DemandUnderstandingContext =
    { Commands: DemandUnderstandingApi
      Queries: DemandUnderstandingQueries }

type UnderstandDemandContext =
    { Observation: DemandObservationContext
      Understanding: DemandUnderstandingContext }

type ForecastDemandContext =
    { Commands: ForecastPublicationApi
      Queries: ForecastPublicationQueries }

type SenseDemandContext =
    { Commands: DemandBehaviorAssessmentApi
      Queries: DemandBehaviorAssessmentQueries }

type DetectDemandExceptionsContext =
    { Commands: DemandExceptionApi
      Queries: DemandExceptionQueries }

type SegmentDemandContext =
    { Commands: PlanningClassificationApi
      Queries: PlanningClassificationQueries }

type ClassifyDemandContext =
    { Commands: DemandBehaviorClassificationApi
      Queries: DemandBehaviorClassificationQueries }

type PrioritizeDemandContext =
    { Commands: PlanningPriorityApi
      Queries: PlanningPriorityQueries }

type EvaluateDemandQualityContext =
    { Commands: ForecastQualityApi
      Queries: ForecastQualityQueries }

type ExplainDemandContext =
    { Commands: DemandExplanationApi
      Queries: DemandExplanationQueries }

type LearnFromDemandContext =
    { Commands: DemandLearningApi
      Queries: DemandLearningQueries }

type ModelDemandInterventionsContext =
    { Commands: DemandInterventionApi
      Queries: DemandInterventionQueries }

type DemandDomainServices =
    { UnderstandDemand: UnderstandDemandContext
      ForecastDemand: ForecastDemandContext
      SenseDemand: SenseDemandContext
      DetectDemandExceptions: DetectDemandExceptionsContext
      SegmentDemand: SegmentDemandContext
      ClassifyDemand: ClassifyDemandContext
      PrioritizeDemand: PrioritizeDemandContext
      EvaluateDemandQuality: EvaluateDemandQualityContext
      ExplainDemand: ExplainDemandContext
      LearnFromDemand: LearnFromDemandContext
      ModelDemandInterventions: ModelDemandInterventionsContext }

// ============================================================================
// Supply Bounded Context Capabilities
// ============================================================================

type UnderstandSupplyCapability =
    { SupplyUnderstandingApi: SupplyUnderstandingApi
      SupplyDataRecordApi: SupplyDataRecordApi
      SupplyUnderstandingQueries: SupplyUnderstandingQueries
      SupplyDataRecordQueries: SupplyDataRecordQueries }

type PlanSupplyCapability =
    { SupplyPlanApi: SupplyPlanApi
      SupplyPlanQueries: SupplyPlanQueries }

type ManageInventoryCapability =
    { ManageInventoryApis: ManageInventoryApis
      InventoryPositionQueries: InventoryPositionQueries
      InventoryPolicyQueries: InventoryPolicyQueries
      InventoryHealthQueries: InventoryHealthQueries }

type ManageCapacityCapability =
    { ManageCapacityApis: ManageCapacityApis
      CapacityPositionQueries: CapacityPositionQueries
      CapacityHealthQueries: CapacityHealthQueries }

type CollaborateWithSuppliersCapability =
    { CollaborateWithSuppliersApis: CollaborateWithSuppliersApis
      SupplierCommitmentQueries: SupplierCommitmentQueries
      SupplierAssessmentQueries: SupplierCommitmentAssessmentQueries
      SupplierHealthQueries: SupplierCollaborationHealthQueries }

type ProcureMaterialsCapability =
    { ProcureMaterialsApis: ProcureMaterialsApis
      ProcurementRecommendationQueries: ProcurementRecommendationQueries
      ProcurementPlanQueries: ProcurementPlanQueries }

type ScheduleProductionCapability =
    { ScheduleProductionApis: ScheduleProductionApis
      ProductionScheduleQueries: ProductionScheduleQueries
      PublishedScheduleQueries: PublishedProductionScheduleQueries }

type ManageDistributionCapability =
    { ManageDistributionApis: ManageDistributionApis
      DistributionRecommendationQueries: DistributionRecommendationQueries
      DistributionPlanQueries: DistributionPlanQueries }

type SenseSupplyChangesCapability =
    { SupplyChangeApi: SupplyChangeApi
      SupplyChangeQueries: SupplyChangeQueries }

type EvaluateSupplyQualityCapability =
    { SupplyQualityApi: SupplyQualityApi
      SupplyQualityQueries: SupplyQualityQueries }

type DetectSupplyExceptionsCapability =
    { SupplyExceptionApi: SupplyExceptionApi
      SupplyExceptionQueries: SupplyExceptionQueries }

type ExplainSupplyDecisionsCapability =
    { SupplyExplanationApi: SupplyExplanationApi
      SupplyExplanationQueries: SupplyExplanationQueries }

type SupplyLearningCapability =
    { SupplyLearningApi: SupplyLearningApi
      SupplyLearningQueries: SupplyLearningQueries }

type SupplyDomainServices =
    { UnderstandSupply: UnderstandSupplyCapability
      PlanSupply: PlanSupplyCapability
      ManageInventory: ManageInventoryCapability
      ManageCapacity: ManageCapacityCapability
      CollaborateWithSuppliers: CollaborateWithSuppliersCapability
      ProcureMaterials: ProcureMaterialsCapability
      ScheduleProduction: ScheduleProductionCapability
      ManageDistribution: ManageDistributionCapability
      SenseSupplyChanges: SenseSupplyChangesCapability
      EvaluateSupplyQuality: EvaluateSupplyQualityCapability
      DetectSupplyExceptions: DetectSupplyExceptionsCapability
      ExplainSupplyDecisions: ExplainSupplyDecisionsCapability
      LearnFromSupply: SupplyLearningCapability }

// ============================================================================
// Platform Composite Gateway
// ============================================================================

type DomainServices =
    { Core: CoreDomainServices
      Demand: DemandDomainServices
      Supply: SupplyDomainServices }
