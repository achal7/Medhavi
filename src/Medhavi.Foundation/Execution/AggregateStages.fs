module Medhavi.Foundation.Execution.AggregateStages

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Observation
open Medhavi.Foundation.Failure
open Medhavi.Foundation.ExecutionContext
open Medhavi.Common

/// A function that enriches ArchitecturalKnowledge with domain‑specific context.
/// Receives the command and the base knowledge; returns enriched knowledge.
type KnowledgeEnricher<'Cmd> = 'Cmd -> ArchitecturalKnowledge -> ArchitecturalKnowledge

type EnvelopeStoreDependencies<'Event> =
    {
        /// Serialize domain event to JSON for persistence
        SerializeEvent: 'Event -> Result<string, string>

        /// Dispatch envelope to the store (write-side)
        DispatchEnvelope: Envelope -> Task<unit>

        /// Codec for serializing/deserializing DecisionTrace
        TraceCodec: Codec<DecisionTrace>

        /// Subscribe to envelopes (read-side)
        /// Returns IDisposable for cleanup
        Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
    }

let private withCorrelation (ctx: ExecutionContext) (k: ArchitecturalKnowledge) =
    { k with
        Attributes = k.Attributes |> Map.add "CorrelationId" (box ctx.CorrelationId) }

let private makeKnowledge ctx enricher (knowledge: ArchitecturalKnowledge) =
    { knowledge with
        Attributes = knowledge.Attributes |> Map.add "CorrelationId" (box ctx.ExecutionCtx.CorrelationId) }
    |> enricher ctx.Command

let load
    (repo: Repository<'Agg, 'Id, 'Event>)
    (getId: 'Cmd -> 'Id)
    (enricher: KnowledgeEnricher<'Cmd>)
    : ExecutionStage<
          Execution<'Agg option, CommandContext<'Cmd, 'Agg, 'Event>>,
          ExecutionOutcome<'Agg, ApplicationError>
       >
    =
    fun exec ->
        let makeKnowledge = makeKnowledge exec.Context enricher

        task {
            let id = getId exec.Context.Command
            let! loaded = repo.Get id

            match loaded with
            | Error e ->
                let knowledge =
                    makeKnowledge(
                        ArchitecturalKnowledge.ofError "AggregateLoadFailed" [ "AggregateId", box id; "Error", box e ]
                    )

                let appErr = ApplicationError.Infrastructure(InfrastructureError.Database $"LOAD_FAILED - %A{e}")

                return Complete(ExecutionOutcome.Failed appErr, [ knowledge ])

            | Ok stateOpt ->
                let knowledge =
                    makeKnowledge(
                        ArchitecturalKnowledge.ofBusinessEvent
                            "AggregateLoaded"
                            [ "AggregateId", box id; "Found", box stateOpt.IsSome ]
                    )

                let next =
                    { exec with
                        State = stateOpt
                        Context =
                            { exec.Context with
                                AggregateVersion = Some 0L } }

                return Continue(next, [ knowledge ])
        }

let decide
    (decideFn: Decide<'Agg, 'Cmd, 'Event>)
    (enricher: KnowledgeEnricher<'Cmd>)
    : ExecutionStage<
          Execution<'Agg option, CommandContext<'Cmd, 'Agg, 'Event>>,
          ExecutionOutcome<'Agg, ApplicationError>
       >
    =

    fun exec ->
        let makeKnowledge = makeKnowledge exec.Context enricher

        task {
            match decideFn exec.Context.Command exec.State with
            | Error err ->
                let knowledge =
                    makeKnowledge(
                        ArchitecturalKnowledge.ofError
                            "DecisionFailed"
                            [ "Command", box(exec.Context.Command.GetType().Name)
                              "Error", box err.Message ]
                    )

                return Complete(ExecutionOutcome.Failed(Domain err), [ knowledge ])

            | Ok decision ->
                let knowledge =
                    makeKnowledge(
                        ArchitecturalKnowledge.ofBusinessEvent
                            "DecisionEvaluated"
                            [ "DecisionId",
                              box(decision.Trace |> Option.map(fun t -> t.DecisionId) |> Option.defaultValue "none")
                              "DecisionTrace", box decision.Trace ]
                    )

                let next =
                    { exec with
                        Context =
                            { exec.Context with
                                Decision = Some decision } }

                return Continue(next, [ knowledge ])
        }

let persist
    (repo: Repository<'Agg, 'Id, 'Event>)
    (getId: 'Cmd -> 'Id)
    (deps: EnvelopeStoreDependencies<'Event>)
    (enricher: KnowledgeEnricher<'Cmd>)
    : ExecutionStage<
          Execution<'Agg option, CommandContext<'Cmd, 'Agg, 'Event>>,
          ExecutionOutcome<'Agg, ApplicationError>
       >
    =
    fun exec ->
        let makeKnowledge = makeKnowledge exec.Context enricher

        task {
            let decision = exec.Context.Decision.Value
            let id = getId exec.Context.Command

            let enrichedEvents =
                decision.Events
                |> List.map(fun evt ->
                    let json = deps.SerializeEvent evt |> Result.defaultValue "{}"
                    let env = Envelope.CreateBasic(evt.GetType().Name, json)
                    let env = Envelope.withExecutionContext exec.Context.ExecutionCtx env
                    let env = Envelope.withAggregateContext (id.ToString()) typeof<'Agg>.Name env

                    let env =
                        match decision.Trace with
                        | Some t -> Envelope.withDecisionTrace deps.TraceCodec t env
                        | None -> Ok env

                    evt, env)

            let newState = decision.NewState

            match! repo.Save(id, None, newState, decision.Events) with
            | Error e ->
                let knowledge =
                    makeKnowledge(
                        ArchitecturalKnowledge.ofError "PersistFailed" [ "AggregateId", box id; "Error", box e ]
                    )

                let appErr = Infrastructure(Database $"SAVE_FAILED - %A{e}")

                return Complete(ExecutionOutcome.Failed appErr, [ knowledge ])

            | Ok _ ->
                enrichedEvents
                |> List.iter(fun (evt, envResult) ->
                    //DomainEventBus.Publish evt

                    match envResult with
                    | Ok envelope -> deps.DispatchEnvelope envelope |> ignore
                    | Error serErr ->
                        // let errorKnowledge =
                        //     makeKnowledge(
                        //         ArchitecturalKnowledge.ofError
                        //             "EnvelopeSerializationFailed"
                        //             [ "AggregateId", box id; "Error", box serErr ]
                        //     )

                        //DomainEventBus.Publish errorKnowledge
                        ())

                let knowledge =
                    makeKnowledge(
                        ArchitecturalKnowledge.ofBusinessEvent
                            "EventsPersisted"
                            [ "AggregateId", box id; "EventCount", box enrichedEvents.Length ]
                    )

                return Continue({ exec with State = Some newState }, [ knowledge ])
        }

let publishKnowledge
    (enricher: KnowledgeEnricher<'Cmd>)
    : ExecutionStage<
          Execution<'Agg option, CommandContext<'Cmd, 'Agg, 'Event>>,
          ExecutionOutcome<'Agg, ApplicationError>
       >
    =
    fun exec ->
        let makeKnowledge = makeKnowledge exec.Context enricher

        task {
            let state = exec.State |> Option.defaultWith(fun () -> failwith "State must exist")

            let knowledge =
                makeKnowledge(
                    ArchitecturalKnowledge.ofBusinessEvent
                        "CommandCompleted"
                        [ "CorrelationId", box exec.Context.ExecutionCtx.CorrelationId ]
                )

            return Complete(Completed state, [ knowledge ])
        }
