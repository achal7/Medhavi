/// Anti-Corruption Layer (ACL) for Forecast Quality Assessment
module Medhavi.Demand.EvaluateDemandQuality.ForecastQualityAssessment.ACL

open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Model

let private toObservationComparison (dto: ObservationPairDto) : Result<ObservationComparison, DomainError> =
    result {
        let! itemId = ItemId.create dto.ItemId |> Result.mapError(fun e -> DomainError.validation $"{e}")
        let! locationId = LocationId.create dto.LocationId |> Result.mapError(fun e -> DomainError.validation $"{e}")
        let! period = Timestamp.create dto.Period |> Result.mapError DomainError.validation

        return
            { ItemId = itemId
              LocationId = locationId
              Period = period
              SystemForecast = dto.SystemForecast
              FinalForecast = dto.FinalForecast
              ActualDemand = dto.ActualDemand }
    }

/// Translates EvaluateForecastQualityReq into EvaluateForecastQualityCmd
let toEvaluateCmd (req: EvaluateForecastQualityReq) : Validation<EvaluateForecastQualityCmd, DomainError> =
    let create scope start end' pubIdOpt obs =
        let id = ForecastQualityAssessmentId.ofComponents scope start end'

        { AssessmentId = id
          Scope = scope
          EvaluationPeriodStart = start
          EvaluationPeriodEnd = end'
          ForecastPublicationId = pubIdOpt
          Observations = obs
          CompletenessScore = req.CompletenessScore
          EvaluationTime = Timestamp.now() }

    let parsedObsResult =
        req.Observations
        |> List.map toObservationComparison
        |> List.fold
            (fun acc res ->
                match acc, res with
                | Ok list, Ok item -> Ok(item :: list)
                | Error e, _ -> Error e
                | _, Error e -> Error e)
            (Ok [])
        |> Result.map List.rev

    let pubIdValidation =
        match req.ForecastPublicationId with
        | Some idStr -> ForecastPublicationId.create idStr |> Result.map Some |> fromResult
        | None -> Ok None |> fromResult

    create <!> validatePlanningScopeId req.ScopeId
    <*> validateTimestamp req.EvaluationPeriodStart
    <*> validateTimestamp req.EvaluationPeriodEnd
    <*> pubIdValidation
    <*> (parsedObsResult |> fromResult)

/// Translates PublishForecastQualityAssessmentReq into PublishForecastQualityAssessmentCmd
let toPublishCmd
    (req: PublishForecastQualityAssessmentReq)
    : Validation<PublishForecastQualityAssessmentCmd, DomainError> =
    let create scope start end' =
        let id = ForecastQualityAssessmentId.ofComponents scope start end'

        { AssessmentId = id
          Scope = scope
          EvaluationPeriodStart = start
          EvaluationPeriodEnd = end'
          VersionNumber = req.VersionNumber
          PublicationTime = Timestamp.now() }

    create <!> validatePlanningScopeId req.ScopeId
    <*> validateTimestamp req.EvaluationPeriodStart
    <*> validateTimestamp req.EvaluationPeriodEnd
