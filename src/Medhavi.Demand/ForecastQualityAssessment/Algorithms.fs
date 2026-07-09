module Medhavi.Demand.ForecastQualityAlgorithms
 
open System
open Medhavi.Demand.ForecastQualityAssessment.Model
 
/// BA‑D‑005: Weighted Absolute Percentage Error
let wape (actuals: decimal list) (forecasts: decimal list) : decimal option =
    match actuals, forecasts with
    | [], _
    | _, [] -> None
    | _ when actuals.Length <> forecasts.Length -> None
    | _ ->
        let sumAbsDiff = (actuals, forecasts) ||> List.map2(fun a f -> abs(a - f)) |> List.sum
        let sumActuals = actuals |> List.sum
        if sumActuals <= 0m then None 
        else 
            let w = sumAbsDiff / sumActuals
            Some(max 0m w)
 
/// BA‑D‑005: Mean Absolute Percentage Error
let mape (actuals: decimal list) (forecasts: decimal list) : decimal option =
    match actuals, forecasts with
    | [], _
    | _, [] -> None
    | _ when actuals.Length <> forecasts.Length -> None
    | _ ->
        let percentages = (actuals, forecasts) ||> List.map2(fun a f -> if a <= 0m then 0m else abs((a - f) / a))
        if percentages.IsEmpty then None 
        else Some(max 0m (List.average percentages))
 
/// BA‑D‑005: Forecast Bias (mean error, negative = under-forecast, positive = over-forecast)
let forecastBias (actuals: decimal list) (forecasts: decimal list) : decimal option =
    match actuals, forecasts with
    | [], _
    | _, [] -> None
    | _ when actuals.Length <> forecasts.Length -> None
    | _ ->
        let errors = (actuals, forecasts) ||> List.map2(fun a f -> f - a)
        if errors.IsEmpty then None else Some(List.average errors)
 
/// BA‑D‑005: Forecast Accuracy (1 - WAPE)
let forecastAccuracy (actuals: decimal list) (forecasts: decimal list) : decimal option =
    wape actuals forecasts |> Option.map(fun w -> max 0m (1m - w))
 
/// BA‑D‑006: Forecast Value Added
let fva (actuals: decimal list) (forecasts: decimal list) (naiveForecasts: decimal list) : decimal option =
    match wape actuals forecasts, wape actuals naiveForecasts with
    | Some wapeF, Some wapeN when wapeN > 0m -> Some((wapeN - wapeF) / wapeN)
    | _ -> None
 
/// BA‑D‑007: Forecast Stability (mean absolute period‑to‑period change)
let forecastStability (forecastHistory: decimal list list) : decimal option =
    let validHist = forecastHistory |> List.filter (fun l -> not l.IsEmpty)
    if validHist.Length < 2 then None
    else
        let changes =
            validHist
            |> List.pairwise
            |> List.collect(fun (prev, next) -> 
                let len = min prev.Length next.Length
                if len = 0 then []
                else (prev |> List.take len, next |> List.take len) ||> List.map2(fun p n -> abs(n - p)))
 
        if changes.IsEmpty then None else Some(max 0m (List.average changes))
 
/// BA‑D‑008: Override Effectiveness (fraction of overrides that improved accuracy)
let overrideEffectiveness (overrides: (decimal * decimal) list) (actuals: decimal list) : decimal option =
    let len = min overrides.Length actuals.Length
    if len = 0 then None
    else
        let targetOverrides = overrides |> List.take len
        let targetActuals = actuals |> List.take len
        let improvements =
            (targetOverrides, targetActuals)
            ||> List.map2(fun (original, overrideVal) actual ->
                let origErr = abs(actual - original)
                let newErr = abs(actual - overrideVal)
                if newErr < origErr then 1.0m else 0.0m)
 
        Some(max 0m (List.average improvements))
 
/// BR‑D‑080 — Checks whether actual data coverage meets the policy‑defined threshold.
let checkDataCompleteness (expectedCount: int) (actualCount: int) (threshold: decimal) : bool =
    if expectedCount <= 0 then false
    else (decimal actualCount / decimal expectedCount) >= threshold
 
/// BR‑D‑081 — Checks whether the evaluation period meets the minimum length.
let checkEvaluationPeriod (start: DateTimeOffset) (end_: DateTimeOffset) (minDays: int) : bool =
    if minDays <= 0 then true
    else (end_ - start).TotalDays >= float minDays
 
/// Computes the overall quality score from core metrics using policy‑defined weights.
let computeOverallQualityScore (core: CoreMetrics) (weights: CoreMetrics) : decimal option =
    let totalWeight = 
        max 0m weights.WAPE 
        + max 0m weights.MAPE 
        + max 0m weights.ForecastBias 
        + max 0m weights.ForecastAccuracy
 
    if totalWeight = 0m then None
    else
        let inverseBias = abs core.ForecastBias
        let score =
            ((max 0m core.WAPE) * (max 0m weights.WAPE)
             + (max 0m core.MAPE) * (max 0m weights.MAPE)
             + inverseBias * (max 0m weights.ForecastBias)
             + (max 0m (1m - core.ForecastAccuracy)) * (max 0m weights.ForecastAccuracy))
            / totalWeight
 
        Some(max 0m (min 1m (1m - score)))
