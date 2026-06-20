namespace Medhavi.SharedKernel

open System
open System.Threading.Tasks
open System.Text.Json.Serialization
open Medhavi.SharedKernel.Logging

[<JsonFSharpConverter>]
type InfrastructureError =
    | Network of string
    | Timeout of string
    | EventStore of string
    | Database of string
    | Http of string
    | CircuitOpen of string
    | OtherInfra of string

type ErrorContext =
    { Code: string
      Message: string
      Data: Map<string, obj>
      InnerError: ErrorContext option
      Timestamp: DateTimeOffset
      CorrelationId: Guid option }

module ErrorCodes =
    // Infrastructure errors
    let TaskCanceled = "INFRA_TASK_CANCELED"
    let OperationCanceled = "INFRA_OPERATION_CANCELED"
    let Timeout = "INFRA_TIMEOUT"
    let HttpError = "INFRA_HTTP_ERROR"
    let NetworkError = "INFRA_NETWORK_ERROR"
    let EventStoreError = "INFRA_EVENTSTORE_ERROR"
    let CircuitOpen = "INFRA_CIRCUIT_OPEN"
    let OtherInfra = "INFRA_OTHER"

    // Domain errors
    let ValidationFailed = "DOMAIN_VALIDATION_FAILED"
    let BusinessRuleViolation = "DOMAIN_BUSINESS_RULE_VIOLATION"
    let InvariantViolation = "DOMAIN_INVARIANT_VIOLATION"
    let NotFound = "DOMAIN_NOT_FOUND"
    let Conflict = "DOMAIN_CONFLICT"
    let VersionMismatch = "DOMAIN_VERSION_MISMATCH"

    // External errors
    let ExternalServiceUnavailable = "EXT_SERVICE_UNAVAILABLE"
    let ExternalTimeout = "EXT_TIMEOUT"
    let ExternalAuthFailed = "EXT_AUTH_FAILED"

    // Others
    let Mismatch = "MISMATCH"
    let Unknown = "UNKNOWN_ERROR"

[<JsonFSharpConverter>]
type DomainError =
    | ValidationError of code: string * message: string * data: Map<string, obj>
    | DomainError of code: string * message: string * data: Map<string, obj>

    /// Human-readable error message
    member this.Message =
        match this with
        | ValidationError(_, msg, _)
        | DomainError(_, msg, _) -> msg

    /// Create domain error
    static member domain code message = DomainError(code, message, Map.empty)

    /// Create domain error with data
    static member domainWith code message data = DomainError(code, message, data)

    /// Create an invariant violation error
    static member invariant message = DomainError(ErrorCodes.InvariantViolation, message, Map.empty)

    /// Create a validation error
    static member validation message = ValidationError(ErrorCodes.ValidationFailed, message, Map.empty)

    /// Create a validation error with contextual data
    static member validationWith message (data: Map<string, obj>) =
        ValidationError(ErrorCodes.ValidationFailed, message, data)

    static member conflict message = DomainError(ErrorCodes.Conflict, message, Map.empty)
    static member notFound message = DomainError(ErrorCodes.NotFound, message, Map.empty)

    /// Combines multiple validation DomainError instances into a single consolidated DomainError.
    static member combineValidationErrors(errors: DomainError list) : DomainError =
        match errors with
        | [] -> DomainError.validation "Validation failed with no specified details"
        | [ single ] -> single
        | _ ->
            let messages = errors |> List.map(fun e -> e.Message)
            let combinedMessage = "Command validation failed: " + String.concat "; " messages
            let data = errors |> List.mapi(fun idx e -> $"error_{idx}", box e.Message) |> Map.ofList

            DomainError.validationWith combinedMessage data

[<JsonFSharpConverter>]
type InfraError =
    | Issue of etype: InfrastructureError * code: string * message: string * data: Map<string, obj>

    /// Create infrastructure error
    static member infrastructure etype code message = Issue(etype, code, message, Map.empty)

    /// Create infrastructure error with data
    static member infrastructureWith etype code message data = Issue(etype, code, message, data)

    /// Create external error
    static member data code message = Issue(InfrastructureError.Database code, code, message, Map.empty)

    /// Create external error with data
    static member dataWith code message data = Issue(InfrastructureError.Database code, code, message, data)

[<JsonFSharpConverter>]
type ApplicationError =
    | Domain of DomainError
    | NotFound of code: string * message: string * data: Map<string, obj>
    | Mismatch of code: string * expected: Version * actual: Version
    | Infrastructure of InfraError
    | External of code: string * message: string * data: Map<string, obj>
    | Unknown of string

    member this.Message =
        match this with
        | NotFound(_, msg, _)
        | External(_, msg, _)
        | Infrastructure(Issue(_, _, msg, _))
        | Domain(DomainError(_, msg, _))
        | Domain(ValidationError(_, msg, _)) -> msg
        | Mismatch(_, expected, actual) -> $"Version mismatch: expected {expected}, actual {actual}"
        | Unknown(msg) -> $"Unknown error: {msg}"

    /// Machine-readable error code
    member this.Code =
        match this with
        | NotFound(code, _, _)
        | External(code, _, _)
        | Infrastructure(InfraError.Issue(_, code, _, _))
        | Domain(DomainError(code, _, _))
        | Domain(ValidationError(code, _, _))
        | Mismatch(code, _, _) -> code
        | Unknown _ -> ""

    /// Optional contextual data for debugging and logging
    member this.Data =
        match this with
        | NotFound(_, _, data)
        | External(_, _, data)
        | Infrastructure(Issue(_, _, _, data))
        | Domain(DomainError(_, _, data))
        | Domain(ValidationError(_, _, data)) -> data
        | Mismatch _
        | Unknown _ -> Map.empty

    /// Convert to error context with detailed information
    member this.ToContext(?correlationId: Guid) : ErrorContext =
        let code, message, data =
            match this with
            | Domain(DomainError(code, _, data))
            | Domain(ValidationError(code, _, data))
            | External(code, _, data)
            | NotFound(code, _, data) -> (code, this.Message, data)
            | Infrastructure(Issue(_, code, _, data)) -> (code, this.Message, data)
            | Mismatch(code, _, _) -> (code, this.Message, Map.empty)
            | Unknown _ -> ("", this.Message, Map.empty)

        { Code = code
          Message = message
          Data = data
          InnerError = None
          Timestamp = DateTimeOffset.UtcNow
          CorrelationId = correlationId }

    /// Create a not found error
    static member notFound message = NotFound(ErrorCodes.NotFound, message, Map.empty)

    static member referenceNotFound (name: string) (idStr: string) =
        NotFound(ErrorCodes.NotFound, sprintf "%s %s not found" name idStr, Map.empty)

    /// Create a not found error with contextual data
    static member notFoundWith message (data: Map<string, obj>) = NotFound(ErrorCodes.NotFound, message, data)

module ApplicationError =

    let mapToApiError (error: ApplicationError) : Medhavi.Contracts.ApiError =
        match error with
        | NotFound(code, msg, _) -> { Code = code; Category = "NotFound"; Message = msg }
        | Mismatch(code, expected, actual) -> { Code = code; Category = "Mismatch"; Message = $"Expected {expected} Actual {actual}" }
        | Infrastructure(Issue(_, code, msg, _)) -> { Code = code; Category = "Infrastructure"; Message = msg }
        | Domain(DomainError(code, msg, _)) -> { Code = code; Category = "Domain"; Message = msg }
        | Domain(ValidationError(code, msg, _)) -> { Code = code; Category = "Validation"; Message = msg }
        | External(code, msg, _) -> { Code = code; Category = "External"; Message = msg }
        | Unknown(msg) -> { Code = ""; Category = "Unknown"; Message = msg }

    let mapDomainError =
        function
        | DomainError(code, msg, data) -> ApplicationError.Domain(DomainError(code, msg, data))
        | ValidationError(code, msg, data) -> ApplicationError.Domain(DomainError(code, msg, data))

    let mapInfraError (infra: InfraError) = ApplicationError.Infrastructure(infra)
    let liftInfraError result = result |> Result.mapError(fun e -> [ mapInfraError e ])
    let liftDomainErrors result = result |> Result.mapError(List.map mapDomainError)

    let rec fromException (ex: exn) : ApplicationError =
        match ex with
        | :? TaskCanceledException as tce ->
            ApplicationError.Infrastructure(
                Issue((Timeout tce.Source), ErrorCodes.TaskCanceled, tce.Message, Map.empty)
            )
        | :? OperationCanceledException as oce ->
            ApplicationError.Infrastructure
            <| Issue((InfrastructureError.Timeout oce.Message), ErrorCodes.OperationCanceled, oce.Message, Map.empty)
        | :? TimeoutException as t ->
            ApplicationError.Infrastructure
            <| Issue((InfrastructureError.Timeout t.Message), ErrorCodes.Timeout, t.Message, Map.empty)
        | :? AggregateException as ae ->
            // flatten and return the first inner mapped
            ae.Flatten().InnerExceptions
            |> Seq.tryHead
            |> Option.map fromException
            |> Option.defaultValue(ApplicationError.Unknown ae.Message)
        | :? Net.Http.HttpRequestException as h ->
            ApplicationError.Infrastructure
            <| Issue((InfrastructureError.Http h.Message), ErrorCodes.HttpError, h.Message, Map.empty)
        | _ -> ApplicationError.Unknown ex.Message

    let logIfError (logger: Logger) (message: string) (result: Result<'T, ApplicationError>) =
        result
        |> Result.mapError(fun e ->
            logger.Error(message)
            Error e)

    let logIfErrorAsync (logger: Logger) (message: string) (taskRes: Task<Result<'T, ApplicationError>>) =
        task {
            let! r = taskRes
            return logIfError logger message r
        }

    let protect (fn: unit -> 'T) : Result<'T, ApplicationError> =
        try
            Ok(fn())
        with ex ->
            Error(fromException ex)

    let protectAsync (fn: unit -> Task<'T>) : Task<Result<'T, ApplicationError>> =
        try
            let t = fn()

            t.ContinueWith(fun (antecedent: Task<'T>) ->
                if antecedent.IsFaulted then
                    Error(fromException antecedent.Exception.InnerException)
                elif antecedent.IsCanceled then
                    Error(
                        ApplicationError.Infrastructure
                        <| Issue(
                            (InfrastructureError.Timeout "Task was canceled"),
                            ErrorCodes.TaskCanceled,
                            "Task was canceled",
                            Map.empty
                        )
                    )
                else
                    Ok antecedent.Result)
        with ex ->
            Task.FromResult(Error(fromException ex))

    let toResult (f: unit -> 'T) : Result<'T, ApplicationError> =
        try
            Ok(f())
        with ex ->
            Error(fromException ex)

    let tryCatchAsync (f: unit -> Task<'T>) : Task<Result<'T, ApplicationError>> =
        task {
            try
                let! v = f()
                return Ok v
            with ex ->
                return Error(fromException ex)
        }

    /// Pattern match on not found errors
    let (|NotFound|_|) =
        function
        | NotFound(_, msg, _) -> Some msg
        | _ -> None

    /// Pattern match on conflict errors
    let (|Conflict|_|) =
        function
        | Domain(DomainError(_, msg, _)) -> Some msg
        | _ -> None

    /// Pattern match on invariant errors
    let (|Invariant|_|) =
        function
        | Domain(DomainError(_, msg, _)) -> Some msg
        | _ -> None

    /// Pattern match on external errors
    let (|External|_|) =
        function
        | External(_, msg, _) -> Some msg
        | _ -> None

    /// Pattern match on business rule errors
    let (|BusinessRule|_|) =
        function
        | Domain(DomainError(_, msg, _)) -> Some msg
        | _ -> None
