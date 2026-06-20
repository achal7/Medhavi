module Medhavi.Web.Stores.ProjectionSubscription

open System
open Medhavi.Contracts.Demand
open Medhavi.SharedKernel.BoundedContexts
open Medhavi.Web

// =============================================================================
// Projection Subscription Layer
// =============================================================================
// This layer listens for projection notifications from Medhavi.Nexus
// and forwards them to the appropriate stores, keeping things decoupled!
// =============================================================================

let private runHandler (handler: string -> Medhavi.Common.Patterns.TaskResult<unit, string>) (id: string) =
    task {
        match! handler id with
        | Ok() -> ()
        | Error err -> printfn $"[ProjectionSubscription] Error handling projection event for {id}: {err}"
    }
    |> Async.AwaitTask
    |> Async.Start

/// Creates and starts the projection subscription layer, wiring notifications to store handlers
let create (demandHandlers: StoreNotificationHandlers) =
    let subscriptions =
        [ DomainEventBus.Subscribe<DemandCreatedNotification>(fun n ->
              runHandler demandHandlers.OnCreated n.DemandLineId)
          DomainEventBus.Subscribe<DemandUpdatedNotification>(fun n ->
              runHandler demandHandlers.OnUpdated n.DemandLineId)
          DomainEventBus.Subscribe<DemandDeletedNotification>(fun n ->
              runHandler demandHandlers.OnDeleted n.DemandLineId) ]

    { new IDisposable with
        member _.Dispose() = subscriptions |> List.iter(fun s -> s.Dispose()) }
