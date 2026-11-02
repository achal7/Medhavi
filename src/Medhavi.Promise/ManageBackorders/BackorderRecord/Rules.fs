/// CA-P-008 Manage Backorders — Rules
/// Traces to: BR-P-009, BR-P-050
module Medhavi.Promise.ManageBackorders.BackorderRecord.Rules

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Model
open Policies

type EnqueueBackorderInput =
    { Cmd: EnqueueBackorderCmd
      CustomerExists: bool
      ItemExists: bool
      LocationExists: bool
      Policy: BacklogGovernancePolicy }

type ResolveBackorderInput =
    { Cmd: ResolveBackorderCmd
      CurrentRecord: BackorderRecord option
      Policy: BacklogGovernancePolicy }

/// BR-P-009: Backorder record identity and entity prerequisites
let backorderPrerequisitesRule: Rule<EnqueueBackorderInput> =
    Rule.create
        Rules.backorderQueueIdentity.Id
        Rules.backorderQueueIdentity.Explanation
        (fun input -> input.CustomerExists && input.ItemExists && input.LocationExists)
        (fun input -> $"CustExists: %b{input.CustomerExists}, ItemExists: %b{input.ItemExists}, LocExists: %b{input.LocationExists}")

let enqueueRules: Rule<EnqueueBackorderInput> list =
    [ backorderPrerequisitesRule ]

/// BR-P-050: Inbound supply netting must serve backorders in priority sequence
let fairShareNettingRule: Rule<ResolveBackorderInput> =
    Rule.create
        Rules.backorderQueueFairShareInvariant.Id
        Rules.backorderQueueFairShareInvariant.Explanation
        (fun input ->
            match input.CurrentRecord with
            | Some r ->
                r.LifecycleState = Queued || r.LifecycleState = UnderReEvaluation
            | None -> false)
        (fun input ->
            match input.CurrentRecord with
            | Some r -> $"State: %A{r.LifecycleState}"
            | None -> "RecordNotFound")

let resolveRules: Rule<ResolveBackorderInput> list =
    [ fairShareNettingRule ]

