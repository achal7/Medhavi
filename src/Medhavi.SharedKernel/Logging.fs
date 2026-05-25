namespace Medhavi.SharedKernel.Logging

open System
open System.Diagnostics
open System.Threading.Tasks
open Microsoft.Extensions.Logging
open Microsoft.FSharp.Control
open Medhavi.Common.Result

/// Log context for structured logging
type LogContext =
    { CorrelationId: Guid option
      Operation: string option
      Component: string
      EntityId: string option
      EntityType: string option
      StreamName: string option
      EventId: Guid option
      EventType: string option
      Duration: TimeSpan option
      AdditionalData: Map<string, obj> option }

    static member Empty =
        { CorrelationId = None
          Operation = None
          Component = ""
          EntityId = None
          EntityType = None
          StreamName = None
          EventId = None
          EventType = None
          Duration = None
          AdditionalData = None }

    member ctx.withEntityId (entityKey: string) (entityId: string) : LogContext =
        let additionalData =
            ctx.AdditionalData
            |> Option.defaultValue Map.empty
            |> Map.add entityKey (box entityId)
            |> Some

        { ctx with
            AdditionalData = additionalData }

    /// Add multiple business entity IDs to LogContext's AdditionalData
    member ctx.withEntityIds(entities: (string * string) list) : LogContext =
        let additionalData =
            entities
            |> List.fold
                (fun acc (key, value) -> Map.add key (box value) acc)
                (ctx.AdditionalData
                 |> Option.defaultValue Map.empty)
            |> Some

        { ctx with
            AdditionalData = additionalData }

    /// Extract business entity ID from LogContext's AdditionalData
    member ctx.getEntityId(entityKey: string) : string option =
        ctx.AdditionalData
        |> Option.bind (fun data -> data.TryFind entityKey)
        |> Option.map (fun v -> v :?> string)

module LogContext =
    /// Merge two LogContexts, with override taking precedence for Some values
    let mergeContexts (baseCtx: LogContext) (overrideCtx: LogContext) : LogContext =
        { CorrelationId =
            overrideCtx.CorrelationId
            |> Option.orElse baseCtx.CorrelationId
          Operation =
            overrideCtx.Operation
            |> Option.orElse baseCtx.Operation
          Component =
            if overrideCtx.Component <> "" then
                overrideCtx.Component
            else
                baseCtx.Component
          EntityId =
            overrideCtx.EntityId
            |> Option.orElse baseCtx.EntityId
          EntityType =
            overrideCtx.EntityType
            |> Option.orElse baseCtx.EntityType
          StreamName =
            overrideCtx.StreamName
            |> Option.orElse baseCtx.StreamName
          EventId =
            overrideCtx.EventId
            |> Option.orElse baseCtx.EventId
          EventType =
            overrideCtx.EventType
            |> Option.orElse baseCtx.EventType
          Duration =
            overrideCtx.Duration
            |> Option.orElse baseCtx.Duration
          AdditionalData =
            match baseCtx.AdditionalData, overrideCtx.AdditionalData with
            | Some baseMap, Some overrideMap -> Some(Map.fold (fun acc k v -> Map.add k v acc) baseMap overrideMap)
            | Some baseMap, None -> Some baseMap
            | None, Some overrideMap -> Some overrideMap
            | None, None -> None }

    let contextToState (context: LogContext) : (string * obj) seq =
        seq {
            if context.CorrelationId.IsSome then
                yield ("CorrelationId", context.CorrelationId.Value :> obj)

            if context.Operation.IsSome then
                yield ("Operation", context.Operation.Value :> obj)

            yield ("Component", context.Component :> obj)

            if context.EntityId.IsSome then
                yield ("EntityId", context.EntityId.Value :> obj)

            if context.EntityType.IsSome then
                yield ("EntityType", context.EntityType.Value :> obj)

            if context.StreamName.IsSome then
                yield ("StreamName", context.StreamName.Value :> obj)

            if context.EventId.IsSome then
                yield ("EventId", context.EventId.Value :> obj)

            if context.EventType.IsSome then
                yield ("EventType", context.EventType.Value :> obj)

            if context.Duration.IsSome then
                yield ("Duration", context.Duration.Value.TotalMilliseconds :> obj)

            if context.AdditionalData.IsSome then
                for kv in context.AdditionalData.Value do
                    yield (kv.Key, kv.Value)
        }

    let logDebug (logger: ILogger) (message: string) (context: LogContext) =
        logger.LogDebug(message, contextToState context |> Seq.toArray)

    let logInformation (logger: ILogger) (message: string) (context: LogContext) =
        logger.LogInformation(message, contextToState context |> Seq.toArray)

    let logWarning (logger: ILogger) (message: string) (context: LogContext) =
        logger.LogWarning(message, contextToState context |> Seq.toArray)

    let logError (logger: ILogger) (message: string) (context: LogContext) =
        logger.LogError(message, contextToState context |> Seq.toArray)

    let logErrorWithException (logger: ILogger) (ex: exn) (message: string) (context: LogContext) =
        logger.LogError(ex, message, contextToState context |> Seq.toArray)

    let logCritical (logger: ILogger) (message: string) (context: LogContext) =
        logger.LogCritical(message, contextToState context |> Seq.toArray)

    let logPerformance (logger: ILogger) (operation: string) (comp: string) (duration: TimeSpan) (context: LogContext) =
        let context =
            { LogContext.Empty with
                Operation = Some operation
                Component = comp
                Duration = Some duration }

        if duration.TotalMilliseconds > 1000.0 then
            logWarning logger $"Operation '{operation}' took {duration.TotalMilliseconds:F2}ms" context
        elif duration.TotalMilliseconds > 100.0 then
            logInformation logger $"Operation '{operation}' completed in {duration.TotalMilliseconds:F2}ms" context
        else
            logDebug logger $"Operation '{operation}' completed in {duration.TotalMilliseconds:F2}ms" context

// =================================================================================================
// COMPONENT NAMING HELPERS (Hierarchical Component Identification)
// =================================================================================================

/// Component naming helpers for hierarchical component identification
module ComponentNaming =

    /// Create hierarchical component name
    let combine (parts: string list) : string =
        parts
        |> List.filter (fun s -> not (String.IsNullOrWhiteSpace s))
        |> String.concat "."

    /// Actor component names
    module Actor =
        let aggregate (aggregateType: string) = combine [ "Actor"; "Aggregate"; aggregateType ]
        let projection (projectionName: string) = combine [ "Actor"; "Projection"; projectionName ]
        let broker (brokerName: string) = combine [ "Actor"; "Broker"; brokerName ]
        let saga (sagaName: string) = combine [ "Actor"; "Saga"; sagaName ]
        let root = "Actor.Root"

    /// Projection component names
    module Projection =
        let projection (projectionName: string) = combine [ "Projection"; projectionName ]

    /// Integration component names
    module Integration =
        let broker (brokerName: string) = combine [ "Integration"; "Broker"; brokerName ]
        let orchestrator = "Integration.Orchestrator"
        let publisher (publisherName: string) = combine [ "Integration"; "Publisher"; publisherName ]

    /// Store component names
    module Store =
        let eventStore = "Store.EventStore"
        let idempotencyStore = "Store.Idempotency"
        let checkpointStore = "Store.Checkpoint"
        let snapshotStore = "Store.Snapshot"

    /// Service component names
    module Service =
        let service (serviceName: string) = combine [ "Service"; serviceName ]
