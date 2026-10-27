module Medhavi.Client.Core.Demand.DemandOrdersGrid

open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client.Rendering

/// Thin Radzen presentation adapter for Enterprise Demand Orders Grid.
let definitionId = ComponentDefId.literal "demand-grid"
let storeId = DemandStore.storeId

let definition: ComponentDefinition =
    {
        Id = definitionId
        DisplayName = "Demands"
        Icon = "view_list"
        Category = ComponentCategory.DataDisplay
        RequiredSchemaCapabilities = [ "Tabular" ]
        DefaultGridSpan = GridSpan.literal 12 6
        DefaultSchemaId = Some DemandGridSchema.schemaId
        DefaultStoreId = Some DemandStore.storeId
        IsResizable = true
        IsDraggable = true
        SelectionMode = SingleSelection
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
