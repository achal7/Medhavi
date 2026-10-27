module Medhavi.Client.Demand.Understanding.DomainHealthStore

open Medhavi.Contracts.Demand
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client

/// Authoritative DataStore ID for Domain Health & Demand Understanding.
let storeId = DataStoreId.literal "demo-domain-health-store"

let store: DataStore =
    { Id = storeId
      Fetch =
        fun (ctx: ApplicationContext) ->
            async {
                match ctx.Resolve<DemandUnderstandingQueries>() with
                | Some queries ->
                    let! dtos = queries.GetAll() |> Async.AwaitTask
                    let rows = dtos |> List.map DomainHealthSchema.adapter.ToUniversal
                    return Tabular rows
                | None ->
                    match ctx.DomainServices with
                    | Some ds ->
                        let! dtos = ds.Demand.UnderstandDemand.Understanding.Queries.GetAll() |> Async.AwaitTask
                        let rows = dtos |> List.map DomainHealthSchema.adapter.ToUniversal
                        return Tabular rows
                    | None ->
                        return Tabular []
            } }
