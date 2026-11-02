/// CA-P-002 Allocate Supply and Manage Priority Decisions
/// Traces to: DE-P-003 Establish Allocation Quota, DE-P-004 Score Dynamic Order Priority
module Medhavi.Promise.AllocateSupply.AllocationPool.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Promise.ArsIdentifiers
open Model
open Rules

type EstablishQuotaOutcome =
    | QuotaEstablished
    | QuotaRejected of reasons: string list

/// DE-P-003: Establish Allocation Quota
let evaluateEstablishQuota
    (rules: Rule<InitializePoolInput> list)
    (input: InitializePoolInput)
    : Result<DecisionOutcome<EstablishQuotaOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = QuotaEstablished
                  Evaluations = evaluations }
        else
            let reasons = failed |> List.map (fun e -> sprintf "[%s] %s" e.RuleId (e.Evidence |> String.concat ", "))
            return
                { Outcome = QuotaRejected reasons
                  Evaluations = evaluations }
    }

type DrawQuotaOutcome =
    | DrawAuthorized
    | DrawRejected of reason: string

let evaluateDrawQuota
    (rules: Rule<DrawQuotaInput> list)
    (input: DrawQuotaInput)
    : Result<DecisionOutcome<DrawQuotaOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = DrawAuthorized
                  Evaluations = evaluations }
        else
            let reason = failed |> List.map (fun e -> e.RuleId) |> String.concat "; "
            return
                { Outcome = DrawRejected $"Quota draw rejected: insufficient available quota or violating overdraw bounds ({reason})"
                  Evaluations = evaluations }
    }

type RebalanceOutcome =
    | RebalanceAuthorized
    | RebalanceRejected of reason: string

let evaluateRebalance
    (rules: Rule<RebalanceInput> list)
    (input: RebalanceInput)
    : Result<DecisionOutcome<RebalanceOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = RebalanceAuthorized
                  Evaluations = evaluations }
        else
            let reason = failed |> List.map (fun e -> e.RuleId) |> String.concat "; "
            return
                { Outcome = RebalanceRejected $"Rebalance unauthorized: {reason}"
                  Evaluations = evaluations }
    }

type DynamicPriorityOutcome =
    | PriorityScored of score: DynamicOrderPriority
    | PriorityRejected of reason: string

/// DE-P-004: Score Dynamic Order Priority
let evaluateDynamicPriority
    (rules: Rule<CalculatePriorityInput> list)
    (input: CalculatePriorityInput)
    (score: DynamicOrderPriority)
    : Result<DecisionOutcome<DynamicPriorityOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = PriorityScored score
                  Evaluations = evaluations }
        else
            let reason = failed |> List.map (fun e -> e.RuleId) |> String.concat "; "
            return
                { Outcome = PriorityRejected $"Dynamic priority calculation rejected: {reason}"
                  Evaluations = evaluations }
    }

