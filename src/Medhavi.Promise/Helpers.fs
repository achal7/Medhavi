[<AutoOpen>]
module Medhavi.Promise.Helpers

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel

let mapSemanticValidationToDomainError (failure: SemanticValidationError) : DomainError =
    match failure with
    | SemanticValidationError.EmptyRequiredField(objectName, fieldName) ->
        DomainError.validation ($"{objectName} {fieldName} cannot be empty")
    | SemanticValidationError.NonUtcTimestamp(fieldName) -> DomainError.validation ($"{fieldName} must be UTC")
    | SemanticValidationError.NegativeQuantity(fieldName) -> DomainError.validation ($"{fieldName} cannot be negative")
    | SemanticValidationError.NonPositiveQuantity(fieldName) ->
        DomainError.validation ($"{fieldName} cannot be zero or negative")
    | SemanticValidationError.NegativeDuration(fieldName) -> DomainError.validation ($"{fieldName} cannot be negative")
    | SemanticValidationError.InvalidPercentage(objectName, fieldName) ->
        DomainError.validation ($"{fieldName} must be between 0 and 100")
    | SemanticValidationError.DuplicateValue(objectName, fieldName) ->
        DomainError.validation ($"{objectName} {fieldName} cannot be duplicated")
    | SemanticValidationError.InvalidWindow(message) -> DomainError.validation message
    | SemanticValidationError.InvariantViolation(objectName, message) -> DomainError.validation message
    | SemanticValidationError.EmptyIdentifier message -> DomainError.validation message
    | SemanticValidationError.InvalidCompositeIdentity(message) -> DomainError.validation message
    | SemanticValidationError.InvalidLifecycleTransition(message) -> DomainError.validation message

let validateItemId itemId =
    ItemId.create itemId |> Result.mapError mapSemanticValidationToDomainError |> fromResult

let validateLocationId locationId =
    LocationId.create locationId |> Result.mapError mapSemanticValidationToDomainError |> fromResult

let validateCustomerId customerId =
    CustomerId.create customerId |> Result.mapError mapSemanticValidationToDomainError |> fromResult

let validateQty quantity =
    Quantity.create quantity |> Result.mapError mapSemanticValidationToDomainError |> fromResult

let validatePlanningScopeId scopeId =
    PlanningScopeId.create scopeId |> Result.mapError mapSemanticValidationToDomainError |> fromResult

let validateTimestamp timestamp =
    Timestamp.create timestamp |> Result.mapError mapSemanticValidationToDomainError |> fromResult

let validatePhysicalResourceId resourceId =
    PhysicalResourceId.create resourceId |> Result.mapError mapSemanticValidationToDomainError |> fromResult

let validateOrderBookSnapshotId snapshotId =
    OrderBookSnapshotId.create snapshotId |> fromResult

let validateOrderLineId lineId =
    OrderLineId.create lineId |> fromResult

let validateCustomerPromiseProfileId profileId =
    CustomerPromiseProfileId.create profileId |> fromResult

let validateAllocationPoolId poolId =
    AllocationPoolId.create poolId |> fromResult

let validateAllocationBucketId bucketId =
    AllocationBucketId.create bucketId |> fromResult

let validateTemporaryReservationId reservationId =
    TemporaryReservationId.create reservationId |> fromResult

let validateOrderCommitmentId commitmentId =
    OrderCommitmentId.create commitmentId |> fromResult

let validateBackorderId backorderId =
    BackorderId.create backorderId |> fromResult

let validatePromiseRiskAssessmentId assessmentId =
    PromiseRiskAssessmentId.create assessmentId |> fromResult

let validatePromiseQualityAssessmentId assessmentId =
    PromiseQualityAssessmentId.create assessmentId |> fromResult

let validatePromiseExceptionId exceptionId =
    PromiseExceptionId.create exceptionId |> fromResult

let validatePromiseExplanationId explanationId =
    PromiseExplanationId.create explanationId |> fromResult

let validatePromiseLearningId learningId =
    PromiseLearningId.create learningId |> fromResult

let validatePromiseScorecardId scorecardId =
    PromiseScorecardId.create scorecardId |> fromResult

let sequenceValidations (validations: Validation<'a, DomainError> list) : Validation<'a list, DomainError> =
    let folder acc v =
        match acc, v with
        | Valid xs, Valid x -> Valid (x :: xs)
        | Invalid e1, Invalid e2 -> Invalid (e1 @ e2)
        | Invalid e1, Valid _ -> Invalid e1
        | Valid _, Invalid e2 -> Invalid e2
    validations
    |> List.fold folder (Valid [])
    |> function
        | Valid xs -> Valid (List.rev xs)
        | Invalid errs -> Invalid errs


