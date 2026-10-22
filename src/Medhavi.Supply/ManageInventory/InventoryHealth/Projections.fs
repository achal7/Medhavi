/// SE-S-032 — Inventory Health Projections
module Medhavi.Supply.ManageInventory.InventoryHealth.Projections

open System
open Medhavi.Contracts.Supply
open Medhavi.SemanticModel
open Medhavi.Supply
open Model

let mapSummaryToDto (s: ItemHealthSummary) : ItemHealthSummaryDto =
    {
        ItemId = ItemId.value s.Item
        LocationId = LocationId.value s.Location
        HealthClassification = s.HealthClassification.ToString()
        DaysOfSupply = s.DaysOfSupply
        PolicyComplianceStatus = s.PolicyComplianceStatus.ToString()
        StockoutRiskExposure = s.StockoutRiskExposure
        ExcessValue = s.ExcessValue
    }

let mapVersionToDto
    (scope: PlanningScopeId)
    (period: EvaluationPeriodId)
    (v: InventoryHealthAssessmentVersion)
    : InventoryHealthAssessmentVersionDto =
    {
        VersionNumber = v.VersionNumber
        LifecycleState = sprintf "%A" v.State
        AssessmentScopeId = PlanningScopeId.value scope
        EvaluationPeriodId = EvaluationPeriodId.value period
        ItemHealthSummaries = v.ItemHealthSummaries |> List.map mapSummaryToDto
        TotalExcessValue = v.TotalExcessValue
        TotalStockoutRiskExposure = v.TotalStockoutRiskExposure
        AssessmentConfidence = v.AssessmentConfidence
        AssessmentRationale = v.AssessmentRationale
        PublicationTimestamp = v.PublicationTimestamp |> Option.map Timestamp.value
    }

let mapToDto (assessment: InventoryHealthAssessment) : InventoryHealthAssessmentDto =
    let versionsDto =
        assessment.Versions |> List.map(mapVersionToDto assessment.AssessmentScopeId assessment.EvaluationPeriodId)

    let currentPublished = versionsDto |> List.tryFind(fun v -> v.LifecycleState = "Published")

    {
        AssessmentScopeId = PlanningScopeId.value assessment.AssessmentScopeId
        EvaluationPeriodId = EvaluationPeriodId.value assessment.EvaluationPeriodId
        CurrentVersion = currentPublished
        AllVersions = versionsDto
    }

let seedFromAggregates
    (aggregates: InventoryHealthAssessment list)
    : Map<InventoryHealthAssessmentId, InventoryHealthAssessmentDto> =
    aggregates
    |> List.map(fun a ->
        let key = InventoryHealthAssessmentId.ofScopeAndPeriod a.AssessmentScopeId a.EvaluationPeriodId
        key, mapToDto a)
    |> Map.ofList

let apply
    (state: Map<InventoryHealthAssessmentId, InventoryHealthAssessmentDto>)
    (event: InventoryHealthEvent)
    : Map<InventoryHealthAssessmentId, InventoryHealthAssessmentDto> =
    match event with
    | InventoryHealthAssessmentPublished(assessment, _, _) ->
        let key =
            InventoryHealthAssessmentId.ofScopeAndPeriod assessment.AssessmentScopeId assessment.EvaluationPeriodId

        Map.add key (mapToDto assessment) state
