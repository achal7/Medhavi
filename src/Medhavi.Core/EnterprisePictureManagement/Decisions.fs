/// CA-C-019 Decisions
/// Evaluates rules and produces decision outcomes with full traceability.
module Medhavi.Core.EnterprisePictureManagement.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Rules

/// Outcome of a composition decision.
type CompositionOutcome =
    | ComposedSuccessfully
    | CompositionRejected of reasons: string list

/// Outcome of a publication decision.
type PublicationOutcome =
    | PublishedSuccessfully
    | PublicationRejected of reasons: string list

/// DE-C-019a: Composition Decision.
/// Evaluates all composition rules against the input and produces an outcome.
let decideComposition
    (rules: Rule<ComposeInput> list)
    (input: ComposeInput)
    : Result<DecisionOutcome<CompositionOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter(fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = ComposedSuccessfully
                  Evaluations = evaluations }
        else
            let reasons = failed |> List.map(fun e -> sprintf "[%s] %s" e.RuleId (e.Evidence |> String.concat ", "))

            return
                { Outcome = CompositionRejected reasons
                  Evaluations = evaluations }
    }

/// DE-C-019b: Publication Decision.
/// Evaluates all publication rules against the input and produces an outcome.
let decidePublication
    (rules: Rule<PublishInput> list)
    (input: PublishInput)
    : Result<DecisionOutcome<PublicationOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter(fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = PublishedSuccessfully
                  Evaluations = evaluations }
        else
            let reasons = failed |> List.map(fun e -> sprintf "[%s] %s" e.RuleId (e.Evidence |> String.concat ", "))

            return
                { Outcome = PublicationRejected reasons
                  Evaluations = evaluations }
    }
