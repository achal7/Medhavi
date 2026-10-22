/// AB-S-110 — Record Supply Explanation Behavior
module Medhavi.Supply.ExplainSupplyDecisions.SupplyExplanation.Behaviors

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers
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
        ArsIdentifiers.Capabilities.explainSupply.Id
        decisionOutcome
        policyId
        policyVersion
        [ "SE-S-120" ]
        (Some summary)

/// AB-S-110: Record Supply Explanation Decider
let composeSupplyExplanation
    (policy: ExplanationPolicy)
    : Decide<SupplyExplanation, ComposeSupplyExplanationCmd, SupplyExplanationEvent> =
    fun (cmd: ComposeSupplyExplanationCmd) (state: SupplyExplanation option) ->
        result {
            let input: ExplanationRuleInput =
                { ExplainedArtifactType = cmd.ExplainedArtifactType
                  ExplainedArtifactId = cmd.ExplainedArtifactId
                  ExplainedArtifactVersion = cmd.ExplainedArtifactVersion
                  ExistingExplanation = state
                  EvidenceRefs = cmd.PreservedEvidenceRefs
                  ReasoningGraph = cmd.StructuredReasoningGraph
                  Confidence = cmd.Confidence
                  Policy = policy }

            let! decision = Decisions.evaluateExplanationRules Rules.explanationRules input

            let explanation: SupplyExplanation =
                { Id = cmd.ExplanationId
                  ExplainedArtifactType = cmd.ExplainedArtifactType
                  ExplainedArtifactId = cmd.ExplainedArtifactId
                  ExplainedArtifactVersion = cmd.ExplainedArtifactVersion
                  State = Created
                  StructuredReasoningGraph = cmd.StructuredReasoningGraph
                  MultiLevelRenderings = cmd.MultiLevelRenderings
                  FactorContributions = cmd.FactorContributions
                  PreservedEvidenceRefs = cmd.PreservedEvidenceRefs
                  RationaleBlocks = cmd.RationaleBlocks
                  IsNegativeExplanation = cmd.IsNegativeExplanation
                  NegativeExplanation = cmd.NegativeExplanation
                  OpaqueModelProvenance = cmd.OpaqueModelProvenance
                  Confidence = cmd.Confidence
                  TemplateVersionReference = cmd.TemplateVersionReference
                  CompositionTimestamp = cmd.CompositionTimestamp }

            let events = [ SupplyExplanationRecorded explanation ]

            let summary =
                sprintf
                    "Supply Explanation RECORDED for %s '%s' (v%d). Confidence: %s (%.1f%%). Template: %s"
                    cmd.ExplainedArtifactType
                    cmd.ExplainedArtifactId
                    cmd.ExplainedArtifactVersion
                    cmd.Confidence.ConfidenceLevel.AsString
                    cmd.Confidence.CompositeIndex
                    cmd.TemplateVersionReference

            return
                buildDecisionTrace
                    (Some policy.PolicyId)
                    (Some policy.Version)
                    "AB-S-110"
                    decision
                    state
                    events
                    summary
        }
