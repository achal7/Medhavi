module Medhavi.Demand.EnterpriseDemandPicture.Model

open Medhavi.Contracts
open Medhavi.SharedKernel
open Medhavi.Demand

// =============================================================================
// SE‑D‑003 — Enterprise Demand Picture
// =============================================================================
type EdpStatus =
    | Draft
    | AwaitingPlanningDemandCalculation
    | ReadyForPublication
    | Published
    | Superseded

type EnterpriseDemandPicture =
    { PlanningScopeId: PlanningScopeId
      Version: int
      Status: EdpStatus
      OperationalDemand: Map<PlanningPeriod, Quantity>
      PlanningDemand: Map<PlanningPeriod, PlanningDemandLine>
      TransactionTime: Timestamp
      PublicationTime: Timestamp option
      SupersededVersionId: int option }

    member this.AssignmentId = PlanningScopeId.value this.PlanningScopeId

and PlanningDemandLine =
    { OperationalDemand: Quantity
      Adjustment: Quantity
      Override: Quantity
      FinalQuantity: Quantity }

// ---------- Commands ----------
type ReviseEdpCmd =
    { PlanningScopeId: PlanningScopeId
      Period: PlanningPeriod
      Quantity: Quantity
      ObservationId: DemandObservationId }

type CalculateEdpCmd =
    { PlanningScopeId: PlanningScopeId
      Adjustments: Map<PlanningPeriod, Quantity>
      Overrides: Map<PlanningPeriod, Quantity> }

type PublishEdpCmd = { PlanningScopeId: PlanningScopeId }

type EdpCommand =
    | Revise of ReviseEdpCmd
    | Calculate of CalculateEdpCmd
    | Publish of PublishEdpCmd

    member this.AssignmentId =
        match this with
        | Revise c -> PlanningScopeId.value c.PlanningScopeId
        | Calculate c -> PlanningScopeId.value c.PlanningScopeId
        | Publish c -> PlanningScopeId.value c.PlanningScopeId

// ---------- Events ----------
type EdpEvent =
    | EdpRevised of EnterpriseDemandPicture
    | EdpCalculated of EnterpriseDemandPicture
    | EdpPublished of EnterpriseDemandPicture * previousVersion: int option

// ---------- Evolve ----------
let evolve (event: EdpEvent) (state: EnterpriseDemandPicture option) : EnterpriseDemandPicture option =
    match event with
    | EdpRevised edp -> Some edp
    | EdpCalculated edp -> Some edp
    | EdpPublished(edp, _) -> Some edp
