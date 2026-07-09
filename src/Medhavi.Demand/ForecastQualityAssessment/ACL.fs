module Medhavi.Demand.ForecastQualityAssessment.ACL

open Medhavi.Common.Validation
open Medhavi.Contracts.Demand.ForecastQualityAssessment
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Failure
open Medhavi.Demand

type ValidatedRequest =
    { PlanningScopeId: PlanningScopeId
      EvaluationPeriodStart: Timestamp
      EvaluationPeriodEnd: Timestamp }

let validateRequest (req: EvaluateForecastQualityReq) : Validation<ValidatedRequest, DomainError> =
    let make scopeId _ =
        { PlanningScopeId = scopeId
          EvaluationPeriodStart = Timestamp.create req.EvaluationPeriodStart
          EvaluationPeriodEnd = Timestamp.create req.EvaluationPeriodEnd }

    let scopeIdVal = PlanningScopeId.fromString req.PlanningScopeId |> fromResult
    let periodVal =
        validate
            (fun _ -> req.EvaluationPeriodStart < req.EvaluationPeriodEnd)
            (DomainError.validation("EvaluationPeriod", "EvaluationPeriodStart must be before EvaluationPeriodEnd"))
            ()

    make <!> scopeIdVal <*> periodVal
