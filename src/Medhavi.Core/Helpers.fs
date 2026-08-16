namespace Medhavi.Core

open Medhavi.Foundation.Failure
open Medhavi.Contracts

[<AutoOpen>]
module Helpers =

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
