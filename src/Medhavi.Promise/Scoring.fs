module Medhavi.Promise.Scoring

open System
open Medhavi.SharedKernel.PromisePolicy
open Medhavi.Promise.PromiseTypes
open Medhavi.Contracts.MasterData.Transport

type Score = private Score of float

module Score =
    let value (Score s) = s
    let create s = Score(max 0.0 (min 1.0 s))
    let add (Score a) (Score b) = Score(a + b)
    let multiply (Score s) (factor: float) = Score(s * factor)

type ScoredRouting =
    { Choice: RoutingChoice
      Score: float }

type ScoredItinerary =
    { Itinerary: Itinerary
      Score: float }



/// Policy-driven weights for scoring
let getPolicyWeights (policy: PromisePolicy) =
    match policy.TimePreference with
    | Fastest -> (0.6, 0.2, 0.2) // Time most important
    | Cheapest -> (0.2, 0.6, 0.2) // Cost most important
    | Balanced -> (0.33, 0.33, 0.34) // Sum to 1.0

/// Normalize cost to scale (avoid division by large numbers)
let private normalizeCost (cost: decimal option) =
    match cost with
    | None -> 0.0
    | Some c -> float c / 1000.0

/// Normalize reliability to risk score (higher reliability = lower risk = better score)
let private normalizeReliability (rel: decimal option) =
    match rel with
    | None -> 0.5
    | Some r -> 1.0 - (float r)

/// Score a routing choice based on policy
let scoreRoutingChoice (policy: PromisePolicy) (choice: RoutingChoice) =
    let timeW, _, riskW = getPolicyWeights policy

    let timeScore =
        match choice.EstimatedDuration with
        | None -> 0.5 // Unknown duration gets neutral score
        | Some d -> (float d.TotalMinutes / 1440.0) ** 2.0  // Squared penalty for longer durations

    let relScore = normalizeReliability choice.Reliability

    timeScore * timeW + relScore * riskW

/// Score an itinerary based on policy
let scoreItinerary (policy: PromisePolicy) (itinerary: Itinerary) =
    let timeW, costW, riskW = getPolicyWeights policy

    let timeScore = (float itinerary.TotalLeadTimeMinutes / 1440.0) ** 2.0
    let costScore = min 1.0 (normalizeCost(Some itinerary.TotalFixedCost))
    let relScore = float itinerary.TotalReliability  // Already 0-1 range

    timeScore * timeW + costScore * costW + relScore * riskW

/// Score and sort routing choices
let scoreRoutingChoices (policy: PromisePolicy) (selection: RoutingSelection) =
    let scored =
        selection.Primary :: selection.Alternates
        |> List.map (fun c -> { Choice = c; Score = scoreRoutingChoice policy c })
        |> List.sortBy (fun s -> s.Score)

    match scored with
    | x :: _ -> x.Choice
    | [] -> selection.Primary

module CostScoring =
    let scoreTimeDeviation (actual: DateTimeOffset) (target: DateTimeOffset) =
        let diff = (actual - target).TotalDays
        if diff <= 0.0 then 0.0 else exp diff - 1.0

    let scoreCostOverrun (actual: decimal) (budget: decimal) =
        if budget <= 0m then 0.0
        elif actual <= budget then 0.0
        else float((actual - budget) / budget)

    let scoreRisk (reliability: float) (confidence: float) =
        (1.0 - reliability) * (1.0 - confidence)

    let scoreCO2Deviation (actual: decimal) (target: decimal) =
        if target <= 0m then 0.0
        elif actual <= target then 0.0
        else float((actual - target) / target)
