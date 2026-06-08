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
