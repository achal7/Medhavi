/// Model Demand Interventions Policies
/// Traces to: PO-D-050 (Specification Chapter 8)
module Medhavi.Demand.ModelDemandInterventions.DemandInterventionImpact.Policies

open Model

/// PO-D-050: Intervention Modeling Governance Policy
/// Governs modeling approach preference, confidence thresholds, historical data sufficiency, and temporal bounds
type InterventionModelingGovernancePolicy =
    { PolicyId: string
      Version: int
      PublicationConfidenceThreshold: decimal
      MinHistoricalPeriodsForElasticity: int
      ModelingApproachPreferenceOrder: ModelingApproach list
      MaxTemporalValidityDays: int
      DefaultPriceElasticity: decimal
      DefaultPromotionLiftMultiplier: decimal }

module InterventionModelingGovernancePolicy =
    let defaultPolicy: InterventionModelingGovernancePolicy =
        { PolicyId = "PO-D-050"
          Version = 1
          PublicationConfidenceThreshold = 0.70m
          MinHistoricalPeriodsForElasticity = 12
          ModelingApproachPreferenceOrder = [ HistoricalElasticity; AnalogBased; ExpertJudgment ]
          MaxTemporalValidityDays = 90
          DefaultPriceElasticity = -1.5m
          DefaultPromotionLiftMultiplier = 1.35m }
