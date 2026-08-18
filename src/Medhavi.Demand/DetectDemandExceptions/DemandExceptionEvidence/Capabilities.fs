/// Child Aggregate Capabilities for Demand Exception Evidence
/// Returns pure domain entity: DemandExceptionEvidenceAggregate
module Medhavi.Demand.DetectDemandExceptions.DemandExceptionEvidence.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Execution.CommandCapabilities
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Model
open Policies

type AggregateApi =
    { Evaluate: EvaluateDemandExceptionReq -> Task<Result<DemandExceptionEvidenceAggregate, ApplicationError>> }

let private liftValidation validationResult =
    validationResult
    |> Validation.toResult
    |> Result.mapError (fun errs ->
        let domainErrs = errs |> List.map (fun e -> ("", $"{e}"))
        ApplicationError.Validation domainErrs)

let create
    (repo: Repository<DemandExceptionEvidenceAggregate, DemandExceptionEvidenceId, DemandExceptionEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: DemandExceptionEvidencePolicy)
    (deps: EnvelopeStoreDependencies<DemandExceptionEvent>)
    : AggregateApi =

    let evaluate (req: EvaluateDemandExceptionReq) : Task<Result<DemandExceptionEvidenceAggregate, ApplicationError>> =
        taskResult {
            let! cmd = liftValidation (ACL.toEvaluateCmd req)

            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: EvaluateDemandExceptionCmd) -> c.EvidenceId)
                    (Behaviors.evaluateDemandException policy)
                    deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    { Evaluate = evaluate }
