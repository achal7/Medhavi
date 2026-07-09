module Medhavi.Demand.EnterpriseDemandPicture.Rules

open Medhavi.SharedKernel.Failure
open Medhavi.Demand.EnterpriseDemandPicture.Model

/// BR‑D‑005 — Exactly one Published version per Planning Scope (repository enforces uniqueness)
let exactlyOnePublished = Ok()

/// BR‑D‑006 — Published EDP must never be modified
let publishedImmutable (status: EdpStatus) =
    if status = Published then
        Error(DomainError.validation "Cannot modify a published Enterprise Demand Picture")
    else
        Ok()

/// BR‑D‑056 — Superseded version must never return to Published
let supersededImmutable (status: EdpStatus) =
    if status = Superseded then
        Error(DomainError.validation "Superseded version cannot be republished")
    else
        Ok()

/// BR‑D‑057 — Must record Business Time, Transaction Time, Publication Time (enforced by evolve)
let timesRecorded = Ok()
