module Medhavi.Client.Core.Traceability.TraceabilityStore

open System
open Medhavi.Contracts.Traceability
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client
open Medhavi.Client.Core.Traceability.TraceabilityGridSchema

/// Authoritative DataStore for Traceability telemetry stream.
let storeId: DataStoreId = DataStoreId.literal "traceability-store"

let store: DataStore =
    {
        Id = storeId
        Fetch =
            fun (ctx: ApplicationContext) ->
                async {
                    match ctx.Resolve<TraceabilityQueryService>() with
                    | Some queryService ->
                        let query =
                            { TelemetryFilterQuery.Default with
                                Limit = 100
                            }

                        let! result = queryService.QueryTelemetry query |> Async.AwaitTask

                        let rows =
                            result.Items
                            |> List.map(fun item ->
                                {
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
                                        not(
                                            String.IsNullOrWhiteSpace item.Message
                                            && String.IsNullOrWhiteSpace item.CommandType
                                        )
                                    HasErrors =
                                        item.Severity.Equals("Error", StringComparison.OrdinalIgnoreCase)
                                        || item.Severity.Equals("Errors", StringComparison.OrdinalIgnoreCase)
                                })
                            |> List.map TraceabilityGridSchema.adapter.ToUniversal

                        return Tabular rows
                    | None -> return Tabular []
                }
    }
