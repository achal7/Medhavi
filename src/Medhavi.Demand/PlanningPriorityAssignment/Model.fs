module Medhavi.Demand.PlanningPriorityAssignment.Model

open Medhavi.SharedKernel
open Medhavi.Demand

module Identity =
    let create (entityType: string) (entityId: string) = $"{entityType}-{entityId}"

// =============================================================================
// SE‑D‑038 — Planning Priority Assignment
// =============================================================================

type PriorityLevel =
    | Critical
    | High
    | Medium
    | Low
    | Unclassified

    member this.AsString() =
        match this with
        | Critical -> "Critical"
        | High -> "High"
        | Medium -> "Medium"
        | Low -> "Low"
        | Unclassified -> "Unclassified"

type PriorityChangeEvent =
    { Timestamp: Timestamp
      PreviousPriority: string option
      NewPriority: string
      PreviousScore: PositiveDecimal option
      NewScore: PositiveDecimal
      DecisionRationale: string
      BusinessValidity: string
      Reason: string
      OverrideJustification: string option
      PolicyVersionRef: string }

type PlanningPriorityAssignment =
    { EntityType: string // "Product" | "Customer" | "ProductCustomer"
      EntityId: string // SkuId, CustomerId, or composite
      CurrentPriority: PriorityLevel
      PriorityScore: PositiveDecimal
      DecisionRationale: string
      BusinessValidity: string
      LastEvaluated: Timestamp
      LastChangeEvent: PriorityChangeEvent option
      BusinessTime: Timestamp
      TransactionTime: Timestamp }

    member this.AssignmentId = Identity.create this.EntityType this.EntityId

// ---------- Commands ----------
type UpdatePlanningPriorityCmd =
    { EntityType: string; EntityId: string }

    member this.AssignmentId = Identity.create this.EntityType this.EntityId

type OverridePlanningPriorityCmd =
    { EntityType: string
      EntityId: string
      NewPriority: PriorityLevel
      Justification: string }

    member this.AssignmentId = Identity.create this.EntityType this.EntityId

type PlanningPriorityCommand =
    | UpdatePlanningPriority of UpdatePlanningPriorityCmd
    | OverridePlanningPriority of OverridePlanningPriorityCmd

    member this.AssignmentId =
        match this with
        | UpdatePlanningPriority c -> c.AssignmentId
        | OverridePlanningPriority c -> c.AssignmentId

// ---------- Events ----------
type PlanningPriorityEvent = PlanningPriorityUpdated of PlanningPriorityAssignment * PriorityChangeEvent

// ---------- Evolve ----------
let evolve
    (evt: PlanningPriorityEvent)
    (stateOpt: PlanningPriorityAssignment option)
    : PlanningPriorityAssignment option =
    match evt with
    | PlanningPriorityUpdated(ass, _) -> Some ass
