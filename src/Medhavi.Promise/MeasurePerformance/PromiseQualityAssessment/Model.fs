/// CA-P-010 Measure Promise Performance & Service Quality — SE-P-070 Domain Model
/// Architectural Lineage: ARS §3, §4, §5, §14, §16, §17, §18; Table A.1 SE-P-070; AB-P-021, AB-P-022
module Medhavi.Promise.MeasurePerformance.PromiseQualityAssessment.Model

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Promise

type OTIFClassification =
    | OTIF_Pass
    | OTIF_LateDefect
    | OTIF_ShortageDefect
    | OTIF_LateAndShortDefect

type OFIFClassification =
    | OFIF_Pass
    | OFIF_Defect

type FailureAttributionCategory =
    | NoFailureExecutionPerfect
    | SupplierDefault
    | ManufacturingShortfall
    | LogisticsCarrierFailure
    | InventoryRecordInaccuracy
    | CustomerInitiatedModification

type DisputeAdjudicationStatus =
    | UnderReview
    | Approved of CreditAmount: decimal
    | Rejected of RejectionReason: string

type CustomerPromiseDisputeClaim =
    {
        DisputeClaimId: string
        CustomerClaimReference: string
        ClaimedPenaltyAmount: decimal
        CustomerClaimDescription: string
        ClaimSubmissionTimestamp: Timestamp
        Adjudication: DisputeAdjudicationStatus
        SettledTimestamp: Timestamp option
    }

type OTIFAuditFactorItem =
    {
        AuditFactorId: string
        ShipmentLegId: string
        FulfillmentNodeId: LocationId
        ShippedQuantity: Quantity
        DeliveredQuantity: Quantity
        PromisedArrivalDate: Timestamp
        ActualArrivalDate: Timestamp
        CarrierTrackingNumber: string
    }

type QualityAssessmentStatus =
    | InitiatedOnPod
    | AuditedFinal of AuditCompletedEpoch: Timestamp
    | InDispute of DisputeTime: Timestamp
    | SettledClosed of SettlementTime: Timestamp

type PromiseQualityAssessment =
    {
        AuditId: PromiseQualityAssessmentId
        CommitmentId: OrderCommitmentId
        OrderLineId: OrderLineId
        CustomerId: CustomerId
        Item: ItemId
        RevisionZeroCommittedDate: Timestamp
        RevisionZeroCommittedQty: Quantity
        FinalPromisedDeliveryDate: Timestamp
        ActualDeliveredDate: Timestamp
        ActualDeliveredQty: Quantity
        OTIFStatus: OTIFClassification
        OFIFStatus: OFIFClassification
        FailureAttribution: FailureAttributionCategory
        CalculatedPenaltyAmount: decimal
        AuditStatus: QualityAssessmentStatus
        AuditFactors: OTIFAuditFactorItem list
        Disputes: CustomerPromiseDisputeClaim list
        DecisionTraceability: string option
    }

type AuditFulfilledCommitmentCmd =
    {
        AssessmentId: PromiseQualityAssessmentId
        CommitmentId: OrderCommitmentId
        OrderLineId: OrderLineId
        CustomerId: CustomerId
        Item: ItemId
        RevisionZeroCommittedDate: Timestamp
        RevisionZeroCommittedQty: Quantity
        FinalPromisedDeliveryDate: Timestamp
        ActualDeliveredDate: Timestamp
        ActualDeliveredQty: Quantity
        AuditFactors: OTIFAuditFactorItem list
        AuditTime: Timestamp
    }

type RecordPromiseDisputeCmd =
    {
        AssessmentId: PromiseQualityAssessmentId
        CustomerClaimReference: string
        DisputeReason: string
        ClaimedLossAmount: decimal
        DisputeTime: Timestamp
    }

type CommitmentQualityEvent =
    | CommitmentQualityAudited of
        {|
            AssessmentId: PromiseQualityAssessmentId
            CommitmentId: OrderCommitmentId
            CustomerId: CustomerId
            OTIFStatus: OTIFClassification
            OFIFStatus: OFIFClassification
            PenaltyAmount: decimal
            AuditCompletedEpoch: Timestamp
        |}
    | CommitmentDisputeRecorded of
        {|
            AssessmentId: PromiseQualityAssessmentId
            DisputeClaimId: string
            CustomerId: CustomerId
            ClaimedAmount: decimal
            DisputeTime: Timestamp
        |}
