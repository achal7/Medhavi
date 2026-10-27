module Medhavi.Client.Supply.Exceptions.SupplyExceptionGrid

open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client.Rendering

let definitionId = ComponentDefId.literal "comp-supply-exceptions-grid"
let storeId = SupplyExceptionStore.storeId
let schemaId = SupplyExceptionGridSchema.schemaId

let definition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Supply Exceptions Workbench"
      Icon = "warning"
      Category = ComponentCategory.DataDisplay
      RequiredSchemaCapabilities = [ "Tabular" ]
      DefaultGridSpan = GridSpan.literal 12 6
      DefaultSchemaId = Some schemaId
      DefaultStoreId = Some storeId
      IsResizable = true
      IsDraggable = true
      SelectionMode = SelectionMode.SingleSelection }

let runtime: ComponentRuntime =
    { Render = UniversalEnterpriseGrid.render }

let registration: ComponentRegistration =
    { Definition = definition
      Runtime = runtime }
