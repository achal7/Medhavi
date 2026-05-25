module Medhavi.Infrastructure.Stores.EnvelopeStoreMem

open System
open System.Collections.Concurrent
open System.Threading
open System.Threading.Tasks
open Medhavi.Infrastructure.Stores.EnvelopeStore
open System.Collections.Immutable
open Medhavi.Common.Patterns
open Medhavi.SharedKernel

[<CLIMutable>]
type StoredEvent =
    { Envelope: Envelope
      GlobalPosition: int64
      StreamRevision: int64
      CommitPosition: int64 }

/// Internal subscriber record
type Subscriber =
    { Id: Guid
      Mode: SubscriptionMode
      Start: Position option
      Handler: EnvelopedEvent -> Task<unit> }

/// Messages for the mailbox agent
type private Msg =
    | Append of
        streamName: string *
        envelopes: Envelope list *
        expected: ExpectedRevision *
        reply: TaskCompletionSource<Result<AppendResult, EnvelopeStoreError>>
    | AppendAtomicOutbox of
        streamName: string *
        envelopes: Envelope list *
        outboxStream: string *
        outboxEnvelope: Envelope *
        expected: ExpectedRevision *
        reply: TaskCompletionSource<Result<AppendResult, EnvelopeStoreError>>
    | ReadStream of
        streamName: string *
        position: Position option *
        count: int option *
        reply: TaskCompletionSource<Result<EnvelopedEvent list, EnvelopeStoreError>>
    | ReadAll of
        pos: Position option *
        count: int option *
        reply: TaskCompletionSource<Result<EnvelopedEvent list, EnvelopeStoreError>>
    | ReadLast of
        streamName: string *
        count: int64 option *
        reply: TaskCompletionSource<Result<EnvelopedEvent list, EnvelopeStoreError>>
    | GetLastRevision of streamName: string * reply: TaskCompletionSource<Result<Position option, EnvelopeStoreError>>
    | Subscribe of
        mode: SubscriptionMode *
        start: Position option *
        handler: (EnvelopedEvent -> Task<unit>) *
        reply: TaskCompletionSource<Result<SubscriptionHandle, EnvelopeStoreError>>
    | Unsubscribe of Guid
    | ClearStream of streamName: string * reply: TaskCompletionSource<Result<unit, EnvelopeStoreError>>
    | Stop

/// Factory to create an in-memory EnvelopeStoreOps instance (Mailbox + immutable collections)
let createEnvelopeStoreMem () : EnvelopeStoreOps =
    // State stored in the agent: streams map and subscribers map and a global counter
    let initialStreams = ImmutableDictionary<string, ImmutableArray<StoredEvent>>.Empty
    let initialSubs = ImmutableDictionary<Guid, Subscriber>.Empty
    let globalCounter = ref 0L

    // Helpers to build Position and EnvelopedEvent
    let positionOf (se: StoredEvent) : Position =
        { GlobalPosition = Some se.GlobalPosition
          StreamPosition = Some se.StreamRevision
          CommitPosition = Some se.CommitPosition }

    let envelopedOf (readFrom: string) (se: StoredEvent) : EnvelopedEvent =
        { Envelope = se.Envelope
          Position = positionOf se
          ReadFrom = Some readFrom }

    // Convert ExpectedRevision check into function
    let checkExpected (bucket: ImmutableArray<StoredEvent>) (expected: ExpectedRevision) : bool =
        let lastRevOpt =
            if bucket.IsEmpty then
                None
            else
                Some(bucket.[bucket.Length - 1].StreamRevision)

        match expected with
        | ExpectedRevision.Any -> true
        | ExpectedRevision.NoStream -> lastRevOpt.IsNone
        | ExpectedRevision.StreamRevision r ->
            match lastRevOpt with
            | None -> false
            | Some lr -> lr = r

    // Append helper that returns (newStreams, appendResult, appendedStoredEvents)
    let appendToStream
        (streams: ImmutableDictionary<string, ImmutableArray<StoredEvent>>)
        (streamName: string)
        (envelopes: Envelope list)
        : ImmutableDictionary<string, ImmutableArray<StoredEvent>> * AppendResult * StoredEvent list =
        let bucket =
            streams.TryGetValue streamName
            |> function
                | true, arr -> arr
                | _ -> ImmutableArray<StoredEvent>.Empty

        let lastRev =
            if bucket.IsEmpty then
                -1L
            else
                bucket.[bucket.Length - 1].StreamRevision

        // produce StoredEvent list immutably
        let items, lastPos, nextRev =
            envelopes
            |> List.mapi (fun i env ->
                let rev = lastRev + int64 i + 1L
                let gp = System.Threading.Interlocked.Increment(globalCounter)

                let se =
                    { Envelope = env
                      GlobalPosition = gp
                      StreamRevision = rev
                      CommitPosition = gp }

                (se, positionOf se))
            |> List.unzip
            |> fun (ses, poss) -> (ses, (List.last poss), (List.last ses).StreamRevision)

        let newBucket =
            if bucket.IsEmpty then
                ImmutableArray.CreateRange(items)
            else
                bucket.AddRange(items)

        let newStreams = streams.SetItem(streamName, newBucket)

        let appendRes =
            { AppendResult.StreamName = streamName
              AppendResult.Position = Some lastPos
              AppendResult.NextExpectedStreamRevision = Some nextRev }

        (newStreams, appendRes, items)

    // Agent body
    let agent =
        MailboxProcessor.Start(fun inbox ->
            let rec loop
                (streams: ImmutableDictionary<string, ImmutableArray<StoredEvent>>)
                (subs: ImmutableDictionary<Guid, Subscriber>)
                =
                async {
                    let! msg = inbox.Receive()

                    match msg with
                    | Stop ->
                        // Complete all pending subscribers? simply clear and stop
                        return ()

                    | Append(streamName, envelopes, expected, reply) ->
                        try
                            let bucket =
                                streams.TryGetValue streamName
                                |> function
                                    | true, arr -> arr
                                    | _ -> ImmutableArray<StoredEvent>.Empty

                            if not (checkExpected bucket expected) then
                                reply.SetResult(
                                    Error(
                                        EnvelopeStoreError.ConcurrencyError(
                                            sprintf "Expected %A for %s" expected streamName
                                        )
                                    )
                                )

                                return! loop streams subs

                            // perform append
                            let newStreams, appendRes, added = appendToStream streams streamName envelopes
                            // notify subscribers asynchronously (do not block agent)
                            for se in added do
                                let ev = envelopedOf streamName se

                                for kv in subs do
                                    let sub = kv.Value
                                    // deliver depending on subscription mode and start position
                                    let deliver =
                                        match sub.Mode with
                                        | SubscriptionMode.All -> true
                                        | SubscriptionMode.Stream s when s = streamName -> true
                                        | _ -> false

                                    if deliver then
                                        // fire-and-forget but capture exceptions
                                        Task.Run(fun () ->
                                            task {
                                                try
                                                    do! sub.Handler ev
                                                with ex ->
                                                    ()
                                            }
                                            :> Task)
                                        |> ignore

                            reply.SetResult(Ok appendRes)
                            return! loop newStreams subs
                        with ex ->
                            reply.SetResult(Error(EnvelopeStoreError.WriteError ex.Message))
                            return! loop streams subs

                    | AppendAtomicOutbox(streamName, envelopes, outboxStream, outboxEnvelope, expected, reply) ->
                        try
                            let bucket =
                                streams.TryGetValue streamName
                                |> function
                                    | true, arr -> arr
                                    | _ -> ImmutableArray<StoredEvent>.Empty

                            if not (checkExpected bucket expected) then
                                reply.SetResult(
                                    Error(
                                        EnvelopeStoreError.ConcurrencyError(
                                            sprintf "Expected %A for %s" expected streamName
                                        )
                                    )
                                )

                                return! loop streams subs

                            // append primary
                            let streams', appendRes, addedPrimary = appendToStream streams streamName envelopes
                            // append outbox to outbox stream (always append)
                            let streams'', _, addedOutbox =
                                appendToStream streams' outboxStream [ outboxEnvelope ]

                            // notify subscribers for both
                            for se in addedPrimary @ addedOutbox do
                                let ev =
                                    envelopedOf
                                        (if addedPrimary |> List.exists (fun s -> s = se) then
                                             streamName
                                         else
                                             outboxStream)
                                        se

                                for kv in subs do
                                    let sub = kv.Value

                                    let deliver =
                                        match sub.Mode with
                                        | SubscriptionMode.All -> true
                                        | SubscriptionMode.Stream s when s = streamName -> true
                                        | _ -> false

                                    if deliver then
                                        Task.Run(fun () ->
                                            task {
                                                try
                                                    do! sub.Handler ev
                                                with _ ->
                                                    ()
                                            }
                                            :> Task)
                                        |> ignore

                            reply.SetResult(Ok appendRes)
                            return! loop streams'' subs
                        with ex ->
                            reply.SetResult(Error(EnvelopeStoreError.WriteError ex.Message))
                            return! loop streams subs

                    | ReadStream(streamName, position, count, reply) ->
                        try
                            let bucket =
                                streams.TryGetValue streamName
                                |> function
                                    | true, arr -> arr
                                    | _ -> ImmutableArray<StoredEvent>.Empty

                            let startIndex =
                                position
                                |> Option.map (fun pos ->
                                    match pos.StreamPosition with
                                    | Some rev ->
                                        let idx =
                                            bucket
                                            |> Seq.tryFindIndex (fun e -> e.StreamRevision >= rev)

                                        defaultArg idx 0
                                    | _ -> 0)
                                |> Option.defaultValue 0

                            let maxCount = defaultArg count Int32.MaxValue

                            let lastIndex = min (bucket.Length - 1) (startIndex + maxCount - 1)

                            let take =
                                if bucket.IsEmpty || startIndex > lastIndex then
                                    []
                                else
                                    [ for i in startIndex..lastIndex -> bucket.[i] ]

                            let items =
                                take
                                |> List.map (fun se -> envelopedOf streamName se)

                            reply.SetResult(Ok items)
                            return! loop streams subs
                        with ex ->
                            reply.SetResult(Error(EnvelopeStoreError.WriteError ex.Message))
                            return! loop streams subs

                    | ReadAll(posOpt, count, reply) ->
                        try
                            // flatten and sort by GlobalPosition
                            let all =
                                streams
                                |> Seq.collect (fun kv -> kv.Value |> Seq.map (fun se -> (kv.Key, se)))
                                |> Seq.sortBy (fun (_, se) -> se.GlobalPosition)
                                |> Seq.toArray

                            let startIdx =
                                match posOpt with
                                | None -> 0
                                | Some p ->
                                    match p.GlobalPosition with
                                    | None -> 0
                                    | Some gp ->
                                        let idx = Array.tryFindIndex (fun (_, se) -> se.GlobalPosition > gp) all
                                        defaultArg idx 0

                            let maxCount = defaultArg count Int32.MaxValue

                            let takeArr =
                                if all.Length = 0 then
                                    [||]
                                else
                                    all.[startIdx .. min (all.Length - 1) (startIdx + maxCount - 1)]

                            let items =
                                takeArr
                                |> Array.map (fun (s, se) -> envelopedOf s se)
                                |> Array.toList

                            reply.SetResult(Ok items)
                            return! loop streams subs
                        with ex ->
                            reply.SetResult(Error(EnvelopeStoreError.ReadError ex.Message))
                            return! loop streams subs

                    | ReadLast(streamName, countOpt, reply) ->
                        try
                            let bucket =
                                streams.TryGetValue streamName
                                |> function
                                    | true, arr -> arr
                                    | _ -> ImmutableArray<StoredEvent>.Empty

                            let take = defaultArg (Option.map int countOpt) (min 50 bucket.Length)
                            let start = max 0 (bucket.Length - take)

                            let items =
                                [ for i in start .. bucket.Length - 1 -> envelopedOf streamName bucket.[i] ]

                            reply.SetResult(Ok items)
                            return! loop streams subs
                        with ex ->
                            reply.SetResult(Error(EnvelopeStoreError.ReadError ex.Message))
                            return! loop streams subs

                    | GetLastRevision(streamName, reply) ->
                        try
                            let bucket =
                                streams.TryGetValue streamName
                                |> function
                                    | true, arr -> arr
                                    | _ -> ImmutableArray<StoredEvent>.Empty

                            if bucket.IsEmpty then
                                reply.SetResult(Ok None)
                            else
                                reply.SetResult(Ok(Some(positionOf bucket.[bucket.Length - 1])))

                            return! loop streams subs
                        with ex ->
                            reply.SetResult(Error(EnvelopeStoreError.ReadError ex.Message))
                            return! loop streams subs

                    | Subscribe(mode, start, handler, reply) ->
                        try
                            let id = Guid.NewGuid()

                            let sub =
                                { Id = id
                                  Mode = mode
                                  Start = start
                                  Handler = handler }

                            let newSubs = subs.Add(id, sub)

                            // deliver historical depending on mode and start
                            match mode with
                            | SubscriptionMode.All ->
                                // schedule historical delivery asynchronously
                                Task.Run(fun () ->
                                    task {
                                        // read from agent state via synchronous call to agent (simple approach: ask for ReadAll)
                                        let tcs =
                                            TaskCompletionSource<Result<EnvelopedEvent list, EnvelopeStoreError>>()

                                        inbox.Post(ReadAll(start, None, tcs))
                                        let! res = tcs.Task

                                        match res with
                                        | Ok evs ->
                                            for e in evs do
                                                try
                                                    do! handler e
                                                with _ ->
                                                    ()
                                        | Error _ -> ()
                                    }
                                    :> Task)
                                |> ignore
                            | SubscriptionMode.Stream s ->
                                Task.Run(fun () ->
                                    task {
                                        let tcs =
                                            TaskCompletionSource<Result<EnvelopedEvent list, EnvelopeStoreError>>()

                                        inbox.Post(ReadStream(s, start, None, tcs))

                                        let! res = tcs.Task

                                        match res with
                                        | Ok evs ->
                                            for e in evs do
                                                try
                                                    do! handler e
                                                with _ ->
                                                    ()
                                        | Error _ -> ()
                                    }
                                    :> Task)
                                |> ignore

                            let handle =
                                { Id = id
                                  Unsubscribe = fun () -> task { } //Task.FromResult(agent.Post(Unsubscribe id))
                                }

                            reply.SetResult(Ok handle)
                            return! loop streams newSubs
                        with ex ->
                            reply.SetResult(Error(EnvelopeStoreError.UnknownError ex.Message))
                            return! loop streams subs

                    | Unsubscribe id ->
                        let newSubs = subs.Remove id
                        return! loop streams newSubs

                    | ClearStream(streamName, reply) ->
                        try
                            let newStreams =
                                if streams.ContainsKey(streamName) then
                                    streams.Remove(streamName)
                                else
                                    streams

                            reply.SetResult(Ok())
                            return! loop newStreams subs
                        with ex ->
                            reply.SetResult(Error(WriteError ex.Message))
                            return! loop streams subs
                }

            loop initialStreams initialSubs)

    // Adapter functions that post messages to the agent and return Task<Result<...>>
    let postAsync f =
        let tcs =
            TaskCompletionSource<_>(TaskCreationOptions.RunContinuationsAsynchronously)

        f tcs

    let publish
        (streamName: string)
        (envs: Envelope list)
        (expected: ExpectedRevision)
        (ct: CancellationToken)
        : AsyncResult<AppendResult, EnvelopeStoreError> =
        let tcs =
            TaskCompletionSource<Result<AppendResult, EnvelopeStoreError>>(
                TaskCreationOptions.RunContinuationsAsynchronously
            )

        agent.Post(Append(streamName, envs, expected, tcs))
        tcs.Task |> AsyncResult.ofTask

    let publishSingle
        (streamName: string)
        (env: Envelope)
        (expected: ExpectedRevision)
        (ct: CancellationToken)
        : AsyncResult<AppendResult, EnvelopeStoreError> =
        publish streamName [ env ] expected ct

    let publishAtomicOutbox
        (streamName: string)
        (envs: Envelope list)
        (outboxStream: string)
        (outboxEnvelope: Envelope)
        (expected: ExpectedRevision)
        (ct: CancellationToken)
        : AsyncResult<AppendResult, EnvelopeStoreError> =
        let tcs =
            TaskCompletionSource<Result<AppendResult, EnvelopeStoreError>>(
                TaskCreationOptions.RunContinuationsAsynchronously
            )

        agent.Post(AppendAtomicOutbox(streamName, envs, outboxStream, outboxEnvelope, expected, tcs))
        tcs.Task |> AsyncResult.ofTask

    let readStream
        (streamName: string)
        (position: Position option)
        (count: int option)
        (ct: CancellationToken)
        : AsyncResult<EnvelopedEvent list, EnvelopeStoreError> =
        let tcs =
            TaskCompletionSource<Result<EnvelopedEvent list, EnvelopeStoreError>>(
                TaskCreationOptions.RunContinuationsAsynchronously
            )

        agent.Post(ReadStream(streamName, position, count, tcs))
        tcs.Task |> AsyncResult.ofTask

    let readAll
        (posOpt: Position option)
        (count: int option)
        (ct: CancellationToken)
        : AsyncResult<EnvelopedEvent list, EnvelopeStoreError> =
        let tcs =
            TaskCompletionSource<Result<EnvelopedEvent list, EnvelopeStoreError>>(
                TaskCreationOptions.RunContinuationsAsynchronously
            )

        agent.Post(ReadAll(posOpt, count, tcs))
        tcs.Task |> AsyncResult.ofTask

    let readLast
        (streamName: string)
        (count: int64 option)
        (ct: CancellationToken)
        : AsyncResult<EnvelopedEvent list, EnvelopeStoreError> =
        let tcs =
            TaskCompletionSource<Result<EnvelopedEvent list, EnvelopeStoreError>>(
                TaskCreationOptions.RunContinuationsAsynchronously
            )

        agent.Post(ReadLast(streamName, count, tcs))
        tcs.Task |> AsyncResult.ofTask

    let getLastRevision
        (streamName: string)
        (ct: CancellationToken)
        : AsyncResult<Position option, EnvelopeStoreError> =
        let tcs =
            TaskCompletionSource<Result<Position option, EnvelopeStoreError>>(
                TaskCreationOptions.RunContinuationsAsynchronously
            )

        agent.Post(GetLastRevision(streamName, tcs))
        tcs.Task |> AsyncResult.ofTask

    let subscribe
        (mode: SubscriptionMode)
        (startPos: Position option)
        (handler: EnvelopedEvent -> Task<unit>)
        (ct: CancellationToken)
        : AsyncResult<SubscriptionHandle, EnvelopeStoreError> =
        let tcs =
            TaskCompletionSource<Result<SubscriptionHandle, EnvelopeStoreError>>(
                TaskCreationOptions.RunContinuationsAsynchronously
            )

        agent.Post(Subscribe(mode, startPos, handler, tcs))
        tcs.Task |> AsyncResult.ofTask

    let clearStream (streamName: string) (ct: CancellationToken) : AsyncResult<unit, EnvelopeStoreError> =
        let tcs =
            TaskCompletionSource<Result<unit, EnvelopeStoreError>>(TaskCreationOptions.RunContinuationsAsynchronously)

        agent.Post(ClearStream(streamName, tcs))
        tcs.Task |> AsyncResult.ofTask

    { Publish = publish
      PublishSingle = publishSingle
      PublishAtomicOutbox = publishAtomicOutbox
      ReadStream = readStream
      ReadAll = readAll
      ReadLast = readLast
      GetLastRevision = getLastRevision
      Subscribe = subscribe
      ClearStream = clearStream }
