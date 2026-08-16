namespace Medhavi.Foundation.ExecutionContext

open System

type CorrelationId = private CorrelationId of Guid

module CorrelationId =
    let create () = CorrelationId(Guid.NewGuid())
    let value (CorrelationId id) = id
    let toString (CorrelationId id) = id.ToString()

    let fromString (s: string) =
        match Guid.TryParse s with
        | true, g -> Some(CorrelationId g)
        | false, _ -> None

/// Execution context for distributed tracing and command correlation
/// Enables tracking of operations across aggregates, services, and boundaries
type ExecutionContext =
    {
        /// Unique identifier for tracking requests across aggregates and services
        CorrelationId: CorrelationId

        /// ID of the command/event that caused this operation (for causality tracking)
        CausationId: CorrelationId option

        /// User/system that initiated the operation
        Principal: string option

        /// Timestamp when context was created
        Timestamp: DateTimeOffset

        /// Tenant identifier for multi-tenancy support (future)
        TenantId: string option

        /// Message ID from external system (for idempotency and audit trail)
        MessageId: string option

        CausalDecisionIds: string list
    }

module ExecutionContext =
    let create () =
        { CorrelationId = CorrelationId.create()
          CausationId = None
          Principal = None
          Timestamp = DateTimeOffset.UtcNow
          TenantId = None
          MessageId = None
          CausalDecisionIds = [] }

    let withPrincipal (principal: string) (ctx: ExecutionContext) = { ctx with Principal = Some principal }

    let withTenantId (tenantId: string) (ctx: ExecutionContext) = { ctx with TenantId = Some tenantId }

    let withMessageId (messageId: string) (ctx: ExecutionContext) = { ctx with MessageId = Some messageId }

    let withCausalDecisionIds (ids: string list) (ctx: ExecutionContext) = { ctx with CausalDecisionIds = ids }

    let asCausation (ctx: ExecutionContext) =
        { CorrelationId = CorrelationId.create()
          CausationId = Some ctx.CorrelationId
          Principal = ctx.Principal
          Timestamp = DateTimeOffset.UtcNow
          TenantId = ctx.TenantId
          MessageId = ctx.MessageId
          CausalDecisionIds = ctx.CausalDecisionIds }

    let toMetadataMap (ctx: ExecutionContext) =
        let m =
            Map.ofList
                [ "correlationId", ctx.CorrelationId |> CorrelationId.toString
                  "timestamp", ctx.Timestamp.ToString("O") ]

        let m =
            match ctx.CausationId with
            | Some c -> m |> Map.add "causationId" (c |> CorrelationId.toString)
            | None -> m

        let m =
            match ctx.Principal with
            | Some p -> m |> Map.add "principal" p
            | None -> m

        let m =
            match ctx.TenantId with
            | Some t -> m |> Map.add "tenantId" t
            | None -> m

        let m =
            match ctx.MessageId with
            | Some mid -> m |> Map.add "messageId" mid
            | None -> m

        m

    let fromMetadataMap (metadata: Map<string, string>) (createdUtc: DateTimeOffset) =
        let correlationId =
            metadata.TryFind "correlationId"
            |> Option.bind CorrelationId.fromString
            |> Option.defaultValue(CorrelationId.create())

        let causationId = metadata.TryFind "causationId" |> Option.bind CorrelationId.fromString
        let principal = metadata.TryFind "principal"
        let tenantId = metadata.TryFind "tenantId"
        let messageId = metadata.TryFind "messageId"

        let causalDecisionIds =
            metadata.TryFind "causalDecisionIds"
            |> Option.map(fun s -> s.Split ',')
            |> Option.map List.ofArray
            |> Option.defaultValue []

        { CorrelationId = correlationId
          CausationId = causationId
          Principal = principal
          Timestamp = createdUtc
          TenantId = tenantId
          MessageId = messageId
          CausalDecisionIds = causalDecisionIds }
