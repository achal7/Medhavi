module Medhavi.Client.Shells.SidePanel

open System
open Elmish
open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor
open Microsoft.AspNetCore.Components.Web
open Medhavi.Client.Controls
open Medhavi.Client.AI

type Model =
    { Expanded: bool
      SelectedTab: int
      SelectedDetail: string option
      ActiveExplanation: CausalExplanation option
      RecentAlerts: string list }

type Msg =
    | SetExpanded of bool
    | SetSelectedTab of int
    | SetSelectedDetail of string option
    | ExplainEntity of entityId: string * entityType: string
    | ClearExplanation
    | ApplyResolution of resolutionId: string * actionCode: string
    | AddAlert of string

[<RequireQualifiedAccess>]
type Output = ResolutionApplied of resolutionId: string * actionCode: string

let init () : Model * Cmd<Msg> =
    { Expanded = false
      SelectedTab = 0
      SelectedDetail = None
      ActiveExplanation = None
      RecentAlerts = [ "Shortage detected on demand order D-1002 (Munich DC)" ] },
    Cmd.none

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> * Output option =
    match msg with
    | SetExpanded expanded -> { model with Expanded = expanded }, Cmd.none, None

    | SetSelectedTab tab -> { model with SelectedTab = tab }, Cmd.none, None

    | SetSelectedDetail detail -> { model with SelectedDetail = detail }, Cmd.none, None

    | ExplainEntity(entityId, entityType) ->
        let expl = CausalExplainer.explain entityId entityType

        { model with
            Expanded = true
            SelectedTab = 2 // Switch to AI tab
            ActiveExplanation = Some expl },
        Cmd.none,
        None

    | ClearExplanation -> { model with ActiveExplanation = None }, Cmd.none, None

    | ApplyResolution(resId, actionCode) -> model, Cmd.none, Some(Output.ResolutionApplied(resId, actionCode))

    | AddAlert alert ->
        { model with
            RecentAlerts = alert :: model.RecentAlerts },
        Cmd.none,
        None

let private severityStyle (severity: ImpactSeverity) =
    match severity with
    | Critical ->
        "color: var(--rz-danger); background: rgba(220, 53, 69, 0.15); border: 1px solid rgba(220, 53, 69, 0.35);"
    | High -> "color: #b78103; background: rgba(255, 193, 7, 0.15); border: 1px solid rgba(255, 193, 7, 0.35);"
    | Medium ->
        "color: var(--rz-info); background: rgba(23, 162, 184, 0.15); border: 1px solid rgba(23, 162, 184, 0.35);"
    | Low -> "color: var(--rz-success); background: rgba(40, 167, 69, 0.15); border: 1px solid rgba(40, 167, 69, 0.35);"

let private severityLabel (severity: ImpactSeverity) =
    match severity with
    | Critical -> "CRITICAL RISK"
    | High -> "HIGH IMPACT"
    | Medium -> "MODERATE"
    | Low -> "LOW RISK"

let private renderActiveExplanation (expl: CausalExplanation) (dispatch: Msg -> unit) : Node =
    div {
        attr.style "display: flex; flex-direction: column; gap: 0.85rem;"

        // Explainer Header Bar
        div {
            attr.style
                "display: flex; align-items: center; justify-content: space-between; border-bottom: 1px solid var(--rz-border-color); padding-bottom: 0.5rem;"

            div {
                attr.style "display: flex; align-items: center; gap: 0.4rem;"

                span {
                    attr.``class`` "rzi"
                    attr.style "font-size: 1.15rem; color: var(--rz-primary);"
                    "auto_awesome"
                }

                span {
                    attr.style "font-size: 13px; font-weight: 700; color: var(--rz-text-color);"
                    "Causal Explanation Trace"
                }
            }

            button {
                attr.``type`` "button"
                attr.title "Clear explanation"

                attr.style
                    "border: none; background: transparent; color: var(--rz-text-secondary-color); cursor: pointer; padding: 2px 6px; font-size: 12px;"

                attr.callback "onclick" (fun (_: MouseEventArgs) -> dispatch ClearExplanation)
                "✕ Clear"
            }
        }

        // Entity Title & Badges
        div {
            attr.style "display: flex; flex-direction: column; gap: 0.35rem;"

            div {
                attr.style "font-size: 14px; font-weight: 700; color: var(--rz-text-color); line-height: 1.2;"
                expl.Title
            }

            div {
                attr.style "display: flex; align-items: center; gap: 0.5rem; flex-wrap: wrap;"

                span {
                    attr.style(
                        "font-size: 11px; font-weight: 700; padding: 2px 8px; border-radius: 4px; "
                        + severityStyle expl.Severity
                    )

                    severityLabel expl.Severity
                }

                span {
                    attr.style
                        "font-size: 11px; font-weight: 600; color: var(--rz-danger); background: rgba(220, 53, 69, 0.08); padding: 2px 8px; border-radius: 4px;"

                    expl.FinancialImpact
                }
            }
        }

        // Root Cause Callout
        div {
            attr.style
                """
                background: rgba(var(--rz-primary-rgb, 67, 80, 222), 0.06);
                border-left: 3px solid var(--rz-primary);
                border-radius: 4px;
                padding: 0.65rem 0.85rem;
                font-size: 12px;
                line-height: 1.4;
                color: var(--rz-text-color);
                """

            div {
                attr.style
                    "font-weight: 700; color: var(--rz-primary); margin-bottom: 2px; display: flex; align-items: center; gap: 0.3rem;"

                span {
                    attr.``class`` "rzi"
                    attr.style "font-size: 14px;"
                    "lightbulb"
                }

                "Identified Root Cause"
            }

            expl.RootCauseStatement
        }

        // Causal Lineage Path (Vertical Tree)
        div {
            attr.style "display: flex; flex-direction: column; gap: 0.4rem;"

            div {
                attr.style
                    "font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; color: var(--rz-text-secondary-color); margin-top: 0.25rem;"

                "Supply Chain Causal Lineage"
            }

            forEach (List.indexed expl.CausalPath) (fun (idx, node) ->
                div {
                    attr.style "display: flex; flex-direction: column; gap: 0.25rem;"

                    if idx > 0 then
                        div {
                            attr.style
                                "display: flex; align-items: center; justify-content: center; height: 16px; color: var(--rz-text-secondary-color);"

                            span {
                                attr.``class`` "rzi"
                                attr.style "font-size: 14px;"
                                "arrow_downward"
                            }
                        }
                    else
                        empty()

                    div {
                        attr.style
                            """
                            background: var(--rz-base-background-color);
                            border: 1px solid var(--rz-border-color);
                            border-radius: 6px;
                            padding: 0.55rem 0.75rem;
                            box-shadow: 0 1px 3px rgba(0, 0, 0, 0.04);
                            """

                        div {
                            attr.style
                                "display: flex; align-items: center; justify-content: space-between; margin-bottom: 2px;"

                            div {
                                attr.style "display: flex; align-items: center; gap: 0.35rem;"

                                span {
                                    attr.``class`` "rzi"
                                    attr.style "font-size: 14px; color: var(--rz-primary);"
                                    node.Icon
                                }

                                span {
                                    attr.style
                                        "font-size: 11px; font-weight: 700; text-transform: uppercase; color: var(--rz-text-secondary-color);"

                                    node.NodeType
                                }
                            }

                            span {
                                attr.style("font-size: 11px; font-weight: 600; color: " + node.StatusColor + ";")
                                node.Status
                            }
                        }

                        div {
                            attr.style "font-size: 13px; font-weight: 600; color: var(--rz-text-color);"
                            node.Title
                        }

                        div {
                            attr.style "font-size: 11px; color: var(--rz-text-secondary-color); margin-bottom: 4px;"
                            node.Subtitle
                        }

                        div {
                            attr.style
                                "display: flex; flex-direction: column; gap: 2px; border-top: 1px dashed var(--rz-border-color); padding-top: 4px;"

                            for (mKey, mVal) in node.Metrics do
                                div {
                                    attr.style
                                        "display: flex; justify-content: space-between; font-size: 10.5px; color: var(--rz-text-secondary-color);"

                                    span { mKey }

                                    span {
                                        attr.style "font-weight: 600; color: var(--rz-text-color);"
                                        mVal
                                    }
                                }
                        }
                    }
                })
        }

        // Autonomous AI Recommendations
        div {
            attr.style "display: flex; flex-direction: column; gap: 0.5rem; margin-top: 0.25rem;"

            div {
                attr.style
                    "font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; color: var(--rz-text-secondary-color);"

                "Autonomous AI Resolutions"
            }

            forEach expl.Recommendations (fun res ->
                div {
                    attr.style
                        """
                        background: var(--rz-base-background-color);
                        border: 1px solid var(--rz-border-color);
                        border-radius: 6px;
                        padding: 0.65rem 0.85rem;
                        box-shadow: 0 1px 4px rgba(0, 0, 0, 0.05);
                        display: flex;
                        flex-direction: column;
                        gap: 0.35rem;
                        """

                    div {
                        attr.style "display: flex; align-items: center; justify-content: space-between;"

                        span {
                            attr.style "font-size: 13px; font-weight: 700; color: var(--rz-text-color);"
                            res.Title
                        }

                        span {
                            attr.style "font-size: 11px; font-weight: 700; color: var(--rz-success);"
                            sprintf "%.0f%% Match" res.ConfidencePct
                        }
                    }

                    div {
                        attr.style "font-size: 11.5px; color: var(--rz-text-secondary-color); line-height: 1.35;"
                        res.Description
                    }

                    div {
                        attr.style
                            "display: flex; justify-content: space-between; font-size: 11px; color: var(--rz-text-secondary-color); margin-top: 2px;"

                        match res.CostEstimate with
                        | Some cost -> span { "Cost: " + cost }
                        | None -> empty()

                        // match res.RecoveryTime with
                        // | Some rec -> span { attr.style "font-weight: 600; color: var(--rz-text-color);" ; rec }
                        // | None -> empty()
                        if res.RecoveryTime.IsSome then
                            span {
                                attr.style $"font-weight: 600; color: var(--rz-text-color);{res.RecoveryTime.Value}"
                                
                            }
                        else
                            empty()
                    }

                    div {
                        attr.style "margin-top: 0.35rem;"

                        button {
                            attr.``type`` "button"

                            attr.style
                                """
                                display: inline-flex;
                                align-items: center;
                                justify-content: center;
                                gap: 0.35rem;
                                width: 100%;
                                padding: 0.4rem 0.75rem;
                                background: var(--rz-primary);
                                color: white;
                                border: none;
                                border-radius: 4px;
                                font-size: 12px;
                                font-weight: 600;
                                cursor: pointer;
                                transition: opacity 0.15s ease;
                                """

                            attr.callback "onclick" (fun (_: MouseEventArgs) ->
                                dispatch(ApplyResolution(res.ResolutionId, res.ActionCode)))

                            span {
                                attr.``class`` "rzi"
                                attr.style "font-size: 14px;"
                                res.ButtonIcon
                            }

                            res.ButtonLabel
                        }
                    }
                })
        }
    }

let private renderStandbyCopilot (dispatch: Msg -> unit) : Node =
    div {
        attr.style "display: flex; flex-direction: column; gap: 0.85rem;"

        div {
            attr.style
                """
                background: rgba(var(--rz-primary-rgb, 67, 80, 222), 0.05);
                border: 1px dashed var(--rz-border-color);
                border-radius: 8px;
                padding: 1rem;
                display: flex;
                flex-direction: column;
                gap: 0.5rem;
                """

            div {
                attr.style "display: flex; align-items: center; gap: 0.4rem;"

                span {
                    attr.``class`` "rzi"
                    attr.style "font-size: 1.35rem; color: var(--rz-primary);"
                    "psychology"
                }

                span {
                    attr.style "font-size: 14px; font-weight: 700; color: var(--rz-text-color);"
                    "Kinaxis-Class Planning Copilot"
                }
            }

            div {
                attr.style "font-size: 12px; color: var(--rz-text-secondary-color); line-height: 1.4;"
                "Instant causal graph explanations for demand shortages, capacity bottlenecks, supplier delays, and what-if trade-off recommendations."
            }
        }

        div {
            attr.style "display: flex; flex-direction: column; gap: 0.45rem;"

            div {
                attr.style
                    "font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; color: var(--rz-text-secondary-color);"

                "Instant Causal Traces"
            }

            button {
                attr.``type`` "button"

                attr.style
                    """
                    display: flex;
                    align-items: center;
                    gap: 0.5rem;
                    text-align: left;
                    padding: 0.6rem 0.75rem;
                    background: var(--rz-base-background-color);
                    border: 1px solid var(--rz-border-color);
                    border-radius: 6px;
                    color: var(--rz-text-color);
                    font-size: 12px;
                    cursor: pointer;
                    transition: border-color 0.15s ease;
                    """

                attr.callback "onclick" (fun (_: MouseEventArgs) -> dispatch(ExplainEntity("D-1002", "Demand")))

                span {
                    attr.``class`` "rzi"
                    attr.style "font-size: 16px; color: var(--rz-danger);"
                    "error"
                }

                div {
                    div {
                        attr.style "font-weight: 600;"
                        "Explain Shortage on D-1002 (Siemens AG)"
                    }

                    div {
                        attr.style "font-size: 11px; color: var(--rz-text-secondary-color);"
                        "Shortage of 450 units · $342.5k at risk"
                    }
                }
            }

            button {
                attr.``type`` "button"

                attr.style
                    """
                    display: flex;
                    align-items: center;
                    gap: 0.5rem;
                    text-align: left;
                    padding: 0.6rem 0.75rem;
                    background: var(--rz-base-background-color);
                    border: 1px solid var(--rz-border-color);
                    border-radius: 6px;
                    color: var(--rz-text-color);
                    font-size: 12px;
                    cursor: pointer;
                    transition: border-color 0.15s ease;
                    """

                attr.callback "onclick" (fun (_: MouseEventArgs) -> dispatch(ExplainEntity("D-1003", "Demand")))

                span {
                    attr.``class`` "rzi"
                    attr.style "font-size: 16px; color: #b78103;"
                    "warning"
                }

                div {
                    div {
                        attr.style "font-weight: 600;"
                        "Explain Bottleneck on D-1003 (ASML Corp)"
                    }

                    div {
                        attr.style "font-size: 11px; color: var(--rz-text-secondary-color);"
                        "Berlin DC Line 2 maintenance downtime"
                    }
                }
            }

            button {
                attr.``type`` "button"

                attr.style
                    """
                    display: flex;
                    align-items: center;
                    gap: 0.5rem;
                    text-align: left;
                    padding: 0.6rem 0.75rem;
                    background: var(--rz-base-background-color);
                    border: 1px solid var(--rz-border-color);
                    border-radius: 6px;
                    color: var(--rz-text-color);
                    font-size: 12px;
                    cursor: pointer;
                    transition: border-color 0.15s ease;
                    """

                attr.callback "onclick" (fun (_: MouseEventArgs) -> dispatch(ExplainEntity("PO-8802", "Supply")))

                span {
                    attr.``class`` "rzi"
                    attr.style "font-size: 16px; color: var(--rz-primary);"
                    "local_shipping"
                }

                div {
                    div {
                        attr.style "font-weight: 600;"
                        "Explain Supplier Delay on PO-8802"
                    }

                    div {
                        attr.style "font-size: 11px; color: var(--rz-text-secondary-color);"
                        "Texas Instruments · 4-day customs slip"
                    }
                }
            }
        }
    }

let view (model: Model) (dispatch: Msg -> unit) : Node =
    if not model.Expanded then
        empty ()
    else
        comp<RadzenSidebar> {
            "Position" => SidebarPosition.End
            "Expanded" => true
            "Style" => "width: 360px;"

            attr.callback "ExpandedChanged" (fun (expanded: bool) -> dispatch (SetExpanded expanded))

            comp<RadzenTabs> {
                "SelectedIndex" => model.SelectedTab
                attr.callback "Change" (SetSelectedTab >> dispatch)

                attr.fragment
                    "Tabs"
                    (concat {
                        // TAB 1: DETAIL INSPECTOR
                        comp<RadzenTabsItem> {
                            "Text" => "Detail"

                            div {
                                attr.``class`` "rz-p-3"

                                match model.SelectedDetail with
                                | Some key ->
                                    div {
                                        attr.style "display: flex; flex-direction: column; gap: 8px;"

                                        div {
                                            attr.style
                                                "font-weight: 600; font-size: 14px; border-bottom: 1px solid var(--rz-border-color); padding-bottom: 4px;"

                                            $"Entity Inspection: {key}"
                                        }

                                        div {
                                            attr.style "font-size: 12px; color: var(--rz-text-secondary-color);"
                                            "Real-time planning constraints, active commitments, and pegging linkages."
                                        }

                                        comp<RadzenCard> {
                                            attr.style "padding: 8px; font-size: 12px;"
                                            div { "Status: Active Inspection" }
                                            div { "Lock Status: User Protected" }
                                            div { "Pegged Supply: PO-8802 (Berlin DC)" }
                                        }
                                    }
                                | None ->
                                    div {
                                        attr.style
                                            "color: var(--rz-text-secondary-color); font-size: 13px; text-align: center; padding: 2rem 0;"

                                        "Select any row in Demand or Supply workbenches to inspect its properties."
                                    }
                            }
                        }

                        // TAB 2: ALERTS
                        comp<RadzenTabsItem> {
                            "Text" => "Alerts"

                            div {
                                attr.``class`` "rz-p-3"

                                if List.isEmpty model.RecentAlerts then
                                    div {
                                        attr.style
                                            "color: var(--rz-text-secondary-color); font-size: 13px; text-align: center; padding: 2rem 0;"

                                        "No active planning alerts."
                                    }
                                else
                                    div {
                                        attr.style "display: flex; flex-direction: column; gap: 6px;"

                                        for alert in model.RecentAlerts do
                                            comp<RadzenAlert> {
                                                "AlertStyle" => AlertStyle.Warning
                                                "Variant" => Variant.Flat
                                                "Shade" => Shade.Lighter
                                                "AllowClose" => false
                                                "Size" => AlertSize.Small
                                                alert
                                            }
                                    }
                            }
                        }

                        // TAB 3: AI PLANNING COPILOT
                        comp<RadzenTabsItem> {
                            "Text" => "AI Copilot"

                            div {
                                attr.``class`` "rz-p-3"

                                match model.ActiveExplanation with
                                | Some expl -> renderActiveExplanation expl dispatch
                                | None -> renderStandbyCopilot dispatch
                            }
                        }

                        // TAB 4: SESSION HISTORY
                        comp<RadzenTabsItem> {
                            "Text" => "History"

                            div {
                                attr.``class`` "rz-p-3"

                                div {
                                    attr.style "color: var(--rz-text-secondary-color); font-size: 13px;"
                                    "Session transaction log is clean."
                                }
                            }
                        }
                    })
            }
        }

type Component() =
    inherit ElmishComponent<Model, Msg>()

    override _.View model dispatch = view model dispatch
