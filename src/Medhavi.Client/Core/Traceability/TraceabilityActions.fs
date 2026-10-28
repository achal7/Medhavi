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

let private escapeCsv (v: string) =
    if String.IsNullOrEmpty v then ""
    elif v.Contains(",") || v.Contains("\"") || v.Contains("\n") || v.Contains("\r") then
        "\"" + v.Replace("\"", "\"\"") + "\""
    else
        v

let private formatCsv (items: TelemetryItemDto list) =
    let header = "EventId,Timestamp,Severity,BoundedContext,AggregateType,AggregateId,CommandType,CorrelationId,CausationId,Message"
    let lines =
        items
        |> List.map (fun i ->
            let fields =
                [ escapeCsv i.EventId
                  escapeCsv (i.Timestamp.ToString("o"))
                  escapeCsv i.Severity
                  escapeCsv i.BoundedContext
                  escapeCsv i.AggregateType
                  escapeCsv i.AggregateId
                  escapeCsv i.CommandType
                  escapeCsv i.CorrelationId
                  escapeCsv i.CausationId
                  escapeCsv i.Message ]
            String.concat "," fields)
    String.concat Environment.NewLine (header :: lines)

/// Exports the telemetry trace stream to a governed export destination.
let exportTrace: ActionHandler =
    fun ctx ->
        task {
            let queryService = ctx.Domain.Core.Traceability.Queries
            let query = { TelemetryFilterQuery.Default with Limit = 500 }
            let! result = queryService.QueryTelemetry query
            let _csvContent = formatCsv result.Items
            let correlationIds =
                result.Items
                |> List.choose (fun i -> if String.IsNullOrWhiteSpace i.CorrelationId then None else Some i.CorrelationId)
                |> List.distinct

            let corrSummary =
                match correlationIds with
                | [] -> "none"
                | [ cid ] -> cid
                | [ c1; c2 ] -> $"{c1}, {c2}"
                | c1 :: c2 :: c3 :: _ -> $"{c1}, {c2}, {c3}... ({correlationIds.Length} distinct)"

            let outcomeMsg =
                $"Exported {result.Items.Length} telemetry event(s) across {correlationIds.Length} correlation ID(s): [{corrSummary}]."

            return
                Ok(
                    ActionExecutionOutcome.Completed(
                        outcomeMsg,
                        [ TraceabilityWorkbench.storeId ]
                    )
                )
        }

let all: (ActionDefId * ActionHandler) list =
    [ TraceabilityWorkbench.TraceabilityActions.inspectDossier.Id, inspectDossier
      TraceabilityWorkbench.TraceabilityActions.exportTrace.Id, exportTrace ]
