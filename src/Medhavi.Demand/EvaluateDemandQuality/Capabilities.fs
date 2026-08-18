/// CA-D-007 — Evaluate Demand Quality Parent Capability API
module Medhavi.Demand.EvaluateDemandQuality.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.CommandCapabilities
open Medhavi.SemanticModel
open Medhavi.Contracts
open Medhavi.Contracts.Demand
open Medhavi.SharedKernel.BusinessNotifications
open Medhavi.Demand
open Medhavi.Demand.EvaluateDemandQuality.ForecastQualityAssessment
open Medhavi.Demand.EvaluateDemandQuality.ForecastQualityAssessment.Model

let create (aggregateApi: Capabilities.AggregateApi) (dispatchEnvelope: Envelope -> Task<unit>) : ForecastQualityApi =

    let evaluateQuality (req: EvaluateForecastQualityReq) : Task<Result<ForecastQualityAssessmentDto, ApiError>> =
        aggregateApi.Evaluate req |> TaskResult.map Projections.mapToDto |> TaskResult.mapError mapAppErrorToApiError

    let publishAssessment
        (req: PublishForecastQualityAssessmentReq)
        : Task<Result<ForecastQualityAssessmentDto, ApiError>> =
        taskResult {
            let! domainAss = aggregateApi.Publish req |> TaskResult.mapError mapAppErrorToApiError
            let dto = Projections.mapToDto domainAss

            let publishedVersionOpt = domainAss.Versions |> List.tryFind(fun v -> v.VersionNumber = req.VersionNumber)

            do!
                match publishedVersionOpt with
                | Some publishedVersion when
                    (match publishedVersion.OverallQualityState with
                     | Poor
                     | Critical -> true
                     | _ -> false)
                    || publishedVersion.Metrics.IsOutOfControl
                    ->
                    taskResult {

                        let notif: ForecastQualityAlertNotification =
                            { AssessmentId = dto.AssessmentId
                              Scope = domainAss.Scope
                              EvaluationPeriodStart = domainAss.EvaluationPeriodStart
                              EvaluationPeriodEnd = domainAss.EvaluationPeriodEnd
                              VersionNumber = publishedVersion.VersionNumber
                              QualityState = publishedVersion.OverallQualityState.AsString
                              Wape = publishedVersion.Metrics.Wape
                              ForecastBias = publishedVersion.Metrics.ForecastBias
                              TrackingSignal = publishedVersion.Metrics.TrackingSignal
                              IsOutOfControl = publishedVersion.Metrics.IsOutOfControl
                              AlertRationale = publishedVersion.Rationale
                              Timestamp = domainAss.LastUpdated }

                        do!
                            dispatchNotification
                                dispatchEnvelope
                                "BN-D-021"
                                "CA-D-007"
                                "ForecastQualityAssessment"
                                dto.AssessmentId
                                notif
                    }
                | _ -> TaskResult.return'()

            return dto
        }

    { EvaluateQuality = evaluateQuality
      PublishAssessment = publishAssessment }
