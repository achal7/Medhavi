/// CA-P-001 Customer Promise Profile Rules
module Medhavi.Promise.UnderstandOrders.CustomerPromiseProfile.Rules

open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Model
open Policies

type ConfigureProfileInput =
    { Cmd: ConfigureCustomerPromiseProfileCmd
      CustomerExists: bool
      Policy: CustomerProfilePolicy }

type UpdateTermsInput =
    { Cmd: UpdateCustomerServiceTermsCmd
      CurrentProfile: CustomerPromiseProfile option
      Policy: CustomerProfilePolicy }

/// BR-P-007: Profile must reference valid enterprise customer
let customerExistencePrerequisite: Rule<ConfigureProfileInput> =
    Rule.create
        Rules.customerPromiseProfileIdentity.Id
        Rules.customerPromiseProfileIdentity.Explanation
        (fun input -> input.CustomerExists)
        (fun input -> $"CustomerId: %s{CustomerId.value input.Cmd.CustomerId}, Exists: %b{input.CustomerExists}")

/// BR-P-052: Default SLA must be within allowable business window
let slaWindowValidity: Rule<ConfigureProfileInput> =
    Rule.create
        Rules.deliveryWindowNonNegativity.Id
        Rules.deliveryWindowNonNegativity.Explanation
        (fun input ->
            input.Cmd.DefaultSlaDays >= input.Policy.MinDefaultSlaDays
            && input.Cmd.DefaultSlaDays <= input.Policy.MaxDefaultSlaDays)
        (fun input -> $"SlaDays: %d{input.Cmd.DefaultSlaDays}, Min: %d{input.Policy.MinDefaultSlaDays}, Max: %d{input.Policy.MaxDefaultSlaDays}")

/// BR-P-051: Daily penalty rate must not exceed policy ceiling
let penaltyRateConstraint: Rule<ConfigureProfileInput> =
    Rule.create
        Rules.penaltyCapUpperBound.Id
        Rules.penaltyCapUpperBound.Explanation
        (fun input ->
            input.Cmd.DailyPenaltyRatePct >= 0m
            && input.Cmd.DailyPenaltyRatePct <= input.Policy.MaxDailyPenaltyRatePct)
        (fun input -> $"DailyPenaltyRate: %M{input.Cmd.DailyPenaltyRatePct}, Max: %M{input.Policy.MaxDailyPenaltyRatePct}")

let configureRules: Rule<ConfigureProfileInput> list =
    [ customerExistencePrerequisite; slaWindowValidity; penaltyRateConstraint ]

/// BR-P-007: Customer profile must exist prior to update
let profileExistencePrerequisite: Rule<UpdateTermsInput> =
    Rule.create
        Rules.customerPromiseProfileIdentity.Id
        Rules.customerPromiseProfileIdentity.Explanation
        (fun input -> input.CurrentProfile.IsSome)
        (fun input -> $"CustomerId: %s{CustomerId.value input.Cmd.CustomerId}")

let updateRules: Rule<UpdateTermsInput> list =
    [ profileExistencePrerequisite ]
