/// PO-D-041 — Forecast Measurement Policy
/// Governs thresholds for WAPE, Bias, Tracking Signal, and Completeness
module Medhavi.Demand.EvaluateDemandQuality.ForecastQualityAssessment.Policies

/// PO-D-041: Forecast Measurement Policy
/// Enterprise governance rules establishing statistical accuracy targets, control limits, and publication criteria
type ForecastMeasurementPolicy =
    { PolicyId: string
      PolicyVersion: string
      Version: int
      /// Maximum WAPE for 'Excellent' quality state (e.g. <= 15%)
      WapeExcellentThreshold: decimal
      /// Maximum WAPE for 'Good' quality state (e.g. <= 25%)
      WapeGoodThreshold: decimal
      /// Maximum WAPE for 'Adequate' quality state before entering 'Poor' (e.g. <= 40%)
      WapeWarningThreshold: decimal
      /// Maximum absolute Forecast Bias for 'Excellent' (e.g. <= 5%)
      BiasExcellentThreshold: decimal
      /// Maximum absolute Forecast Bias for 'Good' (e.g. <= 10%)
      BiasGoodThreshold: decimal
      /// Maximum absolute Forecast Bias for 'Adequate' (e.g. <= 20%)
      BiasWarningThreshold: decimal
      /// Minimum Forecast Accuracy for 'Excellent' (e.g. >= 85%)
      AccuracyExcellentThreshold: decimal
      /// Minimum Forecast Accuracy for 'Good' (e.g. >= 75%)
      AccuracyGoodThreshold: decimal
      /// Minimum Forecast Accuracy for 'Adequate' (e.g. >= 60%)
      AccuracyWarningThreshold: decimal
      /// Tracking Signal control limit (|TS| > limit indicates out-of-control model drift; standard = 4.0)
      TrackingSignalLimit: decimal
      /// Minimum percentage of periods with materialized actuals required for authoritative publication (e.g. >= 80%)
      MinCompletenessThreshold: decimal
      /// Minimum evaluation period duration in days (e.g. >= 7 days)
      MinEvaluationPeriodDays: decimal }

module ForecastMeasurementPolicy =
    let defaultPolicy: ForecastMeasurementPolicy =
        { PolicyId = "PO-D-041"
          PolicyVersion = "PO-D-041:v1.0"
          Version = 1
          WapeExcellentThreshold = 15.0m
          WapeGoodThreshold = 25.0m
          WapeWarningThreshold = 40.0m
          BiasExcellentThreshold = 5.0m
          BiasGoodThreshold = 10.0m
          BiasWarningThreshold = 20.0m
          AccuracyExcellentThreshold = 85.0m
          AccuracyGoodThreshold = 75.0m
          AccuracyWarningThreshold = 60.0m
          TrackingSignalLimit = 4.0m
          MinCompletenessThreshold = 80.0m
          MinEvaluationPeriodDays = 7.0m }
