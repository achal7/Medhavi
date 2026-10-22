/// SE-S-030 — Inventory Position Behaviors
/// Traces to: AB-S-020 (Update Inventory Position Assessment)
module Medhavi.Supply.ManageInventory.InventoryPosition.Behaviors

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.SemanticModel
open Medhavi.Supply
open Medhavi.Supply.ManageInventory
open Model
open Rules
open Decisions

type PositionUpdateOutcome =
    | PositionUpdated of health: string * risk: string
    | PositionUnchanged

let update
    (inputProvider: ItemId -> LocationId -> PositionDeterminationInput)
    (evidenceProvider: ItemId -> LocationId -> int option * int option)
    : Decide<InventoryPositionAssessment, UpdateInventoryPositionCmd, InventoryPositionEvent> =
    fun (cmd: UpdateInventoryPositionCmd) (stateOpt: InventoryPositionAssessment option) ->
        result {
            do! validateAggregateIdentity cmd.Item cmd.Location
            do! validateUpdateEligibility stateOpt

            let currentAssessment =
                match stateOpt with
                | Some existing -> existing
                | None ->
                    { Item = cmd.Item
                      Location = cmd.Location
                      LifecycleState = PositionLifecycleState.Active
                      OnHandQuantity = Quantity.zero
                      AllocatedQuantity = Quantity.zero
                      InTransitQuantity = Quantity.zero
                      AvailableQuantity = Quantity.zero
                      DaysOfSupply = 0m
                      CoverageStatus = InventoryCoverageStatus.Optimal
                      RiskLevel = InventoryRiskLevel.Low
                      HealthStatus = InventoryHealthStatus.Optimal
                      PolicyComplianceStatus = PolicyComplianceStatus.Conformant
                      FinancialExposure = None
                      SourceSupplyUnderstandingVersion = None
                      SourceSupplyPlanVersion = None
                      AssessmentTimestamp = cmd.Timestamp
                      ChangeHistory = [] }

            let input = inputProvider cmd.Item cmd.Location
            let res = determineInventoryPositionStatus input
            let suVerOpt, spVerOpt = evidenceProvider cmd.Item cmd.Location

            let isMaterialChange =
                match stateOpt with
                | None -> true
                | Some existing ->
                    existing.HealthStatus <> res.HealthStatus
                    || existing.RiskLevel <> res.RiskLevel
                    || existing.PolicyComplianceStatus <> res.PolicyComplianceStatus
                    || existing.CoverageStatus <> res.CoverageStatus

            let changeEventOpt =
                if isMaterialChange then
                    let nextSeq = getNextSequenceNumber currentAssessment.ChangeHistory
                    Some { SequenceNumber = nextSeq
                           ChangeTimestamp = cmd.Timestamp
                           PreviousHealthStatus = currentAssessment.HealthStatus
                           NewHealthStatus = res.HealthStatus
                           PreviousRiskLevel = currentAssessment.RiskLevel
                           NewRiskLevel = res.RiskLevel
                           TriggeringEvidence = cmd.TriggeringEvidence
                           ReasonCode = res.ReasonCode }
                else
                    None

            let updatedHistory =
                match changeEventOpt with
                | Some ev -> ev :: currentAssessment.ChangeHistory
                | None -> currentAssessment.ChangeHistory

            let onHandQty = Quantity.create input.OnHand |> Result.defaultWith (fun _ -> Quantity.zero)
            let allocQty = Quantity.create input.Allocated |> Result.defaultWith (fun _ -> Quantity.zero)
            let inTransitQty = Quantity.create input.InTransit |> Result.defaultWith (fun _ -> Quantity.zero)
            let availQty = Quantity.create res.Available |> Result.defaultWith (fun _ -> Quantity.zero)

            let updatedAssessment =
                { currentAssessment with
                    OnHandQuantity = onHandQty
                    AllocatedQuantity = allocQty
                    InTransitQuantity = inTransitQty
                    AvailableQuantity = availQty
                    DaysOfSupply = res.DaysOfSupply
                    CoverageStatus = res.CoverageStatus
                    RiskLevel = res.RiskLevel
                    HealthStatus = res.HealthStatus
                    PolicyComplianceStatus = res.PolicyComplianceStatus
                    FinancialExposure = res.FinancialExposure
                    SourceSupplyUnderstandingVersion = suVerOpt
                    SourceSupplyPlanVersion = spVerOpt
                    AssessmentTimestamp = cmd.Timestamp
                    ChangeHistory = updatedHistory }

            let events = [ InventoryPositionAssessmentChanged(updatedAssessment, changeEventOpt) ]

            let outcome =
                if isMaterialChange then
                    PositionUpdated(string res.HealthStatus, string res.RiskLevel)
                else
                    PositionUnchanged

            let traceOutcome = { Outcome = outcome; Evaluations = [] }

            return
                buildDecisionWithTrace
                    evolve
                    stateOpt
                    events
                    ArsIdentifiers.Decisions.determineInventoryPositionStatus.Id
                    []
                    ArsIdentifiers.Capabilities.manageInventory.Id
                    traceOutcome
                    (Some ArsIdentifiers.Policies.inventoryHealthPolicy.Id)
                    (Some 1)
                    [ ArsIdentifiers.SemanticObjects.inventoryPositionAssessment.Id ]
                    (Some $"Inventory position updated: Health = {res.HealthStatus}, Risk = {res.RiskLevel}")
        }
