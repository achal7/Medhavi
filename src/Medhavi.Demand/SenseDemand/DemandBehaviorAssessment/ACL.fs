/// Sense Demand Anti-Corruption Layer (ACL)
/// Applicative Validation for incoming Requests to Domain Commands
module Medhavi.Demand.SenseDemand.DemandBehaviorAssessment.ACL

open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Validations
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Model

/// Translates InitializeBaselineReq into InitializeBaselineCmd using Applicative validation.
let toInitializeBaselineCmd (req: InitializeBaselineReq) : Validation<InitializeBaselineCmd, DomainError> =
    let create item loc mean stdDev =
        let assessmentId = DemandBehaviorAssessmentId.ofItemAndLocation item loc

        { AssessmentId = assessmentId
          Item = item
          Location = loc
          BaselineMean = mean
          BaselineStdDev = stdDev }

    create <!> validateItemId req.Item
    <*> validateLocationId req.Location
    <*> nonNegative "BaselineStdDev" req.BaselineStdDev
    <*> positive "BaselineMean" req.BaselineMean

/// Translates EvaluateDemandSignalReq into EvaluateSignalCmd using Applicative validation.
let toEvaluateSignalCmd (req: EvaluateDemandSignalReq) : Validation<EvaluateSignalCmd, DomainError> =
    let create item loc qty ts =
        let assessmentId = DemandBehaviorAssessmentId.ofItemAndLocation item loc

        { AssessmentId = assessmentId
          Item = item
          Location = loc
          Quantity = qty
          SignalTimestamp = ts
          CorroboratingSources = req.CorroboratingSources
          IsHighPriority = req.IsHighPriority }

    create <!> validateItemId req.Item
    <*> validateLocationId req.Location
    <*> validateQty req.Quantity
    <*> validateTimestamp req.SignalTimestamp

/// Translates EvaluateForecastRefreshReq into EvaluateForecastRefreshCmd using Applicative validation.
let toEvaluateForecastRefreshCmd
    (req: EvaluateForecastRefreshReq)
    : Validation<EvaluateForecastRefreshCmd, DomainError> =

    let create item loc age wape =
        let assessmentId = DemandBehaviorAssessmentId.ofItemAndLocation item loc

        { AssessmentId = assessmentId
          Item = item
          Location = loc
          ForecastAgeHours = age
          ExpectedAccuracyImprovementWape = wape }

    create <!> validateItemId req.Item
    <*> validateLocationId req.Location
    <*> nonNegative "ForecastAgeHours" req.ForecastAgeHours
    <*> nonNegative "ExpectedAccuracyImprovementWape" req.ExpectedAccuracyImprovementWape
