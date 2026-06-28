namespace Medhavi.SharedKernel

open System
open System.Diagnostics
open System.Text.Json.Serialization
open System.Threading.Tasks
open Microsoft.Extensions.Logging
open Medhavi.SharedKernel.ExceptionHandling
open Medhavi.SharedKernel.Logging

/// Telemetry event severity
type TelemetrySeverity =
    | Trace
    | Debug
    | Information
    | Warning
    | Errors
    | Critical

/// Telemetry event (structured)
type TelemetryEvent =
    { EventId: Guid
      Timestamp: DateTimeOffset
      Severity: TelemetrySeverity
      Message: string
      Properties: Map<string, obj>
      CorrelationId: CorrelationId option
      CausationId: Guid option
      TraceId: string option
      SpanId: string option }

/// Metric type
type MetricType =
    | Counter
    | Gauge
    | Histogram
    | Summary

/// Metric data point
type MetricPoint =
    { MetricName: string
      MetricType: MetricType
      Value: float
      Timestamp: Timestamp
      Tags: Map<string, string>
      Unit: string option }

/// Performance measurement
type PerformanceMeasurement =
    { OperationName: string
      Duration: TimeSpan
      Success: bool
      Timestamp: Timestamp
      Properties: Map<string, obj> }

type LogTelemetryEvent = TelemetryEvent -> unit

/// Identifies the planning bottleneck/limiter causing order promising delays
[<JsonFSharpConverter>]
type LimiterType =
    | MaterialShortage of sku: string * nodeId: string
    | CapacityConstraint of resourceId: string
    | TransportDelay of legId: string
    | Other of reason: string

/// Records latency telemetry for cross-aggregate operations
type LatencyTelemetry =
    { OperationName: string
      Component: string
      StartTime: Timestamp
      EndTime: Timestamp
      DurationMs: float
      IsSuccess: bool
      ErrorDetails: string option
      CorrelationId: CorrelationId
      TenantId: string option
      Metadata: Map<string, string> }

/// Tracks bottleneck frequencies, rate limit metrics, and utilization
type LimiterFrequencyTelemetry =
    { LimiterName: string
      Component: string
      Timestamp: Timestamp
      CurrentRate: float option
      ConfiguredLimitRate: float option
      Utilization: float option // 0.0 to 1.0
      ThrottledCount: int64
      TotalEvaluatedCount: int64
      IsActive: bool
      Metadata: Map<string, string>
      CorrelationId: CorrelationId
      TenantId: string option }

/// Metric reporting error details and system faults
type TelemetryErrorMetric =
    { Component: string
      ErrorCode: string
      ErrorMessage: string
      CorrelationId: CorrelationId
      TenantId: string option }

/// Aggregated Telemetry Metric Envelope
[<JsonFSharpConverter>]
type TelemetryMetric =
    | Latency of LatencyTelemetry
    | LimiterFrequency of LimiterFrequencyTelemetry
    | ErrorEvent of TelemetryErrorMetric

module Telemetry =

    /// Create telemetry event
    let createEvent (severity: TelemetrySeverity) (message: string) (properties: Map<string, obj>) : TelemetryEvent =
        { EventId = Guid.NewGuid()
          Timestamp = DateTimeOffset.UtcNow
          Severity = severity
          Message = message
          Properties = properties
          CorrelationId = None
          CausationId = None
          TraceId = None
          SpanId = None }

    /// Add correlation context
    let withCorrelation (correlationId: CorrelationId) (causationId: Guid option) (event: TelemetryEvent) : TelemetryEvent =
        { event with
            CorrelationId = Some correlationId
            CausationId = causationId }

    /// Add distributed tracing context
    let withTracing (traceId: string) (spanId: string) (event: TelemetryEvent) : TelemetryEvent =
        { event with
            TraceId = Some traceId
            SpanId = Some spanId }

    /// Add property to event
    let addProperty (key: string) (value: obj) (event: TelemetryEvent) : TelemetryEvent =
        { event with
            Properties = event.Properties.Add(key, value) }

    /// Create metric point
    let createMetric (name: string) (metricType: MetricType) (value: float) (tags: Map<string, string>) : MetricPoint =
        { MetricName = name
          MetricType = metricType
          Value = value
          Timestamp = Timestamp.now
          Tags = tags
          Unit = None }

    /// Add unit to metric
    let withUnit (unit: string) (metric: MetricPoint) : MetricPoint = { metric with Unit = Some unit }

    let reportErrorToTelemetry (ctx: ExceptionContext) (error: ApplicationError) =
        let telemetryError =
            { Component = ctx.ServiceName
              ErrorCode = error.Code
              ErrorMessage = error.Message
              CorrelationId = ctx.CorrelationId
              TenantId = None // populated from ExecutionContext if available
            }
        // Emit as a telemetry metric
        let metric = TelemetryMetric.ErrorEvent telemetryError
        DomainEventBus.Publish(metric)

    /// Convert severity to LogLevel
    let toLogLevel (severity: TelemetrySeverity) : LogLevel =
        match severity with
        | TelemetrySeverity.Trace -> LogLevel.Trace
        | TelemetrySeverity.Debug -> LogLevel.Debug
        | TelemetrySeverity.Information -> LogLevel.Information
        | TelemetrySeverity.Warning -> LogLevel.Warning
        | TelemetrySeverity.Errors -> LogLevel.Error
        | TelemetrySeverity.Critical -> LogLevel.Critical

    let logEvent (logger: ILogger) (event: TelemetryEvent) : unit =
        let logLevel = toLogLevel event.Severity

        // Build structured log entry
        let state =
            event.Properties
            |> Map.toSeq
            |> Seq.append
                [ ("EventId", box event.EventId)
                  ("Timestamp", box event.Timestamp)
                  if event.CorrelationId.IsSome then
                      ("CorrelationId", box event.CorrelationId.Value)
                  if event.CausationId.IsSome then
                      ("CausationId", box event.CausationId.Value)
                  if event.TraceId.IsSome then
                      ("TraceId", box event.TraceId.Value)
                  if event.SpanId.IsSome then
                      ("SpanId", box event.SpanId.Value) ]
            |> dict

        logger.Log(logLevel, event.EventId |> string, state, null, (fun _ _ -> event.Message))

    /// Log with correlation
    let logWithCorrelation
        (logger: ILogger)
        (correlationId: CorrelationId)
        (severity: TelemetrySeverity)
        (message: string)
        (properties: Map<string, obj>)
        : unit =

        createEvent severity message properties |> withCorrelation correlationId None |> logEvent logger

    /// Log operation result as telemetry event
    let logResult
        (logger: Logger)
        (operationName: string, result: Result<'T, 'E>, toErrorMessage: 'E -> string, context: LogContext option)
        =
        let ctx = logger.getContext context
        let correlationId = ctx.CorrelationId

        let event =
            match result with
            | Ok _ ->
                createEvent
                    TelemetrySeverity.Information
                    $"Operation '{operationName}' succeeded"
                    (Map.ofList [ ("Operation", box operationName); ("Success", box true) ])
            | Result.Error err ->
                createEvent
                    TelemetrySeverity.Errors
                    $"Operation '{operationName}' failed: {toErrorMessage err}"
                    (Map.ofList
                        [ ("Operation", box operationName)
                          ("Success", box false)
                          ("Error", box(toErrorMessage err)) ])

        let eventWithCorrelation =
            match correlationId with
            | Some cid -> withCorrelation cid None event
            | None -> event

        logEvent logger.InnerLogger eventWithCorrelation

    /// Create a LatencyTelemetry utilizing ExecutionContext parameters
    let createLatency
        (name: string)
        (comp: string)
        (start: Timestamp)
        (duration: TimeSpan)
        (isSuccess: bool)
        (errOpt: string option)
        (ctx: ExecutionContext)
        (metadata: Map<string, string>)
        : LatencyTelemetry =
        { OperationName = name
          Component = comp
          StartTime = start
          EndTime = start + duration
          DurationMs = duration.TotalMilliseconds
          IsSuccess = isSuccess
          ErrorDetails = errOpt
          CorrelationId = ctx.CorrelationId
          TenantId = ctx.TenantId
          Metadata = metadata }

    let toLatencyMetric (latency: LatencyTelemetry) : TelemetryMetric = Latency latency

    /// Create a LimiterFrequencyTelemetry utilizing ExecutionContext parameters
    let createLimiterFrequency
        (limiterName: string)
        (comp: string)
        (current: float option)
        (limit: float option)
        (utilization: float option)
        (throttled: int64)
        (total: int64)
        (isActive: bool)
        (ctx: ExecutionContext)
        (metadata: Map<string, string>)
        : LimiterFrequencyTelemetry =
        { LimiterName = limiterName
          Component = comp
          Timestamp = Timestamp.now
          CurrentRate = current
          ConfiguredLimitRate = limit
          Utilization = utilization
          ThrottledCount = throttled
          TotalEvaluatedCount = total
          IsActive = isActive
          Metadata = metadata
          CorrelationId = ctx.CorrelationId
          TenantId = ctx.TenantId }

    /// Create a TelemetryErrorMetric utilizing ExecutionContext parameters
    let createError (comp: string) (code: string) (msg: string) (ctx: ExecutionContext) : TelemetryErrorMetric =
        { Component = comp
          ErrorCode = code
          ErrorMessage = msg
          CorrelationId = ctx.CorrelationId
          TenantId = ctx.TenantId }

module Performance =

    /// Measure operation performance
    let measure (operationName: string) (operation: unit -> 'T) : 'T * PerformanceMeasurement =
        let sw = Stopwatch.StartNew()
        let mutable success = false

        try
            let result = operation()
            success <- true
            sw.Stop()

            let measurement =
                { OperationName = operationName
                  Duration = sw.Elapsed
                  Success = true
                  Timestamp = Timestamp.now
                  Properties = Map.empty }

            (result, measurement)
        with ex ->
            sw.Stop()

            let measurement =
                { OperationName = operationName
                  Duration = sw.Elapsed
                  Success = false
                  Timestamp = Timestamp.now
                  Properties = Map.ofList [ ("Error", box ex.Message) ] }

            reraise()

    /// Measure async operation
    let measureAsync (operationName: string) (operation: Task<'T>) : Task<Result<'T * PerformanceMeasurement, ApplicationError>> =
        task {
            let sw = Stopwatch.StartNew()

            try
                let! result = operation
                sw.Stop()

                let measurement =
                    { OperationName = operationName
                      Duration = sw.Elapsed
                      Success = true
                      Timestamp = Timestamp.now
                      Properties = Map.empty }

                return Ok(result, measurement)
            with ex ->
                sw.Stop()

                let measurement =
                    { OperationName = operationName
                      Duration = sw.Elapsed
                      Success = false
                      Timestamp = Timestamp.now
                      Properties = Map.ofList [ ("Error", box ex.Message) ] }

                return Result.Error (ApplicationError.fromException ex)
        }

    let convertToTelemetryEvent (measurement: PerformanceMeasurement) : TelemetryEvent =
        let severity =
            if not measurement.Success then Warning
            elif measurement.Duration.TotalSeconds > 5.0 then Warning
            elif measurement.Duration.TotalSeconds > 1.0 then Information
            else Debug

        let properties =
            measurement.Properties
            |> Map.add "Operation" (box measurement.OperationName)
            |> Map.add "Duration" (box measurement.Duration.TotalMilliseconds)
            |> Map.add "DurationMs" (box(int measurement.Duration.TotalMilliseconds))
            |> Map.add "Success" (box measurement.Success)

        let message =
            if measurement.Success then
                $"Operation '{measurement.OperationName}' completed in {measurement.Duration.TotalMilliseconds:F2}ms"
            else
                $"Operation '{measurement.OperationName}' failed after {measurement.Duration.TotalMilliseconds:F2}ms"

        Telemetry.createEvent severity message properties

/// Metrics collection
module Metrics =

    /// Record counter increment
    let recordCounter (name: string) (value: float) (tags: Map<string, string>) : MetricPoint =
        Telemetry.createMetric name MetricType.Counter value tags

    /// Record gauge value
    let recordGauge (name: string) (value: float) (tags: Map<string, string>) : MetricPoint =
        Telemetry.createMetric name MetricType.Gauge value tags

    /// Record histogram value
    let recordHistogram (name: string) (value: float) (tags: Map<string, string>) : MetricPoint =
        Telemetry.createMetric name MetricType.Histogram value tags

    let convertToTelemetryEvent (metric: MetricPoint) : TelemetryEvent =
        let properties =
            metric.Tags
            |> Map.toSeq
            |> Seq.map(fun (k, v) -> (k, box v))
            |> Map.ofSeq
            |> Map.add "MetricName" (box metric.MetricName)
            |> Map.add "MetricType" (box(metric.MetricType.ToString()))
            |> Map.add "Value" (box metric.Value)
            |> fun m ->
                match metric.Unit with
                | Some unit -> m |> Map.add "Unit" (box unit)
                | None -> m

        Telemetry.createEvent Debug $"Metric: {metric.MetricName} = {metric.Value}" properties

/// Health check support
module HealthCheck =

    /// Health status
    type HealthStatus =
        | Healthy
        | Degraded of reason: string
        | Unhealthy of reason: string

    /// Component health
    type ComponentHealth =
        { ComponentName: string
          Status: HealthStatus
          LastChecked: DateTimeOffset
          ResponseTime: TimeSpan option
          Details: Map<string, obj> }

    type HealthCheck = unit -> System.Threading.Tasks.Task<ComponentHealth>

    /// Create health check result
    let createHealth (componentName: string) (status: HealthStatus) : ComponentHealth =
        { ComponentName = componentName
          Status = status
          LastChecked = DateTimeOffset.UtcNow
          ResponseTime = None
          Details = Map.empty }

    /// Add response time
    let withResponseTime (duration: TimeSpan) (health: ComponentHealth) : ComponentHealth =
        { health with
            ResponseTime = Some duration }

    /// Add detail
    let addDetail (key: string) (value: obj) (health: ComponentHealth) : ComponentHealth =
        { health with
            Details = health.Details.Add(key, value) }

    /// Log health check
    let convertToTelemetryEvent (health: ComponentHealth) : TelemetryEvent =
        let severity =
            match health.Status with
            | Healthy -> Information
            | Degraded _ -> Warning
            | Unhealthy _ -> Errors

        let statusText =
            match health.Status with
            | Healthy -> "Healthy"
            | Degraded reason -> $"Degraded: {reason}"
            | Unhealthy reason -> $"Unhealthy: {reason}"

        let properties =
            health.Details
            |> Map.add "Component" (box health.ComponentName)
            |> Map.add "Status" (box statusText)
            |> Map.add "LastChecked" (box health.LastChecked)
            |> fun m ->
                match health.ResponseTime with
                | Some rt -> m |> Map.add "ResponseTimeMs" (box rt.TotalMilliseconds)
                | None -> m

        Telemetry.createEvent severity $"Health check: {health.ComponentName} - {statusText}" properties

/// Activity/Span tracking for distributed tracing
module ActivityTracking =

    /// Start activity (span)
    let startActivity (activityName: string) (tags: (string * string) list) : Activity =
        let activity = new Activity(activityName)

        for key, value in tags do
            activity.SetTag(key, value) |> ignore

        activity.Start()

    /// Stop activity and log
    let stopActivity (activity: Activity) : TelemetryEvent =
        activity.Stop()

        let properties =
            activity.Tags
            |> Seq.map(fun tag -> (tag.Key, box tag.Value))
            |> Map.ofSeq
            |> Map.add "ActivityName" (box activity.OperationName)
            |> Map.add "Duration" (box activity.Duration.TotalMilliseconds)
            |> Map.add "TraceId" (box activity.TraceId)
            |> Map.add "SpanId" (box activity.SpanId)

        Telemetry.createEvent Debug $"Activity completed: {activity.OperationName}" properties
        |> Telemetry.withTracing (activity.TraceId.ToString()) (activity.SpanId.ToString())

    /// Execute with activity tracking
    let withActivity
        (logger: LogTelemetryEvent)
        (activityName: string)
        (tags: (string * string) list)
        (operation: unit -> 'T)
        : 'T =

        let activity = startActivity activityName tags

        try
            let result = operation()
            stopActivity activity |> logger
            result
        with ex ->
            activity.SetTag("error", "true") |> ignore

            activity.SetTag("error.message", ex.Message) |> ignore

            stopActivity activity |> logger
            reraise()

    // For async operations
    let withActivityAsync
        (logger: LogTelemetryEvent)
        (activityName: string)
        (tags: (string * string) list)
        (operation: unit -> Task<'T>)
        : Task<'T> =
        task {
            let activity = startActivity activityName tags

            try
                let! result = operation()
                stopActivity activity |> logger
                return result
            with ex ->
                activity.SetTag("error", "true") |> ignore
                activity.SetTag("error.message", ex.Message) |> ignore
                stopActivity activity |> logger
                return raise ex
        }
