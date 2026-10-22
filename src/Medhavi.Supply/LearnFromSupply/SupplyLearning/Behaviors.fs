// Traceability: AB-S-120 Record Supply Learning, AB-S-121 Record Learning Outcome
module Medhavi.Supply.LearnFromSupply.SupplyLearning.Behaviors

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Supply.ArsIdentifiers
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
        Capabilities.learnFromSupply.Id
        decisionOutcome
        policyId
        policyVersion
        [ SemanticObjects.supplyLearning.Id ]
        (Some summary)

/// AB-S-120: Record Supply Learning Decider
let recordLearning (policy: LearningPolicy) : Decide<SupplyLearning, RecordSupplyLearningCmd, SupplyLearningEvent> =
    fun (cmd: RecordSupplyLearningCmd) (state: SupplyLearning option) ->
        result {
            let distinctSources =
                cmd.SupportingEvidence |> List.map(fun e -> e.CapabilityId) |> List.distinct |> List.length

            let input: RecordLearningRuleInput =
                {
                    LearningId = cmd.LearningId
                    SubjectType = cmd.SubjectType
                    SubjectId = cmd.SubjectId
                    LearningType = cmd.LearningType
                    RecurrenceCount = cmd.RecurrenceCount
                    RecurrenceFrequency = cmd.RecurrenceFrequency
                    PatternConfidence = cmd.PatternConfidence
                    CrossCapabilitySourceCount = distinctSources
                    ExistingLearning = state
                    Policy = policy
                }

            let! decision = Decisions.evaluateRecordEligibility Rules.recordLearningRules input

            let supersededId =
                match state with
                | Some prev when prev.LifecycleState = LearningLifecycleState.OutcomeRecorded -> Some prev.Id
                | _ -> None

            let learning: SupplyLearning =
                {
                    Id = cmd.LearningId
                    SubjectType = cmd.SubjectType
                    SubjectId = cmd.SubjectId
                    LearningType = cmd.LearningType
                    Version = 1L
                    LifecycleState = LearningLifecycleState.PendingOutcome
                    PatternDescription = cmd.PatternDescription
                    RecurrenceCount = cmd.RecurrenceCount
                    RecurrenceFrequency = cmd.RecurrenceFrequency
                    StatisticalSignificance = cmd.StatisticalSignificance
                    Scope = cmd.Scope
                    TimeHorizon = cmd.TimeHorizon
                    LearningStatement = cmd.LearningStatement
                    PatternConfidence = cmd.PatternConfidence
                    PatternConfidenceScore = cmd.PatternConfidenceScore
                    ImprovementOpportunities = cmd.ImprovementOpportunities
                    Outcome = LearningOutcome.Pending
                    OutcomeTimestamp = None
                    OutcomeAuthority = None
                    OutcomeRationale = None
                    SupportingEvidence = cmd.SupportingEvidence
                    SourceEvidenceVersionSet = cmd.SourceEvidenceVersionSet
                    PolicyVersion = cmd.PolicyVersion
                    GenerationTimestamp = cmd.Timestamp
                    SupersededLearningId = supersededId
                }

            let events =
                match supersededId with
                | Some prevId ->
                    [
                        SupplyLearningSuperseded
                            {
                                LearningId = prevId
                                SupersededByLearningId = cmd.LearningId
                                Timestamp = cmd.Timestamp
                            }
                        SupplyLearningRecorded learning
                    ]
                | None -> [ SupplyLearningRecorded learning ]

            let summary = decision.Outcome.Rationale

            return
                buildDecisionTrace
                    (Some policy.PolicyId)
                    (Some policy.Version)
                    Decisions.evaluateSupplyLearning.Id
                    decision
                    state
                    events
                    summary
        }

/// AB-S-121: Record Learning Outcome Decider
let recordOutcome (policy: LearningPolicy) : Decide<SupplyLearning, RecordLearningOutcomeCmd, SupplyLearningEvent> =
    fun (cmd: RecordLearningOutcomeCmd) (state: SupplyLearning option) ->
        result {
            let input: RecordOutcomeRuleInput =
                {
                    LearningId = cmd.LearningId
                    ProposedOutcome = cmd.Outcome
                    Authority = cmd.Authority
                    ExistingLearning = state
                    Policy = policy
                }

            let! decision = Decisions.evaluateOutcomeEligibility Rules.recordOutcomeRules input

            let payload: OutcomePayload =
                {
                    LearningId = cmd.LearningId
                    Outcome = cmd.Outcome
                    Authority = cmd.Authority
                    Rationale = cmd.Rationale
                    Timestamp = cmd.Timestamp
                }

            let events = [ SupplyLearningOutcomeRecorded payload ]
            let summary = decision.Outcome.Rationale

            return
                buildDecisionTrace
                    (Some policy.PolicyId)
                    (Some policy.Version)
                    Decisions.evaluateSupplyLearning.Id
                    decision
                    state
                    events
                    summary
        }
