/// CA-P-004 Evaluate Multi-Option ATP Rules
/// Traces to: BR-P-026, BR-P-060, BR-P-062, BR-P-081
module Medhavi.Promise.EvaluateATP.Rules

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Model
open Policies

type EvaluateATPInput =
    { Cmd: EvaluateATPOptionsCmd
      ItemExists: bool
      DestinationExists: bool
      CustomerExists: bool
      AvailableStockCount: int
      Policy: MultiOptionPromisingPolicy }

/// BR-P-026: Sourcing network route eligibility
let networkRouteEligibilityRule: Rule<EvaluateATPInput> =
    Rule.create
        Rules.multiOptionAtpNetworkRouteEligibility.Id
        Rules.multiOptionAtpNetworkRouteEligibility.Explanation
        (fun input -> input.ItemExists && input.DestinationExists && input.CustomerExists)
        (fun input -> $"ItemExists: %b{input.ItemExists}, DestExists: %b{input.DestinationExists}, StockCount: %d{input.AvailableStockCount}")

let atpEvaluationRules: Rule<EvaluateATPInput> list =
    [ networkRouteEligibilityRule ]

