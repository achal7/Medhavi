module Medhavi.Demand.DemandPlanningCondition.Rules

open Medhavi.SharedKernel.Failure
open Medhavi.Demand.DemandPlanningCondition.Model

/// BR‑D‑086 — Detection thresholds must be met (external evaluation, domain trusts the caller).
let detectionThresholdsMet (_evidence: string) =
    // The workflow/caller has already evaluated thresholds; we trust the provided evidence.
    Ok()

/// BR‑D‑089 — A condition is either Active or Resolved, never both.
let activeOrResolved (currentStatus: ConditionStatus) =
    if currentStatus = Resolved then
        Error(
            DomainError.validation "Cannot modify a resolved condition. A new condition must be created for recurrence."
        )
    else
        Ok()

/// BR‑D‑091 — A resolved condition is terminal. Recurrence creates a new instance.
let resolvedIsTerminal (currentStatus: ConditionStatus) =
    if currentStatus = Resolved then
        Error(
            DomainError.validation
                "A resolved condition is terminal. Recurrence must create a new condition with a new identifier."
        )
    else
        Ok()
