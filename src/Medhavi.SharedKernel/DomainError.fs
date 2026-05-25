namespace Medhavi.SharedKernel

open System
open System.Text.Json.Serialization

/// Domain error codes for programmatic error handling
module DomainErrorCodes =
    let ValidationFailed = "DOMAIN_VALIDATION_FAILED"
    let NotFound = "DOMAIN_NOT_FOUND"
    let Conflict = "DOMAIN_CONFLICT"
    let InvariantViolation = "DOMAIN_INVARIANT_VIOLATION"
    let VersionMismatch = "DOMAIN_VERSION_MISMATCH"
    let External = "DOMAIN_EXTERNAL_ERROR"
    let BusinessRule = "DOMAIN_BUSINESS_RULE_VIOLATION"

[<JsonFSharpConverter>]
type DomainError =
    | Validation of code: string * message: string * data: Map<string, obj>
    | NotFound of code: string * message: string * data: Map<string, obj>
    | Conflict of code: string * message: string * data: Map<string, obj> // e.g., code uniqueness, base unit already exists
    | Invariant of code: string * message: string * data: Map<string, obj>
    | VersionMismatch of code: string * expected: Version * actual: Version
    | External of code: string * message: string * data: Map<string, obj>
    | BusinessRule of code: string * message: string * data: Map<string, obj>

    /// Human-readable error message
    member this.Message =
        match this with
        | Validation(_, msg, _)
        | NotFound(_, msg, _)
        | Conflict(_, msg, _)
        | Invariant(_, msg, _)
        | External(_, msg, _)
        | BusinessRule(_, msg, _) -> msg
        | VersionMismatch(_, expected, actual) -> $"Version mismatch: expected {expected}, actual {actual}"

    /// Machine-readable error code
    member this.Code =
        match this with
        | Validation(code, _, _)
        | NotFound(code, _, _)
        | Conflict(code, _, _)
        | Invariant(code, _, _)
        | External(code, _, _)
        | BusinessRule(code, _, _) -> code
        | VersionMismatch(code, _, _) -> code

    /// Optional contextual data for debugging and logging
    member this.Data =
        match this with
        | Validation(_, _, data)
        | NotFound(_, _, data)
        | Conflict(_, _, data)
        | Invariant(_, _, data)
        | External(_, _, data)
        | BusinessRule(_, _, data) -> data
        | VersionMismatch(_, expected, actual) -> Map.ofList [ ("Expected", box expected); ("Actual", box actual) ]

    /// Create a validation error
    static member validation message = Validation(DomainErrorCodes.ValidationFailed, message, Map.empty)

    /// Create a validation error with contextual data
    static member validationWith message (data: Map<string, obj>) =
        Validation(DomainErrorCodes.ValidationFailed, message, data)

    /// Create a not found error
    static member notFound message = NotFound(DomainErrorCodes.NotFound, message, Map.empty)

    /// Create a not found error with contextual data
    static member notFoundWith message (data: Map<string, obj>) = NotFound(DomainErrorCodes.NotFound, message, data)

    /// Create a conflict error
    static member conflict message = Conflict(DomainErrorCodes.Conflict, message, Map.empty)

    /// Create a conflict error with contextual data
    static member conflictWith message (data: Map<string, obj>) = Conflict(DomainErrorCodes.Conflict, message, data)

    /// Create an invariant violation error
    static member invariant message = Invariant(DomainErrorCodes.InvariantViolation, message, Map.empty)

    /// Create an invariant violation error with contextual data
    static member invariantWith message (data: Map<string, obj>) =
        Invariant(DomainErrorCodes.InvariantViolation, message, data)

    /// Create an external error
    static member external message = External(DomainErrorCodes.External, message, Map.empty)

    /// Create an external error with contextual data
    static member externalWith message (data: Map<string, obj>) = External(DomainErrorCodes.External, message, data)

    /// Create a business rule violation error
    static member businessRule message = BusinessRule(DomainErrorCodes.BusinessRule, message, Map.empty)

    /// Create a business rule violation error with contextual data
    static member businessRuleWith message (data: Map<string, obj>) =
        BusinessRule(DomainErrorCodes.BusinessRule, message, data)

/// Active patterns and utilities for DomainError
[<RequireQualifiedAccess>]
module DomainError =
    /// Pattern match on validation errors
    let (|Validation|_|) =
        function
        | Validation(_, msg, _) -> Some msg
        | _ -> None

    /// Pattern match on not found errors
    let (|NotFound|_|) =
        function
        | NotFound(_, msg, _) -> Some msg
        | _ -> None

    /// Pattern match on conflict errors
    let (|Conflict|_|) =
        function
        | Conflict(_, msg, _) -> Some msg
        | _ -> None

    /// Pattern match on invariant errors
    let (|Invariant|_|) =
        function
        | Invariant(_, msg, _) -> Some msg
        | _ -> None

    /// Pattern match on external errors
    let (|External|_|) =
        function
        | External(_, msg, _) -> Some msg
        | _ -> None

    /// Pattern match on business rule errors
    let (|BusinessRule|_|) =
        function
        | BusinessRule(_, msg, _) -> Some msg
        | _ -> None
