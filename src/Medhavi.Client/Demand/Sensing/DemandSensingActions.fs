module Medhavi.Client.Demand.Sensing.DemandSensingActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Demand
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ActionExecution
open Medhavi.Client

let private resolveItemLocation (ctx: ActionExecutionContext) : Task<(string * string) option> =
    task {
        match ctx.TargetKey with
        | Some key ->
            if key.Contains("@") then
                let parts = key.Split('@')
                return Some (parts.[0], parts.[1])
            elif key.Contains("#") then
                let parts = key.Split('#')
                return Some (parts.[0], parts.[1])
            else
                let! dtoOpt = ctx.Domain.Demand.SenseDemand.Queries.GetById key
                match dtoOpt with
                | Some dto -> return Some (dto.Item, dto.Location)
                | None -> return None
        | None ->
            // Try extracting from CustomParameters
            match ctx.Payload with
            | CustomParameters p ->
                match Map.tryFind "Item" p, Map.tryFind "Location" p with
                | Some (StringValue item), Some (StringValue loc) -> return Some (item, loc)
                | _ -> return None
            | _ -> return None
    }

let evaluateSignal: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Demand.SenseDemand.Commands
            let! itemLocOpt = resolveItemLocation ctx
            match itemLocOpt with
            | Some (item, loc) ->
                let qty =
                    match ctx.Payload with
                    | QuantityValue q -> q
                    | CustomParameters p ->
                        match Map.tryFind "Quantity" p with
                        | Some (DecimalValue d) -> d
                        | Some (IntValue i) -> decimal i
                        | _ -> 100.0m
                    | _ -> 100.0m

                let req: EvaluateDemandSignalReq =
                    { Item = item
                      Location = loc
                      Quantity = qty
                      SignalTimestamp = DateTimeOffset.UtcNow
                      CorroboratingSources = [ "SensingStudio-Interactive" ]
                      IsHighPriority = false }

                match! api.EvaluateSignal req with
                | Ok dto ->
                    return
                        Ok(
                            ActionExecutionOutcome.Completed(
                                $"Demand signal evaluated for {dto.Item} @ {dto.Location}. State: {dto.CurrentState}, Confidence: {dto.AssessmentConfidence}.",
                                [ DemandSensingGrid.storeId ]
                            )
                        )
                | Error err -> return Error err.Message
            | None -> return Error "No item and location identified for demand signal evaluation."
        }

let evaluateRefresh: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Demand.SenseDemand.Commands
            let! itemLocOpt = resolveItemLocation ctx
            match itemLocOpt with
            | Some (item, loc) ->
                let ageHours =
                    match ctx.Payload with
                    | CustomParameters p ->
                        match Map.tryFind "ForecastAgeHours" p with
                        | Some (IntValue h) -> h
                        | _ -> 48
                    | _ -> 48

                let wapeGain =
                    match ctx.Payload with
                    | CustomParameters p ->
                        match Map.tryFind "ExpectedAccuracyImprovementWape" p with
                        | Some (DecimalValue w) -> w
                        | _ -> 0.08m
                    | _ -> 0.08m

                let req: EvaluateForecastRefreshReq =
                    { Item = item
                      Location = loc
                      ForecastAgeHours = ageHours
                      ExpectedAccuracyImprovementWape = wapeGain }

                match! api.EvaluateForecastRefresh req with
                | Ok decision ->
                    return
                        Ok(
                            ActionExecutionOutcome.Completed(
                                $"Forecast refresh evaluation for {item} @ {loc}: {decision.SelectedAlternative}. {decision.Rationale}",
                                [ DemandSensingGrid.storeId ]
                            )
                        )
                | Error err -> return Error err.Message
            | None -> return Error "No item and location identified for forecast refresh evaluation."
        }

let all: (ActionDefId * ActionHandler) list =
    [ DemandSensingGrid.DemandSensingActions.evaluateSignal.Id, evaluateSignal
      DemandSensingGrid.DemandSensingActions.evaluateRefresh.Id, evaluateRefresh ]
