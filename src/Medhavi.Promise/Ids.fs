namespace Medhavi.Promise

open System
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel

type OrderBookSnapshotId = private OrderBookSnapshotId of string

module OrderBookSnapshotId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "OrderBookSnapshotId cannot be empty")
        else
            Ok(OrderBookSnapshotId(id.Trim()))

    let value (OrderBookSnapshotId id) = id

type OrderLineId = private OrderLineId of string

module OrderLineId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "OrderLineId cannot be empty")
        else
            Ok(OrderLineId(id.Trim()))

    let value (OrderLineId id) = id

type CustomerPromiseProfileId = private CustomerPromiseProfileId of string

module CustomerPromiseProfileId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "CustomerPromiseProfileId cannot be empty")
        else
            Ok(CustomerPromiseProfileId(id.Trim()))

    let ofCustomer (customerId: CustomerId) =
        CustomerPromiseProfileId(CustomerId.value customerId)

    let value (CustomerPromiseProfileId id) = id

type AllocationPoolId = private AllocationPoolId of string

module AllocationPoolId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "AllocationPoolId cannot be empty")
        else
            Ok(AllocationPoolId(id.Trim()))

    let ofScopeAndItem (scopeId: PlanningScopeId) (itemId: ItemId) (locationId: LocationId) =
        AllocationPoolId $"{PlanningScopeId.value scopeId}#{ItemId.value itemId}#{LocationId.value locationId}"

    let value (AllocationPoolId id) = id

type AllocationBucketId = private AllocationBucketId of string

module AllocationBucketId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "AllocationBucketId cannot be empty")
        else
            Ok(AllocationBucketId(id.Trim()))

    let value (AllocationBucketId id) = id

type TemporaryReservationId = private TemporaryReservationId of string

module TemporaryReservationId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "TemporaryReservationId cannot be empty")
        else
            Ok(TemporaryReservationId(id.Trim()))

    let value (TemporaryReservationId id) = id

type OrderCommitmentId = private OrderCommitmentId of string

module OrderCommitmentId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "OrderCommitmentId cannot be empty")
        else
            Ok(OrderCommitmentId(id.Trim()))

    let ofOrderAndLine (orderId: string) (lineId: OrderLineId) =
        OrderCommitmentId $"{orderId.Trim()}#{OrderLineId.value lineId}"

    let value (OrderCommitmentId id) = id

type BackorderId = private BackorderId of string

module BackorderId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "BackorderId cannot be empty")
        else
            Ok(BackorderId(id.Trim()))

    let ofOrderLine (lineId: OrderLineId) =
        BackorderId $"BO#{OrderLineId.value lineId}"

    let value (BackorderId id) = id

type PromiseRiskAssessmentId = private PromiseRiskAssessmentId of string

module PromiseRiskAssessmentId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "PromiseRiskAssessmentId cannot be empty")
        else
            Ok(PromiseRiskAssessmentId(id.Trim()))

    let ofCommitment (commitmentId: OrderCommitmentId) =
        PromiseRiskAssessmentId $"RISK#{OrderCommitmentId.value commitmentId}"

    let value (PromiseRiskAssessmentId id) = id

type PromiseQualityAssessmentId = private PromiseQualityAssessmentId of string

module PromiseQualityAssessmentId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "PromiseQualityAssessmentId cannot be empty")
        else
            Ok(PromiseQualityAssessmentId(id.Trim()))

    let ofCommitment (commitmentId: OrderCommitmentId) =
        PromiseQualityAssessmentId $"QA#{OrderCommitmentId.value commitmentId}"

    let value (PromiseQualityAssessmentId id) = id

type PromiseExceptionId = private PromiseExceptionId of string

module PromiseExceptionId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "PromiseExceptionId cannot be empty")
        else
            Ok(PromiseExceptionId(id.Trim()))

    let ofCommitment (commitmentId: OrderCommitmentId) (epoch: int64) =
        PromiseExceptionId $"EX#{OrderCommitmentId.value commitmentId}#{epoch}"

    let value (PromiseExceptionId id) = id

type PromiseExplanationId = private PromiseExplanationId of string

module PromiseExplanationId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "PromiseExplanationId cannot be empty")
        else
            Ok(PromiseExplanationId(id.Trim()))

    let ofCommitment (commitmentId: OrderCommitmentId) =
        PromiseExplanationId $"EXPL#{OrderCommitmentId.value commitmentId}"

    let value (PromiseExplanationId id) = id

type PromiseLearningId = private PromiseLearningId of string

module PromiseLearningId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "PromiseLearningId cannot be empty")
        else
            Ok(PromiseLearningId(id.Trim()))

    let ofScope (scopeId: PlanningScopeId) =
        PromiseLearningId $"LEARN#{PlanningScopeId.value scopeId}"

    let value (PromiseLearningId id) = id

type PromiseScorecardId = private PromiseScorecardId of string

module PromiseScorecardId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "PromiseScorecardId cannot be empty")
        else
            Ok(PromiseScorecardId(id.Trim()))

    let ofScopeAndPeriod (scopeId: PlanningScopeId) (period: string) =
        PromiseScorecardId $"SC#{PlanningScopeId.value scopeId}#{period.Trim()}"

    let value (PromiseScorecardId id) = id

