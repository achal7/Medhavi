module Medhavi.Foundation.Execution.CommandCapabilities

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution
open Medhavi.Foundation.ExecutionContext
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

// ============================================================================
// 2. EXECUTION ENGINE BRIDGES (Pipeline Category -> Railway Category)
// ============================================================================

/// LOW-LEVEL ENGINE RUNNER (The Anamorphism)
/// Executes a fully constructed pipeline with the default strategy, ambient execution context, and knowledge publishing.
/// This is the single source of truth for invoking the ExecutionRunner.
let execute
    (pipeline:
        ExecutionPipeline<
            Execution<'Agg option, CommandContext<'Cmd, 'Agg, 'Event>>,
            ExecutionOutcome<'Agg, ApplicationError>
         >)
    (publishKnowledge: KnowledgeRepresentation)
    (domainCmd: 'Cmd)
    : Task<ExecutionOutcome<'Agg, ApplicationError>> =

    let ctx = ExecutionContextHolder.TryGet() |> Option.defaultValue(ExecutionContext.create())

    let initialModel =
        { State = None
          Context =
            { Command = domainCmd
              ExecutionCtx = ctx
              AggregateVersion = None
              Decision = None } }

    ExecutionRunner.execute
        ExecutionStrategies.defaultStrategy
        pipeline
        initialModel
        publishKnowledge
        3
        (TimeSpan.FromMilliseconds 100.0)
        CancellationToken.None

/// CORE PIPELINE BRIDGE (runPipeline)
/// Executes a pre-built pipeline and maps the ExecutionOutcome to a TaskResult.
let runPipeline<'Cmd, 'Agg, 'Event, 'Contract>
    (pipeline:
        ExecutionPipeline<
            Execution<'Agg option, CommandContext<'Cmd, 'Agg, 'Event>>,
            ExecutionOutcome<'Agg, ApplicationError>
         >)
    (publishKnowledge: KnowledgeRepresentation)
    (mapSuccess: 'Agg -> 'Contract)
    (cmd: 'Cmd)
    : TaskResult<'Contract, ApplicationError> =

    taskResult {
        // The taskResult builder natively binds Task<'T>, lifting it into the Railway
        let! outcome = execute pipeline publishKnowledge cmd

        match outcome with
        | Completed agg -> return mapSuccess agg
        | Failed err -> return! TaskResult.fail err
        | Cancelled ->
            return! TaskResult.fail(ApplicationError.Infrastructure(InfrastructureError.Timeout "Pipeline Cancelled"))
    }

// ============================================================================
// 3. HIGH-LEVEL ORCHESTRATORS
// ============================================================================

/// SIMPLE CAPABILITY ORCHESTRATOR (runCapability)
/// Handles the full application lifecycle: validate → execute pipeline → map outcome to API boundary.
let runCapability<'Req, 'Cmd, 'Agg, 'Event, 'Contract, 'ApiErr>
    (validate: 'Req -> Validation<'Cmd, DomainError>)
    (pipeline:
        ExecutionPipeline<
            Execution<'Agg option, CommandContext<'Cmd, 'Agg, 'Event>>,
            ExecutionOutcome<'Agg, ApplicationError>
         >)
    (publishKnowledge: KnowledgeRepresentation)
    (mapSuccess: 'Agg -> 'Contract)
    (mapError: ApplicationError -> 'ApiErr)
    (req: 'Req)
    : TaskResult<'Contract, 'ApiErr> =

    // 1. Validate request synchronously and lift to TaskResult
    let cmdResult =
        validate req
        |> TaskResult.ofValidation
        |> TaskResult.mapError(DomainError.combineValidationErrors >> ApplicationError.Domain)

    // 2. Execute pipeline and map both success and error to the API boundary types
    taskResult {
        let! cmd = cmdResult
        return! runPipeline pipeline publishKnowledge mapSuccess cmd
    }
    |> TaskResult.mapError mapError

/// AGGREGATE COMMAND CONVENIENCE WRAPPER (runAggregateCommand)
/// Builds the pipeline on the fly from the repository and decider, executes it, and returns the pure Aggregate state.
/// The domain developer maps to the API Contract/ApiError at the very edge of their capability.
let runAggregateCommand<'Cmd, 'Agg, 'Id, 'Event when 'Event : not null and 'Cmd : not null and 'Id: not null>
    (repo: Repository<'Agg, 'Id, 'Event>)
    (publishKnowledge: KnowledgeRepresentation)
    (deps: AggregateStages.EnvelopeStoreDependencies<'Event>)
    (getId: 'Cmd -> 'Id)
    (decider: 'Cmd -> 'Agg option -> Result<Decision<'Agg, 'Event>, DomainError>)
    (cmd: 'Cmd)
    : TaskResult<'Agg, ApplicationError> =

    let pipeline = CommandPipeline.create repo getId decider deps
    // We use the F# `id` function as the mapSuccess parameter, returning the pure Aggregate state
    runPipeline pipeline publishKnowledge id cmd
