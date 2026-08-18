module Medhavi.Demand.PrioritizeDemand.PlanningPriorityAssignment.Capabilities

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
    { Prioritize: PrioritizePlanningEntityReq -> Task<Result<PlanningPriorityAssignment, ApplicationError>>
      Override: OverridePlanningPriorityReq -> Task<Result<PlanningPriorityAssignment, ApplicationError>> }

let private liftValidation validationResult =
    validationResult
    |> Validation.toResult
    |> Result.mapError(fun errs ->
        let domainErrs = errs |> List.map(fun e -> ("", $"{e}"))
        ApplicationError.Validation domainErrs)

let create
    (repo: Repository<PlanningPriorityAssignment, PlanningPriorityAssignmentId, PlanningPriorityEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: PrioritizationPolicy)
    (overridePolicy: PrioritizationOverridePolicy)
    (deps: EnvelopeStoreDependencies<PlanningPriorityEvent>)
    (ports: DemandPorts)
    : AggregateApi =

    let prioritize (req: PrioritizePlanningEntityReq) : Task<Result<PlanningPriorityAssignment, ApplicationError>> =
        taskResult {
            let! cmd = liftValidation(ACL.toPrioritizeCmd req)

            // Attempt to enrich missing dimensions via ports
            let! effectiveCmd =
                task {
                    let! revOpt =
                        match cmd.RevenueContribution with
                        | Some v -> Task.FromResult(Some v)
                        | None -> ports.GetRevenueContribution cmd.EntityType.AsString cmd.EntityId

                    let! stratOpt =
                        match cmd.StrategicImportance with
                        | Some v -> Task.FromResult(Some v)
                        | None -> ports.GetStrategicImportance cmd.EntityType.AsString cmd.EntityId

                    let! riskOpt =
                        match cmd.RiskExposure with
                        | Some v -> Task.FromResult(Some v)
                        | None -> ports.GetRiskExposure cmd.EntityType.AsString cmd.EntityId

                    let! contOpt =
                        match cmd.ContractualObligation with
                        | Some v -> Task.FromResult(Some v)
                        | None -> ports.GetContractualObligation cmd.EntityType.AsString cmd.EntityId

                    return
                        Ok
                            { cmd with
                                RevenueContribution = revOpt
                                StrategicImportance = stratOpt
                                RiskExposure = riskOpt
                                ContractualObligation = contOpt }
                }

            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: PrioritizePlanningEntityCmd) -> c.AssignmentId)
                    (Behaviors.prioritizePlanningEntity policy)
                    deps

            return! runPipeline pipeline publishKnowledge id effectiveCmd
        }

    let overridePriority
        (req: OverridePlanningPriorityReq)
        : Task<Result<PlanningPriorityAssignment, ApplicationError>> =
        taskResult {
            let! cmd = liftValidation(ACL.toOverrideCmd req)

            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: OverridePlanningPriorityCmd) -> c.AssignmentId)
                    (Behaviors.overridePlanningPriority overridePolicy)
                    deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    { Prioritize = prioritize
      Override = overridePriority }
