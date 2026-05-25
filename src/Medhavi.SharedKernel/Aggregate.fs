namespace Medhavi.SharedKernel

/// Shared decide/evolve function signatures for event-sourced aggregates.
type Decide<'State, 'Command, 'Event> = 'Command -> 'State option -> Result<'State * 'Event list, DomainError list>
type Evolve<'State, 'Event> = 'Event -> 'State option -> 'State option

/// Strict aggregate command API signature for state-based aggregates.
type Handle<'State, 'Command, 'Event> = 'Command -> 'State -> Result<'State * 'Event list, DomainError>

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
        : Result<'State * 'Event list, DomainError list> =
        let currentState = replay evolve history
        decide command currentState
