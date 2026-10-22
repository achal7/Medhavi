/// SE-S-030 — Inventory Position Decisions
/// Traces to: DE-S-021 (Determine Inventory Position Status)
module Medhavi.Supply.ManageInventory.InventoryPosition.Decisions

open Medhavi.Supply.ManageInventory

type PositionDeterminationInput =
    { OnHand: decimal
      Allocated: decimal
      InTransit: decimal
      SafetyStock: decimal
      ReorderPoint: decimal
      TargetStockLevel: decimal
      AverageDemand: decimal
      LeadTimeDays: int
      UnitCost: decimal
      ShelfLifeDays: int option
      RemainingLifetimeDemand: decimal option
      IsSingleSource: bool
      SupplierReliability: decimal }

type PositionDeterminationResult =
    { Available: decimal
      DaysOfSupply: decimal
      CoverageStatus: InventoryCoverageStatus
      RiskLevel: InventoryRiskLevel
      HealthStatus: InventoryHealthStatus
      PolicyComplianceStatus: PolicyComplianceStatus
      FinancialExposure: decimal option
      ReasonCode: string }

/// DE-S-021: Determine Inventory Position Status
let determineInventoryPositionStatus (input: PositionDeterminationInput) : PositionDeterminationResult =
    let available = input.OnHand + input.InTransit - input.Allocated

    let daysOfSupply =
        if input.AverageDemand > 0m then
            max 0m (available / input.AverageDemand)
        else
            999m

    // 1. Coverage Status
    let coverageStatus =
        if available < input.ReorderPoint then InventoryCoverageStatus.Understocked
        elif available > input.TargetStockLevel then InventoryCoverageStatus.Overstocked
        else InventoryCoverageStatus.Optimal

    // 2. Policy Compliance Status
    let policyCompliance =
        if available < input.SafetyStock then PolicyComplianceStatus.BelowSafetyStock
        elif available > (input.TargetStockLevel * 1.20m) then PolicyComplianceStatus.PolicyBandBreach
        else PolicyComplianceStatus.Conformant

    // 3. Multi-dimensional criteria evaluation with strict policy precedence
    // Precedence 1: Obsolete
    let isObsoleteByDemand =
        match input.RemainingLifetimeDemand with
        | Some remDem when input.OnHand > remDem -> true
        | _ -> false

    let isObsoleteByShelfLife =
        match input.ShelfLifeDays with
        | Some shelf when shelf < input.LeadTimeDays -> true
        | _ -> false

    let isObsolete = isObsoleteByDemand || isObsoleteByShelfLife

    // Precedence 2: Under-Stocked
    let isUnderStocked = available < input.SafetyStock || available < input.ReorderPoint

    // Precedence 3: At Risk
    let isAtRisk =
        (input.IsSingleSource && input.SupplierReliability < 0.90m)
        || (available >= input.ReorderPoint && daysOfSupply < decimal input.LeadTimeDays)

    // Precedence 4: Over-Stocked
    let isOverStocked = available > (input.TargetStockLevel * 1.10m)

    let healthStatus, reasonCode =
        if isObsolete then
            let code = if isObsoleteByShelfLife then "NearShelfLifeExpiry" else "DecliningLifecycleDemand"
            InventoryHealthStatus.Obsolete, code
        elif isUnderStocked then
            let code = if available < input.SafetyStock then "BelowSafetyStock" else "BelowReorderPoint"
            InventoryHealthStatus.UnderStocked, code
        elif isAtRisk then
            let code = if input.IsSingleSource then "SingleSourceDependency" else "ProjectedStockoutWithinLeadTime"
            InventoryHealthStatus.AtRisk, code
        elif isOverStocked then
            InventoryHealthStatus.OverStocked, "ExcessAboveTargetStock"
        else
            InventoryHealthStatus.Optimal, "OptimalCoverage"

    // 4. Risk Level
    let riskLevel =
        match healthStatus with
        | InventoryHealthStatus.UnderStocked when available <= 0m -> InventoryRiskLevel.Critical
        | InventoryHealthStatus.UnderStocked -> InventoryRiskLevel.High
        | InventoryHealthStatus.Obsolete -> InventoryRiskLevel.High
        | InventoryHealthStatus.AtRisk -> InventoryRiskLevel.Medium
        | InventoryHealthStatus.OverStocked -> InventoryRiskLevel.Medium
        | InventoryHealthStatus.Optimal -> InventoryRiskLevel.Low
        | InventoryHealthStatus.Unassessed -> InventoryRiskLevel.VeryLow

    // 5. Financial Exposure
    let financialExposure =
        match healthStatus with
        | InventoryHealthStatus.OverStocked ->
            let excessQty = max 0m (available - input.TargetStockLevel)
            Some (excessQty * input.UnitCost)
        | InventoryHealthStatus.Obsolete ->
            Some (input.OnHand * input.UnitCost)
        | InventoryHealthStatus.UnderStocked ->
            let deficitQty = max 0m (input.SafetyStock - available)
            Some (deficitQty * input.UnitCost)
        | _ ->
            None

    { Available = available
      DaysOfSupply = daysOfSupply
      CoverageStatus = coverageStatus
      RiskLevel = riskLevel
      HealthStatus = healthStatus
      PolicyComplianceStatus = policyCompliance
      FinancialExposure = financialExposure
      ReasonCode = reasonCode }
