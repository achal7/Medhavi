module internal Medhavi.Demand.PlanningPriorityAssignment.CommandHandler

open System.Threading.Tasks
open Medhavi.SharedKernel.Execution
open Medhavi.SharedKernel.Failure
open Medhavi.SharedKernel.Observation
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.Demand.PlanningPriorityAssignment.Model

// Traceability: Coordinates command execution pipeline for SE-D-038 (Planning Priority Assignment aggregate)
// Exposes execution corridor mapping DomainCommand -> Task<ExecutionOutcome<PlanningPriorityAssignment, ApplicationError>>

let execute
    (repo: Repository<PlanningPriorityAssignment, string, PlanningPriorityEvent>)
    (publishKnowledge: ArchitecturalKnowledge -> unit)
    (cmd: PlanningPriorityCommand)
    : Task<ExecutionOutcome<PlanningPriorityAssignment, ApplicationError>> =

    // Command Pipeline maps to Decisions.decide state transition loop
    let pipeline = CommandPipeline.create repo (fun (c: PlanningPriorityCommand) -> c.AssignmentId) Decisions.decide

    CommandCapabilities.execute pipeline publishKnowledge cmd
