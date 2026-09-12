namespace Medhavi.Client.Controls

open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor
open Microsoft.AspNetCore.Components.Web

type KpiTrendDirection =
    | Up
    | Down
    | Flat

type KpiHealth =
    | Healthy
    | Warning
    | Critical
    | Neutral

type KpiMetric =
    { Id: string
      Title: string
      Domain: string
      Value: string
      Unit: string option
      Target: string option
      ProgressPct: float option
      TrendDirection: KpiTrendDirection
      TrendValue: string
      IsTrendFavorable: bool
      Health: KpiHealth
      Icon: string
      Description: string
      DrillDownViewId: string option }

module KpiCard =

    let private healthColors (health: KpiHealth) =
        match health with
        | Healthy ->
            {| Accent = "var(--rz-success)"
               BadgeBg = "rgba(40, 167, 69, 0.12)"
               BadgeColor = "var(--rz-success)"
               Border = "1px solid rgba(40, 167, 69, 0.25)" |}
        | Warning ->
            {| Accent = "var(--rz-warning)"
               BadgeBg = "rgba(255, 193, 7, 0.15)"
               BadgeColor = "#b78103"
               Border = "1px solid rgba(255, 193, 7, 0.35)" |}
        | Critical ->
            {| Accent = "var(--rz-danger)"
               BadgeBg = "rgba(220, 53, 69, 0.12)"
               BadgeColor = "var(--rz-danger)"
               Border = "1px solid rgba(220, 53, 69, 0.25)" |}
        | Neutral ->
            {| Accent = "var(--rz-info)"
               BadgeBg = "rgba(23, 162, 184, 0.12)"
               BadgeColor = "var(--rz-info)"
               Border = "1px solid rgba(23, 162, 184, 0.25)" |}

    let private trendIcon (trend: KpiTrendDirection) =
        match trend with
        | Up -> "trending_up"
        | Down -> "trending_down"
        | Flat -> "trending_flat"

    let render (metric: KpiMetric) (onClick: (string -> unit) option) : Node =
        let styling = healthColors metric.Health
        let cursorStyle = if onClick.IsSome then "pointer" else "default"

        let cardContainerStyle =
            "background: var(--rz-base-background-color); "
            + "border: " + styling.Border + "; "
            + "border-radius: 8px; "
            + "padding: 0.9rem 1.1rem; "
            + "display: flex; "
            + "flex-direction: column; "
            + "justify-content: space-between; "
            + "box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04); "
            + "transition: transform 0.15s ease, box-shadow 0.15s ease; "
            + "cursor: " + cursorStyle + "; "
            + "height: 100%; "
            + "box-sizing: border-box;"

        div {
            attr.style cardContainerStyle

            if onClick.IsSome then
                attr.callback "onclick" (fun (_: MouseEventArgs) -> onClick.Value metric.Id)
            else
                attr.empty()

            // Header: Category/Domain pill + Icon
            div {
                attr.style "display: flex; align-items: center; justify-content: space-between; margin-bottom: 0.4rem;"

                span {
                    attr.style (
                        "font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; "
                        + "color: " + styling.BadgeColor + "; "
                        + "background: " + styling.BadgeBg + "; "
                        + "padding: 2px 8px; border-radius: 4px;"
                    )
                    metric.Domain
                }

                span {
                    attr.``class`` "rzi"
                    attr.style ("font-size: 1.25rem; color: " + styling.Accent + "; opacity: 0.85;")
                    metric.Icon
                }
            }

            // Title
            div {
                attr.style "font-size: 13px; font-weight: 600; color: var(--rz-text-secondary-color); margin-bottom: 0.2rem; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;"
                metric.Title
            }

            // Hero Value + Unit
            div {
                attr.style "display: flex; align-items: baseline; gap: 0.35rem; margin-bottom: 0.4rem;"

                span {
                    attr.style "font-size: 1.65rem; font-weight: 800; color: var(--rz-text-color); line-height: 1.1;"
                    metric.Value
                }

                match metric.Unit with
                | Some u ->
                    span {
                        attr.style "font-size: 13px; font-weight: 600; color: var(--rz-text-secondary-color);"
                        u
                    }
                | None -> ()
            }

            // Target Progress Bar (if target present)
            match metric.ProgressPct, metric.Target with
            | Some pct, Some targetStr ->
                let clampedPct = min pct 100.0
                div {
                    attr.style "margin-bottom: 0.5rem;"

                    div {
                        attr.style "display: flex; justify-content: space-between; font-size: 11px; color: var(--rz-text-secondary-color); margin-bottom: 2px;"
                        span { "Target: " + targetStr }
                        span { sprintf "%.1f%%" clampedPct }
                    }

                    div {
                        attr.style "width: 100%; height: 5px; background: var(--rz-border-color); border-radius: 3px; overflow: hidden;"

                        div {
                            attr.style (
                                "width: " + string clampedPct + "%; "
                                + "height: 100%; "
                                + "background: " + styling.Accent + "; "
                                + "border-radius: 3px; transition: width 0.3s ease;"
                            )
                        }
                    }
                }
            | _ -> ()

            // Footer: Trend indicator + Description
            div {
                attr.style "display: flex; align-items: center; justify-content: space-between; margin-top: auto; padding-top: 0.35rem; border-top: 1px solid var(--rz-border-color);"

                div {
                    attr.style "display: flex; align-items: center; gap: 0.25rem;"

                    let trendColor =
                        if metric.IsTrendFavorable then "var(--rz-success)"
                        else if metric.TrendDirection = Flat then "var(--rz-text-secondary-color)"
                        else "var(--rz-danger)"

                    span {
                        attr.``class`` "rzi"
                        attr.style ("font-size: 14px; color: " + trendColor + ";")
                        trendIcon metric.TrendDirection
                    }

                    span {
                        attr.style ("font-size: 12px; font-weight: 600; color: " + trendColor + ";")
                        metric.TrendValue
                    }
                }

                span {
                    attr.style "font-size: 11px; color: var(--rz-text-secondary-color); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 55%;"
                    metric.Description
                }
            }
        }

    let renderGrid (metrics: KpiMetric list) (onClick: (string -> unit) option) : Node =
        div {
            attr.style
                """
                display: grid;
                grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
                gap: 0.75rem;
                width: 100%;
                height: 100%;
                box-sizing: border-box;
                """

            forEach metrics (fun m -> render m onClick)
        }
