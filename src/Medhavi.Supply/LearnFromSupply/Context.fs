// Medhavi.Supply.LearnFromSupply.Context
// Traceability: Composition Root for CA-S-013 Learn From Supply
namespace Medhavi.Supply.LearnFromSupply

open System
open System.Threading
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers
open Medhavi.Supply.LearnFromSupply.SupplyLearning.Model
open Medhavi.Supply.LearnFromSupply.SupplyLearning.Policies
open Medhavi.Supply.LearnFromSupply.SupplyLearning.Projections
open Medhavi.Supply.LearnFromSupply.Workflows

type SupplyLearningContext =
    { Commands: SupplyLearningApi
      Queries: QueryService<SupplyLearningDto, SupplyLearningId>
      Dispose: unit -> unit }

module Context =

    let create
        (repo: Repository<SupplyLearning, SupplyLearningId, SupplyLearningEvent>)
        (deps: EnvelopeStoreDependencies<SupplyLearningEvent>)
        (publishKnowledge: KnowledgeRepresentation)
        (createQueryService: CreateQueryService<SupplyLearningEvent, SupplyLearningDto, SupplyLearningId>)
        (policy: LearningPolicy)
        : TaskResult<SupplyLearningContext, ApplicationError> =
        taskResult {

            // 1. Seed projection read model from repository
            let! aggregates =
                repo.GetAll ()
                |> TaskResult.ofTask
                |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

            let state = seedFromAggregates aggregates

            let validEventTypes =
                [ EnterpriseEvents.supplyLearningRecorded.Id
                  EnterpriseEvents.supplyLearningOutcomeRecorded.Id ]
                |> EnvelopeFilter.EventTypes

            // 2. Initialize projection query service
            let! queryCtx: ProjectionContext<SupplyLearningDto, SupplyLearningId> =
                createQueryService apply validEventTypes state "SupplyLearning"

            // 3. Instantiate child aggregate API
            let aggregateApi =
                SupplyLearning.Capabilities.create
                    repo
                    publishKnowledge
                    policy
                    deps

            // 4. Instantiate capability public API
            let commands =
                Capabilities.create aggregateApi

            // 5. Wire automated workflow subscription
            let workflowDeps: SupplyLearningWorkflowDependencies =
                { Subscribe = deps.Subscribe
                  DispatchEnvelope = deps.DispatchEnvelope
                  LearningApi = commands }

            let! (sub1: IDisposable) = createSupplyLearningDiscoveryWorkflow workflowDeps CancellationToken.None
            let! (sub2: IDisposable) = createNotificationNodeWorkflow workflowDeps CancellationToken.None

            // 6. Combine disposables
            let dispose () =
                queryCtx.Dispose ()
                sub1.Dispose ()
                sub2.Dispose ()

            return
                { Commands = commands
                  Queries = queryCtx.QueryService
                  Dispose = dispose }
        }
