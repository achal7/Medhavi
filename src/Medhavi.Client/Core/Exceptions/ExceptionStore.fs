module Medhavi.Client.Core.Exceptions.ExceptionStore

open Medhavi.Contracts.Core.Exception
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client
open Medhavi.Client.Core.Exceptions

/// Authoritative DataStore for Core Constraint Exceptions.
let storeId: DataStoreId = DataStoreId.literal "core-exception-store"

let store: DataStore =
    { Id = storeId
      Fetch =
        fun (ctx: ApplicationContext) ->
            async {
                match ctx.Resolve<ExceptionQueries>() with
                | Some queries ->
                    let! dtos = queries.GetAll() |> Async.AwaitTask
                    let rows = dtos |> List.map ExceptionGridSchema.adapter.ToUniversal
                    return Tabular rows
                | None ->
                    match ctx.DomainServices with
                    | Some ds ->
                        let! dtos = ds.Core.ExceptionManagement.Queries.GetAll() |> Async.AwaitTask
                        let rows = dtos |> List.map ExceptionGridSchema.adapter.ToUniversal
                        return Tabular rows
                    | None ->
                        return Tabular []
            } }
