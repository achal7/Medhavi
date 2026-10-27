module Medhavi.Client.Core.EnterprisePicture.EnterprisePictureStore

open Medhavi.Contracts.Core
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client
open Medhavi.Client.Core.EnterprisePicture.EnterprisePictureGridSchema

/// Authoritative DataStore for Enterprise Picture Versions.
let storeId: DataStoreId = DataStoreId.literal "core-enterprise-picture-store"

let store: DataStore =
    { Id = storeId
      Fetch =
        fun (ctx: ApplicationContext) ->
            async {
                match ctx.Resolve<EnterprisePictureQueries>() with
                | Some queries ->
                    let! pictures = queries.GetAll() |> Async.AwaitTask
                    let rows =
                        pictures
                        |> List.collect (fun pic ->
                            pic.Versions
                            |> List.map (fun v ->
                                { VersionKey = $"{pic.PlanningScopeId}#{v.VersionNumber}"
                                  PlanningScopeId = pic.PlanningScopeId
                                  VersionNumber = v.VersionNumber
                                  LifecycleState = v.LifecycleState
                                  DemandCount = v.DemandReferences.Length
                                  SupplyCount = v.SupplyReferences.Length
                                  InventoryCount = v.InventoryReferences.Length
                                  PublicationTime = v.PublicationTime
                                  IsCurrentPublished = (pic.CurrentPublishedVersion = Some v.VersionNumber) }))
                        |> List.map EnterprisePictureGridSchema.adapter.ToUniversal
                    return Tabular rows
                | None ->
                    return Tabular []
            } }
