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
            let api = ctx.Domain.Demand.UnderstandDemand.Observation.Commands
            match ctx.TargetKey with
            | Some obsId ->
                let req: EvaluateObservationReq =
                    {
                        ObservationId = obsId
                        EvaluationTime = DateTimeOffset.UtcNow
                    }

                match! api.Evaluate req with
                | Ok dto ->
                    return
                        Ok(
                            ActionExecutionOutcome.Completed(
                                $"Demand observation '{dto.ObservationId}' evaluated against acceptance policy (Lifecycle: {dto.LifecycleState}).",
                                [ DemandObservationGrid.storeId; DataStoreId.literal "demo-demand-store" ]
                            )
                        )
                | Error err -> return Error err.Message
            | None -> return Error "No demand observation selected to confirm."
        }

let dismissNoise: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Demand.UnderstandDemand.Observation.Commands
            match ctx.TargetKey with
            | Some obsId ->
                let req: EvaluateObservationReq =
                    {
                        ObservationId = obsId
                        EvaluationTime = DateTimeOffset.UtcNow
                    }

                match! api.Evaluate req with
                | Ok dto ->
                    let outcomeSummary =
                        match dto.LifecycleState.ToLowerInvariant() with
                        | "rejected" ->
                            $"Observation '{dto.ObservationId}' evaluated and rejected by backend acceptance policy."
                        | "quarantined" ->
                            $"Observation '{dto.ObservationId}' evaluated and quarantined for manual review by backend acceptance policy."
                        | "accepted" ->
                            $"Observation '{dto.ObservationId}' evaluated as valid (Accepted) by backend policy; not rejected as noise."
                        | state ->
                            $"Observation '{dto.ObservationId}' evaluated; backend lifecycle state is '{state}'."

                    return Ok(ActionExecutionOutcome.Completed(outcomeSummary, [ DemandObservationGrid.storeId ]))
                | Error err -> return Error err.Message
            | None -> return Error "No demand observation selected to dismiss."
        }

let classifyPattern: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some obsId -> return Ok(ActionExecutionOutcome.ExplainRequested(obsId, "DemandObservationPattern"))
            | None -> return Error "No demand observation selected for pattern classification."
        }

let all: (ActionDefId * ActionHandler) list =
    [
        DemandObservationGrid.DemandObservationActions.confirmObservation.Id, confirmObservation
        DemandObservationGrid.DemandObservationActions.dismissNoise.Id, dismissNoise
        DemandObservationGrid.DemandObservationActions.classifyPattern.Id, classifyPattern
    ]
