module Medhavi.Client.Components.Core.EnterprisePicture.EnterprisePictureGrid

open System
open System.Threading.Tasks
open Medhavi.Contracts.Core
open Medhavi.Contracts.Core.Demand
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.Rendering
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client

let definitionId = ComponentDefId.literal "enterprise-picture-grid"
let storeId = DataStoreId.literal "core-enterprise-picture-store"
let schemaId = EnterprisePictureSchema.schemaId

let definition: ComponentDefinition =
    {
        Id = definitionId
        DisplayName = "Enterprise Picture Workbench"
        Icon = "photo_library"
        Category = ComponentCategory.DataDisplay
        RequiredSchemaCapabilities = [ "Tabular" ]
        DefaultGridSpan = GridSpan.literal 12 6
        DefaultSchemaId = Some schemaId
        DefaultStoreId = Some storeId
        IsResizable = true
        IsDraggable = true
        SelectionMode = SingleSelection
    }

let private fallbackRows: EnterprisePictureVersionRow list =
    [
        {
            VersionKey = "EUROPE-CENTRAL#1"
            PlanningScopeId = "EUROPE-CENTRAL"
            VersionNumber = 1
            LifecycleState = "Superseded"
            DemandCount = 4
            SupplyCount = 3
            InventoryCount = 2
            PublicationTime = Some(DateTimeOffset(2026, 9, 1, 8, 0, 0, TimeSpan.Zero))
            IsCurrentPublished = false
        }
        {
            VersionKey = "EUROPE-CENTRAL#2"
            PlanningScopeId = "EUROPE-CENTRAL"
            VersionNumber = 2
            LifecycleState = "Published"
            DemandCount = 4
            SupplyCount = 4
            InventoryCount = 2
            PublicationTime = Some(DateTimeOffset(2026, 9, 2, 8, 0, 0, TimeSpan.Zero))
            IsCurrentPublished = true
        }
        {
            VersionKey = "EUROPE-CENTRAL#3"
            PlanningScopeId = "EUROPE-CENTRAL"
            VersionNumber = 3
            LifecycleState = "Draft"
            DemandCount = 5
            SupplyCount = 4
            InventoryCount = 3
            PublicationTime = None
            IsCurrentPublished = false
        }
    ]

let store: DataStore =
    {
        Id = storeId
        Fetch =
            fun (ctx: ApplicationContext) ->
                async {
                    match ctx.Resolve<EnterprisePictureQueries>() with
                    | Some queries ->
                        let! pictures = queries.GetAll() |> Async.AwaitTask

                        if pictures.IsEmpty then
                            let rows = fallbackRows |> List.map EnterprisePictureSchema.adapter.ToUniversal
                            return Tabular rows
                        else
                            let rows =
                                pictures
                                |> List.collect(fun pic ->
                                    pic.Versions
                                    |> List.map(fun v ->
                                        {
                                            VersionKey = $"{pic.PlanningScopeId}#{v.VersionNumber}"
                                            PlanningScopeId = pic.PlanningScopeId
                                            VersionNumber = v.VersionNumber
                                            LifecycleState = v.LifecycleState
                                            DemandCount = v.DemandReferences.Length
                                            SupplyCount = v.SupplyReferences.Length
                                            InventoryCount = v.InventoryReferences.Length
                                            PublicationTime = v.PublicationTime
                                            IsCurrentPublished = (pic.CurrentPublishedVersion = Some v.VersionNumber)
                                        }))
                                |> List.map EnterprisePictureSchema.adapter.ToUniversal

                            return Tabular rows
                    | None ->
                        let rows = fallbackRows |> List.map EnterprisePictureSchema.adapter.ToUniversal
                        return Tabular rows
                }
    }

let runtime: ComponentRuntime =
    {
        Render = UniversalEnterpriseGrid.render
    }

let registration: ComponentRegistration =
    {
        Definition = definition
        Runtime = runtime
    }

module ActionHandlers =
    open Medhavi.Client.ActionExecution

    let publishVersion: ActionHandler =
        fun ctx ->
            task {
                match ctx.Resolve<EnterprisePictureApi>() with
                | Some api ->
                    match ctx.TargetKey with
                    | Some key ->
                        let scopeId, versionNum =
                            if key.Contains("#") then
                                let parts = key.Split('#')
                                parts.[0], Int32.Parse(parts.[1])
                            else
                                "EUROPE-CENTRAL", Int32.Parse(key)

                        let req: PublishPictureVersionReq =
                            {
                                PlanningScopeId = scopeId
                                VersionNumber = versionNum
                                PublicationTime = DateTimeOffset.Now
                            }

                        match! api.Publish(req) with
                        | Ok pic ->
                            return
                                Ok(
                                    ActionExecutionOutcome.Completed(
                                        $"Enterprise Picture version {versionNum} published for scope '{pic.PlanningScopeId}'.",
                                        [
                                            storeId
                                            DataStoreId.literal "demo-demand-store"
                                            DataStoreId.literal "core-exception-store"
                                        ]
                                    )
                                )
                        | Error err -> return Error err.Message
                    | None -> return Error "No picture version selected to publish."
                | None -> return Error "EnterprisePictureApi capability is not connected."
            }

    let composeVersion: ActionHandler =
        fun ctx ->
            task {
                match ctx.Resolve<EnterprisePictureApi>() with
                | Some api ->
                    let scopeId = "EUROPE-CENTRAL"

                    let! demandRefs =
                        match ctx.Resolve<DemandQueries>() with
                        | Some dq -> dq.GetAll() |> Task.map(List.map(fun d -> d.Id))
                        | None -> Task.FromResult []

                    let activeDemands = if demandRefs.IsEmpty then [ "D-1001"; "D-1002"; "D-1003" ] else demandRefs

                    let req: ComposePictureVersionReq =
                        {
                            PlanningScopeId = scopeId
                            DemandReferences = activeDemands
                            SupplyReferences = [ "PO-8801"; "PO-8802" ]
                            InventoryReferences = [ "INV-BER-01"; "INV-MUN-01" ]
                            CompositionTime = DateTimeOffset.Now
                        }

                    match! api.Compose(req) with
                    | Ok pic ->
                        let draftVer =
                            pic.Versions
                            |> List.tryLast
                            |> Option.map(fun v -> v.VersionNumber)
                            |> Option.defaultValue 1

                        return
                            Ok(
                                ActionExecutionOutcome.Completed(
                                    $"New Draft Enterprise Picture version {draftVer} composed for scope '{pic.PlanningScopeId}'.",
                                    [ storeId ]
                                )
                            )
                    | Error err -> return Error err.Message
                | None -> return Error "EnterprisePictureApi capability is not connected."
            }

    let all: (ActionDefId * ActionHandler) list =
        [
            EnterprisePictureActions.publish.Id, publishVersion
            EnterprisePictureActions.compose.Id, composeVersion
        ]
