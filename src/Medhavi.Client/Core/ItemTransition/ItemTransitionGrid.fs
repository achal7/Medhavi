module Medhavi.Client.Core.ItemTransition.ItemTransitionGrid

open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client.Rendering
open Medhavi.Client.Core.ItemTransition

let definitionId = ComponentDefId.literal "item-transition-grid"
let storeId = ItemTransitionStore.storeId
let schemaId = ItemTransitionGridSchema.schemaId

let definition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Item Transitions Workbench"
      Icon = "swap_calls"
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
