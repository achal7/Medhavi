module Medhavi.Demand.DemandPlanningCondition.ACL

open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.Contracts.Demand.DemandLearning
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Failure
open Medhavi.Demand
open Medhavi.Demand.DemandPlanningCondition.Model

let private notEmpty field =
    validate (fun s -> not (System.String.IsNullOrWhiteSpace s)) (DomainError.validation(field, $"{field} cannot be empty"))

let toRecognizeCmd (req: RecognizeDemandPlanningConditionReq) : Validation<RecognizeConditionCmd, DomainError> =
    let make condId entity typeStr evidence policy =
        { ConditionId         = condId
          PlanningEntity      = entity
          ConditionType       = typeStr
          NewSeverity         = ConditionSeverity.Critical
          DetectionEvidence   = evidence
          DetectionTimestamp  = Timestamp.create req.DetectionTimestamp
          PolicyVersionRef    = policy
          BusinessTime        = Timestamp.create req.BusinessTime }

    make <!> (DemandPlanningConditionId.create req.ConditionId |> fromResult)
    <*> notEmpty "PlanningEntity" req.PlanningEntity
    <*> notEmpty "ConditionType" req.ConditionType
    <*> notEmpty "DetectionEvidence" req.DetectionEvidence
    <*> notEmpty "PolicyVersionRef" req.PolicyVersionRef

let toResolveCmd (req: ResolveDemandPlanningConditionReq) : Validation<ResolveConditionCmd, DomainError> =
    let make condId evidence =
        { ConditionId         = condId
          ResolutionEvidence  = evidence
          ResolutionTimestamp = Timestamp.create req.ResolutionTimestamp }

    make <!> (DemandPlanningConditionId.create req.ConditionId |> fromResult)
    <*> notEmpty "ResolutionEvidence" req.ResolutionEvidence
