module Medhavi.Client.Demand.Understanding.DomainHealthRadar

open System
open Bolero
open Bolero.Html
open Medhavi.Contracts.Demand
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.ViewComposition.Context
open Medhavi.Client.Registries
open Medhavi.Client.Rendering
open Medhavi.Client.Controls
open Medhavi.Common.Validation
open Medhavi.Client.Components.Shared

/// Declarative metadata descriptors for Demand Understanding & Domain Health actions.
module DomainHealthActions =

    let revise: ActionDescriptor =
        {
            Id = ActionDefId.literal "demand_understanding.revise"
            DisplayName = "Revise Understanding"
            Icon = "refresh"
            Scope = SingleEntityScope
            ExecutionMode = Direct
            RequiredRoles = [ "Planner"; "DemandAnalyst" ]
            IsPrimary = true
        }

    let publish: ActionDescriptor =
        {
            Id = ActionDefId.literal "demand_understanding.publish"
            DisplayName = "Publish Understanding"
            Icon = "publish"
            Scope = SingleEntityScope
            ExecutionMode =
                RequiresConfirmation "Publish this demand understanding assessment to the enterprise ledger?"
            RequiredRoles = [ "Planner"; "LeadPlanner" ]
            IsPrimary = false
        }

    let explain: ActionDescriptor =
        {
            Id = ActionDefId.literal "demand_understanding.explain"
            DisplayName = "Explain Understanding"
            Icon = "psychology"
            Scope = SingleEntityScope
            ExecutionMode = Direct
            RequiredRoles = [ "Planner"; "Viewer" ]
            IsPrimary = false
        }

    let all: ActionDescriptor list = [ revise; publish; explain ]

let schemaId = SchemaDefId.literal "demand-understanding-health-schema"
let storeId = DataStoreId.literal "demo-domain-health-store"
let definitionId = ComponentDefId.literal "domain-health-radar"

let schema: SchemaDefinition =
    {
        Id = schemaId
        DisplayName = "Demand Understanding Domain Health"
        Capabilities = [ "Radar"; "Scorecard"; "Tabular" ]
        SupportedActions = DomainHealthActions.all |> List.map(fun a -> a.Id)
        Actions = DomainHealthActions.all
        Columns =
            [
                {
                    ColumnId = "PlanningScopeId"
                    PropertyName = Some "PlanningScopeId"
                    Title = "Planning Scope"
                    DefaultOrder = 0
                    DefaultWidthInPixels = Some 160
                    DefaultVisible = true
                    DefaultPinned = true
                    Sortable = true
                    Filterable = true
                    Groupable = true
                    FilterValueKind = Some Text
                    UserConfigurable = true
                    RenderHint = None
                    Alignment = Left
                    CellType = TextType
                    IsKey = true
                    FormatString = None
                }
                {
                    ColumnId = "VersionNumber"
                    PropertyName = Some "VersionNumber"
                    Title = "Version"
                    DefaultOrder = 1
                    DefaultWidthInPixels = Some 90
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
                    FormatString = Some "N0"
                }
                {
                    ColumnId = "State"
                    PropertyName = Some "State"
                    Title = "Status"
                    DefaultOrder = 2
                    DefaultWidthInPixels = Some 120
                    DefaultVisible = true
                    DefaultPinned = false
                    Sortable = true
                    Filterable = true
                    Groupable = true
                    FilterValueKind = Some Text
                    UserConfigurable = true
                    RenderHint = Some "StatusBadge"
                    Alignment = Left
                    CellType = TextType
                    IsKey = false
                    FormatString = None
                }
                {
                    ColumnId = "LastPublishedTime"
                    PropertyName = Some "LastPublishedTime"
                    Title = "Last Published"
                    DefaultOrder = 3
                    DefaultWidthInPixels = Some 160
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
                    FormatString = Some "yyyy-MM-dd HH:mm"
                }
                {
                    ColumnId = "Interpretation"
                    PropertyName = Some "Interpretation"
                    Title = "Interpretation Assessment"
                    DefaultOrder = 4
                    DefaultWidthInPixels = Some 220
                    DefaultVisible = true
                    DefaultPinned = false
                    Sortable = false
                    Filterable = false
                    Groupable = false
                    FilterValueKind = Some Text
                    UserConfigurable = true
                    RenderHint = None
                    Alignment = Left
                    CellType = TextType
                    IsKey = false
                    FormatString = None
                }
            ]
        DefaultSort =
            Some
                {
                    Field = "PlanningScopeId"
                    Direction = Ascending
                    Priority = 1
                }
        DefaultFilters = []
    }

let definition: SchemaDefinition = schema

let adapter: ProjectionAdapter<DemandUnderstandingDto> =
    match ProjectionCompiler.compile<DemandUnderstandingDto> schema with
    | Valid a -> a
    | Invalid errs ->
        let msg = errs |> String.concat "; "
        invalidOp $"DomainHealthRadar compilation failed: {msg}"

let componentDefinition: ComponentDefinition =
    {
        Id = definitionId
        DisplayName = "APS Domain Intelligence Radar"
        Icon = "radar"
        Category = ComponentCategory.Visualization
        RequiredSchemaCapabilities = [ "Radar"; "Scorecard" ]
        DefaultGridSpan = GridSpan.literal 6 5
        DefaultSchemaId = Some schemaId
        DefaultStoreId = Some storeId
        IsResizable = true
        IsDraggable = true
        SelectionMode = SelectionMode.SingleSelection
    }

type DomainHealthItem =
    {
        DomainName: string
        Icon: string
        ScorePct: float
        StatusText: string
        Health: KpiHealth
        KeyMetric: string
        SubMetric: string
        ActionLabel: string
        EntityKey: string
    }

let private healthBadgeStyle (health: KpiHealth) =
    match health with
    | Healthy ->
        "color: var(--rz-success); background: var(--rz-success-lighter); border: 1px solid var(--rz-success-light);"
    | Warning ->
        "color: var(--rz-warning-dark); background: var(--rz-warning-lighter); border: 1px solid var(--rz-warning-light);"
    | Critical ->
        "color: var(--rz-danger); background: var(--rz-danger-lighter); border: 1px solid var(--rz-danger-light);"
    | Neutral -> "color: var(--rz-info); background: var(--rz-info-lighter); border: 1px solid var(--rz-info-light);"

let private progressBarColor (health: KpiHealth) =
    match health with
    | Healthy -> "var(--rz-success)"
    | Warning -> "var(--rz-warning)"
    | Critical -> "var(--rz-danger)"
    | Neutral -> "var(--rz-info)"

let private mapRowToItem (row: UniversalRow) : DomainHealthItem =
    let getCellText colId fallback =
        match Map.tryFind colId row.Cells with
        | Some(TextValue s) -> s
        | Some(NumberValue n) -> string n
        | _ -> fallback

    let scopeId = getCellText "PlanningScopeId" row.Key
    let state = getCellText "State" "Active"
    let version = getCellText "VersionNumber" "1"

    let healthStatus = getCellText "HealthStatus" ""
    let pattern = getCellText "PatternStatus" "Normal"
    let confidence = getCellText "PatternConfidence" "Medium"
    let continuity = getCellText "ContinuityStatus" "Stable"
    let volatility = getCellText "VolatilityLevel" "Low"

    let health =
        if not(String.IsNullOrEmpty healthStatus) then
            if healthStatus.Contains("Critical") then
                Critical
            elif healthStatus.Contains("At Risk") || healthStatus.Contains("AtRisk") then
                Warning
            elif healthStatus.Contains("Healthy") then
                Healthy
            else
                Neutral
        else
            match state.ToLowerInvariant() with
            | "published" -> Healthy
            | "draft" -> Warning
            | "superseded" -> Neutral
            | _ -> Neutral

    let scorePct =
        match Map.tryFind "ScorePct" row.Cells with
        | Some(NumberValue n) -> float n
        | _ ->
            // Fallback derivation based on health category and state
            match health with
            | Healthy -> 95.0
            | Warning -> 65.0
            | Critical -> 25.0
            | Neutral -> 50.0

    let statusText =
        if not(String.IsNullOrEmpty healthStatus) then
            $"{state} · {healthStatus}"
        else
            state

    {
        DomainName = $"Demand Scope: {scopeId}"
        Icon = "insights"
        ScorePct = scorePct
        StatusText = statusText
        Health = health
        KeyMetric = $"v{version} · Pattern: {pattern} ({confidence} conf)"
        SubMetric = $"Continuity: {continuity} · Volatility: {volatility}"
        ActionLabel = "Revise Assessment"
        EntityKey = row.Key
    }

let render (context: ComponentRenderContext) (dispatch: ComponentEvent -> unit) : Node =
    match context.Data with
    | Tabular rows when not rows.IsEmpty ->
        let items = rows |> List.map mapRowToItem

        div {
            attr.style
                """
                background: var(--rz-base-background-color);
                border: 1px solid var(--rz-border-color);
                border-radius: 8px;
                padding: 1rem 1.25rem;
                display: flex;
                flex-direction: column;
                height: 100%;
                box-sizing: border-box;
                overflow-y: auto;
                box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
                """

            div {
                attr.style "display: flex; flex-direction: column; gap: 0.75rem; flex: 1;"

                forEach items (fun item ->
                    let isSelected = context.Selection = Some item.EntityKey

                    div {
                        attr.style(
                            if isSelected then
                                "display: flex; flex-direction: column; gap: 0.25rem; padding: 0.5rem 0.6rem; border-radius: 6px; background: var(--rz-primary-lighter); border: 1px solid var(--rz-primary-light); cursor: pointer;"
                            else
                                "display: flex; flex-direction: column; gap: 0.25rem; padding: 0.5rem 0.6rem; border-radius: 6px; background: rgba(0,0,0,0.02); cursor: pointer;"
                        )

                        on.click(fun _ -> dispatch(SelectionChanged(Some item.EntityKey)))

                        div {
                            attr.style "display: flex; align-items: center; justify-content: space-between;"

                            div {
                                attr.style "display: flex; align-items: center; gap: 0.4rem;"

                                span {
                                    attr.``class`` "rzi"
                                    attr.style "font-size: 1rem; color: var(--rz-text-secondary-color);"
                                    item.Icon
                                }

                                span {
                                    attr.style "font-size: 13px; font-weight: 600; color: var(--rz-text-color);"
                                    item.DomainName
                                }
                            }

                            div {
                                attr.style "display: flex; align-items: center; gap: 0.5rem;"

                                span {
                                    attr.style "font-size: 12px; font-weight: 700; color: var(--rz-text-color);"
                                    sprintf "%.1f%%" item.ScorePct
                                }

                                span {
                                    attr.style
                                        $"font-size: 11px; font-weight: 600; padding: 2px 6px; border-radius: 4px; {healthBadgeStyle item.Health}"

                                    item.StatusText
                                }
                            }
                        }

                        div {
                            attr.style
                                "width: 100%; height: 4px; background: var(--rz-border-color); border-radius: 2px; overflow: hidden;"

                            div {
                                attr.style
                                    $"""
                                    width: {item.ScorePct}%%;
                                    height: 100%%;
                                    background: {progressBarColor item.Health};
                                    border-radius: 2px;
                                    """
                            }
                        }

                        div {
                            attr.style
                                "display: flex; justify-content: space-between; font-size: 11px; color: var(--rz-text-secondary-color); margin-top: 1px;"

                            span { item.KeyMetric + " · " + item.SubMetric }

                            span {
                                attr.style "font-weight: 600; color: var(--rz-primary); cursor: pointer;"
                                item.ActionLabel
                            }
                        }
                    })
            }
        }
    | _ ->
        div {
            attr.style
                """
                background: var(--rz-base-background-color);
                border: 1px dashed var(--rz-border-color);
                border-radius: 8px;
                padding: 2rem;
                display: flex;
                flex-direction: column;
                align-items: center;
                justify-content: center;
                height: 100%;
                box-sizing: border-box;
                text-align: center;
                gap: 0.75rem;
                color: var(--rz-text-secondary-color);
                """

            span {
                attr.``class`` "rzi"
                attr.style "font-size: 2.5rem; color: var(--rz-text-disabled-color);"
                "radar"
            }

            span {
                attr.style "font-size: 14px; font-weight: 600; color: var(--rz-text-color);"
                "No active demand understanding assessment published for current scope."
            }

            span {
                attr.style "font-size: 12px; color: var(--rz-text-secondary-color);"
                "Execute 'Revise Understanding' to trigger demand sensing and baseline synthesis."
            }
        }

let runtime: ComponentRuntime = { Render = render }

let registration: ComponentRegistration =
    {
        Definition = componentDefinition
        Schema = schema
        Runtime = runtime
    }
