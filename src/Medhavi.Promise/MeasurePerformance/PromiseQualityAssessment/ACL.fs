/// CA-P-010 Measure Promise Performance & Service Quality — ACL
module Medhavi.Promise.MeasurePerformance.PromiseQualityAssessment.ACL

open System
open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model

let toAuditCmd (req: AuditCommitmentQualityReq) : Validation<AuditFulfilledCommitmentCmd, DomainError> =
    let create assessId commId podDate delQty auditTime =
        let dummyLineId = OrderLineId.create "LINE-01" |> Result.defaultWith (fun _ -> failwith "unreachable")
        let dummyCustId = CustomerId.create "CUST-DEFAULT" |> Result.defaultWith (fun _ -> failwith "unreachable")
        let dummyItem = ItemId.create "ITEM-DEFAULT" |> Result.defaultWith (fun _ -> failwith "unreachable")
        { AssessmentId = assessId
          CommitmentId = commId
          OrderLineId = dummyLineId
          CustomerId = dummyCustId
          Item = dummyItem
          RevisionZeroCommittedDate = podDate
          RevisionZeroCommittedQty = delQty
          FinalPromisedDeliveryDate = podDate
          ActualDeliveredDate = podDate
          ActualDeliveredQty = delQty
          AuditFactors = []
          AuditTime = auditTime }

    create <!> validatePromiseQualityAssessmentId req.AssessmentId
           <*> validateOrderCommitmentId req.CommitmentId
           <*> validateTimestamp req.ProofOfDeliveryDate
           <*> validateQty req.DeliveredQuantity
           <*> validateTimestamp req.AuditTime

let toDisputeCmd (req: RecordPromiseDisputeReq) : Validation<RecordPromiseDisputeCmd, DomainError> =
    let create assessId disputeTime =
        { AssessmentId = assessId
          CustomerClaimReference = req.AssessmentId + "#DISPUTE"
          DisputeReason = req.DisputeReason
          ClaimedLossAmount = req.ClaimedLossAmount
          DisputeTime = disputeTime }

    create <!> validatePromiseQualityAssessmentId req.AssessmentId
           <*> validateTimestamp req.DisputeTime

let toArsEventId = function
    | CommitmentQualityAudited _ -> ArsIdentifiers.EnterpriseEvents.commitmentQualityAudited.Id
    | CommitmentDisputeRecorded _ -> ArsIdentifiers.EnterpriseEvents.commitmentDisputeRecorded.Id

let toDto (entity: PromiseQualityAssessment) : PromiseQualityAssessmentDto =
    let isOnTime =
        match entity.OTIFStatus with
        | OTIF_Pass
        | OTIF_ShortageDefect -> true
        | _ -> false

    let isInFull =
        match entity.OTIFStatus with
        | OTIF_Pass
        | OTIF_LateDefect -> true
        | _ -> false

    let otifPassed =
        match entity.OTIFStatus with
        | OTIF_Pass -> true
        | _ -> false

    let stateStr =
        match entity.AuditStatus with
        | InitiatedOnPod -> "InitiatedOnPod"
        | AuditedFinal _ -> "AuditedFinal"
        | InDispute _ -> "InDispute"
        | SettledClosed _ -> "SettledClosed"

    let lateDays =
        Algorithms.calculateLateDays entity.FinalPromisedDeliveryDate entity.ActualDeliveredDate

    { AssessmentId = PromiseQualityAssessmentId.value entity.AuditId
      CommitmentId = OrderCommitmentId.value entity.CommitmentId
      OrderLineId = OrderLineId.value entity.OrderLineId
      RevisionZeroCommittedDate = Timestamp.value entity.RevisionZeroCommittedDate
      ActualDeliveryDate = Timestamp.value entity.ActualDeliveredDate
      PromisedQuantity = Quantity.value entity.RevisionZeroCommittedQty
      DeliveredQuantity = Quantity.value entity.ActualDeliveredQty
      IsOnTime = isOnTime
      IsInFull = isInFull
      OTIFPassed = otifPassed
      LateDays = lateDays
      AttributedFailureCategory = Some (sprintf "%A" entity.FailureAttribution)
      LiquidatedDamagesAccrued = entity.CalculatedPenaltyAmount
      AssessmentTime =
        match entity.AuditStatus with
        | AuditedFinal t -> Timestamp.value t
        | InDispute t -> Timestamp.value t
        | SettledClosed t -> Timestamp.value t
        | InitiatedOnPod -> Timestamp.value entity.ActualDeliveredDate
      LifecycleState = stateStr
      DecisionTraceability = entity.DecisionTraceability }
