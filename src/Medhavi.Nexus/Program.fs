module Medhavi.Nexus.Program

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Radzen
open Bolero.Server
open Medhavi.Client
open Medhavi.Nexus.Configuration
open Medhavi.Nexus.Composition.ComposeNexus
open Medhavi.Nexus.Api

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)

    let applicationConfiguration =
        match loadClientConfiguration builder.Environment.EnvironmentName builder.Configuration with
        | Ok configuration -> configuration
        | Error failures ->
            failures
            |> List.map string
            |> String.concat "; "
            |> fun message -> invalidOp $"Application configuration is invalid: {message}"

    // Ensure static web assets (Blazor framework & Radzen components) are always loaded
    builder.WebHost.UseStaticWebAssets() |> ignore

    // 1. Load configuration
    let nexusConfig = NexusConfig.load builder.Configuration

    // 2. Configure Logging
    builder.Logging.SetMinimumLevel(
        match nexusConfig.Server.LogMinimumLevel with
        | "Debug" -> LogLevel.Debug
        | "Warning" -> LogLevel.Warning
        | "Error" -> LogLevel.Error
        | _ -> LogLevel.Information
    )
    |> ignore

    // 3. Configure Kestrel Host URLs
    builder.WebHost.UseUrls($"http://0.0.0.0:{nexusConfig.Server.Port}") |> ignore

    // 4. Add Blazor Interactive Server & Bolero Services
    builder.Services.AddRazorComponents().AddInteractiveServerComponents() |> ignore
    builder.Services.AddBoleroComponents() |> ignore

    // 5. Add Radzen Services
    builder.Services.AddRadzenComponents() |> ignore
    builder.Services.AddScoped<ThemeService>() |> ignore
    builder.Services.AddScoped<DialogService>() |> ignore
    builder.Services.AddScoped<NotificationService>() |> ignore
    builder.Services.AddScoped<TooltipService>() |> ignore
    builder.Services.AddScoped<ContextMenuService>() |> ignore

    // 6. Compose Domain Contexts (synchronous initialization at composition root)
    let composeTask = composeAll builder.Services

    let nexusCtx: NexusContext =
        match composeTask.GetAwaiter().GetResult() with
        | Ok ctx -> ctx
        | Error err -> failwithf $"Failed to initialize Medhavi domain contexts: %A{err}"

    builder.Services.AddSingleton<NexusContext>(nexusCtx) |> ignore

    // 7. Initialize Client
    Composition.ComposeClient.addApplicationServices applicationConfiguration nexusCtx builder.Services |> ignore

    // 8. Build Web Application
    let app = builder.Build()

    // 9. Startup Data Seeding (feed data to domain layer upon boot)
    Composition.StartupData.seed nexusCtx
    |> Async.AwaitTask
    |> Async.RunSynchronously

    // 9. Configure HTTP Middleware Pipeline
    // if app.Environment.IsDevelopment() then
    //     app.UseDeveloperExceptionPage() |> ignore

    app.UseHttpsRedirection() |> ignore
    app.UseAntiforgery() |> ignore
    app.MapStaticAssets() |> ignore
    app.MapGet("/error", Func<string>(fun () -> "The application could not complete the request.")) |> ignore
    HealthEndpoints.mapHealthEndpoints app |> ignore

    app
        .MapRazorComponents<UI.Index>()
        .AddInteractiveServerRenderMode()
        .AddAdditionalAssemblies(typeof<Application>.Assembly)
    |> ignore

    // 11. Graceful Lifetime Shutdown
    app.Lifetime.ApplicationStopping.Register(fun () ->
        try
            nexusCtx.Dispose()
            printfn "[Medhavi.Nexus] NexusContext disposed successfully"
        with ex ->
            eprintfn $"[Medhavi.Nexus] Error during shutdown: %s{ex.Message}")
    |> ignore

    printfn "========================================================="
    printfn "  Medhavi APS Server Starting"
    printfn "  Port:      %d" nexusConfig.Server.Port
    printfn "  Theme:     %s" nexusConfig.Radzen.Theme
    printfn "  Dashboard: http://localhost:%d" nexusConfig.Server.Port
    printfn "  Health:    http://localhost:%d/health" nexusConfig.Server.Port
    printfn "========================================================="

    app.Run()
    0
