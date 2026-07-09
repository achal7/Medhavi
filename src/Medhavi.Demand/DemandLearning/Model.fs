module Medhavi.Demand.DemandLearning.Model

open Medhavi.SharedKernel
open Medhavi.Demand

// =============================================================================
// SE‑D‑042 — Demand Learning
// =============================================================================

type DemandLearning =
    { Id: DemandLearningId
      PlanningScopeId: PlanningScopeId option
      LearningType: string
      LearningStatement: string
      SupportingEvidence: string list
      EvidenceStrength: string
      SourceAnalysisRef: string
      BusinessTime: Timestamp
      TransactionTime: Timestamp }

// ---------- Commands ----------
type RecordDemandLearningCmd =
    { LearningId: DemandLearningId
      PlanningScopeId: PlanningScopeId option
      LearningType: string
      LearningStatement: string
      SupportingEvidence: string list
      EvidenceStrength: string
      SourceAnalysisRef: string
      BusinessTime: Timestamp
      TransactionTime: Timestamp }

type DemandLearningCommand =
    | Record of RecordDemandLearningCmd

    member this.LearningId =
        match this with
        | Record c -> c.LearningId
        |> DemandLearningId.value

// ---------- Events ----------
type DemandLearningEvent = DemandLearningRecorded of DemandLearning

// ---------- Evolve ----------
let evolve (evt: DemandLearningEvent) (_: DemandLearning option) : DemandLearning option =
    match evt with
    | DemandLearningRecorded learning -> Some learning
