module Medhavi.Demand.DemandBehaviourAssessment.ACL

open Medhavi.Common.Validation
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Failure
open Medhavi.Contracts.Demand.SenseDemand
open Medhavi.Demand
open Medhavi.Demand.DemandBehaviourAssessment.Model

let private notEmpty field =
    validate (fun s -> not (System.String.IsNullOrWhiteSpace s)) (DomainError.validation(field, $"{field} cannot be empty"))

let toEvaluateSignalCmd
    (req: EvaluateDemandSignalReq)
    (isHighPriority: bool)
    : Validation<EvaluateSignalCmd, DomainError> =
    let signal =
        { SignalId = req.SignalId
          Source = req.Source
          SourceReliability = req.SourceReliability
          Timestamp = req.Timestamp
          Value = req.Value
          StatisticalBound = req.StatisticalBound
          RecentBaseline = req.RecentBaseline }

    let make _ _ sku sp =
        { Signal = signal
          SkuId = sku
          StockingPointId = sp
          IsHighPriority = isHighPriority }

    make
    <!> notEmpty "SignalId" req.SignalId
    <*> notEmpty "Source" req.Source
    <*> (SkuId.create req.SkuId |> fromResult)
    <*> (StockingPointId.create req.StockingPointId |> fromResult)

let toAcknowledgeCmd (req: AcknowledgeAssessmentReq) : Validation<AcknowledgeCmd, DomainError> =
    let make planner justification sku sp =
        { SkuId = sku
          StockingPointId = sp
          PlannerIdentity = planner
          Justification = justification }

    make
    <!> notEmpty "PlannerIdentity" req.PlannerIdentity
    <*> notEmpty "Justification" req.Justification
    <*> (SkuId.create req.SkuId |> fromResult)
    <*> (StockingPointId.create req.StockingPointId |> fromResult)
