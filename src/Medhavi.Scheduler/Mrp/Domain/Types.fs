/// MRP Domain Types — Rich value objects and core domain types
/// FP/DDD Pattern: Pure types, no side effects, immutable data structures
/// Namespace follows existing Engine.fs convention (Medhavi.Planning)
module Medhavi.Planning.Mrp.Domain.Types

open System
open System.Text.Json.Serialization
open Medhavi.SharedKernel
open Medhavi.Planning.Mrp.Domain.Policies

// ============================================================================
// MRP-SPECIFIC IDS
// ============================================================================

[<JsonFSharpConverter>]
type MrpRunId = private MrpRunId of string

module MrpRunId =
    let create = IdsFactory.createExplicitId MrpRunId "MrpRunId"
    let value (MrpRunId id) = id

[<JsonFSharpConverter>]
type SupplyProposalId = private SupplyProposalId of string

module SupplyProposalId =
    let create = IdsFactory.createExplicitId SupplyProposalId "SupplyProposalId"
    let value (SupplyProposalId id) = id

    /// Deterministic proposal ID for idempotent generation (Phase 9.6)
    /// Keyed by demandId/period/type to prevent duplicates across repeated runs
    let createDeterministic (proposalType: string) (anchorId: string) (dueDate: DateTimeOffset) =
        let id = IdsFactory.DeterministicIds.proposalId proposalType anchorId dueDate
        SupplyProposalId id

// ============================================================================
// DEMAND INPUT
// ============================================================================

/// Source reference for demand traceability
[<JsonFSharpConverter>]
type DemandSource =
    | CustomerOrder of orderId: string * lineId: string
    | Forecast of forecastId: string
    | Dependent of parentProposalId: string  // BOM-exploded dependent demand
    | SafetyStock of skuId: string * nodeId: string
    | Manual of reference: string

/// MRP Demand — input to MRP process
/// Represents a single demand signal to be planned
type MrpDemand =
    { DemandId: string
      SkuId: SkuId
      NodeId: NodeId
      StockingPointId: StockingPointId
      Quantity: Quantity
      RequiredDate: Timestamp
      Source: DemandSource
      Priority: int option }

// ============================================================================
// BOM EXPLOSION OUTPUT
// ============================================================================

/// Exploded component from BOM explosion (Phase 9.1)
type ExplodedComponent =
    { SkuId: SkuId
      NodeId: NodeId
      StockingPointId: StockingPointId
      RequiredQuantity: Quantity
      RequiredDate: Timestamp
      BomLevel: int
      BomPath: SkuId list  // Trace from root product for cycle detection
      ParentSkuId: SkuId option
      IsPhantom: bool }

// ============================================================================
// NETTING OUTPUT
// ============================================================================

/// Net Requirement — result of material netting calculation (Phase 9.2)
type NetRequirement =
    { SkuId: SkuId
      NodeId: NodeId
      StockingPointId: StockingPointId
      GrossRequirement: Quantity
      OnHand: Quantity
      Inbound: Quantity
      Reservations: Quantity
      SafetyStock: Quantity
      NetRequirement: Quantity
      RequiredDate: Timestamp
      BomPath: SkuId list option }

// ============================================================================
// SUPPLY PROPOSALS
// ============================================================================

/// Supply proposal type — maps to SupplyOrderType in Medhavi.Supply
[<JsonFSharpConverter>]
type ProposalType =
    | PlannedPurchaseOrder
    | PlannedWorkOrder
    | PlannedTransferOrder

/// Supply proposal status within MRP lifecycle
[<JsonFSharpConverter>]
type ProposalStatus =
    | Planned     // Initial state — can be modified by subsequent runs
    | Firmed      // Firmed — protected from automatic changes
    | Released    // Released to execution (converted to SupplyOrder)
    | Cancelled   // Cancelled

/// Supply Proposal — output of MRP planning (Phase 9.4)
type SupplyProposal =
    { Id: SupplyProposalId
      ProposalType: ProposalType
      SkuId: SkuId
      NodeId: NodeId
      StockingPointId: StockingPointId
      Quantity: Quantity
      DueDate: Timestamp
      StartDate: Timestamp option
      RoutingId: RoutingId option
      SupplierId: SupplierId option
      Priority: int
      IsExpedite: bool
      Status: ProposalStatus
      PeggingRefs: string list
      CapacityCheckedDate: Timestamp option
      CreatedAt: Timestamp }

// ============================================================================
// ACTION MESSAGES (MRP Exception Handling)
// ============================================================================

/// Action message types — planner recommendations
[<JsonFSharpConverter>]
type ActionMessage =
    | Expedite of proposalId: string * reason: string * daysToExpedite: int
    | Defer of proposalId: string * newDate: Timestamp * reason: string
    | Cancel of proposalId: string * reason: string
    | Reschedule of proposalId: string * fromDate: Timestamp * toDate: Timestamp * reason: string
    | IncreaseQuantity of proposalId: string * additionalQty: Quantity * reason: string
    | DecreaseQuantity of proposalId: string * reduceBy: Quantity * reason: string

/// Action severity levels
[<JsonFSharpConverter>]
type ActionSeverity =
    | Critical  // Immediate attention required
    | Warning   // Should be addressed soon
    | Info      // Informational only

/// Action message with metadata
type ActionMessageRecord =
    { Id: string
      Message: ActionMessage
      SkuId: SkuId
      StockingPointId: StockingPointId
      Severity: ActionSeverity
      CreatedAt: Timestamp
      AcknowledgedAt: Timestamp option }

// ============================================================================
// PEGGING
// ============================================================================

/// Pegging link — traces demand to supply (Phase 9.4)
type PeggingLink =
    { DemandId: string
      SupplyId: string
      Quantity: Quantity
      SkuId: SkuId }

// ============================================================================
// MRP RUN RESULT
// ============================================================================

/// MRP Run Status
[<JsonFSharpConverter>]
type MrpRunStatus =
    | Pending
    | Running of progress: int
    | Completed
    | Failed of error: string
    | Cancelled

/// MRP Run Result — aggregated output of a complete MRP run
type MrpRunResult =
    { RunId: MrpRunId
      StartTime: Timestamp
      EndTime: Timestamp
      Status: MrpRunStatus
      BomExplosionCount: int
      NetRequirements: NetRequirement list
      Proposals: SupplyProposal list
      ActionMessages: ActionMessageRecord list
      PeggingLinks: PeggingLink list
      Errors: string list
      Warnings: string list }

// ============================================================================
// MRP CONTEXT (Pipeline Context — threaded immutably through steps)
// ============================================================================

/// MRP telemetry for observability
type MrpTelemetry =
    { BomExplosionDuration: TimeSpan
      NettingDuration: TimeSpan
      CapacityCheckDuration: TimeSpan
      TotalDuration: TimeSpan
      ComponentsProcessed: int
      ProposalsGenerated: int }

module MrpTelemetry =
    let empty =
        { BomExplosionDuration = TimeSpan.Zero
          NettingDuration = TimeSpan.Zero
          CapacityCheckDuration = TimeSpan.Zero
          TotalDuration = TimeSpan.Zero
          ComponentsProcessed = 0
          ProposalsGenerated = 0 }

/// MRP events emitted during a run (for telemetry/event sourcing)
[<JsonFSharpConverter>]
type MrpEvent =
    | MrpRunStarted of runId: string * startedAt: Timestamp
    | BomExplosionCompleted of componentCount: int
    | NettingCompleted of requirementCount: int
    | SupplyProposalCreated of proposal: SupplyProposal
    | CapacityCheckCompleted of checkedCount: int
    | PeggingCompleted of linkCount: int
    | ActionMessageGenerated of ActionMessageRecord
    | MrpRunCompleted of runId: string * completedAt: Timestamp
    | MrpRunFailed of runId: string * error: string * failedAt: Timestamp

// ============================================================================
// MRP CONTEXT DEFINITION
// ============================================================================

/// Immutable context passed through pipeline steps
type MrpContext =
    { RunId: MrpRunId
      StartDate: Timestamp
      EndDate: Timestamp
      StockingPointId: StockingPointId
      Policy: MrpPolicy
      Telemetry: MrpTelemetry
      Events: MrpEvent list
      ActionMessages: ActionMessageRecord list
      Warnings: string list }

module MrpContext =
    /// Create initial context from request
    let create
        (runId: MrpRunId)
        (startDate: Timestamp)
        (endDate: Timestamp)
        (stockingPointId: StockingPointId)
        (policy: MrpPolicy)
        : MrpContext =
        { RunId = runId
          StartDate = startDate
          EndDate = endDate
          StockingPointId = stockingPointId
          Policy = policy
          Telemetry = MrpTelemetry.empty
          Events = []
          ActionMessages = []
          Warnings = [] }

    /// Add event to context
    let addEvent (event: MrpEvent) (ctx: MrpContext) : MrpContext =
        { ctx with Events = ctx.Events @ [ event ] }

    /// Add action message to context
    let addActionMessage (msg: ActionMessageRecord) (ctx: MrpContext) : MrpContext =
        { ctx with ActionMessages = ctx.ActionMessages @ [ msg ] }

    /// Add warning to context
    let addWarning (warning: string) (ctx: MrpContext) : MrpContext =
        { ctx with Warnings = ctx.Warnings @ [ warning ] }

    /// Update telemetry
    let updateTelemetry (update: MrpTelemetry -> MrpTelemetry) (ctx: MrpContext) : MrpContext =
        { ctx with Telemetry = update ctx.Telemetry }
