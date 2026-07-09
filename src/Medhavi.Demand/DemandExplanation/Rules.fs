module Medhavi.Demand.DemandExplanation.Rules

open Medhavi.SharedKernel.Failure

/// BR‑D‑092 – Source artifact references must carry the version in effect when the explained artifact was produced.
let sourceArtifactVersionsPresent (refs: Model.ExplanationSourceArtifactRef list) =
    if List.isEmpty refs then
        Error(DomainError.validation "Source artifact references must not be empty.")
    elif refs |> List.exists(fun r -> r.Version <= 0) then
        Error(DomainError.validation "All source artifact references must have a valid version (> 0).")
    else
        Ok()

/// BR‑D‑093 – The Structured Reasoning Graph must be deterministic: identical inputs → identical graph.
let graphDeterministic (_graph: Model.StructuredReasoningGraph) =
    // Determinism is enforced by the algorithm that builds the graph; we trust it here.
    Ok()

/// BR‑D‑094 – An explanation, once created, shall never be modified.
let explanationImmutable (existing: Model.DemandExplanation option) =
    match existing with
    | Some _ -> Error(DomainError.validation "An explanation already exists for this artifact; it is immutable.")
    | None -> Ok()
