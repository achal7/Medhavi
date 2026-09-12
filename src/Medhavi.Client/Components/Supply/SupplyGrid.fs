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

module SupplyGrid =

    let definitionId = ComponentDefId.literal "supply-grid"
    let storeId = DataStoreId.literal "demo-supply-store"
    let schemaId = SupplySchema.schemaId

    let definition: ComponentDefinition =
        { Id = definitionId
          DisplayName = "Supply Orders Workbench"
          Icon = "inventory_2"
          Category = ComponentCategory.DataDisplay
          RequiredSchemaCapabilities = [ "Tabular" ]
          DefaultGridSpan = GridSpan.literal 12 6
          DefaultSchemaId = Some schemaId
          DefaultStoreId = Some storeId
          IsResizable = true
          IsDraggable = true
          SelectionMode = SingleSelection }

    let demoRows: SupplyRow list =
        [ { SupplyId = "PO-8801"
            Vendor = "TSMC Semiconductor"
            Product = "H100 GPU Board"
            Location = "Berlin DC"
            PromisedDate = DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero)
            Quantity = 1200m
            Confirmed = 1200m
            Status = "On Track"
            PeggedDemandId = "D-1001" }
          { SupplyId = "PO-8802"
            Vendor = "Infineon Power"
            Product = "A100 Power Module"
            Location = "Munich DC"
            PromisedDate = DateTimeOffset(2026, 9, 5, 0, 0, 0, TimeSpan.Zero)
            Quantity = 950m
            Confirmed = 400m
            Status = "Delayed"
            PeggedDemandId = "D-1002" }
          { SupplyId = "PO-8803"
            Vendor = "Delta Electronics"
            Product = "B200 Inverter"
            Location = "Hamburg DC"
            PromisedDate = DateTimeOffset(2026, 9, 4, 0, 0, 0, TimeSpan.Zero)
            Quantity = 1400m
            Confirmed = 1400m
            Status = "On Track"
            PeggedDemandId = "D-1003" }
          { SupplyId = "PO-8804"
            Vendor = "Delta Electronics"
            Product = "B200 Inverter"
            Location = "Berlin DC"
            PromisedDate = DateTimeOffset(2026, 9, 7, 0, 0, 0, TimeSpan.Zero)
            Quantity = 700m
            Confirmed = 100m
            Status = "Delayed"
            PeggedDemandId = "D-1004" } ]

    let store: DataStore =
        { Id = storeId
          Fetch =
            fun (_ctx: ApplicationContext) ->
                async {
                    let rows = demoRows |> List.map SupplySchema.adapter.ToUniversal
                    return Tabular rows
                } }

    let runtime: ComponentRuntime = { Render = UniversalEnterpriseGrid.render }

    let registration: ComponentRegistration =
        { Definition = definition
          Runtime = runtime }
