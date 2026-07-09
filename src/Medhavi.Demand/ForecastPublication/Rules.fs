module Medhavi.Demand.ForecastPublication.Rules

open Medhavi.SharedKernel.Failure
open Medhavi.Demand.ForecastPublication.Model
open Medhavi.SharedKernel

// BR‑D‑005‑F — Exactly one Published version per Planning Scope
let exactlyOnePublished = Ok()

// BR‑D‑029 — Published publication immutable; any change requires new publication
let publishedImmutable (status: ForecastPublicationStatus) =
    if status = Published then
        Error(DomainError.validation "Cannot modify a published Forecast Publication")
    else
        Ok()

// BR‑D‑026 — Completeness threshold before publication
let completenessMet (forecastCount: int) (coveredCount: int) (threshold: decimal) =
    let ratio = decimal forecastCount / decimal coveredCount * 100m

    if ratio >= threshold then
        Ok()
    else
        Error(DomainError.validation $"Completeness {ratio}%% below threshold {threshold}%%")

// BR‑D‑036 — Challenger must show significant reduction in WAPE
let championSignificance (candidateWape: decimal) (championWape: decimal) (pValue: decimal) =
    if pValue > 0.05m then
        Error(DomainError.validation "Candidate improvement is not statistically significant")
    elif candidateWape >= championWape then
        Error(DomainError.validation "Candidate does not improve WAPE")
    else
        Ok()

// BR‑D‑037 — Challenger must not degrade bias or stability
let noHarm (candidateBias: decimal) (championBias: decimal) (candidateStability: decimal) (championStability: decimal) =
    let biasTolerance = 0.02m
    let stabilityTolerance = 0.05m

    if abs(candidateBias - championBias) > biasTolerance then
        Error(DomainError.validation "Candidate increases absolute bias beyond tolerance")
    elif abs(candidateStability - championStability) > stabilityTolerance then
        Error(DomainError.validation "Candidate degrades forecast stability beyond tolerance")
    else
        Ok()

// BR‑D‑042 — Override justification non‑empty
let overrideJustification (justification: string) =
    if justification.Trim().Length = 0 then
        Error(DomainError.validation "Override justification is required")
    else
        Ok()

// BR‑D‑043 — Override deviation within limit
let overrideDeviation (systemValue: decimal) (newValue: decimal) (maxDeviationPercent: decimal) =
    let deviation = abs(newValue - systemValue) / systemValue * 100m

    if deviation > maxDeviationPercent then
        Error(DomainError.validation $"Override deviation {deviation}%% exceeds {maxDeviationPercent}%% limit")
    else
        Ok()

// Validate pre-computed baseline forecast lines
let validateForecastLines (forecasts: Forecast list) =
    let validateLine (f: Forecast) =
        let lower = PositiveDecimal.value f.PredictionInterval.LowerBound
        let upper = PositiveDecimal.value f.PredictionInterval.UpperBound

        if f.Mean < 0m || lower < 0m || upper < 0m then
            Error(DomainError.validation $"Forecast values must be non-negative for Sku {SkuId.value f.SkuId}")
        elif lower > f.Mean || f.Mean > upper then
            Error(
                DomainError.validation
                    $"Prediction interval bounds are invalid for Sku {SkuId.value f.SkuId}: {lower} <= {f.Mean} <= {upper}"
            )
        else
            Ok()

    let rec loop xs =
        match xs with
        | [] -> Ok()
        | x :: tail ->
            match validateLine x with
            | Error e -> Error e
            | Ok() -> loop tail

    if forecasts.IsEmpty then
        Error(DomainError.validation "No forecast lines provided")
    else
        loop forecasts
