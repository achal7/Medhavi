/// SE-D-011 — Demand Learning Aggregate Model
/// Traces to: Demand Intelligence Specification (SE-D-011, AB-D-017, Chapter 4.3.1)
module Medhavi.Demand.LearnFromDemand.DemandLearning.Model

open Medhavi.SemanticModel
open Medhavi.Demand

/// SE-D-011 Learning Type – Category of discovered enterprise pattern
type LearningType =
    | OverrideEffectiveness
    | ForecastBiasPattern
    | ModelPerformanceDegradation
    | ClassificationImpact
    | PolicyEffectiveness
    | PlannerBehavior
    | DemandBehaviourPattern
    | DataQualityIssue
    | RecurringExceptionPattern

    member this.AsString =
        match this with
        | OverrideEffectiveness -> "OverrideEffectiveness"
        | ForecastBiasPattern -> "ForecastBiasPattern"
        | ModelPerformanceDegradation -> "ModelPerformanceDegradation"
        | ClassificationImpact -> "ClassificationImpact"
        | PolicyEffectiveness -> "PolicyEffectiveness"
        | PlannerBehavior -> "PlannerBehavior"
        | DemandBehaviourPattern -> "DemandBehaviourPattern"
        | DataQualityIssue -> "DataQualityIssue"
        | RecurringExceptionPattern -> "RecurringExceptionPattern"

    static member FromString(s: string) : LearningType =
        match s with
        | "OverrideEffectiveness" -> OverrideEffectiveness
        | "ForecastBiasPattern" -> ForecastBiasPattern
        | "ModelPerformanceDegradation" -> ModelPerformanceDegradation
        | "ClassificationImpact" -> ClassificationImpact
        | "PolicyEffectiveness" -> PolicyEffectiveness
        | "PlannerBehavior" -> PlannerBehavior
        | "DemandBehaviourPattern" -> DemandBehaviourPattern
        | "DataQualityIssue" -> DataQualityIssue
        | "RecurringExceptionPattern" -> RecurringExceptionPattern
        | _ -> DataQualityIssue

/// SE-D-011 Evidence Reference – link to a specific historical artifact and period
type EvidenceReference =
    { ArtifactType: string
      ArtifactId: string
      PeriodStart: Timestamp
      PeriodEnd: Timestamp
      SummaryStatistics: Map<string, decimal> }

/// SE-D-011 Improvement Opportunity – concrete actionable improvement for subsequent planning cycles
type ImprovementOpportunity =
    { OpportunityId: string
      TargetCapability: string
      TargetPolicyId: string
      ProposedParameterChange: string
      ExpectedBenefit: string
      InterventionConfidence: decimal }

/// Multi-period Forecast Quality summary record
type ForecastQualityAssessmentSummary =
    { Scope: PlanningScopeId
      PeriodStart: Timestamp
      PeriodEnd: Timestamp
      Wape: decimal
      ForecastBias: decimal
      TrackingSignal: decimal
      ForecastAccuracy: decimal
      CompletenessScore: decimal }

/// Historical planner override summary record
type PlannerOverrideSummary =
    { PlannerId: string
      PublicationId: string
      Scope: PlanningScopeId
      OverriddenAt: Timestamp
      OriginalValue: decimal
      OverrideValue: decimal
      ActualValue: decimal option
      ReasonCode: string }

/// Historical demand exception summary record
type DemandExceptionSummary =
    { ExceptionType: string
      EntityType: string
      EntityId: string
      Scope: PlanningScopeId
      Severity: string
      TriggeringMetric: string
      MetricValue: decimal
      ThresholdValue: decimal
      DetectedAt: Timestamp }

/// Historical classification change summary record
type ClassificationChangeSummary =
    { EntityType: string
      EntityId: string
      Scope: PlanningScopeId
      Scheme: string
      PreviousClassification: string
      NewClassification: string
      ChangedAt: Timestamp }

/// Canonical bundle of multi-period historical evidence passed to BA-D-013
type HistoricalDemandEvidenceBundle =
    { Scope: PlanningScopeId
      WindowStart: Timestamp
      WindowEnd: Timestamp
      QualityAssessments: ForecastQualityAssessmentSummary list
      PlannerOverrides: PlannerOverrideSummary list
      DemandExceptions: DemandExceptionSummary list
      ClassificationChanges: ClassificationChangeSummary list }

/// SE-D-011 Demand Learning – Aggregate Root State (Immutable per BR-D-125)
type DemandLearning =
    { Id: DemandLearningId
      Scope: PlanningScopeId
      LearningType: LearningType
      LearningStatement: string
      PatternConfidence: decimal
      InterventionConfidence: decimal
      SupportingEvidence: EvidenceReference list
      ImprovementOpportunities: ImprovementOpportunity list
      PolicyVersion: string
      CreatedAt: Timestamp }

    member this.AssignmentId = DemandLearningId.value this.Id

/// AB-D-017 Command: Establish Demand Learning
type EstablishLearningCmd =
    { LearningId: DemandLearningId
      Scope: PlanningScopeId
      LearningType: LearningType
      LearningStatement: string
      PatternConfidence: decimal
      InterventionConfidence: decimal
      SupportingEvidence: EvidenceReference list
      ImprovementOpportunities: ImprovementOpportunity list
      PolicyVersion: string
      Timestamp: Timestamp }

/// EV-D-025 Demand Learning Established Event
type DemandLearningEvent =
    | DemandLearningEstablished of DemandLearning

/// Pure evolution (Layer E: Catamorphism)
let evolve: Medhavi.Foundation.Contracts.Evolve<DemandLearning, DemandLearningEvent> =
    fun (_: DemandLearning option) (event: DemandLearningEvent) ->
        match event with
        | DemandLearningEstablished learning -> Some learning

/// Replay event sequence to rehydrate aggregate state
let replay (events: DemandLearningEvent seq) : DemandLearning option =
    Seq.fold evolve None events
