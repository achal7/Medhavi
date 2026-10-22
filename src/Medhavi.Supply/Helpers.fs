[<AutoOpen>]
module Medhavi.Supply.Helpers

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel

let mapSemanticValidationToDomainError (failure: SemanticValidationError) : DomainError =
    match failure with
    | SemanticValidationError.EmptyRequiredField(objectName, fieldName) ->
        DomainError.validation($"{objectName} {fieldName} cannot be empty")
    | SemanticValidationError.NonUtcTimestamp(fieldName) -> DomainError.validation($"{fieldName} must be UTC")
    | SemanticValidationError.NegativeQuantity(fieldName) -> DomainError.validation($"{fieldName} cannot be negative")
    | SemanticValidationError.NonPositiveQuantity(fieldName) ->
        DomainError.validation($"{fieldName} cannot be zero or negative")
    | SemanticValidationError.NegativeDuration(fieldName) -> DomainError.validation($"{fieldName} cannot be negative")
    | SemanticValidationError.InvalidPercentage(objectName, fieldName) ->
        DomainError.validation($"{fieldName} must be between 0 and 100")
    | SemanticValidationError.DuplicateValue(objectName, fieldName) ->
        DomainError.validation($"{objectName} {fieldName} cannot be duplicated")
    | SemanticValidationError.InvalidWindow(message) -> DomainError.validation(message)
    | SemanticValidationError.InvariantViolation(objectName, message) -> DomainError.validation(message)
    | SemanticValidationError.EmptyIdentifier message -> DomainError.validation message
    | SemanticValidationError.InvalidCompositeIdentity(message) -> DomainError.validation(message)
    | SemanticValidationError.InvalidLifecycleTransition(message) -> DomainError.validation(message)

let validateItemId itemId = ItemId.create itemId |> Result.mapError mapSemanticValidationToDomainError |> fromResult

let validateLocationId locationId =
    LocationId.create locationId |> Result.mapError mapSemanticValidationToDomainError |> fromResult

let validateQty quantity = Quantity.create quantity |> Result.mapError mapSemanticValidationToDomainError |> fromResult

let validatePlanningScopeId scopeId =
    PlanningScopeId.create scopeId |> Result.mapError mapSemanticValidationToDomainError |> fromResult

let validateTimestamp timestamp =
    Timestamp.create timestamp |> Result.mapError mapSemanticValidationToDomainError |> fromResult

let validateSupplierId supplierId =
    SupplierId.create supplierId |> Result.mapError mapSemanticValidationToDomainError |> fromResult

let validateRecordId recordId = SupplyDataRecordId.create recordId |> fromResult

let validateForecastHorizonId horizonId = ForecastHorizonId.create horizonId |> fromResult

let validateEvaluationPeriodId periodId = EvaluationPeriodId.create periodId |> fromResult
let validatePlanningPeriodId periodId =
    PlanningPeriodId.create periodId |> Result.mapError mapSemanticValidationToDomainError |> fromResult

let validatePhysicalResourceId resourceId =
    PhysicalResourceId.create resourceId |> Result.mapError mapSemanticValidationToDomainError |> fromResult

let validateSupplierCommitmentId commitmentId = SupplierCommitmentId.create commitmentId |> fromResult

let validateSupplierCollaborationHealthAssessmentId healthId =
    SupplierCollaborationHealthAssessmentId.create healthId |> fromResult

let validateDistributionRequirementId reqId =
    DistributionRequirementId.create reqId |> fromResult

let validateDistributionPlanId planId =
    DistributionPlanId.create planId |> fromResult

let validateSupplyLearningId learningId =
    SupplyLearningId.create learningId |> fromResult

let validateTemporalWindow (startTs: Timestamp) (endTs: Timestamp) : Validation<TemporalWindow, DomainError> =
    TemporalWindow.create startTs endTs
    |> Result.mapError mapSemanticValidationToDomainError
    |> fromResult

/// Creates a DomainError from an authoritative ARS Rule artifact
let ruleViolation (rule: ArsArtifact) : DomainError =
    DomainError.rule(rule.Explanation, rule = rule.Id)

/// Creates a DomainError from an authoritative ARS Rule artifact with dynamic runtime context
let ruleViolationWithDetail (rule: ArsArtifact) (detail: string) : DomainError =
    DomainError.rule($"{rule.Explanation} ({detail})", rule = rule.Id)

/// Dispatches a strongly typed Business Notification as a canonical Envelope to the store
let dispatchNotification
    (dispatch: Envelope -> Task<unit>)
    (notificationId: string)
    (capabilityId: string)
    (aggregateType: string)
    (aggregateId: string)
    (payload: 'T)
    : TaskResult<unit, ApplicationError> =
    taskResult {
        let! json =
            Medhavi.Foundation.Codec.json.Encode payload
            |> Result.mapError(fun err -> Infrastructure(Serialization(sprintf "Notification encoding failed: %A" err)))
            |> TaskResult.ofResult

        let env = Envelope.Create(notificationId, "S", aggregateType, aggregateId, 1L, capabilityId, json)

        do!
            dispatch env
            |> TaskResult.ofTaskValue
            |> TaskResult.mapError(fun ex ->
                Infrastructure(EventStore(sprintf "Notification dispatch failed: %A" ex)))
    }
