module Medhavi.Client.Supply.Collaboration.SupplierCollaborationStore

open Medhavi.Contracts.Supply
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client

let storeId = DataStoreId.literal "supplier-collaboration-store"

let store: DataStore =
    { Id = storeId
      Fetch =
        fun (ctx: ApplicationContext) ->
            async {
                let queriesOpt =
                    match ctx.Resolve<SupplierCommitmentAssessmentQueries>() with
                    | Some queries -> Some queries
                    | None ->
                        ctx.DomainServices
                        |> Option.map (fun ds -> ds.Supply.CollaborateWithSuppliers.SupplierAssessmentQueries)

                match queriesOpt with
                | Some queries ->
                    let! dtos = queries.GetAll() |> Async.AwaitTask
                    let rows = dtos |> List.map SupplierCollaborationGridSchema.adapter.ToUniversal
                    return Tabular rows
                | None ->
                    return Tabular []
            } }
