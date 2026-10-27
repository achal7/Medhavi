module Medhavi.Client.Supply.Inventory.InventoryStore

open Medhavi.Contracts.Supply
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client

let storeId = DataStoreId.literal "inventory-store"

let store: DataStore =
    { Id = storeId
      Fetch =
        fun (ctx: ApplicationContext) ->
            async {
                match ctx.Resolve<InventoryPositionQueries>() with
                | Some queries ->
                    let! dtos = queries.GetAll() |> Async.AwaitTask
                    let rows = dtos |> List.map InventoryGridSchema.adapter.ToUniversal
                    return Tabular rows
                | None ->
                    match ctx.DomainServices with
                    | Some ds ->
                        let! dtos = ds.Supply.ManageInventory.InventoryPositionQueries.GetAll() |> Async.AwaitTask
                        let rows = dtos |> List.map InventoryGridSchema.adapter.ToUniversal
                        return Tabular rows
                    | None ->
                        return Tabular []
            } }
