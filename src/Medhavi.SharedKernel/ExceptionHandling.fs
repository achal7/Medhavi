module Medhavi.SharedKernel.ExceptionHandling

open System
open System.Threading
open System.Threading.Tasks

/// Correlation ID for request tracing
type CorrelationId = private CorrelationId of Guid

module CorrelationId =
    let create () = CorrelationId(Guid.NewGuid())

    let value (CorrelationId id) = id
    let toString (CorrelationId id) = id.ToString()

    let fromString (id: string) =
        match Guid.TryParse(id) with
        | true, g -> Some(CorrelationId g)
        | false, _ -> None

    let unsafeFromString (id: string) = CorrelationId(Guid.Parse(id))

/// Structured error information
type ErrorInfo =
    { CorrelationId: CorrelationId
      Timestamp: DateTimeOffset
      ServiceName: string
      OperationName: string
      ErrorType: string
      ErrorMessage: string
      StackTrace: string option
      InnerException: string option
      ContextData: Map<string, obj> }

/// Error severity levels
type ErrorSeverity =
    | Critical
    | Error
    | Warning
    | Info

/// Error recovery strategies
type RecoveryStrategy =
    | Retry of maxRetries: int * delayMs: int
    | Fallback of fallbackValue: obj
    | CircuitBreak of serviceName: string
    | LogAndContinue
    | FailFast

/// Structured error response
type ErrorResponse =
    { CorrelationId: string
      Timestamp: DateTimeOffset
      ServiceName: string
      OperationName: string
      ErrorCode: string
      Message: string
      Details: Map<string, obj>
      Severity: ErrorSeverity }

/// Exception handling context
type ExceptionContext =
    { CorrelationId: CorrelationId
      ServiceName: string
      OperationName: string
      Logger: string -> unit
      RecoveryStrategy: RecoveryStrategy }

/// Create error info from exception
let createErrorInfo (ctx: ExceptionContext) (ex: Exception) (contextData: Map<string, obj>) =
    { CorrelationId = ctx.CorrelationId
      Timestamp = DateTimeOffset.UtcNow
      ServiceName = ctx.ServiceName
      OperationName = ctx.OperationName
      ErrorType = ex.GetType().Name
      ErrorMessage = ex.Message
      StackTrace = Some(ex.StackTrace)
      InnerException = ex.InnerException |> Option.ofObj |> Option.map(fun ie -> ie.Message)
      ContextData = contextData }

/// Create structured error response
let createErrorResponse (errorInfo: ErrorInfo) (errorCode: string) (severity: ErrorSeverity) =
    { CorrelationId = CorrelationId.toString errorInfo.CorrelationId
      Timestamp = errorInfo.Timestamp
      ServiceName = errorInfo.ServiceName
      OperationName = errorInfo.OperationName
      ErrorCode = errorCode
      Message = errorInfo.ErrorMessage
      Details = errorInfo.ContextData
      Severity = severity }

/// Log error with structured information
let logError (ctx: ExceptionContext) (errorInfo: ErrorInfo) (severity: ErrorSeverity) =
    let severityStr =
        match severity with
        | Critical -> "CRITICAL"
        | Error -> "ERROR"
        | Warning -> "WARNING"
        | Info -> "INFO"

    let message =
        $"[%s{severityStr}] Error in %s{errorInfo.ServiceName}.%s{errorInfo.OperationName} [CorrelationId: %s{CorrelationId.toString errorInfo.CorrelationId}]: %s{errorInfo.ErrorMessage}"

    ctx.Logger message

    // Log additional context if available
    if not errorInfo.ContextData.IsEmpty then
        let contextMessage = sprintf "Error context: %A" errorInfo.ContextData
        ctx.Logger contextMessage

/// Execute operation with comprehensive error handling
let executeWithErrorHandling (ctx: ExceptionContext) (operation: unit -> Task<'T>) (contextData: Map<string, obj>) =
    task {
        try
            let! result = operation()
            return Ok result
        with ex ->
            let errorInfo = createErrorInfo ctx ex contextData

            let severity =
                match ctx.RecoveryStrategy with
                | FailFast -> Critical
                | _ -> Error

            logError ctx errorInfo severity

            match ctx.RecoveryStrategy with
            | Retry _ ->
                // Implement retry logic here
                ctx.Logger $"Retry strategy not yet implemented for %s{ctx.OperationName}"
                return Result.Error ex
            | Fallback(fallbackValue) ->
                ctx.Logger $"Using fallback value for %s{ctx.OperationName}"
                return Ok(fallbackValue :?> 'T)
            | CircuitBreak(serviceName) ->
                ctx.Logger $"Circuit breaker triggered for service %s{serviceName}"
                return Result.Error ex
            | LogAndContinue ->
                ctx.Logger $"Continuing after error in %s{ctx.OperationName}"
                return Result.Error ex
            | FailFast ->
                ctx.Logger $"Failing fast due to critical error in %s{ctx.OperationName}"
                return Result.Error ex
    }

/// Execute async operation with timeout
let executeWithTimeout
    (ctx: ExceptionContext)
    (operation: CancellationToken -> Task<'T>)
    (timeoutMs: int)
    (contextData: Map<string, obj>)
    =
    task {
        use cts = new CancellationTokenSource(timeoutMs)

        try
            let! result = operation cts.Token
            return Ok result
        with
        | :? OperationCanceledException ->
            let errorInfo = createErrorInfo ctx (Exception($"Operation timed out after {timeoutMs}ms")) contextData

            logError ctx errorInfo Error
            return Result.Error(Exception($"Operation timed out after {timeoutMs}ms"))
        | ex ->
            let errorInfo = createErrorInfo ctx ex contextData
            logError ctx errorInfo Error
            return Result.Error ex
    }

/// Safe async execution with default error handling
let safeExecute (serviceName: string) (operationName: string) (logger: string -> unit) (operation: unit -> Task<'T>) =
    let ctx =
        { CorrelationId = CorrelationId.create()
          ServiceName = serviceName
          OperationName = operationName
          Logger = logger
          RecoveryStrategy = LogAndContinue }

    executeWithErrorHandling ctx operation Map.empty

/// Safe async execution with custom recovery strategy
let safeExecuteWithRecovery
    (serviceName: string)
    (operationName: string)
    (logger: string -> unit)
    (recoveryStrategy: RecoveryStrategy)
    (operation: unit -> Task<'T>)
    =
    let ctx =
        { CorrelationId = CorrelationId.create()
          ServiceName = serviceName
          OperationName = operationName
          Logger = logger
          RecoveryStrategy = recoveryStrategy }

    executeWithErrorHandling ctx operation Map.empty

/// Safe async execution with context data
let safeExecuteWithContext
    (serviceName: string)
    (operationName: string)
    (logger: string -> unit)
    (contextData: Map<string, obj>)
    (operation: unit -> Task<'T>)
    =
    let ctx =
        { CorrelationId = CorrelationId.create()
          ServiceName = serviceName
          OperationName = operationName
          Logger = logger
          RecoveryStrategy = LogAndContinue }

    executeWithErrorHandling ctx operation contextData

/// Create error response from exception
let createErrorResponseFromException
    (serviceName: string)
    (operationName: string)
    (correlationId: CorrelationId)
    (ex: Exception)
    (errorCode: string)
    (severity: ErrorSeverity)
    =
    let errorInfo =
        { CorrelationId = correlationId
          Timestamp = DateTimeOffset.UtcNow
          ServiceName = serviceName
          OperationName = operationName
          ErrorType = ex.GetType().Name
          ErrorMessage = ex.Message
          StackTrace = Some(ex.StackTrace)
          InnerException = ex.InnerException |> Option.ofObj |> Option.map(fun ie -> ie.Message)
          ContextData = Map.empty }

    createErrorResponse errorInfo errorCode severity

/// Extension methods for Task<Result<_,_>>
type TaskResultBuilder() =
    member _.Bind(taskResult: Task<Result<'T, 'E>>, binder: 'T -> Task<Result<'U, 'E>>) =
        task {
            let! result = taskResult

            match result with
            | Ok value -> return! binder value
            | Result.Error error -> return Result.Error error
        }

    member _.Return(value: 'T) = Task.FromResult(Ok value)
    member _.ReturnFrom(taskResult: Task<Result<'T, 'E>>) = taskResult

let taskResult = TaskResultBuilder()

/// Helper to convert exception to Result
let exceptionToResult (operation: unit -> 'T) =
    try
        Ok(operation())
    with ex -> Result.Error ex

/// Helper to convert async exception to Task<Result>
let asyncExceptionToResult (operation: unit -> Task<'T>) =
    task {
        try
            let! result = operation()
            return Ok result
        with ex ->
            return Result.Error ex
    }
