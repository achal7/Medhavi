/// SE-D-005 — Planning Classification Assignment Aggregate Model
/// Traces to: Demand Intelligence Specification (SE-D-005, AB-D-011, FS-D-011, Chapter 4.3.1)
module Medhavi.Demand.SegmentDemand.PlanningClassificationAssignment.Model

open Medhavi.SemanticModel
open Medhavi.Demand
open Medhavi.Foundation.Failure

/// Entity types eligible for planning classification
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

/// Classification dimension schemes
type ClassificationType =
    | ABC
    | XYZ

    member this.AsString =
        match this with
        | ABC -> "ABC"
        | XYZ -> "XYZ"

    static member FromString(s: string) : Result<ClassificationType, DomainError> =
        match s.Trim().ToUpperInvariant() with
        | "ABC" -> Ok ABC
        | "XYZ" -> Ok XYZ
        | other -> DomainError.validation $"Unsupported ClassificationType: '{other}'. Expected 'ABC' or 'XYZ'" |> Error

/// Governed Planning Class labels per PO-D-035
type PlanningClassification =
    | ClassA
    | ClassB
    | ClassC
    | ClassX
    | ClassY
    | ClassZ
    | Unclassified

    member this.AsString =
        match this with
        | ClassA -> "A"
        | ClassB -> "B"
        | ClassC -> "C"
        | ClassX -> "X"
        | ClassY -> "Y"
        | ClassZ -> "Z"
        | Unclassified -> "Unclassified"

    static member FromString(s: string) : Result<PlanningClassification, DomainError> =
        match s.Trim().ToUpperInvariant() with
        | "A"
        | "CLASSA" -> Ok ClassA
        | "B"
        | "CLASSB" -> Ok ClassB
        | "C"
        | "CLASSC" -> Ok ClassC
        | "X"
        | "CLASSX" -> Ok ClassX
        | "Y"
        | "CLASSY" -> Ok ClassY
        | "Z"
        | "CLASSZ" -> Ok ClassZ
        | "UNCLASSIFIED" -> Ok Unclassified
        | other -> DomainError.validation $"Unsupported PlanningClassification: '{other}'" |> Error

// ---------- Immutable Audit Record ----------

/// Preserved audit event representing a classification change (SE-D-005)
type AssignmentChangeEvent =
    { Timestamp: Timestamp
      FromClassification: PlanningClassification option
      ToClassification: PlanningClassification
      ClassificationScore: decimal
      ClassificationConfidence: string
      Rationale: string
      PolicyVersion: string
      DecisionTraceId: string }

// ---------- Aggregate State ----------

/// SE-D-005 – Planning Classification Assignment Aggregate Root
/// Identity: EntityType + EntityId + ClassificationType
type PlanningClassificationAssignment =
    { AssignmentId: PlanningClassificationAssignmentId
      EntityType: EntityType
      EntityId: string
      ClassificationType: ClassificationType
      CurrentClassification: PlanningClassification
      AnalogItemReference: ItemId option
      ClassificationScore: decimal
      ClassificationConfidence: string
      AssignmentRationale: string
      PolicyVersion: string
      ChangeEvents: AssignmentChangeEvent list
      LastUpdated: Timestamp }

// ---------- Commands ----------

/// AB-D-011 Command: Classify Planning Entity
type ClassifyPlanningEntityCmd =
    { AssignmentId: PlanningClassificationAssignmentId
      EntityType: EntityType
      EntityId: string
      ClassificationType: ClassificationType
      VolumeOrRevenuePercentage: decimal option
      HistoricalDemandValues: decimal list option
      AnalogItemId: ItemId option
      ClassificationTime: Timestamp }

/// AB-D-011 Command: Manual Planner Override (PO-D-036)
type OverridePlanningClassificationCmd =
    { AssignmentId: PlanningClassificationAssignmentId
      EntityType: EntityType
      EntityId: string
      ClassificationType: ClassificationType
      NewClassification: PlanningClassification
      Justification: string
      PlannerId: string
      OverrideTime: Timestamp }

// ---------- Enterprise Events ----------

/// Enterprise Events emitted by Planning Classification Assignment aggregate
type PlanningClassificationEvent =
    | PlanningClassificationAssigned of
        Assignment: PlanningClassificationAssignment *
        PreviousClassification: PlanningClassification option *
        Event: AssignmentChangeEvent
    | PlanningClassificationOverridden of
        Assignment: PlanningClassificationAssignment *
        PreviousClassification: PlanningClassification option *
        Event: AssignmentChangeEvent

// ---------- Pure State Evolution (Layer E: Catamorphism) ----------

let evolve: Medhavi.Foundation.Contracts.Evolve<PlanningClassificationAssignment, PlanningClassificationEvent> =
    fun (_: PlanningClassificationAssignment option) (event: PlanningClassificationEvent) ->
        match event with
        | PlanningClassificationAssigned(assignment, _, _) -> Some assignment
        | PlanningClassificationOverridden(assignment, _, _) -> Some assignment

/// Replay event sequence to rehydrate aggregate state
let replay (events: PlanningClassificationEvent seq) : PlanningClassificationAssignment option =
    Seq.fold evolve None events
