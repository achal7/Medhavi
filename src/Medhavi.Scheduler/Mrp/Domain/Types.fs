module Medhavi.Scheduler.Mrp.Domain.Types

open System
open System.Text.Json.Serialization
open Medhavi.SharedKernel

type MrpRunId = private MrpRunId of string

module MrpRunId =
    let create = IdsFactory.createExplicitId MrpRunId "MrpRunId"
    let value (MrpRunId id) = id

// ============================================================================
// DEMAND INPUT
// ============================================================================

/// Source reference for demand traceability
[<JsonFSharpConverter>]
type DemandSource =
    | CustomerOrder of orderId: string * lineId: string
    | Forecast of forecastId: string
    | Dependent of parentProposalId: string // BOM-exploded dependent demand
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
      BomPath: SkuId list // Trace from root product for cycle detection
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
