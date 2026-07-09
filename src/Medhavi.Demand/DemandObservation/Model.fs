module Medhavi.Demand.DemandObservation.Model

open Medhavi.Demand
open Medhavi.SharedKernel

// =============================================================================
// SE‑D‑001 — Demand Observation
// =============================================================================
type ObservationType =
    | SalesOrder
    | Shipment
    | POS
    | Return
    | Correction
    | Signal

type ObservationStatus =
    | Received
    | Accepted
    | Quarantined
    | Rejected

type ObservationDecision =
    { DecisionId: string
      Timestamp: Timestamp
      Confidence: decimal
      Rationale: string
      WarningCode: string option }

type DemandObservation =
    { Id: DemandObservationId
      SkuId: SkuId
      StockingPointId: StockingPointId
      Quantity: Quantity
      ObservationType: ObservationType
      BusinessTime: Timestamp
      CustomerId: CustomerId option
      PromotionRef: string option
      CampaignRef: string option
      ContractRef: string option
      PlanningScopeId: PlanningScopeId option
      Status: ObservationStatus
      Decision: ObservationDecision option
      Provenance: Provenance }

    member this.AssignmentId = DemandObservationId.value this.Id

// ---------- Commands ----------
/// FS-D-001 — Receive Business Observation
type EstablishObservationCmd =
    { ObservationId: DemandObservationId
      SkuId: SkuId
      StockingPointId: StockingPointId
      Quantity: Quantity
      ObservationType: ObservationType
      BusinessTime: Timestamp
      CustomerId: CustomerId option
      PromotionRef: string option
      CampaignRef: string option
      ContractRef: string option
      Provenance: Provenance }

type EvaluateObservationCmd =
    { ObservationId: DemandObservationId
      Signal: DemandSignal option } // optional external signal for acceptance rules

type AssignScopeCmd =
    { ObservationId: DemandObservationId
      PlanningScopeId: PlanningScopeId }

type ObservationCommand =
    | Establish of EstablishObservationCmd
    | Evaluate of EvaluateObservationCmd
    | AssignScope of AssignScopeCmd

    member this.AssignmentId =
        match this with
        | Establish c -> DemandObservationId.value c.ObservationId
        | Evaluate c -> DemandObservationId.value c.ObservationId
        | AssignScope c -> DemandObservationId.value c.ObservationId

// ---------- Events ----------
type ObservationEvent =
    | ObservationEstablished of DemandObservation
    | ObservationAccepted of obsId: DemandObservationId * decision: ObservationDecision
    | ObservationQuarantined of obsId: DemandObservationId * decision: ObservationDecision
    | ObservationRejected of obsId: DemandObservationId * decision: ObservationDecision
    | ObservationWarningRecorded of obsId: DemandObservationId * warningCode: string * decision: ObservationDecision
    | ObservationScopeAssigned of obsId: DemandObservationId * PlanningScopeId

// ---------- Evolve ----------
let evolve (event: ObservationEvent) (state: DemandObservation option) : DemandObservation option =
    match event with
    | ObservationEstablished obs -> Some obs
    | ObservationAccepted(_, decision) ->
        state
        |> Option.map(fun s ->
            { s with
                Status = Accepted
                Decision = Some decision })
    | ObservationQuarantined(_, decision) ->
        state
        |> Option.map(fun s ->
            { s with
                Status = Quarantined
                Decision = Some decision })
    | ObservationRejected(_, decision) ->
        state
        |> Option.map(fun s ->
            { s with
                Status = Rejected
                Decision = Some decision })
    | ObservationWarningRecorded(_, code, decision) ->
        state
        |> Option.map(fun s ->
            { s with
                Status = Accepted
                Decision =
                    Some
                        { decision with
                            WarningCode = Some code } })
    | ObservationScopeAssigned(_, scopeId) ->
        state
        |> Option.map(fun s ->
            { s with
                PlanningScopeId = Some scopeId })
