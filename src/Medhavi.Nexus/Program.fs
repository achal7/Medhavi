module Medhavi.Nexus.Program

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Radzen
open Bolero.Server
open Medhavi.Client
open Medhavi.Foundation.Observation
open Medhavi.Infrastructure.Observation
open Medhavi.Nexus.Configuration
open Medhavi.Nexus.Composition.ComposeNexus
open Medhavi.Nexus.Services
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

    // 2. Configure Logging & Observability Infrastructure
    let logMinLevel =
        match nexusConfig.Logging.MinimumLevel with
        | "Trace" -> LogLevel.Trace
        | "Debug" -> LogLevel.Debug
        | "Warning" -> LogLevel.Warning
        | "Error" -> LogLevel.Error
        | "Critical" -> LogLevel.Critical
        | _ -> LogLevel.Information

    builder.Logging.ClearProviders() |> ignore
    builder.Logging.SetMinimumLevel(logMinLevel) |> ignore

    if nexusConfig.Logging.ConsoleEnabled then
        builder.Logging.AddConsole() |> ignore

    let fileLoggerOptions: FileLoggerOptions =
        {
            FilePathPattern = nexusConfig.Logging.FilePath
            MinimumLevel = logMinLevel
            BatchSize = nexusConfig.Logging.MailboxBatchSize
            FlushIntervalMs = nexusConfig.Logging.MailboxTimeoutMs
        }

    let fileLoggerProvider, fileLoggerSink = FileLogger.createProvider fileLoggerOptions

    if nexusConfig.Logging.FileEnabled then
        builder.Logging.AddProvider(fileLoggerProvider) |> ignore
        printfn $"[Medhavi.Nexus] File logging active -> {nexusConfig.Logging.FilePath}"

    builder.Services.AddSingleton<FileLoggerSink>(fileLoggerSink) |> ignore

    // 3. Configure Telemetry Sink
    let telemetrySeverity =
        match nexusConfig.Telemetry.MinimumSeverity with
        | "Trace" -> TelemetrySeverity.Trace
        | "Debug" -> TelemetrySeverity.Debug
        | "Warning" -> TelemetrySeverity.Warning
        | "Errors"
        | "Error" -> TelemetrySeverity.Errors
        | "Critical" -> TelemetrySeverity.Critical
        | _ -> TelemetrySeverity.Information

    let telemetryOptions: TelemetrySinkOptions =
        {
            ConsoleEnabled = nexusConfig.Telemetry.ConsoleEnabled
            FileEnabled = nexusConfig.Telemetry.FileEnabled
            FilePathPattern = nexusConfig.Telemetry.FilePath
            MinimumSeverity = telemetrySeverity
            BatchSize = nexusConfig.Telemetry.BatchSize
            FlushIntervalMs = nexusConfig.Telemetry.FlushIntervalMs
        }

    let telemetrySink = TelemetrySink.create telemetryOptions
    let queryOps = ObservationQueryService.create 25000
    let traceabilityQueryService = TraceabilityQueryAdapter.toTraceabilityQueryService queryOps

    // Hydrate existing telemetry files into in-memory ring buffer
    let logDir =
        let dir = Path.GetDirectoryName(nexusConfig.Telemetry.FilePath)
        if String.IsNullOrWhiteSpace dir then "logs" else dir

    if Directory.Exists logDir then
        let logFiles = Directory.GetFiles(logDir, "telemetry-*.jsonl") |> Array.sort

        for f in logFiles do
            let count = queryOps.HydrateFromFile f
            printfn $"[Medhavi.Nexus] Hydrated %d{count} telemetry events from %s{Path.GetFileName f} into ring buffer"

    let dispatchTelemetry (evt: TelemetryEvent) =
        queryOps.Record(evt)
        telemetrySink.Publish(evt)

    builder.Services.AddSingleton<TelemetrySink>(telemetrySink) |> ignore
    builder.Services.AddSingleton<ObservationQueryOps>(queryOps) |> ignore

    builder.Services.AddSingleton<Medhavi.Contracts.Traceability.TraceabilityQueryService>(traceabilityQueryService)
    |> ignore

    if nexusConfig.Telemetry.FileEnabled then
        printfn $"[Medhavi.Nexus] Telemetry journaling active -> {nexusConfig.Telemetry.FilePath}"

    // 4. Create Domain Knowledge Representation Bridge
    let domainLoggerFactory =
        LoggerFactory.Create(fun logging ->
            logging.SetMinimumLevel(logMinLevel) |> ignore

            if nexusConfig.Logging.ConsoleEnabled then
                logging.AddConsole() |> ignore

            if nexusConfig.Logging.FileEnabled then
                logging.AddProvider(fileLoggerProvider) |> ignore)

    let domainLogger = domainLoggerFactory.CreateLogger("Medhavi.Domain")
    let knowledgeProvider = ObservationComposition.createProductionProvider domainLogger dispatchTelemetry
    let publishKnowledge = ObservationComposition.toSinglePublisher knowledgeProvider

    // 5. Configure Kestrel Host URLs
    builder.WebHost.UseUrls($"http://0.0.0.0:{nexusConfig.Server.Port}") |> ignore

    // 6. Add Blazor Interactive Server & Bolero Services
    builder.Services.AddRazorComponents().AddInteractiveServerComponents() |> ignore
    builder.Services.AddBoleroComponents() |> ignore

    // 7. Add Radzen Services
    builder.Services.AddRadzenComponents() |> ignore
    builder.Services.AddScoped<ThemeService>() |> ignore
    builder.Services.AddScoped<DialogService>() |> ignore
    builder.Services.AddScoped<NotificationService>() |> ignore
    builder.Services.AddScoped<TooltipService>() |> ignore
    builder.Services.AddScoped<ContextMenuService>() |> ignore

    // 8. Compose Domain Contexts with Live Observation Publishers
    let composeTask = composeAll publishKnowledge dispatchTelemetry builder.Services

    let nexusCtx: NexusContext =
        match composeTask.GetAwaiter().GetResult() with
        | Ok ctx -> ctx
        | Error err -> failwithf $"Failed to initialize Medhavi domain contexts: %A{err}"

    builder.Services.AddSingleton<NexusContext>(nexusCtx) |> ignore

    // 9. Initialize Client
    Composition.ComposeClient.addApplicationServices applicationConfiguration nexusCtx builder.Services |> ignore

    // 10. Build Web Application
    let app = builder.Build()

    // 11. Startup Data Seeding (feed data to domain layer upon boot)
    Composition.StartupData.seed nexusCtx |> Async.AwaitTask |> Async.RunSynchronously

    // 12. Configure HTTP Middleware Pipeline
    if app.Environment.IsDevelopment() then
        app.UseDeveloperExceptionPage() |> ignore

    app.UseHttpsRedirection() |> ignore
    app.UseAntiforgery() |> ignore
    app.MapStaticAssets() |> ignore
    app.MapGet("/error", Func<string>(fun () -> "The application could not complete the request.")) |> ignore
    HealthEndpoints.mapHealthEndpoints app |> ignore
    TraceabilityEndpoints.mapTraceabilityEndpoints app |> ignore

    app
        .MapRazorComponents<UI.Index>()
        .AddInteractiveServerRenderMode()
        .AddAdditionalAssemblies(typeof<Application>.Assembly)
    |> ignore

    // 13. Graceful Lifetime Shutdown (Flushing all actors & disposing contexts)
    app.Lifetime.ApplicationStopping.Register(fun () ->
        try
            nexusCtx.Dispose()
            telemetrySink.Flush()
            telemetrySink.Shutdown()
            fileLoggerSink.Flush()
            fileLoggerSink.Close()
            printfn "[Medhavi.Nexus] Observability sinks flushed and NexusContext disposed successfully"
        with ex ->
            eprintfn $"[Medhavi.Nexus] Error during shutdown: %s{ex.Message}")
    |> ignore

    printfn "========================================================="
    printfn "  Medhavi APS Server Starting"
    printfn "  Port:        %d" nexusConfig.Server.Port
    printfn "  Theme:       %s" nexusConfig.Radzen.Theme

    printfn
        "  Logging:     Console=%b, File=%b (%s)"
        nexusConfig.Logging.ConsoleEnabled
        nexusConfig.Logging.FileEnabled
        nexusConfig.Logging.FilePath

    printfn
        "  Telemetry:   Console=%b, File=%b (%s)"
        nexusConfig.Telemetry.ConsoleEnabled
        nexusConfig.Telemetry.FileEnabled
        nexusConfig.Telemetry.FilePath

    printfn "  Dashboard:   http://localhost:%d" nexusConfig.Server.Port
    printfn "  Health:      http://localhost:%d/health" nexusConfig.Server.Port
    printfn "========================================================="

    app.Run()
    0
