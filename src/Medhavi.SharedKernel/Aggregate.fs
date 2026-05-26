namespace Medhavi.SharedKernel

open Medhavi.Common.Patterns
open Medhavi.Common.Validation

type Decision<'State, 'Event> =
    { NewState: 'State
      Events: 'Event list }

/// Shared decide/evolve function signatures for event-sourced aggregates.
type Decide<'State, 'Command, 'Event> = 'Command -> 'State option -> Result<Decision<'State, 'Event>, DomainError>
type Evolve<'State, 'Event> = 'Event -> 'State option -> 'State option

/// Validator type for aggregate commands using applicative Validation functor
type CommandValidator<'Command> = 'Command -> Validation<'Command, DomainError>

/// Helper utilities to run and compose event-sourced aggregate logic.
module Aggregate =
    /// Replays a sequence of events on top of an initial state.
    let replay (evolve: Evolve<'State, 'Event>) (events: 'Event seq) : 'State option =
        Seq.fold (fun state ev -> evolve ev state) None events

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
    let private mapRepositoryErrorToApplicationError (e: RepositoryError) : ApplicationError =
        let etype = InfrastructureError.Database "Repository error"

        match e with
        | ConcurrencyConflict msg -> InfraError.Issue(etype, "Concurrency", msg, Map.empty)
        | NotFound msg -> InfraError.Issue(etype, "NotFound", msg, Map.empty)
        | StorageError msg -> InfraError.Issue(etype, "Storage", msg, Map.empty)
        |> ApplicationError.Infrastructure

    let liftCmdResult f =
        f
        >> Result.mapError ApplicationError.mapDomainError
        >> TaskResult.ofResult

    let liftCmdValidation f =
        f
        >> mapError ApplicationError.mapDomainError
        >> TaskResult.ofValidation

    let handleCommand
        (getId: 'Cmd -> 'Id)
        (repo: Repository<'Agg, 'Id, 'Event>)
        (toDomain: 'Cmd -> 'DomainCmd)
        (decide: Decide<'Agg, 'DomainCmd, 'Event>)
        (cmd: 'Cmd)
        : TaskResult<'Event list, ApplicationError> =

        let id = getId cmd
        let domainCmd = toDomain cmd

        // Morphism 1: Load aggregate
        let load (_: 'Cmd) =
            repo.Get id
            |> TaskResult.mapError mapRepositoryErrorToApplicationError

        // Morphism 2: Domain decision
        let runDecide (stateOpt: 'Agg option) =
            decide domainCmd stateOpt
            |> Result.mapError ApplicationError.mapDomainError
            |> TaskResult.ofResult

        // Morphism 3: Persist state & events, returning events on success
        let save (decision: Decision<'Agg, 'Event>) =
            repo.Save(id, decision.NewState, decision.Events)
            |> TaskResult.mapError mapRepositoryErrorToApplicationError
            |> TaskResult.map (fun _ -> decision.Events)

        // Compose the morphisms in the Kleisli Category
        let pipeline = load >=> runDecide >=> save

        pipeline cmd

    let createAggregate
        (validate: 'Command -> Validation<'Agg, DomainError>)
        (events: 'Agg -> 'Event list)
        (cmd: 'Command)
        : Result<Decision<'Agg, 'Event>, DomainError> =

        validate cmd
        |> toResult
        |> Result.map (fun agg -> { NewState = agg; Events = events agg })
        |> Result.mapError DomainError.combineValidationErrors
