module Medhavi.Demand.DemandBehaviourAssignment.Model

open Medhavi.SharedKernel

module Identity =
    let create (entityType: string) (entityId: string) (behaviourDimension: string) =
        $"{entityType}-{entityId}-{behaviourDimension}"

// =============================================================================
// SE‑D‑037 — Demand Behaviour Assignment
// =============================================================================

type BehaviourChangeEvent =
    { Timestamp: Timestamp
      PreviousClassification: string option
      NewClassification: string
      Reason: string
      OverrideJustification: string option
      ClassificationConfidence: PositiveDecimal
      EvidenceSummary: string
      PolicyVersionRef: string }

type DemandBehaviourAssignment =
    { EntityType: string // "Product"
      EntityId: string // SkuId value
      BehaviourDimension: string // e.g., "StatisticalPattern", "LifecycleBehaviour"
      CurrentClassification: string
      ClassificationConfidence: PositiveDecimal
      EvidenceSummary: string
      LastClassified: Timestamp
      LastChangeEvent: BehaviourChangeEvent option
      BusinessTime: Timestamp
      TransactionTime: Timestamp }

    member this.AssignmentId = Identity.create this.EntityType this.EntityId this.BehaviourDimension

// ---------- Commands ----------
type UpdateBehaviourClassificationCmd =
    { EntityType: string
      EntityId: string
      BehaviourDimension: string }

    member this.AssignmentId = Identity.create this.EntityType this.EntityId this.BehaviourDimension

type OverrideBehaviourClassificationCmd =
    { EntityType: string
      EntityId: string
      BehaviourDimension: string
      NewClassification: string
      Justification: string }

    member this.AssignmentId = Identity.create this.EntityType this.EntityId this.BehaviourDimension

type DemandBehaviourAssignmentCommand =
    | UpdateBehaviourClassification of UpdateBehaviourClassificationCmd
    | OverrideBehaviourClassification of OverrideBehaviourClassificationCmd

    member this.AssignmentId =
        match this with
        | UpdateBehaviourClassification c -> c.AssignmentId
        | OverrideBehaviourClassification c -> c.AssignmentId

// ---------- Events ----------
type DemandBehaviourAssignmentEvent =
    | DemandBehaviourClassificationUpdated of DemandBehaviourAssignment * BehaviourChangeEvent

// ---------- Evolve ----------
let evolve
    (evt: DemandBehaviourAssignmentEvent)
    (stateOpt: DemandBehaviourAssignment option)
    : DemandBehaviourAssignment option =
    match evt with
    | DemandBehaviourClassificationUpdated(ass, _) -> Some ass
