/// Projections for Supplier Commitment Assessment
module Medhavi.Supply.CollaborateWithSuppliers.SupplierCommitmentAssessment.Projections

open Medhavi.Contracts.Supply
open Medhavi.SemanticModel
open Model

let mapExposureToDto (e: DimensionalExposure) : DimensionalExposureDto =
    {
        DimensionName = e.DimensionName
        Level = e.Level.ToString()
        QuantityAtRisk = e.QuantityAtRisk
        ValuationAtRisk = e.ValuationAtRisk
        ReasonCodes = e.ReasonCodes
        ThreatenedObjectives = e.ThreatenedObjectives
    }

let mapAssessmentToDto (a: SupplierCommitmentAssessment) : SupplierCommitmentAssessmentDto =
    let lifecycleStr =
        match a.State with
        | AssessmentLifecycleState.Active -> "Active"
        | AssessmentLifecycleState.Archived -> "Archived"

    {
        SupplierId = SupplierId.value a.SupplierId
        LifecycleState = lifecycleStr
        AssessmentTimestamp = Timestamp.value a.AssessmentTimestamp
        OverallConfidence = a.OverallConfidence
        Exposures = a.Exposures |> List.map mapExposureToDto
        OnTimeDeliveryRate = a.OnTimeDeliveryRate
        QuantityFulfillmentRate = a.QuantityFulfillmentRate
        AverageAcknowledgementDurationHours = a.AverageAcknowledgementHours
        AverageCommitmentDurationHours = a.AverageCommitmentHours
        ActiveRiskSignals = a.ActiveRiskSignals
        AuthoritativeHealthState = a.AuthoritativeHealthState.ToString()
    }

let apply
    (state: Map<SupplierId, SupplierCommitmentAssessmentDto>)
    (event: SupplierCommitmentAssessmentEvent)
    : Map<SupplierId, SupplierCommitmentAssessmentDto> =
    match event with
    | SupplierCommitmentAssessmentChanged(assessment, _) ->
        Map.add assessment.SupplierId (mapAssessmentToDto assessment) state

let seedFromAggregates
    (aggregates: SupplierCommitmentAssessment list)
    : Map<SupplierId, SupplierCommitmentAssessmentDto> =
    aggregates |> List.map(fun a -> a.SupplierId, mapAssessmentToDto a) |> Map.ofList
