namespace Medhavi.SharedKernel

open System
open System.Threading.Tasks
open System.Text.Json.Serialization
open Medhavi.Common.Validator
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
        | Unknown(_) -> ""

    /// Optional contextual data for debugging and logging
    member this.Data =
        match this with
        | NotFound(_, _, data)
        | External(_, _, data)
        | Infrastructure(Issue(_, _, _, data))
        | Domain(DomainError(_, _, data))
        | Domain(ValidationError(_, _, data)) -> data
        | Mismatch(_, _, _)
        | Unknown(_) -> Map.empty

    /// Convert to error context with detailed information
    member this.ToContext(?correlationId: Guid) : ErrorContext =
        let (code, message, data) =
            match this with
            | Domain(DomainError(code, msg, data))
            | Domain(ValidationError(code, msg, data))
            | External(code, msg, data)
            | NotFound(code, msg, data) -> (code, this.Message, data)
            | Infrastructure(Issue(_, code, msg, data)) -> (code, this.Message, data)
            | Mismatch(code, _, _) -> (code, this.Message, Map.empty)
            | Unknown(msg) -> ("", this.Message, Map.empty)

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

    let mapDomainError =
        function
        | DomainError(code, msg, data) -> ApplicationError.Domain(DomainError(code, msg, data))
        | ValidationError(code, msg, data) -> ApplicationError.Domain(DomainError(code, msg, data))

    let mapInfraError (infra: InfraError) = ApplicationError.Infrastructure(infra)

    let liftInfraError result =
        result
        |> Result.mapError (fun e -> [ mapInfraError e ])

    let liftDomainErrors result =
        result
        |> Result.mapError (List.map mapDomainError)

    let rec fromException (ex: exn) : ApplicationError =
        match ex with
        | :? System.Threading.Tasks.TaskCanceledException as tce ->
            ApplicationError.Infrastructure(
                Issue((Timeout tce.Source), ErrorCodes.TaskCanceled, tce.Message, Map.empty)
            )
        | :? System.OperationCanceledException as oce ->
            ApplicationError.Infrastructure
            <| Issue((InfrastructureError.Timeout oce.Message), ErrorCodes.OperationCanceled, oce.Message, Map.empty)
        | :? System.TimeoutException as t ->
            ApplicationError.Infrastructure
            <| Issue((InfrastructureError.Timeout t.Message), ErrorCodes.Timeout, t.Message, Map.empty)
        | :? System.AggregateException as ae ->
            // flatten and return the first inner mapped
            ae.Flatten().InnerExceptions
            |> Seq.tryHead
            |> Option.map fromException
            |> Option.defaultValue (ApplicationError.Unknown ae.Message)
        | :? System.Net.Http.HttpRequestException as h ->
            ApplicationError.Infrastructure
            <| Issue((InfrastructureError.Http h.Message), ErrorCodes.HttpError, h.Message, Map.empty)
        | _ -> ApplicationError.Unknown ex.Message

    let logIfError (logger: Logger) (message: string) (result: Result<'T, ApplicationError>) =
        match result with
        | Ok v -> Ok v
        | Error e ->
            logger.Error(message)
            Error e

    let logIfErrorAsync
        (logger: Logger)
        (message: string)
        (taskRes: Task<Result<'T, ApplicationError>>)
        (context: LogContext option)
        =
        let ctx = logger.getContext context

        task {
            let! r = taskRes

            match r with
            | Ok v -> return Ok v
            | Error e ->
                logger.Error(message)
                return Error e
        }

    let protect (fn: unit -> 'T) : Result<'T, ApplicationError> =
        try
            Ok(fn ())
        with ex ->
            Error(fromException ex)

    let protectAsync (fn: unit -> Task<'T>) : Task<Result<'T, ApplicationError>> =
        try
            let t = fn ()

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
            Ok(f ())
        with ex ->
            Error(fromException ex)

    let tryCatchAsync (f: unit -> Task<'T>) : Task<Result<'T, ApplicationError>> =
        task {
            try
                let! v = f ()
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

(*
    Examples of using DomainError:

    // 1. Creating errors using static factory methods
    let validationError = DomainError.validation "ProductId cannot be empty"
    let notFoundError = DomainError.notFound "Product not found"
    let conflictError = DomainError.conflict "Product code already exists"
    let invariantError = DomainError.invariant "Cannot cancel a completed order"

    // 2. Creating errors with contextual data for debugging
    let validationWithData =
        DomainError.validationWith
            "Quantity must be positive"
            (Map.ofList [ ("Field", box "Quantity"); ("Value", box -5) ])

    let notFoundWithData =
        DomainError.notFoundWith
            "Product not found"
            (Map.ofList [ ("ProductId", box "PROD-123"); ("StockingPointId", box "SP-456") ])

    // 3. Pattern matching using active patterns
    let handleError (error: DomainError) =
        match error with
        | DomainError.Validation msg -> printfn "Validation error: %s" msg
        | DomainError.NotFound msg -> printfn "Not found: %s" msg
        | DomainError.Conflict msg -> printfn "Conflict: %s" msg
        | DomainError.Invariant msg -> printfn "Invariant violation: %s" msg

    // 4. Pattern matching with error code and data extraction
    let handleErrorWithDetails (error: DomainError) =
        match error with
        | DomainError.Validation msg ->
            printfn "Validation error [%s]: %s" error.Code msg
            printfn "Context: %A" error.Data
        | DomainError.NotFound msg ->
            printfn "Not found [%s]: %s" error.Code msg
            printfn "Context: %A" error.Data
        | DomainError.Conflict msg ->
            printfn "Conflict [%s]: %s" error.Code msg
            printfn "Context: %A" error.Data
        | DomainError.Invariant msg ->
            printfn "Invariant violation [%s]: %s" error.Code msg
            printfn "Context: %A" error.Data

    // 5. Using in Result types
    let validateProductId (productId: string) : Result<string, DomainError> =
        if System.String.IsNullOrWhiteSpace(productId) then
            Error(DomainError.validation "ProductId cannot be empty")
        else
            Ok productId

    let findProduct (productId: string) : Result<Product, DomainError> =
        match tryFindProduct productId with
        | Some product -> Ok product
        | None ->
            Error(
                DomainError.notFoundWith
                    $"Product {productId} not found"
                    (Map.ofList [ ("ProductId", box productId) ])
            )

    // 6. Pattern matching in Result handling
    let processResult (result: Result<Product, DomainError>) =
        match result with
        | Ok product -> printfn "Success: %A" product
        | Error(DomainError.Validation msg) -> printfn "Validation failed: %s" msg
        | Error(DomainError.NotFound msg) -> printfn "Not found: %s" msg
        | Error(DomainError.Conflict msg) -> printfn "Conflict: %s" msg
        | Error(DomainError.Invariant msg) -> printfn "Invariant violation: %s" msg

    // 7. Error code-based handling (programmatic)
    let handleByCode (error: DomainError) =
        match error.Code with
        | DomainErrorCodes.ValidationFailed -> "Handle validation error"
        | DomainErrorCodes.NotFound -> "Handle not found error"
        | DomainErrorCodes.Conflict -> "Handle conflict error"
        | DomainErrorCodes.InvariantViolation -> "Handle invariant violation"
        | _ -> "Unknown error"

    // 8. Extracting error information
    let errorInfo (error: DomainError) =
        {
            Code = error.Code
            Message = error.Message
            Data = error.Data
        }

    // 9. Combining multiple validation errors (using Result.mapError)
    let validateOrder (order: Order) : Result<Order, DomainError> =
        validateProductId order.ProductId
        |> Result.bind (fun _ -> validateQuantity order.Quantity)
        |> Result.mapError (fun err ->
            match err with
            | DomainError.Validation msg -> DomainError.validationWith msg (Map.ofList [ ("OrderId", box order.Id) ])
            | _ -> err
        )

    // 10. Pattern matching in async/asyncResult workflows
    let asyncProcessOrder (orderId: string) =
        async {
            match! findOrder orderId with
            | Ok order -> return Ok order
            | Error(DomainError.NotFound msg) ->
                return Error(DomainError.notFoundWith $"Order {orderId} not found" (Map.ofList [ ("OrderId", box orderId) ]))
            | Error err -> return Error err
        }
*)
