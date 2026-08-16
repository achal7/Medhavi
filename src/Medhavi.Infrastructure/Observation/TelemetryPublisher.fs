namespace Medhavi.Infrastructure.Observation

open System
open Medhavi.Foundation
open Medhavi.Foundation.Observation
open Medhavi.Foundation.ExecutionContext

/// Publishes ArchitecturalKnowledge as TelemetryEvent to the DomainEventBus.
module TelemetryPublisher =

    let private knowledgeToEvent (knowledge: ArchitecturalKnowledge) : TelemetryEvent =
        let correlationId =
            knowledge.Attributes
            |> Map.tryFind "CorrelationId"
            |> Option.bind(fun v ->
                match v with
                | :? CorrelationId as c -> Some c
                | _ -> None)

        let causationId =
            knowledge.Attributes
            |> Map.tryFind "CausationId"
            |> Option.bind(fun v ->
                match v with
                | :? CorrelationId as c -> Some c
                | _ -> None)

        let traceId = knowledge.Attributes |> Map.tryFind "TraceId" |> Option.map string

        let spanId = knowledge.Attributes |> Map.tryFind "SpanId" |> Option.map string

        let severity =
            match knowledge.Attributes.TryFind "Severity" with
            | Some(:? TelemetrySeverity as s) -> s
            | _ -> TelemetrySeverity.Information

        { EventId = Guid.NewGuid()
          Timestamp = knowledge.Timestamp
          Severity = severity
          Message = knowledge.Name
          Properties = knowledge.Attributes
          CorrelationId = correlationId
          CausationId = causationId
          TraceId = traceId
          SpanId = spanId }

    /// Create a KnowledgeRepresentation that publishes TelemetryEvents to the DomainEventBus.
    let toKnowledgeRepresentation (dispatchTelemetry: TelemetryEvent -> unit) : KnowledgeRepresentation =
        fun knowledge ->
            let event = knowledgeToEvent knowledge
            dispatchTelemetry event
