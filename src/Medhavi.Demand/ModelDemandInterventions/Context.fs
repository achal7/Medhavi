/// CA-D-011 Model Demand Interventions Composition Root
module Medhavi.Demand.ModelDemandInterventions.Context

open System
open System.Threading
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Medhavi.Demand.ModelDemandInterventions.DemandInterventionImpact.Model
open Medhavi.Demand.ModelDemandInterventions.DemandInterventionImpact.Policies
open Medhavi.Demand.ModelDemandInterventions.DemandInterventionImpact.Projections
open Medhavi.Demand.ModelDemandInterventions.Workflows

/// Public context exposing commands, queries, and lifecycle management
type DemandInterventionContext =
    { Commands: DemandInterventionApi
      Queries: QueryService<DemandInterventionImpactDto, DemandInterventionImpactId>
      Dispose: unit -> unit }

/// Creates the complete Model Demand Interventions context with all dependencies wired
let create
    (repo: Repository<DemandInterventionImpact, DemandInterventionImpactId, DemandInterventionImpactEvent>)
    (deps: EnvelopeStoreDependencies<DemandInterventionImpactEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (createQueryService: CreateQueryService<DemandInterventionImpactEvent, DemandInterventionImpactDto, DemandInterventionImpactId>)
    (policy: InterventionModelingGovernancePolicy)
    (ports: DemandPorts)
    (defaultItemId: string)
    (defaultLocationId: string)
    : TaskResult<DemandInterventionContext, ApplicationError> =
    taskResult {

        // 1. Seed projection read model from repository
        let! aggregates =
            repo.GetAll ()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let state = seedFromAggregates aggregates

        let validEventTypes =
            [ ArsIdentifiers.EnterpriseEvents.demandInterventionImpactPublished.Id ]
            |> EnvelopeFilter.EventTypes

        // 2. Initialize projection query service
        let! queryCtx: ProjectionContext<DemandInterventionImpactDto, DemandInterventionImpactId> =
            createQueryService apply validEventTypes state "DemandInterventionImpact"

        // 3. Instantiate child aggregate API
        let aggregateApi =
            DemandInterventionImpact.Capabilities.create
                repo
                publishKnowledge
                policy
                deps
                ports

        // 4. Instantiate parent capability API
        let commands = Medhavi.Demand.ModelDemandInterventions.Capabilities.create aggregateApi deps.DispatchEnvelope

        // 5. Wire workflow dependencies
        let workflowDeps: DemandInterventionWorkflowDependencies =
            { Subscribe = deps.Subscribe
              CapabilityApi = commands
              DefaultItemId = defaultItemId
              DefaultLocationId = defaultLocationId }

        let! assessSub: IDisposable =
            createDemandInterventionAssessmentWorkflow workflowDeps CancellationToken.None
            |> TaskResult.ofTaskValue
            |> TaskResult.mapError (fun err ->
                Infrastructure(EventStore($"Failed to create demand intervention assessment workflow: {err}")))

        let! pubSub: IDisposable =
            createDemandInterventionPublishWorkflow workflowDeps CancellationToken.None
            |> TaskResult.ofTaskValue
            |> TaskResult.mapError (fun err ->
                Infrastructure(EventStore($"Failed to create demand intervention publish workflow: {err}")))

        // 6. Combine lifecycle disposables
        let dispose () =
            queryCtx.Dispose ()
            assessSub.Dispose ()
            pubSub.Dispose ()

        return
            { Commands = commands
              Queries = queryCtx.QueryService
              Dispose = dispose }
    }
