/// CA-C-020 Decisions
module Medhavi.Core.ExceptionManagement.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Rules
open Policies
open Model

/// DE-C-002 outcomes
type EvidenceOutcome =
    | RegisterNewException
    | UpdateExistingException of effectiveSeverity: VocabularyEntryId option
    | RejectEvidence of reasons: string list

/// DE-C-003 outcomes
type ResolutionOutcome =
    | ResolveException
    | RejectResolution of reasons: string list

/// DE-C-002: Evaluate Exception Evidence (dedup + severity resolution).
let evaluateExceptionEvidence
    (rules: Rule<ProcessInput> list)
    (input: ProcessInput)
    : Result<DecisionOutcome<EvidenceOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        if not failed.IsEmpty then
            let reasons = failed |> List.map (fun e -> sprintf "[%s] %s" e.RuleId (e.Evidence |> String.concat ", "))
            return { Outcome = RejectEvidence reasons; Evaluations = evaluations }
        else
            match input.CurrentState with
            | None ->
                return { Outcome = RegisterNewException; Evaluations = evaluations }
            | Some existing when existing.LifecycleState = ExceptionLifecycleState.Active ->
                // Higher severity prevails (PO-C-002)
                let currentRank = severityRank input.Policy input.CurrentSeverity
                let incomingRank = severityRank input.Policy input.Cmd.Severity
                let effective =
                    if incomingRank >= currentRank then input.Cmd.Severity else input.CurrentSeverity
                return { Outcome = UpdateExistingException effective; Evaluations = evaluations }
            | Some _ ->
                // Existing but not Active (Resolved): treat as new registration is disallowed; reject.
                return { Outcome = RejectEvidence [ "Existing exception is not Active" ]; Evaluations = evaluations }
    }

/// DE-C-003: Evaluate Exception Resolution.
let evaluateExceptionResolution
    (rules: Rule<ResolveInput> list)
    (input: ResolveInput)
    : Result<DecisionOutcome<ResolutionOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)
        if not failed.IsEmpty then
            let reasons = failed |> List.map (fun e -> sprintf "[%s] %s" e.RuleId (e.Evidence |> String.concat ", "))
            return { Outcome = RejectResolution reasons; Evaluations = evaluations }
        else
            return { Outcome = ResolveException; Evaluations = evaluations }
    }
