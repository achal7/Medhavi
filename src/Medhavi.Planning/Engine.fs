namespace Medhavi.Planning

open System
open Medhavi.SharedKernel

/// Core Planning Engine / Solver API
module NettingEngine =

    /// Runs a basic heuristic MRP netting run over supply and demand
    let runNetting (skuId: SkuId) (onHand: Qty) (demandQty: Qty) (safetyStock: Qty) : Qty =
        let net = onHand - demandQty - safetyStock
        if net < Qty.zero then -net else Qty.zero
