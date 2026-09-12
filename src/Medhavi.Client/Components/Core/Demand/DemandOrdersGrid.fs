namespace Medhavi.Client.Components.Core.Demand

open System
open Medhavi.Contracts.Core.Demand
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.Rendering
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client

module DemandOrdersGrid =

    let definitionId = ComponentDefId.literal "demand-grid"
    let storeId = DataStoreId.literal "demo-demand-store"
    let schemaId = DemandOrdersSchema.schemaId

    let definition: ComponentDefinition =
        { Id = definitionId
          DisplayName = "Demand Orders Workbench"
          Icon = "view_list"
          Category = ComponentCategory.DataDisplay
          RequiredSchemaCapabilities = [ "Tabular" ]
          DefaultGridSpan = GridSpan.literal 12 6
          DefaultSchemaId = Some schemaId
          DefaultStoreId = Some storeId
          IsResizable = true
          IsDraggable = true
          SelectionMode = SingleSelection }

    let private fallbackDtos: DemandDto list =
        [ { Id = "D-1001"
            Item = "H100 GPU Board"
            Location = "Berlin DC"
            Customer = Some "Bosch Mobility"
            Quantity = 1200m
            NeedWindowLatest = DateTimeOffset(2026, 9, 2, 0, 0, 0, TimeSpan.Zero)
            NeedWindowEarliest = None
            NeedWindowPreferred = None
            DemandOrigin = "CustomerOrder"
            ParentDemand = None
            LifecycleState = "On Track" }
          { Id = "D-1002"
            Item = "A100 Power Module"
            Location = "Munich DC"
            Customer = Some "Siemens Health"
            Quantity = 950m
            NeedWindowLatest = DateTimeOffset(2026, 9, 3, 0, 0, 0, TimeSpan.Zero)
            NeedWindowEarliest = None
            NeedWindowPreferred = None
            DemandOrigin = "CustomerOrder"
            ParentDemand = None
            LifecycleState = "Shortage" }
          { Id = "D-1003"
            Item = "B200 Inverter"
            Location = "Hamburg DC"
            Customer = Some "Bosch Mobility"
            Quantity = 1400m
            NeedWindowLatest = DateTimeOffset(2026, 9, 4, 0, 0, 0, TimeSpan.Zero)
            NeedWindowEarliest = None
            NeedWindowPreferred = None
            DemandOrigin = "CustomerOrder"
            ParentDemand = None
            LifecycleState = "On Track" }
          { Id = "D-1004"
            Item = "B200 Inverter"
            Location = "Berlin DC"
            Customer = Some "Volkswagen AG"
            Quantity = 700m
            NeedWindowLatest = DateTimeOffset(2026, 9, 5, 0, 0, 0, TimeSpan.Zero)
            NeedWindowEarliest = None
            NeedWindowPreferred = None
            DemandOrigin = "Forecast"
            ParentDemand = None
            LifecycleState = "Delayed" } ]

    let store: DataStore =
        { Id = storeId
          Fetch =
            fun (ctx: ApplicationContext) ->
                async {
                    match ctx.DomainServices with
                    | Some ds ->
                        let! dtos = ds.Core.DemandManagement.Queries.GetAll() |> Async.AwaitTask
                        let rows = dtos |> List.map DemandOrdersSchema.adapter.ToUniversal
                        return Tabular rows
                    | None ->
                        let rows = fallbackDtos |> List.map DemandOrdersSchema.adapter.ToUniversal
                        return Tabular rows
                } }

    let runtime: ComponentRuntime = { Render = UniversalEnterpriseGrid.render }

    let registration: ComponentRegistration =
        { Definition = definition
          Runtime = runtime }
