module internal Medhavi.Demand.PlanningScope.CommandHandler

open System.Threading.Tasks
open Medhavi.SharedKernel.Execution
open Medhavi.SharedKernel.Failure
open Medhavi.SharedKernel.Observation
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.Demand.PlanningScope.Model

// Traceability: Coordinates command execution pipeline for SE-D-002 (Planning Scope)
// Exposes execution corridor mapping DomainCommand -> Task<ExecutionOutcome<PlanningScope, ApplicationError>>

let execute
    (repo: Repository<PlanningScope, string, PlanningScopeEvent>)
    (publishKnowledge: ArchitecturalKnowledge -> unit)
    (cmd: PlanningScopeCommand)
    : Task<ExecutionOutcome<PlanningScope, ApplicationError>> =

    // Command Pipeline maps to Decisions.decide state transition loop
    let pipeline = CommandPipeline.create repo (fun (c: PlanningScopeCommand) -> c.AssignmentId) Decisions.decide

    CommandCapabilities.execute pipeline publishKnowledge cmd
