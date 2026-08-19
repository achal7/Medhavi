namespace Medhavi.Core

open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.Contracts

[<AutoOpen>]
module Helpers =
    open Medhavi.SemanticModel

    let mapAppErrorToApiError (appError: ApplicationError) : ApiError =
        match appError with
        | Domain d ->
            match d with
            | DomainError.ValidationFailed(_, errs) -> ApiError.validation(String.concat "; " (errs |> List.map snd))
            | DomainError.BusinessRuleViolated(_, ruleId, msg) -> ApiError.businessRule($"[{ruleId}] {msg}")
            | DomainError.InvariantViolated(_, msg) -> ApiError.conflict(msg)
            | DomainError.EntityNotFound(_, entityType, entityId) -> ApiError.notFound entityType entityId
            | DomainError.Conflict(_, msg) -> ApiError.conflict(msg)
        | Validation errs -> ApiError.validation(String.concat "; " (errs |> List.map snd))
        | Infrastructure i -> ApiError.infrastructureError i.Message

    let mapSemanticValidationToDomainError (failure: SemanticValidationError) : DomainError =
        match failure with
        | SemanticValidationError.EmptyRequiredField(objectName, fieldName) ->
            DomainError.validation($"{objectName} {fieldName} cannot be empty")
        | SemanticValidationError.NonUtcTimestamp(fieldName) -> DomainError.validation($"{fieldName} must be UTC")
        | SemanticValidationError.NegativeQuantity(fieldName) ->
            DomainError.validation($"{fieldName} cannot be negative")
        | SemanticValidationError.NonPositiveQuantity(fieldName) ->
            DomainError.validation($"{fieldName} cannot be zero or negative")
        | SemanticValidationError.NegativeDuration(fieldName) ->
            DomainError.validation($"{fieldName} cannot be negative")
        | SemanticValidationError.InvalidPercentage(objectName, fieldName) ->
            DomainError.validation($"{fieldName} must be between 0 and 100")
        | SemanticValidationError.DuplicateValue(objectName, fieldName) ->
            DomainError.validation($"{objectName} {fieldName} cannot be duplicated")
        | SemanticValidationError.InvalidWindow(message) -> DomainError.validation(message)
        | SemanticValidationError.InvariantViolation(objectName, message) -> DomainError.validation(message)
        | SemanticValidationError.EmptyIdentifier message -> DomainError.validation message
        | SemanticValidationError.InvalidCompositeIdentity(message) -> DomainError.validation(message)
        | SemanticValidationError.InvalidLifecycleTransition(message) -> DomainError.validation(message)

    let validateDemandId demandId =
        DemandId.create demandId |> Result.mapError mapSemanticValidationToDomainError |> fromResult

    let validateItemId item = ItemId.create item |> Result.mapError mapSemanticValidationToDomainError |> fromResult

    let validateLocationId location =
        LocationId.create location |> Result.mapError mapSemanticValidationToDomainError |> fromResult

    let validateCustomerId customer =
        match customer with
        | None -> Valid None
        | Some c ->
            CustomerId.create c |> Result.mapError mapSemanticValidationToDomainError |> Result.map Some |> fromResult

    let validateQuantity quantity =
        Quantity.create quantity |> Result.mapError mapSemanticValidationToDomainError |> fromResult

    let validateTimestamp timestamp =
        Timestamp.create timestamp |> Result.mapError mapSemanticValidationToDomainError |> fromResult

    let validateScopeId scopeId =
        PlanningScopeId.create scopeId |> Result.mapError mapSemanticValidationToDomainError |> fromResult
