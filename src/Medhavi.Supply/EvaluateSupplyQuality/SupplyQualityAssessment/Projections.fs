/// SE-S-100 — Supply Quality Assessment Projections
module Medhavi.Supply.EvaluateSupplyQuality.SupplyQualityAssessment.Projections

open Medhavi.SemanticModel
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Model

let mapToDto (sqa: SupplyQualityAssessment) : SupplyQualityAssessmentDto option =
    ACL.mapAggregateToDto sqa

let apply
    (state: Map<SupplyQualityAssessmentId, SupplyQualityAssessmentDto>)
    (event: SupplyQualityAssessmentEvent) : Map<SupplyQualityAssessmentId, SupplyQualityAssessmentDto> =
    match event with
    | SupplyQualityAssessmentPublished(sqa, _, _, _) ->
        match mapToDto sqa with
        | Some dto -> Map.add sqa.AssessmentId dto state
        | None -> state
    | SupplyQualityAssessmentDraftCreated(sqa, _) ->
        match mapToDto sqa with
        | Some dto -> Map.add sqa.AssessmentId dto state
        | None -> state

let seedFromAggregates
    (aggregates: SupplyQualityAssessment list) : Map<SupplyQualityAssessmentId, SupplyQualityAssessmentDto> =
    aggregates
    |> List.choose (fun sqa -> mapToDto sqa |> Option.map (fun dto -> sqa.AssessmentId, dto))
    |> Map.ofList
