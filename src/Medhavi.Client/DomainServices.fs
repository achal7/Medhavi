namespace Medhavi.Client

open Medhavi.Contracts.Core
open Medhavi.Contracts.Core.Demand
open Medhavi.Contracts.Core.Exception
open Medhavi.Contracts.Core.ItemTransition
open Medhavi.Contracts.Demand
open Medhavi.Contracts.Supply
open Medhavi.Contracts.Traceability

// Core Bounded Context Capabilities

type DemandManagementCapability =
    {
        Commands: DemandApi
        Queries: DemandQueries
    }

type ExceptionManagementCapability =
    {
        Commands: ExceptionApi
        Queries: ExceptionQueries
    }

type ItemTransitionManagementCapability =
    {
        Commands: ItemTransitionApi
        Queries: ItemTransitionQueries
    }

type EnterprisePictureCapability =
    {
        Commands: EnterprisePictureApi
        Queries: EnterprisePictureQueries
    }

type TraceabilityCapability = { Queries: TraceabilityQueryService }

type MasterDataCapability =
    {
        ItemApi: ItemApi option
        LocationApi: LocationApi option
        ItemQueries: ItemQueries option
        LocationQueries: LocationQueries option
    }

type CoreDomainServices =
    {
        DemandManagement: DemandManagementCapability
        ExceptionManagement: ExceptionManagementCapability
        ItemTransitionManagement: ItemTransitionManagementCapability
        EnterprisePicture: EnterprisePictureCapability
        Traceability: TraceabilityCapability
        MasterData: MasterDataCapability option
    }

// Demand Bounded Context Capabilities

type DemandObservationContext =
    {
        Commands: DemandObservationApi
        Queries: DemandObservationQueries
    }

type DemandUnderstandingContext =
    {
        Commands: DemandUnderstandingApi
        Queries: DemandUnderstandingQueries
    }

type UnderstandDemandContext =
    {
        Observation: DemandObservationContext
        Understanding: DemandUnderstandingContext
    }

type ForecastDemandContext =
    {
        Commands: ForecastPublicationApi
        Queries: ForecastPublicationQueries
    }

type SenseDemandContext =
    {
        Commands: DemandBehaviorAssessmentApi
        Queries: DemandBehaviorAssessmentQueries
    }

type DetectDemandExceptionsContext =
    {
        Commands: DemandExceptionApi
        Queries: DemandExceptionQueries
    }

type SegmentDemandContext =
    {
        Commands: PlanningClassificationApi
        Queries: PlanningClassificationQueries
    }

type ClassifyDemandContext =
    {
        Commands: DemandBehaviorClassificationApi
        Queries: DemandBehaviorClassificationQueries
    }

type PrioritizeDemandContext =
    {
        Commands: PlanningPriorityApi
        Queries: PlanningPriorityQueries
    }

type EvaluateDemandQualityContext =
    {
        Commands: ForecastQualityApi
        Queries: ForecastQualityQueries
    }

type ExplainDemandContext =
    {
        Commands: DemandExplanationApi
        Queries: DemandExplanationQueries
    }

type LearnFromDemandContext =
    {
        Commands: DemandLearningApi
        Queries: DemandLearningQueries
    }

type ModelDemandInterventionsContext =
    {
        Commands: DemandInterventionApi
        Queries: DemandInterventionQueries
    }

type DemandDomainServices =
    {
        UnderstandDemand: UnderstandDemandContext
        ForecastDemand: ForecastDemandContext
        SenseDemand: SenseDemandContext
        DetectDemandExceptions: DetectDemandExceptionsContext
        SegmentDemand: SegmentDemandContext
        ClassifyDemand: ClassifyDemandContext
        PrioritizeDemand: PrioritizeDemandContext
        EvaluateDemandQuality: EvaluateDemandQualityContext
        ExplainDemand: ExplainDemandContext
        LearnFromDemand: LearnFromDemandContext
        ModelDemandInterventions: ModelDemandInterventionsContext
    }

// Supply Bounded Context Capabilities

type UnderstandSupplyCapability =
    {
        SupplyUnderstandingApi: SupplyUnderstandingApi
        SupplyDataRecordApi: SupplyDataRecordApi
        SupplyUnderstandingQueries: SupplyUnderstandingQueries
        SupplyDataRecordQueries: SupplyDataRecordQueries
    }

type PlanSupplyCapability =
    {
        SupplyPlanApi: SupplyPlanApi
        SupplyPlanQueries: SupplyPlanQueries
    }

type ManageInventoryCapability =
    {
        ManageInventoryApis: ManageInventoryApis
        InventoryPositionQueries: InventoryPositionQueries
        InventoryPolicyQueries: InventoryPolicyQueries
        InventoryHealthQueries: InventoryHealthQueries
    }

type ManageCapacityCapability =
    {
        ManageCapacityApis: ManageCapacityApis
        CapacityPositionQueries: CapacityPositionQueries
        CapacityHealthQueries: CapacityHealthQueries
    }

type CollaborateWithSuppliersCapability =
    {
        CollaborateWithSuppliersApis: CollaborateWithSuppliersApis
        SupplierCommitmentQueries: SupplierCommitmentQueries
        SupplierAssessmentQueries: SupplierCommitmentAssessmentQueries
        SupplierHealthQueries: SupplierCollaborationHealthQueries
    }

type ProcureMaterialsCapability =
    {
        ProcureMaterialsApis: ProcureMaterialsApis
        ProcurementRecommendationQueries: ProcurementRecommendationQueries
        ProcurementPlanQueries: ProcurementPlanQueries
    }

type ScheduleProductionCapability =
    {
        ScheduleProductionApis: ScheduleProductionApis
        ProductionScheduleQueries: ProductionScheduleQueries
        PublishedScheduleQueries: PublishedProductionScheduleQueries
    }

type ManageDistributionCapability =
    {
        ManageDistributionApis: ManageDistributionApis
        DistributionRecommendationQueries: DistributionRecommendationQueries
        DistributionPlanQueries: DistributionPlanQueries
    }

type SenseSupplyChangesCapability =
    {
        SupplyChangeApi: SupplyChangeApi
        SupplyChangeQueries: SupplyChangeQueries
    }

type EvaluateSupplyQualityCapability =
    {
        SupplyQualityApi: SupplyQualityApi
        SupplyQualityQueries: SupplyQualityQueries
    }

type DetectSupplyExceptionsCapability =
    {
        SupplyExceptionApi: SupplyExceptionApi
        SupplyExceptionQueries: SupplyExceptionQueries
    }

type ExplainSupplyDecisionsCapability =
    {
        SupplyExplanationApi: SupplyExplanationApi
        SupplyExplanationQueries: SupplyExplanationQueries
    }

type SupplyLearningCapability =
    {
        SupplyLearningApi: SupplyLearningApi
        SupplyLearningQueries: SupplyLearningQueries
    }

type SupplyDomainServices =
    {
        UnderstandSupply: UnderstandSupplyCapability
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
        LearnFromSupply: SupplyLearningCapability
    }

// Promise Intelligence Bounded Context Capabilities (CA-P-001 .. CA-P-011)

type UnderstandOrdersCapability =
    {
        OrderBook: Medhavi.Contracts.Promise.OrderBookApi
        CustomerProfiles: Medhavi.Contracts.Promise.CustomerPromiseProfileApi
        OrderBookQueries: Medhavi.Contracts.Promise.OrderBookQueries
        CustomerProfileQueries: Medhavi.Contracts.Promise.CustomerPromiseProfileQueries
    }

type AllocateSupplyCapability =
    {
        Allocation: Medhavi.Contracts.Promise.AllocationApi
        AllocationQueries: Medhavi.Contracts.Promise.AllocationQueries
    }

type ReserveInventoryCapability =
    {
        Reservation: Medhavi.Contracts.Promise.ReservationApi
        ReservationQueries: Medhavi.Contracts.Promise.ReservationQueries
    }

type EvaluatePromiseCapability =
    {
        Evaluation: Medhavi.Contracts.Promise.PromiseEvaluationApi
    }

type EstablishCommitmentsCapability =
    {
        Commitments: Medhavi.Contracts.Promise.CommitmentApi
        CommitmentQueries: Medhavi.Contracts.Promise.CommitmentQueries
        Explanation: Medhavi.Contracts.Promise.PromiseExplanationApi
        ExplanationQueries: Medhavi.Contracts.Promise.PromiseExplanationQueries
    }

type AssessPromiseRiskCapability =
    {
        Risk: Medhavi.Contracts.Promise.PromiseRiskApi
        RiskQueries: Medhavi.Contracts.Promise.PromiseRiskQueries
    }

type ManageBackordersCapability =
    {
        Backorder: Medhavi.Contracts.Promise.BackorderApi
        BackorderQueries: Medhavi.Contracts.Promise.BackorderQueries
    }

type ResolvePromiseExceptionsCapability =
    {
        ExceptionApi: Medhavi.Contracts.Promise.PromiseExceptionApi
        ExceptionQueries: Medhavi.Contracts.Promise.PromiseExceptionQueries
    }

type MeasurePromisePerformanceCapability =
    {
        Quality: Medhavi.Contracts.Promise.PromiseQualityApi
        QualityQueries: Medhavi.Contracts.Promise.PromiseQualityQueries
    }

type LearnFromPromisesCapability =
    {
        Learning: Medhavi.Contracts.Promise.PromiseLearningApi
        LearningQueries: Medhavi.Contracts.Promise.PromiseLearningQueries
    }

type PromiseDomainServices =
    {
        UnderstandOrders: UnderstandOrdersCapability
        AllocateSupply: AllocateSupplyCapability
        ReserveInventory: ReserveInventoryCapability
        EvaluatePromise: EvaluatePromiseCapability
        EstablishCommitments: EstablishCommitmentsCapability
        AssessPromiseRisk: AssessPromiseRiskCapability
        ManageBackorders: ManageBackordersCapability
        ResolveExceptions: ResolvePromiseExceptionsCapability
        MeasurePerformance: MeasurePromisePerformanceCapability
        LearnFromPromises: LearnFromPromisesCapability
    }

// Platform Composite Gateway

type DomainServices =
    {
        Core: CoreDomainServices
        Demand: DemandDomainServices
        Supply: SupplyDomainServices
        Promise: PromiseDomainServices
    }
