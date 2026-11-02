/// CA-P-006 Establish Commitments Rules
/// Traces to: BR-P-004, BR-P-042, BR-P-048, BR-P-022
module Medhavi.Promise.EstablishCommitments.OrderCommitment.Rules

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Model
open Policies

type EstablishCommitmentInput =
    { Cmd: EstablishCommitmentCmd
      CustomerExists: bool
      ItemExists: bool
      DestinationExists: bool
      Policy: CommitmentGovernancePolicy }

type ReviseCommitmentInput =
    { Cmd: ReviseCommitmentCmd
      CurrentCommitment: OrderCommitment option
      Policy: CommitmentGovernancePolicy }

type CancelCommitmentInput =
    { Cmd: CancelCommitmentCmd
      CurrentCommitment: OrderCommitment option
      Policy: CommitmentGovernancePolicy }

/// BR-P-004: Commitment prerequisites
let commitmentPrerequisitesRule: Rule<EstablishCommitmentInput> =
    Rule.create
        Rules.orderCommitmentIdentity.Id
        Rules.orderCommitmentIdentity.Explanation
        (fun input -> input.CustomerExists && input.ItemExists && input.DestinationExists)
        (fun input -> $"CustExists: %b{input.CustomerExists}, ItemExists: %b{input.ItemExists}, DestExists: %b{input.DestinationExists}")

/// BR-P-042: Sourcing leg quantity conservation
let sourcingLegConservationRule: Rule<EstablishCommitmentInput> =
    Rule.create
        Rules.sourcingLegConservationInvariant.Id
        Rules.sourcingLegConservationInvariant.Explanation
        (fun input ->
            let totalLegQty = input.Cmd.SourcingLegs |> List.sumBy (fun l -> Quantity.value l.SourcedQuantity)
            totalLegQty = Quantity.value input.Cmd.PromisedQuantity)
        (fun input ->
            let totalLegQty = input.Cmd.SourcingLegs |> List.sumBy (fun l -> Quantity.value l.SourcedQuantity)
            $"TotalLegQty: %M{totalLegQty}, PromisedQty: %M{Quantity.value input.Cmd.PromisedQuantity}")

let establishRules: Rule<EstablishCommitmentInput> list =
    [ commitmentPrerequisitesRule; sourcingLegConservationRule ]

/// BR-P-048: First Promise Permanence & revision limit check
let revisionPermissibilityRule: Rule<ReviseCommitmentInput> =
    Rule.create
        Rules.firstPromisePermanenceInvariant.Id
        Rules.firstPromisePermanenceInvariant.Explanation
        (fun input ->
            match input.CurrentCommitment with
            | Some c ->
                (c.LifecycleState = Committed || c.LifecycleState = Revised)
                && c.CurrentRevisionIndex < input.Policy.MaxAllowedRevisions
            | None -> false)
        (fun input ->
            match input.CurrentCommitment with
            | Some c -> $"State: %A{c.LifecycleState}, CurrentRevIndex: %d{c.CurrentRevisionIndex}, MaxRevs: %d{input.Policy.MaxAllowedRevisions}"
            | None -> "CommitmentNotFound")

let reviseRules: Rule<ReviseCommitmentInput> list =
    [ revisionPermissibilityRule ]

/// BR-P-022: Cancellation time fence eligibility
let cancellationTimeFenceRule: Rule<CancelCommitmentInput> =
    Rule.create
        Rules.orderCancellationTimeFenceEligibility.Id
        Rules.orderCancellationTimeFenceEligibility.Explanation
        (fun input ->
            match input.CurrentCommitment with
            | Some c ->
                let latestDelivery = c.LatestRevision.CommittedDeliveryDate
                let hoursToDelivery = ((Timestamp.value latestDelivery) - (Timestamp.value input.Cmd.CancellationTime)).TotalHours
                hoursToDelivery >= float input.Policy.FreezeWindowHours
            | None -> false)
        (fun input ->
            match input.CurrentCommitment with
            | Some c ->
                let latestDelivery = c.LatestRevision.CommittedDeliveryDate
                let hours = ((Timestamp.value latestDelivery) - (Timestamp.value input.Cmd.CancellationTime)).TotalHours
                $"HoursToDelivery: %f{hours}, FreezeWindow: %d{input.Policy.FreezeWindowHours}"
            | None -> "CommitmentNotFound")

let cancelRules: Rule<CancelCommitmentInput> list =
    [ cancellationTimeFenceRule ]

