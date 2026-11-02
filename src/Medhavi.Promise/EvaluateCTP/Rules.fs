/// CA-P-005 Evaluate Capable-to-Promise Rules
/// Traces to: BR-P-027, BR-P-049, BR-P-082
module Medhavi.Promise.EvaluateCTP.Rules

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Model
open Policies

type EvaluateCTPInput =
    { Cmd: EvaluateCTPOptionsCmd
      ItemExists: bool
      PlantExists: bool
      CustomerExists: bool
      HasBomComponents: bool
      HasCapacitySlots: bool
      Policy: CapableToPromisePolicy }

/// BR-P-027: CTP manufacturing eligibility
let ctpEligibilityRule: Rule<EvaluateCTPInput> =
    Rule.create
        Rules.capableToPromiseManufacturingEligibility.Id
        Rules.capableToPromiseManufacturingEligibility.Explanation
        (fun input -> input.ItemExists && input.PlantExists && input.CustomerExists && input.HasBomComponents)
        (fun input -> $"ItemExists: %b{input.ItemExists}, PlantExists: %b{input.PlantExists}, HasBOM: %b{input.HasBomComponents}")

let ctpRules: Rule<EvaluateCTPInput> list =
    [ ctpEligibilityRule ]

