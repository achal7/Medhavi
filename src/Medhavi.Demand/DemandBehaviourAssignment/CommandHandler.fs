module internal Medhavi.Demand.DemandBehaviourAssignment.CommandHandler

open System.Threading.Tasks
open Medhavi.Common.Validation
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Execution
open Medhavi.SharedKernel.Failure
open Medhavi.SharedKernel.Observation
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.Demand
open Medhavi.Demand.DemandBehaviourAssignment.ACL
open Medhavi.Demand.DemandBehaviourAssignment.Model

// Traceability: Coordinates command execution pipeline for SE-D-037 (Demand Behaviour Assignment aggregate)
// Exposes execution corridor mapping DomainCommand -> Task<ExecutionOutcome<DemandBehaviourAssignment, ApplicationError>>

let execute
    (repo: Repository<DemandBehaviourAssignment, string, DemandBehaviourAssignmentEvent>)
    (publishKnowledge: ArchitecturalKnowledge -> unit)
    (cmd: DemandBehaviourAssignmentCommand)
    : Task<ExecutionOutcome<DemandBehaviourAssignment, ApplicationError>> =

    // Command Pipeline maps to Decisions.decide state transition loop
    let pipeline = CommandPipeline.create repo (fun (c: DemandBehaviourAssignmentCommand) -> c.AssignmentId) Decisions.decide

    CommandCapabilities.execute pipeline publishKnowledge cmd
