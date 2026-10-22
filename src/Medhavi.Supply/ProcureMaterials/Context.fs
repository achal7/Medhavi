namespace Medhavi.Supply.ProcureMaterials

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.SemanticModel
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Medhavi.Supply.ProcureMaterials.ProcurementPlan
open Medhavi.Supply.ProcureMaterials.ProcurementPlan.Model
open Medhavi.Supply.ProcureMaterials.ProcurementRecommendation
open Medhavi.Supply.ProcureMaterials.ProcurementRecommendation.Model

type ProcureMaterialsContext =
    { Commands: ProcureMaterialsApis
      RecommendationQueries: QueryService<ProcurementRecommendationDto, ProcurementRequirementId>
      PlanQueries: QueryService<ProcurementPlanDto, ProcurementPlanId>
      Dispose: unit -> unit }

/// CA-S-006 Procure Materials Composition Root
module Context =

    let create
        (recommendationRepo: Repository<ProcurementRecommendation, ProcurementRequirementId, ProcurementRecommendationEvent>)
        (recommendationDeps: EnvelopeStoreDependencies<ProcurementRecommendationEvent>)
        (recommendationQueryService: CreateQueryService<ProcurementRecommendationEvent, ProcurementRecommendationDto, ProcurementRequirementId>)
        (planRepo: Repository<ProcurementPlan, ProcurementPlanId, ProcurementPlanEvent>)
        (planDeps: EnvelopeStoreDependencies<ProcurementPlanEvent>)
        (planQueryService: CreateQueryService<ProcurementPlanEvent, ProcurementPlanDto, ProcurementPlanId>)
        (procurementPolicy: ProcurementPolicyConfig)
        (planPolicy: PlanPublicationPolicyConfig)
        (publishKnowledge: KnowledgeRepresentation)
        (ports: SupplyPorts)
        (defaultRequirementId: string)
        (defaultPlanId: string)
        (defaultItemId: string)
        (defaultLocationId: string)
        (defaultScopeId: string)
        (defaultCycleId: string)
        (defaultAuthority: string)
        : TaskResult<ProcureMaterialsContext, ApplicationError> =
        taskResult {

            // 1. Projections
            let! recAggs: ProcurementRecommendation list =
                recommendationRepo.GetAll ()
                |> TaskResult.mapError Repository.mapRepositoryErrorToApplicationError

            let recInitial = ProcurementRecommendation.Projections.seedFromAggregates recAggs
            let recFilter = [ ArsIdentifiers.EnterpriseEvents.procurementRecommendationCreated.Id ] |> EnvelopeFilter.EventTypes
            let! recQueryCtx: ProjectionContext<ProcurementRecommendationDto, ProcurementRequirementId> =
                recommendationQueryService ProcurementRecommendation.Projections.apply recFilter recInitial "ProcurementRecommendation"

            let! planAggs: ProcurementPlan list =
                planRepo.GetAll ()
                |> TaskResult.mapError Repository.mapRepositoryErrorToApplicationError

            let planInitial = ProcurementPlan.Projections.seedFromAggregates planAggs
            let planFilter = [ ArsIdentifiers.EnterpriseEvents.procurementPlanPublished.Id ] |> EnvelopeFilter.EventTypes
            let! planQueryCtx: ProjectionContext<ProcurementPlanDto, ProcurementPlanId> =
                planQueryService ProcurementPlan.Projections.apply planFilter planInitial "ProcurementPlan"

            // 2. Facts Providers
            let recFactsProvider : ProcurementRecommendation.Behaviors.ProcurementFactsProvider =
                fun itemId locationId ->
                    let paramsOpt = (ports.GetItemPurchasingParameters itemId locationId).GetAwaiter().GetResult()
                    let candidates = (ports.GetCandidateSuppliers itemId locationId).GetAwaiter().GetResult()
                    let disruptions = (ports.GetActiveDisruptionSignals itemId locationId).GetAwaiter().GetResult()
                    let netGaps = (ports.GetFutureNetGaps itemId locationId (Timestamp.now()) (TimeSpan.FromDays(30.0))).GetAwaiter().GetResult()
                    paramsOpt, candidates, Map.empty, disruptions, netGaps

            let planFactsProvider : ProcurementPlan.Behaviors.ProcurementPlanFactsProvider =
                fun _scopeId ->
                    let allRecs =
                        match (recommendationRepo.GetAll ()).GetAwaiter().GetResult() with
                        | Ok recs -> recs
                        | Error _ -> []
                    let active =
                        allRecs
                        |> List.filter (fun r -> r.LifecycleState = ProcurementRecommendationLifecycleState.Active)
                        |> List.map (fun r ->
                            { RequirementId = r.RequirementId
                              ItemId = r.ItemId
                              LocationId = r.LocationId
                              Allocations =
                                r.Allocations
                                |> List.map (fun a ->
                                    { SupplierId = a.SupplierId
                                      Quantity = a.Quantity
                                      OrderReleaseDate = a.OrderReleaseDate
                                      ExpectedDeliveryDate = a.ExpectedDeliveryDate
                                      EstimatedUnitCost = a.EstimatedUnitCost })
                              OverallConfidence = r.OverallConfidence
                              Feasibility = r.Feasibility
                              Strategy = r.Strategy
                              IsEscalated =
                                match r.GovernanceStatus with
                                | ProcurementGovernanceStatus.Escalated _ -> true
                                | _ -> false
                              IsHeldForConsolidation = (r.Determination.Directive = SourcingDirective.HoldForConsolidation) })
                    active, allRecs.Length

            // 3. Aggregate APIs
            let recAggregate =
                ProcurementRecommendation.Capabilities.create
                    recommendationRepo
                    publishKnowledge
                    recommendationDeps
                    recFactsProvider
                    procurementPolicy

            let planAggregate =
                ProcurementPlan.Capabilities.create
                    planRepo
                    publishKnowledge
                    planDeps
                    planFactsProvider
                    planPolicy

            // 4. Parent Capability API
            let apis =
                Medhavi.Supply.ProcureMaterials.Capabilities.create
                    { RecommendationAggregate = recAggregate
                      PlanAggregate = planAggregate }

            // 5. Background Workflows
            let wfDeps: ProcureMaterialsWorkflowDependencies =
                { Subscribe = recommendationDeps.Subscribe
                  DispatchEnvelope = recommendationDeps.DispatchEnvelope
                  Apis = apis
                  DefaultRequirementId = defaultRequirementId
                  DefaultPlanId = defaultPlanId
                  DefaultItemId = defaultItemId
                  DefaultLocationId = defaultLocationId
                  DefaultScopeId = defaultScopeId
                  DefaultCycleId = defaultCycleId
                  DefaultAuthority = defaultAuthority
                  DefaultRequiredQuantity = 0.0m
                  DefaultLeadTimeDays = 0.0 }

            let! (sub1: IDisposable) = Workflows.createProcurementRecommendationsWorkflow wfDeps CancellationToken.None
            let! (sub2: IDisposable) = Workflows.createProcurementPlanPublicationWorkflow wfDeps CancellationToken.None
            let! (sub3: IDisposable) = Workflows.createNotificationNodeWorkflow wfDeps CancellationToken.None

            let dispose () =
                recQueryCtx.Dispose()
                planQueryCtx.Dispose()
                sub1.Dispose()
                sub2.Dispose()
                sub3.Dispose()

            return
                { Commands = apis
                  RecommendationQueries = recQueryCtx.QueryService
                  PlanQueries = planQueryCtx.QueryService
                  Dispose = dispose }
        }
