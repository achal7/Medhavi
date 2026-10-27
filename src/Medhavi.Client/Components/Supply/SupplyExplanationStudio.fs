module Medhavi.Client.Components.Supply.SupplyExplanationStudio

open System
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
open Medhavi.Client.ViewComposition.Data

let definitionId = ComponentDefId.literal "comp-supply-explanation-studio"
let storeId = DataStoreId.literal "supply-explanation-store"

let definition: ComponentDefinition =
    {
        Id = definitionId
        DisplayName = "AI Causal Explanation Studio"
        Icon = "psychology"
        Category = ComponentCategory.Visualization
        RequiredSchemaCapabilities = [ "Detail" ]
        DefaultGridSpan = GridSpan.literal 12 7
        DefaultSchemaId = Some SupplyExplanationSchema.schemaId
        DefaultStoreId = Some storeId
        IsResizable = true
        IsDraggable = true
        SelectionMode = SelectionMode.SingleSelection
    }

let fallbackExplanation: SupplyExplanationDto =
    {
        ExplanationId = "EXP-CAP-2026-001"
        ExplainedArtifactType = "SupplyException"
        ExplainedArtifactId = "EXC-CAP-2026-001"
        ExplainedArtifactVersion = 1L
        StructuredReasoningGraph =
            {
                Nodes =
                    [
                        {
                            NodeId = "N1"
                            NodeType = "RootCause"
                            Label = "Demand DEM-ORD-1001 (+250 EA)"
                            Properties = Map [ ("DemandId", "DEM-ORD-1001"); ("Qty", "250"); ("Due", "2026-09-18") ]
                            IsHypothetical = false
                        }

                        {
                            NodeId = "N2"
                            NodeType = "Constraint"
                            Label = "WC-ASSEMBLY-LINE-01 (160h Max)"
                            Properties =
                                Map [ ("Resource", "WC-ASSEMBLY-LINE-01"); ("Envelope", "160h"); ("Shifts", "2") ]
                            IsHypothetical = false
                        }

                        {
                            NodeId = "N3"
                            NodeType = "Disruption"
                            Label = "Unplanned Calibration (8h)"
                            Properties = Map [ ("Downtime", "8h"); ("Category", "Maintenance"); ("Log", "LOG-CAL-88") ]
                            IsHypothetical = false
                        }

                        {
                            NodeId = "N4"
                            NodeType = "Bottleneck"
                            Label = "Capacity Overload: 184h / 160h (115%)"
                            Properties = Map [ ("Load", "184h"); ("Rated", "160h"); ("Utilization", "115%") ]
                            IsHypothetical = false
                        }

                        {
                            NodeId = "N5"
                            NodeType = "PrescriptiveRecommendation"
                            Label = "Shift 24h to WC-PACKAGING-01"
                            Properties =
                                Map
                                    [
                                        ("TargetResource", "WC-PACKAGING-01")
                                        ("TransferLoad", "24h")
                                        ("CostDelta", "$420")
                                    ]
                            IsHypothetical = true
                        }
                    ]

                Edges =
                    [
                        {
                            EdgeId = "E1"
                            SourceNode = "N1"
                            TargetNode = "N4"
                            Relationship = "Increases required run time (+32h)"
                            IsHypothetical = false
                        }

                        {
                            EdgeId = "E2"
                            SourceNode = "N3"
                            TargetNode = "N4"
                            Relationship = "Reduces effective available hours (-8h)"
                            IsHypothetical = false
                        }

                        {
                            EdgeId = "E3"
                            SourceNode = "N2"
                            TargetNode = "N4"
                            Relationship = "Bound by physical envelope limits"
                            IsHypothetical = false
                        }

                        {
                            EdgeId = "E4"
                            SourceNode = "N4"
                            TargetNode = "N5"
                            Relationship = "Triggers automated mitigation policy POL-CAP-04"
                            IsHypothetical = true
                        }
                    ]
            }

        MultiLevelRenderings =
            {
                PlannerSummary =
                    "Assembly Line 01 is oversubscribed by 24 hours (115% utilization) during week 37. Root cause is driven 70% by high-priority demand surge DEM-ORD-1001 combined with 8 hours unscheduled calibration downtime. Governed policy recommends shift reallocation to Packaging 01 or authorizing an overtime buffer."
                FactorWaterfallMarkdown =
                    "| Driver | Base | Impact | Contribution | Direction |\n|---|---|---|---|---|\n| Demand Run Time | 120h | +56h | 70.0% | Unfavorable |\n| Maintenance Loss | 0h | +8h | 10.0% | Unfavorable |\n| Overtime Buffer | 0h | -16h | -20.0% | Favorable |"
                FullAuditJson = """{"decision": "CAP_OVERLOAD", "policy": "POL-CAP-ENVELOPE-01", "rule": "BR-S-041"}"""
                AiConsumableJsonLd = """{"@context": "https://medhavi.ai/ontology", "@type": "CausalInference"}"""
            }

        FactorContributions =
            [
                {
                    FactorName = "Demand Surge (DEM-ORD-1001)"
                    BaseValue = 120m
                    ImpactValue = 56m
                    PercentageContribution = 70.0m
                    Direction = "Unfavorable"
                    Confidence = 0.95m
                }

                {
                    FactorName = "Unscheduled Calibration Downtime"
                    BaseValue = 0m
                    ImpactValue = 8m
                    PercentageContribution = 10.0m
                    Direction = "Unfavorable"
                    Confidence = 0.92m
                }

                {
                    FactorName = "Overtime Buffer Available"
                    BaseValue = 0m
                    ImpactValue = -16m
                    PercentageContribution = 20.0m
                    Direction = "Favorable"
                    Confidence = 0.88m
                }
            ]

        PreservedEvidenceRefs =
            [
                {
                    ArtifactType = "Telemetry"
                    ArtifactId = "EVD-TELEMETRY-9912"
                    Version = 1L
                    Properties = Map [ ("Source", "IoT-WorkCenter-01") ]
                }
            ]

        RationaleBlocks =
            [
                {
                    RationaleBlockId = "RB-01"
                    OutcomeDimension = "Resource Capacity"
                    CausalRuleRef = "BR-S-041"
                    InputEvidenceRef = "EVD-TELEMETRY-9912"
                    GovernedNarrative =
                        "Under Rule BR-S-041, instantaneous load exceeding rated envelope by >10% triggers mandatory exception notification and evaluation of alternate routing."
                }
            ]

        IsNegativeExplanation = true
        NegativeExplanation =
            Some
                {
                    DiscardedAlternativeId = "ALT-CANCEL-DEMAND"
                    DiscardedAlternativeType = "DemandDecommitment"
                    ReasonCategory = "ViolatesSlaPolicy"
                    FormalReason =
                        "Customer Tier 1 SLA strictly forbids automatic decommitment without human executive escalation."
                    EvaluationContext = Map [ ("CustomerTier", "1"); ("PenaltyRisk", "$15,000") ]
                }

        OpaqueModelProvenance = None
        Confidence =
            {
                InheritedEvidenceConfidence = 0.94m
                EvidenceCompleteness = 0.98m
                EvidenceConsistency = 0.96m
                CompositeIndex = 0.95m
                ConfidenceLevel = "High"
            }

        TemplateVersionReference = "v1.0"
        CompositionTimestamp = DateTimeOffset(2026, 9, 12, 10, 16, 0, TimeSpan.Zero)
    }

let toUniversal (exp: SupplyExplanationDto) : UniversalRow =
    { Key = exp.ExplanationId
      Cells =
        Map.ofList
            [ "ExplanationId", TextValue exp.ExplanationId
              "ExplainedArtifactType", TextValue exp.ExplainedArtifactType
              "ExplainedArtifactId", TextValue exp.ExplainedArtifactId
              "ExplainedArtifactVersion", NumberValue (decimal exp.ExplainedArtifactVersion)
              "Confidence", NumberValue 0.95m ] }

let store: DataStore =
    {
        Id = storeId
        Fetch =
            fun (ctx: ApplicationContext) ->
                async {
                    match ctx.Resolve<SupplyExplanationQueries>() with
                    | Some queries ->
                        let! dtos = queries.GetAll() |> Async.AwaitTask
                        let explanations = if List.isEmpty dtos then [ fallbackExplanation ] else dtos
                        let rows = explanations |> List.map toUniversal
                        return Tabular rows
                    | None ->
                        match ctx.DomainServices with
                        | Some ds ->
                            let! dtos = ds.Supply.ExplainSupplyDecisions.SupplyExplanationQueries.GetAll() |> Async.AwaitTask
                            let explanations = if List.isEmpty dtos then [ fallbackExplanation ] else dtos
                            let rows = explanations |> List.map toUniversal
                            return Tabular rows
                        | None ->
                            let rows = [ fallbackExplanation ] |> List.map toUniversal
                            return Tabular rows
                }
    }

let render (context: ComponentRenderContext) (dispatch: ComponentEvent -> unit) : Node =
    let expl = fallbackExplanation
    let selectedNodeId = None

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
                // Confidence Pill
                span {
                    attr.style
                        "background: var(--rz-success-lighter); color: var(--rz-success); border: 1px solid var(--rz-success-light); border-radius: 12px; padding: 3px 10px; font-size: 11px; font-weight: 600;"

                    sprintf
                        "Confidence: %.0f%% (%s)"
                        (float expl.Confidence.CompositeIndex * 100.0)
                        expl.Confidence.ConfidenceLevel
                }
                // Governed Rule Pill
                span {
                    attr.style
                        "background: var(--rz-primary-lighter); color: var(--rz-primary); border: 1px solid var(--rz-primary-light); border-radius: 12px; padding: 3px 10px; font-size: 11px; font-weight: 600;"

                    "Governed: BR-S-041"
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

            // Causal DAG
            div { SupplyReasoningGraph.renderDag expl.StructuredReasoningGraph selectedNodeId (fun _ -> ()) }

            // Driver Waterfall
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

                forEach expl.RationaleBlocks (fun rb ->
                    div {
                        attr.style
                            "font-size: 11px; display: flex; flex-direction: column; gap: 4px; border: 1px solid rgba(0,0,0,0.06); border-radius: 6px; padding: 8px; background: rgba(0,0,0,0.01);"

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

                        div {
                            attr.style "font-size: 10px; color: var(--rz-text-secondary-color); margin-top: 4px;"
                            "Policy: Customer Tier 1 SLA Non-Decommitment"
                        }
                    }
                | None ->
                    div {
                        attr.style "font-size: 11px; color: var(--rz-text-secondary-color);"
                        "No alternatives discarded for this decision."
                    }
            }

            // 3. Prescriptive Actions (1-Click Resolution)
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

                    // Action 1
                    button {
                        attr.style
                            "background: var(--rz-primary); color: var(--rz-on-primary); border: none; border-radius: 6px; padding: 8px 12px; font-size: 11px; font-weight: 600; cursor: pointer; display: flex; align-items: center; justify-content: space-between;"

                        span { "Shift 24h to Packaging 01" }

                        span {
                            attr.``class`` "rzi"
                            "arrow_forward"
                        }
                    }

                    // Action 2
                    button {
                        attr.style
                            "background: var(--rz-success-lighter); color: var(--rz-success); border: 1px solid var(--rz-success-light); border-radius: 6px; padding: 7px 12px; font-size: 11px; font-weight: 600; cursor: pointer; display: flex; align-items: center; justify-content: space-between;"

                        span { "Approve 16h Overtime Buffer" }

                        span {
                            attr.``class`` "rzi"
                            "more_time"
                        }
                    }

                    // Action 3
                    button {
                        attr.style
                            "background: var(--rz-danger-lighter); color: var(--rz-danger); border: 1px solid var(--rz-danger-light); border-radius: 6px; padding: 7px 12px; font-size: 11px; font-weight: 600; cursor: pointer; display: flex; align-items: center; justify-content: space-between;"

                        span { "Escalate to Plant Director" }

                        span {
                            attr.``class`` "rzi"
                            "arrow_upward"
                        }
                    }
                }
            }
        }
    }

let runtime: ComponentRuntime = { Render = render }

let registration: ComponentRegistration =
    {
        Definition = definition
        Runtime = runtime
    }
