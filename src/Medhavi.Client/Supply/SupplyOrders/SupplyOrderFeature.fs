module Medhavi.Client.Supply.SupplyOrders.SupplyOrderFeature

open Medhavi.Contracts.Supply
open Medhavi.Client

let store: DataStore =
    DataStore.create SupplyGrid.storeId (fun ctx ->
        async {
            let mapPlans (plans: SupplyPlanDto list) =
                plans |> List.choose(fun p -> p.CurrentVersion) |> List.collect(fun v -> v.PlannedOrders)

            let queries = ctx.Domain.Supply.PlanSupply.SupplyPlanQueries
            let! plans = queries.GetAll() |> Async.AwaitTask
            let orders = mapPlans plans
            return orders |> List.map SupplyGrid.adapter.ToUniversal
        })

let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = SupplyOrderActions.all
        Components = [ SupplyGrid.registration ]
    }
