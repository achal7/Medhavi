/// CA-P-004 Evaluate Multi-Option ATP Algorithms
/// Traces to: BA-P-001 Multi-Option Promising Optimization
module Medhavi.Promise.EvaluateATP.Algorithms

open System
open Medhavi.SemanticModel
open Medhavi.Promise
open Model
open Policies

let evaluateCandidateOption
    (policy: MultiOptionPromisingPolicy)
    (category: PromiseOptionCategory)
    (legs: SourcingLeg list)
    (requestedQty: Quantity)
    (requestedDate: Timestamp)
    (standardCogs: decimal)
    (listPrice: decimal)
    : CandidatePromiseOption =

    let totalSourced = legs |> List.sumBy (fun l -> Quantity.value l.SourcedQuantity)
    let promisedQuantity = Quantity.ofDecimal totalSourced (Quantity.unitOfMeasure requestedQty)

    let earliestDelivery =
        if legs.IsEmpty then requestedDate
        else legs |> List.map (fun l -> l.EstimatedDeliveryDate) |> List.maxBy Timestamp.value

    let totalFreight = legs |> List.sumBy (fun l -> l.FreightCost)
    let totalCarbon = legs |> List.sumBy (fun l -> l.CarbonEmissionsKg)
    let totalHandling = policy.HandlingCostPerUnit * totalSourced
    let totalCogs = standardCogs * totalSourced
    let totalLandedCost = totalCogs + totalHandling + totalFreight

    let grossMarginPct =
        let revenue = listPrice * totalSourced
        if revenue > 0m then
            let margin = ((revenue - totalLandedCost) / revenue) * 100.0m
            Math.Round(margin, 2)
        else 0.0m

    // Normalized criteria scores (0-100)
    let speedScore =
        let diffDays = ((Timestamp.value earliestDelivery) - (Timestamp.value requestedDate)).TotalDays
        if diffDays <= 0.0 then 100.0m
        elif diffDays <= 1.0 then 90.0m
        elif diffDays <= 3.0 then 70.0m
        elif diffDays <= 7.0 then 40.0m
        else 10.0m

    let costScore =
        if listPrice > 0m then
            let costRatio = totalLandedCost / (listPrice * Math.Max(1.0m, totalSourced))
            Math.Clamp(100.0m - (costRatio * 100.0m), 0.0m, 100.0m)
        else 50.0m

    let marginScore = Math.Clamp(grossMarginPct * 2.0m, 0.0m, 100.0m)

    let sustainabilityScore =
        let emissionsPerUnit = if totalSourced > 0m then totalCarbon / totalSourced else 0.0m
        Math.Clamp(100.0m - (emissionsPerUnit * 10.0m), 0.0m, 100.0m)

    let compositeRank =
        (policy.WeightSpeed * speedScore)
        + (policy.WeightCost * costScore)
        + (policy.WeightMargin * marginScore)
        + (policy.WeightSustainability * sustainabilityScore)

    let rejectionReasons =
        [ if grossMarginPct < policy.SubstitutionMarginFloorPct && category = Substitution then
            yield $"Gross margin {grossMarginPct}%% below substitution floor {policy.SubstitutionMarginFloorPct}%%"
          if totalSourced < Quantity.value requestedQty && category = DirectShip then
            yield $"Insufficient stock for direct fulfillment: sourced {totalSourced} vs requested {Quantity.value requestedQty}" ]

    { OptionId = Guid.NewGuid().ToString("N").[0..7]
      Category = category
      EarliestDeliveryDate = earliestDelivery
      PromisedQuantity = promisedQuantity
      TotalLandedCost = Math.Round(totalLandedCost, 2)
      GrossMarginPct = grossMarginPct
      CarbonEmissionsKg = Math.Round(totalCarbon, 2)
      SpeedScore = speedScore
      CostScore = costScore
      MarginScore = marginScore
      SustainabilityScore = sustainabilityScore
      CompositeRankScore = Math.Round(compositeRank, 2)
      SourcingLegs = legs
      FeasibilityPassed = rejectionReasons.IsEmpty
      RejectionReasons = rejectionReasons }

