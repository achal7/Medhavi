/// CA-P-007 Assess Promise Risk and Resilience — Aggregate Capabilities
module Medhavi.Promise.AssessPromiseRisk.PromiseRiskAssessment.Capabilities

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

type PromiseRiskAggregateApi =
    { Assess: AssessCommitmentRiskReq -> Task<Result<PromiseRiskAssessment, ApplicationError>>
      Escalate: EscalateRiskBreachReq -> Task<Result<PromiseRiskAssessment, ApplicationError>> }

let create
    (repo: Repository<PromiseRiskAssessment, PromiseRiskAssessmentId, PromiseRiskEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: PromiseRiskAssessmentPolicy)
    (deps: AggregateStages.EnvelopeStoreDependencies<PromiseRiskEvent>)
    (ports: PromisePorts)
    (commitmentRepo: Repository<CommitmentEntity, OrderCommitmentId, CommitmentEvent>)
    (defaultScopeId: PlanningScopeId)
    : PromiseRiskAggregateApi =

    let assess (req: AssessCommitmentRiskReq) : Task<Result<PromiseRiskAssessment, ApplicationError>> =
        taskResult {
            let! cmd: AssessCommitmentRiskCmd = liftValidation (ACL.toAssessCmd req)

            let! itemExists = protect (ports.ItemExists cmd.Item)
            let! (commitmentOpt: CommitmentEntity option) =
                commitmentRepo.Get cmd.CommitmentId
                |> TaskResult.ofTask
                |> TaskResult.mapError (fun e -> Infrastructure(Database(sprintf "%A" e)))

            let! disruptionSignals = protect (ports.GetActiveDisruptionSignals defaultScopeId)

            let committedDeliveryDate =
                commitmentOpt
                |> Option.map (fun c -> c.LatestRevision.CommittedDeliveryDate)
                |> Option.defaultValue cmd.AssessmentTime

            let decideFn = Behaviors.assess policy commitmentOpt.IsSome itemExists committedDeliveryDate disruptionSignals
            let pipeline = CommandPipeline.create repo (fun (c: AssessCommitmentRiskCmd) -> c.AssessmentId) decideFn deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    let escalate (req: EscalateRiskBreachReq) : Task<Result<PromiseRiskAssessment, ApplicationError>> =
        taskResult {
            let! cmd: EscalateRiskBreachCmd = liftValidation (ACL.toEscalateCmd req)
            let decideFn = Behaviors.escalate policy
            let pipeline = CommandPipeline.create repo (fun (c: EscalateRiskBreachCmd) -> c.AssessmentId) decideFn deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    { Assess = assess
      Escalate = escalate }

