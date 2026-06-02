namespace Medhavi.Planning

open System
open Medhavi.SharedKernel

/// Core Planning Engine / Solver API
module NettingEngine =

    /// Runs a basic heuristic MRP netting run over supply and demand
    let runNetting (skuId: SkuId) (onHand: Quantity) (demandQty: Quantity) (safetyStock: Quantity) : Quantity =
        let totalRequirement = demandQty + safetyStock
        totalRequirement - onHand
