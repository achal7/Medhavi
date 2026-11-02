/// CA-P-001 Ingest and Understand Orders Rules
module Medhavi.Promise.UnderstandOrders.OrderBookSnapshot.Rules

open System
open Medhavi.SemanticModel
open Medhavi.Foundation.Contracts
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Model
open Policies

type ValidateOrderLineInput =
    { Cmd: ValidateOrderTermsCmd
      CurrentSnapshot: OrderBookSnapshot option
      TargetLine: OrderLine option
      ItemExists: bool
      DestinationLocationExists: bool
      CustomerProfile: CustomerProfileFact option
      Policy: OrderValidationPolicy }

type CancelOrderLineInput =
    { Cmd: CancelOrderLineCmd
      CurrentSnapshot: OrderBookSnapshot option
      TargetLine: OrderLine option
      Policy: OrderLifecyclePolicy }

/// BR-P-001: Order book snapshot existence prerequisite
let snapshotExistencePrerequisite: Rule<ValidateOrderLineInput> =
    Rule.create
        Rules.orderBookSnapshotIdentity.Id
        Rules.orderBookSnapshotIdentity.Explanation
        (fun input -> input.CurrentSnapshot.IsSome)
        (fun input -> $"SnapshotId: %s{OrderBookSnapshotId.value input.Cmd.SnapshotId}")

/// BR-P-003: Target order line must exist within snapshot
let orderLineExistencePrerequisite: Rule<ValidateOrderLineInput> =
    Rule.create
        Rules.validatedOrderLineIdentity.Id
        Rules.validatedOrderLineIdentity.Explanation
        (fun input -> input.TargetLine.IsSome)
        (fun input -> $"OrderLineId: %s{OrderLineId.value input.Cmd.OrderLineId}")

/// BR-P-020: Order line item must exist in enterprise catalog
let orderLineItemEligibility: Rule<ValidateOrderLineInput> =
    Rule.create
        Rules.orderLineReceptionEligibility.Id
        Rules.orderLineReceptionEligibility.Explanation
        (fun input -> input.ItemExists)
        (fun input ->
            let itemStr = input.TargetLine |> Option.map (fun l -> ItemId.value l.Item) |> Option.defaultValue "None"
            $"Item: %s{itemStr}, Exists: %b{input.ItemExists}")

/// BR-P-003: Destination location must exist within enterprise network
let destinationLocationEligibility: Rule<ValidateOrderLineInput> =
    Rule.create
        Rules.validatedOrderLineIdentity.Id
        Rules.validatedOrderLineIdentity.Explanation
        (fun input -> input.DestinationLocationExists)
        (fun input ->
            let locStr = input.TargetLine |> Option.map (fun l -> LocationId.value l.DestinationLocation) |> Option.defaultValue "None"
            $"Location: %s{locStr}, Exists: %b{input.DestinationLocationExists}")

/// BR-P-021: Customer standing must be active and not in credit hold
let customerStandingEligibility: Rule<ValidateOrderLineInput> =
    Rule.create
        Rules.commercialCustomerStandingEligibility.Id
        Rules.commercialCustomerStandingEligibility.Explanation
        (fun input ->
            match input.CustomerProfile with
            | Some profile -> not profile.CreditHoldActive
            | None -> false)
        (fun input ->
            match input.CustomerProfile with
            | Some p -> $"CreditHoldActive: %b{p.CreditHoldActive}, Tier: %s{p.CustomerTier}"
            | None -> "Customer profile not found")

/// BR-P-040: Order quantity must be positive and within policy limits
let orderQuantityRangeValidity: Rule<ValidateOrderLineInput> =
    Rule.create
        Rules.orderLineQuantityPositivity.Id
        Rules.orderLineQuantityPositivity.Explanation
        (fun input ->
            match input.TargetLine with
            | Some line ->
                let q = Quantity.value line.RequestedQuantity
                q >= input.Policy.MinOrderQuantity && q <= input.Policy.MaxOrderQuantity
            | None -> false)
        (fun input ->
            let q = input.TargetLine |> Option.map (fun l -> Quantity.value l.RequestedQuantity) |> Option.defaultValue 0m
            $"Quantity: %M{q}, Min: %M{input.Policy.MinOrderQuantity}, Max: %M{input.Policy.MaxOrderQuantity}")

/// BR-P-041: Requested delivery date must be in future
let deliveryDateFeasibility: Rule<ValidateOrderLineInput> =
    Rule.create
        Rules.orderDeliveryDateFeasibility.Id
        Rules.orderDeliveryDateFeasibility.Explanation
        (fun input ->
            match input.TargetLine with
            | Some line ->
                if input.Policy.EnforceFutureDeliveryDate then
                    (Timestamp.value line.RequestedDeliveryDate) > (Timestamp.value input.Cmd.EvaluationTime)
                else
                    true
            | None -> false)
        (fun input ->
            let reqDate =
                input.TargetLine
                |> Option.map (fun l -> (Timestamp.value l.RequestedDeliveryDate).ToString("o"))
                |> Option.defaultValue "None"
            let evalDate = (Timestamp.value input.Cmd.EvaluationTime).ToString("o")
            $"Requested: %s{reqDate}, EvalTime: %s{evalDate}")

let validationRules: Rule<ValidateOrderLineInput> list =
    [ snapshotExistencePrerequisite
      orderLineExistencePrerequisite
      orderLineItemEligibility
      destinationLocationEligibility
      customerStandingEligibility
      orderQuantityRangeValidity
      deliveryDateFeasibility ]

/// BR-P-022: Order cancellation eligibility check
let cancellationTimeFenceEligibility: Rule<CancelOrderLineInput> =
    Rule.create
        Rules.orderCancellationTimeFenceEligibility.Id
        Rules.orderCancellationTimeFenceEligibility.Explanation
        (fun input ->
            match input.TargetLine with
            | Some line ->
                let deliveryTime = Timestamp.value line.RequestedDeliveryDate
                let cancelTime = Timestamp.value input.Cmd.CancellationTime
                let hoursRemaining = (deliveryTime - cancelTime).TotalHours
                hoursRemaining >= float input.Policy.CancellationTimeFenceHours
            | None -> false)
        (fun input ->
            let hours =
                input.TargetLine
                |> Option.map (fun l -> ((Timestamp.value l.RequestedDeliveryDate) - (Timestamp.value input.Cmd.CancellationTime)).TotalHours)
                |> Option.defaultValue 0.0
            $"HoursRemaining: %f{hours}, RequiredFenceHours: %d{input.Policy.CancellationTimeFenceHours}")

let cancellationRules: Rule<CancelOrderLineInput> list =
    [ cancellationTimeFenceEligibility ]

