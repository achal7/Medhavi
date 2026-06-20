namespace Medhavi.Scheduler.Planning.Domain

open System
open Medhavi.Contracts.Scenario
open Medhavi.SharedKernel
open Medhavi.Scheduler.Mrp.Domain

type PlannedOrderType =
    | PlannedProduction
    | PlannedPurchase
    | PlannedTransfer

type HorizonZone =
    | Firm
    | Frozen
    | Free

/// A single planned supply order produced by the pipeline.
type PlannedOrder =
    {
        /// Deterministic content-addressed ID.
        OrderId: string
        SkuId: SkuId
        StockingPointId: StockingPointId
        Period: Timestamp
        Quantity: Quantity
        Type: PlannedOrderType
        Zone: HorizonZone
        /// True = solver must not move or cancel this order.
        IsFirm: bool
        /// True = order can be adjusted but cancellation requires approval workflow.
        IsFrozen: bool
        SupplierOrResourceId: string option
        /// Human-readable explanation of why this particular date/qty/source was chosen.
        DecisionRationale: string option
        EarliestStartDate: DateTimeOffset option
        LatestEndDate: DateTimeOffset option
        /// Campaign/batch family for grouped production runs.
        SetupGroup: string option
    }

type OrderDecisionRationale =
    { OrderId: string
      WhyThisDate: string
      WhyNotEarlier: string option
      WhyThisQuantity: string
      WhyThisSource: string option }

type LimiterSeverity =
    | Blocking
    | Degraded
    | Warning

type LimiterDomain =
    | Material
    | Capacity
    | Transport
    | Policy

/// Per-demand explainability entry.
type LimiterEntry =
    { DemandId: string
      Domain: LimiterDomain
      Code: string // e.g. "CAPACITY_OVERLOAD", "MATERIAL_SHORTAGE"
      Severity: LimiterSeverity
      Description: string
      Suggestions: string list
      AffectedBucketKey: string
      QuantityImpact: Quantity option }

type CapacityViolation =
    { ResourceId: string
      Period: Timestamp
      Overload: Quantity }

type Shortage =
    { SkuId: SkuId
      StockingPointId: StockingPointId
      Period: Timestamp
      Quantity: Quantity }

type PlanningResult =
    { RunId: PlanningRunId
      ScenarioId: ScenarioId
      /// Links back to the PlanningSnapshot
      InputFingerprintHash: string
      PlannedOrders: PlannedOrder list
      Pegging: PeggingLink list
      CapacityViolations: CapacityViolation list
      Shortages: Shortage list
      LimiterCatalog: LimiterEntry list
      DecisionRationales: Map<string, OrderDecisionRationale>
      GeneratedAt: DateTimeOffset
      ObjectiveValue: decimal
      KpiSummary: PlanKpiSummary }
