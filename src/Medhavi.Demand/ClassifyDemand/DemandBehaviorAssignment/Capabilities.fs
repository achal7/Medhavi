module Medhavi.Demand.ClassifyDemand.DemandBehaviorAssignment.Capabilities

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
    { Classify: ClassifyDemandBehaviorReq -> Task<Result<DemandBehaviorAssignment, ApplicationError>>
      Override: OverrideDemandBehaviorReq -> Task<Result<DemandBehaviorAssignment, ApplicationError>> }

let private liftValidation validationResult =
    validationResult
    |> Validation.toResult
    |> Result.mapError(fun errs ->
        let domainErrs = errs |> List.map(fun e -> ("", $"{e}"))
        ApplicationError.Validation domainErrs)

let create
    (repo: Repository<DemandBehaviorAssignment, DemandBehaviorAssignmentId, DemandBehaviorEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: ClassificationPolicy)
    (overridePolicy: ClassificationOverridePolicy)
    (deps: EnvelopeStoreDependencies<DemandBehaviorEvent>)
    : AggregateApi =

    let classify (req: ClassifyDemandBehaviorReq) : Task<Result<DemandBehaviorAssignment, ApplicationError>> =
        taskResult {
            let! cmd = liftValidation(ACL.toClassifyCmd req)

            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: ClassifyDemandBehaviorCmd) -> c.AssignmentId)
                    (Behaviors.classifyDemandBehavior policy)
                    deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    let overrideClassification
        (req: OverrideDemandBehaviorReq)
        : Task<Result<DemandBehaviorAssignment, ApplicationError>> =
        taskResult {
            let! cmd = liftValidation(ACL.toOverrideCmd req)

            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: OverrideDemandBehaviorCmd) -> c.AssignmentId)
                    (Behaviors.overrideDemandBehavior overridePolicy)
                    deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    { Classify = classify
      Override = overrideClassification }
