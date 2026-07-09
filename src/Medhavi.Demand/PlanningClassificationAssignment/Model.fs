module Medhavi.Demand.PlanningClassificationAssignment.Model

open Medhavi.SharedKernel
open Medhavi.Demand

// =============================================================================
// SE‑D‑036 — Planning Classification Assignment
// =============================================================================

type ClassificationType =
    | ABC
    | XYZ
    | Strategic

    member this.AsString() =
        match this with
        | ABC -> "ABC"
        | XYZ -> "XYZ"
        | Strategic -> "Strategic"

type AssignmentChangeEvent =
    { Timestamp: Timestamp
      PreviousClassification: string option
      NewClassification: string
      Reason: string
      OverrideJustification: string option
      ClassificationConfidence: PositiveDecimal
      PolicyVersionRef: string }

module Identity =
    let create (entityType: string) (entityId: string) (classificationType: ClassificationType) =
        $"{entityType}-{entityId}-{classificationType.AsString()}"

type PlanningClassificationAssignment =
    { EntityType: string // "Product" | "Customer"
      EntityId: string // SkuId or CustomerId
      ClassificationType: ClassificationType
      CurrentClassification: string
      ClassificationConfidence: PositiveDecimal
      LastClassified: Timestamp
      LastChangeEvent: AssignmentChangeEvent option
      BusinessTime: Timestamp
      TransactionTime: Timestamp }

    member this.AssignmentId = Identity.create this.EntityType this.EntityId this.ClassificationType

// ---------- Commands ----------
type UpdatePlanningClassificationCmd =
    { EntityType: string
      EntityId: string
      ClassificationType: ClassificationType }

    member this.AssignmentId = Identity.create this.EntityType this.EntityId this.ClassificationType

type OverridePlanningClassificationCmd =
    { EntityType: string
      EntityId: string
      ClassificationType: ClassificationType
      NewClassification: string
      Justification: string }

    member this.AssignmentId = Identity.create this.EntityType this.EntityId this.ClassificationType

type PlanningClassificationCommand =
    | UpdatePlanningClassification of UpdatePlanningClassificationCmd
    | OverridePlanningClassification of OverridePlanningClassificationCmd

    member this.AssignmentId =
        match this with
        | UpdatePlanningClassification c -> c.AssignmentId
        | OverridePlanningClassification c -> c.AssignmentId

// ---------- Events ----------
type PlanningClassificationEvent =
    | PlanningClassificationUpdated of PlanningClassificationAssignment * AssignmentChangeEvent

// ---------- Evolve ----------
let evolve
    (evt: PlanningClassificationEvent)
    (stateOpt: PlanningClassificationAssignment option)
    : PlanningClassificationAssignment option =
    match evt with
    | PlanningClassificationUpdated(ass, _) -> Some ass
