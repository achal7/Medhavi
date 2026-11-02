/// CA-P-007 Assess Promise Risk and Resilience — Policies
/// Traces to: PO-P-012 Promise Risk Assessment Policy
module Medhavi.Promise.AssessPromiseRisk.PromiseRiskAssessment.Policies

open Medhavi.Promise.ArsIdentifiers

type PromiseRiskAssessmentPolicy =
    { PolicyId: string
      Version: int
      BreachProbabilityWarningThresholdPct: decimal
      BreachProbabilityEscalationThresholdPct: decimal
      BufferConsumptionWarningRatio: decimal
      BufferConsumptionCriticalRatio: decimal
      RecoveryThresholdPct: decimal
      DefaultTelemetryLookbackHours: int }

let defaultPolicy: PromiseRiskAssessmentPolicy =
    { PolicyId = Policies.promiseRiskAssessmentPolicy.Id
      Version = 1
      BreachProbabilityWarningThresholdPct = 50.0m
      BreachProbabilityEscalationThresholdPct = 70.0m
      BufferConsumptionWarningRatio = 0.80m
      BufferConsumptionCriticalRatio = 1.00m
      RecoveryThresholdPct = 20.0m
      DefaultTelemetryLookbackHours = 72 }

