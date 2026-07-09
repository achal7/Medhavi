module internal Medhavi.Demand.DemandLearning.CommandHandler

open System.Threading.Tasks
open Medhavi.SharedKernel.Execution
open Medhavi.SharedKernel.Failure
open Medhavi.SharedKernel.Observation
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.Demand.DemandLearning.Model

let execute
    (repo: Repository<DemandLearning, string, DemandLearningEvent>)
    (publishKnowledge: ArchitecturalKnowledge -> unit)
    (cmd: DemandLearningCommand)
    : Task<ExecutionOutcome<DemandLearning, ApplicationError>> =

    let pipeline = CommandPipeline.create repo (fun (c: DemandLearningCommand) -> c.LearningId) Decisions.decide

    CommandCapabilities.execute pipeline publishKnowledge cmd
