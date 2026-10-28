module Medhavi.Client.Core.MasterData.MasterDataFeature

open System
open Medhavi.Contracts.Core
open Medhavi.Client
open Medhavi.Client.Core.MasterData

let store =
    DataStore.create MasterDataGrid.storeId (fun ctx ->
        async {
            let! itemRows =
                async {
                    match ctx.Domain.Core.MasterData |> Option.bind(fun md -> md.ItemQueries) with
                    | Some q ->
                        let! items = q.GetAll() |> Async.AwaitTask

                        return
                            items
                            |> List.map(fun i ->
                                ({
                                    EntityType = "Item"
                                    Id = i.Id
                                    Name = i.Name
                                    SubType = string i.ItemType
                                    State = i.State
                                    RoleOrTimeZone =
                                        if List.isEmpty i.ItemRoles then "None" else String.concat ", " i.ItemRoles
                                }
                                : MasterDataGrid.MasterDataRow))
                    | None -> return []
                }

            let! locationRows =
                async {
                    match ctx.Domain.Core.MasterData |> Option.bind(fun md -> md.LocationQueries) with
                    | Some q ->
                        let! locations = q.GetAll() |> Async.AwaitTask

                        return
                            locations
                            |> List.map(fun l ->
                                ({
                                    EntityType = "Location"
                                    Id = l.Id
                                    Name = l.Name
                                    SubType = string l.LocationType
                                    State = l.State
                                    RoleOrTimeZone = l.TimeZoneId
                                }
                                : MasterDataGrid.MasterDataRow))
                    | None -> return []
                }

            let allRows = itemRows @ locationRows
            let universal = allRows |> List.map MasterDataGrid.adapter.ToUniversal
            return universal
        })

/// Feature composition manifest for the Core Master Data bounded context slice.
let manifest: FeatureManifest =
    {
        Stores = [ (store.Id, store) ]
        Actions = MasterDataActions.all
        Components = [ MasterDataGrid.registration ]
    }
