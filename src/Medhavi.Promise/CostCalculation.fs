module Medhavi.Promise.CostCalculation

open System
open Medhavi.Promise.PromiseTypes
open Medhavi.SharedKernel.PromisePolicy
open Medhavi.Transport

let latenessPenaltyPerDay = 50.0m
let defaultSkuCost = 10.0m
let holdingCostPerDayPerUnit = 0.5m // Default holding cost rate - should come from policy or config

/// Calculate cost for a promise based on quantity, providers, and policy
let calculateCost
    (_: PromisePolicy)
    (qty: decimal)
    (materialSnapshot: MaterialSnapshot option)
    (supplierOption: SupplierOption option)
    (itinerary: Itinerary option)
    (promiseDate: DateTimeOffset)
    (dueDate: DateTimeOffset)
    (productionRate: decimal) // Hourly production rate
    (capacityResult: CapacityCheckResult option)
    (fxRate: decimal option) // Currency conversion rate
    : CostBreakdown =

    let applyFx (cost: decimal) =
        match fxRate with
        | Some rate when rate > 0m -> cost * rate
        | _ -> cost

    // Material cost calculation
    let materialCost =
        match supplierOption with
        | Some so -> so.Cost * qty
        | None ->
            match materialSnapshot with
            | Some snap ->
                let availableMaterial = snap.OnHand - snap.Safety
                match availableMaterial >= qty with
                | true -> qty * defaultSkuCost
                | false -> qty * defaultSkuCost
            | None -> qty * defaultSkuCost

    // Production cost calculation
    let productionCost =
        match capacityResult with
        | Some cap ->
            let totalMinutes = cap.RequiredLoads |> Map.toList |> List.sumBy snd
            (decimal totalMinutes / 60.0m) * productionRate
        | None -> 0m

    // Transport cost calculation
    let transportCost =
        match itinerary with
        | Some it ->
            let fixedC = it.TotalFixedCost
            let varC = it.TotalVariableCostPerUnit |> Option.defaultValue 0m
            fixedC + (varC * qty)
        | None -> 0m

    // Holding cost calculation
    let holdingCost =
        let daysToConsume =
            match promiseDate > dueDate with
            | true -> decimal (promiseDate - dueDate).TotalDays
            | false -> 0m
        max 0m daysToConsume * holdingCostPerDayPerUnit * qty

    // Lateness penalty
    let diff = promiseDate - dueDate
    let lateness = if diff.TotalDays <= 0.0 then 0m else decimal diff.TotalDays * latenessPenaltyPerDay

    // Apply FX conversion to all costs
    CostBreakdown.create
        (applyFx materialCost)
        (applyFx productionCost)
        (applyFx transportCost)
        (applyFx holdingCost)
        (applyFx lateness)

/// Calculate supplier cost for shortfall coverage
let calculateSupplierCost (policy: PromisePolicy) (so: SupplierOption) (qty: decimal) : decimal =
    let baseCost = so.Cost * qty
    match so.Reliability with
    | Some rel when policy.RiskPreference = P95 -> baseCost * (1.0m + (1.0m - rel) * 0.5m)
    | _ -> baseCost
