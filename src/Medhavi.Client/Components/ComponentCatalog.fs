module Medhavi.Client.Components.ComponentCatalog

open Medhavi.Client
open Medhavi.Client.Registries
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.ActionExecution

// Feature slices: Core
open Medhavi.Client.Core.Demand
open Medhavi.Client.Core.Exceptions
open Medhavi.Client.Core.ItemTransition
open Medhavi.Client.Core.EnterprisePicture
open Medhavi.Client.Core.Traceability

// Feature slices: Demand
open Medhavi.Client.Demand.Observations
open Medhavi.Client.Demand.Understanding

// Feature slices: Supply
open Medhavi.Client.Supply.Inventory
open Medhavi.Client.Supply.Capacity
open Medhavi.Client.Supply.SupplyOrders
open Medhavi.Client.Supply.Exceptions
open Medhavi.Client.Supply.ChangeSensing
open Medhavi.Client.Supply.Collaboration
open Medhavi.Client.Supply.Explanation
open Medhavi.Client.Supply.Pegging

// AI & Cross-cutting
open Medhavi.Client.Components

let private allFeatureManifests =
    [
        DemandFeature.manifest
        ExceptionFeature.manifest
        ItemTransitionFeature.manifest
        EnterprisePictureFeature.manifest
        TraceabilityFeature.manifest
        DemandObservationFeature.manifest
        DemandUnderstandingFeature.manifest
        InventoryFeature.manifest
        CapacityFeature.manifest
        SupplyOrderFeature.manifest
        SupplyExceptionFeature.manifest
        SupplyChangeSensingFeature.manifest
        SupplierCollaborationFeature.manifest
        SupplyExplanationFeature.manifest
        PeggingFeature.manifest
    ]

let allActions: ActionDescriptor list =
    (allFeatureManifests |> List.collect (fun m -> m.Schemas |> List.collect (fun s -> s.Actions)))
    @ KpiSchema.all

let tryFindAction (id: ActionDefId) : ActionDescriptor option =
    allActions |> List.tryFind (fun a -> a.Id = id)

let components: ComponentRegistry =
    let allComps =
        (allFeatureManifests |> List.collect (fun m -> m.Components))
        @ [ MedhaviAssistant.registration
            KpiSummaryGrid.registration ]
    match ComponentRegistry.ofList allComps with
    | Ok registry -> registry
    | Error error -> invalidArg "components" (sprintf "Invalid component catalog: %A" error)

let schemas: SchemaRegistry =
    let allSchemas =
        (allFeatureManifests |> List.collect (fun m -> m.Schemas))
        @ [ MedhaviAssistant.schemaDefinition
            KpiSchema.definition ]
    match SchemaRegistry.ofList allSchemas with
    | Ok registry -> registry
    | Error error -> invalidArg "schemas" (sprintf "Invalid schema catalog: %A" error)

let dataStores: DataStoreRegistry =
    let allStores =
        (allFeatureManifests |> List.collect (fun m -> m.Stores))
        @ [ MedhaviAssistant.storeId, { Id = MedhaviAssistant.storeId; Fetch = fun _ -> async { return Tabular [] } }
            KpiSummaryGrid.storeId, { Id = KpiSummaryGrid.storeId; Fetch = fun _ -> async { return Tabular [] } } ]
    Map.ofList allStores

let actionRegistry: ActionRegistry =
    let allActionsList =
        allFeatureManifests |> List.collect (fun m -> m.Actions)
    ActionRegistry.empty
    |> ActionRegistry.registerAll allActionsList
