/// CA-P-010 Measure Promise Performance & Service Quality — BA-P-006 Algorithms
/// Architectural Lineage: ARS §3, §14; Table A.6 BA-P-006
module Medhavi.Promise.MeasurePerformance.PromiseQualityAssessment.Algorithms

open System
open Medhavi.SemanticModel
open Model
open Policies

/// Evaluates OTIF classification for final revision
let evaluateOTIF
    (committedDate: Timestamp)
    (actualDate: Timestamp)
    (committedQty: Quantity)
    (deliveredQty: Quantity)
    (graceDays: int)
    : OTIFClassification =
    let committedUtc = Timestamp.value committedDate
    let actualUtc = Timestamp.value actualDate
    let daysLate = (actualUtc - committedUtc).TotalDays
    let isLate = daysLate > float graceDays
    let isShort = Quantity.value deliveredQty < Quantity.value committedQty

    match isLate, isShort with
    | false, false -> OTIF_Pass
    | true, false -> OTIF_LateDefect
    | false, true -> OTIF_ShortageDefect
    | true, true -> OTIF_LateAndShortDefect

/// Evaluates OFIF classification against Revision 0 initial baseline
let evaluateOFIF
    (rev0Date: Timestamp)
    (actualDate: Timestamp)
    (rev0Qty: Quantity)
    (deliveredQty: Quantity)
    (graceDays: int)
    : OFIFClassification =
    let rev0Utc = Timestamp.value rev0Date
    let actualUtc = Timestamp.value actualDate
    let daysLate = (actualUtc - rev0Utc).TotalDays
    let isLate = daysLate > float graceDays
    let isShort = Quantity.value deliveredQty < Quantity.value rev0Qty

    if not isLate && not isShort then OFIF_Pass else OFIF_Defect

/// Computes late days relative to committed date
let calculateLateDays (committedDate: Timestamp) (actualDate: Timestamp) : decimal =
    let committedUtc = Timestamp.value committedDate
    let actualUtc = Timestamp.value actualDate
    let diff = (actualUtc - committedUtc).TotalDays
    if diff > 0.0 then decimal diff else 0.0m

/// Computes liquidated damage penalty liability
let computeLiquidatedDamages
    (lateDays: decimal)
    (dailyPenaltyRate: decimal)
    (lineValue: decimal)
    (maxCap: decimal)
    : decimal =
    if lateDays <= 0.0m || dailyPenaltyRate <= 0.0m then
        0.0m
    else
        let rawPenalty = lineValue * dailyPenaltyRate * lateDays
        SlaMeasurementPolicy.applyContractualPenaltyCap rawPenalty maxCap

/// Attributes root cause failure category deterministically
let attributeFailureCausality
    (otif: OTIFClassification)
    (auditFactors: OTIFAuditFactorItem list)
    : FailureAttributionCategory =
    match otif with
    | OTIF_Pass -> NoFailureExecutionPerfect
    | OTIF_LateDefect -> LogisticsCarrierFailure
    | OTIF_ShortageDefect -> ManufacturingShortfall
    | OTIF_LateAndShortDefect -> SupplierDefault
