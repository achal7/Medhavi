namespace Medhavi.Planning

open Medhavi.SharedKernel

/// Core Planning Engine / Solver API
module NettingEngine =

    /// Runs a basic heuristic MRP netting run over supply and demand
    let runNetting (_: SkuId) (onHand: Quantity) (demandQty: Quantity) (safetyStock: Quantity) : Quantity =
        let totalRequirement = demandQty + safetyStock
        totalRequirement - onHand
