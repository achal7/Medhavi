/// SE-D-006 — Demand Behavior Assignment Aggregate Model
/// Traces to: Demand Intelligence Specification (SE-D-006, AB-D-012, FS-D-012, Chapter 4.3.1)
module Medhavi.Demand.ClassifyDemand.DemandBehaviorAssignment.Model

open System
open Medhavi.SemanticModel
open Medhavi.Demand
open Medhavi.Foundation.Failure

// ---------- Governed Enums ----------

/// Governed behavioral dimensions (SE-D-006)
type BehaviorDimension =
    | StatisticalPattern

    member this.AsString =
        match this with
        | StatisticalPattern -> "StatisticalPattern"

    static member FromString(s: string) : Result<BehaviorDimension, DomainError> =
        match s.Trim().ToLowerInvariant() with
        | "statisticalpattern" | "statistical_pattern" | "pattern" -> Ok StatisticalPattern
        | other -> DomainError.validation $"Unsupported BehaviorDimension: '{other}'. Expected 'StatisticalPattern'" |> Error

/// Governed Behavior Classes per PO-D-037
type BehaviorClass =
    | Continuous
    | Intermittent
    | Seasonal
    | Lumpy
    | Trend
    | Unclassified

    member this.AsString =
        match this with
        | Continuous -> "Continuous"
        | Intermittent -> "Intermittent"
        | Seasonal -> "Seasonal"
        | Lumpy -> "Lumpy"
        | Trend -> "Trend"
        | Unclassified -> "Unclassified"

    static member FromString(s: string) : Result<BehaviorClass, DomainError> =
        match s.Trim().ToLowerInvariant() with
        | "continuous" -> Ok Continuous
        | "intermittent" -> Ok Intermittent
        | "seasonal" -> Ok Seasonal
        | "lumpy" -> Ok Lumpy
        | "trend" -> Ok Trend
        | "unclassified" -> Ok Unclassified
        | other -> DomainError.validation $"Unsupported BehaviorClass: '{other}'" |> Error

// ---------- Statistical Features Model ----------

/// Statistical features computed for behavioral pattern recognition (SE-D-006)
type StatisticalFeatures =
    { CoefficientOfVariation: decimal
      SquaredCoefficientOfVariation: decimal
      AverageDemandInterval: decimal
      AutocorrelationAtSeasonalLag: decimal option
      TrendPValue: decimal option
      ZeroDemandRatio: decimal
      SamplePeriodCount: int }

// ---------- Immutable Audit Record ----------

/// Preserved audit event representing a behavior classification change (SE-D-006)
type BehaviorChangeEvent =
    { Timestamp: Timestamp
      FromClassification: BehaviorClass option
      ToClassification: BehaviorClass
      StatisticalFeatures: StatisticalFeatures option
      Confidence: string
      Rationale: string
      PolicyVersion: string
      DecisionTraceId: string }

// ---------- Aggregate State ----------

/// SE-D-006 – Demand Behavior Assignment Aggregate Root
/// Identity: Item (SE-C-001) + Location (SE-C-002) + Dimension
type DemandBehaviorAssignment =
    { AssignmentId: DemandBehaviorAssignmentId
      Item: ItemId
      Location: LocationId
      Dimension: BehaviorDimension
      CurrentClassification: BehaviorClass
      StatisticalFeatures: StatisticalFeatures option
      ClassificationConfidence: string
      AssignmentRationale: string
      PolicyVersion: string
      ChangeEvents: BehaviorChangeEvent list
      LastUpdated: Timestamp }

// ---------- Commands ----------

/// AB-D-012 Command: Classify Demand Behavior
type ClassifyDemandBehaviorCmd =
    { AssignmentId: DemandBehaviorAssignmentId
      Item: ItemId
      Location: LocationId
      Dimension: BehaviorDimension
      DemandQuantities: decimal list
      ClassificationTime: Timestamp }

/// AB-D-012 Command: Manual Planner Override (PO-D-038)
type OverrideDemandBehaviorCmd =
    { AssignmentId: DemandBehaviorAssignmentId
      Item: ItemId
      Location: LocationId
      Dimension: BehaviorDimension
      NewClassification: BehaviorClass
      Justification: string
      PlannerId: string
      OverrideTime: Timestamp }

// ---------- Enterprise Events ----------

/// Enterprise Events emitted by Demand Behavior Assignment aggregate
type DemandBehaviorEvent =
    | DemandBehaviorClassified of
        Assignment: DemandBehaviorAssignment *
        PreviousClassification: BehaviorClass option *
        Event: BehaviorChangeEvent
    | DemandBehaviorOverridden of
        Assignment: DemandBehaviorAssignment *
        PreviousClassification: BehaviorClass option *
        Event: BehaviorChangeEvent

// ---------- Pure State Evolution (Layer E: Catamorphism) ----------

let evolve: Medhavi.Foundation.Contracts.Evolve<DemandBehaviorAssignment, DemandBehaviorEvent> =
    fun (_: DemandBehaviorAssignment option) (event: DemandBehaviorEvent) ->
        match event with
        | DemandBehaviorClassified(assignment, _, _) -> Some assignment
        | DemandBehaviorOverridden(assignment, _, _) -> Some assignment

/// Replay event sequence to rehydrate aggregate state
let replay (events: DemandBehaviorEvent seq) : DemandBehaviorAssignment option =
    Seq.fold evolve None events
