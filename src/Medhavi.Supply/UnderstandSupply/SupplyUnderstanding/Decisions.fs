module Medhavi.Supply.UnderstandSupply.SupplyUnderstanding.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Rules

// =============================================================================
// SE-S-001 — Supply Understanding Decisions
// Traces to: DE-S-002 (Publish Supply Understanding)
// =============================================================================

/// DE-S-002 — decision alternatives
type PublishOutcome =
    | Publish
    | DoNotPublish

/// AB-S-003 — trace outcome (revision invokes no Decision)
type RevisionOutcome =
    | Revised

/// DE-S-002 — Evaluate whether the Draft Supply Understanding may be published
let evaluatePublication
    (rules: Rule<PublicationInput> list)
    (input: PublicationInput)
    : Result<DecisionOutcome<PublishOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let passed = evaluations |> List.forall(fun e -> e.Passed)
        let outcome = if passed then Publish else DoNotPublish
        return { Outcome = outcome; Evaluations = evaluations }
    }
