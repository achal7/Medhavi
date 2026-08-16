module Medhavi.Infrastructure.EnvelopeStoreDeps

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Infrastructure.Stores.EnvelopeStore

/// Creates EnvelopeStoreDependencies from an EnvelopeStoreOps implementation
let create<'Event>
    (store: EnvelopeStoreOps)
    (serializeEvent: 'Event -> Result<string, string>)
    (traceCodec: Codec<DecisionTrace>)
    : EnvelopeStoreDependencies<'Event> =

    let dispatchEnvelope (envelope: Envelope) : Task<unit> =
        task {
            let streamName = $"category-{envelope.EventType}"
            let! result = store.PublishSingle streamName envelope ExpectedRevision.Any CancellationToken.None

            match result with
            | Ok _ -> return ()
            | Error err ->
                // Log error but don't fail - dispatch is fire-and-forget
                printfn $"Failed to dispatch envelope {envelope.EventId}: {err}"
        }

    let subscribe
        (filter: EnvelopeFilter)
        (handler: Envelope -> Task<unit>)
        (cancellationToken: CancellationToken)
        : Task<IDisposable> =

        task {
            let mode =
                match filter with
                | EnvelopeFilter.All -> SubscriptionMode.All
                | EnvelopeFilter.EventTypes _ -> SubscriptionMode.All // Filter in handler
                | EnvelopeFilter.Aggregate s -> SubscriptionMode.Stream s
                | EnvelopeFilter.Capability c -> SubscriptionMode.Stream $"$ce-{c}"

            let wrappedHandler (envelopedEvent: EnvelopedEvent) =
                task {
                    // Apply event type filter if specified
                    let shouldProcess =
                        match filter with
                        | EventTypes types -> List.contains envelopedEvent.Envelope.EventType types
                        | _ -> true

                    if shouldProcess then
                        do! handler envelopedEvent.Envelope
                }

            let! result = store.Subscribe mode None wrappedHandler cancellationToken

            match result with
            | Ok handle ->
                return
                    { new IDisposable with
                        member _.Dispose() = handle.Unsubscribe() |> Async.AwaitTask |> Async.RunSynchronously }
            | Error err ->
                // Return no-op disposable on error
                return
                    { new IDisposable with
                        member _.Dispose() = () }
        }

    { SerializeEvent = serializeEvent
      DispatchEnvelope = dispatchEnvelope
      TraceCodec = traceCodec
      Subscribe = subscribe }
