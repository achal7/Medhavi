/// BA-D-005 — Compute Planning Classification Mathematical Algorithms
/// Traces to: BA-D-005, PO-D-035 (Specification Chapter 5)
module Medhavi.Demand.SegmentDemand.PlanningClassificationAssignment.Algorithms

open System
open MathNet.Numerics.Statistics
open Medhavi.Demand.SegmentDemand.PlanningClassificationAssignment.Model
open Medhavi.Demand.SegmentDemand.PlanningClassificationAssignment.Policies

type ClassificationComputation =
    { Classification: PlanningClassification
      Score: decimal
      Confidence: string
      Rationale: string }

/// Evaluates ABC volume/revenue cumulative Pareto threshold per PO-D-035
let computeAbcClassification
    (policy: SegmentationPolicy)
    (cumulativeVolumeOrRevenuePct: decimal option)
    : ClassificationComputation =
    match cumulativeVolumeOrRevenuePct with
    | None ->
        { Classification = Unclassified
          Score = 0.0m
          Confidence = "Low"
          Rationale = "No cumulative volume or revenue percentage provided" }
    | Some pct when pct < 0.0m || pct > 100.0m ->
        { Classification = Unclassified
          Score = pct
          Confidence = "Low"
          Rationale = $"Invalid cumulative percentage {pct}%% (must be between 0 and 100)" }
    | Some pct when pct <= policy.AbcClassACutoff ->
        { Classification = ClassA
          Score = pct
          Confidence = "High"
          Rationale =
            sprintf "Cumulative Pareto contribution %.2f%% is within Class A threshold (<= %.2f%%)" pct policy.AbcClassACutoff }
    | Some pct when pct <= policy.AbcClassBCutoff ->
        { Classification = ClassB
          Score = pct
          Confidence = "High"
          Rationale =
            sprintf "Cumulative Pareto contribution %.2f%% is within Class B threshold (<= %.2f%%)" pct policy.AbcClassBCutoff }
    | Some pct ->
        { Classification = ClassC
          Score = pct
          Confidence = "High"
          Rationale =
            sprintf "Cumulative Pareto contribution %.2f%% exceeds Class B threshold (> %.2f%%)" pct policy.AbcClassBCutoff }

/// Evaluates XYZ Demand Variability (Coefficient of Variation) using MathNet.Numerics Statistics
let computeXyzClassification
    (policy: SegmentationPolicy)
    (history: decimal list option)
    : ClassificationComputation =
    match history with
    | None ->
        { Classification = Unclassified
          Score = 0.0m
          Confidence = "Low"
          Rationale = "No historical demand series provided for XYZ variability evaluation" }
    | Some values when values.Length < policy.MinimumHistoryPeriods ->
        { Classification = Unclassified
          Score = 0.0m
          Confidence = "Low"
          Rationale =
            sprintf
                "Insufficient demand history (%d periods provided; minimum %d periods required per %s)"
                values.Length
                policy.MinimumHistoryPeriods
                policy.PolicyVersion }
    | Some values ->
        let floatArray = values |> List.map float |> Array.ofList
        let mean = Statistics.Mean(floatArray)

        if mean <= 0.0 then
            let allZero = values |> List.forall (fun v -> v = 0.0m)

            if allZero then
                { Classification = ClassZ
                  Score = 0.0m
                  Confidence = "Medium"
                  Rationale = "All demand values in historical series are zero (classified as Class Z lumpy/inactive)" }
            else
                { Classification = ClassZ
                  Score = 0.0m
                  Confidence = "Low"
                  Rationale = "Mean historical demand is non-positive; defaulted to Class Z" }
        else
            let stdDev = Statistics.StandardDeviation(floatArray)
            let cvFloat = stdDev / mean
            let cvDec = if Double.IsNaN(cvFloat) || Double.IsInfinity(cvFloat) then 99.99m else decimal cvFloat

            let confidence =
                if values.Length >= policy.MinimumHistoryPeriods * 2 then "High"
                else "Medium"

            if cvDec <= policy.XyzClassXCutoff then
                { Classification = ClassX
                  Score = cvDec
                  Confidence = confidence
                  Rationale =
                    sprintf "Coefficient of Variation (CV = %.4f) is within Class X threshold (<= %.2f)" cvDec policy.XyzClassXCutoff }
            elif cvDec <= policy.XyzClassYCutoff then
                { Classification = ClassY
                  Score = cvDec
                  Confidence = confidence
                  Rationale =
                    sprintf "Coefficient of Variation (CV = %.4f) is within Class Y threshold (<= %.2f)" cvDec policy.XyzClassYCutoff }
            else
                { Classification = ClassZ
                  Score = cvDec
                  Confidence = confidence
                  Rationale =
                    sprintf "Coefficient of Variation (CV = %.4f) exceeds Class Y threshold (> %.2f)" cvDec policy.XyzClassYCutoff }
