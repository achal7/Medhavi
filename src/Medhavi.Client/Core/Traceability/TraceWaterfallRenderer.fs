module Medhavi.Client.Core.Traceability.TraceWaterfallRenderer

open System
open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor
open Medhavi.Contracts.Traceability

let renderWaterfall (dossier: TraceDossierDto) : Node =
    let totalDuration = max 1.0 dossier.TotalDurationMs

    div {
        attr.style "display: flex; flex-direction: column; gap: 1rem; padding: 1rem; overflow-y: auto; height: 100%; box-sizing: border-box;"

        // 1. Stage Latency Gantt Breakdown
        comp<RadzenCard> {
            attr.style "padding: 1rem; border-radius: 8px;"

            div {
                attr.style "display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.75rem;"
                span {
                    attr.style "font-weight: 700; font-size: 13px; color: var(--rz-text-color);"
                    "Pipeline Execution Stage Breakdown"
                }
                span {
                    attr.style "font-size: 12px; font-weight: 600; color: var(--rz-primary);"
                    sprintf "Total Latency: %.1fms" totalDuration
                }
            }

            if List.isEmpty dossier.StageTimings then
                div {
                    attr.style "font-size: 12px; color: var(--rz-text-secondary-color);"
                    "Standard execution pipeline timings captured at envelope boundary."
                }
            else
                forEach dossier.StageTimings (fun stage ->
                    let pct = Math.Min(100.0, (stage.DurationMs / totalDuration) * 100.0)
                    div {
                        attr.style "margin-bottom: 0.5rem; display: flex; flex-direction: column; gap: 2px;"
                        div {
                            attr.style "display: flex; justify-content: space-between; font-size: 11px;"
                            span { attr.style "font-weight: 600;"; stage.StageName }
                            span { attr.style "color: var(--rz-text-secondary-color);"; sprintf "%.1fms (%.1f%%)" stage.DurationMs pct }
                        }
                        comp<RadzenProgressBar> {
                            "Value" => pct
                            "ShowValue" => false
                            "Style" => "height: 6px; border-radius: 3px;"
                        }
                    })
        }

        // 2. Chronological RadzenTimeline
        comp<RadzenCard> {
            attr.style "padding: 1rem; border-radius: 8px; flex: 1;"

            div {
                attr.style "font-weight: 700; font-size: 13px; margin-bottom: 0.75rem; color: var(--rz-text-color);"
                "Chronological Event Hops"
            }

            comp<RadzenTimeline> {
                "Orientation" => Orientation.Vertical
                "LinePosition" => LinePosition.Start

                attr.fragment "Items" (
                    forEach dossier.Events (fun evt ->
                        let isErr =
                            evt.Severity.Equals("Error", StringComparison.OrdinalIgnoreCase)
                            || evt.Severity.Equals("Errors", StringComparison.OrdinalIgnoreCase)
                            || evt.Severity.Equals("Critical", StringComparison.OrdinalIgnoreCase)
                        let isWarn = evt.Severity.Equals("Warning", StringComparison.OrdinalIgnoreCase)
                        let pStyle = if isErr then PointStyle.Danger elif isWarn then PointStyle.Warning else PointStyle.Primary

                        comp<RadzenTimelineItem> {
                            "PointStyle" => pStyle
                            "PointVariant" => Variant.Filled

                            attr.fragment "PointContent" (
                                span {
                                    attr.``class`` "rzi"
                                    attr.style "font-size: 12px; color: #fff;"
                                    if isErr then "error" elif isWarn then "warning" else "check"
                                }
                            )

                            attr.fragment "LabelContent" (
                                span {
                                    attr.style "font-size: 11px; font-weight: 600; color: var(--rz-text-secondary-color);"
                                    evt.Timestamp.ToString("HH:mm:ss.fff")
                                }
                            )

                            attr.fragment "ChildContent" (
                                div {
                                    attr.style "display: flex; flex-direction: column; gap: 2px; margin-bottom: 6px;"
                                    div {
                                        attr.style "font-size: 12px; font-weight: 700; color: var(--rz-text-color);"
                                        sprintf "%s · %s" evt.BoundedContext (if String.IsNullOrWhiteSpace evt.CommandType then evt.Message else evt.CommandType)
                                    }
                                    div {
                                        attr.style "font-size: 11px; color: var(--rz-text-secondary-color);"
                                        sprintf "%s (%s) — %s" evt.AggregateType evt.AggregateId evt.Message
                                    }
                                }
                            )
                        })
                )
            }
        }
    }

