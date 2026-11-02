/// CA-P-009 Resolve Exceptions — Policies
/// Traces to: PO-P-014 Disruption Mitigation Policy, PO-P-015 Disruption Escalation Policy
module Medhavi.Promise.ResolveExceptions.PromiseExceptionAssessment.Policies

open Medhavi.Promise.ArsIdentifiers

type DisruptionMitigationPolicy =
    { PolicyId: string
      Version: int
      MaxExpediteCostCap: decimal
      RequireManagerSignOffAboveCost: decimal
      MinAcceptableMarginPct: decimal }

let defaultPolicy: DisruptionMitigationPolicy =
    { PolicyId = Policies.disruptionMitigationPolicy.Id
      Version = 1
      MaxExpediteCostCap = 5000.0m
      RequireManagerSignOffAboveCost = 2000.0m
      MinAcceptableMarginPct = 10.0m }

