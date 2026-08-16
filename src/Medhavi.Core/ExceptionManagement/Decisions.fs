/// CA-C-020 Exception Management Decisions
module Medhavi.Core.ExceptionManagement.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Rules

type RegistrationOutcome =
    | RegisteredSuccessfully
    | RegistrationRejected of reasons: string list

type ResolutionOutcome =
    | ResolvedSuccessfully
    | ResolutionRejected of reasons: string list

/// DE-C-020a: Exception Registration Decision
let decideRegistration
    (rules: Rule<RegisterInput> list)
    (input: RegisterInput)
    : Result<DecisionOutcome<RegistrationOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter(fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = RegisteredSuccessfully
                  Evaluations = evaluations }
        else
            let reasons = failed |> List.map(fun e -> sprintf "[%s] %s" e.RuleId (e.Evidence |> String.concat ", "))

            return
                { Outcome = RegistrationRejected reasons
                  Evaluations = evaluations }
    }

/// DE-C-020b: Exception Resolution Decision
let decideResolution
    (rules: Rule<ResolveInput> list)
    (input: ResolveInput)
    : Result<DecisionOutcome<ResolutionOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter(fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = ResolvedSuccessfully
                  Evaluations = evaluations }
        else
            let reasons = failed |> List.map(fun e -> sprintf "[%s] %s" e.RuleId (e.Evidence |> String.concat ", "))

            return
                { Outcome = ResolutionRejected reasons
                  Evaluations = evaluations }
    }
