namespace Medhavi.SharedKernel.Observation

open System
open System.Threading.Tasks

type HealthStatus =
    | Healthy
    | Degraded of reason: string
    | Unhealthy of reason: string

type ComponentHealth = {
    ComponentName: string
    Status: HealthStatus
    LastChecked: DateTimeOffset
    ResponseTime: TimeSpan option
    Details: Map<string, obj>
}

type HealthCheck = unit -> Task<ComponentHealth>

module HealthCheck =
    let createHealth (componentName: string) (status: HealthStatus) = {
        ComponentName = componentName
        Status = status
        LastChecked = DateTimeOffset.UtcNow
        ResponseTime = None
        Details = Map.empty
    }

    let withResponseTime (duration: TimeSpan) (health: ComponentHealth) =
        { health with ResponseTime = Some duration }

    let addDetail (key: string) (value: obj) (health: ComponentHealth) =
        { health with Details = health.Details |> Map.add key value }
