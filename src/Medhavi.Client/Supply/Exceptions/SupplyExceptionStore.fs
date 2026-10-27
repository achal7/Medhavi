module Medhavi.Client.Supply.Exceptions.SupplyExceptionStore

open Medhavi.Contracts.Supply
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client

let storeId = DataStoreId.literal "supply-exception-store"

let store: DataStore =
    { Id = storeId
      Fetch =
        fun (ctx: ApplicationContext) ->
            async {
                let queriesOpt =
                    match ctx.Resolve<SupplyExceptionQueries>() with
                    | Some queries -> Some queries
                    | None ->
                        ctx.DomainServices
                        |> Option.map (fun ds -> ds.Supply.DetectSupplyExceptions.SupplyExceptionQueries)

                match queriesOpt with
                | Some queries ->
                    let! dtos = queries.GetAll() |> Async.AwaitTask
                    let rows = dtos |> List.map SupplyExceptionGridSchema.adapter.ToUniversal
                    return Tabular rows
                | None ->
                    return Tabular []
            } }
