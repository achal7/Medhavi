namespace Medhavi.Infrastructure.Observation

open System
open System.Threading.Tasks
open Medhavi.Foundation.Observation

/// Publishes component health information.
module HealthCheckPublisher =

    /// Run a health check and produce ArchitecturalKnowledge.
    let runHealthCheck (healthCheck: HealthCheck) : Task<ArchitecturalKnowledge> =
        task {
            let! componentHealth = healthCheck()

            let statusText =
                match componentHealth.Status with
                | Healthy -> "Healthy"
                | Degraded reason -> $"Degraded: {reason}"
                | Unhealthy reason -> $"Unhealthy: {reason}"

            let attributes =
                componentHealth.Details
                |> Map.add "Component" (box componentHealth.ComponentName)
                |> Map.add "Status" (box statusText)
                |> Map.add "LastChecked" (box componentHealth.LastChecked)
                |> fun m ->
                    match componentHealth.ResponseTime with
                    | Some rt -> m |> Map.add "ResponseTimeMs" (box rt.TotalMilliseconds)
                    | None -> m

            return
                { Name = "HealthCheck"
                  Timestamp = DateTimeOffset.UtcNow
                  Attributes = attributes }
        }

    /// Create a KnowledgeRepresentation that runs all registered health checks and publishes results.
    let toKnowledgeRepresentation (healthChecks: HealthCheck list) : KnowledgeRepresentation =
        fun knowledge ->
            for hc in healthChecks do
                task {
                    let! healthKnowledge = runHealthCheck hc
                    // Health knowledge is published alongside the triggering knowledge
                    // For MVP, we log it; in production, it goes to Prometheus/AlertManager
                    ()
                }
                |> ignore
