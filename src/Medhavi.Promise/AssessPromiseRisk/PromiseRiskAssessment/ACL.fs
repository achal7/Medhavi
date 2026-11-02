/// CA-P-007 Assess Promise Risk and Resilience — ACL
module Medhavi.Promise.AssessPromiseRisk.PromiseRiskAssessment.ACL

open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model

let toAssessCmd (req: AssessCommitmentRiskReq) : Validation<AssessCommitmentRiskCmd, DomainError> =
    let create assessId commId lineId item time =
        { AssessmentId = assessId
          CommitmentId = commId
          OrderLineId = lineId
          Item = item
          AssessmentTime = time }

    create <!> validatePromiseRiskAssessmentId req.AssessmentId
           <*> validateOrderCommitmentId req.CommitmentId
           <*> validateOrderLineId req.OrderLineId
           <*> validateItemId req.Item
           <*> validateTimestamp req.AssessmentTime

let toEscalateCmd (req: EscalateRiskBreachReq) : Validation<EscalateRiskBreachCmd, DomainError> =
    let create assessId time excId =
        { AssessmentId = assessId
          EscalationReason = req.EscalationReason
          EscalationTime = time
          TargetExceptionId = excId }

    let excIdVal = validatePromiseExceptionId ($"EX#{req.AssessmentId}#{req.EscalationTime.ToUnixTimeSeconds()}")

    create <!> validatePromiseRiskAssessmentId req.AssessmentId
           <*> validateTimestamp req.EscalationTime
           <*> excIdVal

let toArsEventId = function
    | CommitmentRiskAssessed _ -> ArsIdentifiers.EnterpriseEvents.commitmentRiskAssessed.Id
    | CommitmentRiskEscalated _ -> ArsIdentifiers.EnterpriseEvents.commitmentRiskEscalated.Id
    | RiskFlagCleared _ -> ArsIdentifiers.EnterpriseEvents.riskFlagCleared.Id
