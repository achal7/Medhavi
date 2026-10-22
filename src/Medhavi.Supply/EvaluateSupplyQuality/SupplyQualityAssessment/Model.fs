/// SE-S-100 — Supply Quality Assessment Aggregate Model
/// Traces to: Supply Intelligence Specification (§4.3.1 SE-S-100, AB-S-090)
module Medhavi.Supply.EvaluateSupplyQuality.SupplyQualityAssessment.Model

open System
open Medhavi.Common
open Medhavi.SemanticModel
open Medhavi.Supply

[<RequireQualifiedAccess>]
type QualityClassification =
    | Excellent
    | Good
    | Adequate
    | Poor
    | Critical
    | Unassessed

module QualityClassification =
    let rank = function
        | QualityClassification.Critical -> 1
        | QualityClassification.Poor -> 2
        | QualityClassification.Adequate -> 3
        | QualityClassification.Good -> 4
        | QualityClassification.Excellent -> 5
        | QualityClassification.Unassessed -> 0

    let min (a: QualityClassification) (b: QualityClassification) =
        match a, b with
        | QualityClassification.Unassessed, x | x, QualityClassification.Unassessed -> x
        | _ -> if rank a <= rank b then a else b

    let max (a: QualityClassification) (b: QualityClassification) =
        match a, b with
        | QualityClassification.Unassessed, x | x, QualityClassification.Unassessed -> x
        | _ -> if rank a >= rank b then a else b

    let isAtLeast (target: QualityClassification) (actual: QualityClassification) =
        rank actual >= rank target

[<RequireQualifiedAccess>]
type TrendTrajectory =
    | Improving
    | Stable
    | Deteriorating
    | Recovering
    | Volatile
    | Unassessed

[<RequireQualifiedAccess>]
type QualityConfidenceLevel =
    | High
    | Medium
    | Low

module QualityConfidenceLevel =
    let rank = function
        | QualityConfidenceLevel.Low -> 1
        | QualityConfidenceLevel.Medium -> 2
        | QualityConfidenceLevel.High -> 3

    let min (a: QualityConfidenceLevel) (b: QualityConfidenceLevel) =
        if rank a <= rank b then a else b

[<RequireQualifiedAccess>]
type QualityDimensionType =
    | Service
    | Inventory
    | Capacity
    | SupplierCollaboration
    | Procurement
    | Distribution
    | PlanningStability
    | Sustainability

module QualityDimensionType =
    let isMandatory = function
        | QualityDimensionType.Service
        | QualityDimensionType.Inventory
        | QualityDimensionType.Capacity
        | QualityDimensionType.SupplierCollaboration
        | QualityDimensionType.Procurement
        | QualityDimensionType.Distribution -> true
        | QualityDimensionType.PlanningStability
        | QualityDimensionType.Sustainability -> false

    let name = function
        | QualityDimensionType.Service -> "Service"
        | QualityDimensionType.Inventory -> "Inventory"
        | QualityDimensionType.Capacity -> "Capacity"
        | QualityDimensionType.SupplierCollaboration -> "Supplier Collaboration"
        | QualityDimensionType.Procurement -> "Procurement"
        | QualityDimensionType.Distribution -> "Distribution"
        | QualityDimensionType.PlanningStability -> "Planning Stability"
        | QualityDimensionType.Sustainability -> "Sustainability"

[<RequireQualifiedAccess>]
type SupplyQualityLifecycleState =
    | Draft
    | Published
    | Superseded

[<RequireQualifiedAccess>]
type PublicationDecisionOutcome =
    | Publish
    | PublishWithFlags
    | Hold

type MetricEvaluation =
    { MetricName: string
      ActualValue: decimal
      ObjectiveTarget: decimal
      StretchTarget: decimal option
      MinimumAcceptable: decimal option
      CriticalThreshold: decimal option
      DeviationPercent: decimal
      Classification: QualityClassification
      IsEffectiveness: bool }

type DimensionalEvaluation =
    { Dimension: QualityDimensionType
      DimensionName: string
      IsMandatory: bool
      EvaluatedMetrics: MetricEvaluation list
      EffectivenessClassification: QualityClassification
      EfficiencyClassification: QualityClassification
      RecommendedClassification: QualityClassification
      AuthoritativeClassification: QualityClassification
      Trend: TrendTrajectory
      Confidence: QualityConfidenceLevel
      DataCompleteness: decimal
      Score: decimal
      IssueCount: int
      EvidenceReferences: string list
      ReasonCodes: string list
      AffectedObjectives: string list }

type AssessmentContextComparator =
    { HistoricalComparator: string option
      CrossScopeComparator: string option
      TargetVarianceSummary: string }

type OverallQualityState =
    { OverallClassification: QualityClassification
      OverallScore: decimal
      OverallConfidence: QualityConfidenceLevel
      SynthesisRationale: string
      PrimaryDrivers: QualityDimensionType list
      TradeOffs: string option
      TriggeredRules: string list
      MandatoryDimensionStatus: Map<QualityDimensionType, QualityClassification> }

/// A single version entity within the Supply Quality Assessment aggregate root
type SupplyQualityAssessmentVersion =
    { VersionNumber: int
      DimensionalEvaluations: DimensionalEvaluation list
      OverallQuality: OverallQualityState
      Comparators: AssessmentContextComparator
      PublicationFlags: string list
      PolicyVersion: string
      CreatedTimestamp: Timestamp
      PublicationTimestamp: Timestamp option
      LifecycleState: SupplyQualityLifecycleState }

/// SE-S-100 Supply Quality Assessment Aggregate Root
type SupplyQualityAssessment =
    { AssessmentId: SupplyQualityAssessmentId
      PlanningScopeId: PlanningScopeId
      PlanningHorizon: PlanningHorizon
      EvaluationPeriodId: EvaluationPeriodId
      Versions: SupplyQualityAssessmentVersion list
      CurrentPublishedVersion: int option }

/// AB-S-090 — Command to evaluate and publish supply quality
type EvaluateSupplyQualityCmd =
    { PlanningScopeId: PlanningScopeId
      PlanningHorizon: PlanningHorizon
      EvaluationPeriodId: EvaluationPeriodId
      ForceMajeureDeclared: bool
      StrategicInvestmentNotes: string option
      Prerequisites: ScopePrerequisitesFact
      DimensionalOutcomes: DimensionalOutcomeFact list
      EvaluationContext: QualityEvaluationContextFact option
      Timestamp: Timestamp }

/// EV-S-090 — Enterprise Events emitted by Supply Quality Assessment
type SupplyQualityAssessmentEvent =
    | SupplyQualityAssessmentDraftCreated of SupplyQualityAssessment * version: int
    | SupplyQualityAssessmentPublished of SupplyQualityAssessment * version: int * previousPublished: int option * publicationTime: Timestamp

/// Pure evolution (catamorphism). No validation.
let evolve (_: SupplyQualityAssessment option) (event: SupplyQualityAssessmentEvent) : SupplyQualityAssessment option =
    match event with
    | SupplyQualityAssessmentDraftCreated(sqa, _) -> Some sqa
    | SupplyQualityAssessmentPublished(sqa, _, _, _) -> Some sqa

let replay (events: SupplyQualityAssessmentEvent seq) : SupplyQualityAssessment option =
    Seq.fold evolve None events
