/// CA-P-006 Establish Commitments Aggregate Capabilities
module Medhavi.Promise.EstablishCommitments.OrderCommitment.Capabilities

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

type CommitmentAggregateApi =
    { Establish: EstablishCommitmentReq -> Task<Result<OrderCommitment, ApplicationError>>
      Revise: ReviseCommitmentReq -> Task<Result<OrderCommitment, ApplicationError>>
      ConfirmFulfillment: ConfirmFulfillmentReq -> Task<Result<OrderCommitment, ApplicationError>>
      Cancel: CancelCommitmentReq -> Task<Result<OrderCommitment, ApplicationError>> }

let create
    (repo: Repository<OrderCommitment, OrderCommitmentId, OrderCommitmentEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: CommitmentGovernancePolicy)
    (deps: AggregateStages.EnvelopeStoreDependencies<OrderCommitmentEvent>)
    (ports: PromisePorts)
    : CommitmentAggregateApi =

    let establish (req: EstablishCommitmentReq) : Task<Result<OrderCommitment, ApplicationError>> =
        taskResult {
            let! cmd: EstablishCommitmentCmd = liftValidation (ACL.toEstablishCmd req)

            do! requireEntityExists ports.CustomerExists cmd.CustomerId "Customer" CustomerId.value
            do! requireEntityExists ports.ItemExists cmd.Item "Item" ItemId.value
            do! requireEntityExists ports.LocationExists cmd.DestinationLocation "Location" LocationId.value

            let decideFn = Behaviors.establish policy true true true
            let pipeline = CommandPipeline.create repo (fun (c: EstablishCommitmentCmd) -> c.CommitmentId) decideFn deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    let revise (req: ReviseCommitmentReq) : Task<Result<OrderCommitment, ApplicationError>> =
        taskResult {
            let! cmd: ReviseCommitmentCmd = liftValidation (ACL.toReviseCmd req)
            let decideFn = Behaviors.revise policy
            let pipeline = CommandPipeline.create repo (fun (c: ReviseCommitmentCmd) -> c.CommitmentId) decideFn deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    let confirmFulfillment (req: ConfirmFulfillmentReq) : Task<Result<OrderCommitment, ApplicationError>> =
        taskResult {
            let! cmd: ConfirmFulfillmentCmd = liftValidation (ACL.toConfirmFulfillmentCmd req)
            let decideFn = Behaviors.confirmFulfillment
            let pipeline = CommandPipeline.create repo (fun (c: ConfirmFulfillmentCmd) -> c.CommitmentId) decideFn deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    let cancel (req: CancelCommitmentReq) : Task<Result<OrderCommitment, ApplicationError>> =
        taskResult {
            let! cmd: CancelCommitmentCmd = liftValidation (ACL.toCancelCmd req)
            let decideFn = Behaviors.cancel policy
            let pipeline = CommandPipeline.create repo (fun (c: CancelCommitmentCmd) -> c.CommitmentId) decideFn deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    { Establish = establish
      Revise = revise
      ConfirmFulfillment = confirmFulfillment
      Cancel = cancel }

