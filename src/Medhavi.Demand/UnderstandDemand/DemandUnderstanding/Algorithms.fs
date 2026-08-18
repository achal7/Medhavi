module Medhavi.Demand.UnderstandDemand.DemandUnderstanding.Algorithms

open Model
open Policies

// =============================================================================
// SE-D-002 — Demand Understanding Business Algorithms (Chapter 10)
// Traces to: BA-D-001 (Evaluate Demand Understanding Materiality)
// =============================================================================

/// BA-D-001 §8 — per-dimension materiality result.
type MaterialityResult =
    | Material of evidence: string
    | NotMaterial of evidence: string
    | NotApplicable of reason: string

/// BA-D-001 §8 — structured materiality assessment across the four interpretation dimensions.
type MaterialityAssessment =
    { Continuity: MaterialityResult
      Pattern: MaterialityResult
      Health: MaterialityResult
      Volatility: MaterialityResult
      HasMaterialChange: bool }

/// BA-D-001 §7 — input contract.
type MaterialityInput =
    { Draft: Interpretation
      Published: Interpretation option
      ContinuityChangeMagnitudePercent: decimal option }

module MaterialityResult =
    let isMaterial (r: MaterialityResult) =
        match r with
        | Material _ -> true
        | NotMaterial _
        | NotApplicable _ -> false

/// BA-D-001 §10.1 — Stable <-> Increasing/Declining continuity magnitude evaluation.
let private continuityMagnitude (policy: MaterialityPolicy) (input: MaterialityInput) : MaterialityResult =
    match policy.ContinuityMagnitudeThresholdPercent, input.ContinuityChangeMagnitudePercent with
    | None, _ -> NotApplicable "ContinuityMagnitudeThresholdNotRatified"
    | Some _, None -> NotApplicable "ContinuityMagnitudeEvidenceUnavailable"
    | Some threshold, Some magnitude ->
        if magnitude >= threshold then
            Material(sprintf "ContinuityMagnitudeExceeded (%.2f%% >= %.2f%%)" magnitude threshold)
        else
            NotMaterial(sprintf "ContinuityMagnitudeBelowThreshold (%.2f%% < %.2f%%)" magnitude threshold)

/// BA-D-001 §10.1 — Demand Continuity Interpretation.
let private evaluateContinuity (policy: MaterialityPolicy) (input: MaterialityInput) : MaterialityResult =
    match input.Published with
    | None -> NotApplicable "FirstPublication"
    | Some published ->
        match input.Draft.Continuity, published.Continuity with
        | Incomplete _, _ -> NotApplicable "DraftContinuityIncomplete"
        | _, Incomplete _ -> NotApplicable "PublishedContinuityIncomplete"
        | Known draft, Known pub ->
            if input.Draft.ContinuityDrivers <> published.ContinuityDrivers then
                Material "KeyDemandDriversChanged"
            else
                match draft, pub with
                | Stable, Volatile
                | Volatile, Stable -> Material "StableVolatileTransition"
                | Stable, Increasing
                | Increasing, Stable -> continuityMagnitude policy input
                | Stable, Declining
                | Declining, Stable -> continuityMagnitude policy input
                | _ -> NotMaterial "NoMaterialContinuityTransition"

/// BA-D-001 §10.2 — Demand Pattern Interpretation.
let private evaluatePattern (input: MaterialityInput) : MaterialityResult =
    match input.Published with
    | None -> NotApplicable "FirstPublication"
    | Some published ->
        match input.Draft.Pattern, published.Pattern with
        | Incomplete _, _ -> NotApplicable "DraftPatternIncomplete"
        | _, Incomplete _ -> NotApplicable "PublishedPatternIncomplete"
        | Known draft, Known pub ->
            let confidenceDelta =
                match input.Draft.PatternConfidence, published.PatternConfidence with
                | Known d, Known p ->
                    let order =
                        function
                        | ConfidenceLevel.High -> 2
                        | ConfidenceLevel.Medium -> 1
                        | ConfidenceLevel.Low -> 0

                    abs (order d - order p)
                | _ -> 0

            match draft, pub with
            | StepChange, _
            | _, StepChange -> Material "StepChangeTransition"
            | Normal, Seasonal
            | Seasonal, Normal
            | Normal, Irregular
            | Irregular, Normal -> Material "PatternStatusTransition"
            | _ ->
                if confidenceDelta > 1 then
                    Material "PatternConfidenceMoreThanOneLevel"
                else
                    NotMaterial "NoMaterialPatternTransition"

/// BA-D-001 §10.3 — Demand Health Interpretation.
let private evaluateHealth (input: MaterialityInput) : MaterialityResult =
    match input.Published with
    | None -> NotApplicable "FirstPublication"
    | Some published ->
        match input.Draft.Health, published.Health with
        | Incomplete _, _ -> NotApplicable "DraftHealthIncomplete"
        | _, Incomplete _ -> NotApplicable "PublishedHealthIncomplete"
        | Known draft, Known pub ->
            if input.Draft.HealthConcerns <> published.HealthConcerns then
                Material "DataQualityConcernsChanged"
            else
                match draft, pub with
                | AtRisk, Healthy
                | Critical, Healthy -> Material "HealthTransitionedToAtRiskOrCritical"
                | Healthy, AtRisk
                | Healthy, Critical -> Material "HealthRestoredToHealthy"
                | _ -> NotMaterial "NoMaterialHealthTransition"

/// BA-D-001 §10.4 — Demand Volatility Interpretation.
let private evaluateVolatility (input: MaterialityInput) : MaterialityResult =
    match input.Published with
    | None -> NotApplicable "FirstPublication"
    | Some published ->
        match input.Draft.Volatility, published.Volatility with
        | Incomplete _, _ -> NotApplicable "DraftVolatilityIncomplete"
        | _, Incomplete _ -> NotApplicable "PublishedVolatilityIncomplete"
        | Known draft, Known pub ->
            let order =
                function
                | VolatilityLevel.High -> 2
                | VolatilityLevel.Medium -> 1
                | VolatilityLevel.Low -> 0

            if input.Draft.VolatilityDrivers <> published.VolatilityDrivers then
                Material "PrimaryVolatilityDriversChanged"
            elif abs (order draft - order pub) > 1 then
                Material "VolatilityMoreThanOneLevel"
            else
                NotMaterial "NoMaterialVolatilityChange"

/// BA-D-001 — Evaluate Demand Understanding Materiality.
/// Applies the thresholds defined in PO-D-011; it does not define them.
let evaluateMateriality (policy: MaterialityPolicy) (input: MaterialityInput) : MaterialityAssessment =
    let continuity = evaluateContinuity policy input
    let pattern = evaluatePattern input
    let health = evaluateHealth input
    let volatility = evaluateVolatility input

    let hasMaterialChange =
        match input.Published with
        | None -> true // BA-D-001 §10: first publication is always material.
        | Some _ ->
            [ continuity; pattern; health; volatility ]
            |> List.exists MaterialityResult.isMaterial

    { Continuity = continuity
      Pattern = pattern
      Health = health
      Volatility = volatility
      HasMaterialChange = hasMaterialChange }

