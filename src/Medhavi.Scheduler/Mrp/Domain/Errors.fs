namespace Medhavi.Scheduler.Mrp.Domain.Errors

open System
open System.Text.Json.Serialization

/// Errors that can occur during preprocessing
[<JsonFSharpConverter>]
type PreprocessError =
    | InvalidDemand of productId: string * reason: string
    | EmptyDemandList
    | ForecastConsumptionError of message: string

/// Errors that can occur during BOM explosion
[<JsonFSharpConverter>]
type BomExplosionError =
    | BomNotFound of productId: string
    | BomNotActive of productId: string
    | CycleDetected of path: string list
    | InvalidQuantity of productId: string * quantity: decimal * reason: string
    | BomSelectionFailed of productId: string * reason: string
    | MaxDepthExceeded of productId: string * depth: int
    | BomNotEffective of productId: string * asOf: DateTimeOffset

/// Errors that can occur during netting
[<JsonFSharpConverter>]
type NettingError =
    | MaterialUnavailable of productId: string * required: decimal * available: decimal
    | InventoryQueryFailed of productId: string * reason: string
    | SafetyStockCalculationFailed of productId: string * reason: string
    | InvalidNettingPolicy of reason: string

/// Errors that can occur during supply generation
[<JsonFSharpConverter>]
type SupplyGenerationError =
    | NoSupplierFound of productId: string
    | NoRoutingFound of productId: string
    | LotSizingError of productId: string * reason: string
    | ProposalCreationFailed of productId: string * reason: string

/// Errors that can occur during capacity checking
[<JsonFSharpConverter>]
type CapacityCheckError =
    | RoutingNotFound of routingId: string
    | RoutingExpired of routingId: string * asOf: DateTimeOffset
    | RoutingNotYetEffective of routingId: string * effectiveFrom: DateTimeOffset
    | MissingStepResourceMap of routingId: string * stepId: string
    | MissingDurationData of routingId: string * stepId: string
    | CapacityUnavailable of resourceId: string * required: TimeSpan * available: TimeSpan
    | AllocationFailed of reason: string

/// Errors that can occur during pegging
[<JsonFSharpConverter>]
type PeggingError =
    | DemandNotFound of demandId: string
    | SupplyNotFound of supplyId: string
    | QuantityMismatch of demandQty: decimal * supplyQty: decimal
    | PeggingLinkCreationFailed of reason: string

// ============================================================================
// PIPELINE ERROR (COMPOSITE)
// ============================================================================

/// Unified pipeline step error
[<JsonFSharpConverter>]
type MrpStepError =
    | Preprocess of PreprocessError list
    | BomExplosion of BomExplosionError list
    | Netting of NettingError list
    | SupplyGeneration of SupplyGenerationError list
    | CapacityCheck of CapacityCheckError list
    | Pegging of PeggingError list
    | Postprocess of string list
    | Cancelled of reason: string
    | InternalError of Exception

// ============================================================================
// APPLICATION ERROR
// ============================================================================

/// Application-level MRP errors
[<JsonFSharpConverter>]
type MrpApplicationError =
    | PipelineError of MrpStepError
    | Timeout of duration: TimeSpan
    | RunAlreadyExists of runId: string
    | RunNotFound of runId: string
    | UnexpectedError of Exception

// ============================================================================
// ERROR HELPERS
// ============================================================================

module MrpStepError =
    /// Convert step error to user-friendly message
    let toMessage (error: MrpStepError) : string =
        match error with
        | Preprocess errors ->
            let messages =
                errors
                |> List.map (function
                    | InvalidDemand(pid, reason) -> $"Invalid demand for {pid}: {reason}"
                    | EmptyDemandList -> "No demands to process"
                    | ForecastConsumptionError msg -> $"Forecast consumption error: {msg}")

            String.concat "; " messages

        | BomExplosion errors ->
            let messages =
                errors
                |> List.map (function
                    | BomNotFound pid -> $"BOM not found for {pid}"
                    | BomNotActive pid -> $"BOM not active for {pid}"
                    | CycleDetected path -> let pathStr = String.concat " → " path in $"BOM cycle detected: {pathStr}"
                    | InvalidQuantity(pid, qty, reason) -> $"Invalid quantity {qty} for {pid}: {reason}"
                    | BomSelectionFailed(pid, reason) -> $"BOM selection failed for {pid}: {reason}"
                    | MaxDepthExceeded(pid, depth) -> $"Max depth {depth} exceeded for {pid}"
                    | BomNotEffective(pid, asOf) -> $"BOM not effective for {pid} at {asOf}")

            String.concat "; " messages

        | Netting errors ->
            let messages =
                errors
                |> List.map (function
                    | MaterialUnavailable(pid, req, avail) ->
                        $"Material unavailable for {pid}: required {req}, available {avail}"
                    | InventoryQueryFailed(pid, reason) -> $"Inventory query failed for {pid}: {reason}"
                    | SafetyStockCalculationFailed(pid, reason) ->
                        $"Safety stock calculation failed for {pid}: {reason}"
                    | InvalidNettingPolicy reason -> $"Invalid netting policy: {reason}")

            String.concat "; " messages

        | SupplyGeneration errors ->
            let messages =
                errors
                |> List.map (function
                    | NoSupplierFound pid -> $"No supplier found for {pid}"
                    | NoRoutingFound pid -> $"No routing found for {pid}"
                    | LotSizingError(pid, reason) -> $"Lot sizing error for {pid}: {reason}"
                    | ProposalCreationFailed(pid, reason) -> $"Proposal creation failed for {pid}: {reason}")

            String.concat "; " messages

        | CapacityCheck errors ->
            let messages =
                errors
                |> List.map (function
                    | RoutingNotFound rid -> $"Routing not found: {rid}"
                    | RoutingExpired(rid, asOf) -> $"Routing expired: {rid} at {asOf}"
                    | RoutingNotYetEffective(rid, from) -> $"Routing not yet effective: {rid} until {from}"
                    | MissingStepResourceMap(rid, sid) -> $"Missing resource map for {rid}/{sid}"
                    | MissingDurationData(rid, sid) -> $"Missing duration data for {rid}/{sid}"
                    | CapacityUnavailable(res, req, avail) ->
                        $"Capacity unavailable for {res}: required {req}, available {avail}"
                    | AllocationFailed reason -> $"Allocation failed: {reason}")

            String.concat "; " messages

        | Pegging errors ->
            let messages =
                errors
                |> List.map (function
                    | DemandNotFound did -> $"Demand not found: {did}"
                    | SupplyNotFound sid -> $"Supply not found: {sid}"
                    | QuantityMismatch(demandQty, supplyQty) ->
                        $"Quantity mismatch: demand {demandQty}, supply {supplyQty}"
                    | PeggingLinkCreationFailed reason -> $"Pegging link creation failed: {reason}")

            String.concat "; " messages

        | Postprocess errors ->
            let message = String.concat "; " errors
            $"Postprocess errors: {message}"

        | Cancelled reason -> $"MRP run cancelled: {reason}"
        | InternalError ex -> $"Internal error: {ex.Message}"

module MrpApplicationError =
    let toMessage (error: MrpApplicationError) : string =
        match error with
        | PipelineError stepError -> MrpStepError.toMessage stepError
        | Timeout duration -> $"MRP run timed out after {duration}"
        | RunAlreadyExists rid -> $"MRP run '{rid}' already exists"
        | RunNotFound rid -> $"MRP run '{rid}' not found"
        | UnexpectedError ex -> $"Unexpected error: {ex.Message}"
