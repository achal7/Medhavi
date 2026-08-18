/// Model Demand Interventions Aggregate Behaviors
/// Traces to: AB-D-018 Assess Demand Intervention Impact, AB-D-019 Publish Demand Intervention Impact (Specification Chapter 4.3.1)
module Medhavi.Demand.ModelDemandInterventions.DemandInterventionImpact.Behaviors

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Foundation.Failure
open Medhavi.Demand
open Medhavi.Demand.ArsIdentifiers
open Model
open Policies
open Rules
open Decisions
open Algorithms

let private buildDecisionTrace policyId policyVersion decisionId decisionOutcome state events summary =
    buildDecisionWithTrace
        evolve
        state
        events
        decisionId
        []
        ArsIdentifiers.Capabilities.modelDemandInterventions.Id
        decisionOutcome
        policyId
        policyVersion
        [ ArsIdentifiers.SemanticObjects.demandInterventionImpact.Id ]
        (Some summary)

/// AB-D-018: Assess Demand Intervention Impact (creates Draft)
let assessInterventionImpact
    (policy: InterventionModelingGovernancePolicy)
    : Decide<DemandInterventionImpact, AssessInterventionImpactCmd, DemandInterventionImpactEvent> =
    fun (cmd: AssessInterventionImpactCmd) (state: DemandInterventionImpact option) ->
        result {
            let assessment =
                modelInterventionLift
                    cmd.InterventionType
                    cmd.InterventionMagnitude
                    cmd.HistoricalPairs
                    cmd.BaselineDemand
                    policy

            let currentVersion =
                match state with
                | Some current -> current.Version + 1
                | None -> 1

            let draftImpact: DemandInterventionImpact =
                { ImpactId = cmd.ImpactId
                  InterventionReference = cmd.InterventionReference
                  Item = cmd.Item
                  Location = cmd.Location
                  AssessedDemandLift = assessment.AssessedLift
                  LiftConfidence = assessment.LiftConfidence
                  TemporalValidity = cmd.TemporalValidity
                  ModelProvenance = assessment.ApproachUsed
                  LifecycleState = Draft
                  Version = currentVersion
                  CreatedAt = cmd.Timestamp
                  PublishedAt = None }

            let events = [ InterventionImpactAssessed draftImpact ]

            let decisionOutcome: DecisionOutcome<string> =
                { Outcome = "DraftAssessed"
                  Evaluations = [] }

            return
                buildDecisionTrace
                    (Some policy.PolicyId)
                    (Some policy.Version)
                    "AB-D-018"
                    decisionOutcome
                    state
                    events
                    assessment.Rationale
        }

/// AB-D-019: Publish Demand Intervention Impact (transitions Draft to Published per DE-D-014)
let publishInterventionImpact
    (policy: InterventionModelingGovernancePolicy)
    (isInterventionActive: bool)
    (previousImpactId: DemandInterventionImpactId option)
    : Decide<DemandInterventionImpact, PublishInterventionImpactCmd, DemandInterventionImpactEvent> =
    fun (cmd: PublishInterventionImpactCmd) (state: DemandInterventionImpact option) ->
        result {
            match state with
            | None ->
                return!
                    Error(
                        DomainError.notFound("DemandInterventionImpact", DemandInterventionImpactId.value cmd.ImpactId)
                    )
            | Some current when current.LifecycleState = Published ->
                // Idempotency: already published, terminate successfully
                let decisionOutcome: DecisionOutcome<InterventionImpactPublicationDecision> =
                    { Outcome =
                        { SelectedAlternative = Publish
                          ImpactId = cmd.ImpactId
                          Rationale = "Impact is already in Published state." }
                      Evaluations = [] }

                return
                    buildDecisionTrace
                        (Some policy.PolicyId)
                        (Some policy.Version)
                        ArsIdentifiers.Decisions.approveInterventionImpactPublication.Id
                        decisionOutcome
                        state
                        []
                        "Demand Intervention Impact is already published."
            | Some current ->
                let ruleInput: InterventionImpactRuleInput =
                    { Impact = current
                      IsInterventionActive = isInterventionActive
                      Policy = policy }

                let! decision = evaluatePublicationApproval Rules.publicationRules ruleInput

                let publishedImpact: DemandInterventionImpact =
                    { current with
                        LifecycleState = Published
                        PublishedAt = Some cmd.Timestamp }

                let events = [ InterventionImpactPublished(publishedImpact, previousImpactId) ]
                let summary = decision.Outcome.Rationale

                return
                    buildDecisionTrace
                        (Some policy.PolicyId)
                        (Some policy.Version)
                        ArsIdentifiers.Decisions.approveInterventionImpactPublication.Id
                        decision
                        state
                        events
                        summary
        }
