/// CA-P-009 Resolve Exceptions — Rules
/// Traces to: BR-P-010, BR-P-065
module Medhavi.Promise.ResolveExceptions.PromiseExceptionAssessment.Rules

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Model
open Policies

type RaiseExceptionInput =
    { Cmd: RaisePromiseExceptionCmd
      CommitmentExists: bool
      Policy: DisruptionMitigationPolicy }

type ResolveExceptionInput =
    { Cmd: ResolvePromiseExceptionCmd
      CurrentAssessment: PromiseExceptionAssessment option
      Policy: DisruptionMitigationPolicy }

/// BR-P-010: Promise exception incident identity and commitment existence prerequisite
let exceptionPrerequisitesRule: Rule<RaiseExceptionInput> =
    Rule.create
        Rules.promiseExceptionIncidentIdentity.Id
        Rules.promiseExceptionIncidentIdentity.Explanation
        (fun input -> input.CommitmentExists)
        (fun input -> $"CommitmentExists: %b{input.CommitmentExists}")

let raiseRules: Rule<RaiseExceptionInput> list =
    [ exceptionPrerequisitesRule ]

/// BR-P-065: Disruption mitigation cost-benefit justification rule
let costBenefitJustificationRule: Rule<ResolveExceptionInput> =
    Rule.create
        Rules.disruptionMitigationExpediteRule.Id
        Rules.disruptionMitigationExpediteRule.Explanation
        (fun input ->
            match input.CurrentAssessment with
            | Some a ->
                match a.CandidatePlans |> List.tryFind (fun p -> p.PlanId = input.Cmd.SelectedPlanId) with
                | Some plan ->
                    plan.EstimatedExpediteCost <= input.Policy.MaxExpediteCostCap
                    && plan.ProjectedGrossMarginPct >= input.Policy.MinAcceptableMarginPct
                | None -> false
            | None -> false)
        (fun input ->
            match input.CurrentAssessment with
            | Some a ->
                match a.CandidatePlans |> List.tryFind (fun p -> p.PlanId = input.Cmd.SelectedPlanId) with
                | Some p -> $"PlanId: %s{p.PlanId}, Cost: %M{p.EstimatedExpediteCost}, Cap: %M{input.Policy.MaxExpediteCostCap}, Margin: %M{p.ProjectedGrossMarginPct}%%"
                | None -> "PlanNotFound"
            | None -> "AssessmentNotFound")

let resolveRules: Rule<ResolveExceptionInput> list =
    [ costBenefitJustificationRule ]

