module internal Medhavi.Demand.DemandExplanation.CommandHandler

open System.Threading.Tasks
open Medhavi.SharedKernel.Execution
open Medhavi.SharedKernel.Failure
open Medhavi.SharedKernel.Observation
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.Demand.DemandExplanation.Model

let execute
    (repo: Repository<DemandExplanation, string, DemandExplanationEvent>)
    (publishKnowledge: ArchitecturalKnowledge -> unit)
    (cmd: DemandExplanationCommand)
    : Task<ExecutionOutcome<DemandExplanation, ApplicationError>> =

    let pipeline = CommandPipeline.create repo (fun (c: DemandExplanationCommand) -> c.ExplanationId) Decisions.decide

    CommandCapabilities.execute pipeline publishKnowledge cmd
