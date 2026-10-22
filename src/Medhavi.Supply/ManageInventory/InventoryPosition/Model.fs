/// SE-S-030 — Inventory Position Assessment Domain Model
module Medhavi.Supply.ManageInventory.InventoryPosition.Model

open Medhavi.SemanticModel
open Medhavi.Supply.ManageInventory

type PositionLifecycleState =
    | Active
    | Archived

/// Child entity: immutable change event per ARS Continuous Assessment pattern
type InventoryAssessmentChangeEvent =
    { SequenceNumber: int
      ChangeTimestamp: Timestamp
      PreviousHealthStatus: InventoryHealthStatus
      NewHealthStatus: InventoryHealthStatus
      PreviousRiskLevel: InventoryRiskLevel
      NewRiskLevel: InventoryRiskLevel
      TriggeringEvidence: string list
      ReasonCode: string }

/// SE-S-030 Aggregate Root
type InventoryPositionAssessment =
    { Item: ItemId
      Location: LocationId
      LifecycleState: PositionLifecycleState
      OnHandQuantity: Quantity
      AllocatedQuantity: Quantity
      InTransitQuantity: Quantity
      AvailableQuantity: Quantity
      DaysOfSupply: decimal
      CoverageStatus: InventoryCoverageStatus
      RiskLevel: InventoryRiskLevel
      HealthStatus: InventoryHealthStatus
      PolicyComplianceStatus: PolicyComplianceStatus
      FinancialExposure: decimal option
      SourceSupplyUnderstandingVersion: int option
      SourceSupplyPlanVersion: int option
      AssessmentTimestamp: Timestamp
      ChangeHistory: InventoryAssessmentChangeEvent list }

type UpdateInventoryPositionCmd =
    { Item: ItemId
      Location: LocationId
      TriggeringEvidence: string list
      Timestamp: Timestamp }

/// EV-S-020: Inventory Position Assessment Changed
type InventoryPositionEvent =
    | InventoryPositionAssessmentChanged of assessment: InventoryPositionAssessment * changeEventOpt: InventoryAssessmentChangeEvent option

let evolve (stateOpt: InventoryPositionAssessment option) (event: InventoryPositionEvent) : InventoryPositionAssessment option =
    match event with
    | InventoryPositionAssessmentChanged(assessment, _) -> Some assessment

let replay (events: InventoryPositionEvent seq) : InventoryPositionAssessment option =
    Seq.fold evolve None events
