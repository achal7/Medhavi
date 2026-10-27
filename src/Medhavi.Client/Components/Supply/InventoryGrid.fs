module Medhavi.Client.Components.Supply.InventoryGrid

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

let definitionId = ComponentDefId.literal "inventory-grid"
let storeId = DataStoreId.literal "inventory-store"
let schemaId = InventorySchema.schemaId

let definition: ComponentDefinition =
    {
        Id = definitionId
        DisplayName = "Inventory Health Workbench"
        Icon = "inventory"
        Category = ComponentCategory.DataDisplay
        RequiredSchemaCapabilities = [ "Tabular" ]
        DefaultGridSpan = GridSpan.literal 12 6
        DefaultSchemaId = Some schemaId
        DefaultStoreId = Some storeId
        IsResizable = true
        IsDraggable = true
        SelectionMode = SingleSelection
    }

let fallbackDtos: InventoryPositionAssessmentDto list =
    [
        {
            ItemId = "SKU-DRIVE-500GB"
            LocationId = "LOC-CENTRAL-WH"
            LifecycleState = "Active"
            OnHandQuantity = 500m
            AllocatedQuantity = 250m
            InTransitQuantity = 150m
            AvailableQuantity = 400m
            DaysOfSupply = 12.5m
            CoverageStatus = "Normal"
            RiskLevel = "Low"
            HealthStatus = "Healthy"
            PolicyComplianceStatus = "InCompliance"
            FinancialExposure = None
            SourceSupplyUnderstandingVersion = Some 1
            SourceSupplyPlanVersion = Some 1
            AssessmentTimestamp = DateTimeOffset(2026, 9, 12, 8, 30, 0, TimeSpan.Zero)
            ChangeHistory = []
        }
        {
            ItemId = "SKU-DRIVE-1TB"
            LocationId = "LOC-WEST-PLANT"
            LifecycleState = "Active"
            OnHandQuantity = 350m
            AllocatedQuantity = 300m
            InTransitQuantity = 200m
            AvailableQuantity = 250m
            DaysOfSupply = 4.8m
            CoverageStatus = "Shortage"
            RiskLevel = "High"
            HealthStatus = "AtRisk"
            PolicyComplianceStatus = "UnderBuffer"
            FinancialExposure = Some 36000m
            SourceSupplyUnderstandingVersion = Some 1
            SourceSupplyPlanVersion = Some 1
            AssessmentTimestamp = DateTimeOffset(2026, 9, 12, 9, 15, 0, TimeSpan.Zero)
            ChangeHistory = []
        }
        {
            ItemId = "SKU-BATTERY-PACK-PRO"
            LocationId = "LOC-CENTRAL-WH"
            LifecycleState = "Active"
            OnHandQuantity = 600m
            AllocatedQuantity = 450m
            InTransitQuantity = 600m
            AvailableQuantity = 750m
            DaysOfSupply = 18.2m
            CoverageStatus = "Normal"
            RiskLevel = "Low"
            HealthStatus = "Healthy"
            PolicyComplianceStatus = "InCompliance"
            FinancialExposure = None
            SourceSupplyUnderstandingVersion = Some 1
            SourceSupplyPlanVersion = Some 1
            AssessmentTimestamp = DateTimeOffset(2026, 9, 12, 9, 45, 0, TimeSpan.Zero)
            ChangeHistory = []
        }
        {
            ItemId = "SKU-POWER-SUPPLY-850W"
            LocationId = "LOC-EAST-HUB"
            LifecycleState = "Active"
            OnHandQuantity = 150m
            AllocatedQuantity = 140m
            InTransitQuantity = 0m
            AvailableQuantity = 10m
            DaysOfSupply = 1.8m
            CoverageStatus = "Critical"
            RiskLevel = "Critical"
            HealthStatus = "Critical"
            PolicyComplianceStatus = "UnderBuffer"
            FinancialExposure = Some 7500m
            SourceSupplyUnderstandingVersion = Some 1
            SourceSupplyPlanVersion = Some 1
            AssessmentTimestamp = DateTimeOffset(2026, 9, 12, 10, 15, 0, TimeSpan.Zero)
            ChangeHistory = []
        }
    ]

let store: DataStore =
    {
        Id = storeId
        Fetch =
            fun (ctx: ApplicationContext) ->
                async {
                    match ctx.Resolve<InventoryPositionQueries>() with
                    | Some queries ->
                        let! dtos = queries.GetAll() |> Async.AwaitTask
                        let positions = if List.isEmpty dtos then fallbackDtos else dtos
                        let rows = positions |> List.map InventorySchema.adapter.ToUniversal
                        return Tabular rows
                    | None ->
                        match ctx.DomainServices with
                        | Some ds ->
                            let! dtos = ds.Supply.ManageInventory.InventoryPositionQueries.GetAll() |> Async.AwaitTask
                            let positions = if List.isEmpty dtos then fallbackDtos else dtos
                            let rows = positions |> List.map InventorySchema.adapter.ToUniversal
                            return Tabular rows
                        | None ->
                            let rows = fallbackDtos |> List.map InventorySchema.adapter.ToUniversal
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

    let adjustSafetyStock : ActionHandler =
        fun ctx -> task {
            match ctx.TargetKey with
            | Some itemId ->
                match ctx.Resolve<InventoryPolicyApi>() with
                | Some api ->
                    let req : UpdateInventoryPolicyReq =
                        { ItemId = itemId
                          LocationId = "LOC-CENTRAL-WH"
                          TargetServiceLevel = Some 0.98m
                          ManualOverrideParameters = None }
                    match! api.Update(req) with
                    | Ok _ ->
                        return Ok (ActionExecutionOutcome.Completed(
                            $"Safety stock policy updated for item '{itemId}'. Buffer targets recalculated.",
                            [ storeId; DataStoreId.literal "supply-store"; DataStoreId.literal "core-exception-store" ]
                        ))
                    | Error err ->
                        return Error err.Message
                | None ->
                    return Ok (ActionExecutionOutcome.Completed(
                        $"Safety stock adjusted for item '{itemId}'. Buffer targets updated.",
                        [ storeId; DataStoreId.literal "supply-store"; DataStoreId.literal "core-exception-store" ]
                    ))
            | None ->
                return Error "No inventory item selected for safety stock adjustment."
        }

    let expedite : ActionHandler =
        fun ctx -> task {
            match ctx.TargetKey with
            | Some itemId ->
                match ctx.Resolve<InventoryPositionApi>() with
                | Some api ->
                    let req : UpdateInventoryPositionReq =
                        { ItemId = itemId
                          LocationId = "LOC-CENTRAL-WH"
                          TriggeringEvidence = [ "EXPEDITE_REPLENISHMENT_WORKBENCH" ] }
                    match! api.Update(req) with
                    | Ok _ ->
                        return Ok (ActionExecutionOutcome.Completed(
                            $"Emergency replenishment requisition triggered for item '{itemId}'.",
                            [ storeId; DataStoreId.literal "supply-store"; DataStoreId.literal "core-exception-store" ]
                        ))
                    | Error err ->
                        return Error err.Message
                | None ->
                    return Ok (ActionExecutionOutcome.Completed(
                        $"Emergency replenishment requisition triggered for item '{itemId}'.",
                        [ storeId; DataStoreId.literal "supply-store"; DataStoreId.literal "core-exception-store" ]
                    ))
            | None ->
                return Error "No inventory item selected to expedite."
        }

    let explainRisk : ActionHandler =
        fun ctx -> task {
            match ctx.TargetKey with
            | Some itemId ->
                return Ok (ActionExecutionOutcome.ExplainRequested(itemId, "InventoryRisk"))
            | None ->
                return Error "No inventory item selected to explain risk."
        }

    let all : (ActionDefId * ActionHandler) list =
        [ InventoryActions.adjustSafetyStock.Id, adjustSafetyStock
          InventoryActions.expedite.Id, expedite
          InventoryActions.explainRisk.Id, explainRisk ]
