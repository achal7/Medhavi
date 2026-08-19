namespace Medhavi.Demand

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.Foundation.Contracts
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

    let validateItemId itemId = ItemId.create itemId |> Result.mapError mapSemanticValidationToDomainError |> fromResult

    let validateLocationId locationId =
        LocationId.create locationId |> Result.mapError mapSemanticValidationToDomainError |> fromResult

    let validateQty quantity =
        Quantity.create quantity |> Result.mapError mapSemanticValidationToDomainError |> fromResult

    let validatePlanningScopeId scopeId =
        PlanningScopeId.create scopeId |> Result.mapError mapSemanticValidationToDomainError |> fromResult

    let validateTimestamp timestamp =
        Timestamp.create timestamp |> Result.mapError mapSemanticValidationToDomainError |> fromResult

    let validateVocabularyEntryId vid =
        VocabularyEntryId.create vid |> Result.mapError mapSemanticValidationToDomainError |> fromResult

    let validateForecastPublicationId pubId = ForecastPublicationId.create pubId |> fromResult

    let validatePlanningClassificationAssignmentId id = PlanningClassificationAssignmentId.create id |> fromResult

    let validateDemandBehaviorAssignmentId id = DemandBehaviorAssignmentId.create id |> fromResult

    let validatePlanningPriorityAssignmentId id = PlanningPriorityAssignmentId.create id |> fromResult

    let validateForecastQualityAssessmentId id = ForecastQualityAssessmentId.create id |> fromResult

    let validateDemandExceptionEvidenceId id = DemandExceptionEvidenceId.create id |> fromResult

    let validateDemandExplanationId id = DemandExplanationId.create id |> fromResult

    let validateDemandLearningId id = DemandLearningId.create id |> fromResult

    let validateScenarioAdjustmentId id = ScenarioAdjustmentId.create id |> fromResult

    let validateDemandInterventionImpactId id = DemandInterventionImpactId.create id |> fromResult

    /// Dispatches a strongly typed Business Notification as a canonical Envelope to the store
    let dispatchNotification
        (dispatch: Envelope -> Task<unit>)
        (notificationId: string)
        (capabilityId: string)
        (aggregateType: string)
        (aggregateId: string)
        (payload: 'T)
        : TaskResult<unit, ApiError> =
        taskResult {
            let! json =
                Medhavi.Foundation.Codec.json.Encode payload
                |> Result.mapError(fun err ->
                    ApiError.infrastructureError(sprintf "Notification encoding failed: %A" err))
                |> TaskResult.ofResult

            let env = Envelope.Create(notificationId, "D", aggregateType, aggregateId, 1L, capabilityId, json)

            do!
                dispatch env
                |> TaskResult.ofTaskValue
                |> TaskResult.mapError(fun ex ->
                    ApiError.infrastructureError(sprintf "Notification dispatch failed: %A" ex))
        }
