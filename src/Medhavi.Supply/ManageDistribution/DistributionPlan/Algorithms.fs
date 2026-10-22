/// BA-S-051 — Compose Distribution Plan Baseline Algorithm
/// Traces to: Supply Intelligence Specification §10 (BA-S-051)
module Medhavi.Supply.ManageDistribution.DistributionPlan.Algorithms

open System
open Medhavi.SemanticModel
open Medhavi.Supply
open Model
open Policies

type PlanCompositionInput =
    { Recommendations: ActiveDistributionRecommendationFact list
      Policy: DistributionPlanPublicationPolicyParameters
      PreviousPlan: DistributionPlanAggregate option }

type PlanCompositionResult =
    { TotalMovements: int
      TotalQuantityMoved: Quantity
      LaneUtilizationSummary: LaneUtilizationSummary list
      NetworkBalanceSummary: NetworkBalanceSummary
      FeasibilitySummary: FeasibilitySummary
      StabilitySummary: StabilitySummary
      ConfidenceDistribution: ConfidenceDistribution
      RiskSignals: RiskSignal list
      RecoveryOpportunities: RecoveryOpportunity list
      RecommendationSignals: RecommendationSignal list }

/// BA-S-051 — Execute pure functional composition of the distribution plan baseline from active facts
let compose (input: PlanCompositionInput) : PlanCompositionResult =
    let recs = input.Recommendations
    let recCount = recs.Length

    // 1. Total movements and quantity from decoupled facts
    let allAllocations = recs |> List.collect (fun r -> r.SourceLocations)
    let totalMovements = allAllocations.Length

    let totalQtyMovedVal =
        allAllocations
        |> List.sumBy (fun (_, qty, _) -> Quantity.value qty)

    let totalQtyMoved = Quantity.create totalQtyMovedVal |> Result.defaultValue Quantity.zero

    // 2. Lane utilization summary from decoupled facts
    let allLaneUsages = recs |> List.collect (fun r -> r.LanesUsed)
    let laneGroups = allLaneUsages |> List.groupBy (fun (lane, _, _) -> lane)

    let laneSummaries =
        laneGroups
        |> List.map (fun (lane, usages) ->
            let totalMovs = usages.Length
            let avgUtil =
                if usages.IsEmpty then 0.0m
                else usages |> List.averageBy (fun (_, _, util) -> util)

            let laneQty =
                recs
                |> List.filter (fun r -> r.LanesUsed |> List.exists (fun (l, _, _) -> l = lane))
                |> List.sumBy (fun r -> r.SourceLocations |> List.sumBy (fun (_, qty, _) -> Quantity.value qty))

            { Lane = lane
              TotalMovements = totalMovs
              TotalQuantity = Quantity.create laneQty |> Result.defaultValue Quantity.zero
              UtilizationPercentage = Math.Round(avgUtil * 100.0m, 2) })

    // 3. Network balance summary
    let netBalanceSummary: NetworkBalanceSummary =
        { TotalInbound = totalQtyMoved
          TotalOutbound = totalQtyMoved
          NetBalance = Quantity.zero }

    // 4. Feasibility summary from decoupled facts
    let feasibleCount =
        recs
        |> List.filter (fun r -> r.Feasibility = DistributionFeasibility.FullyFeasible)
        |> List.length

    let conditionalCount =
        recs
        |> List.filter (fun r -> r.Feasibility = DistributionFeasibility.ConditionallyFeasible)
        |> List.length

    let infeasibleCount =
        recs
        |> List.filter (fun r -> r.Feasibility = DistributionFeasibility.Infeasible)
        |> List.length

    let feasibilityPercentage =
        if recCount > 0 then
            let effectiveFeasible = decimal feasibleCount + (decimal conditionalCount * 0.5m)
            Math.Round((effectiveFeasible / decimal recCount) * 100.0m, 2)
        else 100.0m

    let feasSummary: FeasibilitySummary =
        { FeasibleCount = feasibleCount
          ConditionallyFeasibleCount = conditionalCount
          InfeasibleCount = infeasibleCount
          OverallFeasibilityPercentage = feasibilityPercentage }

    // 5. Stability summary from decoupled facts
    let avgStability =
        if recCount > 0 then
            Math.Round(recs |> List.averageBy (fun r -> r.StabilityScore), 2)
        else 100.0m

    let totalChanges = recs |> List.sumBy (fun r -> r.StabilityChangeCount)

    let highVolatilityCount =
        recs
        |> List.filter (fun r -> r.StabilityScore < input.Policy.MinimumAverageStabilityScore)
        |> List.length

    let stabSummary: StabilitySummary =
        { AverageStabilityScore = avgStability
          TotalChangeCount = totalChanges
          HighVolatilityCount = highVolatilityCount }

    // 6. Confidence distribution from decoupled facts
    let highConf =
        recs
        |> List.filter (fun r -> r.Confidence = DistributionConfidence.High)
        |> List.length

    let medConf =
        recs
        |> List.filter (fun r -> r.Confidence = DistributionConfidence.Medium)
        |> List.length

    let lowConf =
        recs
        |> List.filter (fun r -> r.Confidence = DistributionConfidence.Low)
        |> List.length

    let overallPlanConf =
        if recCount = 0 then DistributionConfidence.High
        elif decimal highConf / decimal recCount >= 0.70m then DistributionConfidence.High
        elif decimal lowConf / decimal recCount <= 0.20m then DistributionConfidence.Medium
        else DistributionConfidence.Low

    let confDist: ConfidenceDistribution =
        { HighConfidenceCount = highConf
          MediumConfidenceCount = medConf
          LowConfidenceCount = lowConf
          Overall = overallPlanConf }

    // 7. Aggregated risks and recovery opportunities (pure functional list comprehensions)
    let aggregatedRisks =
        [ if infeasibleCount > 0 then
            yield { RiskType = "InfeasibleDistributionMovements"
                    Severity = RiskSeverity.High
                    Description = sprintf "%d movement recommendations have unresolved feasibility violations" infeasibleCount
                    AffectedMovementCount = infeasibleCount }
          if highVolatilityCount > input.Policy.MaximumAllowedHighVolatilityMovements then
            yield { RiskType = "HighNetworkVolatility"
                    Severity = RiskSeverity.Medium
                    Description = sprintf "%d movement recommendations exceed the volatility threshold" highVolatilityCount
                    AffectedMovementCount = highVolatilityCount } ]

    let recoveryOps =
        recs
        |> List.filter (fun r -> r.Feasibility = DistributionFeasibility.ConditionallyFeasible)
        |> List.choose (fun r ->
            match r.LanesUsed, r.AlternativeStrategy with
            | (currentLane, _, _) :: _, Some _ ->
                Some { MovementReference = DistributionRequirementId.value r.RequirementId
                       CurrentLane = currentLane
                       AlternativeLane = currentLane
                       EstimatedImprovement = "Alternative lane diversion restores full feasibility"
                       RecoveryStrategy = "Carrier Reroute" }
            | _ -> None)

    let recommendationSignals =
        [ if conditionalCount > 0 then
            yield { SignalType = "ReceivingCongestionFlagged"
                    Description = sprintf "%d movements require dock congestion mitigation" conditionalCount
                    AffectedMovementCount = conditionalCount } ]

    { TotalMovements = totalMovements
      TotalQuantityMoved = totalQtyMoved
      LaneUtilizationSummary = laneSummaries
      NetworkBalanceSummary = netBalanceSummary
      FeasibilitySummary = feasSummary
      StabilitySummary = stabSummary
      ConfidenceDistribution = confDist
      RiskSignals = aggregatedRisks
      RecoveryOpportunities = recoveryOps
      RecommendationSignals = recommendationSignals }
