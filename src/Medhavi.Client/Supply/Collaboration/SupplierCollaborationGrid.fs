module Medhavi.Client.Supply.Collaboration.SupplierCollaborationGrid

open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client.Rendering

let definitionId = ComponentDefId.literal "comp-supplier-collaboration-grid"
let storeId = SupplierCollaborationStore.storeId
let schemaId = SupplierCollaborationGridSchema.schemaId

let definition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Supplier Collaboration Workbench"
      Icon = "handshake"
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
