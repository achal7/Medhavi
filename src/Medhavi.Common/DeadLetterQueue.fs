module Medhavi.Common.DeadLetterQueue

open System
open System.Collections.Concurrent
open System.Threading
open System.Threading.Tasks
open System.Threading
open Microsoft.Extensions.Logging

// -------------------- Domain --------------------

/// Dead letter entry for failed events
type DeadLetterEntry<'T> =
    { Id: Guid
      Key: string // unique id for the failure (event id)
      Event: 'T
      FailureReason: string
      FailureTimestamp: DateTimeOffset // when it first failed
      LastAttempt: DateTimeOffset option
      RetryCount: int
      OriginalSource: string
      ProcessingStage: string
      Metadata: Map<string, string> } // extensible metadata

/// Stats for dead letter queue
type DeadLetterStats =
    { TotalEntries: int
      EntriesByReason: Map<string, int>
      EntriesByStage: Map<string, int>
      OldestEntry: DateTimeOffset option
      NewestEntry: DateTimeOffset option }

type ReplayOptions =
    { MaxConcurrency: int option
      MaxRetries: int option
      RemoveOnSuccess: bool option
      RemoveOnMaxRetries: bool option
      CancellationToken: CancellationToken option }

    static member DefaultReplayOptions =
        { MaxConcurrency = None
          MaxRetries = None
          RemoveOnSuccess = Some true
          RemoveOnMaxRetries = Some false
          CancellationToken = None }

type DeadLetterStore<'T> =
    { AddAsync: DeadLetterEntry<'T> -> Task<Result<unit, string>>
      GetAllAsync: unit -> Task<Result<DeadLetterEntry<'T> list, string>>
      GetByIdAsync: Guid -> Task<Result<DeadLetterEntry<'T> option, string>>
      RemoveAsync: Guid -> Task<Result<unit, string>>
      GetStatsAsync: unit -> Task<Result<DeadLetterStats, string>>
      // Replay entries using user-provided processor. Returns number of successes.
      ReplayAsync: (DeadLetterEntry<'T> -> Task<Result<unit, string>>) -> ReplayOptions -> Task<Result<int, string>> }

// -------------------- Implementation --------------------

/// In-memory dead-letter queue: bounded by maxEntries. Evicts oldest entries when full.
let createInMemoryDeadLetterQueue<'T> maxEntries (logger: ILogger option) =
    let logger = logger
    let entries = ConcurrentDictionary<Guid, DeadLetterEntry<'T>>()
    // keeps insertion order of keys; can contain keys already removed (we lazily skip those)
    let orderQueue = ConcurrentQueue<Guid>()

    let ensureCapacity () =
        while entries.Count > maxEntries do
            match orderQueue.TryDequeue() with
            | true, id -> entries.TryRemove(id) |> ignore
            | false, _ -> ()

    // Add or update entry (thread-safe). Evict oldest if at capacity.
    let addAsync (entry: DeadLetterEntry<'T>) =
        task {
            try
                // use AddOrUpdate with explicit lambdas (wrapped in parentheses to avoid indentation issues)
                let _ =
                    entries.AddOrUpdate(
                        entry.Id,
                        (fun _key ->
                            // add factory: enqueue order and return the new entry
                            orderQueue.Enqueue(entry.Id)
                            entry),
                        (fun _key existing ->
                            // update factory: increment retry count & update metadata
                            { existing with
                                RetryCount = existing.RetryCount + 1
                                LastAttempt = Some DateTimeOffset.UtcNow
                                FailureReason = entry.FailureReason
                                ProcessingStage = entry.ProcessingStage
                                Metadata = entry.Metadata })
                    )

                // Ensure capacity by evicting oldest keys from orderQueue until size <= maxEntries
                let mutable loop = true

                while loop && entries.Count > maxEntries do
                    match orderQueue.TryDequeue() with
                    | true, key ->
                        // TryRemove returns (bool, value). We ignore the removed value.
                        let _ = entries.TryRemove(key)
                        // continue loop until entries.Count <= maxEntries
                        loop <- (entries.Count > maxEntries)
                    | false, _ ->
                        // no more items to dequeue
                        loop <- false

                logger
                |> Option.iter(fun l -> l.LogDebug("InMemoryDeadLetterQueue: added/updated entry {Key}", entry.Key))

                return Ok()
            with ex ->
                logger |> Option.iter(fun l -> l.LogError(ex, "DeadLetter(inmem): add failed"))

                return Error ex.Message
        }

    let getAllAsync () =
        task {
            try
                // snapshot and sort by FailureTimestamp ascending
                let list = entries.Values |> Seq.toList |> List.sortBy(fun e -> e.FailureTimestamp)

                return Ok list
            with ex ->
                logger |> Option.iter(fun l -> l.LogError(ex, "DeadLetter(inmem): GetAll failed"))

                return Error ex.Message
        }

    let getByEventIdAsync (eventId: Guid) =
        task {
            try
                match entries.TryGetValue(eventId) with
                | true, e -> return Ok(Some e)
                | false, _ -> return Ok None
            with ex ->
                logger |> Option.iter(fun l -> l.LogError(ex, "DeadLetter(inmem): GetById failed"))

                return Error ex.Message
        }

    let removeAsync (eventId: Guid) =
        task {
            try
                let removed, _ = entries.TryRemove(eventId)

                if removed then
                    logger |> Option.iter(fun l -> l.LogDebug("InMemoryDeadLetterQueue: removed entry {Key}", eventId))

                return Ok()
            with ex ->
                logger |> Option.iter(fun l -> l.LogError(ex, "DeadLetter(inmem): Remove failed"))

                return Error ex.Message
        }

    let getStatsAsync () =
        task {
            try
                let snapshot = entries.Values |> Seq.toList
                let total = snapshot.Length

                let byReason =
                    snapshot
                    |> Seq.groupBy(fun e -> e.FailureReason)
                    |> Seq.map(fun (k, g) -> k, Seq.length g)
                    |> Map.ofSeq

                let byStage =
                    snapshot
                    |> Seq.groupBy(fun e -> e.ProcessingStage)
                    |> Seq.map(fun (k, g) -> k, Seq.length g)
                    |> Map.ofSeq

                let oldest =
                    if snapshot.IsEmpty then
                        None
                    else
                        snapshot |> Seq.minBy(fun e -> e.FailureTimestamp) |> (fun e -> Some e.FailureTimestamp)

                let newest =
                    if snapshot.IsEmpty then
                        None
                    else
                        snapshot |> Seq.maxBy(fun e -> e.FailureTimestamp) |> (fun e -> Some e.FailureTimestamp)

                return
                    Ok(
                        { TotalEntries = total
                          EntriesByReason = byReason
                          EntriesByStage = byStage
                          OldestEntry = oldest
                          NewestEntry = newest }

                    )
            with ex ->
                logger |> Option.iter(fun l -> l.LogError(ex, "DeadLetter(inmem): GetStats failed"))

                return Error ex.Message
        }

    let replayAsync (processFunction, replyOptions) =
        task {
            try
                let maxConcurrency = defaultArg replyOptions.MaxConcurrency Environment.ProcessorCount

                let maxRetries = defaultArg replyOptions.MaxRetries Int32.MaxValue
                let removeOnSuccess = defaultArg replyOptions.RemoveOnSuccess true
                let removeOnMaxRetries = defaultArg replyOptions.RemoveOnMaxRetries false
                let ct = defaultArg replyOptions.CancellationToken CancellationToken.None

                // snapshot entries to avoid modifying the collection while iterating
                let snapshot = entries.Values |> Seq.toList
                let successes = ref 0
                use sem = new Threading.SemaphoreSlim(maxConcurrency)

                let tasks =
                    snapshot
                    |> Seq.map(fun entry ->
                        task {
                            do! sem.WaitAsync(ct)

                            try
                                let mutable attempt = 1
                                let mutable ok = false

                                while not ok && attempt <= maxRetries && not ct.IsCancellationRequested do
                                    match! processFunction entry with
                                    | Ok() ->
                                        ok <- true

                                        if removeOnSuccess then
                                            entries.TryRemove(entry.Id) |> ignore

                                        Interlocked.Increment(successes) |> ignore
                                    | Error _ -> attempt <- attempt + 1

                                if not ok && removeOnMaxRetries then
                                    entries.TryRemove(entry.Id) |> ignore
                            finally
                                sem.Release() |> ignore
                        }
                        :> Task)
                    |> Seq.toArray

                do! Task.WhenAll(tasks)
                return Ok(successes.Value)
            with ex ->
                logger |> Option.iter(fun l -> l.LogError(ex, "DeadLetter(inmem): Replay failed"))

                return Error ex.Message
        }

    { AddAsync = addAsync
      GetAllAsync = getAllAsync
      GetByIdAsync = getByEventIdAsync
      RemoveAsync = removeAsync
      GetStatsAsync = getStatsAsync
      ReplayAsync = fun processFn opts -> replayAsync(processFn, opts) }

module DeadLetterHelpers =
    /// Create a basic entry (Key must be unique per event)
    let createEntry<'T>
        (event: 'T)
        (key: string)
        (failureReason: string)
        (processingStage: string)
        (originalSource: string)
        (retryCount: int)
        (metadata: Map<string, string> option)
        =
        { Id = Guid.NewGuid()
          Key = key
          Event = event
          FailureReason = failureReason
          FailureTimestamp = DateTimeOffset.UtcNow
          LastAttempt = None
          RetryCount = retryCount
          OriginalSource = originalSource
          ProcessingStage = processingStage
          Metadata = defaultArg metadata Map.empty }

    let createFromException<'T>
        (event: 'T)
        (key: string)
        (ex: Exception)
        (processingStage: string)
        (originalSource: string)
        (retryCount: int)
        =
        createEntry event key ex.Message processingStage originalSource retryCount None

    let createFromError<'T>
        (event: 'T)
        (key: string)
        (error: string)
        (processingStage: string)
        (originalSource: string)
        (retryCount: int)
        =
        createEntry event key error processingStage originalSource retryCount None

// -------------------- Usage Example (small) --------------------
(*
open System.Net.Http

let example () =
    let logger: ILogger option = None
    let q = DeadLetterQueueFactory.CreateInMemory<HttpResponseMessage>(500, ?logger = logger)

    // add an entry
    let entry = DeadLetterHelpers.createEntry (null: HttpResponseMessage) "evt-1" "timeout" "publish" "origin" 0 None
    q.AddAsync(entry).Wait()

    // obtain stats
    let stats = q.GetStatsAsync().Result
    printfn "Total entries: %d" stats.TotalEntries

    // replay with a fake processor
    let processor (resp: HttpResponseMessage) : Task<Result<unit,string>> =
        Task.FromResult(Ok ())
    let replayed = q.ReplayAsync(processor, maxConcurrency = 4, maxRetries = 3, removeOnMaxRetries = true).Result
    printfn "Replayed %d entries" replayed
*)
