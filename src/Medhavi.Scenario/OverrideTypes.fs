namespace Medhavi.Scenario

open System
open System.Text
open System.Security.Cryptography
open System.Text.Json.Serialization

// =============================================================================
// Scenario Data Override — what-if delta mechanism
// =============================================================================

/// A single what-if override to apply on top of live BC data.
/// Only the changed value is stored — not the full data set.
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

    /// Compute a deterministic content hash for identity (used by RemoveOverride).
    /// Using SHA-256 of the string representation avoids fragile index-based removal.
    let contentHash (override_: ScenarioDataOverride) : string =
        let input = sprintf "%A" override_
        let bytes = Encoding.UTF8.GetBytes(input)
        use sha = SHA256.Create()

        Convert
            .ToHexString(sha.ComputeHash(bytes))
            .ToLowerInvariant()

// =============================================================================
// Planning Mode — planning execution strategy
// =============================================================================

/// Controls the scope and warm-start behaviour of a planning run.
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
