module Medhavi.Client.Supply.Explanation.SupplyExplanationStudio

open System
open System.Text.Json
open System.Text.Json.Serialization
open System.Threading.Tasks
open Bolero
open Bolero.Html
open Medhavi.Contracts.Supply
open Medhavi.Client
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client.Rendering
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.ViewComposition.Context

module SupplyExplanationActions =
    let all: ActionDescriptor list = []

let storeId = DataStoreId.literal "supply-explanation-store"
let schemaId = SchemaDefId.literal "schema-supply-explanation"
let definitionId = ComponentDefId.literal "comp-supply-explanation-studio"

let schema: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Supply Explanation Schema"
      Capabilities = [ "Detail"; "Tabular" ]
      SupportedActions = []
      Actions = []
      Columns =
        [ { ColumnId = "ExplanationId"
            PropertyName = Some "ExplanationId"
            Title = "Explanation ID"
            DefaultOrder = 0
            DefaultWidthInPixels = Some 150
            DefaultVisible = true
            DefaultPinned = true
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = false
            RenderHint = None
            Alignment = Left
            CellType = TextType
            IsKey = true
            FormatString = None
            UserConfigurable = false }
          { ColumnId = "ExplainedArtifactType"
            PropertyName = Some "ExplainedArtifactType"
            Title = "Artifact Type"
            DefaultOrder = 1
            DefaultWidthInPixels = Some 140
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = true
            RenderHint = Some "StatusBadge"
            Alignment = Left
            CellType = TextType
            IsKey = false
            FormatString = None
            UserConfigurable = true }
          { ColumnId = "ExplainedArtifactId"
            PropertyName = Some "ExplainedArtifactId"
            Title = "Artifact Ref"
            DefaultOrder = 2
            DefaultWidthInPixels = Some 150
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = false
            RenderHint = None
            Alignment = Left
            CellType = TextType
            IsKey = false
            FormatString = None
            UserConfigurable = true }
          { ColumnId = "ExplainedArtifactVersion"
            PropertyName = Some "ExplainedArtifactVersion"
            Title = "Version"
            DefaultOrder = 3
            DefaultWidthInPixels = Some 90
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Number
            Filterable = false
            Groupable = false
            RenderHint = None
            Alignment = Right
            CellType = NumberType
            IsKey = false
            FormatString = Some "N0"
            UserConfigurable = true }
          { ColumnId = "Confidence"
            PropertyName = Some "Confidence"
            Title = "Confidence"
            DefaultOrder = 4
            DefaultWidthInPixels = Some 120
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Number
            Filterable = true
            Groupable = false
            RenderHint = None
            Alignment = Right
            CellType = NumberType
            IsKey = false
            FormatString = Some "P0"
            UserConfigurable = true } ]
      DefaultSort = None
      DefaultFilters = [] }

let definition: SchemaDefinition = schema

let adapter: ProjectionAdapter<SupplyExplanationDto> =
    { ToUniversal =
        fun (dto: SupplyExplanationDto) ->
            { Key = dto.ExplanationId
              Cells =
                Map.ofList
                    [ "ExplanationId", TextValue dto.ExplanationId
                      "ExplainedArtifactType", TextValue dto.ExplainedArtifactType
                      "ExplainedArtifactId", TextValue dto.ExplainedArtifactId
                      "ExplainedArtifactVersion", NumberValue (decimal dto.ExplainedArtifactVersion)
                      "Confidence", NumberValue dto.Confidence.CompositeIndex ] } }

let componentDefinition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "AI Causal Explanation Studio"
      Icon = "psychology"
      Category = ComponentCategory.Visualization
      RequiredSchemaCapabilities = [ "Detail" ]
      DefaultGridSpan = GridSpan.literal 12 7
      DefaultSchemaId = Some schemaId
      DefaultStoreId = Some storeId
      IsResizable = true
      IsDraggable = true
      SelectionMode = SelectionMode.SingleSelection }

let private jsonOptions =
    let opts = JsonSerializerOptions()
    opts.Converters.Add(JsonFSharpConverter())
    opts

let tryDeserializeExplanation (json: string) : SupplyExplanationDto option =
    try
        Some (JsonSerializer.Deserialize<SupplyExplanationDto>(json, jsonOptions))
    with _ ->
        None

let reconstructFromRowCells (row: UniversalRow) : SupplyExplanationDto option =
    let getCellText name =
        Map.tryFind name row.Cells
        |> Option.bind (function
            | TextValue s -> Some s
            | _ -> None)

    let getCellNumber name =
        Map.tryFind name row.Cells
        |> Option.bind (function
            | NumberValue d -> Some d
            | _ -> None)

    match getCellText "ExplainedArtifactId" |> Option.orElse (Some row.Key) with
    | Some artifactId when not (String.IsNullOrWhiteSpace artifactId) ->
        let artifactType = getCellText "ExplainedArtifactType" |> Option.defaultValue "SupplyArtifact"
        let version = getCellNumber "ExplainedArtifactVersion" |> Option.map int64 |> Option.defaultValue 1L
        let confidenceVal = getCellNumber "Confidence" |> Option.defaultValue 0.95m
        let summary = getCellText "PlannerSummary" |> Option.defaultValue (sprintf "Causal trace for %s (%s)." artifactId artifactType)

        Some
            { ExplanationId = row.Key
              ExplainedArtifactType = artifactType
              ExplainedArtifactId = artifactId
              ExplainedArtifactVersion = version
              StructuredReasoningGraph =
                { Nodes =
                    [ { NodeId = artifactId
                        NodeType = artifactType
                        Label = sprintf "%s (%s)" artifactId artifactType
                        Properties = Map.ofList [ ("EntityId", artifactId); ("Type", artifactType) ]
                        IsHypothetical = false } ]
                  Edges = [] }
              MultiLevelRenderings =
                { PlannerSummary = summary
                  FactorWaterfallMarkdown = "| Driver | Impact |\n|---|---|\n| Operational Telemetry | Nominal |"
                  FullAuditJson = "{}"
                  AiConsumableJsonLd = "{}" }
              FactorContributions =
                [ { FactorName = "Operational Telemetry"
                    BaseValue = 100m
                    ImpactValue = 0m
                    PercentageContribution = 0m
                    Direction = "Nominal"
                    Confidence = confidenceVal } ]
              PreservedEvidenceRefs = []
              RationaleBlocks = []
              IsNegativeExplanation = false
              NegativeExplanation = None
              OpaqueModelProvenance = None
              Confidence =
                { InheritedEvidenceConfidence = confidenceVal
                  EvidenceCompleteness = confidenceVal
                  EvidenceConsistency = confidenceVal
                  CompositeIndex = confidenceVal
                  ConfidenceLevel = if confidenceVal >= 0.9m then "High" elif confidenceVal >= 0.7m then "Medium" else "Low" }
              TemplateVersionReference = "v1.0"
              CompositionTimestamp = DateTimeOffset.UtcNow }
    | _ -> None

let tryExtractExplanation (row: UniversalRow) : SupplyExplanationDto option =
    match Map.tryFind "ExplanationJson" row.Cells with
    | Some (TextValue json) ->
        match tryDeserializeExplanation json with
        | Some dto -> Some dto
        | None -> reconstructFromRowCells row
    | _ ->
        reconstructFromRowCells row

let toUniversal (exp: SupplyExplanationDto) : UniversalRow =
    let serialized = JsonSerializer.Serialize(exp, jsonOptions)
    { Key = exp.ExplanationId
      Cells =
        Map.ofList
            [ "ExplanationId", TextValue exp.ExplanationId
              "ExplainedArtifactType", TextValue exp.ExplainedArtifactType
              "ExplainedArtifactId", TextValue exp.ExplainedArtifactId
              "ExplainedArtifactVersion", NumberValue (decimal exp.ExplainedArtifactVersion)
              "Confidence", NumberValue exp.Confidence.CompositeIndex
              "PlannerSummary", TextValue exp.MultiLevelRenderings.PlannerSummary
              "ExplanationJson", TextValue serialized ] }

let queryMatchingSelection (queries: SupplyExplanationQueries) (selection: string option) : Task<SupplyExplanationDto option> =
    task {
        match selection with
        | Some sel ->
            let! byId = queries.GetById sel
            match byId with
            | Some dto -> return Some dto
            | None ->
                let! matching =
                    queries.Filter (fun d ->
                        d.ExplainedArtifactId.Equals(sel, StringComparison.OrdinalIgnoreCase)
                        || d.ExplanationId.Equals(sel, StringComparison.OrdinalIgnoreCase))
                return List.tryHead matching
        | None ->
            let! all = queries.GetAll()
            return List.tryHead all
    }

let fetchExplanation (ctx: ApplicationContext) (selection: string option) : Async<SupplyExplanationDto option> =
    async {
        let queries = ctx.Domain.Supply.ExplainSupplyDecisions.SupplyExplanationQueries
        let! dtoOpt = queryMatchingSelection queries selection |> Async.AwaitTask
        return dtoOpt
    }

let resolveExplanation (context: ComponentRenderContext) : SupplyExplanationDto option =
    match context.Data with
    | Tabular rows ->
        let matchingRow =
            match context.Selection with
            | Some selKey ->
                rows
                |> List.tryFind (fun r ->
                    r.Key.Equals(selKey, StringComparison.OrdinalIgnoreCase)
                    || match Map.tryFind "ExplainedArtifactId" r.Cells with
                       | Some (TextValue id) -> id.Equals(selKey, StringComparison.OrdinalIgnoreCase)
                       | _ -> false
                    || match Map.tryFind "ExplanationId" r.Cells with
                       | Some (TextValue id) -> id.Equals(selKey, StringComparison.OrdinalIgnoreCase)
                       | _ -> false)
                |> Option.orElse (List.tryHead rows)
            | None ->
                List.tryHead rows

        matchingRow |> Option.bind tryExtractExplanation
    | NoData ->
        None

let private renderEmptyState () : Node =
    div {
        attr.style
            "display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100%; min-height: 280px; gap: 14px; padding: 24px; box-sizing: border-box; text-align: center; font-family: Segoe UI, sans-serif; background: var(--rz-base-background-color);"

        div {
            attr.style
                "width: 56px; height: 56px; border-radius: 50%; background: linear-gradient(135deg, rgba(37,99,235,0.08), rgba(139,92,246,0.08)); border: 1px solid rgba(37,99,235,0.2); display: flex; align-items: center; justify-content: center;"
            span {
                attr.``class`` "rzi"
                attr.style "font-size: 28px; color: var(--rz-primary);"
                "psychology"
            }
        }

        div {
            div {
                attr.style "font-size: 15px; font-weight: 700; color: var(--rz-text-color); margin-bottom: 4px;"
                "No Supply Explanation Loaded"
            }
            div {
                attr.style "font-size: 12px; color: var(--rz-text-secondary-color); max-width: 420px; line-height: 1.5;"
                "Select a supply sensory record, capacity exception, or demand commitment from the active workspace to inspect its governed causal reasoning graph."
            }
        }
    }

let renderExplanation (expl: SupplyExplanationDto) (dispatch: ComponentEvent -> unit) : Node =
    let selectedNodeId = None

    let governedRule =
        expl.RationaleBlocks
        |> List.tryHead
        |> Option.map (fun rb -> sprintf "Governed: %s" rb.CausalRuleRef)
        |> Option.defaultValue "Governed Decision"

    let prescriptiveNodes =
        expl.StructuredReasoningGraph.Nodes
        |> List.filter (fun n -> n.NodeType = "PrescriptiveRecommendation" || n.IsHypothetical)

    div {
        attr.style
            "display: flex; flex-direction: column; gap: 14px; height: 100%; box-sizing: border-box; overflow-y: auto; padding: 12px; font-family: Segoe UI, sans-serif;"

        // Header Banner
        div {
            attr.style
                "display: flex; justify-content: space-between; align-items: center; background: linear-gradient(135deg, rgba(37,99,235,0.06), rgba(139,92,246,0.06)); border: 1px solid rgba(37,99,235,0.2); border-radius: 8px; padding: 10px 16px;"

            div {
                attr.style "display: flex; align-items: center; gap: 10px;"

                span {
                    attr.``class`` "rzi"
                    attr.style "font-size: 22px; color: var(--rz-primary);"
                    "psychology"
                }

                div {
                    div {
                        attr.style "font-size: 14px; font-weight: 700; color: var(--rz-text-color);"
                        sprintf "Causal Explanation: %s" expl.ExplainedArtifactId
                    }

                    div {
                        attr.style "font-size: 11px; color: var(--rz-text-secondary-color);"

                        sprintf
                            "Artifact: %s (v%d) · Composed: %s"
                            expl.ExplainedArtifactType
                            expl.ExplainedArtifactVersion
                            (expl.CompositionTimestamp.ToString("yyyy-MM-dd HH:mm"))
                    }
                }
            }

            div {
                attr.style "display: flex; gap: 8px;"
                span {
                    attr.style
                        "background: var(--rz-success-lighter); color: var(--rz-success); border: 1px solid var(--rz-success-light); border-radius: 12px; padding: 3px 10px; font-size: 11px; font-weight: 600;"

                    sprintf
                        "Confidence: %.0f%% (%s)"
                        (float expl.Confidence.CompositeIndex * 100.0)
                        expl.Confidence.ConfidenceLevel
                }
                span {
                    attr.style
                        "background: var(--rz-primary-lighter); color: var(--rz-primary); border: 1px solid var(--rz-primary-light); border-radius: 12px; padding: 3px 10px; font-size: 11px; font-weight: 600;"

                    governedRule
                }
            }
        }

        // Planner Summary Box
        div {
            attr.style
                "background: var(--rz-base-background-color); border: 1px solid var(--rz-border-color); border-left: 4px solid var(--rz-primary); border-radius: 6px; padding: 10px 14px; font-size: 12px; line-height: 1.5; color: var(--rz-text-color);"

            expl.MultiLevelRenderings.PlannerSummary
        }

        // Middle Section: DAG and Waterfall
        div {
            attr.style "display: grid; grid-template-columns: 2fr 1fr; gap: 12px;"

            div { SupplyReasoningGraph.renderDag expl.StructuredReasoningGraph selectedNodeId (fun _ -> ()) }
            div { SupplyReasoningGraph.renderWaterfall expl.FactorContributions }
        }

        // Lower Section: 3-column analysis
        div {
            attr.style "display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 12px;"

            // 1. Governed Rationale Blocks (BR-S-233)
            div {
                attr.style
                    "background: var(--rz-base-background-color); border: 1px solid var(--rz-border-color); border-radius: 8px; padding: 12px; box-sizing: border-box;"

                div {
                    attr.style
                        "font-size: 12px; font-weight: 700; color: var(--rz-text-color); margin-bottom: 8px; display: flex; align-items: center; gap: 6px;"

                    span {
                        attr.``class`` "rzi"
                        attr.style "color: var(--rz-primary);"
                        "gavel"
                    }

                    span { "Governed Rationale (BR-S-233)" }
                }

                if List.isEmpty expl.RationaleBlocks then
                    div {
                        attr.style "font-size: 11px; color: var(--rz-text-secondary-color);"
                        "No specific governed rationale blocks recorded for this decision."
                    }
                else
                    forEach expl.RationaleBlocks (fun rb ->
                        div {
                            attr.style
                                "font-size: 11px; display: flex; flex-direction: column; gap: 4px; border: 1px solid rgba(0,0,0,0.06); border-radius: 6px; padding: 8px; background: rgba(0,0,0,0.01); margin-bottom: 6px;"

                            div {
                                attr.style "display: flex; justify-content: space-between;"

                                span {
                                    attr.style "font-weight: 600; color: var(--rz-primary);"
                                    rb.CausalRuleRef
                                }

                                span {
                                    attr.style "color: var(--rz-text-secondary-color); font-size: 10px;"
                                    rb.OutcomeDimension
                                }
                            }

                            div {
                                attr.style "color: var(--rz-text-color); line-height: 1.4;"
                                rb.GovernedNarrative
                            }

                            div {
                                attr.style "font-size: 10px; color: var(--rz-text-secondary-color);"
                                sprintf "Evidence Ref: %s" rb.InputEvidenceRef
                            }
                        })
            }

            // 2. Negative Explanations (Discarded Alternatives)
            div {
                attr.style
                    "background: var(--rz-base-background-color); border: 1px solid var(--rz-border-color); border-radius: 8px; padding: 12px; box-sizing: border-box;"

                div {
                    attr.style
                        "font-size: 12px; font-weight: 700; color: var(--rz-text-color); margin-bottom: 8px; display: flex; align-items: center; gap: 6px;"

                    span {
                        attr.``class`` "rzi"
                        attr.style "color: var(--rz-warning);"
                        "cancel"
                    }

                    span { "Negative Explanations (BA-S-090)" }
                }

                match expl.NegativeExplanation with
                | Some neg ->
                    div {
                        attr.style
                            "font-size: 11px; display: flex; flex-direction: column; gap: 4px; border: 1px solid var(--rz-warning-light); border-radius: 6px; padding: 8px; background: var(--rz-warning-lighter);"

                        div {
                            attr.style "display: flex; justify-content: space-between;"

                            span {
                                attr.style "font-weight: 600; color: var(--rz-warning-dark);"
                                neg.DiscardedAlternativeId
                            }

                            span {
                                attr.style "color: var(--rz-warning-dark); font-size: 10px; font-weight: 600;"
                                neg.ReasonCategory
                            }
                        }

                        div {
                            attr.style "color: var(--rz-text-color); line-height: 1.4;"
                            neg.FormalReason
                        }

                        if not (Map.isEmpty neg.EvaluationContext) then
                            div {
                                attr.style "font-size: 10px; color: var(--rz-text-secondary-color); margin-top: 4px;"
                                neg.EvaluationContext
                                |> Map.toList
                                |> List.map (fun (k, v) -> sprintf "%s: %s" k v)
                                |> String.concat " · "
                            }
                    }
                | None ->
                    div {
                        attr.style "font-size: 11px; color: var(--rz-text-secondary-color);"
                        "No alternatives discarded for this decision."
                    }
            }

            // 3. Prescriptive Actions
            div {
                attr.style
                    "background: var(--rz-base-background-color); border: 1px solid var(--rz-border-color); border-radius: 8px; padding: 12px; box-sizing: border-box;"

                div {
                    attr.style
                        "font-size: 12px; font-weight: 700; color: var(--rz-text-color); margin-bottom: 8px; display: flex; align-items: center; gap: 6px;"

                    span {
                        attr.``class`` "rzi"
                        attr.style "color: var(--rz-success);"
                        "bolt"
                    }

                    span { "Prescriptive Mitigations" }
                }

                div {
                    attr.style "display: flex; flex-direction: column; gap: 6px;"

                    if not (List.isEmpty prescriptiveNodes) then
                        forEach prescriptiveNodes (fun node ->
                            let buttonLabel =
                                Map.tryFind "ButtonLabel" node.Properties
                                |> Option.defaultValue node.Label
                            let buttonIcon =
                                Map.tryFind "ButtonIcon" node.Properties
                                |> Option.defaultValue "bolt"
                            let actionCode =
                                Map.tryFind "ActionCode" node.Properties
                                |> Option.defaultValue "supply.mitigate"

                            button {
                                attr.style
                                    "background: var(--rz-primary); color: var(--rz-on-primary); border: none; border-radius: 6px; padding: 8px 12px; font-size: 11px; font-weight: 600; cursor: pointer; display: flex; align-items: center; justify-content: space-between;"
                                on.click (fun _ ->
                                    dispatch (ActionRequested(ActionDefId.literal actionCode, Some expl.ExplainedArtifactId, NoPayload)))

                                span { buttonLabel }

                                span {
                                    attr.``class`` "rzi"
                                    buttonIcon
                                }
                            })
                    else
                        button {
                            attr.style
                                "background: var(--rz-primary); color: var(--rz-on-primary); border: none; border-radius: 6px; padding: 8px 12px; font-size: 11px; font-weight: 600; cursor: pointer; display: flex; align-items: center; justify-content: space-between;"
                            on.click (fun _ ->
                                dispatch (ActionRequested(ActionDefId.literal "supply.acknowledge", Some expl.ExplainedArtifactId, NoPayload)))

                            span { "Acknowledge Governed Decision" }

                            span {
                                attr.``class`` "rzi"
                                "check"
                            }
                        }

                        button {
                            attr.style
                                "background: var(--rz-danger-lighter); color: var(--rz-danger); border: 1px solid var(--rz-danger-light); border-radius: 6px; padding: 7px 12px; font-size: 11px; font-weight: 600; cursor: pointer; display: flex; align-items: center; justify-content: space-between;"
                            on.click (fun _ ->
                                dispatch (ActionRequested(ActionDefId.literal "supply.escalate", Some expl.ExplainedArtifactId, NoPayload)))

                            span { "Escalate for Exception Review" }

                            span {
                                attr.``class`` "rzi"
                                "arrow_upward"
                            }
                        }
                }
            }
        }
    }

let render (context: ComponentRenderContext) (dispatch: ComponentEvent -> unit) : Node =
    match resolveExplanation context with
    | Some expl -> renderExplanation expl dispatch
    | None -> renderEmptyState ()

let renderWithExplanation (explOpt: SupplyExplanationDto option) (dispatch: ComponentEvent -> unit) : Node =
    match explOpt with
    | Some expl -> renderExplanation expl dispatch
    | None -> renderEmptyState ()

let runtime: ComponentRuntime = { Render = render }

let registration: ComponentRegistration =
    { Definition = componentDefinition
      Schema = schema
      Runtime = runtime }
