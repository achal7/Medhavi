/// CA-P-010 Measure Promise Performance & Service Quality — Rules
/// Architectural Lineage: ARS §3, §14; Table A.5 BR-P-011, BR-P-066, BR-P-085
module Medhavi.Promise.MeasurePerformance.PromiseQualityAssessment.Rules

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Model
open Policies

type AuditQualityInput =
    { Cmd: AuditFulfilledCommitmentCmd
      CommitmentExists: bool
      Policy: SlaMeasurementPolicy }

type DisputeInput =
    { Cmd: RecordPromiseDisputeCmd
      CurrentAssessment: PromiseQualityAssessment option
      Policy: SlaMeasurementPolicy }

/// BR-P-011: Promise Quality Assessment must uniquely identify audited commitment
let qualityAuditPrerequisitesRule: Rule<AuditQualityInput> =
    Rule.create
        Rules.promisePerformanceRecordIdentity.Id
        Rules.promisePerformanceRecordIdentity.Explanation
        (fun input ->
            input.CommitmentExists
            && not (String.IsNullOrWhiteSpace(PromiseQualityAssessmentId.value input.Cmd.AssessmentId)))
        (fun input ->
            $"CommitmentExists: %b{input.CommitmentExists}, AuditId: %s{PromiseQualityAssessmentId.value input.Cmd.AssessmentId}")

/// BR-P-066: Delivery is on-time if actual receipt date <= committed date and in-full if qty >= 100%
let otifDeliveredQuantityRule: Rule<AuditQualityInput> =
    Rule.create
        Rules.otifEvaluationStandardRule.Id
        Rules.otifEvaluationStandardRule.Explanation
        (fun input -> Quantity.value input.Cmd.ActualDeliveredQty >= 0.0m)
        (fun input -> $"DeliveredQty: %M{Quantity.value input.Cmd.ActualDeliveredQty}")

let auditRules: Rule<AuditQualityInput> list =
    [ qualityAuditPrerequisitesRule; otifDeliveredQuantityRule ]

/// BR-P-085: Accrued liquidated damages multiplying daily penalty rate by late days
let disputeClaimAmountRule: Rule<DisputeInput> =
    Rule.create
        Rules.penaltyAccrualDerivation.Id
        Rules.penaltyAccrualDerivation.Explanation
        (fun input ->
            match input.CurrentAssessment with
            | Some a ->
                input.Cmd.ClaimedLossAmount >= 0.0m
                && (match a.AuditStatus with
                    | AuditedFinal auditTime ->
                        SlaMeasurementPolicy.isDisputeWithinAllowableWindow auditTime input.Cmd.DisputeTime input.Policy.AllowableDisputeWindowDays
                    | InDispute _ -> true
                    | _ -> false)
            | None -> false)
        (fun input ->
            match input.CurrentAssessment with
            | Some a -> $"ClaimedAmount: %M{input.Cmd.ClaimedLossAmount}, Status: %A{a.AuditStatus}"
            | None -> "AssessmentNotFound")

let disputeRules: Rule<DisputeInput> list =
    [ disputeClaimAmountRule ]
