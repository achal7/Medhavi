/// Aggregate Command Pipeline and Capability API for Supplier Commitment Assessment
module Medhavi.Supply.CollaborateWithSuppliers.SupplierCommitmentAssessment.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution
open Medhavi.Foundation.Execution.CommandCapabilities
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.SemanticModel
open Medhavi.Contracts.Supply
open Model
open Policies
open Behaviors
open ACL

type SupplierCommitmentAssessmentAggregateApi =
    { Update: UpdateSupplierCommitmentAssessmentReq -> Task<Result<SupplierCommitmentAssessment, ApplicationError>> }

let create
    (repo: Repository<SupplierCommitmentAssessment, SupplierId, SupplierCommitmentAssessmentEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (deps: AggregateStages.EnvelopeStoreDependencies<SupplierCommitmentAssessmentEvent>)
    (factsProvider: SupplierAssessmentFactsProvider)
    (policy: SupplierCollaborationPolicy)
    : SupplierCommitmentAssessmentAggregateApi =

    let update (req: UpdateSupplierCommitmentAssessmentReq) : Task<Result<SupplierCommitmentAssessment, ApplicationError>> =
        taskResult {
            let! cmd = liftValidation (toUpdateCmd req)

            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: UpdateSupplierCommitmentAssessmentCmd) -> c.SupplierId)
                    (update factsProvider policy)
                    deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    { Update = update }
