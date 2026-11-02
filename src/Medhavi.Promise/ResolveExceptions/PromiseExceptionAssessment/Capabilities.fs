/// CA-P-009 Resolve Exceptions — Aggregate Capabilities
module Medhavi.Promise.ResolveExceptions.PromiseExceptionAssessment.Capabilities

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

type CommitmentEntity = Medhavi.Promise.EstablishCommitments.OrderCommitment.Model.OrderCommitment
type CommitmentEvent = Medhavi.Promise.EstablishCommitments.OrderCommitment.Model.OrderCommitmentEvent

type PromiseExceptionAggregateApi =
    { RaiseException: RaisePromiseExceptionReq -> Task<Result<PromiseExceptionAssessment, ApplicationError>>
      ResolveException: ResolvePromiseExceptionReq -> Task<Result<PromiseExceptionAssessment, ApplicationError>> }

let create
    (repo: Repository<PromiseExceptionAssessment, PromiseExceptionId, PromiseExceptionEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: DisruptionMitigationPolicy)
    (deps: AggregateStages.EnvelopeStoreDependencies<PromiseExceptionEvent>)
    (commitmentRepo: Repository<CommitmentEntity, OrderCommitmentId, CommitmentEvent>)
    : PromiseExceptionAggregateApi =

    let raiseException (req: RaisePromiseExceptionReq) : Task<Result<PromiseExceptionAssessment, ApplicationError>> =
        taskResult {
            let! cmd: RaisePromiseExceptionCmd = liftValidation (ACL.toRaiseCmd req)

            let! (commitmentOpt: CommitmentEntity option) =
                commitmentRepo.Get cmd.CommitmentId
                |> TaskResult.ofTask
                |> TaskResult.mapError (fun e -> Infrastructure(Database(sprintf "%A" e)))

            let decideFn = Behaviors.raise policy commitmentOpt.IsSome
            let pipeline = CommandPipeline.create repo (fun (c: RaisePromiseExceptionCmd) -> c.ExceptionId) decideFn deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    let resolveException (req: ResolvePromiseExceptionReq) : Task<Result<PromiseExceptionAssessment, ApplicationError>> =
        taskResult {
            let! cmd: ResolvePromiseExceptionCmd = liftValidation (ACL.toResolveCmd req)
            let decideFn = Behaviors.resolve policy
            let pipeline = CommandPipeline.create repo (fun (c: ResolvePromiseExceptionCmd) -> c.ExceptionId) decideFn deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    { RaiseException = raiseException
      ResolveException = resolveException }

