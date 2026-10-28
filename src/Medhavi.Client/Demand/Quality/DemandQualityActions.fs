module Medhavi.Client.Demand.Quality.DemandQualityActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Demand
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ActionExecution
open Medhavi.Client

let private resolveAssessment (ctx: ActionExecutionContext) : Task<ForecastQualityAssessmentDto option> =
    task {
        match ctx.TargetKey with
        | Some key ->
            let! dtoOpt = ctx.Domain.Demand.EvaluateDemandQuality.Queries.GetById key
            return dtoOpt
        | None -> return None
    }

let evaluateQuality: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Demand.EvaluateDemandQuality.Commands
            let! existingOpt = resolveAssessment ctx

            let scopeId, pStart, pEnd, pubIdOpt =
                match existingOpt with
                | Some dto ->
                    dto.ScopeId,
                    dto.EvaluationPeriodStart,
                    dto.EvaluationPeriodEnd,
                    None
                | None ->
                    let sId =
                        match ctx.Payload with
                        | CustomParameters p ->
                            match Map.tryFind "ScopeId" p with
                            | Some (StringValue s) -> s
                            | _ -> "SCOPE-GLOBAL-DEMAND"
                        | _ -> "SCOPE-GLOBAL-DEMAND"

                    let now = DateTimeOffset.UtcNow
                    let pS = DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero).AddMonths(-1)
                    let pE = DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero)
                    sId, pS, pE, None

            let sampleObservations: ObservationPairDto list =
                [ { ItemId = "SKU-DRIVE-500GB"
                    LocationId = "LOC-CENTRAL-WH"
                    Period = pStart
                    SystemForecast = 120.0m
                    FinalForecast = 125.0m
                    ActualDemand = 122.0m }
                  { ItemId = "SKU-DRIVE-1TB"
                    LocationId = "LOC-CENTRAL-WH"
                    Period = pStart
                    SystemForecast = 95.0m
                    FinalForecast = 90.0m
                    ActualDemand = 94.0m } ]

            let req: EvaluateForecastQualityReq =
                { ScopeId = scopeId
                  EvaluationPeriodStart = pStart
                  EvaluationPeriodEnd = pEnd
                  ForecastPublicationId = pubIdOpt
                  Observations = sampleObservations
                  CompletenessScore = 1.0m }

            match! api.EvaluateQuality req with
            | Ok dto ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Forecast quality evaluated for scope '{dto.ScopeId}': Quality State is '{dto.LatestQualityState}' (WAPE: {dto.LatestMetrics.Wape:F1}%%, Accuracy: {dto.LatestMetrics.ForecastAccuracy:F1}%%).",
                            [ DemandQualityGrid.storeId ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let publishAssessment: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Demand.EvaluateDemandQuality.Commands
            let! existingOpt = resolveAssessment ctx

            let scopeId, pStart, pEnd, versionNum =
                match existingOpt with
                | Some dto ->
                    let nextVer =
                        match dto.CurrentPublishedVersion with
                        | Some v -> v + 1
                        | None -> 1
                    dto.ScopeId, dto.EvaluationPeriodStart, dto.EvaluationPeriodEnd, nextVer
                | None ->
                    let sId = ctx.TargetKey |> Option.defaultValue "SCOPE-GLOBAL-DEMAND"
                    let now = DateTimeOffset.UtcNow
                    let pS = DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero).AddMonths(-1)
                    let pE = DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero)
                    sId, pS, pE, 1

            let req: PublishForecastQualityAssessmentReq =
                { ScopeId = scopeId
                  EvaluationPeriodStart = pStart
                  EvaluationPeriodEnd = pEnd
                  VersionNumber = versionNum }

            match! api.PublishAssessment req with
            | Ok dto ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Forecast quality assessment for scope '{dto.ScopeId}' published at version {versionNum}.",
                            [ DemandQualityGrid.storeId ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let all: (ActionDefId * ActionHandler) list =
    [ DemandQualityGrid.DemandQualityActions.evaluateQuality.Id, evaluateQuality
      DemandQualityGrid.DemandQualityActions.publishAssessment.Id, publishAssessment ]
