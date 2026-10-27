module Medhavi.Client.Demand.Observations.DemandObservationStore

open Medhavi.Contracts.Demand
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client

/// Canonical DataStore ID for Demand Observations.
let storeId = DataStoreId.literal "demand-observation-store"

let store: DataStore =
    { Id = storeId
      Fetch =
        fun (ctx: ApplicationContext) ->
            async {
                match ctx.Resolve<DemandObservationQueries>() with
                | Some queries ->
                    let! dtos = queries.GetAll() |> Async.AwaitTask
                    let rows = dtos |> List.map DemandObservationGridSchema.adapter.ToUniversal
                    return Tabular rows
                | None ->
                    match ctx.DomainServices with
                    | Some ds ->
                        let! dtos = ds.Demand.UnderstandDemand.Observation.Queries.GetAll() |> Async.AwaitTask
                        let rows = dtos |> List.map DemandObservationGridSchema.adapter.ToUniversal
                        return Tabular rows
                    | None ->
                        return Tabular []
            } }
