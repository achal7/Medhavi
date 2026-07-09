module internal Medhavi.Demand.ForecastPublication.CommandHandler

open System.Threading.Tasks
open Medhavi.SharedKernel.Execution
open Medhavi.SharedKernel.Failure
open Medhavi.SharedKernel.Observation
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.Demand.ForecastPublication.Model

// Traceability: Coordinates command execution pipeline for SE-D-035 (Forecast Publication)
// Exposes execution corridor mapping DomainCommand -> Task<ExecutionOutcome<ForecastPublication, ApplicationError>>

let execute
    (repo: Repository<ForecastPublication, string, ForecastPublicationEvent>)
    (publishKnowledge: ArchitecturalKnowledge -> unit)
    (cmd: ForecastPublicationCommand)
    : Task<ExecutionOutcome<ForecastPublication, ApplicationError>> =

    // Command Pipeline maps to Decisions.decide state transition loop
    let pipeline = CommandPipeline.create repo (fun (c: ForecastPublicationCommand) -> c.PublicationId) Decisions.decide

    CommandCapabilities.execute pipeline publishKnowledge cmd
