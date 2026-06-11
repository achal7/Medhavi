namespace Medhavi.Scheduler.Planning.Domain

open System
open System.Text.Json.Serialization
open Medhavi.SharedKernel
open Medhavi.SharedKernel.ScenarioContracts

[<JsonFSharpConverter>]
type SupplyOrigin =
    | Inventory
    | PurchaseOrder
    | ProductionOrder
    | PlannedOrder
    | Transfer

/// Supply available for netting.
type SupplyBucket =
    { SupplyId: string
      StockingPointId: StockingPointId
      SkuId: SkuId
      Period: Timestamp
      Quantity: Quantity
      Origin: SupplyOrigin
      IsFirm: bool }

/// A demand the engine must satisfy.
type DemandBucket =
    { DemandId: string
      StockingPointId: StockingPointId
      SkuId: SkuId
      Period: Timestamp
      Quantity: Quantity
      DemandType: string // "CustomerOrder" | "Forecast" | etc.
      Priority: int }

type BomEdge =
    { ParentProduct: SkuId
      Component: SkuId
      QtyPer: decimal }

type Routing =
    { Product: SkuId
      ResourceId: string
      LoadPerUnit: Quantity }

type ResourceCapacity =
    { ResourceId: string
      Period: Timestamp
      Available: Quantity }

type PlanningInputData =
    { ScenarioId: ScenarioId
      AsOf: DateTimeOffset
      SupplyBuckets: SupplyBucket list
      Demands: DemandBucket list
      Capacities: ResourceCapacity list
      BomEdges: BomEdge list
      Routings: Routing list }

// ─── PlanningSnapshot aggregate ───────────────────────────────────────────────

type PlanningSnapshot =
    {
        Id: SnapshotId
        ScenarioId: ScenarioId
        /// Point-in-time anchor. PlanningSnapshotBuilder queries all BCs "as-of" this timestamp.
        AsOfTimestamp: DateTimeOffset
        /// SHA-256 of the canonical InputVersionVectors JSON -> O(1) duplicate-run detection.
        InputFingerprintHash: string
        /// Aggregate-level version numbers captured at snapshot creation.
        InputVersionVectors: InputVersionVector
        /// True while a PlanningRun holds the planning lock — prevents concurrent runs.
        IsLocked: bool
        CreatedAt: DateTimeOffset
    }

type PlanningSnapshotCommand =
    | Create of
        scenarioId: ScenarioId *
        asOfTimestamp: DateTimeOffset *
        inputFingerprintHash: string *
        inputVersionVectors: InputVersionVector
    | Lock
    | Unlock
    | Expire

type PlanningSnapshotEvent =
    | SnapshotCreated of SnapshotId * ScenarioId * asOf: DateTimeOffset * fingerprintHash: string * InputVersionVector
    | SnapshotLocked of SnapshotId
    | SnapshotUnlocked of SnapshotId
    | SnapshotExpired of SnapshotId

module PlanningSnapshotAgg =
    let private errConflict msg = Error (DomainError.conflict msg)
    let private errNotFound msg = Error (DomainError.notFound msg)
    let private errInvariant msg = Error (DomainError.invariant msg)

    let handle: Decide<PlanningSnapshot, PlanningSnapshotCommand, PlanningSnapshotEvent> =
        fun command stateOpt ->
            match command, stateOpt with

            | Create(scenarioId, asOf, fingerprintHash, versions), None ->
                let id = Guid.NewGuid()

                let snapshot =
                    { Id = id
                      ScenarioId = scenarioId
                      AsOfTimestamp = asOf
                      InputFingerprintHash = fingerprintHash
                      InputVersionVectors = versions
                      IsLocked = false
                      CreatedAt = DateTimeOffset.UtcNow }

                Ok { NewState = snapshot; Events = [ SnapshotCreated(id, scenarioId, asOf, fingerprintHash, versions) ] }

            | Create _, Some _ -> errConflict "PlanningSnapshot already exists"

            | Lock, Some state when not state.IsLocked ->
                Ok ({ NewState = { state with IsLocked = true }; Events = [ SnapshotLocked state.Id ] })

            | Lock, Some _ -> errInvariant "Snapshot is already locked — another run may be active"
            | Lock, None -> errNotFound "PlanningSnapshot not found"

            | Unlock, Some state when state.IsLocked ->
                Ok ({ NewState = { state with IsLocked = false }; Events = [ SnapshotUnlocked state.Id ] })

            | Unlock, Some _ -> errInvariant "Snapshot is not locked"
            | Unlock, None -> errNotFound "PlanningSnapshot not found"

            | Expire, Some state ->
                Ok ({ NewState = state; Events = [ SnapshotExpired state.Id ] })

            | Expire, None -> errNotFound "PlanningSnapshot not found"

    let evolve (event: PlanningSnapshotEvent) (stateOpt: PlanningSnapshot option) : PlanningSnapshot option =
        match event, stateOpt with
        | SnapshotCreated(id, scenarioId, asOf, fingerprintHash, versions), None ->
            Some
                { Id = id
                  ScenarioId = scenarioId
                  AsOfTimestamp = asOf
                  InputFingerprintHash = fingerprintHash
                  InputVersionVectors = versions
                  IsLocked = false
                  CreatedAt = DateTimeOffset.UtcNow }

        | SnapshotLocked _, Some s -> Some { s with IsLocked = true }
        | SnapshotUnlocked _, Some s -> Some { s with IsLocked = false }
        | SnapshotExpired _, Some s -> Some s // archived externally

        | _, _ -> stateOpt
