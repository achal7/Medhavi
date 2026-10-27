module Medhavi.Client.Demand.Observations.DemandObservationGrid

open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client.Rendering

let definitionId = ComponentDefId.literal "demand-observation-grid"
let storeId = DemandObservationStore.storeId
let schemaId = DemandObservationGridSchema.schemaId

let definition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Demand Observations Stream"
      Icon = "sensors"
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
