/// BA-D-010 & BA-D-011 — Demand Exception Evaluation & Lifecycle Assessment Engine
/// Pure mathematical algorithms implementing threshold breaching, statistical anomaly detection, and resolution hysteresis
module Medhavi.Demand.DetectDemandExceptions.DemandExceptionEvidence.Algorithms

open Model
open Policies

// =============================================================================
// DOMAIN KNOWLEDGE & EXCEPTION INTELLIGENCE:
//
// 1. Exception Detection Criteria (BA-D-010):
//    - Forecast Bias: Evaluates systemic directional error (|Bias| > threshold).
//    - Forecast Accuracy: Evaluates extreme degradation (WAPE > threshold).
//    - Completeness Gap: Evaluates missing actuals (Completeness < threshold).
//    - Demand Behavior Critical: Evaluates sudden demand structural regime shifts.
//    - Tracking Signal Drift: Evaluates statistical process control limits (|TS| > 4.0).
//    - Negative FVA: Evaluates destructive planner touches (FVA < threshold).
//
// 2. Statistical Z-Score Anomaly Detection:
//    Z = (Value - Mean(History)) / StdDev(History)
//    - When historical values exist, Z-score quantifies how unusual the current
//      metric deviation is compared to past cycles.
//
// 3. Hysteresis Recovery (BA-D-011):
//    To prevent "alert fatigue" and rapid toggling between active/resolved states
//    when a metric oscillates around a threshold, a recovery hysteresis band
//    (e.g. 80% of threshold) is enforced before an exception is declared resolved.
// =============================================================================

/// Compute mean and sample standard deviation of a series
let private calculateStats (values: decimal list) : (decimal * decimal) option =
    if values.Length < 3 then
        None
    else
        let n = decimal values.Length
        let mean = (values |> List.sum) / n
        let sumSquaredDiffs = values |> List.sumBy (fun v -> (v - mean) * (v - mean))
        let variance = sumSquaredDiffs / (n - 1.0m)
        let stdDev = decimal (sqrt (double variance))
        Some (mean, stdDev)

/// Compute statistical Z-score
let computeZScore (current: decimal) (history: decimal list) : decimal option =
    match calculateStats history with
    | Some (mean, stdDev) when stdDev > 0.0m ->
        Some ((current - mean) / stdDev)
    | _ -> None

/// Evaluates whether a metric breaches governed detection thresholds and assigns severity (BA-D-010)
let evaluateBreach
    (exceptionType: DemandExceptionType)
    (metricValue: decimal)
    (policy: DemandExceptionEvidencePolicy)
    : (bool * DemandExceptionSeverity option * decimal * string) =

    match exceptionType with
    | ForecastBiasElevated ->
        let absBias = abs metricValue
        if absBias >= policy.ForecastBiasCriticalThreshold then
            true, Some Critical, policy.ForecastBiasCriticalThreshold,
            sprintf "Critical Forecast Bias: %.2f%% breaches critical threshold (%.2f%%)" metricValue policy.ForecastBiasCriticalThreshold
        elif absBias >= policy.ForecastBiasHighThreshold then
            true, Some High, policy.ForecastBiasHighThreshold,
            sprintf "High Forecast Bias: %.2f%% breaches high threshold (%.2f%%)" metricValue policy.ForecastBiasHighThreshold
        elif absBias >= policy.ForecastBiasMediumThreshold then
            true, Some Medium, policy.ForecastBiasMediumThreshold,
            sprintf "Medium Forecast Bias: %.2f%% breaches warning threshold (%.2f%%)" metricValue policy.ForecastBiasMediumThreshold
        else
            false, None, policy.ForecastBiasMediumThreshold,
            sprintf "Forecast Bias %.2f%% is within acceptable bounds (< %.2f%%)" metricValue policy.ForecastBiasMediumThreshold

    | ForecastAccuracyDegraded ->
        if metricValue >= policy.WapeCriticalThreshold then
            true, Some Critical, policy.WapeCriticalThreshold,
            sprintf "Critical WAPE Degradation: %.2f%% breaches critical threshold (%.2f%%)" metricValue policy.WapeCriticalThreshold
        elif metricValue >= policy.WapeHighThreshold then
            true, Some High, policy.WapeHighThreshold,
            sprintf "High WAPE Degradation: %.2f%% breaches high threshold (%.2f%%)" metricValue policy.WapeHighThreshold
        elif metricValue >= policy.WapeMediumThreshold then
            true, Some Medium, policy.WapeMediumThreshold,
            sprintf "Medium WAPE Degradation: %.2f%% breaches warning threshold (%.2f%%)" metricValue policy.WapeMediumThreshold
        else
            false, None, policy.WapeMediumThreshold,
            sprintf "WAPE %.2f%% is within acceptable bounds (< %.2f%%)" metricValue policy.WapeMediumThreshold

    | DataCompletenessGap ->
        if metricValue <= policy.CompletenessCriticalThreshold then
            true, Some Critical, policy.CompletenessCriticalThreshold,
            sprintf "Critical Data Completeness Gap: %.2f%% is below critical cutoff (%.2f%%)" metricValue policy.CompletenessCriticalThreshold
        elif metricValue <= policy.CompletenessHighThreshold then
            true, Some High, policy.CompletenessHighThreshold,
            sprintf "High Data Completeness Gap: %.2f%% is below high cutoff (%.2f%%)" metricValue policy.CompletenessHighThreshold
        elif metricValue <= policy.CompletenessMediumThreshold then
            true, Some Medium, policy.CompletenessMediumThreshold,
            sprintf "Medium Data Completeness Gap: %.2f%% is below warning cutoff (%.2f%%)" metricValue policy.CompletenessMediumThreshold
        else
            false, None, policy.CompletenessMediumThreshold,
            sprintf "Data Completeness %.2f%% meets requirement (>= %.2f%%)" metricValue policy.CompletenessMediumThreshold

    | TrackingSignalDrift ->
        let absTs = abs metricValue
        if absTs >= policy.TrackingSignalCriticalLimit then
            true, Some Critical, policy.TrackingSignalCriticalLimit,
            sprintf "Critical Tracking Signal Drift: %.2f exceeds critical limit (%.2f)" metricValue policy.TrackingSignalCriticalLimit
        elif absTs >= policy.TrackingSignalHighLimit then
            true, Some High, policy.TrackingSignalHighLimit,
            sprintf "High Tracking Signal Drift: %.2f exceeds statistical process limit (%.2f)" metricValue policy.TrackingSignalHighLimit
        else
            false, None, policy.TrackingSignalHighLimit,
            sprintf "Tracking Signal %.2f is within statistical control (< %.2f)" metricValue policy.TrackingSignalHighLimit

    | NegativeForecastValueAdd ->
        if metricValue <= policy.NegativeFvaCriticalThreshold then
            true, Some Critical, policy.NegativeFvaCriticalThreshold,
            sprintf "Critical Value-Destroying Overrides: FVA %.2f%% breaches critical threshold (%.2f%%)" metricValue policy.NegativeFvaCriticalThreshold
        elif metricValue <= policy.NegativeFvaHighThreshold then
            true, Some High, policy.NegativeFvaHighThreshold,
            sprintf "High Value-Destroying Overrides: FVA %.2f%% breaches high threshold (%.2f%%)" metricValue policy.NegativeFvaHighThreshold
        else
            false, None, policy.NegativeFvaHighThreshold,
            sprintf "Forecast Value Add %.2f%% is acceptable (>= %.2f%%)" metricValue policy.NegativeFvaHighThreshold

    | DemandBehaviorCritical ->
        if metricValue >= 1.0m then
            true, Some Critical, 1.0m, "Critical Demand Behavior state detected by Demand Sensing"
        else
            false, None, 1.0m, "Demand Behavior is within normal bounds"

/// Assesses lifecycle transition (Detection vs. Resolution vs. No Evidence) per BA-D-011
let assessLifecycle
    (isCurrentlyActive: bool)
    (exceptionType: DemandExceptionType)
    (metricValue: decimal)
    (historicalValues: decimal list)
    (policy: DemandExceptionEvidencePolicy)
    : LifecycleDetermination =

    let isBreached, severityOpt, threshold, baseRationale =
        evaluateBreach exceptionType metricValue policy

    let zScoreStr =
        computeZScore metricValue historicalValues
        |> Option.map (fun z -> sprintf " (Anomaly Z-Score: %.2fσ)" z)
        |> Option.defaultValue ""

    let fullRationale = baseRationale + zScoreStr

    if isBreached then
        match severityOpt with
        | Some sev -> DetectionEvidenceExists(sev, exceptionType.AsString, metricValue, threshold, fullRationale)
        | None -> NoEvidence
    elif isCurrentlyActive then
        // Check hysteresis recovery: value has returned safely past the recovery factor
        let recoveryThreshold =
            match exceptionType with
            | DataCompletenessGap -> threshold / policy.HysteresisRecoveryFactor // For completeness, higher is better
            | _ -> threshold * policy.HysteresisRecoveryFactor // For error/bias, lower is better

        let isRecovered =
            match exceptionType with
            | DataCompletenessGap -> metricValue >= recoveryThreshold
            | _ -> abs metricValue <= recoveryThreshold

        if isRecovered then
            ResolutionEvidenceExists(
                exceptionType.AsString,
                metricValue,
                threshold,
                sprintf "Resolved: %s normalized to %.2f (recovery threshold %.2f)" exceptionType.AsString metricValue recoveryThreshold)
        else
            // In hysteresis deadband; maintain active status
            NoEvidence
    else
        NoEvidence
