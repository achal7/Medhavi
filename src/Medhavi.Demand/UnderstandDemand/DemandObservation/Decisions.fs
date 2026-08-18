module Medhavi.Demand.UnderstandDemand.DemandObservation.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Demand.ArsIdentifiers
open Rules

type AcceptanceOutcome =
    | Accept
    | Quarantine of reasons: string list
    | Reject of reasons: string list

/// DE-D-001: Accept Demand Observation
let evaluateObservation
    (rules: Rule<EvaluateInput> list)
    (input: EvaluateInput)
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

            // Prerequisite failures result in hard Reject. Quality failures result in Quarantine.
            let prerequisiteRuleIds =
                Set.ofList
                    [ Rules.observationExistencePrerequisite.Id
                      Rules.receivedStatePrerequisite.Id ]

            let hasHardFailure = failed |> List.exists(fun e -> Set.contains e.RuleId prerequisiteRuleIds)

            if hasHardFailure then
                return
                    { Outcome = Reject reasons
                      Evaluations = evaluations }
            else
                return
                    { Outcome = Quarantine reasons
                      Evaluations = evaluations }
    }
