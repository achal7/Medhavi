/// SE-S-110 — Supply Exception Assessment ACL
/// Governed by: PO-S-120 Exception Detection Policy, ARS §14
module Medhavi.Supply.DetectSupplyExceptions.SupplyExceptionAssessment.ACL

open System
open Medhavi.Common.Validation
open Medhavi.Contracts.Supply
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model

/// Validated evaluation request payload translated from external EvaluateSupplyExceptionReq
type ValidatedExceptionEvaluationReq =
    {
        GovernedPolicyReference: string
        Scope: ExceptionScope
        PlanningScopeId: PlanningScopeId
        ExceptionType: ExceptionType
        TriggeringEvidence: string list
        EvaluationTimestamp: Timestamp
    }

let validateExceptionScope (entityType: string) (entityId: string) : Validation<ExceptionScope, DomainError> =
    if String.IsNullOrWhiteSpace entityType then
        fromResult(Error(DomainError.validation "ScopeEntityType cannot be empty"))
    else
        match entityType.Trim().ToLowerInvariant() with
        | "item" ->
            ItemId.create entityId
            |> Result.map(fun item -> ExceptionScope.ItemScope(item, None))
            |> Result.mapError(fun e -> DomainError.validation(sprintf "Invalid ItemId: %A" e))
            |> fromResult
        | "location" ->
            LocationId.create entityId
            |> Result.map ExceptionScope.LocationScope
            |> Result.mapError(fun e -> DomainError.validation(sprintf "Invalid LocationId: %A" e))
            |> fromResult
        | "supplier" -> (fun s -> ExceptionScope.SupplierScope s) <!> validateSupplierId entityId
        | "resource" -> (fun r -> ExceptionScope.ResourceScope r) <!> validatePhysicalResourceId entityId
        | "planningscope" -> (fun p -> ExceptionScope.PlanningScope p) <!> validatePlanningScopeId entityId
        | "enterprise" -> fromResult(Ok ExceptionScope.EnterpriseScope)
        | unknown -> fromResult(Error(DomainError.validation(sprintf "Unknown ScopeEntityType: %s" unknown)))

let validateExceptionType (exTypeStr: string) : Validation<ExceptionType, DomainError> =
    if String.IsNullOrWhiteSpace exTypeStr then
        fromResult(Error(DomainError.validation "ExceptionType cannot be empty"))
    else
        match exTypeStr.Trim().ToLowerInvariant() with
        | "supplydisruption" -> fromResult(Ok ExceptionType.SupplyDisruption)
        | "servicepolicyviolation" -> fromResult(Ok ExceptionType.ServicePolicyViolation)
        | "capacitypolicyviolation" -> fromResult(Ok ExceptionType.CapacityPolicyViolation)
        | "inventorypolicyviolation" -> fromResult(Ok ExceptionType.InventoryPolicyViolation)
        | "suppliercollaborationbreach" -> fromResult(Ok ExceptionType.SupplierCollaborationBreach)
        | "externalevent" -> fromResult(Ok ExceptionType.ExternalEvent)
        | "emergingrisk" -> fromResult(Ok ExceptionType.EmergingRisk)
        | unknown -> fromResult(Error(DomainError.validation(sprintf "Unknown ExceptionType: %s" unknown)))

/// Translates external request into domain ValidatedExceptionEvaluationReq via applicative validation
let toEvaluationReq (req: EvaluateSupplyExceptionReq) : Validation<ValidatedExceptionEvaluationReq, DomainError> =
    let construct policyRef scope plScope exType ev timestamp =
        {
            GovernedPolicyReference = policyRef
            Scope = scope
            PlanningScopeId = plScope
            ExceptionType = exType
            TriggeringEvidence = ev
            EvaluationTimestamp = timestamp
        }

    let policyRefV =
        if String.IsNullOrWhiteSpace req.GovernedPolicyReference then
            fromResult(Error(DomainError.validation "GovernedPolicyReference cannot be empty"))
        else
            fromResult(Ok req.GovernedPolicyReference)

    let timestampV =
        Timestamp.create req.EvaluationTimestamp
        |> Result.mapError(fun e -> DomainError.validation(sprintf "Invalid Timestamp: %A" e))
        |> fromResult

    construct <!> policyRefV
    <*> validateExceptionScope req.ScopeEntityType req.ScopeEntityId
    <*> validatePlanningScopeId req.PlanningScopeId
    <*> validateExceptionType req.ExceptionType
    <*> fromResult(Ok req.TriggeringEvidence)
    <*> timestampV

/// Outbound ACL: maps domain aggregate events to canonical ARS Enterprise Events (EV-S-100, EV-S-101, EV-S-102)
let toArsEventId =
    function
    | SupplyExceptionEvent.SupplyExceptionDetected _ ->
        ArsIdentifiers.EnterpriseEvents.supplyExceptionAssessmentUpdated.Id
    | SupplyExceptionEvent.SupplyExceptionConfirmed(_, escalated) ->
        if escalated then
            ArsIdentifiers.EnterpriseEvents.supplyExceptionDetectionEvidenceProduced.Id
        else
            ArsIdentifiers.EnterpriseEvents.supplyExceptionAssessmentUpdated.Id
    | SupplyExceptionEvent.SupplyExceptionEvidenceUpdated _ ->
        ArsIdentifiers.EnterpriseEvents.supplyExceptionAssessmentUpdated.Id
    | SupplyExceptionEvent.SupplyExceptionResolved(_, wasEscalated) ->
        if wasEscalated then
            ArsIdentifiers.EnterpriseEvents.supplyExceptionResolutionEvidenceProduced.Id
        else
            ArsIdentifiers.EnterpriseEvents.supplyExceptionAssessmentUpdated.Id
    | SupplyExceptionEvent.SupplyExceptionArchived _ ->
        ArsIdentifiers.EnterpriseEvents.supplyExceptionAssessmentUpdated.Id
