namespace Medhavi.Supply.ProcureMaterials.ProcurementRecommendation

open System

type ProcurementPolicyConfig =
    { AutoApprovalSpendLimit: decimal
      ExecutiveEscalationSpendLimit: decimal
      MaxRiskScoreForAutonomous: decimal
      MaxInducedExcessRatioForAutonomous: decimal
      GracePeriodDays: decimal
      BundlingHorizonDays: decimal
      TierBreakQuantity: decimal
      HoldingHorizonDays: decimal
      DualSourcingMandatoryItemCategories: Set<string>
      ConfidenceThreshold: decimal
      MinimumConfidenceThreshold: decimal }

module Policies =
    let defaultConfig : ProcurementPolicyConfig =
        { AutoApprovalSpendLimit = 50000.0m
          ExecutiveEscalationSpendLimit = 250000.0m
          MaxRiskScoreForAutonomous = 30.0m
          MaxInducedExcessRatioForAutonomous = 0.20m
          GracePeriodDays = 2.0m
          BundlingHorizonDays = 14.0m
          TierBreakQuantity = 5000.0m
          HoldingHorizonDays = 3.0m
          DualSourcingMandatoryItemCategories = Set.ofList [ "CRITICAL_CHEMICAL"; "ACTIVE_INGREDIENT"; "HIGH_RISK" ]
          ConfidenceThreshold = 0.75m
          MinimumConfidenceThreshold = 0.50m }
