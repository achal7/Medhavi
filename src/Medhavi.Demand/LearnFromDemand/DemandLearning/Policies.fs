/// Learn From Demand Policies
/// Traces to: PO-D-048
module Medhavi.Demand.LearnFromDemand.DemandLearning.Policies

/// PO-D-048: Learning Analysis Policy
/// Governs recurrence thresholds, minimum sample sizes, statistical significance, and confidence criteria
type LearningAnalysisPolicy =
    { PolicyId: string
      Version: int
      MinRecurrencePeriods: int
      MinSampleSize: int
      PatternConfidenceHigh: decimal
      PatternConfidenceMedium: decimal
      InterventionConfidenceHigh: decimal
      InterventionConfidenceMedium: decimal
      StatisticalSignificanceThreshold: decimal
      TrendMinSlopeForDegradation: decimal
      MaxOverrideNegativeFvaRatio: decimal
      MinAccuracyLiftForClassification: decimal
      MaxRecurrentExceptionCount: int }

module LearningAnalysisPolicy =
    let defaultPolicy: LearningAnalysisPolicy =
        { PolicyId = "PO-D-048"
          Version = 1
          MinRecurrencePeriods = 3
          MinSampleSize = 12
          PatternConfidenceHigh = 0.80m
          PatternConfidenceMedium = 0.60m
          InterventionConfidenceHigh = 0.75m
          InterventionConfidenceMedium = 0.50m
          StatisticalSignificanceThreshold = 0.05m
          TrendMinSlopeForDegradation = 0.10m
          MaxOverrideNegativeFvaRatio = 0.60m
          MinAccuracyLiftForClassification = 2.0m
          MaxRecurrentExceptionCount = 3 }
