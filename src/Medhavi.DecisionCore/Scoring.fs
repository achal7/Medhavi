namespace Medhavi.DecisionCore

open Medhavi.SharedKernel

type PlanScore =
    { TotalCost: PositiveDecimal
      ServiceLevel: PositiveDecimal
      CapacityUtilization: PositiveDecimal
      LatenessPenalty: PositiveDecimal
      RiskScore: PositiveDecimal }

    static member Default() =
        { TotalCost = PositiveDecimal.Zero
          ServiceLevel = PositiveDecimal.Zero
          CapacityUtilization = PositiveDecimal.Zero
          LatenessPenalty = PositiveDecimal.Zero
          RiskScore = PositiveDecimal.Zero }

type ScoreWeights =
    { CostWeight: PositiveDecimal
      ServiceWeight: PositiveDecimal
      CapacityWeight: PositiveDecimal
      RiskWeight: PositiveDecimal }

type PlanScoreCard =
    { VariantId: string
      Score: PlanScore
      WeightedTotal: decimal
      Rank: int }

module PlanScore =

    let combineScores (a: PlanScore) (b: PlanScore) : PlanScore =
        { TotalCost = a.TotalCost + b.TotalCost
          ServiceLevel = a.ServiceLevel + b.ServiceLevel // Note: may need averaging later
          CapacityUtilization = a.CapacityUtilization + b.CapacityUtilization
          LatenessPenalty = a.LatenessPenalty + b.LatenessPenalty
          RiskScore = a.RiskScore + b.RiskScore }

    let weightedObjectiveScore (score: PlanScore) (weights: ScoreWeights) : decimal =
        // For cost and lateness, lower is better → negative contribution
        // For service and utilization, higher is better → positive contribution
        (PositiveDecimal.value score.TotalCost * -(PositiveDecimal.value weights.CostWeight))
        + (PositiveDecimal.value score.ServiceLevel * (PositiveDecimal.value weights.ServiceWeight))
        + (PositiveDecimal.value score.CapacityUtilization * (PositiveDecimal.value weights.CapacityWeight))
        + (PositiveDecimal.value score.LatenessPenalty * -(PositiveDecimal.value weights.CostWeight))
        + (PositiveDecimal.value score.RiskScore * -(PositiveDecimal.value weights.RiskWeight))

    let candidateRanking (candidates: (string * PlanScore) list) (weights: ScoreWeights) : PlanScoreCard list =
        candidates
        |> List.map(fun (variantId, score) ->
            { VariantId = variantId
              Score = score
              WeightedTotal = weightedObjectiveScore score weights
              Rank = 0 }) // temporary; will be set after sorting
        |> List.sortByDescending(fun card -> card.WeightedTotal)
        |> List.mapi(fun i card -> { card with Rank = i + 1 })

    let cardComparison (a: PlanScoreCard) (b: PlanScoreCard) : int = compare b.WeightedTotal a.WeightedTotal // descending by weighted total
