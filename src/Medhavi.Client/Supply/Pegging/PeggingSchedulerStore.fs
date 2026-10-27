module Medhavi.Client.Supply.Pegging.PeggingSchedulerStore

open System
open Medhavi.Contracts.Core.Demand
open Medhavi.Contracts.Supply
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client

let storeId = DataStoreId.literal "pegging-scheduler-store"

let store: DataStore =
    { Id = storeId
      Fetch =
        fun (ctx: ApplicationContext) ->
            async {
                let! demands =
                    async {
                        match ctx.Resolve<DemandQueries>() with
                        | Some queries ->
                            let! dtos = queries.GetAll() |> Async.AwaitTask
                            return dtos
                        | None ->
                            match ctx.DomainServices with
                            | Some ds ->
                                let! dtos = ds.Core.DemandManagement.Queries.GetAll() |> Async.AwaitTask
                                return dtos
                            | None -> return []
                    }

                let! supplies =
                    async {
                        match ctx.Resolve<SupplyPlanQueries>() with
                        | Some queries ->
                            let! plans = queries.GetAll() |> Async.AwaitTask
                            return
                                plans
                                |> List.choose (fun p -> p.CurrentVersion)
                                |> List.collect (fun v -> v.PlannedOrders)
                        | None ->
                            match ctx.DomainServices with
                            | Some ds ->
                                let! plans = ds.Supply.PlanSupply.SupplyPlanQueries.GetAll() |> Async.AwaitTask
                                return
                                    plans
                                    |> List.choose (fun p -> p.CurrentVersion)
                                    |> List.collect (fun v -> v.PlannedOrders)
                            | None -> return []
                    }

                let demandRows =
                    demands
                    |> List.map (fun d ->
                        PeggingSchedulerSchema.createRow
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
                    |> List.map (fun o ->
                        PeggingSchedulerSchema.createRow
                            o.OrderId
                            $"Supply: {o.OrderId} ({o.Item} · {o.Quantity} {o.UnitOfMeasure})"
                            o.ReleaseDate
                            o.DueDate
                            "PlannedSupply"
                            o.Quantity
                            o.Item
                            "Planned")

                return Tabular (demandRows @ supplyRows)
            } }
