/// CA-P-004 Evaluate Multi-Option ATP Policies
/// Traces to: PO-P-009
module Medhavi.Promise.EvaluateATP.Policies

type MultiOptionPromisingPolicy =
    { WeightSpeed: decimal
      WeightCost: decimal
      WeightMargin: decimal
      WeightSustainability: decimal
      MaxSplitShipments: int
      SubstitutionMarginFloorPct: decimal
      HandlingCostPerUnit: decimal }

let defaultMultiOptionPolicy: MultiOptionPromisingPolicy =
    { WeightSpeed = 0.35m
      WeightCost = 0.30m
      WeightMargin = 0.20m
      WeightSustainability = 0.15m
      MaxSplitShipments = 3
      SubstitutionMarginFloorPct = 15.0m
      HandlingCostPerUnit = 2.50m }

