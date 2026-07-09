module Medhavi.Demand.DemandLearning.Rules

open Medhavi.SharedKernel.Failure

/// BR‑D‑096 — Learning must be supported by evidence from at least one completed analysis or evaluation.
let evidenceRequired (evidence: string list) =
    if List.isEmpty evidence then
        Error(DomainError.validation "At least one piece of supporting evidence is required to record a demand learning.")
    else
        Ok()

/// BR‑D‑097 — A learning, once created, shall never be modified.
let learningImmutable (existing: Model.DemandLearning option) =
    match existing with
    | Some _ -> Error(DomainError.validation "A demand learning already exists with this identifier and is immutable.")
    | None -> Ok()
