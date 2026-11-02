/// CA-P-007 Assess Promise Risk and Resilience — Rules
/// Traces to: BR-P-008, BR-P-064, BR-P-083, BR-P-084
module Medhavi.Promise.AssessPromiseRisk.PromiseRiskAssessment.Rules

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Model
open Policies

type AssessRiskInput =
    { Cmd: AssessCommitmentRiskCmd
      CommitmentExists: bool
      ItemExists: bool
      Policy: PromiseRiskAssessmentPolicy }

type EscalateRiskInput =
    { Cmd: EscalateRiskBreachCmd
      CurrentAssessment: PromiseRiskAssessment option
      Policy: PromiseRiskAssessmentPolicy }

/// BR-P-008: Promise risk assessment identity and commitment existence prerequisite
let riskAssessmentPrerequisitesRule: Rule<AssessRiskInput> =
    Rule.create
        Rules.promiseRiskAssessmentIdentity.Id
        Rules.promiseRiskAssessmentIdentity.Explanation
        (fun input -> input.CommitmentExists && input.ItemExists)
        (fun input -> $"CommitmentExists: %b{input.CommitmentExists}, ItemExists: %b{input.ItemExists}")

let assessRules: Rule<AssessRiskInput> list =
    [ riskAssessmentPrerequisitesRule ]

/// BR-P-064: Commitment breach escalation threshold eligibility
let breachEscalationRule: Rule<EscalateRiskInput> =
    Rule.create
        Rules.commitmentBreachEscalationRule.Id
        Rules.commitmentBreachEscalationRule.Explanation
        (fun input ->
            match input.CurrentAssessment with
            | Some a ->
                (a.AssessmentStatus = Evaluated || a.AssessmentStatus = MonitoredActive)
                && a.BreachProbabilityPct >= input.Policy.BreachProbabilityEscalationThresholdPct
                && not a.AutoEscalationTriggered
            | None -> false)
        (fun input ->
            match input.CurrentAssessment with
            | Some a -> $"Probability: %M{a.BreachProbabilityPct}, Threshold: %M{input.Policy.BreachProbabilityEscalationThresholdPct}, AlreadyEscalated: %b{a.AutoEscalationTriggered}"
            | None -> "AssessmentNotFound")

let escalateRules: Rule<EscalateRiskInput> list =
    [ breachEscalationRule ]

