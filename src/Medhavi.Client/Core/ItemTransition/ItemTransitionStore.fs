module Medhavi.Client.Core.ItemTransition.ItemTransitionStore

open Medhavi.Contracts.Core.ItemTransition
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client
open Medhavi.Client.Core.ItemTransition

/// Authoritative DataStore for Item Transitions & Supersessions.
let storeId: DataStoreId = DataStoreId.literal "core-item-transition-store"

let store: DataStore =
    { Id = storeId
      Fetch =
        fun (ctx: ApplicationContext) ->
            async {
                match ctx.Resolve<ItemTransitionQueries>() with
                | Some queries ->
                    let! dtos = queries.GetAll() |> Async.AwaitTask
                    let rows = dtos |> List.map ItemTransitionGridSchema.adapter.ToUniversal
                    return Tabular rows
                | None ->
                    match ctx.DomainServices with
                    | Some ds ->
                        let! dtos = ds.Core.ItemTransitionManagement.Queries.GetAll() |> Async.AwaitTask
                        let rows = dtos |> List.map ItemTransitionGridSchema.adapter.ToUniversal
                        return Tabular rows
                    | None ->
                        return Tabular []
            } }
