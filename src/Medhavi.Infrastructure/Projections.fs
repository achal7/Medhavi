namespace Medhavi.Infrastructure.Projections

open System
open System.Threading.Tasks
open Medhavi.Contracts

type ProjectionStats =
    { EventsProcessed: int64
      ItemCount: int64
      LastUpdated: DateTimeOffset
      LastMessageId: Guid option }

    static member Default =
        { EventsProcessed = 0L
          ItemCount = 0L
          LastUpdated = DateTimeOffset.MinValue
          LastMessageId = None }

type private ProjectionAgentMsg<'State, 'Event> =
    | Apply of ev: 'Event * msgId: Guid * pos: int64 option
    | SetState of 'State
    | GetState of AsyncReplyChannel<'State>
    | GetStats of AsyncReplyChannel<ProjectionStats>
    | Query of query: ('State -> obj) * AsyncReplyChannel<obj>
    | Reset

type ProjectionAgent<'State, 'Event>(applyFn: 'State -> 'Event -> 'State, initial: 'State, projectionName: string) =

    let eventApplied = Event<'Event>()

    let agent =
        MailboxProcessor.Start(fun inbox ->
            let rec loop (state: 'State, stats: ProjectionStats) =
                async {
                    let! msg = inbox.Receive()

                    match msg with
                    | Apply(ev, msgId, _) ->
                        match stats.LastMessageId with
                        | Some lm when lm = msgId -> return! loop(state, stats)
                        | _ ->
                            let newState = applyFn state ev
                            eventApplied.Trigger ev

                            let newStats =
                                { stats with
                                    EventsProcessed = stats.EventsProcessed + 1L
                                    LastUpdated = DateTimeOffset.UtcNow
                                    LastMessageId = Some msgId }

                            return! loop(newState, newStats)

                    | SetState newState -> return! loop(newState, stats)

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
    member _.Post(ev, msgId, pos) = agent.Post(Apply(ev, msgId, pos))
    member _.SetState(state) = agent.Post(SetState state)
    member _.GetStateAsync() : Task<'State> = agent.PostAndAsyncReply(GetState) |> Async.StartAsTask

    member _.GetStatsAsync() : Task<ProjectionStats> = agent.PostAndAsyncReply(GetStats) |> Async.StartAsTask

    member _.Reset() = agent.Post(Reset)

    member _.QueryAsync<'Result>(query: 'State -> 'Result) : Task<'Result> =
        task {
            let boxedQuery = fun s -> box(query s)
            let! result = agent.PostAndAsyncReply(fun reply -> Query(boxedQuery, reply)) |> Async.StartAsTask
            return unbox<'Result> result
        }

    member this.QueryPredicateAsync(predicate: 'State -> bool) : System.Threading.Tasks.Task<bool> =
        this.QueryAsync(predicate)

    /// Query with item predicate - for collections like Map, Filtering executes inside the agent
    member this.QueryItemsAsync<'a>
        (predicate: 'a -> bool, getItems: 'State -> 'a seq)
        : System.Threading.Tasks.Task<'a list> =
        this.QueryAsync(fun state -> getItems state |> Seq.filter predicate |> Seq.toList)

module QueryServiceBase =
    let getById
        (queryAgent: ProjectionAgent<Map<string, 'Entity>, 'Event>)
        (idToKey: 'Id -> string)
        (id: 'Id)
        : Task<'Entity option> =
        let key = idToKey id
        queryAgent.QueryAsync(fun state -> Map.tryFind key state)

    let getAll (queryAgent: ProjectionAgent<Map<string, 'Entity>, 'Event>) : Task<'Entity list> =
        queryAgent.QueryAsync(fun state -> Map.values state |> Seq.toList)

    let exists
        (queryAgent: ProjectionAgent<Map<string, 'Entity>, 'Event>)
        (idToKey: 'Id -> string)
        (id: 'Id)
        : Task<bool> =
        let key = idToKey id
        queryAgent.QueryAsync(fun state -> Map.containsKey key state)

    let filter
        (queryAgent: ProjectionAgent<Map<string, 'Entity>, 'Event>)
        (predicate: 'Entity -> bool)
        : Task<'Entity list> =
        queryAgent.QueryAsync(fun state -> state |> Map.values |> Seq.filter predicate |> Seq.toList)

    let tryFind
        (queryAgent: ProjectionAgent<Map<string, 'Entity>, 'Event>)
        (predicate: 'Entity -> bool)
        : Task<'Entity option> =
        queryAgent.QueryAsync(fun state -> state |> Map.values |> Seq.tryFind predicate)

    let getQueryService
        (queryAgent: ProjectionAgent<Map<string, 'Entity>, 'Event>)
        (idToKey: 'Id -> string)
        : QueryService<'Entity, 'Id> =
        { GetAll = fun () -> getAll queryAgent
          GetById = getById queryAgent idToKey
          Exists = exists queryAgent idToKey
          Filter = filter queryAgent
          SubscribeApiEvents = fun handler -> queryAgent.EventApplied |> Observable.subscribe handler }
