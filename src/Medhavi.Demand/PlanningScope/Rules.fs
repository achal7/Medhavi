module Medhavi.Demand.PlanningScope.Rules

open Medhavi.SharedKernel.Failure
open Medhavi.Demand.PlanningScope.Model

// BR‑D‑025 — Identity unique (enforced by repository)
let identityUnique = Ok()

// BR‑D‑027 — At most one Active scope per identity (enforced by repository)
let atMostOneActive = Ok()

// BR‑D‑048 — Never deleted, only archived
let neverDeleted (status: PlanningScopeStatus) =
    if status = Archived then
        Error(DomainError.validation "Cannot modify an archived Planning Scope")
    else
        Ok()
