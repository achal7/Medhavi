module Medhavi.Client.Demand.Forecast.ForecastActions

open System
open Medhavi.Contracts.Demand
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ActionExecution
open Medhavi.Client

let publishForecast: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Demand.ForecastDemand.Commands
            match ctx.TargetKey with
            | Some pubId ->
                let req: PublishForecastPublicationReq = { PublicationId = pubId }

                match! api.Publish req with
                | Ok dto ->
                    return
                        Ok(
                            ActionExecutionOutcome.Completed(
                                $"Forecast publication '{dto.PublicationId}' (v{dto.VersionNumber}) successfully published to planning horizon.",
                                [ ForecastGrid.storeId ]
                            )
                        )
                | Error err -> return Error err.Message
            | None -> return Error "No forecast publication selected to publish."
        }

let selectChampionModel: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Demand.ForecastDemand.Commands
            match ctx.TargetKey with
            | Some pubId ->
                let modelId =
                    match ctx.Payload with
                    | CustomParameters p ->
                        match Map.tryFind "ChampionModelId" p with
                        | Some (StringValue m) when not (String.IsNullOrWhiteSpace m) -> m
                        | _ -> "Ensemble-AutoARIMA-Croston"
                    | ReasonedAction(code, _) when not (String.IsNullOrWhiteSpace code) -> code
                    | _ -> "Ensemble-AutoARIMA-Croston"

                let req: SelectChampionModelReq =
                    { PublicationId = pubId
                      ChampionModelId = modelId }

                match! api.SelectChampionModel req with
                | Ok dto ->
                    return
                        Ok(
                            ActionExecutionOutcome.Completed(
                                $"Champion model '{dto.ChampionModelId}' selected for forecast publication '{dto.PublicationId}'.",
                                [ ForecastGrid.storeId ]
                            )
                        )
                | Error err -> return Error err.Message
            | None -> return Error "No forecast publication selected to choose champion model."
        }

let all: (ActionDefId * ActionHandler) list =
    [ ForecastGrid.ForecastActions.publishForecast.Id, publishForecast
      ForecastGrid.ForecastActions.selectChampionModel.Id, selectChampionModel ]
