module Medhavi.Client.Demand.Exceptions.DemandExceptionActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Demand
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ActionExecution
open Medhavi.Client

let private resolveEvidence (ctx: ActionExecutionContext) : Task<DemandExceptionEvidenceDto option> =
    task {
        match ctx.TargetKey with
        | Some key ->
            let! dtoOpt = ctx.Domain.Demand.DetectDemandExceptions.Queries.GetById key
            return dtoOpt
        | None -> return None
    }

let evaluateException: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Demand.DetectDemandExceptions.Commands
            let! dtoOpt = resolveEvidence ctx

            let req =
                match dtoOpt with
                | Some dto ->
                    { ScopeId = dto.ScopeId
                      EntityType = dto.EntityType
                      EntityId = dto.EntityId
                      ExceptionType = dto.ExceptionType
                      TriggeringMetric = dto.TriggeringMetric
                      MetricValue = dto.MetricValue
                      HistoricalValues = None }
                | None ->
                    let scope =
                        ctx.PlanningContext.ScopeId
                        |> Option.defaultValue "PS-GLOBAL-SUPPLY-CHAIN"

                    let entityId = ctx.TargetKey |> Option.defaultValue "SKU-DRIVE-500GB"

                    let exType =
                        match ctx.Payload with
                        | CustomParameters p ->
                            match Map.tryFind "ExceptionType" p with
                            | Some (StringValue t) -> t
                            | _ -> "ForecastBiasExceeded"
                        | _ -> "ForecastBiasExceeded"

                    let metric =
                        match ctx.Payload with
                        | CustomParameters p ->
                            match Map.tryFind "TriggeringMetric" p with
                            | Some (StringValue m) -> m
                            | _ -> "TrackingSignal"
                        | _ -> "TrackingSignal"

                    let metricVal =
                        match ctx.Payload with
                        | CustomParameters p ->
                            match Map.tryFind "MetricValue" p with
                            | Some (DecimalValue v) -> v
                            | Some (IntValue i) -> decimal i
                            | _ -> 4.5m
                        | _ -> 4.5m

                    { ScopeId = scope
                      EntityType = "ItemLocation"
                      EntityId = entityId
                      ExceptionType = exType
                      TriggeringMetric = metric
                      MetricValue = metricVal
                      HistoricalValues = None }

            match! api.EvaluateException req with
            | Ok (Some evidence) ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Demand exception evaluated: {evidence.ExceptionType} on {evidence.EntityId} ({evidence.Severity}). {evidence.Rationale}",
                            [ DemandExceptionGrid.storeId; DataStoreId.literal "core-exception-store" ]
                        )
                    )
            | Ok None ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Demand exception evaluated for {req.EntityId}: No active violation detected under current threshold policies.",
                            [ DemandExceptionGrid.storeId ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let all: (ActionDefId * ActionHandler) list =
    [ DemandExceptionGrid.DemandExceptionActions.evaluateException.Id, evaluateException ]
