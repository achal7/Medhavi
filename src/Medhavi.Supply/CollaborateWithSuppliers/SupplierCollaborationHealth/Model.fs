/// SE-S-052 — Supplier Collaboration Health Assessment Domain Model
/// Traces to: Supply Intelligence Specification §4.3.1 (SE-S-052, AB-S-042, EV-S-042)
module Medhavi.Supply.CollaborateWithSuppliers.SupplierCollaborationHealth.Model

open Medhavi.SemanticModel
open Medhavi.Supply

type HealthVersionLifecycleState =
    | Draft
    | Published
    | Superseded

type CollaborationHealthPublicationOutcome =
    | Publish
    | PublishWithFlags of UnassessedCount: int
    | Hold of Reason: string

type SupplierCollaborationHealthClassification =
    | Excellent
    | Good
    | NeedsImprovement
    | AtRisk
    | Unassessed

type SupplierCommitmentAssessmentFact =
    { SupplierId: SupplierId
      OnTimeDeliveryRate: decimal
      AverageAcknowledgementHours: decimal
      ActiveRiskSignalsCount: int
      AuthoritativeHealthState: SupplierCollaborationHealthState
      OverallConfidence: string }

type SupplierCollaborationHealthVersion =
    {
        VersionNumber: int
        State: HealthVersionLifecycleState
        AssessmentScopeId: PlanningScopeId
        EvaluationPeriodId: EvaluationPeriodId
        OverallPublicationOutcome: CollaborationHealthPublicationOutcome
        SupplierClassifications: (SupplierId * SupplierCollaborationHealthClassification) list
        ResponsivenessTrends: (SupplierId * string) list
        CommitmentAccuracyTrends: (SupplierId * string) list
        RecommendationSignals: string list
        CompletenessRatio: decimal
        AssessmentConfidence: string
        EvidenceReferences: string list
        TransactionTimestamp: Timestamp
        PublicationTimestamp: Timestamp option
    }

type SupplierCollaborationHealthAssessment =
    {
        AssessmentScopeId: PlanningScopeId
        EvaluationPeriodId: EvaluationPeriodId
        Versions: SupplierCollaborationHealthVersion list
        CurrentPublishedVersion: int option
    }

type PublishSupplierCollaborationHealthCmd =
    {
        AssessmentScopeId: PlanningScopeId
        EvaluationPeriodId: EvaluationPeriodId
        PublicationTimestamp: Timestamp
    }

type SupplierCollaborationHealthEvent =
    | SupplierCollaborationHealthAssessmentPublished of
        assessment: SupplierCollaborationHealthAssessment *
        publishedVersion: int *
        supersededVersionOpt: int option

let evolve
    (stateOpt: SupplierCollaborationHealthAssessment option)
    (event: SupplierCollaborationHealthEvent)
    : SupplierCollaborationHealthAssessment option =
    match event with
    | SupplierCollaborationHealthAssessmentPublished(assessment, _, _) -> Some assessment

let replay (events: SupplierCollaborationHealthEvent seq) : SupplierCollaborationHealthAssessment option =
    Seq.fold evolve None events
