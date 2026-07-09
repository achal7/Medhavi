module internal Medhavi.Demand.EnterpriseDemandPicture.CommandHandler

open System.Threading.Tasks
open Medhavi.Common.Validation
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Execution
open Medhavi.SharedKernel.Failure
open Medhavi.SharedKernel.Observation
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.Demand
open Medhavi.Demand.EnterpriseDemandPicture.ACL
open Medhavi.Demand.EnterpriseDemandPicture.Model

// Traceability: Coordinates command execution pipeline for SE-D-003 (Enterprise Demand Picture)
// Exposes execution corridor mapping DomainCommand -> Task<ExecutionOutcome<EnterpriseDemandPicture, ApplicationError>>

let execute
    (repo: Repository<EnterpriseDemandPicture, string, EdpEvent>)
    (publishKnowledge: ArchitecturalKnowledge -> unit)
    (cmd: EdpCommand)
    : Task<ExecutionOutcome<EnterpriseDemandPicture, ApplicationError>> =

    // Command Pipeline maps to Decisions.decide state transition loop
    let pipeline = CommandPipeline.create repo (fun (c: EdpCommand) -> c.AssignmentId) Decisions.decide

    CommandCapabilities.execute pipeline publishKnowledge cmd
