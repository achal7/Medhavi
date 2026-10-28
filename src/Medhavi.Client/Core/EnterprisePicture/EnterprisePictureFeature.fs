module Medhavi.Client.Core.EnterprisePicture.EnterprisePictureFeature

open Medhavi.Client
open Medhavi.Client.Core.EnterprisePicture

let store =
    DataStore.create EnterprisePictureGrid.storeId (fun ctx ->
        async {
            let! pictures = ctx.Domain.Core.EnterprisePicture.Queries.GetAll() |> Async.AwaitTask

            let rows =
                pictures
                |> List.collect(fun pic ->
                    pic.Versions
                    |> List.map(fun v ->
                        {
                            EnterprisePictureGrid.VersionKey = $"{pic.PlanningScopeId}#{v.VersionNumber}"
                            PlanningScopeId = pic.PlanningScopeId
                            VersionNumber = v.VersionNumber
                            LifecycleState = v.LifecycleState
                            DemandCount = v.DemandReferences.Length
                            SupplyCount = v.SupplyReferences.Length
                            InventoryCount = v.InventoryReferences.Length
                            PublicationTime = v.PublicationTime
                            IsCurrentPublished = (pic.CurrentPublishedVersion = Some v.VersionNumber)
                        }
                        : EnterprisePictureGrid.EnterprisePictureVersionRow))
                |> List.map EnterprisePictureGrid.adapter.ToUniversal

            return rows
        })

/// Feature composition manifest for the Enterprise Picture bounded context slice.
let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = EnterprisePictureActions.all
        Components = [ EnterprisePictureGrid.registration ]
    }
