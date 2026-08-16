module Medhavi.Foundation.Execution.CapabilityPrimitives

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation

/// Lifts a Validation into a TaskResult, mapping DomainErrors to ApplicationErrors.
let liftValidation (validation: Validation<'T, DomainError>) : TaskResult<'T, ApplicationError> =
    validation
    |> TaskResult.ofValidation
    |> TaskResult.mapError(DomainError.combineValidationErrors >> ApplicationError.Domain)

/// Protects an async port operation, catching exceptions and mapping them to ApplicationErrors.
let protect (portOp: Task<'T>) : TaskResult<'T, ApplicationError> =
    portOp |> TaskResult.ofTaskValue |> TaskResult.catch ApplicationError.fromException

/// Ensures an entity exists via a port, returning a NotFound DomainError if it does not.
let requireEntityExists
    (port: 'Id -> Task<bool>)
    (id: 'Id)
    (entityName: string)
    (idValue: 'Id -> string)
    : TaskResult<unit, ApplicationError> =
    taskResult {
        let! exists = protect(port id)

        if exists then
            return ()
        else
            return! TaskResult.fail(Domain(DomainError.notFound(entityName, (idValue id))))
    }

/// Executes a command pipeline and maps the ExecutionOutcome to a TaskResult.
let runPipeline
    (repo: Repository<'State, string, 'Event>)
    (publishKnowledge: KnowledgeRepresentation)
    (deps: AggregateStages.EnvelopeStoreDependencies<'Event>)
    (id: string)
    (decider: 'Cmd -> 'State option -> Result<Decision<'State, 'Event>, DomainError>)
    (cmd: 'Cmd)
    : TaskResult<'State, ApplicationError> =
    taskResult {
        let getIdentity = fun _ -> id
        let pipeline = CommandPipeline.create repo getIdentity decider deps
        let! outcome = CommandCapabilities.execute pipeline publishKnowledge cmd

        match outcome with
        | Completed state -> return state
        | Failed err -> return! TaskResult.fail err
        | Cancelled -> return! TaskResult.fail(Infrastructure(Timeout "Pipeline Cancelled"))
    }
