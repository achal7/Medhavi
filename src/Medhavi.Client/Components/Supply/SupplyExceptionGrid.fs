module Medhavi.Client.Components.Supply.SupplyExceptionGrid

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

let definitionId = ComponentDefId.literal "comp-supply-exceptions-grid"
let storeId = DataStoreId.literal "supply-exceptions-store"
let schemaId = SupplyExceptionSchema.schemaId

let definition: ComponentDefinition =
    {
        Id = definitionId
        DisplayName = "Supply Exceptions Workbench"
        Icon = "warning"
        Category = ComponentCategory.DataDisplay
        RequiredSchemaCapabilities = [ "Tabular" ]
        DefaultGridSpan = GridSpan.literal 12 6
        DefaultSchemaId = Some schemaId
        DefaultStoreId = Some storeId
        IsResizable = true
        IsDraggable = true
        SelectionMode = SelectionMode.SingleSelection
    }

let fallbackDtos: SupplyExceptionAssessmentDto list =
    [
        {
            AssessmentId = "EXC-CAP-2026-001"
            GovernedPolicyReference = "POL-CAP-ENVELOPE-01"
            ViolatedRuleId = "BR-S-041"
            ExceptionType = "CapacityExceeded"
            ScopeEntityType = "WorkCenter"
            ScopeEntityId = "WC-ASSEMBLY-LINE-01"
            PlanningScopeId = "PS-GLOBAL-SUPPLY-CHAIN"
            LifecycleState = "Detected"
            BusinessImpact = "Severe"
            Urgency = "Immediate"
            Scope = "WorkCenter"
            PotentialImpact = "Delayed 250 units of Demand DEM-ORD-1001"
            CurrentImpact = "Load: 184h / 160h (115% Utilization)"
            ExceptionHorizon = "Immediate"
            DetectionTimestamp = DateTimeOffset(2026, 9, 12, 10, 15, 0, TimeSpan.Zero)
            ResolutionTimestamp = None
            EvidenceReferences = [ "EVD-TELEMETRY-9912"; "CR-CAP-BOTTLENECK-01" ]
            EvaluationConfidence = "High"
            ReasonCodes = [ "RC-BOTTLENECK-OVERLOAD" ]
            CorrelatedExceptionIds = []
            History = []
            CreatedTimestamp = DateTimeOffset(2026, 9, 12, 10, 15, 0, TimeSpan.Zero)
            UpdatedTimestamp = DateTimeOffset(2026, 9, 12, 10, 15, 0, TimeSpan.Zero)
        }

        {
            AssessmentId = "EXC-MAT-2026-002"
            GovernedPolicyReference = "POL-INV-BUFFER-02"
            ViolatedRuleId = "BR-S-022"
            ExceptionType = "MaterialShortage"
            ScopeEntityType = "ItemLocation"
            ScopeEntityId = "SKU-POWER-SUPPLY-850W@LOC-EAST-HUB"
            PlanningScopeId = "PS-GLOBAL-SUPPLY-CHAIN"
            LifecycleState = "Confirmed"
            BusinessImpact = "Major"
            Urgency = "Immediate"
            Scope = "ItemLocation"
            PotentialImpact = "Stockout risk for Demand DEM-PROD-3001"
            CurrentImpact = "Available: 10 EA / Safety Stock: 50 EA"
            ExceptionHorizon = "Immediate"
            DetectionTimestamp = DateTimeOffset(2026, 9, 12, 9, 0, 0, TimeSpan.Zero)
            ResolutionTimestamp = None
            EvidenceReferences = [ "EVD-STOCK-ALERT-403"; "CR-MAT-SHORTAGE-02" ]
            EvaluationConfidence = "High"
            ReasonCodes = [ "RC-BUFFER-BREACH" ]
            CorrelatedExceptionIds = []
            History = []
            CreatedTimestamp = DateTimeOffset(2026, 9, 12, 9, 0, 0, TimeSpan.Zero)
            UpdatedTimestamp = DateTimeOffset(2026, 9, 12, 9, 0, 0, TimeSpan.Zero)
        }

        {
            AssessmentId = "EXC-SUP-2026-003"
            GovernedPolicyReference = "POL-SUP-COLLAB-03"
            ViolatedRuleId = "BR-S-053"
            ExceptionType = "LeadTimeBreach"
            ScopeEntityType = "Supplier"
            ScopeEntityId = "SUP-GLOBAL-CHIPS"
            PlanningScopeId = "PS-GLOBAL-SUPPLY-CHAIN"
            LifecycleState = "Detected"
            BusinessImpact = "Moderate"
            Urgency = "ShortTerm"
            Scope = "Supplier"
            PotentialImpact = "Purchase Order PO-GLOBAL-9012 commitment slipped +5 days"
            CurrentImpact = "Promised: Sep 25 vs Expected: Sep 20"
            ExceptionHorizon = "ShortTerm"
            DetectionTimestamp = DateTimeOffset(2026, 9, 12, 9, 15, 0, TimeSpan.Zero)
            ResolutionTimestamp = None
            EvidenceReferences = [ "COMM-CHIPS-441"; "SCS-SIG-2026-101" ]
            EvaluationConfidence = "Medium"
            ReasonCodes = [ "RC-SUPPLIER-SLIPPAGE" ]
            CorrelatedExceptionIds = []
            History = []
            CreatedTimestamp = DateTimeOffset(2026, 9, 12, 9, 15, 0, TimeSpan.Zero)
            UpdatedTimestamp = DateTimeOffset(2026, 9, 12, 9, 15, 0, TimeSpan.Zero)
        }

        {
            AssessmentId = "EXC-TRANS-2026-004"
            GovernedPolicyReference = "POL-DIST-TRANS-01"
            ViolatedRuleId = "BR-S-081"
            ExceptionType = "CarrierCongestion"
            ScopeEntityType = "TransportLane"
            ScopeEntityId = "LANE-CENTRAL-TO-EAST"
            PlanningScopeId = "PS-GLOBAL-SUPPLY-CHAIN"
            LifecycleState = "Detected"
            BusinessImpact = "Minor"
            Urgency = "ShortTerm"
            Scope = "TransportLane"
            PotentialImpact = "Transfer TRF-ORD-7712 transit delay +1 day"
            CurrentImpact = "Port/Rail yard congestion alert"
            ExceptionHorizon = "ShortTerm"
            DetectionTimestamp = DateTimeOffset(2026, 9, 12, 5, 0, 0, TimeSpan.Zero)
            ResolutionTimestamp = None
            EvidenceReferences = [ "EVD-CARRIER-STATUS-882" ]
            EvaluationConfidence = "Medium"
            ReasonCodes = [ "RC-TRANSIT-DELAY" ]
            CorrelatedExceptionIds = []
            History = []
            CreatedTimestamp = DateTimeOffset(2026, 9, 12, 5, 0, 0, TimeSpan.Zero)
            UpdatedTimestamp = DateTimeOffset(2026, 9, 12, 5, 0, 0, TimeSpan.Zero)
        }
    ]

let store: DataStore =
    {
        Id = storeId
        Fetch =
            fun (ctx: ApplicationContext) ->
                async {
                    match ctx.Resolve<SupplyExceptionQueries>() with
                    | Some queries ->
                        let! dtos = queries.GetAll() |> Async.AwaitTask
                        let records = if List.isEmpty dtos then fallbackDtos else dtos
                        let rows = records |> List.map SupplyExceptionSchema.adapter.ToUniversal
                        return Tabular rows
                    | None ->
                        match ctx.DomainServices with
                        | Some ds ->
                            let! dtos = ds.Supply.DetectSupplyExceptions.SupplyExceptionQueries.GetAll() |> Async.AwaitTask
                            let records = if List.isEmpty dtos then fallbackDtos else dtos
                            let rows = records |> List.map SupplyExceptionSchema.adapter.ToUniversal
                            return Tabular rows
                        | None ->
                            let rows = fallbackDtos |> List.map SupplyExceptionSchema.adapter.ToUniversal
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

    let explainRootCause : ActionHandler =
        fun ctx -> task {
            match ctx.TargetKey with
            | Some exceptionId ->
                return Ok (ActionExecutionOutcome.ExplainRequested(exceptionId, "SupplyException"))
            | None ->
                return Error "No supply exception selected to explain."
        }

    let confirm : ActionHandler =
        fun ctx -> task {
            match ctx.TargetKey with
            | Some exceptionId ->
                return Ok (ActionExecutionOutcome.Completed(
                    $"Supply exception '{exceptionId}' confirmed for active mitigation.",
                    [ storeId; DataStoreId.literal "core-exception-store" ]
                ))
            | None ->
                return Error "No supply exception selected to confirm."
        }

    let resolve : ActionHandler =
        fun ctx -> task {
            match ctx.TargetKey with
            | Some exceptionId ->
                return Ok (ActionExecutionOutcome.Completed(
                    $"Supply exception '{exceptionId}' marked as resolved. Buffers and operational plan rebalanced.",
                    [ storeId; DataStoreId.literal "core-exception-store"; DataStoreId.literal "supply-store" ]
                ))
            | None ->
                return Error "No supply exception selected to resolve."
        }

    let escalate : ActionHandler =
        fun ctx -> task {
            match ctx.TargetKey with
            | Some exceptionId ->
                return Ok (ActionExecutionOutcome.Completed(
                    $"Supply exception '{exceptionId}' escalated to enterprise core governance.",
                    [ storeId; DataStoreId.literal "core-exception-store" ]
                ))
            | None ->
                return Error "No supply exception selected to escalate."
        }

    let all : (ActionDefId * ActionHandler) list =
        [ SupplyExceptionActions.explainRootCause.Id, explainRootCause
          SupplyExceptionActions.confirm.Id, confirm
          SupplyExceptionActions.resolve.Id, resolve
          SupplyExceptionActions.escalate.Id, escalate ]
