/// SE-S-041 — Capacity Health Projections
/// Maps pure domain models to Medhavi.Contracts.Supply DTOs and provides projection reducers
module Medhavi.Supply.ManageCapacity.CapacityHealth.Projections

open System
open Medhavi.Contracts.Supply
open Medhavi.SemanticModel
open Medhavi.Supply
open Medhavi.Supply.ManageCapacity.CapacityHealth
open Model

let mapHealthState (state: CapacityHealthState) : Medhavi.Contracts.Supply.CapacityHealthState =
    match state with
    | CapacityHealthState.CriticallyConstrained -> Medhavi.Contracts.Supply.CapacityHealthState.CriticallyConstrained
    | CapacityHealthState.Constrained -> Medhavi.Contracts.Supply.CapacityHealthState.Constrained
    | CapacityHealthState.AtRisk -> Medhavi.Contracts.Supply.CapacityHealthState.AtRisk
    | CapacityHealthState.Underutilized -> Medhavi.Contracts.Supply.CapacityHealthState.Underutilized
    | CapacityHealthState.Balanced -> Medhavi.Contracts.Supply.CapacityHealthState.Balanced
    | CapacityHealthState.Unassessed -> Medhavi.Contracts.Supply.CapacityHealthState.Unassessed

let mapVersionToDto (v: CapacityHealthAssessmentVersion) : CapacityHealthAssessmentVersionDto =
    let dto: CapacityHealthAssessmentVersionDto =
        { VersionNumber = v.VersionNumber
          LifecycleState = sprintf "%A" v.State
          AssessmentScopeId = PlanningScopeId.value v.AssessmentScopeId
          EvaluationPeriodId = EvaluationPeriodId.value v.EvaluationPeriodId
          OverallCapacityHealthState = mapHealthState v.OverallCapacityHealthState
          ClassificationRationale = v.ClassificationRationale
          AssessmentConfidence = sprintf "%A" v.AssessmentConfidence
          AverageUtilizationPercentage = v.AverageUtilizationPercentage
          PeakUtilizationPercentage = v.PeakUtilizationPercentage
          UnderutilizedResourceCount = v.UnderutilizedResourceCount
          OverloadedResourceCount = v.OverloadedResourceCount
          CapacityStability = v.CapacityStability
          CapacityVolatility = v.CapacityVolatility
          BindingConstraints = v.BindingConstraints |> List.map PhysicalResourceId.value
          BottleneckResources = v.BottleneckResources |> List.map PhysicalResourceId.value
          OverloadRiskExposureHours = v.OverloadRiskExposureHours
          UnderloadCostExposureHours = v.UnderloadCostExposureHours
          BottleneckImpactSummary = v.BottleneckImpactSummary
          ChronicOverloadResources = v.ChronicOverloadResources |> List.map PhysicalResourceId.value
          ChronicUnderutilizedResources = v.ChronicUnderutilizedResources |> List.map PhysicalResourceId.value
          RecurringBottlenecks = v.RecurringBottlenecks |> List.map PhysicalResourceId.value
          LoadBalancingOpportunities = v.LoadBalancingOpportunities
          FlexibilityImprovementOpportunities = v.FlexibilityImprovementOpportunities
          EvidenceReferences = v.EvidenceReferences
          TransactionTimestamp = Timestamp.value v.TransactionTimestamp
          PublicationTimestamp = v.PublicationTimestamp |> Option.map Timestamp.value }
    dto

let mapToDto (a: CapacityHealthAssessment) : CapacityHealthAssessmentDto =
    let currentPublished =
        a.Versions
        |> List.tryFind (fun v -> v.State = HealthVersionLifecycleState.Published)
        |> Option.map mapVersionToDto

    let dto: CapacityHealthAssessmentDto =
        { AssessmentScopeId = PlanningScopeId.value a.AssessmentScopeId
          EvaluationPeriodId = EvaluationPeriodId.value a.EvaluationPeriodId
          CurrentVersion = currentPublished
          AllVersions = a.Versions |> List.map mapVersionToDto }
    dto

let seedFromAggregates (aggregates: CapacityHealthAssessment list) : Map<CapacityHealthAssessmentId, CapacityHealthAssessmentDto> =
    aggregates
    |> List.map (fun a ->
        let key = CapacityHealthAssessmentId.ofScopeAndPeriod a.AssessmentScopeId a.EvaluationPeriodId
        key, mapToDto a)
    |> Map.ofList

let apply (state: Map<CapacityHealthAssessmentId, CapacityHealthAssessmentDto>) (event: CapacityHealthEvent) : Map<CapacityHealthAssessmentId, CapacityHealthAssessmentDto> =
    match event with
    | CapacityHealthAssessmentPublished(assessment, _, _) ->
        let key = CapacityHealthAssessmentId.ofScopeAndPeriod assessment.AssessmentScopeId assessment.EvaluationPeriodId
        Map.add key (mapToDto assessment) state
