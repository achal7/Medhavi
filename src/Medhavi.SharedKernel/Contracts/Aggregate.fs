module Medhavi.SharedKernel.Contracts.Aggregate

open Medhavi.Common.Patterns
open Medhavi.Common.Validation
open Medhavi.SharedKernel.Failure
open Medhavi.SharedKernel

open DecisionTrace

/// Versioned aggregate for optimistic concurrency
type VersionedAggregate<'Aggregate> = { Aggregate: 'Aggregate; Version: int }

type Decision<'State, 'Event> =
    { NewState: 'State
      Events: 'Event list
      Trace: DecisionTrace option }

/// Shared decide/evolve function signatures for event-sourced aggregates.
type Decide<'State, 'Command, 'Event> = 'Command -> 'State option -> Result<Decision<'State, 'Event>, DomainError>
type Evolve<'State, 'Event> = 'Event -> 'State option -> 'State option

/// Validator type for aggregate commands using applicative Validation functor
type CommandValidator<'Command> = 'Command -> Validation<'Command, DomainError>

/// Replays a sequence of events on top of an initial state.
let replay (evolve: Evolve<'State, 'Event>) (events: 'Event seq) : 'State option =
    Seq.fold (fun state ev -> evolve ev state) None events

let buildDecision<'State, 'Event> (evolve: Evolve<'State, 'Event>) stateOpt events traceOpt =
    let newState = events |> List.fold (fun s ev -> evolve ev s) stateOpt

    { NewState = newState |> Option.defaultWith(fun () -> failwith "Aggregate state must exist after applying events")
      Events = events
      Trace = traceOpt }

/// Error types for repository operations
type RepositoryError =
    | ConcurrencyConflict of string
    | NotFound of string
    | StorageError of string

/// Pluggable Repository port defined as a record-of-functions.
type Repository<'Aggregate, 'Id, 'Event> =
    { Get: 'Id -> TaskResult<'Aggregate option, RepositoryError>
      Save: 'Id * 'Aggregate * 'Event list -> TaskResult<unit, RepositoryError>
      SaveBatch: ('Id * 'Aggregate * 'Event list) list -> TaskResult<unit, RepositoryError>
      Delete: 'Id -> TaskResult<unit, RepositoryError>
      GetEvents: 'Id -> TaskResult<'Event list, RepositoryError>
      GetEventsByType: ('Event -> bool) -> TaskResult<'Event list, RepositoryError>
      GetAll: unit -> TaskResult<'Aggregate list, RepositoryError> }

/// Executes a command against an aggregate's historical events.
let handleCommandFromHistory
    (decide: Decide<'State, 'Command, 'Event>)
    (evolve: Evolve<'State, 'Event>)
    (command: 'Command)
    (history: 'Event seq)
    : Result<Decision<'State, 'Event>, DomainError> =
    let currentState = replay evolve history
    decide command currentState

//Natural transformation from RepositoryError to InfraError
let mapRepositoryErrorToApplicationError (e: RepositoryError) : ApplicationError =
    match e with
    | ConcurrencyConflict msg -> InfrastructureError.Database msg
    | RepositoryError.NotFound msg -> InfrastructureError.Database msg
    | StorageError msg -> InfrastructureError.Database msg
    |> ApplicationError.Infrastructure

let liftCmdResult f = f >> Result.mapError ApplicationError.fromDomainError >> TaskResult.ofResult

let liftCmdValidation f =
    f
    >> toResult
    >> Result.mapError(DomainError.combineValidationErrors >> ApplicationError.fromDomainError)
    >> TaskResult.ofResult

let handleCommand
    (getId: 'Cmd -> 'Id)
    (repo: Repository<'Agg, 'Id, 'Event>)
    (toDomain: 'Cmd -> 'DomainCmd)
    (decide: Decide<'Agg, 'DomainCmd, 'Event>)
    (cmd: 'Cmd)
    : TaskResult<Decision<'Agg, 'Event>, ApplicationError> =

    let id = getId cmd
    let domainCmd = toDomain cmd

    // Morphism 1: Load aggregate
    let load (_: 'Cmd) = repo.Get id |> TaskResult.mapError mapRepositoryErrorToApplicationError

    // Morphism 2: Domain decision
    let runDecide (stateOpt: 'Agg option) =
        decide domainCmd stateOpt |> Result.mapError ApplicationError.fromDomainError |> TaskResult.ofResult

    // Morphism 3: Persist state & events, returning events on success
    let save (decision: Decision<'Agg, 'Event>) =
        repo.Save(id, decision.NewState, decision.Events)
        |> TaskResult.mapError mapRepositoryErrorToApplicationError
        |> TaskResult.map(fun _ ->
            for ev in decision.Events do
                DomainEventBus.Publish(ev)

            decision)

    // Compose the morphisms in the Kleisli Category
    let pipeline = load >=> runDecide >=> save

    pipeline cmd
//
// let createAggregate
//     (validate: 'Command -> Validation<'Agg, DomainError>)
//     (events: 'Agg -> 'Event list)
//     (cmd: 'Command)
//     : Result<Decision<'Agg, 'Event>, DomainError> =
//
//     validate cmd
//     |> toResult
//     |> Result.map(fun agg -> { NewState = agg; Events = events agg })
//     |> Result.mapError DomainError.combineValidationErrors
