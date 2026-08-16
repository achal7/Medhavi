namespace Medhavi.Foundation.Failure

open System
open System.Threading.Tasks
open System.Text.Json.Serialization

[<JsonFSharpConverter>]
type InfrastructureError =
    | Network of message: string
    | Timeout of message: string
    | Database of message: string
    | EventStore of message: string
    | Serialization of message: string
    | CircuitOpen of message: string
    | ExternalService of serviceName: string * message: string

    member this.Message =
        match this with
        | Network msg
        | Timeout msg
        | Database msg
        | EventStore msg
        | Serialization msg
        | CircuitOpen msg -> msg
        | ExternalService(svc, msg) -> $"[{svc}] {msg}"

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

[<JsonFSharpConverter>]
type DomainError =
    | ValidationFailed of code: string * errors: (string * string) list
    | BusinessRuleViolated of code: string * ruleId: string * message: string
    | InvariantViolated of code: string * message: string
    | EntityNotFound of code: string * entityType: string * entityId: string
    | Conflict of code: string * message: string

    member this.Message =
        match this with
        | ValidationFailed(_, errors) -> errors |> List.map(fun (f, m) -> $"{f}: {m}") |> String.concat "; "
        | BusinessRuleViolated(_, ruleId, msg) -> $"[{ruleId}] {msg}"
        | InvariantViolated(_, msg) -> msg
        | EntityNotFound(_, entityType, entityId) -> $"{entityType} {entityId} not found"
        | Conflict(_, msg) -> msg

    member this.Code =
        match this with
        | ValidationFailed(code, _)
        | BusinessRuleViolated(code, _, _)
        | InvariantViolated(code, _)
        | EntityNotFound(code, _, _)
        | Conflict(code, _) -> code

    static member validation(message: string, ?rule: string) =
        let code = defaultArg rule ErrorCodes.ValidationFailed
        ValidationFailed(code, [ ("", message) ])

    static member rule(message: string, ?rule: string, ?code: string) =
        let code = defaultArg code ErrorCodes.BusinessRuleViolation
        let ruleId = defaultArg rule ""
        BusinessRuleViolated(code, ruleId, message)

    static member invariant(message: string, ?code: string) =
        let code = defaultArg code ErrorCodes.InvariantViolation
        InvariantViolated(code, message)

    static member notFound(entityType: string, entityId: string, ?code: string) =
        let code = defaultArg code ErrorCodes.NotFound
        EntityNotFound(code, entityType, entityId)

    static member conflict(message: string, ?code: string) =
        let code = defaultArg code ErrorCodes.Conflict
        Conflict(code, message)

    static member combineValidationErrors(errors: DomainError list) : DomainError =
        match errors with
        | [] -> DomainError.validation("Validation failed with no specified details")
        | [ single ] -> single
        | _ ->
            let allErrors =
                errors
                |> List.collect(fun e ->
                    match e with
                    | ValidationFailed(_, errs) -> errs
                    | _ -> [ "", e.Message ])

            ValidationFailed(ErrorCodes.ValidationFailed, allErrors)

[<JsonFSharpConverter>]
type ApplicationError =
    | Domain of DomainError
    | Validation of errors: (string * string) list
    | Infrastructure of InfrastructureError

    member this.Message =
        match this with
        | Domain e -> e.Message
        | Validation errors -> errors |> List.map(fun (f, m) -> $"{f}: {m}") |> String.concat "; "
        | Infrastructure e -> e.Message

    member this.Code =
        match this with
        | Domain e -> e.Code
        | Validation _ -> ErrorCodes.ValidationFailed
        | Infrastructure e ->
            match e with
            | Network _ -> ErrorCodes.NetworkError
            | Timeout _ -> ErrorCodes.Timeout
            | Database _ -> ErrorCodes.EventStoreError
            | EventStore _ -> ErrorCodes.EventStoreError
            | Serialization _ -> ErrorCodes.OtherInfra
            | CircuitOpen _ -> ErrorCodes.CircuitOpen
            | ExternalService _ -> ErrorCodes.ExternalServiceUnavailable

    static member validation errors = Validation errors

    static member infraTimeout msg = Infrastructure(InfrastructureError.Timeout msg)

    static member repository(err: exn) = Infrastructure(InfrastructureError.Database err.Message)

module ApplicationError =

    let fromDomainError (err: DomainError) : ApplicationError = Domain err

    let fromInfrastructureError (err: InfrastructureError) : ApplicationError = Infrastructure err

    let rec fromExceptionToInfra (ex: exn) : InfrastructureError =
        match ex with
        | :? TaskCanceledException as tce -> Timeout tce.Message
        | :? OperationCanceledException as oce -> Timeout oce.Message
        | :? TimeoutException as t -> Timeout t.Message
        | :? AggregateException as ae ->
            ae.Flatten().InnerExceptions
            |> Seq.tryHead
            |> Option.map fromExceptionToInfra
            |> Option.defaultValue(Database ae.Message)
        | :? Net.Http.HttpRequestException as h -> Network h.Message
        | _ -> Database ex.Message

    let fromException (ex: exn) : ApplicationError = Infrastructure(fromExceptionToInfra ex)

    let protect (fn: unit -> 'T) : Result<'T, ApplicationError> =
        try
            Ok(fn())
        with ex ->
            Error(fromException ex)

    let toResult (fn: unit -> 'T) : Result<'T, ApplicationError> =
        try
            Ok(fn())
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
