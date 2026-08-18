/// BA-D-006 — Determine Demand Behavior Classification
module Medhavi.Demand.ClassifyDemand.DemandBehaviorAssignment.Algorithms

open MathNet.Numerics.Statistics
open Policies
open Model

/// Compute statistical features from demand series in chronological order
let computeStatisticalFeatures (quantities: decimal list) (seasonalLag: int) : StatisticalFeatures =
    let n = quantities.Length
    let floatValues = quantities |> List.map float |> Array.ofList

    let cv, cv2 =
        if n > 1 then
            let mean = Statistics.Mean floatValues

            if mean > 0.0 then
                let stdDev = Statistics.StandardDeviation floatValues
                let c = stdDev / mean
                decimal c, decimal(c * c)
            else
                0.0m, 0.0m
        else
            0.0m, 0.0m

    let zeroCount = quantities |> List.filter(fun q -> q = 0m) |> List.length
    let nonZeroCount = n - zeroCount
    let zeroRatio = if n > 0 then decimal zeroCount / decimal n else 0.0m

    let adi = if nonZeroCount > 0 then decimal n / decimal nonZeroCount else decimal n

    let autocorr =
        if n > seasonalLag + 3 && seasonalLag > 0 then
            let x = floatValues[.. (n - seasonalLag - 1)]
            let y = floatValues[seasonalLag..]
            let r = Correlation.Pearson(x, y)

            if System.Double.IsNaN r || System.Double.IsInfinity r then
                None
            else
                Some(decimal r)
        else
            None

    let trendP =
        if n > 3 then
            let x = Array.init n float
            let y = floatValues
            let struct (intercept, slope) = MathNet.Numerics.LinearRegression.SimpleRegression.Fit(x, y)
            let residuals = y |> Array.mapi(fun i yi -> yi - (intercept + slope * x[i]))
            let rss = residuals |> Array.sumBy(fun r -> r * r)
            let xMean = Array.average x
            let ssxx = x |> Array.sumBy(fun xi -> (xi - xMean) ** 2.0)
            let df = float(n - 2)

            if ssxx > 0.0 && df > 0.0 && rss >= 0.0 then
                let seSlope = sqrt(rss / (df * ssxx))

                if seSlope > 0.0 then
                    let tStat = slope / seSlope
                    let pValue = 2.0 * (1.0 - MathNet.Numerics.Distributions.StudentT.CDF(0.0, 1.0, df, abs tStat))

                    if System.Double.IsNaN pValue || System.Double.IsInfinity pValue then
                        None
                    else
                        Some(decimal pValue)
                else
                    None
            else
                None
        else
            None

    { CoefficientOfVariation = cv
      SquaredCoefficientOfVariation = cv2
      AverageDemandInterval = adi
      AutocorrelationAtSeasonalLag = autocorr
      TrendPValue = trendP
      ZeroDemandRatio = zeroRatio
      SamplePeriodCount = n }

/// Determine the behavior class using Syntetos-Boylan matrix and policy thresholds (BA-D-006)
let determineClass (features: StatisticalFeatures) (policy: ClassificationPolicy) : BehaviorClass =
    if features.SamplePeriodCount < policy.MinimumHistoryDataPoints then
        Unclassified
    elif
        features.AutocorrelationAtSeasonalLag |> Option.exists(fun ac -> ac >= policy.SeasonalAutocorrelationThreshold)
    then
        Seasonal
    elif
        features.SquaredCoefficientOfVariation > policy.LumpyCv2Threshold
        && features.AverageDemandInterval > policy.IntermittentAdiThreshold
    then
        Lumpy
    elif features.AverageDemandInterval > policy.IntermittentAdiThreshold then
        Intermittent
    elif features.TrendPValue |> Option.exists(fun p -> p <= policy.TrendPValueThreshold) then
        Trend
    else
        Continuous

/// Produce a human-readable rationale and confidence level
let classificationRationale (cls: BehaviorClass) (features: StatisticalFeatures) : string * string =
    match cls with
    | Continuous ->
        sprintf
            "Continuous demand: ADI=%.2f <= threshold, CV²=%.2f <= threshold across %d periods"
            features.AverageDemandInterval
            features.SquaredCoefficientOfVariation
            features.SamplePeriodCount,
        "High"
    | Intermittent ->
        sprintf
            "Intermittent demand: ADI=%.2f > threshold (infrequent demand) with stable size CV²=%.2f"
            features.AverageDemandInterval
            features.SquaredCoefficientOfVariation,
        "Medium"
    | Seasonal ->
        let acStr = features.AutocorrelationAtSeasonalLag |> Option.map(sprintf "%.2f") |> Option.defaultValue "N/A"
        sprintf "Seasonal demand: significant autocorrelation (%s) at seasonal lag" acStr, "High"
    | Lumpy ->
        sprintf
            "Lumpy demand: high variability (CV²=%.2f) combined with intermittent intervals (ADI=%.2f)"
            features.SquaredCoefficientOfVariation
            features.AverageDemandInterval,
        "Medium"
    | Trend ->
        let pStr = features.TrendPValue |> Option.map(sprintf "%.4f") |> Option.defaultValue "N/A"
        sprintf "Trending demand: statistically significant trend detected (p=%s)" pStr, "Medium"
    | Unclassified ->
        sprintf "Unclassified: insufficient historical sample (%d < minimum required)" features.SamplePeriodCount, "Low"
