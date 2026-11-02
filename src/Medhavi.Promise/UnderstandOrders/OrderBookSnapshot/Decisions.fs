/// CA-P-001 Ingest and Understand Orders Decisions
module Medhavi.Promise.UnderstandOrders.OrderBookSnapshot.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Promise.ArsIdentifiers
open Rules

type OrderIngestionOutcome =
    | IngestionAccepted

type OrderLineValidationOutcome =
    | Validated
    | ValidationFailed of reasons: string list

/// DE-P-001: Validate Order Line
let evaluateOrderLine
    (rules: Rule<ValidateOrderLineInput> list)
    (input: ValidateOrderLineInput)
    : Result<DecisionOutcome<OrderLineValidationOutcome>, DomainError> =
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

type CancellationOutcome =
    | CancellationAuthorized
    | CancellationRejected of reason: string

/// DE-P-002: Authorize Order Cancellation
let evaluateOrderCancellation
    (rules: Rule<CancelOrderLineInput> list)
    (input: CancelOrderLineInput)
    : Result<DecisionOutcome<CancellationOutcome>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let failed = evaluations |> List.filter (fun e -> not e.Passed)

        if failed.IsEmpty then
            return
                { Outcome = CancellationAuthorized
                  Evaluations = evaluations }
        else
            let reason = failed |> List.map (fun e -> e.RuleId) |> String.concat "; "
            return
                { Outcome = CancellationRejected $"Cancellation outside permissible time-fence ({reason})"
                  Evaluations = evaluations }
    }

