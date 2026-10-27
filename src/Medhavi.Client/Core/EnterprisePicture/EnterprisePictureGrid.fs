module Medhavi.Client.Core.EnterprisePicture.EnterprisePictureGrid

open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client.Rendering
open Medhavi.Client.Core.EnterprisePicture

let definitionId = ComponentDefId.literal "enterprise-picture-grid"
let storeId = EnterprisePictureStore.storeId
let schemaId = EnterprisePictureGridSchema.schemaId

let definition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Enterprise Picture Workbench"
      Icon = "photo_library"
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
