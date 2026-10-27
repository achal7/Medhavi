module Medhavi.Nexus.Api.TraceabilityEndpoints

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing
open Microsoft.Extensions.DependencyInjection
open Medhavi.Contracts.Traceability

let mapTraceabilityEndpoints (app: IEndpointRouteBuilder) : IEndpointRouteBuilder =
    app.MapGet(
        "/api/traceability/events",
        Func<HttpContext, System.Threading.Tasks.Task<IResult>>(fun ctx ->
            task {
                let queryService = ctx.RequestServices.GetService<TraceabilityQueryService>()

                if isNull(box queryService) then
                    return Results.StatusCode(503)
                else
                    let toNonEmptyOpt (s: string) = if String.IsNullOrWhiteSpace s then None else Some(s.Trim())

                    let search = toNonEmptyOpt(ctx.Request.Query["search"].ToString())
                    let corrId = toNonEmptyOpt(ctx.Request.Query["correlationId"].ToString())
                    let aggId = toNonEmptyOpt(ctx.Request.Query["aggregateId"].ToString())
                    let bc = toNonEmptyOpt(ctx.Request.Query["context"].ToString())
                    let sev = toNonEmptyOpt(ctx.Request.Query["severity"].ToString())

                    let limit =
                        match Int32.TryParse(ctx.Request.Query["limit"].ToString()) with
                        | true, l when l > 0 -> min 500 l
                        | _ -> 100

                    let skip =
                        match Int32.TryParse(ctx.Request.Query["skip"].ToString()) with
                        | true, s when s >= 0 -> s
                        | _ -> 0

                    let startTime =
                        match DateTimeOffset.TryParse(ctx.Request.Query["startTime"].ToString()) with
                        | true, st -> Some st
                        | _ -> None

                    let endTime =
                        match DateTimeOffset.TryParse(ctx.Request.Query["endTime"].ToString()) with
                        | true, et -> Some et
                        | _ -> None

                    let query: TelemetryFilterQuery =
                        {
                            SearchTerm = search
                            CorrelationId = corrId
                            AggregateId = aggId
                            BoundedContext = bc
                            MinimumSeverity = sev
                            StartTime = startTime
                            EndTime = endTime
                            Limit = limit
                            Skip = skip
                        }

                    let! result = queryService.QueryTelemetry query
                    return Results.Ok result
            })
    )
    |> ignore

    app.MapGet(
        "/api/traceability/dossier/{correlationId}",
        Func<string, HttpContext, System.Threading.Tasks.Task<IResult>>(fun correlationId ctx ->
            task {
                let queryService = ctx.RequestServices.GetService<TraceabilityQueryService>()

                if isNull(box queryService) then
                    return Results.StatusCode(503)
                else
                    let! dossierOpt = queryService.GetTraceDossier correlationId

                    match dossierOpt with
                    | Some dossier -> return Results.Ok dossier
                    | None ->
                        return
                            Results.NotFound
                                {|
                                    message = $"No trace dossier found for CorrelationId: {correlationId}"
                                |}
            })
    )
    |> ignore

    app.MapGet(
        "/api/traceability/transitions",
        Func<HttpContext, System.Threading.Tasks.Task<IResult>>(fun ctx ->
            task {
                let queryService = ctx.RequestServices.GetService<TraceabilityQueryService>()

                if isNull(box queryService) then
                    return Results.StatusCode(503)
                else
                    let! transitions = queryService.GetRecentTransitions()
                    return Results.Ok transitions
            })
    )
    |> ignore

    app
