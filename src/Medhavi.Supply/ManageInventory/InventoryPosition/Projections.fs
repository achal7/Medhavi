/// SE-S-030 — Inventory Position Projections
module Medhavi.Supply.ManageInventory.InventoryPosition.Projections

open System
open Medhavi.Contracts.Supply
open Medhavi.SemanticModel
open Medhavi.Supply
open Model

let mapChangeEventToDto (ev: InventoryAssessmentChangeEvent) : InventoryAssessmentChangeEventDto =
    { SequenceNumber = ev.SequenceNumber
      ChangeTimestamp = Timestamp.value ev.ChangeTimestamp
      PreviousHealthStatus = ev.PreviousHealthStatus.ToString()
      NewHealthStatus = ev.NewHealthStatus.ToString()
      PreviousRiskLevel = ev.PreviousRiskLevel.ToString()
      NewRiskLevel = ev.NewRiskLevel.ToString()
      TriggeringEvidence = ev.TriggeringEvidence
      ReasonCode = ev.ReasonCode }

let mapToDto (assessment: InventoryPositionAssessment) : InventoryPositionAssessmentDto =
    { ItemId = ItemId.value assessment.Item
      LocationId = LocationId.value assessment.Location
      LifecycleState = sprintf "%A" assessment.LifecycleState
      OnHandQuantity = Quantity.value assessment.OnHandQuantity
      AllocatedQuantity = Quantity.value assessment.AllocatedQuantity
      InTransitQuantity = Quantity.value assessment.InTransitQuantity
      AvailableQuantity = Quantity.value assessment.AvailableQuantity
      DaysOfSupply = assessment.DaysOfSupply
      CoverageStatus = assessment.CoverageStatus.ToString()
      RiskLevel = assessment.RiskLevel.ToString()
      HealthStatus = assessment.HealthStatus.ToString()
      PolicyComplianceStatus = assessment.PolicyComplianceStatus.ToString()
      FinancialExposure = assessment.FinancialExposure
      SourceSupplyUnderstandingVersion = assessment.SourceSupplyUnderstandingVersion
      SourceSupplyPlanVersion = assessment.SourceSupplyPlanVersion
      AssessmentTimestamp = Timestamp.value assessment.AssessmentTimestamp
      ChangeHistory = assessment.ChangeHistory |> List.map mapChangeEventToDto }

let seedFromAggregates (aggregates: InventoryPositionAssessment list) : Map<ItemLocationId, InventoryPositionAssessmentDto> =
    aggregates
    |> List.map (fun a ->
        let key = ItemLocationId.ofItemAndLocation a.Item a.Location
        key, mapToDto a)
    |> Map.ofList

let apply (state: Map<ItemLocationId, InventoryPositionAssessmentDto>) (event: InventoryPositionEvent) : Map<ItemLocationId, InventoryPositionAssessmentDto> =
    match event with
    | InventoryPositionAssessmentChanged(assessment, _) ->
        let key = ItemLocationId.ofItemAndLocation assessment.Item assessment.Location
        Map.add key (mapToDto assessment) state
