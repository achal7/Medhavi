namespace Medhavi.Supply

open System
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel

type SupplyDataRecordId = private SupplyDataRecordId of string

module SupplyDataRecordId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "SupplyDataRecordId cannot be empty")
        else
            Ok(SupplyDataRecordId(id.Trim()))

    let value (SupplyDataRecordId id) = id

type SupplyUnderstandingId = private SupplyUnderstandingId of string

module SupplyUnderstandingId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "SupplyUnderstandingId cannot be empty")
        else
            Ok(SupplyUnderstandingId(id.Trim()))

    let ofPlanningScope (scopeId: PlanningScopeId) = SupplyUnderstandingId(PlanningScopeId.value scopeId)

    let value (SupplyUnderstandingId id) = id

type ForecastHorizonId = private ForecastHorizonId of string

module ForecastHorizonId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "ForecastHorizonId cannot be empty")
        else
            Ok(ForecastHorizonId(id.Trim()))

    let value (ForecastHorizonId id) = id

type SupplyPlanId = private SupplyPlanId of string

module SupplyPlanId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "SupplyPlanId cannot be empty")
        else
            Ok(SupplyPlanId(id.Trim()))

    let ofScopeAndHorizon (scopeId: PlanningScopeId) (horizonId: ForecastHorizonId) =
        SupplyPlanId $"{PlanningScopeId.value scopeId}#{ForecastHorizonId.value horizonId}"

    let value (SupplyPlanId id) = id

type SupplyPlanVersionId = private SupplyPlanVersionId of string

module SupplyPlanVersionId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "SupplyPlanVersionId cannot be empty")
        else
            Ok(SupplyPlanVersionId(id.Trim()))

    let value (SupplyPlanVersionId id) = id
    let defaultId = SupplyPlanVersionId "SPV-1"

type PlannedSupplyOrderId = private PlannedSupplyOrderId of string

module PlannedSupplyOrderId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "PlannedSupplyOrderId cannot be empty")
        else
            Ok(PlannedSupplyOrderId(id.Trim()))

    let value (PlannedSupplyOrderId id) = id
    let defaultId = PlannedSupplyOrderId "ORD-1"

type ItemLocationId = private ItemLocationId of string

module ItemLocationId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "ItemLocationId cannot be empty")
        else
            Ok(ItemLocationId(id.Trim()))

    let ofItemAndLocation (itemId: ItemId) (locationId: LocationId) =
        ItemLocationId $"{ItemId.value itemId}#{LocationId.value locationId}"

    let value (ItemLocationId id) = id

type EvaluationPeriodId = private EvaluationPeriodId of string

module EvaluationPeriodId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "EvaluationPeriodId cannot be empty")
        else
            Ok(EvaluationPeriodId(id.Trim()))

    let value (EvaluationPeriodId id) = id
    let defaultId = EvaluationPeriodId "default"

type InventoryHealthAssessmentId = private InventoryHealthAssessmentId of string

module InventoryHealthAssessmentId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "InventoryHealthAssessmentId cannot be empty")
        else
            Ok(InventoryHealthAssessmentId(id.Trim()))

    let ofScopeAndPeriod (scopeId: PlanningScopeId) (periodId: EvaluationPeriodId) =
        InventoryHealthAssessmentId $"{PlanningScopeId.value scopeId}#{EvaluationPeriodId.value periodId}"

    let value (InventoryHealthAssessmentId id) = id

type CapacityHealthAssessmentId = private CapacityHealthAssessmentId of string

module CapacityHealthAssessmentId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "CapacityHealthAssessmentId cannot be empty")
        else
            Ok(CapacityHealthAssessmentId(id.Trim()))

    let ofScopeAndPeriod (scopeId: PlanningScopeId) (periodId: EvaluationPeriodId) =
        CapacityHealthAssessmentId $"{PlanningScopeId.value scopeId}#{EvaluationPeriodId.value periodId}"

    let value (CapacityHealthAssessmentId id) = id

type SupplierCommitmentId = private SupplierCommitmentId of string

module SupplierCommitmentId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "SupplierCommitmentId cannot be empty")
        else
            Ok(SupplierCommitmentId(id.Trim()))

    let ofComposite (supplierId: SupplierId) (itemId: ItemId) (requiredDate: Timestamp) =
        let dateStr = (Timestamp.value requiredDate).ToString("yyyy-MM-dd")
        SupplierCommitmentId $"{SupplierId.value supplierId}#{ItemId.value itemId}#{dateStr}"

    let value (SupplierCommitmentId id) = id

type SupplierCollaborationHealthAssessmentId = private SupplierCollaborationHealthAssessmentId of string

module SupplierCollaborationHealthAssessmentId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "SupplierCollaborationHealthAssessmentId cannot be empty")
        else
            Ok(SupplierCollaborationHealthAssessmentId(id.Trim()))

    let ofScopeAndPeriod (scopeId: PlanningScopeId) (periodId: EvaluationPeriodId) =
        SupplierCollaborationHealthAssessmentId $"{PlanningScopeId.value scopeId}#{EvaluationPeriodId.value periodId}"

    let value (SupplierCollaborationHealthAssessmentId id) = id

type ProcurementRequirementId = private ProcurementRequirementId of string

module ProcurementRequirementId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "ProcurementRequirementId cannot be empty")
        else
            Ok(ProcurementRequirementId(id.Trim()))

    let value (ProcurementRequirementId id) = id

type ProcurementPlanId = private ProcurementPlanId of string

module ProcurementPlanId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "ProcurementPlanId cannot be empty")
        else
            Ok(ProcurementPlanId(id.Trim()))

    let ofScopeAndPeriod (scopeId: PlanningScopeId) (cycleId: string) =
        ProcurementPlanId $"{PlanningScopeId.value scopeId}#{cycleId.Trim()}"

    let value (ProcurementPlanId id) = id

type DistributionRequirementId = private DistributionRequirementId of string

module DistributionRequirementId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "DistributionRequirementId cannot be empty")
        else
            Ok(DistributionRequirementId(id.Trim()))

    let value (DistributionRequirementId id) = id

type DistributionPlanId = private DistributionPlanId of string

module DistributionPlanId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "DistributionPlanId cannot be empty")
        else
            Ok(DistributionPlanId(id.Trim()))

    let ofScopeAndCycle (scopeId: PlanningScopeId) (cycle: string) =
        DistributionPlanId $"{PlanningScopeId.value scopeId}#{cycle.Trim()}"

    let value (DistributionPlanId id) = id

type SupplyChangeAssessmentId = private SupplyChangeAssessmentId of string

module SupplyChangeAssessmentId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "SupplyChangeAssessmentId cannot be empty")
        else
            Ok(SupplyChangeAssessmentId(id.Trim()))

    let value (SupplyChangeAssessmentId id) = id

type SupplyQualityAssessmentId = private SupplyQualityAssessmentId of string

module SupplyQualityAssessmentId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "SupplyQualityAssessmentId cannot be empty")
        else
            Ok(SupplyQualityAssessmentId(id.Trim()))

    let ofScopeAndPeriod (scopeId: PlanningScopeId) (periodId: EvaluationPeriodId) =
        SupplyQualityAssessmentId $"{PlanningScopeId.value scopeId}#{EvaluationPeriodId.value periodId}"

    let ofScopeAndPeriodVersion (scopeId: PlanningScopeId) (periodId: EvaluationPeriodId) (version: int) =
        SupplyQualityAssessmentId $"{PlanningScopeId.value scopeId}#{EvaluationPeriodId.value periodId}#v{version}"

    let ofScopeHorizonPeriod (scopeId: PlanningScopeId) (horizon: PlanningHorizon) (periodId: EvaluationPeriodId) =
        SupplyQualityAssessmentId $"{PlanningScopeId.value scopeId}#{Timestamp.value horizon.Start:yyyyMMdd}..{Timestamp.value horizon.End:yyyyMMdd}#{EvaluationPeriodId.value periodId}"

    let ofScopeHorizonPeriodVersion (scopeId: PlanningScopeId) (horizon: PlanningHorizon) (periodId: EvaluationPeriodId) (version: int) =
        SupplyQualityAssessmentId $"{PlanningScopeId.value scopeId}#{Timestamp.value horizon.Start:yyyyMMdd}..{Timestamp.value horizon.End:yyyyMMdd}#{EvaluationPeriodId.value periodId}#v{version}"

    let value (SupplyQualityAssessmentId id) = id

type SupplyExceptionId = private SupplyExceptionId of string

module SupplyExceptionId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "SupplyExceptionId cannot be empty")
        else
            Ok(SupplyExceptionId(id.Trim()))

    let newId () =
        SupplyExceptionId(Guid.NewGuid().ToString("N"))

    let ofBusinessIdentity (policyRef: string) (scopeKey: string) (exceptionType: string) =
        SupplyExceptionId $"{policyRef.Trim()}#{scopeKey.Trim()}#{exceptionType.Trim()}"

    let value (SupplyExceptionId id) = id

type SupplyExplanationId = private SupplyExplanationId of string

module SupplyExplanationId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "SupplyExplanationId cannot be empty")
        else
            Ok(SupplyExplanationId(id.Trim()))

    let ofComponents (artifactType: string) (artifactId: string) (version: int64) =
        SupplyExplanationId $"{artifactType.Trim()}#{artifactId.Trim()}#v{version}"

    let value (SupplyExplanationId id) = id

type SupplyLearningId = private SupplyLearningId of string

module SupplyLearningId =
    let create (id: string) =
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "SupplyLearningId cannot be empty")
        else
            Ok(SupplyLearningId(id.Trim()))

    let ofComponents (subjectType: string) (subjectId: string) (learningType: string) (version: int64) =
        SupplyLearningId $"{subjectType.Trim()}#{subjectId.Trim()}#{learningType.Trim()}#v{version}"

    let ofIdentity (id: string) =
        SupplyLearningId(id.Trim())

    let value (SupplyLearningId id) = id

