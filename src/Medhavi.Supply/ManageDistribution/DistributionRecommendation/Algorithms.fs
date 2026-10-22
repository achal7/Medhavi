/// BA-S-050 — Construct Distribution Recommendation Algorithm
/// Traces to: Supply Intelligence Specification §10 (BA-S-050)
module Medhavi.Supply.ManageDistribution.DistributionRecommendation.Algorithms

open System
open Medhavi.SemanticModel
open Medhavi.Supply
open Model
open Policies

type ConstructionInput =
    { Requirement: TransferRequirementFact
      CandidateLanes: CandidateLaneFact list
      SourceInventoryPositions: Map<LocationId, decimal>
      DockCapacity: DockCapacityFact option
      Policy: DistributionPolicyParameters
      EvaluationTimestamp: Timestamp
      PreviousRecommendation: DistributionRecommendationAggregate option }

type ConstructionResult =
    { Strategy: DistributionStrategy
      AlternativeStrategy: DistributionStrategy option
      AllocationPolicyUsed: AllocationPolicy option
      AllocationOutcome: AllocationOutcome option
      SourceLocations: SourceLocationAllocation list
      LanesUsed: LaneUsage list
      DepartureTiming: Timestamp
      ExpectedArrivalTiming: Timestamp
      Feasibility: FeasibilityEvaluation
      FeasibilityConstraints: FeasibilityConstraint list
      Confidence: ConfidenceEvaluation
      Stability: StabilityMetrics
      RiskSignals: RiskSignal list
      ReasonCodes: DistributionReasonCode list
      Rationale: string
      EvidenceReferences: string list }

let private computeStability
    (prevOpt: DistributionRecommendationAggregate option)
    (newQty: Quantity)
    (newDeparture: Timestamp)
    (newLanes: LaneUsage list)
    : StabilityMetrics =
    match prevOpt with
    | None ->
        { StabilityScore = 100.0m
          ChangeCount = 0
          ChangeMagnitudeQuantity = 0.0m
          ChangeMagnitudeTimingHours = 0.0m
          LaneChangesCount = 0 }
    | Some prev ->
        let qtyDiff = abs (Quantity.value newQty - Quantity.value prev.RequiredQuantity)
        let timeDiffHours = abs ((Timestamp.value newDeparture - Timestamp.value prev.DepartureTiming).TotalHours |> decimal)
        let prevLanes = prev.LanesUsed |> List.map (fun l -> l.Lane) |> Set.ofList
        let currentLanes = newLanes |> List.map (fun l -> l.Lane) |> Set.ofList
        let laneChanges = Set.difference currentLanes prevLanes |> Set.count

        let qtyScore = max 0.0m (100.0m - (if Quantity.value prev.RequiredQuantity > 0m then (qtyDiff / Quantity.value prev.RequiredQuantity) * 100.0m else 0m))
        let timeScore = max 0.0m (100.0m - (timeDiffHours * 2.0m))
        let laneScore = if laneChanges = 0 then 100.0m else max 0.0m (100.0m - decimal laneChanges * 25.0m)
        let stabilityScore = (qtyScore * 0.4m) + (timeScore * 0.3m) + (laneScore * 0.3m)

        let changes =
            (if qtyDiff > 0m then 1 else 0)
            + (if timeDiffHours > 0m then 1 else 0)
            + laneChanges

        { StabilityScore = Math.Round(stabilityScore, 2)
          ChangeCount = changes
          ChangeMagnitudeQuantity = qtyDiff
          ChangeMagnitudeTimingHours = timeDiffHours
          LaneChangesCount = laneChanges }

let private evaluateConfidence
    (inventoryAvail: decimal)
    (reqQty: decimal)
    (dockCapOpt: DockCapacityFact option)
    (lane: CandidateLaneFact option)
    (policy: DistributionPolicyParameters)
    : ConfidenceEvaluation =
    let invCertainty =
        if inventoryAvail >= reqQty * 1.2m then DistributionConfidence.High
        elif inventoryAvail >= reqQty then DistributionConfidence.Medium
        else DistributionConfidence.Low

    let laneRel =
        match lane with
        | Some l when l.DailyCapacity >= reqQty * 1.5m -> DistributionConfidence.High
        | Some l when l.DailyCapacity >= reqQty -> DistributionConfidence.Medium
        | _ -> DistributionConfidence.Low

    let transitAcc =
        match lane with
        | Some l when l.TransitTimeHours <= 48.0m -> DistributionConfidence.High
        | Some _ -> DistributionConfidence.Medium
        | None -> DistributionConfidence.Low

    let dockConf =
        match dockCapOpt with
        | Some d when (d.CapacityUnits - d.AllocatedUnits) >= reqQty * 1.2m -> DistributionConfidence.High
        | Some d when (d.CapacityUnits - d.AllocatedUnits) >= reqQty -> DistributionConfidence.Medium
        | _ -> DistributionConfidence.Low

    let toNumeric = function
        | DistributionConfidence.High -> 100.0m
        | DistributionConfidence.Medium -> 70.0m
        | DistributionConfidence.Low -> 40.0m

    let weightedScore =
        (toNumeric invCertainty * policy.InventoryWeight)
        + (toNumeric laneRel * policy.LaneReliabilityWeight)
        + (toNumeric transitAcc * policy.TransitTimeAccuracyWeight)
        + (toNumeric dockConf * policy.ReceivingCapacityWeight)
        + (80.0m * policy.DemandStabilityWeight)
        + (80.0m * policy.DisruptionExposureWeight)

    let overall =
        if weightedScore >= 80.0m then DistributionConfidence.High
        elif weightedScore >= policy.MinimumConfidenceThreshold then DistributionConfidence.Medium
        else DistributionConfidence.Low

    { InventoryCertainty = invCertainty
      LaneReliability = laneRel
      TransitTimeAccuracy = transitAcc
      ReceivingCapacityConfidence = dockConf
      DemandVolatility = DistributionConfidence.Medium
      DisruptionExposure = DistributionConfidence.Medium
      Overall = overall }

/// BA-S-050 — Execute pure functional recommendation construction
let construct (input: ConstructionInput) : ConstructionResult =
    let req = input.Requirement
    let reqQtyVal = Quantity.value req.RequiredQuantity

    // 1. Filter out circular routes (anti-circular routing invariant)
    let nonCircularLanes =
        input.CandidateLanes
        |> List.filter (fun lane -> lane.SourceLocation <> req.DestinationLocation)

    // 2. Score candidate lanes purely
    let scoredLanes =
        nonCircularLanes
        |> List.map (fun lane ->
            let availInv =
                input.SourceInventoryPositions
                |> Map.tryFind lane.SourceLocation
                |> Option.defaultValue 0.0m

            let remainingLaneCap = max 0.0m (lane.DailyCapacity - lane.AllocatedCapacity)
            let transitPenalty = lane.TransitTimeHours * input.Policy.TimeCostPenaltyRate
            let capacityBonus = if remainingLaneCap >= reqQtyVal then 50.0m else 0.0m
            let inventoryBonus = if availInv >= reqQtyVal then 100.0m else 0.0m
            let score = inventoryBonus + capacityBonus - transitPenalty

            (lane, availInv, remainingLaneCap, score))
        |> List.sortByDescending (fun (_, _, _, score) -> score)

    match scoredLanes with
    | [] ->
        // No feasible lane or all circular
        let arrTiming = req.RequiredArrivalDate
        let depTiming = req.RequiredArrivalDate

        let feas: FeasibilityEvaluation =
            { SourceInventoryAvailability = DimensionFeasibility.Violated "No valid candidate transportation lanes"
              LaneCapacityAvailability = DimensionFeasibility.Violated "No valid candidate transportation lanes"
              TransitTimeFeasibility = DimensionFeasibility.Violated "No valid candidate transportation lanes"
              DestinationReceivingCapacity = DimensionFeasibility.Unassessed "Prerequisites unfulfilled"
              TimeWindowAchievability = DimensionFeasibility.Violated "No valid candidate transportation lanes"
              Overall = DistributionFeasibility.Infeasible }

        let constraints =
            [ { ConstraintType = "LaneUnavailable"
                Description = "No candidate transportation lanes found between origin network and destination"
                AffectedDimension = "TransportationLane" } ]

        let conf: ConfidenceEvaluation =
            { InventoryCertainty = DistributionConfidence.Low
              LaneReliability = DistributionConfidence.Low
              TransitTimeAccuracy = DistributionConfidence.Low
              ReceivingCapacityConfidence = DistributionConfidence.Low
              DemandVolatility = DistributionConfidence.Low
              DisruptionExposure = DistributionConfidence.Low
              Overall = DistributionConfidence.Low }

        let allocOutcome: AllocationOutcome =
            { RequirementReference = req.RequirementId
              AllocatedQuantity = Quantity.zero
              DeferredQuantity = req.RequiredQuantity
              DeferralReason = Some "No non-circular transportation lanes available" }

        let risks =
            [ { RiskType = "CompleteStockoutRisk"
                Severity = RiskSeverity.High
                Description = "Distribution requirement cannot be fulfilled from any network node"
                AffectedMovementCount = 1 } ]

        let reasonCodes = [ DistributionReasonCode.InsufficientSourceInventory ]
        let stability = computeStability input.PreviousRecommendation Quantity.zero depTiming []

        { Strategy = DistributionStrategy.Deferral
          AlternativeStrategy = None
          AllocationPolicyUsed = Some input.Policy.DefaultAllocationPolicy
          AllocationOutcome = Some allocOutcome
          SourceLocations = []
          LanesUsed = []
          DepartureTiming = depTiming
          ExpectedArrivalTiming = arrTiming
          Feasibility = feas
          FeasibilityConstraints = constraints
          Confidence = conf
          Stability = stability
          RiskSignals = risks
          ReasonCodes = reasonCodes
          Rationale = "Distribution deferral required: No candidate transportation lanes connecting to destination."
          EvidenceReferences = [ sprintf "RequirementId: %s" (DistributionRequirementId.value req.RequirementId) ] }

    | (bestLane, availInv, remLaneCap, _) :: alternates ->
        let transitHours = bestLane.TransitTimeHours
        let arrTiming = req.RequiredArrivalDate
        let depTiming = Timestamp.add arrTiming (TimeSpan.FromHours(- float transitHours))

        let laneUsage: LaneUsage =
            { Lane = bestLane.LaneId
              TransitTimeHours = transitHours
              LaneCapacityUtilization =
                if bestLane.DailyCapacity > 0m then
                    Math.Round(min 1.0m ((bestLane.AllocatedCapacity + reqQtyVal) / bestLane.DailyCapacity), 4)
                else 1.0m }

        let dockAvail =
            match input.DockCapacity with
            | Some d -> max 0.0m (d.CapacityUnits - d.AllocatedUnits)
            | None -> reqQtyVal

        // Determine sourcing strategy and allocations
        let (strategy, altStrategy, allocPolicyUsed, allocOutcome, allocations, isConstrained) =
            if availInv >= reqQtyVal && remLaneCap >= reqQtyVal then
                // Fully unconstrained preferred source
                let alloc: SourceLocationAllocation =
                    { Location = bestLane.SourceLocation
                      Quantity = req.RequiredQuantity
                      DepartureTiming = depTiming }

                (DistributionStrategy.PreferredSource, None, None, None, [ alloc ], false)
            elif availInv > 0.0m then
                // Constrained supply: apply allocation policy
                let allocatedQtyVal = min availInv (min remLaneCap reqQtyVal)
                let allocatedQty = Quantity.create allocatedQtyVal |> Result.defaultValue req.RequiredQuantity
                let deferredQtyVal = reqQtyVal - allocatedQtyVal
                let deferredQty = Quantity.create deferredQtyVal |> Result.defaultValue Quantity.zero

                let alloc: SourceLocationAllocation =
                    { Location = bestLane.SourceLocation
                      Quantity = allocatedQty
                      DepartureTiming = depTiming }

                let outcome: AllocationOutcome =
                    { RequirementReference = req.RequirementId
                      AllocatedQuantity = allocatedQty
                      DeferredQuantity = deferredQty
                      DeferralReason =
                        if deferredQtyVal > 0m then
                            Some (sprintf "Partial allocation due to source capacity limits (Available: %M, Required: %M)" availInv reqQtyVal)
                        else None }

                let altStrat =
                    match alternates with
                    | (altLane, _, _, _) :: _ -> Some DistributionStrategy.AlternateSource
                    | [] -> None

                (DistributionStrategy.PreferredSource, altStrat, Some input.Policy.DefaultAllocationPolicy, Some outcome, [ alloc ], true)
            else
                // Zero inventory at best source: try alternate or defer
                match alternates with
                | (altLane, altAvail, altRemCap, _) :: _ when altAvail >= reqQtyVal ->
                    let altTransitHours = altLane.TransitTimeHours
                    let altDepTiming = Timestamp.add arrTiming (TimeSpan.FromHours(- float altTransitHours))

                    let alloc: SourceLocationAllocation =
                        { Location = altLane.SourceLocation
                          Quantity = req.RequiredQuantity
                          DepartureTiming = altDepTiming }

                    (DistributionStrategy.AlternateSource, Some DistributionStrategy.PreferredSource, None, None, [ alloc ], false)
                | _ ->
                    let outcome: AllocationOutcome =
                        { RequirementReference = req.RequirementId
                          AllocatedQuantity = Quantity.zero
                          DeferredQuantity = req.RequiredQuantity
                          DeferralReason = Some "Source inventory unavailable across candidate network nodes" }

                    (DistributionStrategy.Deferral, None, Some input.Policy.DefaultAllocationPolicy, Some outcome, [], true)

        // Feasibility evaluations
        let invDim =
            if availInv >= reqQtyVal then DimensionFeasibility.Satisfied
            elif availInv > 0m then DimensionFeasibility.Conditional (sprintf "Partial inventory available: %M of %M" availInv reqQtyVal)
            else DimensionFeasibility.Violated "Source inventory depleted"

        let laneDim =
            if remLaneCap >= reqQtyVal then DimensionFeasibility.Satisfied
            elif remLaneCap > 0m then DimensionFeasibility.Conditional (sprintf "Remaining lane capacity constrained: %M of %M" remLaneCap reqQtyVal)
            else DimensionFeasibility.Violated "Lane carrier capacity exhausted"

        let transitDim =
            if Timestamp.value depTiming >= Timestamp.value input.EvaluationTimestamp then DimensionFeasibility.Satisfied
            else DimensionFeasibility.Violated "Transit lead time exceeds required delivery deadline"

        let dockDim =
            if dockAvail >= reqQtyVal then DimensionFeasibility.Satisfied
            elif dockAvail > 0m then DimensionFeasibility.Conditional (sprintf "Destination dock receiving congestion: %M available" dockAvail)
            else DimensionFeasibility.Violated "Destination receiving throughput capacity exceeded"

        let timeWindowDim =
            if Timestamp.value depTiming >= Timestamp.value input.EvaluationTimestamp then DimensionFeasibility.Satisfied
            else DimensionFeasibility.Violated "Delivery timing unachievable within required window"

        let overallFeasibility =
            let dims = [ invDim; laneDim; transitDim; dockDim; timeWindowDim ]
            let hasViolations = dims |> List.exists (function DimensionFeasibility.Violated _ -> true | _ -> false)
            let hasConditionals = dims |> List.exists (function DimensionFeasibility.Conditional _ -> true | _ -> false)

            if hasViolations then DistributionFeasibility.Infeasible
            elif hasConditionals then DistributionFeasibility.ConditionallyFeasible
            else DistributionFeasibility.FullyFeasible

        let feasibility: FeasibilityEvaluation =
            { SourceInventoryAvailability = invDim
              LaneCapacityAvailability = laneDim
              TransitTimeFeasibility = transitDim
              DestinationReceivingCapacity = dockDim
              TimeWindowAchievability = timeWindowDim
              Overall = overallFeasibility }

        // Constraints
        let constraints =
            [ if availInv < reqQtyVal then
                yield { ConstraintType = "SourceInventoryShortage"
                        Description = sprintf "Available inventory %M is less than required %M" availInv reqQtyVal
                        AffectedDimension = "Inventory" }
              if remLaneCap < reqQtyVal then
                yield { ConstraintType = "LaneCapacityExceeded"
                        Description = sprintf "Remaining lane capacity %M is less than required %M" remLaneCap reqQtyVal
                        AffectedDimension = "TransportationLane" }
              if Timestamp.value depTiming < Timestamp.value input.EvaluationTimestamp then
                yield { ConstraintType = "LeadTimeViolation"
                        Description = "Departure time is in the past relative to evaluation time"
                        AffectedDimension = "LeadTime" }
              if dockAvail < reqQtyVal then
                yield { ConstraintType = "DockCapacityCongestion"
                        Description = sprintf "Destination dock capacity %M is below transfer requirement %M" dockAvail reqQtyVal
                        AffectedDimension = "DockReceiving" } ]

        let confidence = evaluateConfidence availInv reqQtyVal input.DockCapacity (Some bestLane) input.Policy
        let stability = computeStability input.PreviousRecommendation req.RequiredQuantity depTiming [ laneUsage ]

        // Risk signals
        let risks =
            [ if isConstrained then
                yield { RiskType = "ConstrainedSupplyStockoutRisk"
                        Severity = RiskSeverity.High
                        Description = "Transfer quantity constrained by source inventory availability"
                        AffectedMovementCount = 1 }
              if remLaneCap < reqQtyVal * 1.1m then
                yield { RiskType = "CarrierCapacityBottleneck"
                        Severity = RiskSeverity.Medium
                        Description = "Carrier capacity utilization exceeds 90%"
                        AffectedMovementCount = 1 }
              if dockAvail < reqQtyVal * 1.1m then
                yield { RiskType = "ReceivingCongestionRisk"
                        Severity = RiskSeverity.Medium
                        Description = "Destination dock utilization exceeds threshold"
                        AffectedMovementCount = 1 } ]

        // Reason codes
        let reasonCodes =
            [ if strategy = DistributionStrategy.PreferredSource then yield DistributionReasonCode.OptimalLaneSelected
              if availInv >= reqQtyVal then yield DistributionReasonCode.SourceStockAllocated
              if Timestamp.value depTiming >= Timestamp.value input.EvaluationTimestamp then yield DistributionReasonCode.TransitTimeFeasible
              if dockAvail >= reqQtyVal then yield DistributionReasonCode.DestinationDockThroughputVerified
              if availInv < reqQtyVal then yield DistributionReasonCode.InsufficientSourceInventory
              if remLaneCap < reqQtyVal then yield DistributionReasonCode.LaneCapacityExceeded
              if dockAvail < reqQtyVal then yield DistributionReasonCode.DockCongestionBacklog
              if req.IsExpedited then yield DistributionReasonCode.ExpeditedDeployment ]

        let rationale =
            sprintf "Distribution recommendation evaluated via BA-S-050. Strategy: %A; Feasibility: %A; Confidence: %A; Stability: %.2f."
                strategy overallFeasibility confidence.Overall stability.StabilityScore

        let evidenceRefs =
            [ sprintf "RequirementId: %s" (DistributionRequirementId.value req.RequirementId)
              sprintf "Item: %s" (ItemId.value req.Item)
              sprintf "Destination: %s" (LocationId.value req.DestinationLocation)
              sprintf "SelectedLane: %s" (TransportationLaneId.value bestLane.LaneId) ]

        { Strategy = strategy
          AlternativeStrategy = altStrategy
          AllocationPolicyUsed = allocPolicyUsed
          AllocationOutcome = allocOutcome
          SourceLocations = allocations
          LanesUsed = [ laneUsage ]
          DepartureTiming = depTiming
          ExpectedArrivalTiming = arrTiming
          Feasibility = feasibility
          FeasibilityConstraints = constraints
          Confidence = confidence
          Stability = stability
          RiskSignals = risks
          ReasonCodes = reasonCodes
          Rationale = rationale
          EvidenceReferences = evidenceRefs }
