/// Projections for Supplier Collaboration Health Assessment Aggregate
module Medhavi.Supply.CollaborateWithSuppliers.SupplierCollaborationHealth.Projections

open Medhavi.Contracts.Supply
open Medhavi.SemanticModel
open Medhavi.Supply
open Model

let mapVersionToDto (v: SupplierCollaborationHealthVersion) : SupplierCollaborationHealthAssessmentDto =
    let versionId =
        $"{PlanningScopeId.value v.AssessmentScopeId}#{EvaluationPeriodId.value v.EvaluationPeriodId}#v{v.VersionNumber}"

    let lifecycleStr =
        match v.State with
        | HealthVersionLifecycleState.Draft -> "Draft"
        | HealthVersionLifecycleState.Published -> "Published"
        | HealthVersionLifecycleState.Superseded -> "Superseded"

    let classifications = v.SupplierClassifications |> List.map(fun (sid, c) -> (SupplierId.value sid, c.ToString()))

    let respTrends = v.ResponsivenessTrends |> List.map(fun (sid, t) -> (SupplierId.value sid, t))

    let accTrends = v.CommitmentAccuracyTrends |> List.map(fun (sid, t) -> (SupplierId.value sid, t))

    {
        AssessmentScopeId = PlanningScopeId.value v.AssessmentScopeId
        EvaluationPeriod = EvaluationPeriodId.value v.EvaluationPeriodId
        AssessmentVersionId = versionId
        VersionNumber = v.VersionNumber
        LifecycleState = lifecycleStr
        PublicationTimestamp = v.PublicationTimestamp |> Option.defaultValue v.TransactionTimestamp |> Timestamp.value
        OverallPublicationOutcome = v.OverallPublicationOutcome.ToString()
        SupplierClassifications = classifications
        ResponsivenessTrends = respTrends
        CommitmentAccuracyTrends = accTrends
        RecommendationSignals = v.RecommendationSignals
    }

let apply
    (state: Map<SupplierCollaborationHealthAssessmentId, SupplierCollaborationHealthAssessmentDto>)
    (event: SupplierCollaborationHealthEvent)
    : Map<SupplierCollaborationHealthAssessmentId, SupplierCollaborationHealthAssessmentDto> =
    match event with
    | SupplierCollaborationHealthAssessmentPublished(assessment, publishedVer, _) ->
        match assessment.Versions |> List.tryFind(fun v -> v.VersionNumber = publishedVer) with
        | Some v ->
            let id =
                SupplierCollaborationHealthAssessmentId.ofScopeAndPeriod
                    assessment.AssessmentScopeId
                    assessment.EvaluationPeriodId

            Map.add id (mapVersionToDto v) state
        | None -> state

let seedFromAggregates
    (aggregates: SupplierCollaborationHealthAssessment list)
    : Map<SupplierCollaborationHealthAssessmentId, SupplierCollaborationHealthAssessmentDto> =
    aggregates
    |> List.choose(fun a ->
        a.Versions
        |> List.filter(fun v -> v.State = HealthVersionLifecycleState.Published)
        |> List.sortByDescending(fun v -> v.VersionNumber)
        |> List.tryHead
        |> Option.map(fun v ->
            let id = SupplierCollaborationHealthAssessmentId.ofScopeAndPeriod a.AssessmentScopeId a.EvaluationPeriodId
            id, mapVersionToDto v))
    |> Map.ofList
