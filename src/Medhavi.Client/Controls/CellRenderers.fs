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
            | "active" | "on track" | "healthy" | "stable" | "normal" ->
                "background: rgba(40, 167, 69, 0.15); color: #28a745; border: 1px solid rgba(40, 167, 69, 0.3);",
                "#28a745"
            | "shortage" | "delayed" | "critical" | "atrisk" | "at risk" | "high" ->
                "background: rgba(220, 53, 69, 0.15); color: #dc3545; border: 1px solid rgba(220, 53, 69, 0.3);",
                "#dc3545"
            | "medium" | "volatile" | "warning" ->
                "background: rgba(255, 193, 7, 0.15); color: #ffc107; border: 1px solid rgba(255, 193, 7, 0.3);",
                "#ffc107"
            | "satisfied" | "resolved" | "completed" ->
                "background: rgba(23, 162, 184, 0.15); color: #17a2b8; border: 1px solid rgba(23, 162, 184, 0.3);",
                "#17a2b8"
            | "cancelled" | "inactive" | "retired" ->
                "background: rgba(108, 117, 125, 0.15); color: #6c757d; border: 1px solid rgba(108, 117, 125, 0.3);",
                "#6c757d"
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
                attr.style "color: #ffc107; font-size: 14px;"
                attr.title "Locked against automated rescheduling"
                rawHtml "<i class=\"rzi\">lock</i>"
            }
        else
            empty ()

    /// Standard template lookup dictionary for DynamicRadzenGrid
    let templates : System.Collections.Generic.IDictionary<string, Func<obj, Node>> =
        dict [
            "StatusBadge", Func<obj, Node>(statusBadge)
            "LockIcon", Func<obj, Node>(lockIcon)
        ]
