module Medhavi.Client.Promise.Risk.PromiseRiskGrid

open System
open Medhavi.Contracts.Promise
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.ViewComposition.Context
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client.Rendering
open Medhavi.Common.Validation
open Medhavi.Client.Components.Shared

module PromiseRiskActions =

    let assessRisk: ActionDescriptor =
        { Id = ActionDefId.literal "promise_risk.assess"
          DisplayName = "Evaluate Longitudinal Risk"
          Icon = "warning"
          Scope = GlobalScope
          ExecutionMode = Direct
          RequiredRoles = [ "Planner"; "RiskAnalyst" ]
          IsPrimary = true }

    let escalate: ActionDescriptor =
        { Id = ActionDefId.literal "promise_risk.escalate"
          DisplayName = "Escalate to Disruption Triage"
          Icon = "crisis_alert"
          Scope = SingleEntityScope
          ExecutionMode = Parameterized("escalate-risk-form", "Escalate Imminent Delivery Breach")
          RequiredRoles = [ "Planner"; "CustomerServiceLead" ]
          IsPrimary = false }

    let explainRisk: ActionDescriptor =
        { Id = ActionDefId.literal "promise_risk.explain_drivers"
          DisplayName = "Causal Risk Explanation"
          Icon = "account_tree"
          Scope = SingleEntityScope
          ExecutionMode = Direct
          RequiredRoles = [ "Planner"; "Executive" ]
          IsPrimary = false }

    let all: ActionDescriptor list = [ assessRisk; escalate; explainRisk ]

let schemaId = SchemaDefId.literal "promise-risk-schema"
let storeId = DataStoreId.literal "promise-risk-store"
let definitionId = ComponentDefId.literal "promise-risk-grid"

let schema: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Promise Risk & Disruption Telemetry (SE-P-060)"
      Capabilities = [ "Tabular" ]
      SupportedActions = PromiseRiskActions.all |> List.map (fun a -> a.Id)
      Actions = PromiseRiskActions.all
      Columns =
        [ { ColumnId = "AssessmentId"
            PropertyName = Some "AssessmentId"
            Title = "Assessment ID"
            DefaultOrder = 0
            DefaultWidthInPixels = Some 160
            DefaultVisible = true
            DefaultPinned = true
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = None
            Alignment = Left
            CellType = TextType
            IsKey = true
            FormatString = None }
          { ColumnId = "CommitmentId"
            PropertyName = Some "CommitmentId"
            Title = "Commitment ID"
            DefaultOrder = 1
            DefaultWidthInPixels = Some 150
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = None
            Alignment = Left
            CellType = TextType
            IsKey = false
            FormatString = None }
          { ColumnId = "OrderLineId"
            PropertyName = Some "OrderLineId"
            Title = "Order Line"
            DefaultOrder = 2
            DefaultWidthInPixels = Some 140
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = None
            Alignment = Left
            CellType = TextType
            IsKey = false
            FormatString = None }
          { ColumnId = "Item"
            PropertyName = Some "Item"
            Title = "Item SKU"
            DefaultOrder = 3
            DefaultWidthInPixels = Some 140
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = true
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = None
            Alignment = Left
            CellType = TextType
            IsKey = false
            FormatString = None }
          { ColumnId = "SeverityTier"
            PropertyName = Some "SeverityTier"
            Title = "Severity"
            DefaultOrder = 4
            DefaultWidthInPixels = Some 110
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = true
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = Some "StatusBadge"
            Alignment = Center
            CellType = TextType
            IsKey = false
            FormatString = None }
          { ColumnId = "BreachProbabilityPct"
            PropertyName = Some "BreachProbabilityPct"
            Title = "Breach %"
            DefaultOrder = 5
            DefaultWidthInPixels = Some 110
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Number
            UserConfigurable = true
            RenderHint = None
            Alignment = Right
            CellType = NumberType
            IsKey = false
            FormatString = Some "0.0%" }
          { ColumnId = "ProjectedLateDays"
            PropertyName = Some "ProjectedLateDays"
            Title = "Late (Days)"
            DefaultOrder = 6
            DefaultWidthInPixels = Some 110
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Number
            UserConfigurable = true
            RenderHint = None
            Alignment = Right
            CellType = NumberType
            IsKey = false
            FormatString = Some "0.0" }
          { ColumnId = "BufferConsumptionRatio"
            PropertyName = Some "BufferConsumptionRatio"
            Title = "Buffer Used"
            DefaultOrder = 7
            DefaultWidthInPixels = Some 110
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Number
            UserConfigurable = true
            RenderHint = None
            Alignment = Right
            CellType = NumberType
            IsKey = false
            FormatString = Some "0.0%" }
          { ColumnId = "AssessmentTime"
            PropertyName = Some "AssessmentTime"
            Title = "Assessed At"
            DefaultOrder = 8
            DefaultWidthInPixels = Some 150
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some FilterValueKind.DateTimeOffset
            UserConfigurable = true
            RenderHint = None
            Alignment = Left
            CellType = DateTimeType
            IsKey = false
            FormatString = Some "yyyy-MM-dd HH:mm" }
          { ColumnId = "LifecycleState"
            PropertyName = Some "LifecycleState"
            Title = "State"
            DefaultOrder = 9
            DefaultWidthInPixels = Some 120
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = true
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = Some "StatusBadge"
            Alignment = Center
            CellType = TextType
            IsKey = false
            FormatString = None }
          { ColumnId = "DecisionTraceability"
            PropertyName = Some "DecisionTraceability"
            Title = "Decision Trace"
            DefaultOrder = 10
            DefaultWidthInPixels = Some 140
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = None
            Alignment = Left
            CellType = TextType
            IsKey = false
            FormatString = None } ]
      DefaultSort =
        Some
            { Field = "BreachProbabilityPct"
              Direction = Descending
              Priority = 1 }
      DefaultFilters = [] }

let definition: SchemaDefinition = schema

let adapter: ProjectionAdapter<PromiseRiskAssessmentDto> =
    match ProjectionCompiler.compile<PromiseRiskAssessmentDto> schema with
    | Valid a -> a
    | Invalid errs ->
        let msg = errs |> String.concat "; "
        invalidOp $"PromiseRiskGrid compilation failed: {msg}"

let componentDefinition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Promise Risk Assessments"
      Icon = "warning"
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
    { Definition = componentDefinition
      Schema = schema
      Runtime = runtime }

