/// SE-S-041 — Capacity Health Assessment Domain Model
/// Traces to: Supply Intelligence Specification §4.3.1 (SE-S-041, AB-S-031, EV-S-031)
module Medhavi.Supply.ManageCapacity.CapacityHealth.Model

open Medhavi.SemanticModel
open Medhavi.Supply

type HealthVersionLifecycleState =
    | Draft
    | Published
    | Superseded

type CapacityHealthState =
    | CriticallyConstrained
    | Constrained
    | AtRisk
    | Underutilized
    | Balanced
    | Unassessed

type AssessmentConfidence =
    | High
    | Medium
    | Low

/// Read-only position input facts for capacity health evaluation
type CapacityPositionFact =
    { ResourceId: PhysicalResourceId
      EffectiveUtilization: decimal
      UtilizationStatus: CapacityUtilizationStatus
      ConstraintStatus: CapacityConstraintStatus
      BottleneckStatus: BottleneckAssessmentStatus
      HasSinglePointOfFailureRisk: bool
      ChangeEventsCount: int
      AvailableCapacityHours: decimal
      CurrentCommittedHours: decimal
      ProjectedPlannedHours: decimal }

/// Version child entity of SE-S-041 Published Knowledge pattern (ARS Appendix A.3)
type CapacityHealthAssessmentVersion =
    {
        VersionNumber: int
        State: HealthVersionLifecycleState
        AssessmentScopeId: PlanningScopeId
        EvaluationPeriodId: EvaluationPeriodId
        OverallCapacityHealthState: CapacityHealthState
        ClassificationRationale: string
        AssessmentConfidence: AssessmentConfidence
        AverageUtilizationPercentage: decimal
        PeakUtilizationPercentage: decimal
        UnderutilizedResourceCount: int
        OverloadedResourceCount: int
        CapacityStability: string
        CapacityVolatility: decimal
        BindingConstraints: PhysicalResourceId list
        BottleneckResources: PhysicalResourceId list
        OverloadRiskExposureHours: decimal
        UnderloadCostExposureHours: decimal
        BottleneckImpactSummary: string
        ChronicOverloadResources: PhysicalResourceId list
        ChronicUnderutilizedResources: PhysicalResourceId list
        RecurringBottlenecks: PhysicalResourceId list
        LoadBalancingOpportunities: string list
        FlexibilityImprovementOpportunities: string list
        EvidenceReferences: string list
        TransactionTimestamp: Timestamp
        PublicationTimestamp: Timestamp option
    }

/// SE-S-041 Aggregate Root (Published Knowledge pattern per AssessmentScope and EvaluationPeriod)
type CapacityHealthAssessment =
    {
        AssessmentScopeId: PlanningScopeId
        EvaluationPeriodId: EvaluationPeriodId
        Versions: CapacityHealthAssessmentVersion list
    }

type PublishCapacityHealthCmd =
    {
        AssessmentScopeId: PlanningScopeId
        EvaluationPeriodId: EvaluationPeriodId
        Timestamp: Timestamp
    }

/// EV-S-031: Capacity Health Assessment Published
type CapacityHealthEvent =
    | CapacityHealthAssessmentPublished of
        assessment: CapacityHealthAssessment *
        versionNumber: int *
        supersededVersionOpt: int option

let evolve (stateOpt: CapacityHealthAssessment option) (event: CapacityHealthEvent) : CapacityHealthAssessment option =
    match event with
    | CapacityHealthAssessmentPublished(assessment, _, _) -> Some assessment

let replay (events: CapacityHealthEvent seq) : CapacityHealthAssessment option = Seq.fold evolve None events
