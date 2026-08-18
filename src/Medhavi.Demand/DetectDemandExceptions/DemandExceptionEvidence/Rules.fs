/// BR-D-311 — Demand Exception Business Rules
module Medhavi.Demand.DetectDemandExceptions.DemandExceptionEvidence.Rules

open Medhavi.Foundation.Contracts
open Medhavi.Demand
open Medhavi.Demand.ArsIdentifiers
open Policies
open Model

/// Typed input context for evaluating demand exception rules
type ExceptionRuleInput =
    { ExceptionType: DemandExceptionType
      MetricValue: decimal
      HistoricalValues: decimal list
      IsCurrentlyActive: bool
      Policy: DemandExceptionEvidencePolicy }

/// BR-D-311: Demand exception evidence must meet governed detection or resolution criteria in PO-D-044
let policyComplianceRule: Rule<ExceptionRuleInput> =
    Rule.create
        ArsIdentifiers.Rules.demandExceptionEvidenceRequirement.Id
        ArsIdentifiers.Rules.demandExceptionEvidenceRequirement.Explanation
        (fun input ->
            not (System.String.IsNullOrWhiteSpace input.Policy.PolicyId)
            && input.Policy.ForecastBiasCriticalThreshold > 0.0m
            && input.Policy.WapeCriticalThreshold > 0.0m)
        (fun input ->
            if not (System.String.IsNullOrWhiteSpace input.Policy.PolicyId) then
                $"Exception detection governed by active policy '{input.Policy.PolicyId}' (v{input.Policy.Version})"
            else
                "No active Demand Exception Policy configured")

let exceptionRules: Rule<ExceptionRuleInput> list =
    [ policyComplianceRule ]
