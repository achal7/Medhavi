/// Aggregate Command Pipeline and Capability API for Supplier Commitment
module Medhavi.Supply.CollaborateWithSuppliers.SupplierCommitment.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution
open Medhavi.Foundation.Execution.CommandCapabilities
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Model
open Behaviors
open ACL

type SupplierCommitmentAggregateApi =
    { RecordRequest: RecordEnterpriseRequestReq -> Task<Result<SupplierCommitment, ApplicationError>>
      RecordResponse: RecordSupplierResponseReq -> Task<Result<SupplierCommitment, ApplicationError>> }

let create
    (repo: Repository<SupplierCommitment, SupplierCommitmentId, SupplierCommitmentEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (deps: AggregateStages.EnvelopeStoreDependencies<SupplierCommitmentEvent>)
    (factsProvider: SupplierEvaluationFactsProvider)
    : SupplierCommitmentAggregateApi =

    let recordRequest (req: RecordEnterpriseRequestReq) : Task<Result<SupplierCommitment, ApplicationError>> =
        taskResult {
            let! cmd = liftValidation (toRecordRequestCmd req)

            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: RecordEnterpriseRequestCmd) -> SupplierCommitmentId.ofComposite c.SupplierId c.ItemId c.RequiredDate)
                    recordRequest
                    deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    let recordResponse (req: RecordSupplierResponseReq) : Task<Result<SupplierCommitment, ApplicationError>> =
        taskResult {
            let! cmd = liftValidation (toRecordResponseCmd req)

            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: RecordSupplierResponseCmd) -> SupplierCommitmentId.ofComposite c.SupplierId c.ItemId c.RequiredDate)
                    (recordResponse factsProvider)
                    deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    { RecordRequest = recordRequest
      RecordResponse = recordResponse }
