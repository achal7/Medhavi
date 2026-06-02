module Medhavi.Promise.CostCalculation

open System
open Medhavi.Promise.PromiseTypes
open Medhavi.SharedKernel.PromisePolicy
open Medhavi.Transport

/// Calculate cost for a promise based on quantity, providers, and policy
let calculateCost
    (policy: PromisePolicy)
    (qty: decimal)
    (materialSnapshot: MaterialSnapshot option)
    (supplierOption: SupplierOption option)
    (itinerary: Itinerary option)
    (promiseDate: DateTimeOffset)
    (dueDate: DateTimeOffset)
    : CostBreakdown =

    let materialCost =
        match supplierOption with
        | Some so -> so.Cost * qty
        | None ->
            match materialSnapshot with
            | Some _ -> qty * 10.0m // standard SKU cost fallback
            | None -> 0m

    let productionCost = 0m // Hook for production cost calculation in CTP

    let transportCost =
        match itinerary with
        | Some it ->
            let fixedC = it.TotalFixedCost
            let varC = it.TotalVariableCostPerUnit |> Option.defaultValue 0m
            fixedC + (varC * qty)
        | None -> 0m

    let holdingCost = 0m // Hook for holding cost calculation

    let diff = promiseDate - dueDate
    let lateness = if diff.TotalDays <= 0.0 then 0m else decimal diff.TotalDays * 50.0m

    CostBreakdown.create materialCost productionCost transportCost holdingCost lateness