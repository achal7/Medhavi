/// Traces to: DE-D-015
module Medhavi.Demand.LearnFromDemand.DemandLearning.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Demand
open Model
open Rules

/// DE-D-015: Decision Alternatives for Demand Learning Approval
type LearningApprovalAlternative =
    | ApproveLearning
    | RejectLearning

/// DE-D-015 Decision Result payload
type LearningApprovalDecision =
    { SelectedAlternative: LearningApprovalAlternative
      LearningId: DemandLearningId
      Rationale: string }

/// DE-D-015 — Approve Demand Learning
let evaluateLearningApproval
    (rules: Rule<DemandLearningRuleInput> list)
    (input: DemandLearningRuleInput)
    : Result<DecisionOutcome<LearningApprovalDecision>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let passed = evaluations |> List.forall(fun e -> e.Passed)

        let alternative = if passed then ApproveLearning else RejectLearning

        let rationale =
            if passed then
                sprintf
                    "Demand Learning '%s' (%s) meets all policy recurrence and confidence criteria per PO-D-048."
                    (DemandLearningId.value input.LearningId)
                    input.LearningType.AsString
            else
                let failureReasons =
                    evaluations
                    |> List.filter(fun e -> not e.Passed)
                    |> List.map(fun e -> e.Evidence |> String.concat "; ")
                    |> String.concat " | "

                sprintf "Demand Learning '%s' rejected: %s" (DemandLearningId.value input.LearningId) failureReasons

        if not passed then
            return! Error(DomainError.rule(rationale, rule = ArsIdentifiers.Decisions.approveDemandLearning.Id))
        else
            let decision =
                { SelectedAlternative = alternative
                  LearningId = input.LearningId
                  Rationale = rationale }

            return
                { Outcome = decision
                  Evaluations = evaluations }
    }
