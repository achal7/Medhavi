open System
open Bolero.Server
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Medhavi.Web
open Medhavi.Nexus

[<EntryPoint>]
let main args =

    let builder = WebApplication.CreateBuilder(args)
    builder.WebHost.UseStaticWebAssets() |> ignore

    // Configure fast shutdown for local development
    builder.Services.Configure<HostOptions>(fun (options: HostOptions) ->
        options.ShutdownTimeout <- TimeSpan.FromSeconds(2.0))
    |> ignore

    builder.Services
        .AddRazorComponents()
        .AddInteractiveServerComponents()
    |> ignore

    builder.Services.AddBoleroComponents() |> ignore

    // Initialize and register MedhaviEngine singleton
    let engine = MedhaviEngine()
    engine.Initialize().Wait()

    builder.Services.AddSingleton<MedhaviEngine>(engine)
    |> ignore

    // Register Query Stores & Services
    Services.ServiceRegistration.configureServices builder

    let app = builder.Build()

    app.UseStaticFiles().UseRouting().UseAntiforgery()
    |> ignore

    app
        .MapRazorComponents<Medhavi.Web.SystemPage>()
        .AddInteractiveServerRenderMode()
    |> ignore

    app.MapPost("/api/auth/login", System.Func<Medhavi.Web.Services.LoginRequest, Microsoft.AspNetCore.Http.IResult>(fun (req: Medhavi.Web.Services.LoginRequest) ->
        if String.IsNullOrWhiteSpace(req.Username) then
            Microsoft.AspNetCore.Http.Results.BadRequest("Username cannot be empty")
        elif String.IsNullOrWhiteSpace(req.Password) then
            Microsoft.AspNetCore.Http.Results.BadRequest("Password cannot be empty")
        else
            let email = sprintf "%s@medhavi.com" (req.Username.ToLower().Replace(" ", ""))
            let resp = {| Username = req.Username; Email = email; Role = "Supervisor" |}
            Microsoft.AspNetCore.Http.Results.Ok(resp)
    ))
    |> ignore

    app.Run()

    0
