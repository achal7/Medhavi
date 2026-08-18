/// AB-D-016 — Establish Demand Explanation Behavior
module Medhavi.Demand.ExplainDemand.DemandExplanation.Behaviors

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Demand
open Medhavi.Demand.ArsIdentifiers
open Policies
open Model
open Rules

let private buildDecisionTrace policyId policyVersion decisionId decisionOutcome state events summary =
    buildDecisionWithTrace
        evolve
        state
        events
        decisionId
        []
        ArsIdentifiers.Capabilities.explainDemand.Id
        decisionOutcome
        policyId
        policyVersion
        [ "SE-D-010" ]
        (Some summary)

/// AB-D-016: Establish Demand Explanation Decider (DE-D-014)
let establishExplanation
    (policy: ExplanationGovernancePolicy)
    : Decide<DemandExplanation, EstablishExplanationCmd, DemandExplanationEvent> =
    fun (cmd: EstablishExplanationCmd) (state: DemandExplanation option) ->
        result {
            let input: ExplanationRuleInput =
                { ExplainedArtifactType = cmd.ExplainedArtifactType
                  ExplainedArtifactId = cmd.ExplainedArtifactId
                  Version = cmd.Version
                  ExistingExplanation = state
                  ExplainabilityScore = cmd.ExplainabilityScore
                  Policy = policy }

            let! decision = Decisions.evaluateExplanationApproval Rules.explanationRules input

            let explanation: DemandExplanation =
                { Id = cmd.ExplanationId
                  ExplainedArtifactType = cmd.ExplainedArtifactType
                  ExplainedArtifactId = cmd.ExplainedArtifactId
                  Version = cmd.Version
                  StructuredReasoningGraph = cmd.StructuredReasoningGraph
                  MultiLevelRenderings = cmd.MultiLevelRenderings
                  FactorContributions = cmd.FactorContributions
                  PreservedEvidenceRefs = cmd.PreservedEvidenceRefs
                  TemplateVersion = cmd.TemplateVersion
                  ExplainabilityScore = cmd.ExplainabilityScore
                  WhatIfAssumption = cmd.WhatIfAssumption
                  CreatedAt = cmd.CreationTime }

            let events = [ DemandExplanationEstablished explanation ]

            let summary =
                sprintf
                    "Demand Explanation ESTABLISHED for %s '%s' (v%d). Explainability Score: %.1f%%. Template: %s"
                    cmd.ExplainedArtifactType
                    cmd.ExplainedArtifactId
                    cmd.Version
                    cmd.ExplainabilityScore
                    cmd.TemplateVersion

            return
                buildDecisionTrace
                    (Some policy.PolicyId)
                    (Some policy.Version)
                    ArsIdentifiers.Decisions.approveDemandExplanation.Id
                    decision
                    state
                    events
                    summary
        }
