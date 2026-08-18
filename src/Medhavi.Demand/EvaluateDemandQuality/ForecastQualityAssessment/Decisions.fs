/// DE-D-011 — Publish Forecast Quality Assessment Decision
module Medhavi.Demand.EvaluateDemandQuality.ForecastQualityAssessment.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Demand
open Rules
open Policies
open Model

/// Evaluates DE-D-011: Publish Forecast Quality Assessment against governance rules
let evaluatePublication
    (rules: Rule<PublicationRuleInput> list)
    (input: PublicationRuleInput)
    : Result<DecisionOutcome<ForecastQualityAssessmentVersion>, DomainError> =
    result {
        let! targetVersion =
            match input.Assessment.Versions |> List.tryFind(fun v -> v.VersionNumber = input.TargetVersionNumber) with
            | Some v -> Ok v
            | None ->
                Error(DomainError.validation $"Target version v{input.TargetVersionNumber} does not exist in aggregate")

        let! evaluations = Rule.evaluateAll rules input

        let allPassed = evaluations |> List.forall(fun e -> e.Passed)

        if not allPassed then
            let failureMsg =
                evaluations
                |> List.filter(fun e -> not e.Passed)
                |> List.map(fun e -> e.Evidence |> String.concat "; ")
                |> String.concat " | "

            return!
                Error(DomainError.rule(failureMsg, rule = ArsIdentifiers.Decisions.publishForecastQualityAssessment.Id))
        else
            return
                { Outcome = targetVersion
                  Evaluations = evaluations }
    }
