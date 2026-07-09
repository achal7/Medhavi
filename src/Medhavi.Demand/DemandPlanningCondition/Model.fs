module Medhavi.Demand.DemandPlanningCondition.Model

open Medhavi.SharedKernel
open Medhavi.Demand

// =============================================================================
// SE‑D‑040 — Demand Planning Condition
// =============================================================================

type ConditionStatus =
    | Active
    | Resolved

type ConditionSeverity =
    | Critical
    | High
    | Medium
    | Low

type ConditionChangeEvent =
    { EventId: string
      Timestamp: Timestamp
      EventType: string // Detected | SeverityChanged | Resolved
      CurrentStateAfterEvent: ConditionStatus
      PreviousSeverity: ConditionSeverity option
      NewSeverity: ConditionSeverity
      Evidence: string
      PolicyVersionRef: string }

type DemandPlanningCondition =
    { Id: DemandPlanningConditionId
      PlanningEntity: string // Product, Product‑Location, Segment, Enterprise‑wide
      ConditionType: string // ForecastBiasElevated, DataCompletenessGap, etc.
      CurrentStatus: ConditionStatus
      Severity: ConditionSeverity
      DetectionEvidence: string
      DetectionTimestamp: Timestamp
      ResolutionTimestamp: Timestamp option
      ResolutionEvidence: string option
      LastChangeEvent: ConditionChangeEvent option
      BusinessTime: Timestamp
      TransactionTime: Timestamp }

    member this.AssignmentId = DemandPlanningConditionId.value this.Id

// ---------- Commands ----------
type RecognizeConditionCmd =
    { ConditionId: DemandPlanningConditionId
      PlanningEntity: string
      ConditionType: string
      NewSeverity: ConditionSeverity
      DetectionEvidence: string
      DetectionTimestamp: Timestamp
      PolicyVersionRef: string
      BusinessTime: Timestamp }

type ResolveConditionCmd =
    { ConditionId: DemandPlanningConditionId
      ResolutionEvidence: string
      ResolutionTimestamp: Timestamp }

type DemandPlanningConditionCommand =
    | Recognize of RecognizeConditionCmd
    | Resolve of ResolveConditionCmd

    member this.ConditionId =
        match this with
        | Recognize c -> DemandPlanningConditionId.value c.ConditionId
        | Resolve c -> DemandPlanningConditionId.value c.ConditionId

// ---------- Events ----------
type DemandPlanningConditionEvent =
    | ConditionRecognized of DemandPlanningCondition * ConditionChangeEvent
    | ConditionResolved of DemandPlanningCondition * ConditionChangeEvent

// ---------- Evolve ----------
let evolve (evt: DemandPlanningConditionEvent) (_: DemandPlanningCondition option) : DemandPlanningCondition option =
    match evt with
    | ConditionRecognized(condition, _) -> Some condition
    | ConditionResolved(condition, _) -> Some condition
