module Medhavi.Nexus.Api.HealthEndpoints

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing
open Microsoft.Extensions.DependencyInjection
open Medhavi.Nexus.Composition.ComposeNexus

let mapHealthEndpoints (app: IEndpointRouteBuilder) : IEndpointRouteBuilder =
    app.MapGet(
        "/health",
        Func<HttpContext, IResult>(fun ctx ->
            let nexus = ctx.RequestServices.GetService<NexusContext>()

            match box nexus with
            | null -> Results.StatusCode(503)
            | _ ->
                Results.Ok
                    {| status = "healthy"
                       timestamp = DateTimeOffset.UtcNow |})
    )
    |> ignore

    app.MapGet(
        "/ready",
        Func<HttpContext, IResult>(fun ctx ->
            let nexus = ctx.RequestServices.GetService<NexusContext>()

            match box nexus with
            | null -> Results.StatusCode(503)
            | _ ->
                let isReady = not(isNull (box nexus.Core)) && not(isNull (box nexus.Demand)) && not(isNull (box nexus.Store))

                if isReady then
                    Results.Ok
                        {| status = "ready"
                           timestamp = DateTimeOffset.UtcNow
                           components =
                            {| core = "healthy"
                               demand = "healthy"
                               store = "healthy" |} |}
                else
                    Results.StatusCode(503))
    )
    |> ignore

    app
