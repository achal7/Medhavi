/// Aggregate Command Pipeline and Capability API for Supplier Collaboration Health
module Medhavi.Supply.CollaborateWithSuppliers.SupplierCollaborationHealth.Capabilities

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
open Policies
open Behaviors
open ACL

type SupplierCollaborationHealthAggregateApi =
    { Publish: PublishSupplierCollaborationHealthAssessmentReq -> Task<Result<SupplierCollaborationHealthAssessment, ApplicationError>> }

let create
    (repo: Repository<SupplierCollaborationHealthAssessment, SupplierCollaborationHealthAssessmentId, SupplierCollaborationHealthEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (deps: AggregateStages.EnvelopeStoreDependencies<SupplierCollaborationHealthEvent>)
    (assessmentsProvider: ScopeAssessmentsProvider)
    (policy: SupplierCollaborationHealthPolicy)
    : SupplierCollaborationHealthAggregateApi =

    let publish (req: PublishSupplierCollaborationHealthAssessmentReq) : Task<Result<SupplierCollaborationHealthAssessment, ApplicationError>> =
        taskResult {
            let! cmd = liftValidation (toPublishCmd req)

            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: PublishSupplierCollaborationHealthCmd) ->
                        SupplierCollaborationHealthAssessmentId.ofScopeAndPeriod c.AssessmentScopeId c.EvaluationPeriodId)
                    (publish assessmentsProvider policy)
                    deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    { Publish = publish }
