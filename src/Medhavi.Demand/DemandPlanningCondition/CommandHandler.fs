module internal Medhavi.Demand.DemandPlanningCondition.CommandHandler

open System.Threading.Tasks
open Medhavi.SharedKernel.Execution
open Medhavi.SharedKernel.Failure
open Medhavi.SharedKernel.Observation
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.Demand.DemandPlanningCondition.Model

let execute
    (repo: Repository<DemandPlanningCondition, string, DemandPlanningConditionEvent>)
    (publishKnowledge: ArchitecturalKnowledge -> unit)
    (cmd: DemandPlanningConditionCommand)
    : Task<ExecutionOutcome<DemandPlanningCondition, ApplicationError>> =

    let pipeline =
        CommandPipeline.create repo (fun (c: DemandPlanningConditionCommand) -> c.ConditionId) Decisions.decide

    CommandCapabilities.execute pipeline publishKnowledge cmd
