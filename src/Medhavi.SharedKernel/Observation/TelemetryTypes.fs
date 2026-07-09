namespace Medhavi.SharedKernel.Observation

open System
open Medhavi.SharedKernel
open Medhavi.SharedKernel.ExecutionContext

type TelemetrySeverity =
    | Trace
    | Debug
    | Information
    | Warning
    | Errors
    | Critical

type TelemetryEvent = {
    EventId: Guid
    Timestamp: DateTimeOffset
    Severity: TelemetrySeverity
    Message: string
    Properties: Map<string, obj>
    CorrelationId: CorrelationId option
    CausationId: CorrelationId option
    TraceId: string option
    SpanId: string option
}

type MetricType =
    | Counter
    | Gauge
    | Histogram
    | Summary

type MetricPoint = {
    MetricName: string
    MetricType: MetricType
    Value: float
    Timestamp: Timestamp
    Tags: Map<string, string>
    Unit: string option
}

type PerformanceMeasurement = {
    OperationName: string
    Duration: TimeSpan
    Success: bool
    Timestamp: Timestamp
    Properties: Map<string, obj>
}

type LimiterType =
    | MaterialShortage of sku: string * nodeId: string
    | CapacityConstraint of resourceId: string
    | TransportDelay of legId: string
    | Other of reason: string

type LatencyTelemetry = {
    OperationName: string
    Component: string
    StartTime: Timestamp
    EndTime: Timestamp
    DurationMs: float
    IsSuccess: bool
    ErrorDetails: string option
    CorrelationId: CorrelationId
    TenantId: string option
    Metadata: Map<string, string>
}

type LimiterFrequencyTelemetry = {
    LimiterName: string
    Component: string
    Timestamp: Timestamp
    CurrentRate: float option
    ConfiguredLimitRate: float option
    Utilization: float option
    ThrottledCount: int64
    TotalEvaluatedCount: int64
    IsActive: bool
    Metadata: Map<string, string>
    CorrelationId: CorrelationId
    TenantId: string option
}

type TelemetryErrorMetric = {
    Component: string
    ErrorCode: string
    ErrorMessage: string
    CorrelationId: CorrelationId
    TenantId: string option
}

type TelemetryMetric =
    | Latency of LatencyTelemetry
    | LimiterFrequency of LimiterFrequencyTelemetry
    | ErrorEvent of TelemetryErrorMetric

module Telemetry =

    let createEvent (severity: TelemetrySeverity) (message: string) (properties: Map<string, obj>) =
        { EventId = Guid.NewGuid()
          Timestamp = DateTimeOffset.UtcNow
          Severity = severity
          Message = message
          Properties = properties
          CorrelationId = None
          CausationId = None
          TraceId = None
          SpanId = None }

    let withCorrelation (correlationId: CorrelationId) (causationId: CorrelationId option) (event: TelemetryEvent) =
        { event with CorrelationId = Some correlationId; CausationId = causationId }

    let withTracing (traceId: string) (spanId: string) (event: TelemetryEvent) =
        { event with TraceId = Some traceId; SpanId = Some spanId }

    let addProperty (key: string) (value: obj) (event: TelemetryEvent) =
        { event with Properties = event.Properties |> Map.add key value }

    let createMetric (name: string) (metricType: MetricType) (value: float) (tags: Map<string, string>) =
        { MetricName = name; MetricType = metricType; Value = value; Timestamp = Timestamp.now; Tags = tags; Unit = None }

    let withUnit (unit: string) (metric: MetricPoint) = { metric with Unit = Some unit }

    let createLatency (name: string) (comp: string) (start: Timestamp) (duration: TimeSpan) (isSuccess: bool) (errOpt: string option) (ctx: ExecutionContext) (metadata: Map<string, string>) =
        { OperationName = name; Component = comp; StartTime = start; EndTime = start + duration; DurationMs = duration.TotalMilliseconds; IsSuccess = isSuccess; ErrorDetails = errOpt; CorrelationId = ctx.CorrelationId; TenantId = ctx.TenantId; Metadata = metadata }

    let toLatencyMetric latency = Latency latency

    let createLimiterFrequency (limiterName: string) (comp: string) (current: float option) (limit: float option) (utilization: float option) (throttled: int64) (total: int64) (isActive: bool) (ctx: ExecutionContext) (metadata: Map<string, string>) =
        { LimiterName = limiterName; Component = comp; Timestamp = Timestamp.now; CurrentRate = current; ConfiguredLimitRate = limit; Utilization = utilization; ThrottledCount = throttled; TotalEvaluatedCount = total; IsActive = isActive; Metadata = metadata; CorrelationId = ctx.CorrelationId; TenantId = ctx.TenantId }

    let createError (comp: string) (code: string) (msg: string) (ctx: ExecutionContext) =
        { Component = comp; ErrorCode = code; ErrorMessage = msg; CorrelationId = ctx.CorrelationId; TenantId = ctx.TenantId }
