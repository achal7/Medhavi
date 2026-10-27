module Medhavi.Client.Supply.Explanation.SupplyExplanationStore

open Medhavi.Contracts.Supply
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client

let storeId = DataStoreId.literal "supply-explanation-store"

let store: DataStore =
    { Id = storeId
      Fetch =
        fun (ctx: ApplicationContext) ->
            async {
                let queriesOpt =
                    match ctx.Resolve<SupplyExplanationQueries>() with
                    | Some queries -> Some queries
                    | None ->
                        ctx.DomainServices
                        |> Option.map (fun ds -> ds.Supply.ExplainSupplyDecisions.SupplyExplanationQueries)

                match queriesOpt with
                | Some queries ->
                    let! dtos = queries.GetAll() |> Async.AwaitTask
                    let rows = dtos |> List.map SupplyExplanationSchema.adapter.ToUniversal
                    return Tabular rows
                | None ->
                    return Tabular []
            } }
