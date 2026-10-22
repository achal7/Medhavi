/// CA-S-008 — Manage Distribution Composition Root
/// Traces to: CR-S-019, CR-S-020, FS-S-080, FS-S-081
module Medhavi.Supply.ManageDistribution.Context

open System
open System.Threading
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.SemanticModel
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers
open Medhavi.Supply.ManageDistribution.DistributionRecommendation.Model
open Medhavi.Supply.ManageDistribution.DistributionRecommendation.Policies
open Medhavi.Supply.ManageDistribution.DistributionPlan.Model
open Medhavi.Supply.ManageDistribution.DistributionPlan.Policies
open Medhavi.Supply.ManageDistribution.Workflows
open Capabilities

/// Public context exposing commands, queries, and lifecycle management for Manage Distribution
type ManageDistributionContext =
    { Commands: ManageDistributionApis
      RecommendationQueries: QueryService<DistributionRecommendationDto, DistributionRequirementId>
      PlanQueries: QueryService<DistributionPlanDto, DistributionPlanId>
      Dispose: unit -> unit }

/// Creates the complete Manage Distribution context with child aggregates, projections, and workflows wired
let create
    (recRepo: Repository<DistributionRecommendationAggregate, DistributionRequirementId, DistributionRecommendationEvent>)
    (recDeps: EnvelopeStoreDependencies<DistributionRecommendationEvent>)
    (recQueryService: CreateQueryService<DistributionRecommendationEvent, DistributionRecommendationDto, DistributionRequirementId>)
    (recPolicy: DistributionPolicyParameters)
    (planRepo: Repository<DistributionPlanAggregate, DistributionPlanId, DistributionPlanEvent>)
    (planDeps: EnvelopeStoreDependencies<DistributionPlanEvent>)
    (planQueryService: CreateQueryService<DistributionPlanEvent, DistributionPlanDto, DistributionPlanId>)
    (planPolicy: DistributionPlanPublicationPolicyParameters)
    (publishKnowledge: KnowledgeRepresentation)
    (ports: SupplyPorts)
    (defaultScopeId: string)
    (defaultCycleId: string)
    : TaskResult<ManageDistributionContext, ApplicationError> =
    taskResult {

        // 1. Initialize Distribution Recommendation projection
        let! recAggregates: DistributionRecommendationAggregate list =
            recRepo.GetAll ()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let recInitialState = DistributionRecommendation.Projections.seedFromAggregates recAggregates

        let recValidEvents =
            [ ArsIdentifiers.EnterpriseEvents.distributionRecommendationCreated.Id ]
            |> EnvelopeFilter.EventTypes

        let! recQueryCtx: ProjectionContext<DistributionRecommendationDto, DistributionRequirementId> =
            recQueryService DistributionRecommendation.Projections.apply recValidEvents recInitialState "DistributionRecommendation"

        // 2. Initialize Distribution Plan projection
        let! planAggregates: DistributionPlanAggregate list =
            planRepo.GetAll ()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let planInitialState = DistributionPlan.Projections.seedFromAggregates planAggregates

        let planValidEvents =
            [ ArsIdentifiers.EnterpriseEvents.distributionPlanPublished.Id ]
            |> EnvelopeFilter.EventTypes

        let! planQueryCtx: ProjectionContext<DistributionPlanDto, DistributionPlanId> =
            planQueryService DistributionPlan.Projections.apply planValidEvents planInitialState "DistributionPlan"

        // 3. Instantiate Child Aggregate APIs
        let recAggApi =
            DistributionRecommendation.Capabilities.create
                recRepo
                publishKnowledge
                recPolicy
                recDeps
                ports

        let planAggApi =
            DistributionPlan.Capabilities.create
                planRepo
                publishKnowledge
                planPolicy
                planDeps
                ports

        // 4. Instantiate Parent Capability API
        let apis = Capabilities.create recAggApi planAggApi

        // 5. Wire workflow dependencies
        let workflowDeps: ManageDistributionWorkflowDependencies =
            { RecommendationCodec = Medhavi.Foundation.Codec.json
              PlanCodec = Medhavi.Foundation.Codec.json
              Subscribe = planDeps.Subscribe
              DispatchEnvelope = planDeps.DispatchEnvelope
              Apis = apis
              DefaultScopeId = defaultScopeId
              DefaultCycleId = defaultCycleId }

        let! (sub1: IDisposable) = createDistributionRequirementWorkflow workflowDeps CancellationToken.None
        let! (sub2: IDisposable) = createDistributionPlanPublishWorkflow workflowDeps CancellationToken.None
        let! (sub3: IDisposable) = createNotificationNodeWorkflow workflowDeps CancellationToken.None

        let dispose () =
            recQueryCtx.Dispose ()
            planQueryCtx.Dispose ()
            sub1.Dispose ()
            sub2.Dispose ()
            sub3.Dispose ()

        return
            { Commands = apis
              RecommendationQueries = recQueryCtx.QueryService
              PlanQueries = planQueryCtx.QueryService
              Dispose = dispose }
    }
