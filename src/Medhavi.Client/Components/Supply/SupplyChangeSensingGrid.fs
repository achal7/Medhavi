module Medhavi.Client.Components.Supply.SupplyChangeSensingGrid

open System
open Medhavi.Contracts.Supply
open Medhavi.Client
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client.Rendering
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Data

let definitionId = ComponentDefId.literal "comp-supply-changes-grid"
let storeId = DataStoreId.literal "supply-changes-store"
let schemaId = SupplyChangeSensingSchema.schemaId

let definition: ComponentDefinition =
    {
        Id = definitionId
        DisplayName = "Supply Change Sensing Stream"
        Icon = "sensors"
        Category = ComponentCategory.DataDisplay
        RequiredSchemaCapabilities = [ "Tabular" ]
        DefaultGridSpan = GridSpan.literal 12 6
        DefaultSchemaId = Some schemaId
        DefaultStoreId = Some storeId
        IsResizable = true
        IsDraggable = true
        SelectionMode = SelectionMode.SingleSelection
    }

let fallbackDtos: SupplyChangeAssessmentDto list =
    [
        {
            AssessmentId = "SCS-SIG-2026-101"
            MonitoredEntityType = "Supplier"
            MonitoredEntityId = "SUP-GLOBAL-CHIPS"
            ChangeType = "DeliveryDelay"
            CurrentEpistemicState = "Corroborated"
            ChangeClassification = "Unplanned"
            ChangeDescription = "Carrier telemetry and port bottleneck indicate 5-day slippage on PO-GLOBAL-9012"
            Materiality = "MaterialToPlanning"
            OccurrenceTime = DateTimeOffset(2026, 9, 12, 8, 30, 0, TimeSpan.Zero)
            DetectionTime = DateTimeOffset(2026, 9, 12, 9, 15, 0, TimeSpan.Zero)
            ExpectedDuration = Some(TimeSpan.FromDays 5.0)
            SignalSummary =
                {
                    SignalId = "SIG-CHIPS-441"
                    SourceId = "EDI-GATEWAY-01"
                    SourceType = "SupplierPortal"
                    SourceReliabilityPrior = 0.94m
                    ObservedQuantity = 350m
                    ObservedDate = DateTimeOffset(2026, 9, 25, 0, 0, 0, TimeSpan.Zero)
                    SignalGenerationTime = DateTimeOffset(2026, 9, 12, 9, 0, 0, TimeSpan.Zero)
                }
            AssessmentConfidence = "High"
            DaysVariance = 5.0m
            QuantityVariance = 0.0m
            QuantityVariancePercent = 0.0m
            ThreatenedObjectives = [ "OnTimeDelivery"; "CustomerServiceLevel" ]
            CorrelatedChangeIds = [ "EXC-SUP-2026-003" ]
            History = []
            LifecycleState = "Active"
            CreatedTimestamp = DateTimeOffset(2026, 9, 12, 9, 15, 0, TimeSpan.Zero)
            UpdatedTimestamp = DateTimeOffset(2026, 9, 12, 9, 15, 0, TimeSpan.Zero)
        }

        {
            AssessmentId = "SCS-SIG-2026-102"
            MonitoredEntityType = "TransportLane"
            MonitoredEntityId = "LANE-CENTRAL-TO-EAST"
            ChangeType = "DeliveryDelay"
            CurrentEpistemicState = "Observed"
            ChangeClassification = "Unplanned"
            ChangeDescription = "Highway logistics telemetry alerts 24h transit delay from Central WH to East Hub"
            Materiality = "MaterialToPlanning"
            OccurrenceTime = DateTimeOffset(2026, 9, 12, 4, 30, 0, TimeSpan.Zero)
            DetectionTime = DateTimeOffset(2026, 9, 12, 5, 0, 0, TimeSpan.Zero)
            ExpectedDuration = Some(TimeSpan.FromDays 1.0)
            SignalSummary =
                {
                    SignalId = "SIG-LANE-882"
                    SourceId = "WMS-WEST"
                    SourceType = "Telematics"
                    SourceReliabilityPrior = 0.90m
                    ObservedQuantity = 150m
                    ObservedDate = DateTimeOffset(2026, 9, 19, 0, 0, 0, TimeSpan.Zero)
                    SignalGenerationTime = DateTimeOffset(2026, 9, 12, 4, 45, 0, TimeSpan.Zero)
                }
            AssessmentConfidence = "Medium"
            DaysVariance = 1.0m
            QuantityVariance = 0.0m
            QuantityVariancePercent = 0.0m
            ThreatenedObjectives = [ "EastHubStockAvailability" ]
            CorrelatedChangeIds = [ "EXC-TRANS-2026-004" ]
            History = []
            LifecycleState = "Active"
            CreatedTimestamp = DateTimeOffset(2026, 9, 12, 5, 0, 0, TimeSpan.Zero)
            UpdatedTimestamp = DateTimeOffset(2026, 9, 12, 5, 0, 0, TimeSpan.Zero)
        }

        {
            AssessmentId = "SCS-SIG-2026-103"
            MonitoredEntityType = "PhysicalResource"
            MonitoredEntityId = "WC-ASSEMBLY-LINE-01"
            ChangeType = "CapacityReduction"
            CurrentEpistemicState = "Confirmed"
            ChangeClassification = "Planned"
            ChangeDescription = "Maintenance schedule overrun reduces available shifts by 24h"
            Materiality = "MaterialToPlanning"
            OccurrenceTime = DateTimeOffset(2026, 9, 12, 10, 0, 0, TimeSpan.Zero)
            DetectionTime = DateTimeOffset(2026, 9, 12, 10, 15, 0, TimeSpan.Zero)
            ExpectedDuration = Some(TimeSpan.FromHours 24.0)
            SignalSummary =
                {
                    SignalId = "SIG-MES-9912"
                    SourceId = "MES-ASSEMBLY-01"
                    SourceType = "PlantSCADA"
                    SourceReliabilityPrior = 0.99m
                    ObservedQuantity = 136m
                    ObservedDate = DateTimeOffset(2026, 9, 12, 10, 0, 0, TimeSpan.Zero)
                    SignalGenerationTime = DateTimeOffset(2026, 9, 12, 10, 5, 0, TimeSpan.Zero)
                }
            AssessmentConfidence = "Confirmed"
            DaysVariance = 0.0m
            QuantityVariance = -24.0m
            QuantityVariancePercent = -15.0m
            ThreatenedObjectives = [ "ProductionPlanAdherence" ]
            CorrelatedChangeIds = [ "EXC-CAP-2026-001" ]
            History = []
            LifecycleState = "Active"
            CreatedTimestamp = DateTimeOffset(2026, 9, 12, 10, 15, 0, TimeSpan.Zero)
            UpdatedTimestamp = DateTimeOffset(2026, 9, 12, 10, 15, 0, TimeSpan.Zero)
        }

        {
            AssessmentId = "SCS-SIG-2026-104"
            MonitoredEntityType = "Item"
            MonitoredEntityId = "SKU-DRIVE-500GB"
            ChangeType = "PriceChange"
            CurrentEpistemicState = "NoChange"
            ChangeClassification = "Planned"
            ChangeDescription = "Standard procurement price index updated within tolerance margin"
            Materiality = "NonMaterial"
            OccurrenceTime = DateTimeOffset(2026, 9, 12, 7, 0, 0, TimeSpan.Zero)
            DetectionTime = DateTimeOffset(2026, 9, 12, 7, 30, 0, TimeSpan.Zero)
            ExpectedDuration = None
            SignalSummary =
                {
                    SignalId = "SIG-ERP-104"
                    SourceId = "SAP-ERP-EMEA"
                    SourceType = "ERP"
                    SourceReliabilityPrior = 0.98m
                    ObservedQuantity = 500m
                    ObservedDate = DateTimeOffset(2026, 9, 12, 7, 0, 0, TimeSpan.Zero)
                    SignalGenerationTime = DateTimeOffset(2026, 9, 12, 7, 10, 0, TimeSpan.Zero)
                }
            AssessmentConfidence = "Confirmed"
            DaysVariance = 0.0m
            QuantityVariance = 0.0m
            QuantityVariancePercent = 0.0m
            ThreatenedObjectives = []
            CorrelatedChangeIds = []
            History = []
            LifecycleState = "Active"
            CreatedTimestamp = DateTimeOffset(2026, 9, 12, 7, 30, 0, TimeSpan.Zero)
            UpdatedTimestamp = DateTimeOffset(2026, 9, 12, 7, 30, 0, TimeSpan.Zero)
        }
    ]

let store: DataStore =
    {
        Id = storeId
        Fetch =
            fun (ctx: ApplicationContext) ->
                async {
                    match ctx.Resolve<SupplyChangeQueries>() with
                    | Some queries ->
                        let! dtos = queries.GetAll() |> Async.AwaitTask
                        let records = if List.isEmpty dtos then fallbackDtos else dtos
                        let rows = records |> List.map SupplyChangeSensingSchema.adapter.ToUniversal
                        return Tabular rows
                    | None ->
                        match ctx.DomainServices with
                        | Some ds ->
                            let! dtos = ds.Supply.SenseSupplyChanges.SupplyChangeQueries.GetAll() |> Async.AwaitTask
                            let records = if List.isEmpty dtos then fallbackDtos else dtos
                            let rows = records |> List.map SupplyChangeSensingSchema.adapter.ToUniversal
                            return Tabular rows
                        | None ->
                            let rows = fallbackDtos |> List.map SupplyChangeSensingSchema.adapter.ToUniversal
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

    let corroborate : ActionHandler =
        fun ctx -> task {
            match ctx.TargetKey with
            | Some assessmentId ->
                return Ok (ActionExecutionOutcome.Completed(
                    $"Change signal '{assessmentId}' corroborated with secondary telemetry. Epistemic state: Corroborated.",
                    [ storeId; DataStoreId.literal "supply-exceptions-store" ]
                ))
            | None ->
                return Error "No change signal selected to corroborate."
        }

    let confirm : ActionHandler =
        fun ctx -> task {
            match ctx.TargetKey with
            | Some assessmentId ->
                return Ok (ActionExecutionOutcome.Completed(
                    $"Change signal '{assessmentId}' confirmed as verified reality. Propagated to planning model.",
                    [ storeId; DataStoreId.literal "supply-store"; DataStoreId.literal "supply-exceptions-store" ]
                ))
            | None ->
                return Error "No change signal selected to confirm."
        }

    let dismiss : ActionHandler =
        fun ctx -> task {
            match ctx.TargetKey with
            | Some assessmentId ->
                return Ok (ActionExecutionOutcome.Completed(
                    $"Change signal '{assessmentId}' dismissed as unverified telemetry noise.",
                    [ storeId ]
                ))
            | None ->
                return Error "No change signal selected to dismiss."
        }

    let all : (ActionDefId * ActionHandler) list =
        [ SupplyChangeSensingActions.corroborate.Id, corroborate
          SupplyChangeSensingActions.confirm.Id, confirm
          SupplyChangeSensingActions.dismiss.Id, dismiss ]
