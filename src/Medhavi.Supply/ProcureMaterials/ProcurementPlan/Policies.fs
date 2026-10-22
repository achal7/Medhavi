namespace Medhavi.Supply.ProcureMaterials.ProcurementPlan

open System

type PlanPublicationPolicyConfig =
    { GovernedConfidenceThreshold: decimal
      MinimumConfidenceThreshold: decimal
      RequireCompleteCoverage: bool }

module Policies =
    let defaultConfig : PlanPublicationPolicyConfig =
        { GovernedConfidenceThreshold = 0.75m
          MinimumConfidenceThreshold = 0.50m
          RequireCompleteCoverage = true }
