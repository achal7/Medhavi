module Medhavi.Client.Supply.Pegging.PeggingFeature

open System
open Medhavi.Contracts.Core.Demand
open Medhavi.Contracts.Supply
open Medhavi.Client

let store =
    DataStore.create PeggingScheduler.storeId (fun ctx ->
        async {
            let! demands = ctx.Domain.Core.DemandManagement.Queries.GetAll() |> Async.AwaitTask
            let! plans = ctx.Domain.Supply.PlanSupply.SupplyPlanQueries.GetAll() |> Async.AwaitTask
            let supplies = plans |> List.choose(fun p -> p.CurrentVersion) |> List.collect(fun v -> v.PlannedOrders)

            let demandRows =
                demands
                |> List.map(fun d ->
                    PeggingScheduler.createRow
                        d.Id
                        $"Demand: {d.Id} ({d.Item} · {d.Quantity} EA)"
                        d.NeedWindowLatest
                        (d.NeedWindowLatest.AddHours 8.0)
                        "Demand"
                        d.Quantity
                        d.Item
                        d.LifecycleState)

            let supplyRows =
                supplies
                |> List.map(fun o ->
                    PeggingScheduler.createRow
                        o.OrderId
                        $"Supply: {o.OrderId} ({o.Item} · {o.Quantity} {o.UnitOfMeasure})"
                        o.ReleaseDate
                        o.DueDate
                        "PlannedSupply"
                        o.Quantity
                        o.Item
                        "Planned")

            return demandRows @ supplyRows
        })

let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = PeggingSchedulerActions.all
        Components = [ PeggingScheduler.registration ]
    }
