module Medhavi.Client.Core.Traceability.TraceRuleInspectorRenderer

open System
open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor
open Medhavi.Contracts.Traceability

let renderRuleInspector (dossier: TraceDossierDto) : Node =
    div {
        attr.style "display: flex; flex-direction: column; gap: 1rem; padding: 1rem; overflow-y: auto; height: 100%; box-sizing: border-box;"

        if List.isEmpty dossier.Decisions then
            comp<RadzenAlert> {
                "AlertStyle" => AlertStyle.Info
                "Variant" => Variant.Flat
                "Shade" => Shade.Lighter
                "AllowClose" => false
                "No explicit business rule evaluation dossiers were attached to this execution trace."
            }
        else
            forEach dossier.Decisions (fun dec ->
                comp<RadzenCard> {
                    attr.style "padding: 1.1rem; border-radius: 8px; display: flex; flex-direction: column; gap: 0.75rem;"

                    // Header Bar
                    div {
                        attr.style "display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid var(--rz-border-color); padding-bottom: 0.5rem;"
                        div {
                            attr.style "display: flex; align-items: center; gap: 0.5rem;"
                            span {
                                attr.``class`` "rzi"
                                attr.style "color: var(--rz-primary); font-size: 1.2rem;"
                                "gavel"
                            }
                            span {
                                attr.style "font-weight: 700; font-size: 13px;"
                                sprintf "Policy: %s (v%s)" dec.PolicyId dec.PolicyVersion
                            }
                        }
                        comp<RadzenBadge> {
                            "Text" => dec.Outcome
                            "BadgeStyle" => (if dec.Outcome.Equals("Accepted", StringComparison.OrdinalIgnoreCase) then BadgeStyle.Success else BadgeStyle.Warning)
                            "IsPill" => true
                        }
                    }

                    // Rationale Summary Callout
                    div {
                        attr.style "background: rgba(var(--rz-primary-rgb, 67, 80, 222), 0.06); border-left: 3px solid var(--rz-primary); border-radius: 4px; padding: 0.6rem 0.8rem; font-size: 12px;"
                        div {
                            attr.style "font-weight: 700; color: var(--rz-primary); margin-bottom: 2px;"
                            "Governed Engine Rationale Summary"
                        }
                        dec.RationaleSummary
                    }

                    // Rule Evaluation Table
                    div {
                        attr.style "font-size: 11px; font-weight: 700; color: var(--rz-text-secondary-color); text-transform: uppercase;"
                        "Rules Evaluated & Fact Evidence"
                    }

                    div {
                        attr.style "display: flex; flex-direction: column; gap: 6px;"
                        forEach dec.RulesEvaluated (fun rule ->
                            div {
                                attr.style "border: 1px solid var(--rz-border-color); border-radius: 6px; padding: 0.6rem; background: var(--rz-base-background-color);"
                                div {
                                    attr.style "display: flex; justify-content: space-between; align-items: center; margin-bottom: 4px;"
                                    span {
                                        attr.style "font-weight: 600; font-size: 12px;"
                                        rule.RuleId
                                    }
                                    comp<RadzenBadge> {
                                        "Text" => (if rule.Passed then "PASSED" else "FAILED")
                                        "BadgeStyle" => (if rule.Passed then BadgeStyle.Success else BadgeStyle.Danger)
                                        "Variant" => Variant.Flat
                                    }
                                }
                                if not rule.Passed && not (String.IsNullOrWhiteSpace rule.ReasonCode) then
                                    div {
                                        attr.style "font-size: 11px; color: var(--rz-danger); font-weight: 600; margin-bottom: 4px;"
                                        sprintf "Reason Code: %s" rule.ReasonCode
                                    }
                                if not (List.isEmpty rule.Evidence) then
                                    div {
                                        attr.style "font-size: 11px; color: var(--rz-text-secondary-color); padding-left: 0.5rem; border-left: 2px solid var(--rz-border-color);"
                                        forEach rule.Evidence (fun ev -> div { ev })
                                    }
                            })
                    }

                    // Alternatives evaluated
                    if not (List.isEmpty dec.Alternatives) then
                        div {
                            attr.style "font-size: 11px; color: var(--rz-text-secondary-color); margin-top: 4px;"
                            span { attr.style "font-weight: 700;"; "Alternatives Considered: " }
                            span { dec.Alternatives |> List.map (fun (k, v) -> sprintf "%s: %s" k v) |> String.concat " | " }
                        }
                })
    }

