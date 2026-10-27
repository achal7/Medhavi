module Medhavi.Client.Core.Exceptions.ExceptionGrid

open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client.Rendering
open Medhavi.Client.Core.Exceptions

let definitionId = ComponentDefId.literal "exception-grid"
let storeId = ExceptionStore.storeId
let schemaId = ExceptionGridSchema.schemaId

let definition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Core Exceptions Workbench"
      Icon = "warning"
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
