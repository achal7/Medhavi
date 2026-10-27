namespace Medhavi.Client.Components.Supply

open System
open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor
open Microsoft.AspNetCore.Components.Web
open Medhavi.Contracts.Supply
open Medhavi.Client
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client.Rendering
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Data

module CapacityStudio =

    let definitionId = ComponentDefId.literal "capacity-studio"
    let storeId = CapacityGrid.storeId

    let definition: ComponentDefinition =
        { Id = definitionId
          DisplayName = "Capacity Planning Studio (Gantt & Histograms)"
          Icon = "view_timeline"
          Category = ComponentCategory.Visualization
          RequiredSchemaCapabilities = [ "Tabular" ]
          DefaultGridSpan = GridSpan.literal 12 8
          DefaultSchemaId = Some CapacitySchema.schemaId
          DefaultStoreId = Some storeId
          IsResizable = true
          IsDraggable = true
          SelectionMode = SelectionMode.SingleSelection }

    type StudioTab =
        | GanttTimeline
        | OperatingEnvelopes
        | PositionTable

    type WorkCenterTask =
        { OrderId: string
          Item: string
          StartDay: int
          DurationDays: int
          PlannedHours: decimal
          ProgressPct: float
          IsOverload: bool
          StatusBadge: string }

    type WorkCenterSchedule =
        { ResourceId: string
          DisplayName: string
          AvailableHours: decimal
          LoadHours: decimal
          UtilizationPct: decimal
          Status: CapacityUtilizationStatus
          Constraint: CapacityConstraintStatus
          Bottleneck: BottleneckAssessmentStatus
          OverloadRisk: CapacityRiskLevel
          Envelope: CapacityOperatingEnvelopeDto
          Tasks: WorkCenterTask list }

    let private defaultEnvelope =
        { MinEconomicHours = 40.0m
          TargetLowerHours = 120.0m
          TargetUpperHours = 150.0m
          MaxPlannedHours = 160.0m
          AbsoluteCapacityHours = 180.0m
          OvertimeThresholdHours = 155.0m }

    let schedules: WorkCenterSchedule list =
        [ { ResourceId = "WC-ASSEMBLY-LINE-01"
            DisplayName = "Main Automated Assembly Line"
            AvailableHours = 160.0m
            LoadHours = 184.0m
            UtilizationPct = 115.0m
            Status = CapacityUtilizationStatus.Overloaded
            Constraint = CapacityConstraintStatus.ActiveConstraint
            Bottleneck = BottleneckAssessmentStatus.CurrentBottleneck
            OverloadRisk = CapacityRiskLevel.Critical
            Envelope = defaultEnvelope
            Tasks =
              [ { OrderId = "WO-2026-W37-01"
                  Item = "SKU-BATTERY-PACK-PRO"
                  StartDay = 6
                  DurationDays = 10
                  PlannedHours = 120.0m
                  ProgressPct = 65.0
                  IsOverload = false
                  StatusBadge = "In Production" }
                { OrderId = "WO-2026-W37-04"
                  Item = "SKU-DRIVE-500GB"
                  StartDay = 12
                  DurationDays = 5
                  PlannedHours = 64.0m
                  ProgressPct = 0.0
                  IsOverload = true
                  StatusBadge = "Conflict Overload" } ] }

          { ResourceId = "WC-PACKAGING-01"
            DisplayName = "High-Speed Palletizing & Boxing"
            AvailableHours = 160.0m
            LoadHours = 128.0m
            UtilizationPct = 80.0m
            Status = CapacityUtilizationStatus.Optimal
            Constraint = CapacityConstraintStatus.NonConstraint
            Bottleneck = BottleneckAssessmentStatus.NotABottleneck
            OverloadRisk = CapacityRiskLevel.Low
            Envelope = defaultEnvelope
            Tasks =
              [ { OrderId = "WO-2026-W37-02"
                  Item = "SKU-BATTERY-PACK-PRO"
                  StartDay = 8
                  DurationDays = 6
                  PlannedHours = 72.0m
                  ProgressPct = 30.0
                  IsOverload = false
                  StatusBadge = "Scheduled" }
                { OrderId = "WO-2026-W37-05"
                  Item = "SKU-DRIVE-500GB"
                  StartDay = 13
                  DurationDays = 5
                  PlannedHours = 56.0m
                  ProgressPct = 0.0
                  IsOverload = false
                  StatusBadge = "Planned" } ] }

          { ResourceId = "WC-TESTING-BAY-02"
            DisplayName = "Thermal Burn-In & Automated QA Rig"
            AvailableHours = 120.0m
            LoadHours = 112.0m
            UtilizationPct = 93.3m
            Status = CapacityUtilizationStatus.NearCapacity
            Constraint = CapacityConstraintStatus.NearConstraint
            Bottleneck = BottleneckAssessmentStatus.PotentialBottleneck
            OverloadRisk = CapacityRiskLevel.Medium
            Envelope = { defaultEnvelope with AbsoluteCapacityHours = 140.0m; MaxPlannedHours = 120.0m }
            Tasks =
              [ { OrderId = "WO-2026-W37-03"
                  Item = "SKU-POWER-SUPPLY-850W"
                  StartDay = 2
                  DurationDays = 4
                  PlannedHours = 48.0m
                  ProgressPct = 90.0
                  IsOverload = false
                  StatusBadge = "QA Testing" }
                { OrderId = "WO-2026-W37-06"
                  Item = "SKU-DRIVE-1TB"
                  StartDay = 10
                  DurationDays = 5
                  PlannedHours = 64.0m
                  ProgressPct = 0.0
                  IsOverload = false
                  StatusBadge = "Scheduled" } ] } ]

    let private horizonDays = 18 // Sep 12 to Sep 30, 2026
    let private days = [ 0 .. horizonDays - 1 ]

    let private statusColor (status: CapacityUtilizationStatus) =
        match status with
        | CapacityUtilizationStatus.Optimal -> "var(--rz-success)"
        | CapacityUtilizationStatus.NearCapacity -> "var(--rz-warning)"
        | CapacityUtilizationStatus.Overloaded -> "var(--rz-danger)"
        | CapacityUtilizationStatus.Underutilized -> "var(--rz-info)"

    let private statusBadgeStyle (status: CapacityUtilizationStatus) =
        match status with
        | CapacityUtilizationStatus.Optimal ->
            "color: var(--rz-success); background: var(--rz-success-lighter); border: 1px solid var(--rz-success-light);"
        | CapacityUtilizationStatus.NearCapacity ->
            "color: var(--rz-warning-dark); background: var(--rz-warning-lighter); border: 1px solid var(--rz-warning-light);"
        | CapacityUtilizationStatus.Overloaded ->
            "color: var(--rz-danger); background: var(--rz-danger-lighter); border: 1px solid var(--rz-danger-light);"
        | CapacityUtilizationStatus.Underutilized ->
            "color: var(--rz-info); background: var(--rz-info-lighter); border: 1px solid var(--rz-info-light);"

    let renderGanttTimeline (selectedOrder: string option) (onSelectOrder: string -> unit) : Node =
        div {
            attr.style "display: flex; flex-direction: column; gap: 12px; height: 100%; min-height: 480px;"

            // Timeline Header Days
            div {
                attr.style "display: flex; align-items: center; border-bottom: 2px solid var(--rz-border-color); padding-bottom: 8px;"

                div {
                    attr.style "width: 280px; min-width: 280px; font-weight: 700; font-size: 13px; color: var(--rz-text-color);"
                    "Work Center / Resource"
                }

                div {
                    attr.style "flex: 1; display: grid; grid-template-columns: repeat(18, 1fr); gap: 2px; text-align: center;"

                    for dayIdx in days do
                        let calDay = 12 + dayIdx
                        let isWeekend = (dayIdx % 7 = 1) || (dayIdx % 7 = 2)
                        let isCurrent = dayIdx = 0

                        div {
                            attr.style(
                                if isCurrent then
                                    "padding: 4px 0; font-size: 11px; font-weight: 800; border-radius: 4px; background: rgba(var(--rz-primary-rgb), 0.15); color: var(--rz-primary); border: 1px solid var(--rz-primary);"
                                elif isWeekend then
                                    "padding: 4px 0; font-size: 11px; font-weight: 500; border-radius: 4px; background: rgba(0,0,0,0.03); color: var(--rz-text-secondary-color);"
                                else
                                    "padding: 4px 0; font-size: 11px; font-weight: 600; color: var(--rz-text-color);"
                            )
                            $"Sep {calDay}"
                        }
                }
            }

            // Resource Swimlanes
            div {
                attr.style "display: flex; flex-direction: column; gap: 16px; overflow-y: auto;"

                for wc in schedules do
                    div {
                        attr.style
                            """
                            display: flex;
                            align-items: stretch;
                            background: rgba(var(--rz-base-background-color-rgb), 0.6);
                            border: 1px solid var(--rz-border-color);
                            border-radius: 8px;
                            padding: 8px 12px;
                            transition: box-shadow 0.2s ease;
                            """

                        // Work Center Metadata Card
                        div {
                            attr.style "width: 280px; min-width: 280px; display: flex; flex-direction: column; justify-content: center; gap: 4px; padding-right: 16px; border-right: 1px solid var(--rz-border-color);"

                            div {
                                attr.style "display: flex; align-items: center; justify-content: space-between;"
                                span {
                                    attr.style "font-weight: 700; font-size: 13px; color: var(--rz-text-color);"
                                    wc.ResourceId
                                }
                                span {
                                    attr.style $"font-size: 11px; font-weight: 700; padding: 2px 6px; border-radius: 4px; {statusBadgeStyle wc.Status}"
                                    $"{wc.UtilizationPct:F1}%%"
                                }
                            }

                            div {
                                attr.style "font-size: 11px; color: var(--rz-text-secondary-color);"
                                wc.DisplayName
                            }

                            div {
                                attr.style "display: flex; align-items: center; justify-content: space-between; font-size: 11px; margin-top: 2px;"
                                span {
                                    attr.style "color: var(--rz-text-secondary-color);"
                                    $"Load: {wc.LoadHours:F0}h / {wc.AvailableHours:F0}h"
                                }
                                if wc.Bottleneck = BottleneckAssessmentStatus.CurrentBottleneck then
                                    span {
                                        attr.style "color: var(--rz-danger); font-weight: 700; font-size: 10px;"
                                        "BOTTLENECK"
                                    }
                            }

                            // Micro progress bar
                            div {
                                attr.style "width: 100%; height: 5px; background: var(--rz-secondary-lighter); border-radius: 3px; overflow: hidden; margin-top: 2px;"
                                div {
                                    let fillWidth = min 100.0 (float wc.UtilizationPct)
                                    attr.style $"width: {fillWidth}%%; height: 100%%; background: {statusColor wc.Status}; border-radius: 3px;"
                                }
                            }
                        }

                        // Interactive Timeline Lane
                        div {
                            attr.style "flex: 1; position: relative; min-height: 80px; padding-left: 8px; display: flex; align-items: center;"

                            // Grid background guidelines
                            div {
                                attr.style "position: absolute; inset: 0; display: grid; grid-template-columns: repeat(18, 1fr); gap: 2px; pointer-events: none;"
                                for dayIdx in days do
                                    let isWeekend = (dayIdx % 7 = 1) || (dayIdx % 7 = 2)
                                    div {
                                        attr.style(
                                            if isWeekend then "background: rgba(0,0,0,0.02); border-right: 1px dashed rgba(0,0,0,0.05);"
                                            else "border-right: 1px dashed rgba(0,0,0,0.06);"
                                        )
                                    }
                            }

                            // Task bars
                            for task in wc.Tasks do
                                let leftPct = (float task.StartDay / float horizonDays) * 100.0
                                let widthPct = (float task.DurationDays / float horizonDays) * 100.0
                                let isSelected = selectedOrder = Some task.OrderId

                                let barBg =
                                    if task.IsOverload then "var(--rz-danger)"
                                    elif isSelected then "var(--rz-primary)"
                                    else "var(--rz-secondary)"

                                let textColor =
                                    if task.IsOverload then "var(--rz-on-danger)"
                                    elif isSelected then "var(--rz-on-primary)"
                                    else "var(--rz-on-secondary)"

                                div {
                                    attr.style
                                        $"""
                                        position: absolute;
                                        left: {leftPct:F2}%%;
                                        width: {widthPct:F2}%%;
                                        height: 38px;
                                        background: {barBg};
                                        border-radius: 6px;
                                        color: {textColor};
                                        display: flex;
                                        flex-direction: column;
                                        justify-content: center;
                                        padding: 0 8px;
                                        box-shadow: 0 2px 6px rgba(0,0,0,0.18);
                                        cursor: pointer;
                                        overflow: hidden;
                                        box-sizing: border-box;
                                        border: {if isSelected then "2px solid var(--rz-warning)" else "1px solid var(--rz-border-color)"};
                                        z-index: {if isSelected then "10" else "2"};
                                        transition: transform 0.15s ease, box-shadow 0.15s ease;
                                        """

                                    on.click (fun _ -> onSelectOrder task.OrderId)

                                    // Task inner info
                                    div {
                                        attr.style "display: flex; align-items: center; justify-content: space-between; width: 100%; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;"
                                        span {
                                            attr.style "font-weight: 700; font-size: 11px; overflow: hidden; text-overflow: ellipsis;"
                                            task.OrderId
                                        }
                                        span {
                                            attr.style "font-size: 10px; opacity: 0.9; margin-left: 4px;"
                                            $"{task.PlannedHours:F0}h"
                                        }
                                    }

                                    div {
                                        attr.style "display: flex; align-items: center; justify-content: space-between; font-size: 9px; opacity: 0.85; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;"
                                        span {
                                            attr.style "overflow: hidden; text-overflow: ellipsis;"
                                            task.Item
                                        }
                                        span {
                                            if task.IsOverload then "OVERLOAD"
                                            else $"{task.ProgressPct:F0}%%"
                                        }
                                    }

                                    // Internal progress fill line
                                    div {
                                        attr.style "position: absolute; bottom: 0; left: 0; height: 3px; background: rgba(255,255,255,0.3); width: 100%;"
                                        div {
                                            attr.style $"height: 100%%; width: {task.ProgressPct}%%; background: var(--rz-success);"
                                        }
                                    }
                                }
                        }
                    }
            }

            // Bottom Aggregate Workload Histogram
            div {
                attr.style "margin-top: 12px; padding: 12px; background: rgba(0,0,0,0.02); border: 1px solid var(--rz-border-color); border-radius: 8px;"

                div {
                    attr.style "display: flex; align-items: center; justify-content: space-between; margin-bottom: 8px;"
                    span {
                        attr.style "font-weight: 700; font-size: 12px; color: var(--rz-text-color);"
                        "Daily Multi-Resource Workload Histogram (Planned Load vs Fleet Safe Capacity)"
                    }
                    div {
                        attr.style "display: flex; align-items: center; gap: 12px; font-size: 11px; color: var(--rz-text-secondary-color);"
                        span {
                            attr.style "display: flex; align-items: center; gap: 4px;"
                            span { attr.style "display: inline-block; width: 10px; height: 10px; border-radius: 2px; background: var(--rz-success);" }
                            "Optimal Load (<85%)"
                        }
                        span {
                            attr.style "display: flex; align-items: center; gap: 4px;"
                            span { attr.style "display: inline-block; width: 10px; height: 10px; border-radius: 2px; background: var(--rz-danger);" }
                            "Bottleneck Spike (>100%)"
                        }
                    }
                }

                div {
                    attr.style "display: grid; grid-template-columns: repeat(18, 1fr); gap: 4px; height: 50px; align-items: flex-end;"
                    for dayIdx in days do
                        // calculate simulated aggregate hours load on dayIdx
                        let loadOnDay =
                            schedules
                            |> List.sumBy (fun wc ->
                                wc.Tasks
                                |> List.filter (fun t -> dayIdx >= t.StartDay && dayIdx < t.StartDay + t.DurationDays)
                                |> List.sumBy (fun t -> t.PlannedHours / decimal t.DurationDays))

                        let maxDailyCapacity = 24.0m // 3 work centers * 8h shift
                        let fillPct = min 100.0 (float (loadOnDay / maxDailyCapacity * 100.0m))
                        let isSpike = loadOnDay > maxDailyCapacity

                        div {
                            attr.style "display: flex; flex-direction: column; align-items: center; height: 100%; justify-content: flex-end;"
                            div {
                                attr.style
                                    $"""
                                    width: 100%%;
                                    height: {fillPct:F0}%%;
                                    background: {if isSpike then "var(--rz-danger)" else "var(--rz-primary)"};
                                    border-radius: 3px 3px 0 0;
                                    transition: height 0.3s ease;
                                    """
                                attr.title $"Sep {12 + dayIdx}: {loadOnDay:F1}h Load / {maxDailyCapacity:F0}h Daily Fleet Shift"
                            }
                        }
                }
            }
        }

    let renderOperatingEnvelopes () : Node =
        div {
            attr.style "display: grid; grid-template-columns: repeat(auto-fit, minmax(360px, 1fr)); gap: 16px; padding: 8px 0;"

            for wc in schedules do
                div {
                    attr.style
                        """
                        background: var(--rz-base-background-color);
                        border: 1px solid var(--rz-border-color);
                        border-radius: 8px;
                        padding: 16px;
                        display: flex;
                        flex-direction: column;
                        gap: 12px;
                        box-shadow: 0 2px 6px rgba(0,0,0,0.03);
                        """

                    // Header
                    div {
                        attr.style "display: flex; align-items: center; justify-content: space-between; border-bottom: 1px solid var(--rz-border-color); padding-bottom: 8px;"
                        div {
                            span {
                                attr.style "font-weight: 700; font-size: 14px; color: var(--rz-text-color);"
                                wc.ResourceId
                            }
                            div {
                                attr.style "font-size: 11px; color: var(--rz-text-secondary-color);"
                                wc.DisplayName
                            }
                        }
                        span {
                            attr.style $"font-size: 11px; font-weight: 700; padding: 2px 8px; border-radius: 4px; {statusBadgeStyle wc.Status}"
                            string wc.Status
                        }
                    }

                    // Envelope Stats Cards
                    div {
                        attr.style "display: grid; grid-template-columns: repeat(3, 1fr); gap: 8px; text-align: center;"

                        div {
                            attr.style "padding: 8px; background: rgba(0,0,0,0.02); border-radius: 6px;"
                            div { attr.style "font-size: 10px; color: var(--rz-text-secondary-color);"; "Target Upper" }
                            div { attr.style "font-size: 13px; font-weight: 700; color: var(--rz-text-color);"; $"{wc.Envelope.TargetUpperHours:F0}h" }
                        }

                        div {
                            attr.style "padding: 8px; background: rgba(0,0,0,0.02); border-radius: 6px;"
                            div { attr.style "font-size: 10px; color: var(--rz-text-secondary-color);"; "Max Planned" }
                            div { attr.style "font-size: 13px; font-weight: 700; color: var(--rz-text-color);"; $"{wc.Envelope.MaxPlannedHours:F0}h" }
                        }

                        div {
                            attr.style "padding: 8px; background: rgba(0,0,0,0.02); border-radius: 6px;"
                            div { attr.style "font-size: 10px; color: var(--rz-text-secondary-color);"; "Absolute Cap" }
                            div { attr.style "font-size: 13px; font-weight: 700; color: var(--rz-text-color);"; $"{wc.Envelope.AbsoluteCapacityHours:F0}h" }
                        }
                    }

                    // Operating Envelope Bar Visualizer
                    div {
                        attr.style "display: flex; flex-direction: column; gap: 4px; margin-top: 4px;"
                        div {
                            attr.style "display: flex; justify-content: space-between; font-size: 11px;"
                            span { attr.style "color: var(--rz-text-secondary-color);"; "Current Scheduled Load" }
                            span {
                                attr.style $"font-weight: 700; color: {statusColor wc.Status};"
                                $"{wc.LoadHours:F1}h ({wc.UtilizationPct:F1}%%)"
                            }
                        }

                        div {
                            attr.style "width: 100%; height: 22px; background: var(--rz-secondary-lighter); border-radius: 4px; position: relative; overflow: hidden; border: 1px solid var(--rz-border-color);"

                            // Target Upper Guideline
                            let targetPct = (float (wc.Envelope.TargetUpperHours / wc.Envelope.AbsoluteCapacityHours)) * 100.0
                            div {
                                attr.style $"position: absolute; left: {targetPct:F1}%%; top: 0; bottom: 0; width: 2px; background: var(--rz-warning); z-index: 2;"
                                attr.title $"Target Upper Buffer: {wc.Envelope.TargetUpperHours}h"
                            }

                            // Max Planned Guideline
                            let maxPlannedPct = (float (wc.Envelope.MaxPlannedHours / wc.Envelope.AbsoluteCapacityHours)) * 100.0
                            div {
                                attr.style $"position: absolute; left: {maxPlannedPct:F1}%%; top: 0; bottom: 0; width: 2px; background: var(--rz-danger); z-index: 2;"
                                attr.title $"Max Planned Capacity: {wc.Envelope.MaxPlannedHours}h"
                            }

                            // Load Bar
                            let loadFillPct = min 100.0 ((float (wc.LoadHours / wc.Envelope.AbsoluteCapacityHours)) * 100.0)
                            div {
                                attr.style
                                    $"""
                                    height: 100%%;
                                    width: {loadFillPct:F1}%%;
                                    background: {statusColor wc.Status};
                                    opacity: 0.85;
                                    transition: width 0.3s ease;
                                    """
                            }
                        }

                        div {
                            attr.style "display: flex; justify-content: space-between; font-size: 9px; color: var(--rz-text-secondary-color);"
                            span { "0h (Idle)" }
                            span { $"Safe: {wc.Envelope.TargetUpperHours:F0}h" }
                            span { $"Max: {wc.Envelope.MaxPlannedHours:F0}h" }
                            span { $"Ceiling: {wc.Envelope.AbsoluteCapacityHours:F0}h" }
                        }
                    }

                    // Bottleneck & Risk Diagnostics
                    div {
                        attr.style "padding: 8px; border-radius: 6px; font-size: 11px; display: flex; flex-direction: column; gap: 4px; background: rgba(0,0,0,0.02);"
                        div {
                            attr.style "display: flex; justify-content: space-between;"
                            span { attr.style "color: var(--rz-text-secondary-color);"; "Constraint Assessment:" }
                            span { attr.style "font-weight: 600;"; string wc.Constraint }
                        }
                        div {
                            attr.style "display: flex; justify-content: space-between;"
                            span { attr.style "color: var(--rz-text-secondary-color);"; "Bottleneck Role:" }
                            span { attr.style "font-weight: 600;"; string wc.Bottleneck }
                        }
                        div {
                            attr.style "display: flex; justify-content: space-between;"
                            span { attr.style "color: var(--rz-text-secondary-color);"; "Single Point of Failure:" }
                            span {
                                attr.style(if wc.OverloadRisk = CapacityRiskLevel.Critical then "color: var(--rz-danger); font-weight: 700;" else "color: var(--rz-text-color);")
                                if wc.OverloadRisk = CapacityRiskLevel.Critical then "YES (High Vulnerability)" else "No (Alternate Resource Available)"
                            }
                        }
                    }
                }
        }

    let render (ctx: ComponentRenderContext) (dispatch: ComponentEvent -> unit) : Node =
        let activeTab =
            match ctx.Configuration.Parameters |> Map.tryFind "ActiveStudioTab" with
            | Some (StringValue "Envelopes") -> OperatingEnvelopes
            | Some (StringValue "Table") -> PositionTable
            | _ -> GanttTimeline

        let selectedOrder = ctx.Selection

        div {
            attr.style
                """
                display: flex;
                flex-direction: column;
                height: 100%;
                width: 100%;
                background: var(--rz-base-background-color);
                border: 1px solid var(--rz-border-color);
                border-radius: 8px;
                padding: 12px 16px;
                box-sizing: border-box;
                overflow: hidden;
                """

            // Top Studio Toolbar & KPI Metrics
            div {
                attr.style "display: flex; align-items: center; justify-content: space-between; border-bottom: 1px solid var(--rz-border-color); padding-bottom: 10px; margin-bottom: 12px; flex-wrap: wrap; gap: 8px;"

                // Left: Title & Scope
                div {
                    attr.style "display: flex; align-items: center; gap: 8px;"
                    span {
                        attr.``class`` "rzi"
                        attr.style "font-size: 1.4rem; color: var(--rz-primary);"
                        "view_timeline"
                    }
                    div {
                        div {
                            attr.style "font-weight: 700; font-size: 15px; color: var(--rz-text-color);"
                            "Capacity Planning Studio"
                        }
                        div {
                            attr.style "font-size: 11px; color: var(--rz-text-secondary-color);"
                            "Horizon: Sep 12 - Sep 30, 2026 · Planning Scope: PS-GLOBAL-SUPPLY-CHAIN"
                        }
                    }
                }

                // Middle: Quick Fleet Badges
                div {
                    attr.style "display: flex; align-items: center; gap: 8px;"

                    span {
                        attr.style "padding: 3px 8px; border-radius: 12px; font-size: 11px; font-weight: 600; background: rgba(220, 53, 69, 0.12); color: var(--rz-danger); border: 1px solid rgba(220, 53, 69, 0.3);"
                        "1 Bottleneck Active: WC-ASSEMBLY-LINE-01 (115%)"
                    }

                    span {
                        attr.style "padding: 3px 8px; border-radius: 12px; font-size: 11px; font-weight: 600; background: rgba(40, 167, 69, 0.12); color: var(--rz-success); border: 1px solid rgba(40, 167, 69, 0.3);"
                        "Fleet Utilization: 96.1%"
                    }
                }

                // Right: View Tabs & Actions
                div {
                    attr.style "display: flex; align-items: center; gap: 6px;"

                    Rz.button(
                        "Gantt Timeline",
                        onClick = (fun _ ->
                            let updated = { ctx.Configuration with Parameters = ctx.Configuration.Parameters |> Map.add "ActiveStudioTab" (StringValue "Gantt") }
                            dispatch (ConfigurationChangeRequested updated)),
                        style = (if activeTab = GanttTimeline then ButtonStyle.Primary else ButtonStyle.Light),
                        icon = "view_timeline"
                    )

                    Rz.button(
                        "Operating Envelopes",
                        onClick = (fun _ ->
                            let updated = { ctx.Configuration with Parameters = ctx.Configuration.Parameters |> Map.add "ActiveStudioTab" (StringValue "Envelopes") }
                            dispatch (ConfigurationChangeRequested updated)),
                        style = (if activeTab = OperatingEnvelopes then ButtonStyle.Primary else ButtonStyle.Light),
                        icon = "bar_chart"
                    )

                    Rz.button(
                        "Position Grid",
                        onClick = (fun _ ->
                            let updated = { ctx.Configuration with Parameters = ctx.Configuration.Parameters |> Map.add "ActiveStudioTab" (StringValue "Table") }
                            dispatch (ConfigurationChangeRequested updated)),
                        style = (if activeTab = PositionTable then ButtonStyle.Primary else ButtonStyle.Light),
                        icon = "table_rows"
                    )

                    Rz.iconButton(
                        "refresh",
                        onClick = (fun _ -> dispatch RefreshRequested),
                        tooltip = "Refresh telemetry"
                    )
                }
            }

            // Main Studio Body
            div {
                attr.style "flex: 1; min-height: 0; overflow-y: auto;"

                match activeTab with
                | GanttTimeline ->
                    renderGanttTimeline selectedOrder (fun ord -> dispatch (SelectionChanged (Some ord)))
                | OperatingEnvelopes ->
                    renderOperatingEnvelopes ()
                | PositionTable ->
                    UniversalEnterpriseGrid.render ctx dispatch
            }
        }

    let runtime: ComponentRuntime = { Render = render }

    let registration: ComponentRegistration =
        { Definition = definition
          Runtime = runtime }
