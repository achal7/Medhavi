module Medhavi.SharedKernel.Execution.AggregateStages

open System
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Observation
open Medhavi.SharedKernel.Failure
open Medhavi.SharedKernel.Contracts
open Medhavi.SharedKernel.ExecutionContext
open Medhavi.SharedKernel

/// A function that enriches ArchitecturalKnowledge with domain‑specific context.
/// Receives the command and the base knowledge; returns enriched knowledge.
type KnowledgeEnricher<'Cmd> = 'Cmd -> ArchitecturalKnowledge -> ArchitecturalKnowledge

let private withCorrelation (ctx: ExecutionContext) (k: ArchitecturalKnowledge) =
    { k with
        Attributes = k.Attributes |> Map.add "CorrelationId" (box ctx.CorrelationId) }

let private makeKnowledge ctx enricher name attributes =
    { Name = name
      Timestamp = DateTimeOffset.UtcNow
      Attributes = attributes }
    |> withCorrelation ctx.ExecutionCtx
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
                    makeKnowledge "AggregateLoadFailed" (Map.ofList [ "AggregateId", box id; "Error", box e ])

                let appErr = ApplicationError.Infrastructure(InfrastructureError.Database $"LOAD_FAILED - %A{e}")

                return Complete(ExecutionOutcome.Failed appErr, [ knowledge ])

            | Ok stateOpt ->
                let knowledge =
                    makeKnowledge "AggregateLoaded" (Map.ofList [ "AggregateId", box id; "Found", box stateOpt.IsSome ])

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
                    makeKnowledge
                        "DecisionFailed"
                        (Map.ofList
                            [ "Command", box(exec.Context.Command.GetType().Name)
                              "Error", box err.Message ])

                return Complete(ExecutionOutcome.Failed(ApplicationError.Domain err), [ knowledge ])

            | Ok decision ->
                let knowledge =
                    makeKnowledge
                        "DecisionEvaluated"
                        (Map.ofList
                            [ "DecisionId",
                              box(decision.Trace |> Option.map(fun t -> t.DecisionId) |> Option.defaultValue "none")
                              "DecisionTrace", box decision.Trace ])

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
            let id: string = getId exec.Context.Command

            let enrichedEvents =
                decision.Events
                |> List.map(fun evt ->
                    let json = Envelope.serialize evt |> Result.defaultValue "{}"
                    let env = Envelope.createEnvelope (evt.GetType().Name) json 1
                    let env = Envelope.withExecutionContext exec.Context.ExecutionCtx env
                    let env = Envelope.withAggregateContext id (typeof<'Agg>.Name) env

                    let env =
                        match decision.Trace with
                        | Some t -> Envelope.withDecisionTrace t env
                        | None -> Ok env

                    evt, env)

            let newState = decision.NewState

            match! repo.Save(id, newState, decision.Events) with
            | Error e ->
                let knowledge = makeKnowledge "PersistFailed" (Map.ofList [ "AggregateId", box id; "Error", box e ])

                let appErr = ApplicationError.Infrastructure(InfrastructureError.Database $"SAVE_FAILED - %A{e}")

                return Complete(ExecutionOutcome.Failed appErr, [ knowledge ])

            | Ok _ ->
                enrichedEvents
                |> List.iter(fun (evt, envResult) ->
                    DomainEventBus.Publish evt

                    match envResult with
                    | Ok envelope -> DomainEventBus.Publish envelope
                    | Error _ -> ()) // TODO: Slience error need to taken care

                let knowledge =
                    makeKnowledge
                        "EventsPersisted"
                        (Map.ofList [ "AggregateId", box id; "EventCount", box enrichedEvents.Length ])

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
                makeKnowledge
                    "CommandCompleted"
                    (Map.ofList [ "CorrelationId", box exec.Context.ExecutionCtx.CorrelationId ])

            return Complete(ExecutionOutcome.Completed state, [ knowledge ])
        }
