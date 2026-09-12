namespace Medhavi.Client.Components.Supply

open System
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.Rendering
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client

module CapacityGrid =

    let definitionId = ComponentDefId.literal "capacity-grid"
    let storeId = DataStoreId.literal "demo-capacity-store"
    let schemaId = CapacitySchema.schemaId

    let definition: ComponentDefinition =
        { Id = definitionId
          DisplayName = "Work Center Capacity Workbench"
          Icon = "precision_manufacturing"
          Category = ComponentCategory.DataDisplay
          RequiredSchemaCapabilities = [ "Tabular" ]
          DefaultGridSpan = GridSpan.literal 12 6
          DefaultSchemaId = Some schemaId
          DefaultStoreId = Some storeId
          IsResizable = true
          IsDraggable = true
          SelectionMode = SingleSelection }

    let demoRows: CapacityRow list =
        [ { WorkCenterId = "WC-BER-01"
            PlantId = "Berlin DC"
            LineName = "SMT Board Assembly Cell 1"
            AvailableHours = 160.0m
            RequiredHours = 148.0m
            UtilizationPct = 92.5m
            ShiftPattern = "2 Shifts (16h)"
            BottleneckStatus = "Elevated Load"
            IsOvertimeAuthorized = false }
          { WorkCenterId = "WC-MUN-02"
            PlantId = "Munich DC"
            LineName = "Power Inverter Test Bay"
            AvailableHours = 120.0m
            RequiredHours = 138.0m
            UtilizationPct = 115.0m
            ShiftPattern = "2 Shifts (16h)"
            BottleneckStatus = "Critical Overload"
            IsOvertimeAuthorized = true }
          { WorkCenterId = "WC-HAM-01"
            PlantId = "Hamburg DC"
            LineName = "Sensor Calibration Center"
            AvailableHours = 160.0m
            RequiredHours = 112.0m
            UtilizationPct = 70.0m
            ShiftPattern = "2 Shifts (16h)"
            BottleneckStatus = "Normal"
            IsOvertimeAuthorized = false } ]

    let store: DataStore =
        { Id = storeId
          Fetch =
            fun (_ctx: ApplicationContext) ->
                async {
                    let rows = demoRows |> List.map CapacitySchema.adapter.ToUniversal
                    return Tabular rows
                } }

    let runtime: ComponentRuntime = { Render = UniversalEnterpriseGrid.render }

    let registration: ComponentRegistration =
        { Definition = definition
          Runtime = runtime }
