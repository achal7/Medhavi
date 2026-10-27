module Medhavi.Client.Supply.SupplyOrders.SupplyOrderStore

open Medhavi.Contracts.Supply
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client

type SupplyPlanningQueries = SupplyPlanQueries

let storeId = DataStoreId.literal "supply-store"

let store: DataStore =
    { Id = storeId
      Fetch =
        fun (ctx: ApplicationContext) ->
            async {
                let mapPlans (plans: SupplyPlanDto list) =
                    plans
                    |> List.choose (fun p -> p.CurrentVersion)
                    |> List.collect (fun v -> v.PlannedOrders)

                let queriesOpt =
                    match ctx.Resolve<SupplyPlanQueries>() with
                    | Some q -> Some q
                    | None ->
                        match ctx.Resolve<SupplyPlanningQueries>() with
                        | Some q -> Some q
                        | None ->
                            ctx.DomainServices
                            |> Option.map (fun ds -> ds.Supply.PlanSupply.SupplyPlanQueries)

                match queriesOpt with
                | Some queries ->
                    let! plans = queries.GetAll() |> Async.AwaitTask
                    let orders = mapPlans plans
                    let rows = orders |> List.map SupplyOrderGridSchema.adapter.ToUniversal
                    return Tabular rows
                | None ->
                    return Tabular []
            } }
