module Medhavi.Client.Supply.Inventory.InventoryGrid

open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client.Rendering

let definitionId = ComponentDefId.literal "inventory-grid"
let storeId = InventoryStore.storeId
let schemaId = InventoryGridSchema.schemaId

let definition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Inventory Health Workbench"
      Icon = "inventory"
      Category = ComponentCategory.DataDisplay
      RequiredSchemaCapabilities = [ "Tabular" ]
      DefaultGridSpan = GridSpan.literal 12 6
      DefaultSchemaId = Some schemaId
      DefaultStoreId = Some storeId
      IsResizable = true
      IsDraggable = true
      SelectionMode = SingleSelection }

let runtime: ComponentRuntime =
    { Render = UniversalEnterpriseGrid.render }

let registration: ComponentRegistration =
    { Definition = definition
      Runtime = runtime }
