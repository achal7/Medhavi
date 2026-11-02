/// CA-P-001 Customer Promise Profile Decisions
module Medhavi.Promise.UnderstandOrders.CustomerPromiseProfile.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Rules

type ConfigureProfileOutcome =
    | Validated
    | ValidationFailed of reasons: string list

let evaluateConfigureProfile
    (rules: Rule<ConfigureProfileInput> list)
    (input: ConfigureProfileInput)
    : Result<DecisionOutcome<ConfigureProfileOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = Validated
                  Evaluations = evaluations }
        else
            let reasons = failed |> List.map (fun e -> sprintf "[%s] %s" e.RuleId (e.Evidence |> String.concat ", "))
            return
                { Outcome = ValidationFailed reasons
                  Evaluations = evaluations }
    }

type UpdateTermsOutcome =
    | UpdateAuthorized
    | UpdateRejected of reason: string

let evaluateUpdateTerms
    (rules: Rule<UpdateTermsInput> list)
    (input: UpdateTermsInput)
    : Result<DecisionOutcome<UpdateTermsOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = UpdateAuthorized
                  Evaluations = evaluations }
        else
            let reason = failed |> List.map (fun e -> e.RuleId) |> String.concat "; "
            return
                { Outcome = UpdateRejected reason
                  Evaluations = evaluations }
    }

