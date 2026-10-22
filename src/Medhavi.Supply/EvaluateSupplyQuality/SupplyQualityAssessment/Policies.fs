/// PO-S-110 — Supply Quality Policy
/// Traces to: Supply Intelligence Specification §8 (PO-S-110)
module Medhavi.Supply.EvaluateSupplyQuality.SupplyQualityAssessment.Policies

open System
open Model

type MetricThresholdConfig =
    { MetricName: string
      Target: decimal
      Stretch: decimal option
      Minimum: decimal option
      Critical: decimal option
      IsHigherBetter: bool
      IsEffectiveness: bool
      Weight: decimal }

type DimensionPolicyConfig =
    { Dimension: QualityDimensionType
      IsMandatory: bool
      ObjectiveDescription: string
      Metrics: MetricThresholdConfig list
      Weight: decimal }

type SupplyQualityPolicy =
    { PolicyVersion: string
      Dimensions: DimensionPolicyConfig list
      TrendDeadbandPercent: decimal
      MinimumPublicationConfidence: QualityConfidenceLevel
      AllowPublicationWithFlags: bool }

let defaultPolicy : SupplyQualityPolicy =
        { PolicyVersion = "PO-S-110.v1.0"
          TrendDeadbandPercent = 2.0m
          MinimumPublicationConfidence = QualityConfidenceLevel.Medium
          AllowPublicationWithFlags = true
          Dimensions =
            [ { Dimension = QualityDimensionType.Service
                IsMandatory = true
                ObjectiveDescription = "Service Level >= 95% (target 98%)"
                Weight = 0.25m
                Metrics =
                  [ { MetricName = "ServiceLevel"
                      Target = 95.0m
                      Stretch = Some 99.0m
                      Minimum = Some 90.0m
                      Critical = Some 80.0m
                      IsHigherBetter = true
                      IsEffectiveness = true
                      Weight = 1.0m } ] }
              { Dimension = QualityDimensionType.Inventory
                IsMandatory = true
                ObjectiveDescription = "Inventory Turns >= 8; Days of Supply within policy bounds"
                Weight = 0.20m
                Metrics =
                  [ { MetricName = "InventoryTurns"
                      Target = 8.0m
                      Stretch = Some 12.0m
                      Minimum = Some 6.0m
                      Critical = Some 4.0m
                      IsHigherBetter = true
                      IsEffectiveness = false
                      Weight = 1.0m } ] }
              { Dimension = QualityDimensionType.Capacity
                IsMandatory = true
                ObjectiveDescription = "Capacity Utilization within Operating Envelope for >= 90% of resources"
                Weight = 0.15m
                Metrics =
                  [ { MetricName = "CapacityUtilizationWithinEnvelope"
                      Target = 90.0m
                      Stretch = Some 100.0m
                      Minimum = Some 75.0m
                      Critical = Some 50.0m
                      IsHigherBetter = true
                      IsEffectiveness = false
                      Weight = 1.0m } ] }
              { Dimension = QualityDimensionType.SupplierCollaboration
                IsMandatory = true
                ObjectiveDescription = ">= 80% of active suppliers classified Good or Excellent"
                Weight = 0.15m
                Metrics =
                  [ { MetricName = "SupplierHealthRate"
                      Target = 80.0m
                      Stretch = Some 95.0m
                      Minimum = Some 60.0m
                      Critical = Some 40.0m
                      IsHigherBetter = true
                      IsEffectiveness = true
                      Weight = 1.0m } ] }
              { Dimension = QualityDimensionType.Procurement
                IsMandatory = true
                ObjectiveDescription = "Procurement Plan adherence >= 90%; >= 80% recommendations Feasible"
                Weight = 0.15m
                Metrics =
                  [ { MetricName = "ProcurementPlanAdherence"
                      Target = 90.0m
                      Stretch = Some 98.0m
                      Minimum = Some 75.0m
                      Critical = Some 60.0m
                      IsHigherBetter = true
                      IsEffectiveness = false
                      Weight = 1.0m } ] }
              { Dimension = QualityDimensionType.Distribution
                IsMandatory = true
                ObjectiveDescription = "Distribution Plan adherence >= 90%; Lane Utilization within capacity"
                Weight = 0.10m
                Metrics =
                  [ { MetricName = "DistributionPlanAdherence"
                      Target = 90.0m
                      Stretch = Some 98.0m
                      Minimum = Some 75.0m
                      Critical = Some 60.0m
                      IsHigherBetter = true
                      IsEffectiveness = false
                      Weight = 1.0m } ] }
              { Dimension = QualityDimensionType.PlanningStability
                IsMandatory = false
                ObjectiveDescription = "Plan Stability Score >= 70; < 10% frozen-period changes without approved exception"
                Weight = 0.05m
                Metrics =
                  [ { MetricName = "PlanStabilityScore"
                      Target = 70.0m
                      Stretch = Some 85.0m
                      Minimum = Some 50.0m
                      Critical = Some 30.0m
                      IsHigherBetter = true
                      IsEffectiveness = false
                      Weight = 1.0m } ] }
              { Dimension = QualityDimensionType.Sustainability
                IsMandatory = false
                ObjectiveDescription = "Carbon footprint <= 2.5 kg CO2e/unit; Supplier ESG compliance >= 85%"
                Weight = 0.05m
                Metrics =
                  [ { MetricName = "CarbonFootprint"
                      Target = 2.5m
                      Stretch = Some 2.0m
                      Minimum = Some 3.0m
                      Critical = Some 3.5m
                      IsHigherBetter = false
                      IsEffectiveness = true
                      Weight = 1.0m } ] } ] }
