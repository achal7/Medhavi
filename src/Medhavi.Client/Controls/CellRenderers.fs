namespace Medhavi.Client.Controls

open System
open Bolero
open Bolero.Html

module CellRenderers =

    /// Universal Status Badge renderer for any status/lifecycle state text
    let statusBadge (value: obj) : Node =
        let textVal =
            if isNull value then ""
            else string value

        let badgeStyle, dotColor =
            match textVal.ToLowerInvariant() with
            | "active" | "on track" | "healthy" | "stable" | "normal"
            | "optimal" | "nonconstraint" | "notabottleneck" | "negligible" | "low" | "incompliance" ->
                "background: var(--rz-success-lighter); color: var(--rz-success); border: 1px solid var(--rz-success-light);",
                "var(--rz-success)"
            | "shortage" | "delayed" | "critical" | "atrisk" | "at risk" | "high"
            | "overloaded" | "activeconstraint" | "currentbottleneck" | "criticallyconstrained"
            | "disrupted" | "underbuffer" ->
                "background: var(--rz-danger-lighter); color: var(--rz-danger); border: 1px solid var(--rz-danger-light);",
                "var(--rz-danger)"
            | "medium" | "volatile" | "warning" | "nearcapacity" | "nearconstraint"
            | "potentialbottleneck" | "constrained" | "overbuffer" ->
                "background: var(--rz-warning-lighter); color: var(--rz-warning-dark); border: 1px solid var(--rz-warning-light);",
                "var(--rz-warning)"
            | "satisfied" | "resolved" | "completed" | "underutilized" | "balanced"
            | "sufficient" | "production" | "procurement" | "transfer" | "inventory"
            | "purchaseorder" | "productionorder" | "suppliercommitment" ->
                "background: var(--rz-info-lighter); color: var(--rz-info); border: 1px solid var(--rz-info-light);",
                "var(--rz-info)"
            | "cancelled" | "inactive" | "retired" | "draft" | "superseded" | "unassessed" ->
                "background: var(--rz-secondary-lighter); color: var(--rz-secondary); border: 1px solid var(--rz-border-color);",
                "var(--rz-secondary)"
            | _ ->
                "background: var(--rz-base-background-color); color: var(--rz-text-color); border: 1px solid var(--rz-border-color);",
                "var(--rz-text-secondary-color)"

        span {
            attr.style (
                "display: inline-flex; align-items: center; gap: 6px; padding: 2px 8px; border-radius: 12px; font-size: 11px; font-weight: 600; "
                + badgeStyle
            )
            span {
                attr.style ("width: 6px; height: 6px; border-radius: 50%; background-color: " + dotColor + ";")
            }
            text textVal
        }

    /// Universal Lock Icon renderer for boolean locked status
    let lockIcon (value: obj) : Node =
        let isLocked =
            match value with
            | :? bool as b -> b
            | :? string as s ->
                match Boolean.TryParse s with
                | true, b -> b
                | _ -> false
            | _ -> false

        if isLocked then
            span {
                attr.style "color: var(--rz-warning); font-size: 14px;"
                attr.title "Locked against automated rescheduling"
                rawHtml "<i class=\"rzi\">lock</i>"
            }
        else
            empty ()

    /// Operating Envelope Utilization Bar renderer for capacity metrics
    let operatingEnvelopeBar (value: obj) : Node =
        let pct =
            match value with
            | :? decimal as d -> d
            | :? double as f -> decimal f
            | :? int as i -> decimal i
            | :? string as s ->
                match Decimal.TryParse s with
                | true, d -> d
                | _ -> 0m
            | _ -> 0m

        let barColor =
            if pct > 100m then "var(--rz-danger)"
            elif pct >= 85m then "var(--rz-warning)"
            elif pct < 50m then "var(--rz-info)"
            else "var(--rz-success)"

        let widthPct = Math.Min(100m, Math.Max(0m, pct))

        div {
            attr.style "display: flex; align-items: center; gap: 8px; width: 100%; min-width: 120px;"
            div {
                attr.style "flex: 1; height: 8px; background: var(--rz-secondary-lighter); border-radius: 4px; overflow: hidden; border: 1px solid var(--rz-border-color);"
                div {
                    attr.style $"width: {widthPct}%%; height: 100%%; background-color: {barColor}; border-radius: 3px;"
                }
            }
            span {
                attr.style $"font-size: 11px; font-weight: 600; color: {barColor}; min-width: 44px; text-align: right; font-variant-numeric: tabular-nums;"
                $"{pct:F1}%%"
            }
        }

    /// Multi-dimensional Severity Vector renderer for Impact x Urgency (PO-S-120)
    let severityVectorBadge (value: obj) : Node =
        let textVal = if isNull value then "" else string value
        let parts = textVal.Split([| '/'; '·'; 'x'; 'X' |], StringSplitOptions.RemoveEmptyEntries)
        let impact = if parts.Length > 0 then parts.[0].Trim() else textVal
        let urgency = if parts.Length > 1 then parts.[1].Trim() else ""

        let impactBg, impactColor =
            match impact.ToLowerInvariant() with
            | "severe" | "critical" -> "var(--rz-danger-lighter)", "var(--rz-danger)"
            | "major" | "high" -> "var(--rz-warning-lighter)", "var(--rz-warning-dark)"
            | "moderate" | "medium" -> "var(--rz-warning-lighter)", "var(--rz-warning-dark)"
            | "minor" | "low" -> "var(--rz-info-lighter)", "var(--rz-info)"
            | _ -> "var(--rz-secondary-lighter)", "var(--rz-secondary)"

        span {
            attr.style "display: inline-flex; align-items: center; border-radius: 12px; font-size: 11px; font-weight: 600; overflow: hidden; border: 1px solid var(--rz-border-color);"
            span {
                attr.style $"padding: 2px 7px; background: {impactBg}; color: {impactColor}; font-weight: 700;"
                impact
            }
            if not (String.IsNullOrWhiteSpace urgency) then
                span {
                    attr.style "padding: 2px 7px; background: var(--rz-secondary-lighter); color: var(--rz-text-secondary-color); font-size: 10px; border-left: 1px solid var(--rz-border-color);"
                    urgency
                }
        }

    /// Epistemic State Badge renderer for SenseSupplyChanges tracking
    let epistemicBadge (value: obj) : Node =
        let textVal = if isNull value then "" else string value
        let badgeBg, badgeColor, iconName =
            match textVal.ToLowerInvariant() with
            | "confirmed" -> "var(--rz-success-lighter)", "var(--rz-success)", "verified"
            | "corroborated" -> "var(--rz-warning-lighter)", "var(--rz-warning-dark)", "fact_check"
            | "observed" -> "var(--rz-primary-lighter)", "var(--rz-primary)", "visibility"
            | "resolved" -> "var(--rz-info-lighter)", "var(--rz-info)", "task_alt"
            | "nochange" -> "var(--rz-secondary-lighter)", "var(--rz-secondary)", "remove"
            | _ -> "var(--rz-secondary-lighter)", "var(--rz-text-secondary-color)", "info"

        span {
            attr.style $"display: inline-flex; align-items: center; gap: 4px; padding: 2px 8px; border-radius: 12px; font-size: 11px; font-weight: 600; background: {badgeBg}; color: {badgeColor}; border: 1px solid {badgeColor};"
            span {
                attr.``class`` "rzi"
                attr.style "font-size: 13px;"
                iconName
            }
            text textVal
        }

    /// Standard template lookup dictionary for DynamicRadzenGrid
    let templates : System.Collections.Generic.IDictionary<string, Func<obj, Node>> =
        dict [
            "StatusBadge", Func<obj, Node>(statusBadge)
            "LockIcon", Func<obj, Node>(lockIcon)
            "OperatingEnvelopeBar", Func<obj, Node>(operatingEnvelopeBar)
            "SeverityVectorBadge", Func<obj, Node>(severityVectorBadge)
            "EpistemicBadge", Func<obj, Node>(epistemicBadge)
        ]

