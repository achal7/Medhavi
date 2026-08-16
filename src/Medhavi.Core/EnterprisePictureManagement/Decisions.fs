/// CA-C-019 Decisions
module Medhavi.Core.EnterprisePictureManagement.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Algorithms
open Rules

/// Outcome of a composition decision.
type CompositionOutcome =
    | ComposedSuccessfully
    | CompositionRejected of reasons: string list

type PublicationOutcome =
    | PublishVersion
    | RetainDraft of reason: string

/// Composition Decision.
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

/// DE-C-001: Assess Picture Materiality. Consumes BA-C-001 output; determines publication.
let assessMateriality
    (rules: Rule<PublishInput> list)
    (input: PublishInput)
    (assessment: MaterialityAssessment)
    : Result<DecisionOutcome<PublicationOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        if not failed.IsEmpty then
            let reasons = failed |> List.map (fun e -> sprintf "[%s] %s" e.RuleId (e.Evidence |> String.concat ", "))
            return { Outcome = RetainDraft (String.concat "; " reasons); Evaluations = evaluations }
        elif assessment.HasMaterialChange then
            return { Outcome = PublishVersion; Evaluations = evaluations }
        else
            return { Outcome = RetainDraft assessment.Reason; Evaluations = evaluations }
    }
