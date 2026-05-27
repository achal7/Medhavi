namespace Medhavi.SharedKernel

open System
open Medhavi.SharedKernel.Logging

/// Execution context for distributed tracing and command correlation
/// Enables tracking of operations across aggregates, services, and boundaries
type ExecutionContext =
    {
        /// Unique identifier for tracking requests across aggregates and services
        CorrelationId: Guid

        /// ID of the command/event that caused this operation (for causality tracking)
        CausationId: Guid option

        /// User/system that initiated the operation
        Principal: string option

        /// Timestamp when context was created
        Timestamp: DateTimeOffset

        /// Tenant identifier for multi-tenancy support (future)
        TenantId: string option

        /// Message ID from external system (for idempotency and audit trail)
        MessageId: string option
    }

module ExecutionContext =

    /// Create new execution context with fresh correlation ID
    let create () : ExecutionContext =
        { CorrelationId = Guid.NewGuid()
          CausationId = None
          Principal = None
          Timestamp = DateTimeOffset.UtcNow
          TenantId = None
          MessageId = None }

    /// Create context with specific correlation ID
    let withCorrelationId (correlationId: Guid) (ctx: ExecutionContext) : ExecutionContext =
        { ctx with
            CorrelationId = correlationId }

    /// Add principal (user) to context
    let withPrincipal (principal: string) (ctx: ExecutionContext) : ExecutionContext =
        { ctx with Principal = Some principal }

    /// Add tenant ID to context
    let withTenantId (tenantId: string) (ctx: ExecutionContext) : ExecutionContext =
        { ctx with TenantId = Some tenantId }

    /// Add message ID to context (from external system)
    let withMessageId (messageId: string) (ctx: ExecutionContext) : ExecutionContext =
        { ctx with MessageId = Some messageId }

    /// Create child context (causation = parent's correlation)
    /// Useful for saga orchestration and multi-step workflows
    let asCausation (ctx: ExecutionContext) : ExecutionContext =
        { CorrelationId = Guid.NewGuid()
          CausationId = Some ctx.CorrelationId
          Principal = ctx.Principal
          Timestamp = DateTimeOffset.UtcNow
          TenantId = ctx.TenantId
          MessageId = ctx.MessageId // Preserve MessageId in child context
        }

    /// Create context from correlation and causation IDs (for integration scenarios)
    let fromIds (correlationId: Guid) (causationId: Guid option) : ExecutionContext =
        { CorrelationId = correlationId
          CausationId = causationId
          Principal = None
          Timestamp = DateTimeOffset.UtcNow
          TenantId = None
          MessageId = None }

    /// Extract execution context from envelope metadata
    let fromMetadataMap (metadata: Map<string, string>) (createdUtc: DateTimeOffset) : ExecutionContext =
        let correlationId =
            metadata.TryFind "correlationId"
            |> Option.bind (fun s ->
                match Guid.TryParse s with
                | true, g -> Some g
                | false, _ -> None)
            |> Option.defaultValue Guid.Empty

        let causationId =
            metadata.TryFind "causationId"
            |> Option.bind (fun s ->
                match Guid.TryParse s with
                | true, g -> Some g
                | false, _ -> None)

        let principal = metadata.TryFind "principal"
        let tenantId = metadata.TryFind "tenantId"
        let messageId = metadata.TryFind "messageId"

        { CorrelationId = correlationId
          CausationId = causationId
          Principal = principal
          Timestamp = createdUtc
          TenantId = tenantId
          MessageId = messageId }

    /// Enrich envelope with execution context metadata
    let toMetadataMap (ctx: ExecutionContext) : Map<string, string> =
        let baseMetadata =
            Map.empty
            |> Map.add "correlationId" (ctx.CorrelationId.ToString())
            |> Map.add
                "causationId"
                (ctx.CausationId
                 |> Option.map (fun id -> id.ToString())
                 |> Option.defaultValue (ctx.CorrelationId.ToString()))
            |> Map.add "timestamp" (ctx.Timestamp.ToString("O"))

        let withPrincipal =
            match ctx.Principal with
            | Some principal -> baseMetadata |> Map.add "principal" principal
            | None -> baseMetadata

        let withTenant =
            match ctx.TenantId with
            | Some tenantId -> withPrincipal |> Map.add "tenantId" tenantId
            | None -> withPrincipal

        match ctx.MessageId with
        | Some messageId -> withTenant |> Map.add "messageId" messageId
        | None -> withTenant

    // =================================================================================================
    // EXECUTION CONTEXT ↔ LOG CONTEXT BRIDGE
    // =================================================================================================

    /// Convert ExecutionContext to LogContext
    let fromExecutionContext (execCtx: ExecutionContext) (comp: string) : LogContext =
        { CorrelationId = Some execCtx.CorrelationId
          Operation = None
          Component = comp
          EntityId = None
          EntityType = None
          StreamName = None
          EventId = None
          EventType = None
          Duration = None
          AdditionalData =
            [ if execCtx.CausationId.IsSome then
                  ("CausationId", box execCtx.CausationId.Value)
              if execCtx.Principal.IsSome then
                  ("Principal", box execCtx.Principal.Value)
              if execCtx.TenantId.IsSome then
                  ("TenantId", box execCtx.TenantId.Value)
              ("Timestamp", box execCtx.Timestamp) ]
            |> Map.ofList
            |> Some }

    /// Extract ExecutionContext from LogContext (pure function)
    let toExecutionContext (logCtx: LogContext) : ExecutionContext option =
        logCtx.CorrelationId
        |> Option.map (fun correlationId ->
            let causationId =
                logCtx.AdditionalData
                |> Option.bind (fun data ->
                    data.TryFind "CausationId"
                    |> Option.map (fun v -> v :?> Guid))

            let messageId =
                logCtx.AdditionalData
                |> Option.bind (fun data ->
                    data.TryFind "MessageId"
                    |> Option.map (fun v -> v :?> string))

            let principal =
                logCtx.AdditionalData
                |> Option.bind (fun data ->
                    data.TryFind "Principal"
                    |> Option.map (fun v -> v :?> string))

            let tenantId =
                logCtx.AdditionalData
                |> Option.bind (fun data ->
                    data.TryFind "TenantId"
                    |> Option.map (fun v -> v :?> string))

            let timestamp =
                logCtx.AdditionalData
                |> Option.bind (fun data ->
                    data.TryFind "Timestamp"
                    |> Option.map (fun v -> v :?> DateTimeOffset))
                |> Option.map (fun t -> t.ToUniversalTime())
                |> Option.defaultValue DateTimeOffset.UtcNow

            { CorrelationId = correlationId
              MessageId = messageId
              CausationId = causationId
              Principal = principal
              TenantId = tenantId
              Timestamp = timestamp })

// TODO - Open item (Moved from Envelope to here)
/// Apply ExecutionContext metadata to envelope (for distributed tracing)
// let withExecutionContext (ctx: ExecutionContext) (envelope: Envelope) : Envelope =
//     let metadataMap = ExecutionContext.toMetadataMap ctx

//     { envelope with
//         Metadata =
//             metadataMap
//             |> Map.fold (fun acc key value -> Map.add key value acc) envelope.Metadata }

// TODO - Open item (Moved from Envelope to here)
/// Build typed ExecutionContext from envelope (uses CreatedUtc as timestamp)
// let executionContextOf (env: Envelope) : ExecutionContext =
//     ExecutionContext.fromMetadataMap env.Metadata env.CreatedUtc

// type EnvelopeRuntime =
//     { Envelope: Envelope
//         ExecutionContext: ExecutionContext }

// let toRuntime (env: Envelope) : EnvelopeRuntime =
//     { Envelope = env
//         ExecutionContext = executionContextOf env }

type ContextWrapper<'Payload> =
    { Context: ExecutionContext
      Payload: 'Payload }

type ExecutionContextHolder =
    static member val private CurrentContext = System.Threading.AsyncLocal<ExecutionContext>() with get

    static member Set(ctx: ExecutionContext) = ExecutionContextHolder.CurrentContext.Value <- ctx

    static member TryGet() =
        let value = ExecutionContextHolder.CurrentContext.Value

        if obj.ReferenceEquals(value, null) then
            None
        else
            Some value

    static member Clear() =
        ExecutionContextHolder.CurrentContext.Value <- Unchecked.defaultof<ExecutionContext>

module ExecutionContextValidation =
    let requireTenant (ctx: ExecutionContext) : Result<string, DomainError> =
        match ctx.TenantId with
        | Some tenantId when not (String.IsNullOrWhiteSpace tenantId) -> Ok tenantId
        | _ -> Error(DomainError.validation "TenantId is required but was missing or empty")
