module Medhavi.Infrastructure.EnvelopeStoreDeps

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.ExecutionContext
open Medhavi.Infrastructure.Stores.EnvelopeStore

let create<'Event>
    (store: EnvelopeStoreOps)
    (getEventType: 'Event -> string)
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
                        | EventTypes types ->
                            List.contains envelopedEvent.Envelope.EventType types
                            || (let agg = Envelope.tryGetAggregateType envelopedEvent.Envelope |> Option.defaultValue ""
                                not(String.IsNullOrWhiteSpace agg) && List.contains agg types)
                        | Aggregate s ->
                            let agg = Envelope.tryGetAggregateType envelopedEvent.Envelope |> Option.defaultValue ""
                            agg = s || envelopedEvent.Envelope.AggregateType = s
                        | _ -> true

                    if shouldProcess then
                        let env = envelopedEvent.Envelope
                        let corrId =
                            env.CorrelationId
                            |> Option.defaultValue (CorrelationId.create())
                        let causeId =
                            match env.EventId with
                            | EventId g -> CorrelationId.fromString (g.ToString())
                        let ctx = ExecutionContext.fromCorrelationAndCausation corrId causeId
                        ExecutionContextHolder.Set(ctx)
                        try
                            do! handler env
                        finally
                            ExecutionContextHolder.Clear()
                }

            let! result = store.Subscribe mode None wrappedHandler cancellationToken

            match result with
            | Ok handle ->
                return
                    { new IDisposable with
                        member _.Dispose() = handle.Unsubscribe() |> Async.AwaitTask |> Async.RunSynchronously }
            | Error _ ->
                // Return no-op disposable on error
                return
                    { new IDisposable with
                        member _.Dispose() = () }
        }

    { GetEventType = getEventType
      SerializeEvent = serializeEvent
      DispatchEnvelope = dispatchEnvelope
      TraceCodec = traceCodec
      Subscribe = subscribe }
