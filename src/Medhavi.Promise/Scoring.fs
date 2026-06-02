module Medhavi.Promise.Scoring

open System
open Medhavi.SharedKernel.PromisePolicy
open Medhavi.Promise.PromiseTypes
open Medhavi.Transport

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
    | Fastest -> (0.6, 0.2, 0.1, 0.1) // Time most important
    | Cheapest -> (0.2, 0.6, 0.1, 0.1) // Cost most important
    | Balanced -> (0.33, 0.33, 0.24, 0.1)

/// Normalize cost to scale (avoid division by large numbers)
let private normalizeCost (cost: decimal option) =
    match cost with
    | None -> 0.0
    | Some c -> float c / 1000.0

/// Normalize reliability to risk score (higher reliability = lower risk = better score)
let private normalizeReliability (rel: float option) =
    match rel with
    | None -> 0.5 // Unknown reliability gets neutral score
    | Some r -> 1.0 - r

/// Normalize CO2 to scale
let private normalizeCo2 (co2: decimal option) =
    match co2 with
    | None -> 0.0
    | Some c -> float c / 1000.0

/// Score a routing choice based on policy
let scoreRoutingChoice (policy: PromisePolicy) (choice: RoutingChoice) =
    let (timeW, costW, riskW, co2W) = getPolicyWeights policy

    // Time score (lower is better, so we invert)
    let timeScore =
        match choice.EstimatedDuration with
        | None -> float(1e6) // Penalize unknown duration
        | Some d -> d.TotalMinutes

    let relScore = normalizeReliability choice.Reliability

    timeScore * timeW + relScore * riskW

/// Score an itinerary based on policy
let scoreItinerary (policy: PromisePolicy) (itinerary: Itinerary) =
    let (timeW, costW, riskW, co2W) = getPolicyWeights policy

    let timeScore = float itinerary.TotalLeadTimeMinutes
    let costScore = normalizeCost(Some itinerary.TotalFixedCost)
    let relScore = float itinerary.TotalReliability
    let co2Score = normalizeCo2 itinerary.TotalCO2

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