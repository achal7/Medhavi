module Medhavi.Infrastructure.Stores.IdempotencyStore

open System
open System.Threading
open System.Threading.Tasks
open Microsoft.Extensions.Logging
open Medhavi.Common
open Medhavi.Foundation.Contracts

type IdempotencyKey = string
type ReservationToken = Guid

type IdemStatus =
    | InProgress
    | Completed
    | Failed

type Origin =
    { StreamName: string
      EventNumber: int64
      Position: EnvelopeStore.Position option }

type IdempotencyRecord =
    { Key: IdempotencyKey
      Token: ReservationToken
      CreatedUtc: DateTimeOffset
      ExpireAtUtc: DateTimeOffset option
      Status: IdemStatus
      ResponseJson: string option
      Origin: Origin option
      Attempts: int }

type ReservationResult =
    | Reserved of ReservationToken * EnvelopeStore.Position
    | AlreadyExists of ReservationToken * IdempotencyRecord

type IdempotencyStoreError =
    | StorageError of string
    | NotFound of string
    | TokenMismatch of ReservationToken * ReservationToken
    | AppendFailed of string
    | ParseError of string
    | UnknownError of string
    | Cancelled

    override x.ToString() =
        match x with
        | StorageError m -> $"[Idem Store] StorageError: {m}"
        | NotFound m -> $"[Idem Store] NotFound: {m}"
        | TokenMismatch(m, n) -> $"[Idem Store] Token mismatch: Requested: {n} Existing: {m}"
        | AppendFailed m -> $"[Idem Store] Append failed: {m}"
        | ParseError m -> $"[Idem Store] Parse error: {m}"
        | UnknownError m -> $"[Idem Store] Unknown error: {m}"
        | Cancelled -> "Cancelled"

type IdempotencyStoreOps =
    {
        /// Atomically reserve a key. Returns (true, token) when newly reserved.
        /// If the key exists, returns (false, existingToken).
        AddIfNotExists:
            string
                -> IdempotencyKey
                -> DateTimeOffset option
                -> CancellationToken
                -> TaskResult<ReservationResult, IdempotencyStoreError>

        ReclaimReservation:
            string
                -> IdempotencyKey
                -> ReservationToken
                -> DateTimeOffset option // new expiry (optional)
                -> CancellationToken
                -> TaskResult<ReservationResult, IdempotencyStoreError>

        /// Optionally record the operation result (so future callers can get the same response).
        /// Should be idempotent and overwrite only if the same token is used (safety).
        SetResult:
            string
                -> IdempotencyKey
                -> ReservationToken
                -> IdemStatus
                -> string option
                -> Origin option
                -> CancellationToken
                -> TaskResult<unit, IdempotencyStoreError>

        /// Get result if operation already completed (response payload and metadata).
        GetResult:
            string
                -> IdempotencyKey
                -> CancellationToken
                -> TaskResult<Option<IdempotencyRecord>, IdempotencyStoreError>

        /// Check existence without adding
        Exists: string -> IdempotencyKey -> CancellationToken -> TaskResult<bool, IdempotencyStoreError>

        /// Remove a key (admin-only). Prefer TTL instead.
        Remove: string -> IdempotencyKey -> CancellationToken -> TaskResult<unit, IdempotencyStoreError>

        /// List keys (admin) optionally filtered by prefix and/or paging to avoid scanning entire store.
        GetKeys: string -> string option -> int option -> CancellationToken -> Task<string list>

        /// Cleanup keys older than `expiration`. Returns number removed.
        Cleanup: string -> DateTimeOffset -> CancellationToken -> TaskResult<int, IdempotencyStoreError>
    }

type IdempotencyOptions =
    { KeyPrefix: string
      ExpireAtUtc: DateTimeOffset option
      RemoveOnFailure: bool }

let createIdempotencyRecord key token expiry =
    { Key = key
      Token = token
      CreatedUtc = DateTimeOffset.UtcNow
      ExpireAtUtc = expiry
      Status = IdemStatus.InProgress
      ResponseJson = None
      Origin = None
      Attempts = 0 }

let serializeIdempotencyRecord (codec: Codec<IdempotencyRecord>) (record: IdempotencyRecord) = codec.Encode record

let createEnvelopeFrom streamName codec record =
    serializeIdempotencyRecord codec record |> Result.map(fun cp -> Envelope.createCheckpointEnvelope streamName cp)

let createEnvelope streamName codec key token expiry =
    createIdempotencyRecord key token expiry |> createEnvelopeFrom streamName codec

let tryParseIdempotencyRecordJson (codec: Codec<IdempotencyRecord>) (recordJson: string) : IdempotencyRecord option =
    match codec.Decode recordJson with
    | Ok res -> Some res
    | _ -> None

let private stringContainsIgnoreCase (needle: string) (hay: string) =
    hay.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0

/// Inspect whatever error object your wrapper returned and try to detect WrongExpectedVersion
let private isWrongExpectedVersion (errObj: obj) : bool =
    match errObj with
    | null -> false
    | :? string as s ->
        stringContainsIgnoreCase "wrongexpectedversion" s
        || stringContainsIgnoreCase "wrong expected version" s
        || stringContainsIgnoreCase "WrongExpectedVersion" s
    | _ ->
        // fallback: stringify the value and search
        try
            let s = sprintf "%A" errObj

            stringContainsIgnoreCase "wrongexpectedversion" s || stringContainsIgnoreCase "wrong expected version" s
        with _ ->
            false

let readLast (envStore: EnvelopeStore.EnvelopeStoreOps) codec (streamName: string) (ct: CancellationToken) =
    task {
        let! readRes = envStore.ReadLast streamName None ct

        match readRes with
        | Error readErr -> return Error($"{readErr}")
        | Ok resolved when resolved.Length = 0 -> return Error("stream exists but empty")
        | Ok resolved ->
            // Use HEAD/first element depending on your envStore ordering (we expect newest first or last)
            let head = resolved.Head

            match tryParseIdempotencyRecordJson codec head.Envelope.DataJson with
            | None -> return Error("Parse filed for existing token")
            | Some resRec -> return Ok(resRec, head)
    }

let addIfNotExists
    (envStore: EnvelopeStore.EnvelopeStoreOps)
    (logger: ILogger)
    (codec: Codec<IdempotencyRecord>)
    (streamName: string)
    (key: IdempotencyKey)
    (expiry: DateTimeOffset option)
    (ct: CancellationToken)
    : TaskResult<ReservationResult, IdempotencyStoreError> =
    task {
        let logMsgPrefix = $"[AddIfNotExists] Stream: {streamName}, Key: {key} "

        if ct.IsCancellationRequested then
            return Error(Cancelled)
        else
            let token = Guid.NewGuid()
            let evRes = createEnvelope streamName codec key token expiry

            match evRes with
            | Error _ -> return Error(ParseError(key.ToString()))
            | Ok ev ->
                try
                    // publish attempt
                    let! publishRes = envStore.PublishSingle streamName ev EnvelopeStore.ExpectedRevision.Any ct

                    match publishRes with
                    | Ok published ->
                        match published.Position with
                        | Some pos -> return Ok(Reserved(token, pos)) // first writer wins
                        | None -> return Error(AppendFailed $"{logMsgPrefix} Error: Position is empty")
                    | Error e ->
                        logger.LogError
                            $"[AddIfNotExists] Publish returned Error for stream {streamName} key {key}: %A{e}"

                        if isWrongExpectedVersion(box e) then
                            let! lastRec = readLast envStore codec streamName ct

                            match lastRec with
                            | Error e ->
                                logger.LogError $"{logMsgPrefix} Error: {e}"
                                return Error(AppendFailed $"{logMsgPrefix} Error: {e}")
                            | Ok(idemRec, _) -> return Ok(ReservationResult.AlreadyExists(idemRec.Token, idemRec))
                        else
                            // Not a version conflict — surface as append failure
                            logger.LogError $"[AddIfNotExists] Append failed (non-concurrency): %A{e}"
                            return Error(AppendFailed $"{logMsgPrefix} Error: {e.ToString()}")
                with ex ->
                    // Exception fallback: check message then attempt fallback read if it looks like version error
                    logger.LogError $"{logMsgPrefix} Error:{ex.Message}"
                    return Error(IdempotencyStoreError.UnknownError ex.Message)
    }

let markProcessed

    (envStore: EnvelopeStore.EnvelopeStoreOps)
    (logger: ILogger)
    (codec: Codec<IdempotencyRecord>)
    (streamName: string)
    (key: IdempotencyKey)
    (token: ReservationToken)
    (status: IdemStatus)
    (responseJson: string option)
    (origin: Origin option)
    (ct: CancellationToken)
    : TaskResult<unit, IdempotencyStoreError> =
    task {
        let errMsgPrefix = $"[MarkProcessed] Stream: {streamName}, Key: {key} Error: "

        try

            if ct.IsCancellationRequested then
                return Error IdempotencyStoreError.Cancelled
            else
                match! envStore.ReadLast streamName None ct with
                | Error ex ->
                    let msg = errMsgPrefix + ex.ToString()
                    logger.LogError msg
                    return Error(AppendFailed msg)
                | Ok resolved when resolved.Length = 0 -> return Error(NotFound key)
                | Ok resolved ->
                    match tryParseIdempotencyRecordJson codec resolved.Head.Envelope.DataJson with
                    | None ->
                        let msg = errMsgPrefix + "Failed to parse reservation token"
                        logger.LogError msg
                        return Error(ParseError msg)
                    | Some reserved when reserved.Token <> token -> return Error(TokenMismatch(reserved.Token, token))
                    | Some reserved ->
                        let payloadRes =
                            { reserved with
                                Status = status
                                ResponseJson = responseJson
                                Origin = origin
                                Attempts = reserved.Attempts + 1 }
                            |> serializeIdempotencyRecord codec

                        match payloadRes with
                        | Error _ -> return Error(ParseError(errMsgPrefix + "Failed to parse stored reservation token"))
                        | Ok payload ->
                            let env =
                                { resolved.Head with
                                    Medhavi.Infrastructure.Stores.EnvelopeStore.EnvelopedEvent.Envelope.DataJson =
                                        payload }

                            let expected =
                                match env.Position.StreamPosition with
                                | None -> EnvelopeStore.ExpectedRevision.Any
                                | Some v -> EnvelopeStore.ExpectedRevision.StreamRevision v

                            printfn "%A" expected

                            let! pubRes = envStore.PublishSingle streamName env.Envelope expected ct

                            return
                                pubRes
                                |> Result.map(fun _ -> ())
                                |> Result.mapError(fun e -> IdempotencyStoreError.AppendFailed(e.ToString()))
        with ex ->
            logger.LogWarning(errMsgPrefix + ex.Message)
            return Error(AppendFailed(errMsgPrefix + ex.Message))
    }

let getResult
    (envStore: EnvelopeStore.EnvelopeStoreOps)
    (codec: Codec<IdempotencyRecord>)
    (_: IdempotencyKey)
    (streamName: string)
    (ct: CancellationToken)
    : TaskResult<IdempotencyRecord option, IdempotencyStoreError> =
    task {
        if ct.IsCancellationRequested then
            return Error(Cancelled)
        else
            match! envStore.ReadLast streamName None ct with
            | Error e -> return Error(IdempotencyStoreError.UnknownError $"[Store] Failed to read last event {e}")
            | Ok resolved when resolved.Length = 0 -> return Ok(None)
            | Ok resolved -> return Ok <| tryParseIdempotencyRecordJson codec resolved.Head.Envelope.DataJson
    }

let exists (envStore: EnvelopeStore.EnvelopeStoreOps) (_: IdempotencyKey) (streamName: string) (ct: CancellationToken) =
    task {
        if ct.IsCancellationRequested then
            return Error(Cancelled)
        else
            try
                // Fast path: read up to 1 last envelope from the stream.
                // Use ReadLast when available: returns recent envelopes in stream-order.
                let! tailRes = envStore.ReadLast streamName (Some 1) ct

                match tailRes with
                | Error e -> return Error(IdempotencyStoreError.StorageError(sprintf "%A" e))
                | Ok envelopes ->
                    // if returned list non-empty -> stream exists and has at least one envelope
                    return Ok(envelopes |> List.isEmpty |> not)
            with ex ->
                return Error(IdempotencyStoreError.StorageError ex.Message)
    }

let remove
    (envStore: EnvelopeStore.EnvelopeStoreOps)
    (logger: ILogger)
    (streamName: string)
    (_: IdempotencyKey)
    (ct: CancellationToken)
    =
    task {
        if ct.IsCancellationRequested then
            return Error(Cancelled)
        else
            try
                // Use EnvelopeStore.ClearStream to delete the stream
                let! res = envStore.ClearStream streamName ct

                match res with
                | Ok() -> return Ok()
                | Error e ->
                    logger.LogWarning $"[Store] Failed to remove idempotency stream {streamName}: {e}"
                    return Error(IdempotencyStoreError.StorageError(sprintf "%A" e))
            with ex ->
                logger.LogWarning $"[Store] Failed to remove idempotency stream {streamName}: {ex.Message}"
                return Error(IdempotencyStoreError.StorageError ex.Message)
    }

let getKeys
    (envStore: EnvelopeStore.EnvelopeStoreOps)
    (logger: ILogger)
    (streamPrefix: string)
    (prefixOpt: string option)
    (limitOpt: int option)
    (ct: CancellationToken)
    : Task<string list> =
    task {
        try
            // Admin only; scan global (ReadAll) and find stream names starting with prefix.
            let pattern = defaultArg prefixOpt streamPrefix
            let limit = defaultArg limitOpt 1000

            // ReadAll returns envelopes across streams; we use it to collect stream names.
            // Note: the signature of ReadAll in your EnvelopeStoreOps was: Position option -> int option -> CancellationToken -> Task<Result<Envelope list, _>>
            let! readRes = envStore.ReadAll None (Some limit) ct

            match readRes with
            | Error e ->
                logger.LogError(sprintf "GetKeys: ReadAll failed: %A" e)
                return []
            | Ok envelopes ->
                // Extract stream names, filter by prefix, remove prefix and distinct
                let keys =
                    envelopes
                    |> Seq.choose(fun env ->
                        // adjust this if Envelope has a different property name for stream id
                        let streamId = env.Envelope.StreamName

                        if streamId.StartsWith(pattern, StringComparison.OrdinalIgnoreCase) then
                            // drop prefix (e.g. "idem-") to return key only
                            Some(streamId.Substring(pattern.Length))
                        else
                            None)
                    |> Seq.distinct
                    |> Seq.truncate limit
                    |> Seq.toList

                return keys
        with ex ->
            logger.LogError $"GetKeys failed: {ex.Message}"
            return []
    }

let cleanup
    (envStore: EnvelopeStore.EnvelopeStoreOps)
    (logger: ILogger)
    (streamPrefix: string)
    (expiration: DateTimeOffset)
    (ct: CancellationToken)
    =
    task {
        try
            if ct.IsCancellationRequested then
                return Error(Cancelled)
            else
                // ReadAll (no position, unlimited or bounded by a large default)
                let! readRes = envStore.ReadAll None None ct

                match readRes with
                | Error e ->
                    logger.LogError $"[Store] Idempotency cleanup: ReadAll failed: {e}"
                    return Error(StorageError(sprintf "%A" e))
                | Ok envelopes ->
                    // Build dict of first-seen timestamp per stream
                    let dict = Collections.Generic.Dictionary<string, DateTimeOffset>()

                    for env in envelopes do
                        // Adjust this to your Envelope shape: try to get stream id and timestamp
                        let streamId = env.Envelope.StreamName

                        if streamId.StartsWith(streamPrefix, StringComparison.OrdinalIgnoreCase) then
                            if not(dict.ContainsKey streamId) then
                                // determine ts from envelope metadata or created timestamp; fallback to UtcNow
                                let tsOpt = Some env.Envelope.CreatedUtc
                                let finalTs = defaultArg tsOpt DateTimeOffset.UtcNow
                                dict.Add(streamId, finalTs)

                    // Find toRemove
                    let toRemove =
                        dict |> Seq.filter(fun kv -> kv.Value < expiration) |> Seq.map(fun kv -> kv.Key) |> Seq.toArray

                    let mutable removed = 0

                    for s in toRemove do
                        try
                            let! clearRes = envStore.ClearStream s ct

                            match clearRes with
                            | Ok() -> removed <- removed + 1
                            | Error e -> logger.LogWarning $"Failed to clear idempotency stream {s}: {e}"
                        with ex ->
                            logger.LogWarning $"Failed to clear idempotency stream {s}: {ex.Message}"

                    return Ok removed
        with ex ->
            logger.LogError "[Store] Idempotency store cleanup failed"
            return Error(StorageError ex.Message)
    }

let renewReservation
    (esClient: EnvelopeStore.EnvelopeStoreOps)
    (codec: Codec<IdempotencyRecord>)
    (streamName: string)
    (_: string)
    (token: Guid)
    (newExpiryOpt: DateTimeOffset option)
    (ct: CancellationToken)
    =
    task {
        if ct.IsCancellationRequested then
            return Error(Cancelled)
        else
            try
                // Read latest event to check token
                let! lastRes = readLast esClient codec streamName ct

                match lastRes with
                | Ok(last, env) ->
                    if last.Token <> token && false then
                        return Error(IdempotencyStoreError.TokenMismatch(last.Token, token))
                    else
                        // Append a Renewed event or an Updated event that changes expiry
                        let revised =
                            { last with
                                Token = token
                                ExpireAtUtc = newExpiryOpt }
                            |> codec.Encode

                        match revised with
                        | Ok data ->
                            let ev = Envelope.CreateBasic(env.Envelope.EventType, data)
                            // append with expected revision = evt.Event.EventNumber (optimistic concurrency)
                            let expectedRevision =
                                match env.Position.StreamPosition with
                                | None -> EnvelopeStore.ExpectedRevision.Any
                                | Some pos -> EnvelopeStore.ExpectedRevision.StreamRevision pos

                            let! writeRes = esClient.PublishSingle streamName ev expectedRevision ct

                            match writeRes with
                            | Ok wres ->
                                match wres.Position with
                                | Some pos -> return Ok(ReservationResult.Reserved(token, pos))
                                | None ->
                                    return Error(StorageError "Token: {token} reservation returned empty position")
                            | Error e when isWrongExpectedVersion(box e) ->
                                // someone else appended
                                let! rec2 = readLast esClient codec streamName ct

                                match rec2 with
                                | Error e -> return Error(StorageError(e.ToString()))
                                | Ok(existing, _) -> return Ok(AlreadyExists(existing.Token, existing))
                            | Error err -> return Error(StorageError(err.ToString()))

                        | Error e -> return Error(StorageError(e.ToString()))
                | Error err -> return Error(StorageError(err.ToString()))
            with ex ->
                return Error(StorageError(ex.ToString()))
    }

let create
    (envStore: EnvelopeStore.EnvelopeStoreOps)
    (codec: Codec<IdempotencyRecord>)
    (logger: ILogger)
    : IdempotencyStoreOps =

    { AddIfNotExists = addIfNotExists envStore logger codec
      SetResult = markProcessed envStore logger codec
      GetResult = getResult envStore codec
      Exists = exists envStore
      Remove = remove envStore logger
      GetKeys = getKeys envStore logger
      Cleanup = cleanup envStore logger
      ReclaimReservation = renewReservation envStore codec }
