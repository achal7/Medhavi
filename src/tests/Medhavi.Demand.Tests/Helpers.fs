module Medhavi.Demand.Tests.Helpers

open System
open System.Diagnostics
open System.Threading.Tasks
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Failure
open Medhavi.Infrastructure.Projections

/// Execute a domain command using decide, persist events, and return
/// the new aggregate state together with the emitted events.
let runCommand
    (repo: Repository<'Agg, 'Id, 'Event>)
    (decide: Decide<'Agg, 'Cmd, 'Event>)
    (getId: 'Cmd -> 'Id)
    (initialState: 'Agg option)
    (command: 'Cmd)
    : Task<Result<'Agg * 'Event list, DomainError>> =
    task {
        let decision = decide command initialState

        match decision with
        | Error e -> return Error e
        | Ok d ->
            let id = getId command
            let! _ = repo.Save(id, d.NewState, d.Events)
            return Ok(d.NewState, d.Events)
    }

let requireDecisionTrace (decision: Decision<'State, 'Event>) =
    match decision.Trace with
    | None -> failwith "Expected a DecisionTrace but none was present"
    | Some trace -> trace

let waitForProjection (timeout: TimeSpan) (agent: ProjectionAgent<'State,'Event>) (predicate: 'State -> bool) =
    task {
        let sw = Stopwatch.StartNew()
        let mutable found = false
        while sw.Elapsed < timeout && not found do
            let! state = agent.QueryAsync id
            if predicate state then
                found <- true
            else
                do! Task.Delay 50
        return found
    }
