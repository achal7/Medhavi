module Medhavi.Demand.UnderstandDemand.DemandUnderstanding.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Rules

// =============================================================================
// SE-D-002 — Demand Understanding Decisions
// Traces to: DE-D-002 (Publish Demand Understanding)
// =============================================================================

/// DE-D-002 — decision alternatives.
type PublishOutcome =
    | Publish
    | DoNotPublish

/// AB-D-003 — trace outcome (revision invokes no Decision).
type RevisionOutcome =
    | Revised

/// DE-D-002 — Evaluate whether the Draft Demand Understanding may be published.
let evaluatePublication
    (rules: Rule<PublicationInput> list)
    (input: PublicationInput)
    : Result<DecisionOutcome<PublishOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let passed = evaluations |> List.forall (fun e -> e.Passed)
        let outcome = if passed then Publish else DoNotPublish
        return { Outcome = outcome; Evaluations = evaluations }
    }
