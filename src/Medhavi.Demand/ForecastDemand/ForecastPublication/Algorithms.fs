/// BA-D-001, BA-D-002, BA-D-012 — Statistical Forecasting and Prediction Interval Algorithms
module Medhavi.Demand.ForecastDemand.ForecastPublication.Algorithms

open System
open MathNet.Numerics.Statistics
open MathNet.Numerics.LinearRegression
open Medhavi.SemanticModel
open Model
open Policies

/// Standard deviation of a series, returning a safe positive floor for short or constant series.
let standardDeviation (values: decimal list) : decimal =
    if values.Length < 2 then
        1.0m
    else
        let floatValues = values |> List.map float
        let sd = Statistics.StandardDeviation(floatValues)
        let sdDec = if Double.IsNaN(sd) || Double.IsInfinity(sd) then 1.0m else decimal sd
        if sdDec > 0.0m then sdDec else 1.0m

/// Simple Exponential Smoothing (SES) — Pure Tail-Recursive
let simpleExponentialSmoothing (alpha: decimal) (history: decimal list) (horizon: int) : decimal list =
    match history with
    | [] -> List.replicate horizon 0.0m
    | first :: rest ->
        let rec computeLevel (remaining: decimal list) (currentLevel: decimal) : decimal =
            match remaining with
            | [] -> currentLevel
            | y :: tail ->
                let nextLevel = alpha * y + (1.0m - alpha) * currentLevel
                computeLevel tail nextLevel

        let finalLevel = computeLevel rest first
        List.replicate horizon (max 0.0m finalLevel)

/// Croston's Method with Syntetos-Boylan Approximation (SBA) for Intermittent Demand
let crostonSba (alpha: decimal) (history: decimal list) (horizon: int) : decimal list =
    let nonZeros = history |> List.filter(fun v -> v > 0.0m)

    match nonZeros with
    | [] -> List.replicate horizon 0.0m
    | _ ->
        let initialDemandSize = List.average nonZeros
        let initialInterval = decimal history.Length / decimal nonZeros.Length

        let rec compute (remaining: decimal list) (size: decimal) (interval: decimal) (periodsElapsed: int) : decimal =
            match remaining with
            | [] ->
                if interval = 0.0m then
                    0.0m
                else
                    // Syntetos-Boylan debiasing factor: (1 - alpha / 2)
                    let sbaFactor = 1.0m - (alpha / 2.0m)
                    (size / interval) * sbaFactor
            | y :: tail ->
                if y > 0.0m then
                    let nextSize = alpha * y + (1.0m - alpha) * size
                    let nextInterval = alpha * decimal periodsElapsed + (1.0m - alpha) * interval
                    compute tail nextSize nextInterval 1
                else
                    compute tail size interval (periodsElapsed + 1)

        let rate = compute history initialDemandSize initialInterval 1
        List.replicate horizon (max 0.0m rate)

/// Linear Trend Regression via MathNet.Numerics Ordinary Least Squares
let linearTrendRegression (history: decimal list) (horizon: int) : decimal list =
    let n = history.Length

    if n < 2 then
        let avg = if history.IsEmpty then 0.0m else List.average history
        List.replicate horizon (max 0.0m avg)
    else
        let xData = [| 1.0 .. float n |]
        let yData = history |> List.map float |> Array.ofList
        let struct (intercept, slope) = SimpleRegression.Fit(xData, yData)

        let interceptDec = if Double.IsNaN(intercept) then 0.0m else decimal intercept
        let slopeDec = if Double.IsNaN(slope) then 0.0m else decimal slope

        [ 1..horizon ]
        |> List.map(fun h ->
            let step = decimal(n + h)
            let projected = interceptDec + slopeDec * step
            max 0.0m projected)

/// Holt-Winters Additive Seasonality & Trend — Pure Functional Fold
let holtWintersAdditive
    (alpha: decimal)
    (beta: decimal)
    (gamma: decimal)
    (seasonLength: int)
    (history: decimal list)
    (horizon: int)
    : decimal list =
    let P = seasonLength

    if history.Length < 2 * P || P < 2 then
        simpleExponentialSmoothing alpha history horizon
    else
        // 1. Initial level, trend, and seasonal components
        let firstSeason = history |> List.take P
        let secondSeason = history |> List.skip P |> List.take P
        let l0 = List.average firstSeason
        let t0 = (List.average secondSeason - l0) / decimal P
        let (s0: decimal list) = firstSeason |> List.map(fun y -> y - l0)

        // 2. Pure state accumulator tuple: (level, trend, seasonalList, stepIndex)
        let (initialState: decimal * decimal * decimal list * int) = (l0, t0, s0, 0)
        let remainingData = history |> List.skip P

        let (finalLevel: decimal), (finalTrend: decimal), (finalSeasonals: decimal list), (totalSteps: int) =
            remainingData
            |> List.fold
                (fun (lvl: decimal, trd: decimal, seasonals: decimal list, idx: int) (y: decimal) ->
                    let seasonIdx = idx % P
                    let sOld = seasonals.[seasonIdx]
                    let nextLevel = alpha * (y - sOld) + (1.0m - alpha) * (lvl + trd)
                    let nextTrend = beta * (nextLevel - lvl) + (1.0m - beta) * trd
                    let nextSeasonal = gamma * (y - nextLevel) + (1.0m - gamma) * sOld

                    let updatedSeasonals =
                        seasonals |> List.mapi(fun i sVal -> if i = seasonIdx then nextSeasonal else sVal)

                    (nextLevel, nextTrend, updatedSeasonals, idx + 1))
                initialState

        // 3. Multi-step forward projection
        [ 1..horizon ]
        |> List.map(fun h ->
            let seasonIdx = (totalSteps + h - 1) % P
            let seasonalComponent = finalSeasonals.[seasonIdx]
            let projected = finalLevel + decimal h * finalTrend + seasonalComponent
            max 0.0m projected)

/// Automated Model Selection based on series characteristics and sparsity
let autoSelectModel (history: decimal list) (policy: ForecastModelParametersPolicy) (horizon: int) : decimal list =
    let totalCount = history.Length
    let zeroCount = history |> List.filter(fun v -> v = 0.0m) |> List.length

    let sparsity =
        if totalCount > 0 then
            (decimal zeroCount / decimal totalCount) * 100.0m
        else
            0.0m

    if sparsity >= 40.0m then
        // Intermittent demand pattern
        crostonSba 0.3m history horizon
    elif history.Length >= 2 * policy.DefaultSeasonLength then
        // Sufficient history for seasonality + trend
        holtWintersAdditive
            policy.HoltWintersAlpha
            policy.HoltWintersBeta
            policy.HoltWintersGamma
            policy.DefaultSeasonLength
            history
            horizon
    elif history.Length >= 4 then
        // Trend regression
        linearTrendRegression history horizon
    else
        // Baseline SES
        simpleExponentialSmoothing policy.SesAlpha history horizon

/// BA-D-002: Parametric Prediction Interval Calculation with Horizon Uncertainty Scaling
let calculatePredictionInterval
    (mean: decimal)
    (historyStdDev: decimal)
    (horizonStep: int)
    (zScore: decimal)
    : decimal * decimal =
    // Variance growth factor: sigma_h = sigma * sqrt(1 + 0.08 * (h - 1))
    let varianceInflation = Math.Sqrt(1.0 + 0.08 * float(horizonStep - 1))
    let scaledStdDev = historyStdDev * decimal varianceInflation
    let margin = zScore * scaledStdDev
    let lower = max 0.0m (mean - margin)
    let upper = mean + margin
    (lower, upper)

/// BA-D-001 / BA-D-002: Forecast a single series for all target buckets
let forecastSeries
    (history: decimal list)
    (horizon: int)
    (modelId: string)
    (policy: ForecastModelParametersPolicy)
    : (decimal * decimal * decimal) list =
    let projectedMeans =
        match modelId with
        | "Croston"
        | "CrostonSba" -> crostonSba 0.3m history horizon
        | "HoltWinters" ->
            holtWintersAdditive
                policy.HoltWintersAlpha
                policy.HoltWintersBeta
                policy.HoltWintersGamma
                policy.DefaultSeasonLength
                history
                horizon
        | "LinearTrend" -> linearTrendRegression history horizon
        | "SES" -> simpleExponentialSmoothing policy.SesAlpha history horizon
        | _ -> autoSelectModel history policy horizon

    let stdDev = standardDeviation history

    projectedMeans
    |> List.mapi(fun stepIdx mean ->
        let lower, upper = calculatePredictionInterval mean stdDev (stepIdx + 1) policy.DefaultZScore
        (mean, lower, upper))

/// BA-D-012: Top-Down Disaggregation Matrix Allocation
let disaggregateTopDown
    (aggregateForecast: decimal list)
    (proportions: Map<ItemId * LocationId, decimal>)
    : Map<ItemId * LocationId, decimal list> =
    proportions |> Map.map(fun _ weight -> aggregateForecast |> List.map(fun totalQty -> totalQty * weight))
