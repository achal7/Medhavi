namespace Medhavi.SharedKernel

open System
open Medhavi.Common.Serialization

type Envelope =
    {
        /// Unique id for the recorded event
        EventId: Guid

        /// Event type for routing, filtering and decoding
        EventType: string

        /// Serialized domain event
        DataJson: string

        /// Schema version for evolution handling
        SchemaVersion: int

        /// Stream name where the event was written
        /// Useful when reading from global subscriptions or when projection needs origin stream.
        StreamName: string

        /// Infrastructure timestamp (UTC) when event was created/persisted
        CreatedUtc: DateTimeOffset

        CorrelationId: Guid option
        CausationId: Guid option
        TenantId: string option

        /// Extensible metadata for tracing and context
        Metadata: Map<string, string>

    }

module Envelope =

    /// Safely parse a Guid from string; return None on failure.
    let private tryGuid (s: string) =
        match Guid.TryParse s with
        | true, g -> Some g
        | _ -> None

    /// Create envelope with current timestamp
    let createEnvelope (eventType: string) (dataJson: string) (version: int) : Envelope =
        { EventType = eventType
          DataJson = dataJson
          EventId = Guid.NewGuid()
          SchemaVersion = version
          Metadata = Map.empty
          CreatedUtc = DateTimeOffset.UtcNow
          StreamName = ""
          CorrelationId = None
          CausationId = None
          TenantId = None }

    let withMetadata (key: string) (value: string) (envelope: Envelope) : Envelope =
        let newMeta = envelope.Metadata |> Map.add key value

        // Extract typed correlation + causation + tenant from metadata (if present)
        let correlationId =
            match Map.tryFind "correlationId" newMeta with
            | Some v -> tryGuid v
            | None -> envelope.CorrelationId

        let causationId =
            match Map.tryFind "causationId" newMeta with
            | Some v -> tryGuid v
            | None -> envelope.CausationId

        let tenantId =
            match Map.tryFind "tenantId" newMeta with
            | Some v -> Some v
            | None -> envelope.TenantId

        { envelope with
            Metadata = newMeta
            CorrelationId = correlationId
            CausationId = causationId
            TenantId = tenantId }

    /// Rehydrate from full metadata map
    let withMetadataMap (metadata: Map<string, string>) (envelope: Envelope) : Envelope =
        let correlationId =
            metadata
            |> Map.tryFind "correlationId"
            |> Option.bind tryGuid

        let causationId =
            metadata
            |> Map.tryFind "causationId"
            |> Option.bind tryGuid

        let tenantId =
            metadata
            |> Map.tryFind "tenantId"

        { envelope with
            Metadata = metadata
            CorrelationId = correlationId
            CausationId = causationId
            TenantId = tenantId }

    let withCorrelationId (id: string) (envelope: Envelope) : Envelope = envelope |> withMetadata "correlationId" id

    let withCausationId (id: string) (envelope: Envelope) : Envelope = envelope |> withMetadata "causationId" id

    let withTenantId (id: string) (envelope: Envelope) : Envelope = envelope |> withMetadata "tenantId" id

    let withAggregateId (id: string) (envelope: Envelope) : Envelope = envelope |> withMetadata "aggregateId" id

    let withAggregateType (aggregateType: string) (envelope: Envelope) : Envelope =
        envelope
        |> withMetadata "aggregateType" aggregateType

    let withAggregateContext (aggregateId: string) (aggregateType: string) (envelope: Envelope) : Envelope =
        envelope
        |> withAggregateId aggregateId
        |> withAggregateType aggregateType

    let withTimestamp (timestamp: DateTimeOffset) (envelope: Envelope) : Envelope =
        envelope
        |> withMetadata "timestamp" (timestamp.ToString("O"))

    let withOriginInfo (streamName: string) (eventNumber: int64) (envelope: Envelope) : Envelope =
        envelope
        |> withMetadata "originStream" streamName
        |> withMetadata "originEventNumber" (eventNumber.ToString())

    let withPrincipal (principal: string) (envelope: Envelope) : Envelope =
        envelope |> withMetadata "principal" principal

    let withMessageId (messageId: string) (envelope: Envelope) : Envelope =
        envelope |> withMetadata "messageId" messageId

    /// Enrich envelope with tracing context
    let withTracingContext (correlationId: string) (causationId: string) (envelope: Envelope) : Envelope =
        envelope
        |> withCorrelationId correlationId
        |> withCausationId causationId

    /// Enrich envelope with full context (when you need rich tracing)
    let withFullContext
        (aggregateId: string)
        (aggregateType: string)
        (correlationId: string)
        (causationId: string)
        (timestamp: DateTimeOffset)
        (envelope: Envelope)
        : Envelope =

        envelope
        |> withAggregateContext aggregateId aggregateType
        |> withTracingContext correlationId causationId
        |> withTimestamp timestamp

    /// Apply ExecutionContext metadata to envelope (for distributed tracing)
    let withExecutionContext (ctx: ExecutionContext) (envelope: Envelope) : Envelope =
        let metadataMap = ExecutionContext.toMetadataMap ctx

        { envelope with
            Metadata =
                metadataMap
                |> Map.fold (fun acc key value -> Map.add key value acc) envelope.Metadata }

    // small helper to extract telemetry-friendly context
    let toTelemetryContext (env: Envelope) =
        Map.empty
        |> Map.add "stream" env.StreamName
        |> Map.add "event_type" env.EventType

    let deserialize<'T> (envelope: Envelope) : Result<'T, SerializationError> = deserialize<'T> (envelope.DataJson)

    let serialize (value: obj) : Result<string, SerializationError> = Medhavi.Common.Serialization.serialize (value)

    let tryGetMetadata (key: string) (env: Envelope) : string option = env.Metadata |> Map.tryFind key

    let tryGetAggregateId (env: Envelope) = tryGetMetadata "aggregateId" env
    let tryGetAggregateType (env: Envelope) = tryGetMetadata "aggregateType" env

    let tryGetCorrelationId (env: Envelope) : Guid option =
        tryGetMetadata "correlationId" env
        |> Option.bind (fun s ->
            match Guid.TryParse s with
            | true, g -> Some g
            | _ -> None)

    let tryGetCausationId (env: Envelope) : Guid option =
        tryGetMetadata "causationId" env
        |> Option.bind (fun s ->
            match Guid.TryParse s with
            | true, g -> Some g
            | _ -> None)

    let tryGetPrincipal (env: Envelope) : string option = tryGetMetadata "principal" env
    let tryGetTenantId (env: Envelope) : string option = env.TenantId
    let tryGetMessageId (env: Envelope) : string option = tryGetMetadata "messageId" env

    /// Build typed ExecutionContext from envelope (uses CreatedUtc as timestamp)
    let executionContextOf (env: Envelope) : ExecutionContext =
        ExecutionContext.fromMetadataMap env.Metadata env.CreatedUtc

    type EnvelopeRuntime =
        { Envelope: Envelope
          ExecutionContext: ExecutionContext }

    let toRuntime (env: Envelope) : EnvelopeRuntime =
        { Envelope = env
          ExecutionContext = executionContextOf env }

    let createCheckpointEnvelope (streamName: string) (payload: string) : Envelope =
        let env = createEnvelope "checkpoint" payload 1
        { env with StreamName = streamName; TenantId = None }

    let tryParsePositionFromData (env: Envelope) : int64 option =
        try
            let text = env.DataJson

            match Int64.TryParse text with
            | true, v -> Some v
            | _ -> None
        with _ ->
            None
