/// CA-P-010 Measure Promise Performance & Service Quality — Aggregate Capabilities
module Medhavi.Promise.MeasurePerformance.PromiseQualityAssessment.Capabilities

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

type PromiseQualityAggregateApi =
    { AuditQuality: AuditCommitmentQualityReq -> Task<Result<PromiseQualityAssessment, ApplicationError>>
      RecordDispute: RecordPromiseDisputeReq -> Task<Result<PromiseQualityAssessment, ApplicationError>> }

let create
    (repo: Repository<PromiseQualityAssessment, PromiseQualityAssessmentId, CommitmentQualityEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: SlaMeasurementPolicy)
    (deps: AggregateStages.EnvelopeStoreDependencies<CommitmentQualityEvent>)
    : PromiseQualityAggregateApi =

    let auditQuality (req: AuditCommitmentQualityReq) : Task<Result<PromiseQualityAssessment, ApplicationError>> =
        taskResult {
            let! cmd = liftValidation (ACL.toAuditCmd req)
            let decideFn = Behaviors.audit policy true
            let pipeline = CommandPipeline.create repo (fun (c: AuditFulfilledCommitmentCmd) -> c.AssessmentId) decideFn deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    let recordDispute (req: RecordPromiseDisputeReq) : Task<Result<PromiseQualityAssessment, ApplicationError>> =
        taskResult {
            let! cmd = liftValidation (ACL.toDisputeCmd req)
            let decideFn = Behaviors.dispute policy
            let pipeline = CommandPipeline.create repo (fun (c: RecordPromiseDisputeCmd) -> c.AssessmentId) decideFn deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    { AuditQuality = auditQuality
      RecordDispute = recordDispute }
