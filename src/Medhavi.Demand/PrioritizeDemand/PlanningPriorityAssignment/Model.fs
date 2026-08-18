/// SE-D-007 — Planning Priority Assignment Aggregate Model
/// Traces to: Demand Intelligence Specification (SE-D-007, AB-D-013, FS-D-013, Chapter 4.3.1)
module Medhavi.Demand.PrioritizeDemand.PlanningPriorityAssignment.Model

open System
open Medhavi.SemanticModel
open Medhavi.Demand
open Medhavi.Foundation.Failure

// ---------- Governed Enums ----------

/// Governed Planning Entity Types supported for Prioritization per PO-D-039 (SE-D-007)
type EntityType =
    | Item
    | Customer

    member this.AsString =
        match this with
        | Item -> "Item"
        | Customer -> "Customer"

    static member FromString(s: string) : Result<EntityType, DomainError> =
        match s.Trim().ToLowerInvariant() with
        | "item" -> Ok Item
        | "customer" -> Ok Customer
        | other -> DomainError.validation $"Unsupported EntityType: '{other}'. Expected 'Item' or 'Customer'" |> Error

/// Governed Priority Levels per PO-D-039 (SE-D-007)
type PriorityLevel =
    | Critical
    | High
    | Medium
    | Low
    | Unclassified

    member this.AsString =
        match this with
        | Critical -> "Critical"
        | High -> "High"
        | Medium -> "Medium"
        | Low -> "Low"
        | Unclassified -> "Unclassified"

    static member FromString(s: string) : Result<PriorityLevel, DomainError> =
        match s.Trim().ToLowerInvariant() with
        | "critical" -> Ok Critical
        | "high" -> Ok High
        | "medium" -> Ok Medium
        | "low" -> Ok Low
        | "unclassified" -> Ok Unclassified
        | other -> DomainError.validation $"Unsupported PriorityLevel: '{other}'" |> Error

// ---------- Dimension Score Breakdown ----------

/// Dimension score breakdown for multi-criteria prioritization (SE-D-007)
type DimensionScoreBreakdown =
    { RevenueScore: decimal
      StrategyScore: decimal
      RiskScore: decimal
      ContractualScore: decimal }

// ---------- Immutable Audit Record ----------

/// Preserved audit event representing a planning priority change (SE-D-007)
type PriorityChangeEvent =
    { Timestamp: Timestamp
      FromPriority: PriorityLevel option
      ToPriority: PriorityLevel
      PriorityScore: decimal
      DimensionBreakdown: DimensionScoreBreakdown
      Rationale: string
      BusinessValidity: string
      PolicyVersion: string
      DecisionTraceId: string }

// ---------- Aggregate State ----------

/// SE-D-007 – Planning Priority Assignment Aggregate Root
/// Identity: EntityType (Item/Customer) + EntityId
type PlanningPriorityAssignment =
    { AssignmentId: PlanningPriorityAssignmentId
      EntityType: EntityType
      EntityId: string
      CurrentPriority: PriorityLevel
      PriorityScore: decimal
      DimensionBreakdown: DimensionScoreBreakdown
      DecisionRationale: string
      BusinessValidity: string
      PolicyVersion: string
      ChangeEvents: PriorityChangeEvent list
      LastUpdated: Timestamp }

// ---------- Commands ----------

/// AB-D-013 Command: Prioritize Planning Entity
type PrioritizePlanningEntityCmd =
    { AssignmentId: PlanningPriorityAssignmentId
      EntityType: EntityType
      EntityId: string
      RevenueContribution: decimal option
      StrategicImportance: decimal option
      RiskExposure: decimal option
      ContractualObligation: decimal option
      PrioritizationTime: Timestamp }

/// AB-D-013 Command: Manual Planner Override (PO-D-040)
type OverridePlanningPriorityCmd =
    { AssignmentId: PlanningPriorityAssignmentId
      EntityType: EntityType
      EntityId: string
      NewPriority: PriorityLevel
      Justification: string
      PlannerId: string
      OverrideTime: Timestamp }

// ---------- Enterprise Events ----------

/// Enterprise Events emitted by Planning Priority Assignment aggregate
type PlanningPriorityEvent =
    | PlanningPriorityAssigned of
        Assignment: PlanningPriorityAssignment *
        PreviousPriority: PriorityLevel option *
        Event: PriorityChangeEvent
    | PlanningPriorityOverridden of
        Assignment: PlanningPriorityAssignment *
        PreviousPriority: PriorityLevel option *
        Event: PriorityChangeEvent

// ---------- Pure State Evolution (Layer E: Catamorphism) ----------

let evolve: Medhavi.Foundation.Contracts.Evolve<PlanningPriorityAssignment, PlanningPriorityEvent> =
    fun (_: PlanningPriorityAssignment option) (event: PlanningPriorityEvent) ->
        match event with
        | PlanningPriorityAssigned(assignment, _, _) -> Some assignment
        | PlanningPriorityOverridden(assignment, _, _) -> Some assignment

/// Replay event sequence to rehydrate aggregate state
let replay (events: PlanningPriorityEvent seq) : PlanningPriorityAssignment option =
    Seq.fold evolve None events
