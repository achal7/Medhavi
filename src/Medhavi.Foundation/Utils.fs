module Medhavi.Foundation.Helpers

open System.Threading
open Microsoft.Extensions.Logging
open Medhavi.Common
open Medhavi.Foundation.Contracts.Aggregate
open Medhavi.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Common.Validation
open System.Threading.Tasks
open Medhavi.Foundation.Execution

/// Execute an operation returning TaskResult<'T,'TError> with the full Medhavi retry machinery.
/// All settings (max attempts, delays, backoff) are taken from the given RetryConfig.
let executeTaskResult
    (config: RetryConfig)
    (operation: CancellationToken -> int -> TaskResult<'T, 'TError>)
    (logger: ILogger)
    (ct: CancellationToken)
    (cancellationError: 'TError)
    : TaskResult<'T, 'TError> =
    // The operation is already of the shape expected by executeWithRetry.
    executeWithRetry operation logger (Some config) ct (fun () -> cancellationError)

let repoToApiError (err: RepositoryError) : ApiError = err |> mapToApplicationError |> ApplicationError.mapToApiError

let liftValidation (validation: Validation<'T, DomainError>) : TaskResult<'T, ApiError> =
    validation
    |> TaskResult.ofValidation
    |> TaskResult.mapError(DomainError.combineValidationErrors >> ApplicationError.mapToApiErrorFromDomain)

let protect (portOp: Task<'T>) : TaskResult<'T, ApiError> =
    portOp
    |> TaskResult.ofTaskValue
    |> TaskResult.catch(ApplicationError.fromException >> ApplicationError.mapToApiError)

let requireEntityExists
    (port: 'Id -> Task<bool>)
    (id: 'Id)
    (entityName: string)
    (idValue: 'Id -> string)
    : TaskResult<unit, ApiError> =
    taskResult {
        let! exists = protect(port id)

        if exists then
            return ()
        else
            return!
                TaskResult.fail(
                    DomainError.notFound(entityName, idValue id) |> ApplicationError.mapToApiErrorFromDomain
                )
    }

let runPipeline
    (repo: Repository<'State, string, 'Event>)
    (publishKnowledge: Observation.KnowledgeRepresentation)
    (id: string)
    //(getIdentity: 'Cmd -> string)
    (decider: 'Cmd -> 'State option -> Result<Decision<'State, 'Event>, DomainError>)
    (cmd: 'Cmd)
    : TaskResult<'State, ApiError> =
    let getIdentity = fun _ -> id
    let pipeline = CommandPipeline.create repo getIdentity decider
    CommandCapabilities.execute pipeline publishKnowledge cmd |> ExecutionOutcome.toTaskResult
