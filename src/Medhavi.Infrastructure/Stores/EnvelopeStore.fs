namespace Medhavi.Infrastructure.Stores.EnvelopeStore

open System
open System.Threading.Tasks
open Medhavi.Common.Patterns
open Medhavi.SharedKernel.Contracts

/// Position information captured from the event store
type Position =
    { GlobalPosition: int64 option // e.g., commit/prepare/global pos (for $all / category resume)
      StreamPosition: int64 option // event number within stream (stream revision)
      CommitPosition: int64 option } // commit/prepare position if available

    static member Default =
        { GlobalPosition = None
          StreamPosition = None
          CommitPosition = None }

type EnvelopedEvent =
    { Envelope: Envelope
      Position: Position
      ReadFrom: string option } // the stream we read from (e.g., "$all" or "$et-MyType")

type AppendResult =
    { StreamName: string
      Position: Position option
      NextExpectedStreamRevision: int64 option }

type SubscriptionMode =
    | All
    | Stream of string

type SubscriptionHandle = { Id: Guid; Unsubscribe: unit -> Task }

/// Expected revision / concurrency token for append operations.
type ExpectedRevision =
    | Any
    | NoStream
    | StreamRevision of int64

type EnvelopeStoreError =
    | ReadError of string
    | WriteError of string
    | TransientError of string
    | ConcurrencyError of string
    | UnknownError of string
    | Cancelled

/// Functional abstraction for event store operations.
/// All operations return TaskResult for better composability with Medhavi.Common patterns.
type EnvelopeStoreOps =
    {

        /// Append domain events (as Envelopes) to stream `streamName`.
        /// expectedRevision: pass Some(lastRevision) for optimistic concurrency, or None for NoStream.
        Publish:
            string
                -> Envelope list
                -> ExpectedRevision
                -> System.Threading.CancellationToken
                -> TaskResult<AppendResult, EnvelopeStoreError>
        PublishSingle:
            string
                -> Envelope
                -> ExpectedRevision
                -> Threading.CancellationToken
                -> TaskResult<AppendResult, EnvelopeStoreError>
        PublishAtomicOutbox:
            string
                -> Envelope list
                -> string
                -> Envelope
                -> ExpectedRevision
                -> Threading.CancellationToken
                -> TaskResult<AppendResult, EnvelopeStoreError>

        /// Read events from a named stream starting at an optional stream revision (inclusive).
        ReadStream:
            string
                -> Position option
                -> int option
                -> Threading.CancellationToken
                -> TaskResult<EnvelopedEvent list, EnvelopeStoreError>

        /// Read all events from global position
        ReadAll:
            Position option
                -> int option
                -> Threading.CancellationToken
                -> TaskResult<EnvelopedEvent list, EnvelopeStoreError>

        ReadLast:
            string
                -> int64 option
                -> Threading.CancellationToken
                -> TaskResult<EnvelopedEvent list, EnvelopeStoreError>
        GetLastRevision: string -> Threading.CancellationToken -> TaskResult<Position option, EnvelopeStoreError>

        /// Subscribe to a stream or all-stream; handler receives each Envelope (positions populated).
        /// Handler returns Task<unit> so caller can handle errors and DLQ accordingly.
        Subscribe:
            SubscriptionMode
                -> Position option
                -> (EnvelopedEvent -> Task<unit>)
                -> Threading.CancellationToken
                -> TaskResult<SubscriptionHandle, EnvelopeStoreError>

        ClearStream: string -> Threading.CancellationToken -> TaskResult<unit, EnvelopeStoreError>
    }

type StreamType =
    | Category
    | ByCategory
    | ByEvent
    | Global
    | System
    | Regular

    static member Get(stream: string) =
        match stream with
        // Special system streams
        | s when s.StartsWith("$ce-") -> StreamType.Category
        | "$all" -> StreamType.Global
        | s when s.StartsWith("$by_category") -> StreamType.ByCategory
        | s when s.StartsWith("$by_event_type") -> StreamType.ByEvent
        | s when s.StartsWith("$") -> StreamType.System
        | _ -> StreamType.Regular

module EnvelopeStore =
    let posToLong =
        function
        | None -> Int64.MinValue
        | Some v -> v
