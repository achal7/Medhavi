namespace Medhavi.Demand

open System
open System.Text.Json.Serialization
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel

[<JsonFSharpConverter>]
type DemandObservationId = private DemandObservationId of string

module DemandObservationId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "DemandObservationId cannot be empty")
        else
            Ok(DemandObservationId(id.Trim()))

    let value (DemandObservationId id) = id

type ForecastPublicationId = private ForecastPublicationId of string

module ForecastPublicationId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace(id) then
            Error(DomainError.validation "ForecastPublicationId cannot be empty")
        else
            Ok(ForecastPublicationId(id.Trim()))

    let value (ForecastPublicationId id) = id

type DemandUnderstandingId = private DemandUnderstandingId of string

module DemandUnderstandingId =
    let create (id: string) =
        if System.String.IsNullOrWhiteSpace(id) then
            Error(Medhavi.Foundation.Failure.DomainError.validation "DemandUnderstandingId cannot be empty")
        else
            Ok(DemandUnderstandingId(id.Trim()))

    let value (DemandUnderstandingId id) = id

type DemandBehaviorAssessmentId = private DemandBehaviorAssessmentId of string

module DemandBehaviorAssessmentId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "DemandBehaviorAssessmentId cannot be empty")
        else
            Ok(DemandBehaviorAssessmentId(id.Trim()))

    let ofItemAndLocation (itemId: Medhavi.SemanticModel.ItemId) (locationId: Medhavi.SemanticModel.LocationId) =
        DemandBehaviorAssessmentId
            $"{Medhavi.SemanticModel.ItemId.value itemId}-{Medhavi.SemanticModel.LocationId.value locationId}"

    let value (DemandBehaviorAssessmentId id) = id

[<JsonFSharpConverter>]
type PlanningClassificationAssignmentId = private PlanningClassificationAssignmentId of string

module PlanningClassificationAssignmentId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "PlanningClassificationAssignmentId cannot be empty")
        else
            Ok(PlanningClassificationAssignmentId(id.Trim()))

    let ofComponents (entityType: string) (entityId: string) (classificationType: string) =
        PlanningClassificationAssignmentId $"{entityType.Trim()}-{entityId.Trim()}-{classificationType.Trim()}"

    let value (PlanningClassificationAssignmentId id) = id

[<JsonFSharpConverter>]
type DemandBehaviorAssignmentId = private DemandBehaviorAssignmentId of string

module DemandBehaviorAssignmentId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "DemandBehaviorAssignmentId cannot be empty")
        else
            Ok(DemandBehaviorAssignmentId(id.Trim()))

    let ofComponents
        (itemId: Medhavi.SemanticModel.ItemId)
        (locationId: Medhavi.SemanticModel.LocationId)
        (dimension: string)
        =
        DemandBehaviorAssignmentId
            $"{Medhavi.SemanticModel.ItemId.value itemId}-{Medhavi.SemanticModel.LocationId.value locationId}-{dimension.Trim()}"

    let value (DemandBehaviorAssignmentId id) = id

[<JsonFSharpConverter>]
type PlanningPriorityAssignmentId = private PlanningPriorityAssignmentId of string

module PlanningPriorityAssignmentId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "PlanningPriorityAssignmentId cannot be empty")
        else
            Ok(PlanningPriorityAssignmentId(id.Trim()))

    let ofComponents (entityType: string) (entityId: string) =
        PlanningPriorityAssignmentId $"{entityType.Trim()}-{entityId.Trim()}"

    let value (PlanningPriorityAssignmentId id) = id

[<JsonFSharpConverter>]
type ForecastQualityAssessmentId = private ForecastQualityAssessmentId of string

module ForecastQualityAssessmentId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "ForecastQualityAssessmentId cannot be empty")
        else
            Ok(ForecastQualityAssessmentId(id.Trim()))

    let ofComponents (scope: PlanningScopeId) (periodStart: Timestamp) (periodEnd: Timestamp) =
        ForecastQualityAssessmentId
            $"{PlanningScopeId.value scope}-{Timestamp.value periodStart:O}-{Timestamp.value periodEnd:O}"

    let value (ForecastQualityAssessmentId id) = id

[<JsonFSharpConverter>]
type DemandExceptionEvidenceId = private DemandExceptionEvidenceId of string

module DemandExceptionEvidenceId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "DemandExceptionEvidenceId cannot be empty")
        else
            Ok(DemandExceptionEvidenceId(id.Trim()))

    let ofComponents (exceptionType: string) (entityType: string) (entityId: string) (scope: PlanningScopeId) =
        DemandExceptionEvidenceId
            $"{exceptionType.Trim()}-{entityType.Trim()}-{entityId.Trim()}-{PlanningScopeId.value scope}"

    let value (DemandExceptionEvidenceId id) = id

[<JsonFSharpConverter>]
type DemandExplanationId = private DemandExplanationId of string

module DemandExplanationId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "DemandExplanationId cannot be empty")
        else
            Ok(DemandExplanationId(id.Trim()))

    let ofComponents (artifactType: string) (artifactId: string) (version: int) =
        DemandExplanationId $"{artifactType.Trim()}-{artifactId.Trim()}-v{version}"

    let value (DemandExplanationId id) = id

[<JsonFSharpConverter>]
type DemandLearningId = private DemandLearningId of string

module DemandLearningId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "DemandLearningId cannot be empty")
        else
            Ok(DemandLearningId(id.Trim()))

    let ofComponents (scope: PlanningScopeId) (learningType: string) (windowStart: Timestamp) (windowEnd: Timestamp) =
        let startStr = (Timestamp.value windowStart).ToString("yyyyMMdd")
        let endStr = (Timestamp.value windowEnd).ToString("yyyyMMdd")
        DemandLearningId $"learning-{PlanningScopeId.value scope}-{learningType.Trim()}-{startStr}-{endStr}"

    let value (DemandLearningId id) = id

[<JsonFSharpConverter>]
type ScenarioAdjustmentId = private ScenarioAdjustmentId of string

module ScenarioAdjustmentId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "ScenarioAdjustmentId cannot be empty")
        else
            Ok(ScenarioAdjustmentId(id.Trim()))

    let value (ScenarioAdjustmentId id) = id

[<JsonFSharpConverter>]
type DemandInterventionImpactId = private DemandInterventionImpactId of string

module DemandInterventionImpactId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "DemandInterventionImpactId cannot be empty")
        else
            Ok(DemandInterventionImpactId(id.Trim()))

    let ofComponents (interventionRef: ScenarioAdjustmentId) (item: ItemId) (location: LocationId) =
        DemandInterventionImpactId $"impact-{ScenarioAdjustmentId.value interventionRef}-{ItemId.value item}-{LocationId.value location}"

    let value (DemandInterventionImpactId id) = id

