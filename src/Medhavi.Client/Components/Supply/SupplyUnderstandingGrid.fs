module Medhavi.Client.Components.Supply.SupplyUnderstandingGrid

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

let definitionId = ComponentDefId.literal "supply-understanding-grid"
let storeId = DataStoreId.literal "supply-understanding-store"
let schemaId = SupplyUnderstandingSchema.schemaId

let definition: ComponentDefinition =
    {
        Id = definitionId
        DisplayName = "Supply Sensory Stream"
        Icon = "sensors"
        Category = ComponentCategory.DataDisplay
        RequiredSchemaCapabilities = [ "Tabular" ]
        DefaultGridSpan = GridSpan.literal 12 6
        DefaultSchemaId = Some schemaId
        DefaultStoreId = Some storeId
        IsResizable = true
        IsDraggable = true
        SelectionMode = SingleSelection
    }

let fallbackDtos: SupplyDataRecordDto list =
    [
        {
            RecordId = "SREC-INV-2026-001"
            Item = "SKU-DRIVE-500GB"
            Location = "LOC-CENTRAL-WH"
            SupplyType = "Inventory"
            Quantity = 500m
            UnitOfMeasure = "EA"
            BatchIdentifier = Some "BATCH-2026-09-A"
            OrderIdentifier = None
            CommitmentIdentifier = None
            SupplierIdentifier = None
            ExpectedDeliveryDate = None
            BusinessTime = DateTimeOffset(2026, 9, 12, 8, 0, 0, TimeSpan.Zero)
            ObservationTime = DateTimeOffset(2026, 9, 12, 8, 30, 0, TimeSpan.Zero)
            SourceSystemProvenance = "SAP-ERP-EMEA"
            LifecycleState = "Evaluated"
            DecisionTraceability = Some "BR-S-001"
        }
        {
            RecordId = "SREC-PO-2026-002"
            Item = "SKU-DRIVE-1TB"
            Location = "LOC-WEST-PLANT"
            SupplyType = "PurchaseOrder"
            Quantity = 350m
            UnitOfMeasure = "EA"
            BatchIdentifier = None
            OrderIdentifier = Some "PO-GLOBAL-9012"
            CommitmentIdentifier = Some "COMM-CHIPS-441"
            SupplierIdentifier = Some "SUP-GLOBAL-CHIPS"
            ExpectedDeliveryDate = Some(DateTimeOffset(2026, 9, 25, 0, 0, 0, TimeSpan.Zero))
            BusinessTime = DateTimeOffset(2026, 9, 12, 9, 0, 0, TimeSpan.Zero)
            ObservationTime = DateTimeOffset(2026, 9, 12, 9, 15, 0, TimeSpan.Zero)
            SourceSystemProvenance = "EDI-GATEWAY-01"
            LifecycleState = "Evaluated"
            DecisionTraceability = Some "BR-S-002"
        }
        {
            RecordId = "SREC-WO-2026-003"
            Item = "SKU-BATTERY-PACK-PRO"
            Location = "LOC-CENTRAL-WH"
            SupplyType = "ProductionOrder"
            Quantity = 600m
            UnitOfMeasure = "EA"
            BatchIdentifier = Some "WO-BATTERY-LINE-1"
            OrderIdentifier = Some "WO-2026-W37-01"
            CommitmentIdentifier = None
            SupplierIdentifier = None
            ExpectedDeliveryDate = Some(DateTimeOffset(2026, 9, 28, 0, 0, 0, TimeSpan.Zero))
            BusinessTime = DateTimeOffset(2026, 9, 12, 9, 30, 0, TimeSpan.Zero)
            ObservationTime = DateTimeOffset(2026, 9, 12, 9, 45, 0, TimeSpan.Zero)
            SourceSystemProvenance = "MES-ASSEMBLY-01"
            LifecycleState = "Evaluated"
            DecisionTraceability = Some "BR-S-003"
        }
        {
            RecordId = "SREC-TRF-2026-004"
            Item = "SKU-POWER-SUPPLY-850W"
            Location = "LOC-EAST-HUB"
            SupplyType = "Transfer"
            Quantity = 150m
            UnitOfMeasure = "EA"
            BatchIdentifier = None
            OrderIdentifier = Some "TRF-ORD-7712"
            CommitmentIdentifier = None
            SupplierIdentifier = None
            ExpectedDeliveryDate = Some(DateTimeOffset(2026, 9, 18, 0, 0, 0, TimeSpan.Zero))
            BusinessTime = DateTimeOffset(2026, 9, 12, 10, 0, 0, TimeSpan.Zero)
            ObservationTime = DateTimeOffset(2026, 9, 12, 10, 15, 0, TimeSpan.Zero)
            SourceSystemProvenance = "WMS-WEST"
            LifecycleState = "Evaluated"
            DecisionTraceability = Some "BR-S-004"
        }
    ]

let store: DataStore =
    {
        Id = storeId
        Fetch =
            fun (ctx: ApplicationContext) ->
                async {
                    match ctx.Resolve<SupplyDataRecordQueries>() with
                    | Some queries ->
                        let! dtos = queries.GetAll() |> Async.AwaitTask
                        let records = if List.isEmpty dtos then fallbackDtos else dtos
                        let rows = records |> List.map SupplyUnderstandingSchema.adapter.ToUniversal
                        return Tabular rows
                    | None ->
                        match ctx.DomainServices with
                        | Some ds ->
                            let! dtos = ds.Supply.UnderstandSupply.SupplyDataRecordQueries.GetAll() |> Async.AwaitTask
                            let records = if List.isEmpty dtos then fallbackDtos else dtos
                            let rows = records |> List.map SupplyUnderstandingSchema.adapter.ToUniversal
                            return Tabular rows
                        | None ->
                            let rows = fallbackDtos |> List.map SupplyUnderstandingSchema.adapter.ToUniversal
                            return Tabular rows
                }
    }

let runtime: ComponentRuntime =
    {
        Render = UniversalEnterpriseGrid.render
    }

let registration: ComponentRegistration =
    {
        Definition = definition
        Runtime = runtime
    }

module ActionHandlers =
    open System.Threading.Tasks
    open Medhavi.Client.ActionExecution

    let acknowledge : ActionHandler =
        fun ctx -> task {
            match ctx.TargetKey with
            | Some recordId ->
                return Ok (ActionExecutionOutcome.Completed(
                    $"Supply sensory record '{recordId}' acknowledged into planning operational memory.",
                    [ storeId; DataStoreId.literal "supply-changes-store" ]
                ))
            | None ->
                return Error "No supply sensory record selected to acknowledge."
        }

    let evaluate : ActionHandler =
        fun ctx -> task {
            match ctx.TargetKey with
            | Some recordId ->
                return Ok (ActionExecutionOutcome.Completed(
                    $"Supply materiality evaluated for record '{recordId}'. Variance signals updated.",
                    [ storeId; DataStoreId.literal "supply-changes-store"; DataStoreId.literal "supply-exceptions-store" ]
                ))
            | None ->
                return Error "No supply sensory record selected to evaluate."
        }

    let all : (ActionDefId * ActionHandler) list =
        [ SupplyUnderstandingActions.acknowledge.Id, acknowledge
          SupplyUnderstandingActions.evaluate.Id, evaluate ]
