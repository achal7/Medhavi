module Medhavi.Nexus.Composition.ComposeClient

open System
open System.Threading.Tasks
open Microsoft.Extensions.DependencyInjection
open Medhavi.Contracts
open Medhavi.Contracts.Client
open Medhavi.Contracts.Core
open Medhavi.Contracts.Core.Demand
open Medhavi.Contracts.Core.Exception
open Medhavi.Contracts.Core.ItemTransition
open Medhavi.Contracts.Demand
open Medhavi.Contracts.Supply
open Medhavi.Client
open Medhavi.SemanticModel
open Medhavi.Demand
open Medhavi.Supply

[<RequireQualifiedAccess>]
module private QueryAdapter =

    /// Contramap: adapts a Foundation QueryService to a Contracts QueryService
    /// across an architectural boundary using a failable ID parser.
    let adaptWith
        (parseId: 'ClientId -> Result<'DomainId, 'Error>)
        (q: Medhavi.Foundation.Contracts.QueryService<'Entity, 'DomainId>)
        : Medhavi.Contracts.QueryService<'Entity, 'ClientId> =
        { GetAll = q.GetAll
          GetById =
            fun (id: 'ClientId) ->
                match parseId id with
                | Ok domainId -> q.GetById domainId
                | Error _ -> Task.FromResult None
          Exists =
            fun (id: 'ClientId) ->
                match parseId id with
                | Ok domainId -> q.Exists domainId
                | Error _ -> Task.FromResult false
          Filter = q.Filter
          SubscribeApiEvents = q.SubscribeApiEvents }

    /// Bridge: adapts a Foundation QueryService to a Contracts QueryService
    /// when the ID type is identical across the boundary (e.g. string).
    let bridge
        (q: Medhavi.Foundation.Contracts.QueryService<'Entity, 'Id>)
        : Medhavi.Contracts.QueryService<'Entity, 'Id> =
        { GetAll = q.GetAll
          GetById = q.GetById
          Exists = q.Exists
          Filter = q.Filter
          SubscribeApiEvents = q.SubscribeApiEvents }

let addApplicationServices
    (configuration: ClientConfiguration)
    (nexusCtx: ComposeNexus.NexusContext)
    (services: IServiceCollection)
    : IServiceCollection =

    // 1. Register Clock & ClientConfiguration
    services.AddSingleton<Clock>(system) |> ignore
    services.AddSingleton<ClientConfiguration>(configuration) |> ignore

    // 2. Core Domain Services (CA-C-019 .. CA-C-022)
    let demandApi: DemandApi = nexusCtx.Core.DemandManagement.Commands
    let demandQueries: DemandQueries = QueryAdapter.adaptWith DemandId.create nexusCtx.Core.DemandManagement.Queries

    let exceptionApi: ExceptionApi = nexusCtx.Core.ExceptionManagement.Commands
    let exceptionQueries: ExceptionQueries = QueryAdapter.adaptWith ExceptionId.create nexusCtx.Core.ExceptionManagement.Queries

    let itemTransitionApi: ItemTransitionApi = nexusCtx.Core.ItemTransitionManagement.Commands
    let itemTransitionQueries: ItemTransitionQueries =
        QueryAdapter.adaptWith TransitionId.create nexusCtx.Core.ItemTransitionManagement.Queries

    let epicApi: EnterprisePictureApi = nexusCtx.Core.EnterprisePictureManagement.Commands
    let epicQueries: EnterprisePictureQueries =
        QueryAdapter.adaptWith PlanningScopeId.create nexusCtx.Core.EnterprisePictureManagement.Queries

    let coreServices: CoreDomainServices =
        { DemandManagement =
            { Commands = demandApi
              Queries = demandQueries }
          ExceptionManagement =
            { Commands = exceptionApi
              Queries = exceptionQueries }
          ItemTransitionManagement =
            { Commands = itemTransitionApi
              Queries = itemTransitionQueries } }

    services.AddSingleton<CoreDomainServices>(coreServices) |> ignore
    services.AddSingleton<DemandApi>(demandApi) |> ignore
    services.AddSingleton<DemandQueries>(demandQueries) |> ignore
    services.AddSingleton<ExceptionApi>(exceptionApi) |> ignore
    services.AddSingleton<ExceptionQueries>(exceptionQueries) |> ignore
    services.AddSingleton<ItemTransitionApi>(itemTransitionApi) |> ignore
    services.AddSingleton<ItemTransitionQueries>(itemTransitionQueries) |> ignore
    services.AddSingleton<EnterprisePictureApi>(epicApi) |> ignore
    services.AddSingleton<EnterprisePictureQueries>(epicQueries) |> ignore

    // 3. Demand Intelligence Domain Services (CA-D-001)
    let demandObsApi: DemandObservationApi = nexusCtx.Demand.UnderstandDemandContext.Commands.Observations
    let demandObsQueries: DemandObservationQueries =
        QueryAdapter.adaptWith DemandObservationId.create nexusCtx.Demand.UnderstandDemandContext.ObservationQueries

    let demandUndApi: DemandUnderstandingApi = nexusCtx.Demand.UnderstandDemandContext.Commands.Understanding
    let demandUndQueries: DemandUnderstandingQueries =
        QueryAdapter.adaptWith PlanningScopeId.create nexusCtx.Demand.UnderstandDemandContext.UnderstandingQueries

    let demandServices: DemandDomainServices =
        { UnderstandDemand =
            { Observation =
                { Commands = demandObsApi
                  Queries = demandObsQueries }
              Understanding =
                { Commands = demandUndApi
                  Queries = demandUndQueries } } }

    services.AddSingleton<DemandDomainServices>(demandServices) |> ignore
    services.AddSingleton<DemandObservationApi>(demandObsApi) |> ignore
    services.AddSingleton<DemandObservationQueries>(demandObsQueries) |> ignore
    services.AddSingleton<DemandUnderstandingApi>(demandUndApi) |> ignore
    services.AddSingleton<DemandUnderstandingQueries>(demandUndQueries) |> ignore

    // 4. Supply Intelligence Domain Services (CA-S-001 .. CA-S-013)
    let supplyServices: SupplyDomainServices =
        { UnderstandSupply =
            { SupplyUnderstandingApi = nexusCtx.Supply.UnderstandSupply.Commands.Understanding
              SupplyDataRecordApi = nexusCtx.Supply.UnderstandSupply.Commands.Records
              SupplyUnderstandingQueries = QueryAdapter.adaptWith PlanningScopeId.create nexusCtx.Supply.UnderstandSupply.UnderstandingQueries
              SupplyDataRecordQueries = QueryAdapter.adaptWith SupplyDataRecordId.create nexusCtx.Supply.UnderstandSupply.RecordQueries }
          PlanSupply =
            { SupplyPlanApi = nexusCtx.Supply.PlanSupply.Commands.Plan
              SupplyPlanQueries = QueryAdapter.adaptWith SupplyPlanId.create nexusCtx.Supply.PlanSupply.PlanQueries }
          ManageInventory =
            { ManageInventoryApis = nexusCtx.Supply.ManageInventory.Commands
              InventoryPositionQueries = QueryAdapter.adaptWith ItemLocationId.create nexusCtx.Supply.ManageInventory.PositionQueries
              InventoryPolicyQueries = QueryAdapter.adaptWith ItemLocationId.create nexusCtx.Supply.ManageInventory.PolicyQueries
              InventoryHealthQueries = QueryAdapter.adaptWith InventoryHealthAssessmentId.create nexusCtx.Supply.ManageInventory.HealthQueries }
          ManageCapacity =
            { ManageCapacityApis = nexusCtx.Supply.ManageCapacity.Commands
              CapacityPositionQueries = QueryAdapter.adaptWith PhysicalResourceId.create nexusCtx.Supply.ManageCapacity.PositionQueries
              CapacityHealthQueries = QueryAdapter.adaptWith CapacityHealthAssessmentId.create nexusCtx.Supply.ManageCapacity.HealthQueries }
          CollaborateWithSuppliers =
            { CollaborateWithSuppliersApis = nexusCtx.Supply.CollaborateWithSuppliers.Commands
              SupplierCommitmentQueries = QueryAdapter.adaptWith SupplierCommitmentId.create nexusCtx.Supply.CollaborateWithSuppliers.CommitmentQueries
              SupplierAssessmentQueries = QueryAdapter.adaptWith SupplierId.create nexusCtx.Supply.CollaborateWithSuppliers.AssessmentQueries
              SupplierHealthQueries = QueryAdapter.adaptWith SupplierCollaborationHealthAssessmentId.create nexusCtx.Supply.CollaborateWithSuppliers.HealthQueries }
          ProcureMaterials =
            { ProcureMaterialsApis = nexusCtx.Supply.ProcureMaterials.Commands
              ProcurementRecommendationQueries = QueryAdapter.adaptWith ProcurementRequirementId.create nexusCtx.Supply.ProcureMaterials.RecommendationQueries
              ProcurementPlanQueries = QueryAdapter.adaptWith ProcurementPlanId.create nexusCtx.Supply.ProcureMaterials.PlanQueries }
          ScheduleProduction =
            { ScheduleProductionApis = nexusCtx.Supply.ScheduleProduction.Commands
              ProductionScheduleQueries = QueryAdapter.bridge nexusCtx.Supply.ScheduleProduction.ScheduleQueries
              PublishedScheduleQueries = QueryAdapter.bridge nexusCtx.Supply.ScheduleProduction.PublishedQueries }
          ManageDistribution =
            { ManageDistributionApis = nexusCtx.Supply.ManageDistribution.Commands
              DistributionRecommendationQueries = QueryAdapter.adaptWith DistributionRequirementId.create nexusCtx.Supply.ManageDistribution.RecommendationQueries
              DistributionPlanQueries = QueryAdapter.adaptWith DistributionPlanId.create nexusCtx.Supply.ManageDistribution.PlanQueries }
          SenseSupplyChanges =
            { SupplyChangeApi =
                { Assess = nexusCtx.Supply.SenseSupplyChanges.Commands.Assess
                  Archive = nexusCtx.Supply.SenseSupplyChanges.Commands.Archive }
              SupplyChangeQueries = QueryAdapter.adaptWith SupplyChangeAssessmentId.create nexusCtx.Supply.SenseSupplyChanges.Queries }
          EvaluateSupplyQuality =
            { SupplyQualityApi = { Evaluate = nexusCtx.Supply.EvaluateSupplyQuality.Commands.Evaluate }
              SupplyQualityQueries = QueryAdapter.adaptWith SupplyQualityAssessmentId.create nexusCtx.Supply.EvaluateSupplyQuality.Queries }
          DetectSupplyExceptions =
            { SupplyExceptionApi = nexusCtx.Supply.DetectSupplyExceptions.Commands.Exceptions
              SupplyExceptionQueries = QueryAdapter.adaptWith SupplyExceptionId.create nexusCtx.Supply.DetectSupplyExceptions.Queries }
          ExplainSupplyDecisions =
            { SupplyExplanationApi = nexusCtx.Supply.ExplainSupplyDecisions.Commands.Explanation
              SupplyExplanationQueries = QueryAdapter.adaptWith SupplyExplanationId.create nexusCtx.Supply.ExplainSupplyDecisions.Queries }
          LearnFromSupply =
            { SupplyLearningApi = nexusCtx.Supply.LearnFromSupply.Commands
              SupplyLearningQueries = QueryAdapter.adaptWith SupplyLearningId.create nexusCtx.Supply.LearnFromSupply.Queries } }

    services.AddSingleton<SupplyDomainServices>(supplyServices) |> ignore

    // Cluster 3A Singletons (CA-S-002, CA-S-003, CA-S-004)
    services.AddSingleton<InventoryPositionQueries>(supplyServices.ManageInventory.InventoryPositionQueries) |> ignore
    services.AddSingleton<InventoryPolicyQueries>(supplyServices.ManageInventory.InventoryPolicyQueries) |> ignore
    services.AddSingleton<InventoryHealthQueries>(supplyServices.ManageInventory.InventoryHealthQueries) |> ignore
    services.AddSingleton<InventoryPositionApi>(supplyServices.ManageInventory.ManageInventoryApis.Position) |> ignore
    services.AddSingleton<InventoryPolicyApi>(supplyServices.ManageInventory.ManageInventoryApis.Policy) |> ignore

    services.AddSingleton<CapacityPositionQueries>(supplyServices.ManageCapacity.CapacityPositionQueries) |> ignore
    services.AddSingleton<CapacityHealthQueries>(supplyServices.ManageCapacity.CapacityHealthQueries) |> ignore
    services.AddSingleton<CapacityPositionApi>(supplyServices.ManageCapacity.ManageCapacityApis.Position) |> ignore

    services.AddSingleton<SupplyPlanQueries>(supplyServices.PlanSupply.SupplyPlanQueries) |> ignore
    services.AddSingleton<SupplyPlanApi>(supplyServices.PlanSupply.SupplyPlanApi) |> ignore

    // Cluster 3B Singletons (CA-S-005, CA-S-006, CA-S-007, CA-S-008)
    services.AddSingleton<SupplierCommitmentQueries>(supplyServices.CollaborateWithSuppliers.SupplierCommitmentQueries) |> ignore
    services.AddSingleton<SupplierCommitmentAssessmentQueries>(supplyServices.CollaborateWithSuppliers.SupplierAssessmentQueries) |> ignore
    services.AddSingleton<SupplierCollaborationHealthQueries>(supplyServices.CollaborateWithSuppliers.SupplierHealthQueries) |> ignore
    services.AddSingleton<SupplierCommitmentApi>(supplyServices.CollaborateWithSuppliers.CollaborateWithSuppliersApis.Commitment) |> ignore
    services.AddSingleton<SupplierCommitmentAssessmentApi>(supplyServices.CollaborateWithSuppliers.CollaborateWithSuppliersApis.Assessment) |> ignore
    services.AddSingleton<SupplierCollaborationHealthApi>(supplyServices.CollaborateWithSuppliers.CollaborateWithSuppliersApis.Health) |> ignore

    services.AddSingleton<ProcurementRecommendationQueries>(supplyServices.ProcureMaterials.ProcurementRecommendationQueries) |> ignore
    services.AddSingleton<ProcurementPlanQueries>(supplyServices.ProcureMaterials.ProcurementPlanQueries) |> ignore
    services.AddSingleton<ProcureMaterialsApis>(supplyServices.ProcureMaterials.ProcureMaterialsApis) |> ignore

    services.AddSingleton<ProductionScheduleQueries>(supplyServices.ScheduleProduction.ProductionScheduleQueries) |> ignore
    services.AddSingleton<PublishedProductionScheduleQueries>(supplyServices.ScheduleProduction.PublishedScheduleQueries) |> ignore
    services.AddSingleton<ScheduleProductionApis>(supplyServices.ScheduleProduction.ScheduleProductionApis) |> ignore

    services.AddSingleton<DistributionRecommendationQueries>(supplyServices.ManageDistribution.DistributionRecommendationQueries) |> ignore
    services.AddSingleton<DistributionPlanQueries>(supplyServices.ManageDistribution.DistributionPlanQueries) |> ignore
    services.AddSingleton<ManageDistributionApis>(supplyServices.ManageDistribution.ManageDistributionApis) |> ignore

    // Cluster 3C Singletons (CA-S-001, CA-S-009, CA-S-010, CA-S-011, CA-S-012, CA-S-013)
    services.AddSingleton<SupplyDataRecordQueries>(supplyServices.UnderstandSupply.SupplyDataRecordQueries) |> ignore
    services.AddSingleton<SupplyUnderstandingQueries>(supplyServices.UnderstandSupply.SupplyUnderstandingQueries) |> ignore
    services.AddSingleton<SupplyDataRecordApi>(supplyServices.UnderstandSupply.SupplyDataRecordApi) |> ignore
    services.AddSingleton<SupplyUnderstandingApi>(supplyServices.UnderstandSupply.SupplyUnderstandingApi) |> ignore

    services.AddSingleton<SupplyChangeQueries>(supplyServices.SenseSupplyChanges.SupplyChangeQueries) |> ignore
    services.AddSingleton<SupplyChangeApi>(supplyServices.SenseSupplyChanges.SupplyChangeApi) |> ignore

    services.AddSingleton<SupplyQualityQueries>(supplyServices.EvaluateSupplyQuality.SupplyQualityQueries) |> ignore
    services.AddSingleton<SupplyQualityApi>(supplyServices.EvaluateSupplyQuality.SupplyQualityApi) |> ignore

    services.AddSingleton<SupplyExceptionQueries>(supplyServices.DetectSupplyExceptions.SupplyExceptionQueries) |> ignore
    services.AddSingleton<SupplyExceptionApi>(supplyServices.DetectSupplyExceptions.SupplyExceptionApi) |> ignore

    services.AddSingleton<SupplyExplanationQueries>(supplyServices.ExplainSupplyDecisions.SupplyExplanationQueries) |> ignore
    services.AddSingleton<SupplyExplanationApi>(supplyServices.ExplainSupplyDecisions.SupplyExplanationApi) |> ignore

    services.AddSingleton<SupplyLearningQueries>(supplyServices.LearnFromSupply.SupplyLearningQueries) |> ignore
    services.AddSingleton<SupplyLearningApi>(supplyServices.LearnFromSupply.SupplyLearningApi) |> ignore

    // 5. Platform Domain Services Composite
    let domainServices: DomainServices =
        { Core = coreServices
          Demand = demandServices
          Supply = supplyServices }

    services.AddSingleton<DomainServices>(domainServices) |> ignore

    services
