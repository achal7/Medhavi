module Medhavi.Client.Core.Traceability.TraceabilityFeature

open System
open Medhavi.Contracts.Traceability
open Medhavi.Client
open Medhavi.Client.Core.Traceability

let store =
    DataStore.create TraceabilityWorkbench.storeId (fun ctx ->
        async {
            let queryService = ctx.Domain.Core.Traceability.Queries

            let query =
                { TelemetryFilterQuery.Default with
                    Limit = 100
                }

            let! result = queryService.QueryTelemetry query |> Async.AwaitTask

            let rows =
                result.Items
                |> List.map(fun item ->
                    ({
                        EventId = item.EventId
                        Timestamp = item.Timestamp
                        Severity = item.Severity
                        BoundedContext = item.BoundedContext
                        AggregateType = item.AggregateType
                        AggregateId = item.AggregateId
                        CommandType = item.CommandType
                        Message = item.Message
                        CorrelationId = item.CorrelationId
                        CausationId = item.CausationId
                        HasDecision =
                            not(String.IsNullOrWhiteSpace item.Message && String.IsNullOrWhiteSpace item.CommandType)
                        HasErrors =
                            item.Severity.Equals("Error", StringComparison.OrdinalIgnoreCase)
                            || item.Severity.Equals("Errors", StringComparison.OrdinalIgnoreCase)
                    }
                    : TraceabilityWorkbench.TraceabilityRow))
                |> List.map TraceabilityWorkbench.adapter.ToUniversal

            return rows
        })

/// Feature composition manifest for the Traceability bounded context slice.
let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = TraceabilityActions.all
        Components = [ TraceabilityWorkbench.registration ]
    }
