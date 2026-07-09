module internal Medhavi.Demand.DemandObservation.CommandHandler

open System.Threading.Tasks
open Medhavi.SharedKernel.Execution
open Medhavi.SharedKernel.Failure
open Medhavi.SharedKernel.Observation
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.Demand.DemandObservation.Model

// Traceability: Coordinates command execution pipeline for SE-D-001 (Demand Observation)
// Exposes execution corridor mapping DomainCommand -> Task<ExecutionOutcome<DemandObservation, ApplicationError>>

let execute
    (repo: Repository<DemandObservation, string, ObservationEvent>)
    (publishKnowledge: ArchitecturalKnowledge -> unit)
    (cmd: ObservationCommand)
    : Task<ExecutionOutcome<DemandObservation, ApplicationError>> =

    // Command Pipeline maps to Decisions.decide state transition loop
    let pipeline = CommandPipeline.create repo (fun (c: ObservationCommand) -> c.AssignmentId) Decisions.decide

    CommandCapabilities.execute pipeline publishKnowledge cmd
