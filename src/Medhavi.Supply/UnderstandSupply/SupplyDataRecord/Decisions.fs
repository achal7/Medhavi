module Medhavi.Supply.UnderstandSupply.SupplyDataRecord.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Supply.ArsIdentifiers

type AcceptanceOutcome =
    | Accept
    | Quarantine of reasons: string list
    | Reject of reasons: string list

/// DE-S-001: Accept Supply Data
let evaluateSupplyData
    (rules: Rule<Rules.EvaluateInput> list)
    (input: Rules.EvaluateInput)
    : Result<DecisionOutcome<AcceptanceOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter(fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = Accept
                  Evaluations = evaluations }
        else
            let reasons = failed |> List.map(fun e -> sprintf "[%s] %s" e.RuleId (e.Evidence |> String.concat ", "))

            // Critical failures produce Reject; marginal/borderline produce Quarantine
            let criticalRuleIds =
                Set.ofList
                    [ Rules.supplyDataRecordIdentity.Id
                      Rules.supplyDataEvaluationEligibility.Id
                      Rules.duplicateDataDetection.Id
                      Rules.supplyQuantityRangeValidity.Id ]

            let hasCriticalFailure =
                failed |> List.exists(fun e -> Set.contains e.RuleId criticalRuleIds)
                || (input.SourceReliability < input.Policy.BorderlineSourceReliability)

            if hasCriticalFailure then
                return
                    { Outcome = Reject reasons
                      Evaluations = evaluations }
            else
                return
                    { Outcome = Quarantine reasons
                      Evaluations = evaluations }
    }
