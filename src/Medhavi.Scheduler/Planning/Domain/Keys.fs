namespace Medhavi.Scheduler.Planning.Domain

open Medhavi.SharedKernel
open Medhavi.SharedKernel.ScenarioContracts

type SnapshotId = System.Guid

/// Strongly-typed wrapper for the content-addressed PlanVersion identifier.
/// Its value IS the SHA-256 hash of the canonical InputFingerprint JSON.
type PlanVersionId = private PlanVersionId of string

module PlanVersionId =
    let value (PlanVersionId id) = id
    let create (id: string) : PlanVersionId = PlanVersionId(id)

type PlanningRunId = System.Guid

/// Uniquely identifies a (sku, stocking-point, time-bucket) triplet within the
/// current planning run scope. This is the primary Map key throughout the pipeline.
type PlanKey =
    { Sku: SkuId
      StockingPoint: StockingPointId
      Bucket: TimeBucket }

module PlanKey =
    let create sku stockingPoint bucket : PlanKey =
        { Sku = sku
          StockingPoint = stockingPoint
          Bucket = bucket }

/// Links a demand to the specific version that was used during this planning run.
type DemandKey =
    { DemandId: string; Version: Version }

module DemandKey =
    let create demandId version : DemandKey =
        { DemandId = demandId
          Version = version }

/// Links a planned supply order to the supply record version it was generated from.
type SupplyKey =
    { SupplyId: string; Version: Version }

module SupplyKey =
    let create supplyId version : SupplyKey =
        { SupplyId = supplyId
          Version = version }
