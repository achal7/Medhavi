namespace Medhavi.Infrastructure.Observation

open Microsoft.Extensions.Logging
open Medhavi.Foundation.Observation
open Logger

/// Composes all observation adapters into a coherent observation layer.
/// This is the single place where logging, telemetry, tracing, and health checks are wired together.

module ObservationComposition =

    /// Create the default ArchitecturalKnowledgeProvider from the configured adapters.
    let createDefaultProvider
        (loggerRep: KnowledgeRepresentation)
        (telemetryRep: KnowledgeRepresentation)
        (tracingRep: KnowledgeRepresentation)
        (healthRep: KnowledgeRepresentation)
        : ArchitecturalKnowledgeProvider =
        [ loggerRep; telemetryRep; tracingRep; healthRep ]

    /// Create a minimal provider suitable for development and testing.
    let createDevelopmentProvider (dispatchTelemetry: TelemetryEvent -> unit) : ArchitecturalKnowledgeProvider =
        let logger = ConsoleLogger("Medhavi.Development") |> Logger.Create |> toKnowledgeRepresentation
        let telemetryRep = TelemetryPublisher.toKnowledgeRepresentation dispatchTelemetry

        [ logger; telemetryRep ]

    /// Create the production ArchitecturalKnowledgeProvider from an ILogger.
    let createProductionProvider
        (ilogger: ILogger)
        (dispatchTelemetry: TelemetryEvent -> unit)
        : ArchitecturalKnowledgeProvider =
        let logger = Logger.Create ilogger
        //let logger = NullLoggerFactory.Instance.CreateLogger("Medhavi.Development")
        let loggerRep = toKnowledgeRepresentation logger
        let telemetryRep = TelemetryPublisher.toKnowledgeRepresentation dispatchTelemetry
        let tracingRep = fun _ -> () // OpenTelemetry export is configured at startup
        let healthRep = fun _ -> () // Health checks are registered separately
        [ loggerRep; telemetryRep; tracingRep; healthRep ]

    /// Flattens a provider into a single KnowledgeRepresentation.
    let toSinglePublisher (provider: ArchitecturalKnowledgeProvider) : KnowledgeRepresentation =
        fun knowledge -> provider |> List.iter(fun rep -> rep knowledge)
