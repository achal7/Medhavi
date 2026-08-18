/// Traces to: AB-D-017 Establish Demand Learning
module Medhavi.Demand.LearnFromDemand.DemandLearning.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Demand
open Medhavi.Demand.ArsIdentifiers
open Model
open Policies
open Rules
open Decisions

let private buildDecisionTrace policyId policyVersion decisionId decisionOutcome state events summary =
    buildDecisionWithTrace
        evolve
        state
        events
        decisionId
        []
        ArsIdentifiers.Capabilities.learnFromDemand.Id
        decisionOutcome
        policyId
        policyVersion
        [ ArsIdentifiers.SemanticObjects.demandLearning.Id ]
        (Some summary)

/// AB-D-017: Establish Demand Learning Decider (DE-D-015)
let establishLearning
    (policy: LearningAnalysisPolicy)
    : Decide<DemandLearning, EstablishLearningCmd, DemandLearningEvent> =
    fun (cmd: EstablishLearningCmd) (state: DemandLearning option) ->
        result {
            let input: DemandLearningRuleInput =
                { LearningId = cmd.LearningId
                  Scope = cmd.Scope
                  LearningType = cmd.LearningType
                  PatternConfidence = cmd.PatternConfidence
                  SupportingEvidenceCount = cmd.SupportingEvidence.Length
                  ExistingLearning = state
                  Policy = policy }

            let! decision = Decisions.evaluateLearningApproval Rules.learningRules input

            let learning: DemandLearning =
                { Id = cmd.LearningId
                  Scope = cmd.Scope
                  LearningType = cmd.LearningType
                  LearningStatement = cmd.LearningStatement
                  PatternConfidence = cmd.PatternConfidence
                  InterventionConfidence = cmd.InterventionConfidence
                  SupportingEvidence = cmd.SupportingEvidence
                  ImprovementOpportunities = cmd.ImprovementOpportunities
                  PolicyVersion = cmd.PolicyVersion
                  CreatedAt = cmd.Timestamp }

            let events = [ DemandLearningEstablished learning ]
            let summary = decision.Outcome.Rationale

            return
                buildDecisionTrace
                    (Some policy.PolicyId)
                    (Some policy.Version)
                    ArsIdentifiers.Decisions.approveDemandLearning.Id
                    decision
                    state
                    events
                    summary
        }
