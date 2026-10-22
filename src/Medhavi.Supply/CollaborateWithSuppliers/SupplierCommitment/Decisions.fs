/// DE-S-041 — Accept Supplier Response Decision
/// Traces to: Supply Intelligence Specification §6 (DE-S-041)
module Medhavi.Supply.CollaborateWithSuppliers.SupplierCommitment.Decisions

open System
open Medhavi.Common
open Medhavi.SemanticModel
open Model

type ResponseEvaluationFacts =
    { AcknowledgeHours: decimal
      CommitHours: decimal
      HistoricalOtif: decimal option
      IsCriticalItem: bool
      ActiveRiskSignals: string list }

/// Evaluates supplier responsiveness against PO-S-060 thresholds (BR-S-080)
let evaluateResponsiveness (ackHours: decimal) (commitHours: decimal) : {| IsResponsive: bool; IsBorderline: bool |} =
    let ackTarget = 24.0m
    let commitTarget = 72.0m
    let isResponsive = ackHours <= ackTarget && commitHours <= commitTarget
    let isBorderline = (ackHours > ackTarget * 0.90m && ackHours <= ackTarget * 1.10m)
                       || (commitHours > commitTarget * 0.90m && commitHours <= commitTarget * 1.10m)
    {| IsResponsive = isResponsive; IsBorderline = isBorderline |}

/// Evaluates historical reliability against PO-S-060 thresholds (BR-S-081)
let evaluateReliability (otifOpt: decimal option) : {| IsReliable: bool; IsBorderline: bool |} =
    match otifOpt with
    | None -> {| IsReliable = false; IsBorderline = false |}
    | Some otif ->
        let isReliable = otif >= 0.95m
        let isBorderline = otif >= 0.90m && otif < 0.95m
        {| IsReliable = isReliable; IsBorderline = isBorderline |}

/// Evaluates commitment confidence per BR-S-082
let evaluateConfidence
    (isResponsive: bool)
    (isReliable: bool)
    (hasHistory: bool)
    (riskSignalsCount: int)
    : CommitmentConfidenceLevel =
    if not hasHistory then
        CommitmentConfidenceLevel.Estimated
    elif isResponsive && isReliable && riskSignalsCount = 0 then
        CommitmentConfidenceLevel.Firm
    elif (isResponsive || isReliable) && riskSignalsCount = 0 then
        CommitmentConfidenceLevel.Conditional
    elif riskSignalsCount = 1 then
        CommitmentConfidenceLevel.Tentative
    else
        CommitmentConfidenceLevel.Estimated

/// DE-S-041: Accept Supplier Response applying governed conflict resolution rules
let acceptResponse
    (eventType: CollaborationEventType)
    (requestedQty: Quantity)
    (committedQtyOpt: Quantity option)
    (requestedDate: Timestamp)
    (committedDateOpt: Timestamp option)
    (facts: ResponseEvaluationFacts)
    : {| AcceptanceOutcome: SupplierResponseAcceptanceOutcome
         NewState: SupplierCommitmentState
         Confidence: CommitmentConfidenceLevel
         DataQualityFlag: string option |} =

    let respEval = evaluateResponsiveness facts.AcknowledgeHours facts.CommitHours
    let relEval = evaluateReliability facts.HistoricalOtif
    let hasHistory = facts.HistoricalOtif.IsSome
    let riskCount = facts.ActiveRiskSignals.Length
    let confidence = evaluateConfidence respEval.IsResponsive relEval.IsReliable hasHistory riskCount

    match eventType with
    | CollaborationEventType.SupplierAcknowledgement ->
        {| AcceptanceOutcome = SupplierResponseAcceptanceOutcome.EscalateForReview "Supplier acknowledgement received; awaiting commitment"
           NewState = SupplierCommitmentState.Acknowledged
           Confidence = confidence
           DataQualityFlag = None |}

    | CollaborationEventType.SupplierRejection ->
        // Priority 1: Supplier rejects the requirement entirely
        {| AcceptanceOutcome = SupplierResponseAcceptanceOutcome.RejectAndSeekAlternative "Supplier rejected requirement entirely (Priority 1)"
           NewState = SupplierCommitmentState.Rejected
           Confidence = confidence
           DataQualityFlag = None |}

    | CollaborationEventType.SupplierAlternative ->
        // Priority 2: Supplier proposes an alternative
        {| AcceptanceOutcome = SupplierResponseAcceptanceOutcome.EscalateForReview "Supplier proposed alternative; requires planner review (Priority 2)"
           NewState = SupplierCommitmentState.AlternativeProposed
           Confidence = confidence
           DataQualityFlag = None |}

    | CollaborationEventType.EnterpriseConfirmation ->
        {| AcceptanceOutcome = SupplierResponseAcceptanceOutcome.AcceptAsFirm
           NewState = SupplierCommitmentState.Confirmed
           Confidence = confidence
           DataQualityFlag = None |}

    | CollaborationEventType.EnterpriseRequest ->
        {| AcceptanceOutcome = SupplierResponseAcceptanceOutcome.EscalateForReview "Enterprise request recorded; awaiting supplier response"
           NewState = SupplierCommitmentState.Proposed
           Confidence = confidence
           DataQualityFlag = None |}

    | CollaborationEventType.SupplierCommitment ->
        let isFullQty =
            match committedQtyOpt with
            | Some q -> Quantity.value q >= Quantity.value requestedQty
            | None -> false

        let isOnTime =
            match committedDateOpt with
            | Some d -> Timestamp.value d <= Timestamp.value requestedDate
            | None -> false

        if not isFullQty || not isOnTime then
            // Priority 2: Supplier partially commits
            {| AcceptanceOutcome = SupplierResponseAcceptanceOutcome.EscalateForReview $"Partial commitment: Qty={committedQtyOpt}, Date={committedDateOpt} (Priority 2)"
               NewState = SupplierCommitmentState.PartiallyCommitted
               Confidence = confidence
               DataQualityFlag = None |}
        else
            // Full commitment
            if facts.IsCriticalItem && confidence = CommitmentConfidenceLevel.Estimated then
                // Priority 7: Supplier commits fully, confidence is Estimated, requirement is Critical
                {| AcceptanceOutcome = SupplierResponseAcceptanceOutcome.RejectAndSeekAlternative "Critical requirement cannot rely on Estimated confidence (Priority 7)"
                   NewState = SupplierCommitmentState.Rejected
                   Confidence = confidence
                   DataQualityFlag = None |}
            elif facts.IsCriticalItem && not relEval.IsReliable then
                // Priority 6: Supplier commits fully, requirement is Critical and reliability checks fail
                {| AcceptanceOutcome = SupplierResponseAcceptanceOutcome.EscalateForReview "Critical requirement with sub-threshold supplier reliability (Priority 6)"
                   NewState = SupplierCommitmentState.Committed
                   Confidence = confidence
                   DataQualityFlag = None |}
            elif confidence = CommitmentConfidenceLevel.Firm || confidence = CommitmentConfidenceLevel.Conditional then
                if respEval.IsBorderline || relEval.IsBorderline then
                    // Priority 5: Borderline responsiveness or reliability
                    {| AcceptanceOutcome = SupplierResponseAcceptanceOutcome.AcceptAsFirm
                       NewState = SupplierCommitmentState.Confirmed
                       Confidence = confidence
                       DataQualityFlag = Some "Responsiveness or reliability metrics are borderline (within 10% of threshold)" |}
                else
                    // Priority 3: Full commitment, Firm/Conditional confidence, Responsive, Reliable
                    {| AcceptanceOutcome = SupplierResponseAcceptanceOutcome.AcceptAsFirm
                       NewState = SupplierCommitmentState.Confirmed
                       Confidence = confidence
                       DataQualityFlag = None |}
            elif confidence = CommitmentConfidenceLevel.Tentative || confidence = CommitmentConfidenceLevel.Estimated then
                // Priority 4: Full commitment, Tentative or Estimated confidence -> Apply Buffer
                let bufferQty = 0.20m * Quantity.value requestedQty
                let bufferDays = 3.0m
                {| AcceptanceOutcome = SupplierResponseAcceptanceOutcome.AcceptWithBuffer(bufferQty, bufferDays)
                   NewState = SupplierCommitmentState.Confirmed
                   Confidence = confidence
                   DataQualityFlag = None |}
            else
                // Fallback
                {| AcceptanceOutcome = SupplierResponseAcceptanceOutcome.EscalateForReview "No matching deterministic rule; review required"
                   NewState = SupplierCommitmentState.Committed
                   Confidence = confidence
                   DataQualityFlag = None |}
