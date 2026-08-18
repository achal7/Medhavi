/// BA-D-008 & BA-D-009 — Forecast Quality Metrics & State Determination Engine
/// Pure mathematical algorithms implementing enterprise accuracy metrics, tracking signals, and FVA
module Medhavi.Demand.EvaluateDemandQuality.ForecastQualityAssessment.Algorithms

open Model
open Policies

// =============================================================================
// DOMAIN KNOWLEDGE & MATHEMATICAL FOUNDATIONS:
//
// 1. WAPE (Weighted Absolute Percentage Error):
//    WAPE = (Sum(|FinalForecast_t - Actual_t|) / Sum(Actual_t)) * 100%
//    - Why WAPE over MAPE? Simple MAPE treats a 10-unit error on a 1-unit sale as
//      a 1000% error, distorting metrics on long-tail items, and fails completely
//      when actuals are zero (division by zero). WAPE weights errors by sales
//      volume, providing an operationally meaningful, aggregate metric.
//
// 2. Forecast Bias (Normalized Directional Error):
//    Bias = (Sum(FinalForecast_t - Actual_t) / Sum(Actual_t)) * 100%
//    - Interpretation:
//      * Positive Bias (+%): Systemic OVER-forecasting -> leads to excess inventory,
//        working capital waste, and shelf expiration.
//      * Negative Bias (-%): Systemic UNDER-forecasting -> leads to stockouts, lost
//        sales, and poor OTIF (On-Time In-Full) service levels.
//
// 3. Tracking Signal (Statistical Process Control):
//    CFE = Cumulative Forecast Error = Sum(FinalForecast_t - Actual_t)
//    MAD = Mean Absolute Deviation = (1/N) * Sum(|FinalForecast_t - Actual_t|)
//    Tracking Signal (TS) = CFE / MAD
//    - Statistical Limit: In a normal distribution, 1 MAD ≈ 0.8 standard deviations.
//      A tracking signal exceeding ±4.0 indicates that the forecasting algorithm
//      is statistically out of control and requires model re-training or recalibration.
//
// 4. Forecast Value Add (FVA - Gilliland Framework):
//    FVA = WAPE(System Baseline Forecast) - WAPE(Final Overridden Forecast)
//    - Positive FVA (+%): Human planner adjustments IMPROVED forecast accuracy.
//    - Negative FVA (-%): Human planner adjustments DEGRADED forecast accuracy,
//      signaling non-value-adding "planner touch" and process waste.
//
// 5. Override Effectiveness:
//    Percentage of manual planner touches where |Final - Actual| < |System - Actual|.
// =============================================================================

/// Pure function to compute comprehensive forecast quality metrics from observation pairs (BA-D-008)
let computeMetrics
    (observations: ObservationComparison list)
    (completenessScore: decimal)
    (policy: ForecastMeasurementPolicy)
    : ForecastQualityMetrics =

    if observations.IsEmpty then
        { Wape = 0.0m
          ForecastBias = 0.0m
          ForecastAccuracy = 0.0m
          TrackingSignal = None
          IsOutOfControl = false
          ForecastValueAdd = None
          OverrideEffectiveness = None
          ForecastStability = None
          Mape = None
          CompletenessScore = completenessScore }
    else
        let n = decimal observations.Length
        let totalActual = observations |> List.sumBy (fun o -> o.ActualDemand)
        let totalFinalAbsError = observations |> List.sumBy (fun o -> abs (o.FinalForecast - o.ActualDemand))
        let totalFinalError = observations |> List.sumBy (fun o -> o.FinalForecast - o.ActualDemand)
        let totalSystemAbsError = observations |> List.sumBy (fun o -> abs (o.SystemForecast - o.ActualDemand))

        // 1. WAPE
        let wape =
            if totalActual > 0.0m then
                (totalFinalAbsError / totalActual) * 100.0m
            elif totalFinalAbsError = 0.0m then
                0.0m
            else
                100.0m

        // 2. Forecast Bias
        let bias =
            if totalActual > 0.0m then
                (totalFinalError / totalActual) * 100.0m
            elif totalFinalError = 0.0m then
                0.0m
            elif totalFinalError > 0.0m then
                100.0m
            else
                -100.0m

        // 3. Forecast Accuracy
        let accuracy = max 0.0m (100.0m - wape)

        // 4. Tracking Signal (CFE / MAD)
        let mad = totalFinalAbsError / n
        let trackingSignal, isOutOfControl =
            if mad > 0.0m then
                let ts = totalFinalError / mad
                let outOfControl = abs ts > policy.TrackingSignalLimit
                Some ts, outOfControl
            else
                None, false

        // 5. Forecast Value Add (FVA)
        let systemWape =
            if totalActual > 0.0m then
                (totalSystemAbsError / totalActual) * 100.0m
            else
                0.0m

        let fva =
            let anyOverrides = observations |> List.exists (fun o -> o.FinalForecast <> o.SystemForecast)
            if anyOverrides then
                Some (systemWape - wape)
            else
                None

        // 6. Override Effectiveness
        let overriddenObservations =
            observations |> List.filter (fun o -> o.FinalForecast <> o.SystemForecast)

        let overrideEffectiveness =
            if not overriddenObservations.IsEmpty then
                let improvedCount =
                    overriddenObservations
                    |> List.filter (fun o ->
                        abs (o.FinalForecast - o.ActualDemand) < abs (o.SystemForecast - o.ActualDemand))
                    |> List.length

                Some ((decimal improvedCount / decimal overriddenObservations.Length) * 100.0m)
            else
                None

        // 7. MAPE (Mean Absolute Percentage Error on non-zero actuals)
        let nonZeroActuals =
            observations |> List.filter (fun o -> o.ActualDemand > 0.0m)

        let mape =
            if not nonZeroActuals.IsEmpty then
                let sumPercentageError =
                    nonZeroActuals
                    |> List.sumBy (fun o -> (abs (o.FinalForecast - o.ActualDemand) / o.ActualDemand) * 100.0m)
                Some (sumPercentageError / decimal nonZeroActuals.Length)
            else
                None

        { Wape = wape
          ForecastBias = bias
          ForecastAccuracy = accuracy
          TrackingSignal = trackingSignal
          IsOutOfControl = isOutOfControl
          ForecastValueAdd = fva
          OverrideEffectiveness = overrideEffectiveness
          ForecastStability = None
          Mape = mape
          CompletenessScore = completenessScore }

/// Pure function to evaluate the overall Governed Quality State from computed metrics (BA-D-009)
let determineQualityState
    (metrics: ForecastQualityMetrics)
    (policy: ForecastMeasurementPolicy)
    : QualityState =

    // Data Completeness Check
    if metrics.CompletenessScore < policy.MinCompletenessThreshold then
        Critical
    else
        // 1. Evaluate WAPE component
        let wapeState =
            if metrics.Wape <= policy.WapeExcellentThreshold then Excellent
            elif metrics.Wape <= policy.WapeGoodThreshold then Good
            elif metrics.Wape <= policy.WapeWarningThreshold then Adequate
            else Poor

        // 2. Evaluate Forecast Bias component
        let biasState =
            let absBias = abs metrics.ForecastBias
            if absBias <= policy.BiasExcellentThreshold then Excellent
            elif absBias <= policy.BiasGoodThreshold then Good
            elif absBias <= policy.BiasWarningThreshold then Adequate
            else Poor

        // 3. Evaluate Forecast Accuracy component
        let accuracyState =
            if metrics.ForecastAccuracy >= policy.AccuracyExcellentThreshold then Excellent
            elif metrics.ForecastAccuracy >= policy.AccuracyGoodThreshold then Good
            elif metrics.ForecastAccuracy >= policy.AccuracyWarningThreshold then Adequate
            else Poor

        // Determine base state as the most severe across WAPE, Bias, and Accuracy
        let baseState =
            [ wapeState; biasState; accuracyState ]
            |> List.maxBy (function
                | Critical -> 5
                | Poor -> 4
                | Adequate -> 3
                | Good -> 2
                | Excellent -> 1)

        // If tracking signal tripped out-of-control limits, escalate quality state
        if metrics.IsOutOfControl then
            match baseState with
            | Excellent | Good | Adequate -> Poor
            | Poor -> Critical
            | Critical -> Critical
        else
            baseState

/// Generate comprehensive business explainability rationale for the quality assessment
let generateRationale
    (metrics: ForecastQualityMetrics)
    (state: QualityState)
    (policy: ForecastMeasurementPolicy)
    : string =

    let tsStr =
        metrics.TrackingSignal
        |> Option.map (fun ts -> sprintf "TrackingSignal=%.2f (Limit=%.1f, OutOfControl=%b)" ts policy.TrackingSignalLimit metrics.IsOutOfControl)
        |> Option.defaultValue "TrackingSignal=N/A"

    let fvaStr =
        metrics.ForecastValueAdd
        |> Option.map (fun fva -> sprintf "FVA=%.2f%%" fva)
        |> Option.defaultValue "FVA=None"

    let oeffStr =
        metrics.OverrideEffectiveness
        |> Option.map (fun oeff -> sprintf "OverrideEffectiveness=%.1f%%" oeff)
        |> Option.defaultValue "Overrides=None"

    sprintf
        "Forecast Quality %s: WAPE=%.2f%%, Bias=%.2f%%, Accuracy=%.2f%%, Completeness=%.1f%%, %s, %s, %s"
        state.AsString
        metrics.Wape
        metrics.ForecastBias
        metrics.ForecastAccuracy
        metrics.CompletenessScore
        tsStr
        fvaStr
        oeffStr
