/// CA-P-010 Measure Promise Performance & Service Quality — PO-P-010 Policies
/// Architectural Lineage: ARS §3, §14; Table A.4 PO-P-010
module Medhavi.Promise.MeasurePerformance.PromiseQualityAssessment.Policies

open System
open Medhavi.SemanticModel

type SlaMeasurementPolicy =
    {
        PolicyId: string
        Version: int
        AllowableDisputeWindowDays: int
        MaxPenaltyCap: decimal
        DefaultDailyLatePenaltyRate: decimal
        GraceDays: int
    }

module SlaMeasurementPolicy =
    let defaultPolicy =
        {
            PolicyId = "PO-P-010"
            Version = 1
            AllowableDisputeWindowDays = 30
            MaxPenaltyCap = 10000.0m
            DefaultDailyLatePenaltyRate = 0.01m
            GraceDays = 0
        }

    /// Governs whether a customer dispute was submitted within the contractual dispute window
    let isDisputeWithinAllowableWindow (auditCompleted: Timestamp) (disputeTime: Timestamp) (windowDays: int) : bool =
        let auditUtc = Timestamp.value auditCompleted
        let disputeUtc = Timestamp.value disputeTime
        let elapsedDays = (disputeUtc - auditUtc).TotalDays
        elapsedDays >= 0.0 && elapsedDays <= float windowDays

    /// Caps a calculated liquidated damages penalty to contractual max cap
    let applyContractualPenaltyCap (rawPenalty: decimal) (maxCap: decimal) : decimal =
        if maxCap <= 0.0m then rawPenalty else min rawPenalty maxCap
