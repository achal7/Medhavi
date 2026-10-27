namespace Medhavi.Client

open System
open System.Globalization
open Medhavi.Contracts.Core.Demand
open Medhavi.Contracts.Core.Exception
open Medhavi.Contracts.Core.ItemTransition
open Medhavi.Contracts.Demand
open Medhavi.Contracts.Supply
open Medhavi.Client.Registries

type DemandManagementCapability =
    { Commands: DemandApi
      Queries: DemandQueries }

type ExceptionManagementCapability =
    { Commands: ExceptionApi
      Queries: ExceptionQueries }

type ItemTransitionManagementCapability =
    { Commands: ItemTransitionApi
      Queries: ItemTransitionQueries }

type CoreDomainServices =
    { DemandManagement: DemandManagementCapability
      ExceptionManagement: ExceptionManagementCapability
      ItemTransitionManagement: ItemTransitionManagementCapability }

type DemandObservationContext =
    { Commands: DemandObservationApi
      Queries: DemandObservationQueries }

type DemandUnderstandingContext =
    { Commands: DemandUnderstandingApi
      Queries: DemandUnderstandingQueries }

type UnderstandDemandContext =
    { Observation: DemandObservationContext
      Understanding: DemandUnderstandingContext }

type DemandDomainServices =
    { UnderstandDemand: UnderstandDemandContext }


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
// Platform Domain Services Bundle
// ============================================================================

type DomainServices =
    { Core: CoreDomainServices
      Demand: DemandDomainServices
      Supply: SupplyDomainServices }

open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.ActionExecution

// ============================================================================
// Data Store Semantics
// ============================================================================

type DataStore =
    { Id: DataStoreId
      Fetch: ApplicationContext -> Async<DataPayload> }

and DataStoreRegistry = Map<DataStoreId, DataStore>

and ApplicationContext =
    { PlanningContextService: Services.PlanningContextService
      ComponentRegistry: ComponentRegistry
      SchemaRegistry: SchemaRegistry
      DataStoreRegistry: DataStoreRegistry
      ActionRegistry: ActionRegistry
      Services: IServiceProvider
      DomainServices: DomainServices option }

    member this.Resolve<'T when 'T : not struct>() : 'T option =
        match box this.Services with
        | null -> None
        | _ ->
            match this.Services.GetService(typeof<'T>) with
            | null -> None
            | s -> Some(s :?> 'T)
