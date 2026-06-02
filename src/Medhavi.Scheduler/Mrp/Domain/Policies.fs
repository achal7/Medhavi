/// MRP Policies — Immutable configuration for MRP behaviour
/// FP/DDD Pattern: Algebraic data types for policy expression
module Medhavi.Planning.Mrp.Domain.Policies

open System
open System.Text.Json.Serialization
open Medhavi.SharedKernel

// ============================================================================
// LOT SIZING POLICY (Phase 9.3)
// ============================================================================

/// Lot sizing algorithm selection
[<JsonFSharpConverter>]
type LotSizingPolicy =
    | LotForLot                                                      // Order exactly what's needed
    | FixedLot of lotSize: Quantity                                   // Fixed order quantity
    | MinimumLot of minQty: Quantity                                  // At least this much per order
    | EOQ of annualDemand: decimal * orderingCost: decimal * holdingCost: decimal // Economic Order Quantity
    | SilverMeal of orderingCost: decimal * holdingCost: decimal      // Minimize cost per period
    | PeriodOrderQuantity of periods: int                             // Combine N periods of demand
    | RoundingLot of lotSize: Quantity * roundUp: bool                // Round to lot size multiples

// ============================================================================
// NETTING POLICY (Phase 9.2)
// ============================================================================

/// Netting policy configuration
type NettingPolicy =
    { SafetyStockQty: Quantity       // Static safety stock override
      MinOrderQty: Quantity option   // Minimum order quantity
      MaxOrderQty: Quantity option   // Maximum order quantity
      CoverDays: int option          // Number of days of demand to cover
      LotSizing: LotSizingPolicy option }

module NettingPolicy =
    let defaults =
        { SafetyStockQty = Quantity.Zero
          MinOrderQty = None
          MaxOrderQty = None
          CoverDays = None
          LotSizing = None }

// ============================================================================
// BOM SELECTION POLICY (Phase 9.1)
// ============================================================================

/// BOM selection strategy when multiple BOMs exist for a SKU
[<JsonFSharpConverter>]
type BomSelectionPolicy =
    | DefaultBom           // Use first active BOM found
    | PrimaryPreferred     // Use primary BOM, fallback to alternates
    | AvailabilityBased    // Choose BOM where components are most available
    | CostBased            // Choose lowest-cost BOM

// ============================================================================
// CAPACITY POLICY (Phase 9.8)
// ============================================================================

/// Capacity checking policy for finite capacity MRP
type CapacityPolicy =
    { Finite: bool                           // Enable finite capacity checking
      SafetyBuffer: TimeSpan option          // Buffer added to capacity requirements
      ReliabilityFactor: float option        // Factor for capacity utilization (0.0–1.0)
      MaxAlternateAttempts: int              // How many alternate routings to try on failure
      BottleneckProtection: TimeSpan option } // Extra buffer for known bottleneck resources

module CapacityPolicy =
    let infiniteCapacity =
        { Finite = false
          SafetyBuffer = None
          ReliabilityFactor = None
          MaxAlternateAttempts = 0
          BottleneckProtection = None }

    let finiteCapacity =
        { Finite = true
          SafetyBuffer = Some (TimeSpan.FromHours 1.0)
          ReliabilityFactor = Some 0.85
          MaxAlternateAttempts = 2
          BottleneckProtection = None }

// ============================================================================
// EXPEDITE POLICY
// ============================================================================

/// Expedite decision policy
[<JsonFSharpConverter>]
type ExpeditePolicy =
    | AlwaysExpedite
    | NeverExpedite
    | ExpediteIfUrgent                     // Expedite if priority <= threshold
    | ExpediteIfShortLeadTime of days: int // Expedite if within N days of due date

// ============================================================================
// FORECAST CONSUMPTION POLICY (Phase 9.5)
// ============================================================================

/// Forecast consumption strategy
[<JsonFSharpConverter>]
type ForecastConsumptionStrategy =
    | BackwardConsumption       // Consume forecast before order date
    | ForwardConsumption        // Consume forecast after order date
    | BidirectionalConsumption  // Both directions within window

/// Forecast consumption policy
type ForecastConsumptionPolicy =
    { Enabled: bool
      ConsumptionWindow: TimeSpan   // How far to look for matching forecasts
      Strategy: ForecastConsumptionStrategy }

module ForecastConsumptionPolicy =
    let disabled =
        { Enabled = false
          ConsumptionWindow = TimeSpan.Zero
          Strategy = BackwardConsumption }

    let defaultEnabled =
        { Enabled = true
          ConsumptionWindow = TimeSpan.FromDays 7.0
          Strategy = BidirectionalConsumption }

// ============================================================================
// FROZEN HORIZON POLICY
// ============================================================================

/// Frozen horizon policy — controls change protection
type FrozenHorizonPolicy =
    { FrozenDays: int    // Orders within this period cannot be changed
      SlushyDays: int    // Orders within this period require approval
      FreeZone: bool }   // Allow changes freely after frozen/slushy period

/// Horizon zone classification
[<JsonFSharpConverter>]
type HorizonZone =
    | Frozen    // Cannot change
    | Slushy    // Can change with approval
    | Free      // Can change freely

module FrozenHorizon =
    /// Determine which zone an order date falls into
    let getZone (policy: FrozenHorizonPolicy) (currentDate: Timestamp) (orderDate: Timestamp) : HorizonZone =
        let current = Timestamp.value currentDate
        let order = Timestamp.value orderDate
        let daysFromNow = (order - current).Days
        if daysFromNow <= policy.FrozenDays then Frozen
        elif daysFromNow <= (policy.FrozenDays + policy.SlushyDays) then Slushy
        else Free

    /// Check if order can be modified at all
    let canModify (policy: FrozenHorizonPolicy) (currentDate: Timestamp) (orderDate: Timestamp) : bool =
        match getZone policy currentDate orderDate with
        | Frozen -> false
        | Slushy -> true
        | Free -> true

    /// Check if modification requires approval
    let requiresApproval (policy: FrozenHorizonPolicy) (currentDate: Timestamp) (orderDate: Timestamp) : bool =
        match getZone policy currentDate orderDate with
        | Frozen -> true
        | Slushy -> true
        | Free -> false

// ============================================================================
// FIRMING POLICY
// ============================================================================

/// Auto-firming policy
type FirmingPolicy =
    { AutoFirmDays: int             // Auto-firm proposals within this window
      RequireConfirmation: bool }   // Require human confirmation outside auto-firm

module Firming =
    /// Check if a proposal should be auto-firmed
    let shouldAutoFirm (policy: FirmingPolicy) (currentDate: Timestamp) (proposalDueDate: Timestamp) : bool =
        let current = Timestamp.value currentDate
        let dueDate = Timestamp.value proposalDueDate
        let daysFromNow = (dueDate - current).Days
        daysFromNow <= policy.AutoFirmDays

// ============================================================================
// TIME BUCKET POLICY (Phase 9.7)
// ============================================================================

/// Time bucket granularity for netting reports
[<JsonFSharpConverter>]
type TimeBucketGranularity =
    | Daily
    | Weekly
    | Monthly

// ============================================================================
// COMPOSITE MRP POLICY
// ============================================================================

/// Top-level MRP policy — aggregates all sub-policies
type MrpPolicy =
    { NettingPolicy: NettingPolicy
      LotSizingPolicy: LotSizingPolicy option
      BomSelectionPolicy: BomSelectionPolicy
      ExpeditePolicy: ExpeditePolicy
      CapacityPolicy: CapacityPolicy
      ForecastConsumption: ForecastConsumptionPolicy option
      FrozenHorizon: FrozenHorizonPolicy option
      Firming: FirmingPolicy option
      TimeBucketGranularity: TimeBucketGranularity }

module MrpPolicy =
    /// Default MRP policy — infinite capacity, lot-for-lot, no forecast consumption
    let defaults =
        { NettingPolicy = NettingPolicy.defaults
          LotSizingPolicy = None
          BomSelectionPolicy = DefaultBom
          ExpeditePolicy = NeverExpedite
          CapacityPolicy = CapacityPolicy.infiniteCapacity
          ForecastConsumption = None
          FrozenHorizon = None
          Firming = None
          TimeBucketGranularity = Daily }
