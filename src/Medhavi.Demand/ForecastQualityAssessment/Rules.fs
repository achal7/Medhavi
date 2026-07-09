module Medhavi.Demand.ForecastQualityAssessment.Rules

open Medhavi.SharedKernel
open Medhavi.SharedKernel.Failure

/// BR‑D‑080 — Data must cover the full evaluation period and meet completeness threshold.
let dataCompleteness (actualCount: int) (expectedCount: int) (threshold: decimal) =
    if expectedCount = 0 then
        Error(DomainError.validation "Expected data count cannot be zero.")
    else
        let ratio = decimal actualCount / decimal expectedCount
        if ratio < threshold then
            Error(DomainError.validation $"Source data completeness ({ratio:P0}) is below policy threshold ({threshold:P0}).")
        else
            Ok()

/// BR‑D‑081 — Evaluation period must meet minimum length defined in policy.
let minimumEvaluationPeriod (start: Timestamp) (end_: Timestamp) (minDays: int) =
    let days = (Timestamp.value end_ - Timestamp.value start).TotalDays
    if days < float minDays then
        Error(DomainError.validation $"Evaluation period ({days:F1} days) does not meet minimum policy requirement of {minDays} days.")
    else
        Ok()
