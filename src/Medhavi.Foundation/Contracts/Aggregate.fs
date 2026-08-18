namespace Medhavi.Foundation.Contracts

open System.Threading.Tasks
open Medhavi.Common.Validation
open Medhavi.Foundation.Failure

/// Versioned aggregate for optimistic concurrency
type VersionedAggregate<'Aggregate> = { Aggregate: 'Aggregate; Version: int }

type Decision<'State, 'Event> =
    { NewState: 'State
      Events: 'Event list
      Trace: DecisionTrace option }

/// Shared decide/evolve function signatures for event-sourced aggregates.
type Decide<'State, 'Command, 'Event> = 'Command -> 'State option -> Result<Decision<'State, 'Event>, DomainError>
type Evolve<'State, 'Event> = 'State option -> 'Event -> 'State option

/// Validator type for aggregate commands using applicative Validation functor
type CommandValidator<'Command> = 'Command -> Validation<'Command, DomainError>

type NotificationPublisher =
    { Publish: obj -> unit // type‑safe overloads will be added per type
      PublishAsync: obj -> Task<unit> }

module NotificationPublisher =
    let publish<'T> (pub: NotificationPublisher) (notification: 'T) = pub.Publish(box notification)

    let nullPublisher =
        { Publish = fun _ -> ()
          PublishAsync = fun _ -> task { () } }

module Decision =
    /// Replays a sequence of events on top of an initial state.
    let replay (evolve: Evolve<'State, 'Event>) (events: 'Event seq) : 'State option = Seq.fold evolve None events

    let buildDecision<'State, 'Event> (evolve: Evolve<'State, 'Event>) stateOpt events traceOpt =
        let newState = events |> List.fold evolve stateOpt

        { NewState =
            newState
            |> Option.defaultWith(fun () -> failwith $"{newState.GetType()} state must exist after applying events")
          Events = events
          Trace = traceOpt }

    /// Executes a command against an aggregate's historical events.
    let handleCommandFromHistory
        (decide: Decide<'State, 'Command, 'Event>)
        (evolve: Evolve<'State, 'Event>)
        (command: 'Command)
        (history: 'Event seq)
        : Result<Decision<'State, 'Event>, DomainError> =
        let currentState = replay evolve history
        decide command currentState

    /// Build a Decision from events AND a DecisionOutcome, along with audit metadata.
    /// - Automatically derives the Outcome string from 'Outcome.ToString().
    /// - Uses the rule evaluations stored inside DecisionOutcome.
    /// - If a summary is provided, it is used; otherwise a default summary is generated from the evaluation results.
    let buildDecisionWithTrace
        (evolve: Evolve<'State, 'Event>)
        (stateOpt: 'State option)
        (events: 'Event list)
        (decisionId: string)
        (causalDecisionIds: string list)
        (capabilityId: string)
        (outcome: DecisionOutcome<'Outcome>)
        (policyId: string option)
        (policyVersion: int option)
        (semanticObjectIds: string list)
        (summary: string option)
        : Decision<'State, 'Event> =

        let outcomeStr = outcome.Outcome.ToString()
        let evaluations = outcome.Evaluations

        let failed = evaluations |> List.filter(fun r -> not r.Passed)

        let summary =
            summary
            |> Option.defaultValue(
                if failed.IsEmpty then
                    "All rules passed"
                else
                    sprintf "%d rule(s) failed" failed.Length
            )

        let evidence = evaluations |> List.collect(fun r -> r.Evidence)

        let trace: DecisionTrace =
            { DecisionId = decisionId
              CausalDecisionIds = causalDecisionIds @ [ decisionId ]
              CapabilityId = capabilityId
              Outcome = outcomeStr
              RulesEvaluated = evaluations
              PolicyId = policyId
              PolicyVersion = policyVersion
              SemanticObjectIds = semanticObjectIds
              Rationale =
                { Summary = summary
                  Evidence = evidence
                  Alternatives = [] } }

        buildDecision evolve stateOpt events (Some trace)
