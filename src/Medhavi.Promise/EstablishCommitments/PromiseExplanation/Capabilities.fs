/// CA-P-006 Establish Commitments — Promise Explanation Capabilities
module Medhavi.Promise.EstablishCommitments.PromiseExplanation.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution
open Medhavi.Foundation.Execution.CommandCapabilities
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model

type CommitmentEntity = Medhavi.Promise.EstablishCommitments.OrderCommitment.Model.OrderCommitment
type CommitmentEvent = Medhavi.Promise.EstablishCommitments.OrderCommitment.Model.OrderCommitmentEvent

type PromiseExplanationAggregateApi =
    { Generate: GeneratePromiseExplanationReq -> Task<Result<PromiseExplanation, ApplicationError>> }

let create
    (repo: Repository<PromiseExplanation, PromiseExplanationId, PromiseExplanationEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (deps: AggregateStages.EnvelopeStoreDependencies<PromiseExplanationEvent>)
    (commitmentRepo: Repository<CommitmentEntity, OrderCommitmentId, CommitmentEvent>)
    : PromiseExplanationAggregateApi =

    let generate (req: GeneratePromiseExplanationReq) : Task<Result<PromiseExplanation, ApplicationError>> =
        taskResult {
            let! cmd: GeneratePromiseExplanationCmd = liftValidation (ACL.toGenerateCmd req)

            let! (commitmentOpt: CommitmentEntity option) =
                commitmentRepo.Get cmd.CommitmentId
                |> TaskResult.ofTask
                |> TaskResult.mapError (fun e -> Infrastructure(Database(sprintf "%A" e)))

            let decideFn = Behaviors.generate commitmentOpt.IsSome
            let pipeline = CommandPipeline.create repo (fun (c: GeneratePromiseExplanationCmd) -> c.ExplanationId) decideFn deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    { Generate = generate }
