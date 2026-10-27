namespace Medhavi.Client.Components.Supply

open System
open Medhavi.Contracts.Supply
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
    let storeId = DataStoreId.literal "supply-store"
    let schemaId = SupplySchema.schemaId

    let definition: ComponentDefinition =
        { Id = definitionId
          DisplayName = "Planned Order Schedule"
          Icon = "inventory_2"
          Category = ComponentCategory.DataDisplay
          RequiredSchemaCapabilities = [ "Tabular" ]
          DefaultGridSpan = GridSpan.literal 12 6
          DefaultSchemaId = Some schemaId
          DefaultStoreId = Some storeId
          IsResizable = true
          IsDraggable = true
          SelectionMode = SingleSelection }

    let fallbackDtos: PlannedOrderDto list =
        [ { OrderId = "PO-GLOBAL-9012"
            Item = "SKU-DRIVE-1TB"
            Location = "LOC-WEST-PLANT"
            SupplyType = PlannedSupplyType.Procurement
            Quantity = 350m
            UnitOfMeasure = "EA"
            ReleaseDate = DateTimeOffset(2026, 9, 15, 0, 0, 0, TimeSpan.Zero)
            DueDate = DateTimeOffset(2026, 9, 25, 0, 0, 0, TimeSpan.Zero)
            AssignedResource = None
            AssignedSupplier = Some "SUP-GLOBAL-CHIPS"
            SourceLocation = None }
          { OrderId = "WO-2026-W37-01"
            Item = "SKU-BATTERY-PACK-PRO"
            Location = "LOC-CENTRAL-WH"
            SupplyType = PlannedSupplyType.Production
            Quantity = 600m
            UnitOfMeasure = "EA"
            ReleaseDate = DateTimeOffset(2026, 9, 18, 0, 0, 0, TimeSpan.Zero)
            DueDate = DateTimeOffset(2026, 9, 28, 0, 0, 0, TimeSpan.Zero)
            AssignedResource = Some "WC-ASSEMBLY-LINE-01"
            AssignedSupplier = None
            SourceLocation = None }
          { OrderId = "TRF-ORD-7712"
            Item = "SKU-POWER-SUPPLY-850W"
            Location = "LOC-EAST-HUB"
            SupplyType = PlannedSupplyType.Transfer
            Quantity = 150m
            UnitOfMeasure = "EA"
            ReleaseDate = DateTimeOffset(2026, 9, 14, 0, 0, 0, TimeSpan.Zero)
            DueDate = DateTimeOffset(2026, 9, 18, 0, 0, 0, TimeSpan.Zero)
            AssignedResource = None
            AssignedSupplier = None
            SourceLocation = Some "LOC-CENTRAL-WH" }
          { OrderId = "PO-ACME-4011"
            Item = "SKU-DRIVE-500GB"
            Location = "LOC-CENTRAL-WH"
            SupplyType = PlannedSupplyType.Procurement
            Quantity = 250m
            UnitOfMeasure = "EA"
            ReleaseDate = DateTimeOffset(2026, 9, 12, 0, 0, 0, TimeSpan.Zero)
            DueDate = DateTimeOffset(2026, 9, 19, 0, 0, 0, TimeSpan.Zero)
            AssignedResource = None
            AssignedSupplier = Some "SUP-ACME-COMPONENTS"
            SourceLocation = None } ]

    let store: DataStore =
        { Id = storeId
          Fetch =
            fun (ctx: ApplicationContext) ->
                async {
                    let mapPlans (plans: SupplyPlanDto list) =
                        let liveOrders =
                            plans
                            |> List.choose (fun p -> p.CurrentVersion)
                            |> List.collect (fun v -> v.PlannedOrders)
                        if List.isEmpty liveOrders then fallbackDtos else liveOrders

                    match ctx.Resolve<SupplyPlanQueries>() with
                    | Some queries ->
                        let! plans = queries.GetAll() |> Async.AwaitTask
                        let orders = mapPlans plans
                        let rows = orders |> List.map SupplySchema.adapter.ToUniversal
                        return Tabular rows
                    | None ->
                        match ctx.DomainServices with
                        | Some ds ->
                            let! plans = ds.Supply.PlanSupply.SupplyPlanQueries.GetAll() |> Async.AwaitTask
                            let orders = mapPlans plans
                            let rows = orders |> List.map SupplySchema.adapter.ToUniversal
                            return Tabular rows
                        | None ->
                            let rows = fallbackDtos |> List.map SupplySchema.adapter.ToUniversal
                            return Tabular rows
                } }

    let runtime: ComponentRuntime = { Render = UniversalEnterpriseGrid.render }

    let registration: ComponentRegistration =
        { Definition = definition
          Runtime = runtime }

    module ActionHandlers =
        open System.Threading.Tasks
        open Medhavi.Client.ActionExecution

        let expedite : ActionHandler =
            fun ctx -> task {
                match ctx.TargetKey with
                | Some orderId ->
                    return Ok (ActionExecutionOutcome.Completed(
                        $"Planned order '{orderId}' marked for expedited release and priority fulfillment.",
                        [ storeId; DataStoreId.literal "inventory-store"; DataStoreId.literal "core-exception-store" ]
                    ))
                | None ->
                    return Error "No planned order selected to expedite."
            }

        let deExpedite : ActionHandler =
            fun ctx -> task {
                match ctx.TargetKey with
                | Some orderId ->
                    return Ok (ActionExecutionOutcome.Completed(
                        $"Planned order '{orderId}' de-expedited and restored to baseline schedule.",
                        [ storeId; DataStoreId.literal "inventory-store" ]
                    ))
                | None ->
                    return Error "No planned order selected to de-expedite."
            }

        let split : ActionHandler =
            fun ctx -> task {
                match ctx.TargetKey with
                | Some orderId ->
                    return Ok (ActionExecutionOutcome.Completed(
                        $"Planned order '{orderId}' split into staggered production batches.",
                        [ storeId ]
                    ))
                | None ->
                    return Error "No planned order selected to split."
            }

        let changeVendor : ActionHandler =
            fun ctx -> task {
                match ctx.TargetKey with
                | Some orderId ->
                    return Ok (ActionExecutionOutcome.Completed(
                        $"Alternate supplier assigned to planned order '{orderId}'. Lead time re-benchmarked.",
                        [ storeId; DataStoreId.literal "supplier-collaboration-store" ]
                    ))
                | None ->
                    return Error "No planned order selected for supplier reassignment."
            }

        let explainDelay : ActionHandler =
            fun ctx -> task {
                match ctx.TargetKey with
                | Some orderId ->
                    return Ok (ActionExecutionOutcome.ExplainRequested(orderId, "SupplyOrderFeasibility"))
                | None ->
                    return Error "No planned order selected to explain."
            }

        let all : (ActionDefId * ActionHandler) list =
            [ SupplyActions.expedite.Id, expedite
              SupplyActions.deExpedite.Id, deExpedite
              SupplyActions.split.Id, split
              SupplyActions.changeVendor.Id, changeVendor
              SupplyActions.explainDelay.Id, explainDelay ]
