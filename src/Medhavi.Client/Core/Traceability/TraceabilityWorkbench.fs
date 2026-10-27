module Medhavi.Client.Core.Traceability.TraceabilityWorkbench

open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client.Rendering
open Medhavi.Client.Core.Traceability

let definitionId = ComponentDefId.literal "traceability-workbench"
let storeId = TraceabilityStore.storeId
let schemaId = TraceabilityGridSchema.schemaId

let definition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Traceability & Investigation Studio"
      Icon = "account_tree"
      Category = ComponentCategory.Visualization
      RequiredSchemaCapabilities = [ "Tabular" ]
      DefaultGridSpan = GridSpan.literal 12 8
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
