namespace Medhavi.Planning

open System
open Medhavi.SharedKernel
open Medhavi.Supply

/// Core Planning Engine / Solver API
module NettingEngine =

    /// Runs a basic heuristic MRP netting run over supply and demand
    let runNetting (skuId: SkuId) (onHand: Qty) (demandQty: Qty) (safetyStock: Qty) : Qty =
        let net = onHand - demandQty - safetyStock
        if net < 0.0m then -net
        else 0.0m
