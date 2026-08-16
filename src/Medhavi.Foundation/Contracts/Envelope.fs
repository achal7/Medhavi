namespace Medhavi.Foundation.Contracts

open System
open Medhavi.Foundation.ExecutionContext

type EventId = EventId of Guid

module EventId =
    let create () = EventId(Guid.NewGuid())
    let value (EventId id) = id
    let toString (EventId id) = id.ToString()

    let fromString (s: string) =
        match Guid.TryParse s with
        | true, g -> Some(EventId g)
        | false, _ -> None

type Envelope =
    {
        /// Unique id for the recorded event
        EventId: EventId

        /// Event type for routing, filtering and decoding
        EventType: string

        AggregateId: string

        OccurrenceNumber: int64

        /// The type of aggregate that produced this event (e.g., "DemandObservation")
        AggregateType: string

        /// The domain that owns the producing capability (e.g., "D", "S", "C")
        DomainCode: string

        /// The capability that produced this event (e.g., "CA-D-001")
        CapabilityId: string

        /// Serialized domain event
        DataJson: string

        /// Schema version for evolution handling
        SchemaVersion: int

        StreamName: string

        /// Infrastructure timestamp (UTC) when event was created/persisted
        CreatedUtc: DateTimeOffset

        CorrelationId: CorrelationId option
        CausationId: CorrelationId option
        TenantId: string option

        /// Extensible metadata for tracing and context
        Metadata: Map<string, string>

    }

    static member Create
        (
            eventType: string,
            domainCode: string,
            aggregateType: string,
            aggregateId: string,
            occurrenceNumber: int64,
            capabilityId: string,
            dataJson: string,
            ?correlationId: CorrelationId,
            ?causationId: CorrelationId,
            ?metadata: Map<string, string>,
            ?tenantId: string,
            ?version: int,
            ?streamName: string
        ) : Envelope =
        { EventType = eventType
          DataJson = dataJson
          EventId = Guid.NewGuid() |> EventId
          SchemaVersion = version |> Option.defaultValue 1
          Metadata = metadata |> Option.defaultValue Map.empty
          CreatedUtc = DateTimeOffset.UtcNow
          CorrelationId = correlationId
          CausationId = causationId
          TenantId = tenantId
          AggregateId = aggregateId
          OccurrenceNumber = occurrenceNumber
          AggregateType = aggregateType
          DomainCode = domainCode
          CapabilityId = capabilityId
          StreamName = streamName |> Option.defaultValue "" }

    static member CreateBasic(eventType: string, dataJson: string) : Envelope =
        Envelope.Create(eventType, "", "", "", 0, "", dataJson)

/// Composable subscription criteria.
type SubscriptionCriteria =
    { DomainFilter: string option
      CapabilityFilter: string option
      EventTypeFilter: string option
      AggregateTypeFilter: string option }

module Envelope =
    open Medhavi.Common

    let matchesCriteria (criteria: SubscriptionCriteria) (env: Envelope) : bool =
        (criteria.DomainFilter |> Option.forall(fun d -> env.DomainCode = d))
        && (criteria.CapabilityFilter |> Option.forall(fun c -> env.CapabilityId = c))
        && (criteria.EventTypeFilter |> Option.forall(fun e -> env.EventType = e))
        && (criteria.AggregateTypeFilter |> Option.forall(fun a -> env.AggregateType = a))
    //(criteria.StreamFilter      |> Option.forall (fun s -> env.StreamName = s))

    /// Safely parse a Guid from string; return None on failure.
    let private tryGuid (s: string) =
        match Guid.TryParse s with
        | true, g -> Some g
        | _ -> None

    /// Create envelope with current timestamp
    // let createEnvelope (eventType: string) (dataJson: string) (version: int) : Envelope =
    //     { EventType = eventType
    //       DataJson = dataJson
    //       EventId = Guid.NewGuid() |> EventId
    //       SchemaVersion = version
    //       Metadata = Map.empty
    //       CreatedUtc = DateTimeOffset.UtcNow
    //       CorrelationId = None
    //       CausationId = None
    //       TenantId = None }

    let withMetadata (key: string) (value: string) (envelope: Envelope) : Envelope =
        let newMeta = envelope.Metadata |> Map.add key value

        // Extract typed correlation + causation + tenant from metadata (if present)
        let correlationId =
            match Map.tryFind "correlationId" newMeta with
            | Some v -> CorrelationId.fromString v
            | None -> envelope.CorrelationId

        let causationId =
            match Map.tryFind "causationId" newMeta with
            | Some v -> CorrelationId.fromString v
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
        let correlationId = metadata |> Map.tryFind "correlationId" |> Option.bind CorrelationId.fromString

        let causationId = metadata |> Map.tryFind "causationId" |> Option.bind CorrelationId.fromString

        let tenantId = metadata |> Map.tryFind "tenantId"

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
        envelope |> withMetadata "aggregateType" aggregateType

    let withAggregateContext (aggregateId: string) (aggregateType: string) (envelope: Envelope) : Envelope =
        envelope |> withAggregateId aggregateId |> withAggregateType aggregateType

    let withTimestamp (timestamp: DateTimeOffset) (envelope: Envelope) : Envelope =
        envelope |> withMetadata "timestamp" (timestamp.ToString("O"))

    let withOriginInfo (streamName: string) (eventNumber: int64) (envelope: Envelope) : Envelope =
        envelope |> withMetadata "originStream" streamName |> withMetadata "originEventNumber" (eventNumber.ToString())

    let withPrincipal (principal: string) (envelope: Envelope) : Envelope =
        envelope |> withMetadata "principal" principal

    let withMessageId (messageId: string) (envelope: Envelope) : Envelope =
        envelope |> withMetadata "messageId" messageId

    let withCausalDecisionIds (ids: string list) (envelope: Envelope) : Envelope =
        let json = System.Text.Json.JsonSerializer.Serialize(ids)
        withMetadata "causalDecisionIds" json envelope

    let withExecutionContext (ctx: ExecutionContext) (env: Envelope) =
        { env with
            CorrelationId = Some ctx.CorrelationId
            CausationId = ctx.CausationId
            TenantId = ctx.TenantId
            Metadata =
                env.Metadata
                |> Map.add "correlationId" (ctx.CorrelationId |> CorrelationId.toString)
                |> (fun m ->
                    match ctx.CausationId with
                    | Some c -> Map.add "causationId" (c |> CorrelationId.toString) m
                    | None -> m)
                |> (fun m ->
                    match ctx.Principal with
                    | Some p -> Map.add "principal" p m
                    | None -> m)
                |> (fun m ->
                    match ctx.MessageId with
                    | Some mid -> Map.add "messageId" mid m
                    | None -> m) }

    let withDecisionTrace (codec: Codec<DecisionTrace>) (trace: DecisionTrace) (env: Envelope) =
        codec.Encode trace
        |> Result.map(fun json ->
            { env with
                Metadata = env.Metadata |> Map.add "decisionTrace" json })

    /// Enrich envelope with tracing context
    let withTracingContext (correlationId: string) (causationId: string) (envelope: Envelope) : Envelope =
        envelope |> withCorrelationId correlationId |> withCausationId causationId

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

    let deserializeWith<'T> (codec: Codec<'T>) (envelope: Envelope) : Result<'T, CodecError> =
        codec.Decode envelope.DataJson

    // let createFromValue<'T>
    //     (codec: Codec<'T>)
    //     (eventType: string)
    //     (version: int)
    //     (value: 'T)
    //     : Result<Envelope, CodecError> =
    //     codec.Encode value |> Result.map(fun json -> createEnvelope eventType json version)

    let tryGetMetadata (key: string) (env: Envelope) : string option = env.Metadata |> Map.tryFind key

    let tryGetAggregateId (env: Envelope) = tryGetMetadata "aggregateId" env

    let tryGetAggregateType (env: Envelope) = tryGetMetadata "aggregateType" env

    let tryGetCorrelationId (env: Envelope) : Guid option =
        tryGetMetadata "correlationId" env
        |> Option.bind(fun s ->
            match Guid.TryParse s with
            | true, g -> Some g
            | _ -> None)

    let tryGetCausationId (env: Envelope) : Guid option =
        tryGetMetadata "causationId" env
        |> Option.bind(fun s ->
            match Guid.TryParse s with
            | true, g -> Some g
            | _ -> None)

    let tryGetPrincipal (env: Envelope) : string option = tryGetMetadata "principal" env
    let tryGetTenantId (env: Envelope) : string option = env.TenantId
    let tryGetMessageId (env: Envelope) : string option = tryGetMetadata "messageId" env

    let createCheckpointEnvelope (streamName: string) (payload: string) : Envelope =
        Envelope.Create("checkpoint", "", "", "", 1, "", payload, streamName = streamName)

    let tryParsePositionFromData (env: Envelope) : int64 option =
        try
            let text = env.DataJson

            match Int64.TryParse text with
            | true, v -> Some v
            | _ -> None
        with _ ->
            None

    let tryGetCausalDecisionIds (envelope: Envelope) : string list =
        tryGetMetadata "causalDecisionIds" envelope
        |> Option.bind(fun json ->
            try
                Some(System.Text.Json.JsonSerializer.Deserialize<string list>(json))
            with _ ->
                None)
        |> Option.defaultValue []

    let getCausionAndCorrelationGuid (envenlope: Envelope) =
        let cid = tryGetCorrelationId envenlope
        let caid = tryGetCausationId envenlope
        (cid, caid)
