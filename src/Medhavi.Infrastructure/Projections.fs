module Medhavi.Infrastructure.Projections

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.Infrastructure.Stores.EnvelopeStore

/// Configuration for projection behavior
type ProjectionConfig =
    {
        /// Maximum mailbox size before backpressure kicks in
        MaxMailboxSize: int
        /// Number of events to process in a batch
        BatchSize: int
        /// Checkpoint interval (every N events)
        CheckpointInterval: int
    }

    static member Default =
        { MaxMailboxSize = 10000
          BatchSize = 100
          CheckpointInterval = 1000 }

/// Dependencies for projection agent creation
type ProjectionDependencies<'Event> =
    { EnvelopeStore: EnvelopeStoreOps
      EventCodec: Codec<'Event>
      //DeadLetterQueue: DeadLetterQueue
      Config: ProjectionConfig
      TelemetryPublisher: TelemetryEvent -> unit }


type private ProjectionAgentMsg<'State, 'Event> =
    | Apply of ev: 'Event * msgId: Guid * causationId: Guid option * correlationId: Guid option * pos: int64 option
    | SetState of 'State
    | GetState of AsyncReplyChannel<'State>
    | GetStats of AsyncReplyChannel<ProjectionStats>
    | Query of query: ('State -> obj) * AsyncReplyChannel<obj>
    | Reset

type ProjectionAgent<'State, 'Event>(applyFn: 'State -> 'Event -> 'State, initial: 'State, config: ProjectionConfig) =

    let eventApplied = Event<'Event>()

    let stateRef = ref initial
    let cts = new CancellationTokenSource()

    let agent =
        MailboxProcessor.Start(fun inbox ->
            let rec loop (state: 'State, stats: ProjectionStats) =
                async {
                    if cts.Token.IsCancellationRequested then
                            return ()
                    // Backpressure
                    if inbox.CurrentQueueLength > config.MaxMailboxSize then
                        do! Async.Sleep(100)
                        return! loop(state, stats)
                        
                    let! msg = inbox.Receive()

                    match msg with
                    | Apply(ev, msgId, causationId, correlationId, _) ->
                        try
                            if stats.ProcessedMessageIds |> Set.contains msgId then
                                return! loop(state, stats)
                            else
                                let newState = applyFn state ev
                                stateRef := newState
                                eventApplied.Trigger ev

                                let newStats =
                                    { stats with
                                        EventsProcessed = stats.EventsProcessed + 1L
                                        LastUpdated = DateTimeOffset.UtcNow
                                        LastMessageId = Some msgId
                                        LastCausationId = causationId
                                        LastCorrelationId = correlationId
                                        ProcessedMessageIds = Set.add msgId stats.ProcessedMessageIds }

                                return! loop(newState, newStats)
                        with ex ->
                            let errorStats = { stats with LastError = Some ex.Message }
                            return! loop(state, errorStats)

                    | SetState newState ->
                        stateRef := newState
                        return! loop(newState, stats)

                    | GetState reply ->
                        reply.Reply state
                        return! loop(state, stats)

                    | GetStats reply ->
                        reply.Reply stats
                        return! loop(state, stats)

                    | Query(query, reply) ->
                        let result = query state
                        reply.Reply result
                        return! loop(state, stats)

                    | Reset -> return! loop(initial, ProjectionStats.Default)
                }

            loop(initial, ProjectionStats.Default))

    member _.EventApplied = eventApplied.Publish
    member _.Post(ev, msgId, causationId, correlationId, pos) = agent.Post(Apply(ev, msgId, causationId, correlationId, pos))
    member _.SetState state = agent.Post(SetState state)
    member _.GetStateAsync() : Task<'State> = agent.PostAndAsyncReply GetState |> Async.StartAsTask

    member _.GetStatsAsync() : Task<ProjectionStats> = agent.PostAndAsyncReply GetStats |> Async.StartAsTask

    member _.Reset() = agent.Post Reset

    member _.Cancel() = cts.Cancel()

    member _.QueryAsyncToAgent<'Result>(query: 'State -> 'Result) : Task<'Result> =
        task {
            let boxedQuery = fun s -> box(query s)
            let! result = agent.PostAndAsyncReply(fun reply -> Query(boxedQuery, reply)) |> Async.StartAsTask
            return unbox<'Result> result
        }

    member _.QueryAsync<'Result>(query: 'State -> 'Result) : Task<'Result> =
        let currentState = !stateRef
        Task.FromResult(query currentState)

    member this.QueryPredicateAsync(predicate: 'State -> bool) : System.Threading.Tasks.Task<bool> =
        this.QueryAsync predicate

    /// Query with item predicate - for collections like Map, Filtering executes inside the agent
    member this.QueryItemsAsync<'a>
        (predicate: 'a -> bool, getItems: 'State -> 'a seq)
        : System.Threading.Tasks.Task<'a list> =
        this.QueryAsync(fun state -> getItems state |> Seq.filter predicate |> Seq.toList)

    interface IDisposable with
        member this.Dispose() =
            this.Cancel()
            cts.Dispose()

module QueryServiceBase =
    let getById
        (queryAgent: ProjectionAgent<Map<'Id, 'Entity>, 'Event>)
        (id: 'Id)
        : Task<'Entity option> =
        queryAgent.QueryAsync(fun state -> Map.tryFind id state)

    let getAll (queryAgent: ProjectionAgent<Map<'Id, 'Entity>, 'Event>) : Task<'Entity list> =
        queryAgent.QueryAsync(fun state -> Map.values state |> Seq.toList)

    let exists
        (queryAgent: ProjectionAgent<Map<'Id, 'Entity>, 'Event>)
        (id: 'Id)
        : Task<bool> =
        queryAgent.QueryAsync(fun state -> Map.containsKey id state)

    let filter
        (queryAgent: ProjectionAgent<Map<'Id, 'Entity>, 'Event>)
        (predicate: 'Entity -> bool)
        : Task<'Entity list> =
        queryAgent.QueryAsync(fun state -> state |> Map.values |> Seq.filter predicate |> Seq.toList)

    let tryFind
        (queryAgent: ProjectionAgent<Map<'Id, 'Entity>, 'Event>)
        (predicate: 'Entity -> bool)
        : Task<'Entity option> =
        queryAgent.QueryAsync(fun state -> state |> Map.values |> Seq.tryFind predicate)

    let getQueryService
        (queryAgent: ProjectionAgent<Map<'Id, 'Entity>, 'Event>)
        : QueryService<'Entity, 'Id> =
        { GetAll = fun () -> getAll queryAgent
          GetById = getById queryAgent
          Exists = exists queryAgent
          Filter = filter queryAgent
          SubscribeApiEvents = fun handler -> queryAgent.EventApplied |> Observable.subscribe handler }

let createProjectionAgent<'Event, 'Entity, 'Id when 'Id: comparison and 'Event: not null>
    (deps: ProjectionDependencies<'Event>): CreateQueryService<'Event, 'Entity, 'Id> =

    let createEnvelopeHandler (agent:ProjectionAgent<Map<'Id,'Entity>,'Event>) (deps: ProjectionDependencies<'Event>) eventFilters projectionName: Envelope -> Task<unit> =
        fun (envelope: Envelope) ->
            task {
                // Surface-level routing
                let shouldProcess =
                    match Set.isEmpty eventFilters with
                    | true -> true  // All events
                    | false -> Set.contains envelope.EventType eventFilters
                if shouldProcess then
                    // Decode the payload
                    match deps.EventCodec.Decode envelope.DataJson with
                    | Ok domainEvent ->
                            let causionId, corId = envelope |> Envelope.getCausionAndCorrelationGuid
                            let eid = EventId.value envelope.EventId
                            agent.Post( domainEvent, eid, causionId, corId, None)
                    | Error decodeErr ->
                        //deps.DeadLetterQueue.Enqueue envelope (sprintf "%A" decodeErr)
                        deps.TelemetryPublisher {
                            EventId = EventId.value envelope.EventId
                            Severity = TelemetrySeverity.Critical
                            Message = $"Failed to decode EnterprisePicture envelope {envelope.EventId} : {decodeErr}"
                            CausationId = envelope.CorrelationId
                            CorrelationId = envelope.CorrelationId
                            TraceId = None
                            SpanId = None
                            Properties = Map [
                                "EnvelopeId", box envelope.EventId
                                "EventType", box envelope.EventType
                                "Error", box decodeErr
                                "Projection", box projectionName
                            ]
                            Timestamp = DateTimeOffset.UtcNow
                        }
            }

    let create: CreateQueryService<'Event, 'Entity, 'Id> =
        fun applyFn validEventTypes initialState projectionName ->
            taskResult{

                // 1. Create the agent
                let agent = new ProjectionAgent<_,_>(applyFn, initialState, deps.Config)
                agent.SetState initialState

                // 2. Create envelope handler
                let filterEvents =
                    match validEventTypes with
                    | EventTypes types -> Set.ofList types
                    | _ -> Set.empty
                let handler = createEnvelopeHandler agent deps filterEvents projectionName
                // 3. Subscribe to envelopes

                let! subscription:SubscriptionHandle = 
                    deps.EnvelopeStore.Subscribe SubscriptionMode.All None (fun env -> handler env.Envelope) CancellationToken.None
                    |> TaskResult.mapError(fun e -> Infrastructure(EventStore $"Failed to subscribe to envelopes for projection {projectionName} : {e}"))
                
                // 4. Create query service
                let queryService = QueryServiceBase.getQueryService agent

                // 5. Create disposal function that cleans up BOTH subscription and agent
                let dispose () =
                    try
                        subscription.Unsubscribe() |> Async.AwaitTask |> Async.RunSynchronously
                    with ex ->
                        deps.TelemetryPublisher {
                            EventId = Guid.NewGuid()
                            Severity = TelemetrySeverity.Warning
                            Message = $"Failed to dispose subscription for {projectionName}: {ex.Message}"
                            CausationId = None
                            CorrelationId = None
                            TraceId = None
                            SpanId = None
                            Properties = Map.empty
                            Timestamp = DateTimeOffset.UtcNow
                        }
                    
                    try
                        (agent :> IDisposable).Dispose()
                    with ex ->
                        deps.TelemetryPublisher {
                            EventId = Guid.NewGuid()
                            Severity = TelemetrySeverity.Warning
                            Message = $"Failed to dispose agent for {projectionName}: {ex.Message}"
                            CausationId = None
                            CorrelationId = None
                            TraceId = None
                            SpanId = None
                            Properties = Map.empty
                            Timestamp = DateTimeOffset.UtcNow
                        }
                
                return { QueryService = queryService; Dispose = dispose }
            }

    create