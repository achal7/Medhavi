/// Child Aggregate Capabilities for Planning Classification Assignment
/// Returns pure domain entity: PlanningClassificationAssignment
module Medhavi.Demand.SegmentDemand.PlanningClassificationAssignment.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Execution.CommandCapabilities
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.SemanticModel
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Model
open Policies

type AggregateApi =
    { Classify: ClassifyPlanningEntityReq -> Task<Result<PlanningClassificationAssignment, ApplicationError>>
      Override: OverridePlanningClassificationReq -> Task<Result<PlanningClassificationAssignment, ApplicationError>> }

let private liftValidation validationResult =
    validationResult
    |> Validation.toResult
    |> Result.mapError (fun errs ->
        let domainErrs = errs |> List.map (fun e -> ("", $"{e}"))
        ApplicationError.Validation domainErrs)

let create
    (repo: Repository<PlanningClassificationAssignment, PlanningClassificationAssignmentId, PlanningClassificationEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: SegmentationPolicy)
    (overridePolicy: SegmentationOverridePolicy)
    (deps: EnvelopeStoreDependencies<PlanningClassificationEvent>)
    (ports: DemandPorts)
    : AggregateApi =

    let classify (req: ClassifyPlanningEntityReq) : Task<Result<PlanningClassificationAssignment, ApplicationError>> =
        taskResult {
            let! cmd = liftValidation (ACL.toClassifyCmd req)

            // If analog item is not explicitly provided in request for an Item entity, attempt lookup via port
            let! effectiveCmd =
                task {
                    match cmd.EntityType, cmd.AnalogItemId with
                    | Item, None ->
                        match ItemId.create cmd.EntityId with
                        | Ok itemId ->
                            let! analogOpt = ports.GetAnalogItem itemId
                            return Ok { cmd with AnalogItemId = analogOpt }
                        | Error _ -> return Ok cmd
                    | _ -> return Ok cmd
                }

            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: ClassifyPlanningEntityCmd) -> c.AssignmentId)
                    (Behaviors.classifyPlanningEntity policy)
                    deps

            return! runPipeline pipeline publishKnowledge id effectiveCmd
        }

    let overrideClassification (req: OverridePlanningClassificationReq) : Task<Result<PlanningClassificationAssignment, ApplicationError>> =
        taskResult {
            let! cmd = liftValidation (ACL.toOverrideCmd req)

            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: OverridePlanningClassificationCmd) -> c.AssignmentId)
                    (Behaviors.overrideClassification overridePolicy)
                    deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    { Classify = classify
      Override = overrideClassification }
