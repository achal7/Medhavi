module internal Medhavi.Demand.PlanningClassificationAssignment.CommandHandler

open System.Threading.Tasks
open Medhavi.Common.Validation
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Execution
open Medhavi.SharedKernel.Failure
open Medhavi.SharedKernel.Observation
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.Demand
open Medhavi.Demand.PlanningClassificationAssignment.ACL
open Medhavi.Demand.PlanningClassificationAssignment.Model

// Traceability: Coordinates command execution pipeline for SE-D-036 (Planning Classification Assignment aggregate)
// Exposes execution corridor mapping DomainCommand -> Task<ExecutionOutcome<PlanningClassificationAssignment, ApplicationError>>

let execute
    (repo: Repository<PlanningClassificationAssignment, string, PlanningClassificationEvent>)
    (publishKnowledge: ArchitecturalKnowledge -> unit)
    (cmd: PlanningClassificationCommand)
    : Task<ExecutionOutcome<PlanningClassificationAssignment, ApplicationError>> =

    // Command Pipeline maps to Decisions.decide state transition loop
    let pipeline = CommandPipeline.create repo (fun (c: PlanningClassificationCommand) -> c.AssignmentId) Decisions.decide

    CommandCapabilities.execute pipeline publishKnowledge cmd
