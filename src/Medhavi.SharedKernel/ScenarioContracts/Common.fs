namespace Medhavi.SharedKernel.ScenarioContracts

open System
open System.Text.Json.Serialization
open Medhavi.SharedKernel

// =============================================================================
// Scenario Status — used by UI, Nexus, and Scenario BC for lifecycle tracking
// =============================================================================

[<JsonFSharpConverter>]
type ScenarioStatus =
    | Draft
    | Ready
    | PlanningRunning
    | PlanningComplete
    | PlanningFailed
    | PlanningPaused
    | UnderReview
    | Approved
    | Archived

// =============================================================================
// Time Bucket Granularity
// =============================================================================

/// Granularity of the planning time buckets used in a plan run.
[<JsonFSharpConverter>]
type TimeBucketGranularity =
    | Daily
    | Weekly
    | Monthly

// =============================================================================
// Plan Run Horizon (was: PlanningHorizon — renamed to avoid clash with
// Medhavi.Analytics.PlanningHorizon namespace)
// =============================================================================

/// Defines the time window and rules for a single planning run.
/// - StartDate / EndDate: when the plan run covers
/// - FirmHorizonDays: orders within this window are immutable (optimizer cannot touch)
/// - FrozenHorizonDays: orders can be adjusted but not cancelled
/// - Granularity: how time is bucketed during the run
type PlanRunHorizon =
    {
        StartDate         : DateTimeOffset
        EndDate           : DateTimeOffset
        FirmHorizonDays   : int
        FrozenHorizonDays : int
        Granularity       : TimeBucketGranularity
    }

// =============================================================================
// Input Version Vector — used by both Scenario and Planning BCs for dirty detection
// =============================================================================

/// Aggregate-level version vector captured at snapshot creation time.
/// Stored on PlanningSnapshot (reproducibility) and Scenario.LastKnownInputVersions
/// (dirty detection).
type InputVersionVector =
    {
        DemandAggregateVersion    : int
        InventoryAggregateVersion : int
        CapacityAggregateVersion  : int
        BomAggregateVersion       : int
        PolicyVersion             : int
        /// Version of the WhatIf overlay set (0 for Baseline scenarios).
        OverlaySetVersion         : int
    }

module InputVersionVector =
    let initial =
        { DemandAggregateVersion    = 0
          InventoryAggregateVersion = 0
          CapacityAggregateVersion  = 0
          BomAggregateVersion       = 0
          PolicyVersion             = 0
          OverlaySetVersion         = 0 }

// =============================================================================
// Planning Run Phase — shared by PlanningRun aggregate and Scenario status display
// =============================================================================

/// Pipeline execution phase — emitted as events for telemetry and checkpoint-based resume.
[<JsonFSharpConverter>]
type PlanningRunPhase =
    | Initializing
    | SnapshotLoading
    | ForecastNetting
    | BomExplosion
    | TimePhasedNetting
    | LotSizing
    | CapacityNetting
    | OrderGeneration
    | HeuristicPass
    | Postprocessing
    | Persisting
    | Publishing

// =============================================================================
// Objective Term — used by Scenario (to configure objectives) and Scheduler
// =============================================================================

/// Individual KPI objective terms usable by both Scenario and Planning BCs.
[<JsonFSharpConverter>]
type ObjectiveTerm =
    | ServiceLevel
    | OnTimeDelivery
    | InventoryCarrying
    | TotalCost
    | CO2Emissions
    | PlanChurn
    | AverageTardiness

// =============================================================================
// Plan Reference — cross-BC reference to a specific plan version
// =============================================================================

type PlanReference = { PlanId: Guid; Version: int }

// =============================================================================
// Planning Mode — canonical definition (consolidated from Scenario + Scheduler)
// Controls the scope and warm-start behaviour of a planning run.
// =============================================================================

/// Planning mode determines which solver strategy and scope to use for a run.
/// This is the SINGLE canonical definition — all BCs reference this type.
[<JsonFSharpConverter>]
type PlanningMode =
    /// Full horizon, all demands — used for nightly batch or after structural changes.
    | FullReplan
    /// Targeted repair on a specific subset of demands — fast, warm-started from prior plan.
    | ReactiveRepair of changedDemandIds: string list
    /// Insert a single new demand into an existing plan — neighbourhood solve only.
    | IncrementalInsert of demandId: string
    /// Full horizon but initialised from a prior plan version — faster convergence.
    | WarmStart of baseRunId: Guid

// =============================================================================
// Scenario Data Override — what-if delta mechanism
// Shared between Scenario BC (stores overlays) and Scheduler (applies during planning).
// =============================================================================

/// A single what-if override to apply on top of live BC data.
/// Only the changed value is stored — not the full data set (delta pattern).
[<JsonFSharpConverter>]
type ScenarioDataOverride =
    /// Override the quantity of a specific demand record.
    | DemandOverride of demandId: string * overrideQty: decimal * reason: string
    /// Override on-hand inventory for a specific SKU + stocking point.
    | InventoryOverride of skuId: string * stockingPointId: string * overrideQty: decimal
    /// Override the lead time (in days) for a specific SKU.
    | LeadTimeOverride of skuId: string * overrideDays: int * reason: string
    /// Add or adjust available capacity for a specific resource + time bucket.
    | CapacityOverride of resourceId: string * bucketKey: DateTimeOffset * extraQty: decimal
    /// Mark a supplier as unavailable during a specific period.
    | SupplierSuspension of supplierId: string * duringBucketKey: string
    /// Override the quantity-per relationship between BOM parent and component.
    | BomOverride of parentProduct: string * componentProduct: string * overrideQtyPer: decimal

module ScenarioDataOverride =
    open System.Text
    open System.Security.Cryptography

    /// Compute a deterministic content hash for identity (used by RemoveOverride).
    /// Using SHA-256 of the string representation avoids fragile index-based removal.
    let contentHash (override_: ScenarioDataOverride) : string =
        let input = sprintf "%A" override_
        let bytes = Encoding.UTF8.GetBytes(input)
        use sha = SHA256.Create()
        Convert.ToHexString(sha.ComputeHash(bytes)).ToLowerInvariant()

// =============================================================================
// Plan KPI Summary — lightweight KPI snapshot for cross-BC display
// Stored on Scenario aggregate and PlanVersion events. Never bulk data.
// =============================================================================

/// Condensed KPI summary enabling the UI to show service level, cost, etc.
/// without loading the full PlanVersion bulk data from the store.
type PlanKpiSummary =
    { ServiceLevel: float             // 0.0 – 1.0 (ratio of demand met on-time)
      OnTimeDelivery: float           // 0.0 – 1.0
      InventoryCarryingCost: decimal  // monetary units
      TotalCost: decimal              // monetary units
      CO2Emissions: decimal           // kg CO2-equivalent
      PlanChurn: float                // ratio of planned orders changed vs. prior
      AverageTardiness: float         // average days late per demand
      ObjectiveValue: decimal         // weighted composite objective function value
      HardConstraintViolations: int   // count; 0 = fully feasible
      SoftConstraintViolations: int   // count
      PlanHorizonDays: int            // calendar days covered
      PlannedOrderCount: int
      ShortageCount: int }

module PlanKpiSummary =
    let empty =
        { ServiceLevel = 0.0
          OnTimeDelivery = 0.0
          InventoryCarryingCost = 0m
          TotalCost = 0m
          CO2Emissions = 0m
          PlanChurn = 0.0
          AverageTardiness = 0.0
          ObjectiveValue = 0m
          HardConstraintViolations = 0
          SoftConstraintViolations = 0
          PlanHorizonDays = 0
          PlannedOrderCount = 0
          ShortageCount = 0 }

// =============================================================================
// Plan Ref — lightweight pointer stored on Scenario aggregate
// Enables scenario-level plan display without loading PlanVersion bulk data.
// =============================================================================

/// A lightweight pointer to a PlanVersion. Stored on the Scenario aggregate.
type PlanRef =
    { PlanVersionId: string          // SHA-256 hash content address
      StorageKey: string             // Key for bulk data retrieval
      GeneratedAt: DateTimeOffset
      KpiSummary: PlanKpiSummary option }

// =============================================================================
// Dirty Reason — why a scenario needs replanning
// Enables automatic planning mode derivation (ScenarioPolicy.determinePlanningMode).
// =============================================================================

/// Records what changed and carries entity IDs so the orchestrator can
/// automatically select the narrowest planning mode and solve scope.
[<JsonFSharpConverter>]
type DirtyReason =
    /// Some demand records changed — carry their IDs to scope a ReactiveRepair.
    | DemandDataChanged of fromVersion: int * toVersion: int * changedDemandIds: string list
    /// Some capacity records changed — carry resource IDs.
    | CapacityDataChanged of fromVersion: int * toVersion: int * changedResourceIds: string list
    /// Inventory snapshot refreshed — triggers a ReactiveRepair over all demands.
    | InventoryDataChanged of fromVersion: int * toVersion: int
    /// BOM or routing changed — structural; always triggers FullReplan.
    | BomOrRoutingChanged of fromVersion: int * toVersion: int
    /// Planning policy (objectives, constraints, horizon) changed — always FullReplan.
    | PolicyChanged of fromVersion: int * toVersion: int
    /// WhatIf overlay set changed — triggers ReactiveRepair.
    | OverlayChanged of fromVersion: int * toVersion: int
    /// Planner manually edited something (ad-hoc change).
    | ManualPlannerChange of note: string

// =============================================================================
// Scenario Classification — shared because both BCs need to branch on type
// =============================================================================

/// Classifies the intended purpose of a scenario.
[<JsonFSharpConverter>]
type ScenarioType =
    /// The single authoritative baseline for operational execution.
    | Baseline
    /// A what-if branch exploring policy or demand changes.
    | WhatIf
    /// A throwaway workspace for ad-hoc analysis (never promoted).
    | Sandbox

// =============================================================================
// Scenario Metadata — extensible tags for analytics, ML, and governance
// =============================================================================

/// Key-value metadata attached to scenarios for analytics, ML labeling,
/// search/filter, and governance workflows. Extensible by design.
type ScenarioTag = { Key: string; Value: string }

/// Metadata record for AI/ML and analytics integration.
type ScenarioMetadata =
    { Tags: ScenarioTag list
      Description: string option
      /// Template ID this scenario was created from (for scenario catalog).
      TemplateId: string option
      /// Owner/creator for governance and access control.
      OwnerId: string option
      /// Custom KPIs or solver hints from ML models.
      CustomAttributes: Map<string, string> }

module ScenarioMetadata =
    let empty =
        { Tags = []
          Description = None
          TemplateId = None
          OwnerId = None
          CustomAttributes = Map.empty }

type ScenarioReadModel =
    { ScenarioId: string
      Name: string
      BaseScenarioId: string option
      Version: int
      CreatedAt: DateTimeOffset
      IsActive: bool
      Overrides: ScenarioDataOverride list }
