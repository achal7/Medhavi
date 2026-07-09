module Medhavi.Demand.DemandBehaviourAssessment.Model

open Medhavi.SharedKernel
open Medhavi.Demand

type BehaviourState =
    | Normal
    | Elevated
    | Depressed
    | Critical

    member this.AsString() =
        match this with
        | Normal -> "Normal"
        | Elevated -> "Elevated"
        | Depressed -> "Depressed"
        | Critical -> "Critical"

type DeviationDirection =
    | Increase
    | Decrease

type StateChangeEvent =
    { EventId: string
      Timestamp: Timestamp
      PreviousState: BehaviourState
      NewState: BehaviourState
      DeviationMagnitude: PositiveDecimal
      DeviationDirection: DeviationDirection
      ConfidenceScore: PositiveDecimal
      CorroboratingSignalCount: int
      BaselineReference: string
      TriggeringSignalId: string option }

module Identity =
    let create (skuId: SkuId) (spId: StockingPointId) =
        $"{SkuId.value skuId}-{StockingPointId.value spId}"

type DemandBehaviourAssessment =
    { SkuId: SkuId
      StockingPointId: StockingPointId
      CurrentState: BehaviourState
      LastUpdated: Timestamp
      CurrentDeviation: PositiveDecimal option
      Confidence: PositiveDecimal
      CorroboratingSignalCount: int
      BaselineReference: string
      ActiveSources: string list
      LastSignalTime: Timestamp option
      LastStateChange: StateChangeEvent option
      BusinessTime: Timestamp
      TransactionTime: Timestamp }

    member this.AssignmentId = Identity.create this.SkuId this.StockingPointId

type EvaluateSignalCmd =
    { Signal: DemandSignal
      SkuId: SkuId
      StockingPointId: StockingPointId
      IsHighPriority: bool }

    member this.AssignmentId = Identity.create this.SkuId this.StockingPointId

type AcknowledgeCmd =
    { SkuId: SkuId
      StockingPointId: StockingPointId
      PlannerIdentity: string
      Justification: string }

    member this.AssignmentId = Identity.create this.SkuId this.StockingPointId

type DemandBehaviourAssessmentCommand =
    | EvaluateSignal of EvaluateSignalCmd
    | Acknowledge of AcknowledgeCmd

    member this.AssignmentId =
        match this with
        | EvaluateSignal c -> c.AssignmentId
        | Acknowledge c -> c.AssignmentId

type DemandBehaviourAssessmentEvent =
    | BehaviourStateChanged of state: DemandBehaviourAssessment * change: StateChangeEvent
    | BehaviourAssessmentAcknowledged of
        sku: SkuId *
        sp: StockingPointId *
        PlannerIdentity: string *
        Justification: string *
        timestamp: Timestamp

let evolve (evt: DemandBehaviourAssessmentEvent) (stateOpt: DemandBehaviourAssessment option) =
    match evt with
    | BehaviourStateChanged(state, _) -> Some state
    | BehaviourAssessmentAcknowledged _ -> stateOpt
