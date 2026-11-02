/// CA-P-010 Measure Promise Performance & Service Quality — Behaviors
/// Architectural Lineage: ARS §3, §4, §14; AB-P-021, AB-P-022
module Medhavi.Promise.MeasurePerformance.PromiseQualityAssessment.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Model
open Rules
open Decisions
open Policies

let evolve (state: PromiseQualityAssessment option) (event: CommitmentQualityEvent) : PromiseQualityAssessment option =
    match event with
    | CommitmentQualityAudited e ->
        match state with
        | Some s ->
            Some
                { s with
                    OTIFStatus = e.OTIFStatus
                    OFIFStatus = e.OFIFStatus
                    CalculatedPenaltyAmount = e.PenaltyAmount
                    AuditStatus = AuditedFinal e.AuditCompletedEpoch
                }
        | None -> None
    | CommitmentDisputeRecorded e ->
        match state with
        | Some s ->
            Some
                { s with
                    AuditStatus = InDispute e.DisputeTime
                }
        | None -> None

let buildTrace policy decisionId decisionOutcome state events summary =
    let policyId = policy |> Option.map(fun (p: SlaMeasurementPolicy) -> p.PolicyId)
    let policyVersion = policy |> Option.map(fun (p: SlaMeasurementPolicy) -> p.Version)

    buildDecisionWithTrace
        evolve
        state
        events
        decisionId
        []
        Capabilities.measurePromisePerformanceAndServiceQuality.Id
        decisionOutcome
        policyId
        policyVersion
        [ SemanticObjects.promiseQualityAssessment.Id ]
        (Some summary)

/// AB-P-021: Audit Fulfilled Commitment
let audit
    (policy: SlaMeasurementPolicy)
    (commitmentExists: bool)
    : Decide<PromiseQualityAssessment, AuditFulfilledCommitmentCmd, CommitmentQualityEvent> =
    fun (cmd: AuditFulfilledCommitmentCmd) (state: PromiseQualityAssessment option) ->
        result {
            match state with
            | Some _ ->
                return!
                    Error(
                        DomainError.conflict(
                            $"Audit already exists: {PromiseQualityAssessmentId.value cmd.AssessmentId}",
                            Capabilities.measurePromisePerformanceAndServiceQuality.Id
                        )
                    )
            | None ->
                let input: AuditQualityInput =
                    {
                        Cmd = cmd
                        CommitmentExists = commitmentExists
                        Policy = policy
                    }

                let! decision = evaluateAudit auditRules input
                let traceId = Guid.NewGuid().ToString()

                match decision.Outcome with
                | AuditAuthorized ->
                    let otif =
                        Algorithms.evaluateOTIF
                            cmd.FinalPromisedDeliveryDate
                            cmd.ActualDeliveredDate
                            cmd.RevisionZeroCommittedQty
                            cmd.ActualDeliveredQty
                            policy.GraceDays

                    let ofif =
                        Algorithms.evaluateOFIF
                            cmd.RevisionZeroCommittedDate
                            cmd.ActualDeliveredDate
                            cmd.RevisionZeroCommittedQty
                            cmd.ActualDeliveredQty
                            policy.GraceDays

                    let lateDays = Algorithms.calculateLateDays cmd.FinalPromisedDeliveryDate cmd.ActualDeliveredDate

                    let penalty =
                        Algorithms.computeLiquidatedDamages
                            lateDays
                            policy.DefaultDailyLatePenaltyRate
                            1000.0m
                            policy.MaxPenaltyCap

                    let failureCat = Algorithms.attributeFailureCausality otif cmd.AuditFactors

                    let assessment: PromiseQualityAssessment =
                        {
                            AuditId = cmd.AssessmentId
                            CommitmentId = cmd.CommitmentId
                            OrderLineId = cmd.OrderLineId
                            CustomerId = cmd.CustomerId
                            Item = cmd.Item
                            RevisionZeroCommittedDate = cmd.RevisionZeroCommittedDate
                            RevisionZeroCommittedQty = cmd.RevisionZeroCommittedQty
                            FinalPromisedDeliveryDate = cmd.FinalPromisedDeliveryDate
                            ActualDeliveredDate = cmd.ActualDeliveredDate
                            ActualDeliveredQty = cmd.ActualDeliveredQty
                            OTIFStatus = otif
                            OFIFStatus = ofif
                            FailureAttribution = failureCat
                            CalculatedPenaltyAmount = penalty
                            AuditStatus = AuditedFinal cmd.AuditTime
                            AuditFactors = cmd.AuditFactors
                            Disputes = []
                            DecisionTraceability = Some traceId
                        }

                    let events =
                        [
                            CommitmentQualityAudited
                                {|
                                    AssessmentId = cmd.AssessmentId
                                    CommitmentId = cmd.CommitmentId
                                    CustomerId = cmd.CustomerId
                                    OTIFStatus = otif
                                    OFIFStatus = ofif
                                    PenaltyAmount = penalty
                                    AuditCompletedEpoch = cmd.AuditTime
                                |}
                        ]

                    return
                        buildTrace
                            (Some policy)
                            traceId
                            decision
                            state
                            events
                            $"Commitment quality audited for {OrderCommitmentId.value cmd.CommitmentId}"

                | AuditRejected reason ->
                    return! Error(DomainError.rule(reason, rule = Rules.promisePerformanceRecordIdentity.Id))
        }

/// AB-P-022: Record Customer Commercial Dispute
let dispute
    (policy: SlaMeasurementPolicy)
    : Decide<PromiseQualityAssessment, RecordPromiseDisputeCmd, CommitmentQualityEvent> =
    fun (cmd: RecordPromiseDisputeCmd) (state: PromiseQualityAssessment option) ->
        result {
            match state with
            | None ->
                return!
                    Error(
                        DomainError.notFound(
                            SemanticObjects.promiseQualityAssessment.Id,
                            PromiseQualityAssessmentId.value cmd.AssessmentId
                        )
                    )
            | Some assessment ->
                let input: DisputeInput =
                    {
                        Cmd = cmd
                        CurrentAssessment = state
                        Policy = policy
                    }

                let! decision = evaluateDispute disputeRules input
                let traceId = Guid.NewGuid().ToString()

                match decision.Outcome with
                | DisputeAuthorized ->
                    let claim: CustomerPromiseDisputeClaim =
                        {
                            DisputeClaimId = cmd.CustomerClaimReference
                            CustomerClaimReference = cmd.CustomerClaimReference
                            ClaimedPenaltyAmount = cmd.ClaimedLossAmount
                            CustomerClaimDescription = cmd.DisputeReason
                            ClaimSubmissionTimestamp = cmd.DisputeTime
                            Adjudication = UnderReview
                            SettledTimestamp = None
                        }

                    let events =
                        [
                            CommitmentDisputeRecorded
                                {|
                                    AssessmentId = cmd.AssessmentId
                                    DisputeClaimId = cmd.CustomerClaimReference
                                    CustomerId = assessment.CustomerId
                                    ClaimedAmount = cmd.ClaimedLossAmount
                                    DisputeTime = cmd.DisputeTime
                                |}
                        ]

                    return
                        buildTrace
                            (Some policy)
                            traceId
                            decision
                            state
                            events
                            $"Customer dispute logged for audit {PromiseQualityAssessmentId.value cmd.AssessmentId}"

                | DisputeRejected reason ->
                    return! Error(DomainError.rule(reason, rule = Rules.penaltyAccrualDerivation.Id))
        }
