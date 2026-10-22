/// CA-S-002 Plan Supply Composition Root
module Medhavi.Supply.PlanSupply.Context

open System
open System.Threading
open Medhavi.Common
open Medhavi.Contracts.Supply
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers
open Medhavi.Supply.PlanSupply.SupplyPlan.Model
open Medhavi.Supply.PlanSupply.SupplyPlan.Policies
open Medhavi.Supply.PlanSupply.Workflows
open Capabilities

/// Public context exposing commands, queries, and lifecycle management for Plan Supply
type PlanSupplyContext =
    { Commands: PlanSupplyApis
      PlanQueries: QueryService<SupplyPlanDto, SupplyPlanId>
      Dispose: unit -> unit }

/// Creates the complete Plan Supply context with aggregates, projections, and workflows wired
let create
    (repo: Repository<SupplyPlan, SupplyPlanId, SupplyPlanEvent>)
    (deps: EnvelopeStoreDependencies<SupplyPlanEvent>)
    (queryService: CreateQueryService<SupplyPlanEvent, SupplyPlanDto, SupplyPlanId>)
    (relaxationPolicy: ConstraintRelaxationPolicy)
    (acceptancePolicy: SupplyPlanAcceptancePolicy)
    (cadencePolicy: SupplyPlanPublicationCadencePolicy)
    (publishKnowledge: KnowledgeRepresentation)
    (ports: SupplyPorts)
    (defaultScopeId: string)
    (defaultHorizonId: string)
    : TaskResult<PlanSupplyContext, ApplicationError> =
    taskResult {

        // 1. Initialize Supply Plan projection
        let! planAggregates: SupplyPlan list =
            repo.GetAll ()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let planInitialState = SupplyPlan.Projections.seedFromAggregates planAggregates

        let planValidEvents =
            [ ArsIdentifiers.EnterpriseEvents.supplyPlanGenerated.Id
              ArsIdentifiers.EnterpriseEvents.supplyPlanEvaluated.Id
              ArsIdentifiers.EnterpriseEvents.supplyPlanPublished.Id ]
            |> EnvelopeFilter.EventTypes

        let! planQueryCtx: ProjectionContext<SupplyPlanDto, SupplyPlanId> =
            queryService SupplyPlan.Projections.apply planValidEvents planInitialState "SupplyPlan"

        // 2. Wire aggregate command capabilities
        let planAggApi =
            SupplyPlan.Capabilities.create
                repo
                publishKnowledge
                relaxationPolicy
                acceptancePolicy
                cadencePolicy
                deps
                ports

        // 3. Compose capability parent API
        let apis = Capabilities.create planAggApi

        // 4. Wire background event-driven workflows
        let wfDeps: PlanSupplyWorkflowDependencies =
            { PlanCodec = Medhavi.Foundation.Codec.json
              Subscribe = deps.Subscribe
              DispatchEnvelope = deps.DispatchEnvelope
              Apis = apis
              DefaultScopeId = defaultScopeId
              DefaultHorizonId = defaultHorizonId }

        let! (sub1: IDisposable) = createPlanTriggerWorkflow wfDeps CancellationToken.None
        let! (sub2: IDisposable) = createPlanEvaluationWorkflow wfDeps CancellationToken.None
        let! (sub3: IDisposable) = createPlanPublishWorkflow wfDeps CancellationToken.None
        let! (sub4: IDisposable) = createNotificationNodeWorkflow wfDeps CancellationToken.None

        let dispose () =
            planQueryCtx.Dispose ()
            sub1.Dispose ()
            sub2.Dispose ()
            sub3.Dispose ()
            sub4.Dispose ()

        return
            { Commands = apis
              PlanQueries = planQueryCtx.QueryService
              Dispose = dispose }
    }
