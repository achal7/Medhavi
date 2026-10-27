module Medhavi.Client.Core.Demand.DemandStore

open Medhavi.Contracts.Core.Demand
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client

/// Authoritative Application State and DataStore for Enterprise Demand Orders.
/// Canonical Store ID shared across all demand presentations (workbench, cards, AI tools).
let storeId = DataStoreId.literal "demo-demand-store"

let store: DataStore =
    {
        Id = storeId
        Fetch =
            fun (ctx: ApplicationContext) ->
                async {
                    match ctx.Resolve<DemandQueries>() with
                    | Some queries ->
                        let! dtos = queries.GetAll() |> Async.AwaitTask
                        let rows = dtos |> List.map DemandGridSchema.adapter.ToUniversal
                        return Tabular rows
                    | None ->
                        match ctx.DomainServices with
                        | Some ds ->
                            let! dtos = ds.Core.DemandManagement.Queries.GetAll() |> Async.AwaitTask
                            let rows = dtos |> List.map DemandGridSchema.adapter.ToUniversal
                            return Tabular rows
                        | None -> return Tabular []
                }
    }
