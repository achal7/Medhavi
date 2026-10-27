module Medhavi.Client.Demand.Observations.DemandObservationActions

open System
open Medhavi.Contracts.Demand
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ActionExecution
open Medhavi.Client

/// Authoritative Application Command Handlers for Demand Observations.
let confirmObservation: ActionHandler =
    fun ctx ->
        task {
            let apiOpt =
                match ctx.Resolve<DemandObservationApi>() with
                | Some api -> Some api
                | None ->
                    ctx.Resolve<DomainServices>()
                    |> Option.map (fun ds -> ds.Demand.UnderstandDemand.Observation.Commands)

            match apiOpt with
            | Some api ->
                match ctx.TargetKey with
                | Some obsId ->
                    let req: EvaluateObservationReq =
                        { ObservationId = obsId
                          EvaluationTime = DateTimeOffset.UtcNow }

                    match! api.Evaluate req with
                    | Ok dto ->
                        return
                            Ok(
                                ActionExecutionOutcome.Completed(
                                    $"Demand observation '{dto.ObservationId}' confirmed and evaluated into enterprise sensory stream.",
                                    [ DemandObservationStore.storeId; DataStoreId.literal "demo-demand-store" ]
                                )
                            )
                    | Error err -> return Error err.Message
                | None -> return Error "No demand observation selected to confirm."
            | None -> return Error "DemandObservationApi capability is not connected."
        }

let dismissNoise: ActionHandler =
    fun ctx ->
        task {
            let apiOpt =
                match ctx.Resolve<DemandObservationApi>() with
                | Some api -> Some api
                | None ->
                    ctx.Resolve<DomainServices>()
                    |> Option.map (fun ds -> ds.Demand.UnderstandDemand.Observation.Commands)

            match apiOpt with
            | Some api ->
                match ctx.TargetKey with
                | Some obsId ->
                    let req: EvaluateObservationReq =
                        { ObservationId = obsId
                          EvaluationTime = DateTimeOffset.UtcNow }

                    match! api.Evaluate req with
                    | Ok dto ->
                        return
                            Ok(
                                ActionExecutionOutcome.Completed(
                                    $"Demand observation '{dto.ObservationId}' evaluated and flagged as noise.",
                                    [ DemandObservationStore.storeId ]
                                )
                            )
                    | Error err -> return Error err.Message
                | None -> return Error "No demand observation selected to dismiss."
            | None -> return Error "DemandObservationApi capability is not connected."
        }

let classifyPattern: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some obsId ->
                return Ok(ActionExecutionOutcome.ExplainRequested(obsId, "DemandObservationPattern"))
            | None ->
                return Error "No demand observation selected for pattern classification."
        }

let all: (ActionDefId * ActionHandler) list =
    [ DemandObservationGridSchema.DemandObservationActions.confirmObservation.Id, confirmObservation
      DemandObservationGridSchema.DemandObservationActions.dismissNoise.Id, dismissNoise
      DemandObservationGridSchema.DemandObservationActions.classifyPattern.Id, classifyPattern ]
