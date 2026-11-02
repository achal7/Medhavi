/// CA-P-002 Allocate Supply and Manage Priority Composition Root
module Medhavi.Promise.AllocateSupply.Context

open System
open System.Threading
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Medhavi.Promise.AllocateSupply.AllocationPool.Model
open Medhavi.Promise.AllocateSupply.AllocationPool.Policies
open Medhavi.Promise.AllocateSupply.Workflows
open Capabilities

type AllocateSupplyContext =
    { Commands: AllocateSupplyApis
      AllocationQueries: QueryService<AllocationPoolDto, AllocationPoolId>
      Dispose: unit -> unit }

let create
    (repo: Repository<AllocationPool, AllocationPoolId, AllocationPoolEvent>)
    (deps: EnvelopeStoreDependencies<AllocationPoolEvent>)
    (queryService: CreateQueryService<AllocationPoolEvent, AllocationPoolDto, AllocationPoolId>)
    (govPolicy: AllocationGovernancePolicy)
    (overdrawPolicy: ChannelOverdrawPolicy)
    (priorityPolicy: DynamicOrderPriorityPolicy)
    (publishKnowledge: KnowledgeRepresentation)
    (ports: PromisePorts)
    (defaultScopeId: string)
    : TaskResult<AllocateSupplyContext, ApplicationError> =
    taskResult {

        // 1. Initialize Allocation Pool projection
        let! poolAggregates: AllocationPool list =
            repo.GetAll ()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let poolInitialState = AllocationPool.Projections.seedFromAggregates poolAggregates

        let poolValidEvents =
            [ ArsIdentifiers.EnterpriseEvents.allocationBucketInitialized.Id
              ArsIdentifiers.EnterpriseEvents.allocationQuotaConsumed.Id
              ArsIdentifiers.EnterpriseEvents.allocationBucketsRebalanced.Id ]
            |> EnvelopeFilter.EventTypes

        let! queryCtx: ProjectionContext<AllocationPoolDto, AllocationPoolId> =
            queryService AllocationPool.Projections.apply poolValidEvents poolInitialState "AllocationPool"

        // 2. Instantiate Aggregate APIs
        let aggApi =
            AllocationPool.Capabilities.create
                repo
                publishKnowledge
                govPolicy
                overdrawPolicy
                priorityPolicy
                deps
                ports

        // 3. Instantiate Capability Parent API
        let apis = Capabilities.create aggApi

        // 4. Wire workflow dependencies
        let workflowDeps: AllocateSupplyWorkflowDependencies =
            { PoolCodec = Medhavi.Foundation.Codec.json
              Subscribe = deps.Subscribe
              DispatchEnvelope = deps.DispatchEnvelope
              Apis = apis
              DefaultScopeId = defaultScopeId }

        let! workflowSub: IDisposable =
            createAllocationWorkflow workflowDeps CancellationToken.None

        // 5. Lifecycle management
        let dispose () =
            queryCtx.Dispose ()
            workflowSub.Dispose ()

        return
            { Commands = apis
              AllocationQueries = queryCtx.QueryService
              Dispose = dispose }
    }

