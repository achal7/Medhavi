namespace Medhavi.SharedKernel

open Medhavi.Common.Validator

/// Shared decide/evolve function signatures for event-sourced aggregates.
type Decide<'State, 'Command, 'Event> = 'Command -> 'State option -> Result<'State * 'Event list, DomainError>
type Evolve<'State, 'Event> = 'Event -> 'State option -> 'State option

/// Validator type for aggregate commands using applicative Validation functor
type CommandValidator<'Command> = 'Command -> Validation<'Command, DomainError>

/// Helper utilities to run and compose event-sourced aggregate logic.
module Aggregate =
    /// Replays a sequence of events on top of an initial state.
    let replay (evolve: Evolve<'State, 'Event>) (events: 'Event seq) : 'State option =
        Seq.fold (fun state ev -> evolve ev state) None events

    /// Executes a command against an aggregate's historical events.
    let handleCommand
        (decide: Decide<'State, 'Command, 'Event>)
        (evolve: Evolve<'State, 'Event>)
        (command: 'Command)
        (history: 'Event seq)
        : Result<'State * 'Event list, DomainError> =
        let currentState = replay evolve history
        decide command currentState

    /// Combines multiple validation DomainError instances into a single consolidated DomainError.
    let combineValidationErrors (errors: DomainError list) : DomainError =
        match errors with
        | [] -> DomainError.validation "Validation failed with no specified details"
        | [single] -> single
        | _ ->
            let messages = errors |> List.map (fun e -> e.Message)
            let combinedMessage = "Command validation failed: " + String.concat "; " messages
            let data = 
                errors 
                |> List.mapi (fun idx e -> $"error_{idx}", box e.Message) 
                |> Map.ofList
            DomainError.validationWith combinedMessage data

    /// Validates an incoming command before replaying history and executing decision logic.
    /// If validation fails, short-circuits and returns a consolidated DomainError.
    let handleCommandWithValidation
        (validate: CommandValidator<'Command>)
        (decide: Decide<'State, 'Command, 'Event>)
        (evolve: Evolve<'State, 'Event>)
        (command: 'Command)
        (history: 'Event seq)
        : Result<'State * 'Event list, DomainError> =
        match validate command with
        | Valid validatedCommand ->
            handleCommand decide evolve validatedCommand history
        | Invalid errors ->
            Error (combineValidationErrors errors)
