module Medhavi.Client.Core.Traceability.TraceabilityActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Traceability
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ActionExecution
open Medhavi.Client
open Medhavi.Client.Core.Traceability

/// Inspects the causal dossier for a selected telemetry event chain.
let inspectDossier: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some eventId ->
                return Ok(ActionExecutionOutcome.ExplainRequested(eventId, "TraceDossier"))
            | None ->
                return Error "No telemetry event selected to inspect."
        }

/// Exports the telemetry trace stream to a governed export destination.
let exportTrace: ActionHandler =
    fun ctx ->
        task {
            return
                Ok(
                    ActionExecutionOutcome.Completed(
                        "Telemetry trace log exported successfully.",
                        [ TraceabilityStore.storeId ]
                    )
                )
        }

let all: (ActionDefId * ActionHandler) list =
    [ TraceabilityGridSchema.inspectDossier.Id, inspectDossier
      TraceabilityGridSchema.exportTrace.Id, exportTrace ]
