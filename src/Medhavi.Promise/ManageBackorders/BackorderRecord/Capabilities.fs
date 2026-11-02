/// CA-P-008 Manage Backorders — Aggregate Capabilities
module Medhavi.Promise.ManageBackorders.BackorderRecord.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution
open Medhavi.Foundation.Execution.CommandCapabilities
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model
open Policies

type BackorderAggregateApi =
    { Enqueue: EnqueueBackorderReq -> Task<Result<BackorderRecord, ApplicationError>>
      Resolve: ResolveBackorderReq -> Task<Result<BackorderRecord, ApplicationError>> }

let create
    (repo: Repository<BackorderRecord, BackorderId, BackorderEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: BacklogGovernancePolicy)
    (deps: AggregateStages.EnvelopeStoreDependencies<BackorderEvent>)
    (ports: PromisePorts)
    : BackorderAggregateApi =

    let enqueue (req: EnqueueBackorderReq) : Task<Result<BackorderRecord, ApplicationError>> =
        taskResult {
            let! cmd: EnqueueBackorderCmd = liftValidation (ACL.toEnqueueCmd req)

            let! custExists = protect (ports.CustomerExists cmd.CustomerId)
            let! itemExists = protect (ports.ItemExists cmd.Item)
            let! locExists = protect (ports.LocationExists cmd.Location)

            let decideFn = Behaviors.enqueue policy custExists itemExists locExists
            let pipeline = CommandPipeline.create repo (fun (c: EnqueueBackorderCmd) -> c.BackorderId) decideFn deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    let resolve (req: ResolveBackorderReq) : Task<Result<BackorderRecord, ApplicationError>> =
        taskResult {
            let! cmd: ResolveBackorderCmd = liftValidation (ACL.toResolveCmd req)
            let decideFn = Behaviors.resolve policy
            let pipeline = CommandPipeline.create repo (fun (c: ResolveBackorderCmd) -> c.BackorderId) decideFn deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    { Enqueue = enqueue
      Resolve = resolve }

