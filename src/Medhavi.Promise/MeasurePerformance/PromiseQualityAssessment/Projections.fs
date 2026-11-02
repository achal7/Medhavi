/// CA-P-010 Measure Promise Performance & Service Quality — Projections
/// Architectural Lineage: ARS §3, §14; Table A.3 EV-P-022, EV-P-023
module Medhavi.Promise.MeasurePerformance.PromiseQualityAssessment.Projections

open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model

type State = Map<PromiseQualityAssessmentId, PromiseQualityAssessmentDto>

let initial: State = Map.empty

let apply (state: State) (event: CommitmentQualityEvent) : State =
    match event with
    | CommitmentQualityAudited e ->
        state
        |> Map.change e.AssessmentId (function
            | Some dto ->
                Some
                    { dto with
                        OTIFPassed =
                            match e.OTIFStatus with
                            | OTIF_Pass -> true
                            | _ -> false
                        LiquidatedDamagesAccrued = e.PenaltyAmount
                        LifecycleState = "AuditedFinal" }
            | None ->
                Some
                    { AssessmentId = PromiseQualityAssessmentId.value e.AssessmentId
                      CommitmentId = OrderCommitmentId.value e.CommitmentId
                      OrderLineId = "LINE-01"
                      RevisionZeroCommittedDate = Medhavi.SemanticModel.Timestamp.value e.AuditCompletedEpoch
                      ActualDeliveryDate = Medhavi.SemanticModel.Timestamp.value e.AuditCompletedEpoch
                      PromisedQuantity = 0.0m
                      DeliveredQuantity = 0.0m
                      IsOnTime =
                        match e.OTIFStatus with
                        | OTIF_Pass | OTIF_ShortageDefect -> true
                        | _ -> false
                      IsInFull =
                        match e.OTIFStatus with
                        | OTIF_Pass | OTIF_LateDefect -> true
                        | _ -> false
                      OTIFPassed =
                        match e.OTIFStatus with
                        | OTIF_Pass -> true
                        | _ -> false
                      LateDays = 0.0m
                      AttributedFailureCategory = None
                      LiquidatedDamagesAccrued = e.PenaltyAmount
                      AssessmentTime = Medhavi.SemanticModel.Timestamp.value e.AuditCompletedEpoch
                      LifecycleState = "AuditedFinal"
                      DecisionTraceability = None })

    | CommitmentDisputeRecorded e ->
        state
        |> Map.change e.AssessmentId (Option.map (fun dto ->
            { dto with
                LifecycleState = "InDispute" }))

let seedFromAggregates (aggregates: PromiseQualityAssessment list) : State =
    aggregates
    |> List.map (fun a -> a.AuditId, ACL.toDto a)
    |> Map.ofList
