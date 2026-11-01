module Medhavi.Client.Supply.Capacity.CapacityStudio

open System
open Bolero
open Bolero.Html
open Radzen
open Medhavi.Contracts.Supply
open Medhavi.Client
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client.Rendering
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Data

let definitionId = ComponentDefId.literal "capacity-studio"
let storeId = CapacityGrid.storeId

let definition: ComponentDefinition =
    {
        Id = definitionId
        DisplayName = "Capacity Planning Studio (Gantt & Histograms)"
        Icon = "view_timeline"
        Category = ComponentCategory.Visualization
        RequiredSchemaCapabilities = [ "Tabular" ]
        DefaultGridSpan = GridSpan.literal 12 8
        DefaultSchemaId = Some CapacityGrid.schemaId
        DefaultStoreId = Some storeId
        IsResizable = true
        IsDraggable = true
        SelectionMode = SelectionMode.SingleSelection
    }

type StudioTab =
    | GanttTimeline
    | OperatingEnvelopes
    | PositionTable

type WorkCenterTask =
    {
        OrderId: string
        Item: string
        StartDay: int
        DurationDays: int
        PlannedHours: decimal
        ProgressPct: float
        IsOverload: bool
        StatusBadge: string
    }

type WorkCenterSchedule =
    {
        ResourceId: string
        DisplayName: string
        AvailableHours: decimal
        LoadHours: decimal
        UtilizationPct: decimal
        Status: CapacityUtilizationStatus
        Constraint: CapacityConstraintStatus
        Bottleneck: BottleneckAssessmentStatus
        OverloadRisk: CapacityRiskLevel
        Envelope: CapacityOperatingEnvelopeDto
        Tasks: WorkCenterTask list
    }

let private defaultEnvelope (availHours: decimal) : CapacityOperatingEnvelopeDto =
    let safeAvail = max 1.0m availHours

    {
        MinEconomicHours = safeAvail * 0.25m
        TargetLowerHours = safeAvail * 0.75m
        TargetUpperHours = safeAvail * 0.90m
        MaxPlannedHours = safeAvail
        AbsoluteCapacityHours = max (safeAvail * 1.125m) 1.0m
        OvertimeThresholdHours = safeAvail * 0.95m
    }

let private tasksForWc
    (resId: string)
    (loadHours: decimal)
    (availHours: decimal)
    (utilPct: decimal)
    : WorkCenterTask list =
    if loadHours <= 0m then
        []
    else
        let isOverloaded = loadHours > availHours
        let baseHours = min loadHours availHours
        let overloadHours = max 0m (loadHours - availHours)
        let baseDuration = min 12 (max 3 (int(Math.Ceiling(float baseHours / 16.0))))

        let baseTask =
            {
                OrderId = sprintf "WO-%s-01" resId
                Item = sprintf "PROD-%s" resId
                StartDay = 2
                DurationDays = baseDuration
                PlannedHours = baseHours
                ProgressPct = min 100.0 (float(baseHours / (if availHours > 0m then availHours else 1m) * 65.0m))
                IsOverload = false
                StatusBadge = if utilPct >= 90m then "Near Capacity" else "In Production"
            }

        if isOverloaded && overloadHours > 0m then
            let ovDuration = max 2 (int(Math.Ceiling(float overloadHours / 16.0)))

            let ovTask =
                {
                    OrderId = sprintf "WO-%s-02" resId
                    Item = sprintf "OVERLOAD-%s" resId
                    StartDay = min 16 (baseTask.StartDay + baseTask.DurationDays - 1)
                    DurationDays = ovDuration
                    PlannedHours = overloadHours
                    ProgressPct = 0.0
                    IsOverload = true
                    StatusBadge = "Conflict Overload"
                }

            [ baseTask; ovTask ]
        else
            [ baseTask ]

let private cleanResourceId (raw: string) : string =
    if String.IsNullOrWhiteSpace raw then "WC-ASSEMBLY-LINE-01"
    else
        let s = raw.Trim()
        let s =
            if s.StartsWith("PlanningScopeId", StringComparison.OrdinalIgnoreCase) then
                s.Substring(15).Trim().Trim('"', '\'')
            else s
        let s = s.Trim('"', '\'')
        if s.StartsWith("PS-", StringComparison.OrdinalIgnoreCase) then
            "WC-PLANT-FLEET"
        else s

let private resourceGroupOf (resId: string) : string * string =
    let upper = resId.ToUpperInvariant()
    if upper.Contains("ASSEMBLY") || upper.Contains("ASSY") then
        "Final Assembly Lines", "build"
    elif upper.Contains("PACKAG") || upper.Contains("PACK") then
        "Packaging & Finishing Line", "inventory_2"
    elif upper.Contains("MACHIN") || upper.Contains("CNC") || upper.Contains("SMT") then
        "Machining & Precision Cells", "precision_manufacturing"
    elif upper.Contains("FLEET") || upper.Contains("PLANT") then
        "Primary Manufacturing Fleet", "domain"
    else
        "Core Operations & Work Centers", "settings"

type ResourceGroupSchedule =
    {
        GroupId: string
        GroupName: string
        Icon: string
        TotalLoadHours: decimal
        TotalAvailableHours: decimal
        UtilizationPct: decimal
        HasBottleneck: bool
        Resources: WorkCenterSchedule list
    }

let private groupSchedules (schedules: WorkCenterSchedule list) : ResourceGroupSchedule list =
    schedules
    |> List.groupBy (fun s -> resourceGroupOf s.ResourceId)
    |> List.map (fun ((grpName, icon), wcs) ->
        let totalLoad = wcs |> List.sumBy (fun w -> w.LoadHours)
        let totalAvail = wcs |> List.sumBy (fun w -> w.AvailableHours)
        let utilPct =
            if totalAvail > 0m then (totalLoad / totalAvail) * 100.0m
            else 0.0m
        let hasBottleneck =
            wcs |> List.exists (fun w ->
                w.Bottleneck = BottleneckAssessmentStatus.CurrentBottleneck
                || w.Status = CapacityUtilizationStatus.Overloaded)
        {
            GroupId = grpName.Replace(" ", "-").ToLowerInvariant()
            GroupName = grpName
            Icon = icon
            TotalLoadHours = totalLoad
            TotalAvailableHours = totalAvail
            UtilizationPct = utilPct
            HasBottleneck = hasBottleneck
            Resources = wcs
        })

let private parseRowToSchedule (row: UniversalRow) : WorkCenterSchedule =
    let getCellText col =
        match Map.tryFind col row.Cells with
        | Some(TextValue t) -> t
        | Some(EnumValue e) -> e
        | _ -> ""

    let getCellNumber col defVal =
        match Map.tryFind col row.Cells with
        | Some(NumberValue n) -> n
        | _ -> defVal

    let resId =
        let id = getCellText "PhysicalResourceId"
        cleanResourceId (if String.IsNullOrWhiteSpace id then row.Key else id)

    let availHours = getCellNumber "AvailableCapacityHours" 160.0m
    let loadHours = getCellNumber "CurrentLoadHours" 0.0m

    let utilPct =
        let u = getCellNumber "CurrentUtilizationPercentage" 0.0m

        if u > 0.0m then u
        elif availHours > 0.0m then (loadHours / availHours) * 100.0m
        else 0.0m

    let status =
        match getCellText "UtilizationStatus" with
        | "Optimal" -> CapacityUtilizationStatus.Optimal
        | "NearCapacity" -> CapacityUtilizationStatus.NearCapacity
        | "Overloaded" -> CapacityUtilizationStatus.Overloaded
        | "Underutilized" -> CapacityUtilizationStatus.Underutilized
        | _ ->
            if utilPct > 100.0m then CapacityUtilizationStatus.Overloaded
            elif utilPct >= 85.0m then CapacityUtilizationStatus.NearCapacity
            elif utilPct < 50.0m then CapacityUtilizationStatus.Underutilized
            else CapacityUtilizationStatus.Optimal

    let constraintStatus =
        match getCellText "ConstraintStatus" with
        | "ActiveConstraint" -> CapacityConstraintStatus.ActiveConstraint
        | "NearConstraint" -> CapacityConstraintStatus.NearConstraint
        | _ ->
            if utilPct >= 100.0m then CapacityConstraintStatus.ActiveConstraint
            elif utilPct >= 85.0m then CapacityConstraintStatus.NearConstraint
            else CapacityConstraintStatus.NonConstraint

    let bottleneck =
        match getCellText "BottleneckAssessment" with
        | "CurrentBottleneck" -> BottleneckAssessmentStatus.CurrentBottleneck
        | "PotentialBottleneck" -> BottleneckAssessmentStatus.PotentialBottleneck
        | _ ->
            if utilPct > 100.0m then BottleneckAssessmentStatus.CurrentBottleneck
            elif utilPct >= 90.0m then BottleneckAssessmentStatus.PotentialBottleneck
            else BottleneckAssessmentStatus.NotABottleneck

    let overloadRisk =
        match getCellText "OverloadRisk" with
        | "Critical" -> CapacityRiskLevel.Critical
        | "High" -> CapacityRiskLevel.High
        | "Medium" -> CapacityRiskLevel.Medium
        | "Low" -> CapacityRiskLevel.Low
        | _ ->
            if utilPct > 110.0m then CapacityRiskLevel.Critical
            elif utilPct > 100.0m then CapacityRiskLevel.High
            elif utilPct >= 90.0m then CapacityRiskLevel.Medium
            else CapacityRiskLevel.Low

    let env = defaultEnvelope availHours
    let tasks = tasksForWc resId loadHours availHours utilPct

    {
        ResourceId = resId
        DisplayName = sprintf "Work Center %s" resId
        AvailableHours = availHours
        LoadHours = loadHours
        UtilizationPct = utilPct
        Status = status
        Constraint = constraintStatus
        Bottleneck = bottleneck
        OverloadRisk = overloadRisk
        Envelope = env
        Tasks = tasks
    }

let private horizonDays = 18
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

let renderGanttTimeline
    (schedules: WorkCenterSchedule list)
    (selectedResource: string option)
    (collapsedGroups: Set<string>)
    (onToggleGroup: string -> unit)
    (onSelectResource: string -> unit)
    : Node =
    if schedules.IsEmpty then
        div {
            attr.style
                "display: flex; flex-direction: column; align-items: center; justify-content: center; height: 320px; color: var(--rz-text-secondary-color); font-size: 13px; gap: 8px;"

            span {
                attr.``class`` "rzi"
                attr.style "font-size: 32px; color: var(--rz-text-disabled-color);"
                "view_timeline"
            }

            "No active capacity positions available. Ingest telemetry or run assessment to populate schedule."
        }
    else
        let baseDate = DateTime.UtcNow.Date
        let grouped = groupSchedules schedules

        div {
            attr.style "display: flex; flex-direction: column; gap: 8px; height: 100%; min-height: 420px;"

            // Timeline Header Days
            div {
                attr.style
                    "display: flex; align-items: center; border-bottom: 2px solid var(--rz-border-color); padding-bottom: 6px;"

                div {
                    attr.style
                        "width: 270px; min-width: 270px; font-weight: 700; font-size: 12px; color: var(--rz-text-color); display: flex; align-items: center; gap: 6px; padding-left: 6px;"

                    span {
                        attr.``class`` "rzi"
                        attr.style "font-size: 16px; color: var(--rz-primary);"
                        "account_tree"
                    }
                    "Resource Hierarchy & Work Centers"
                }

                div {
                    attr.style
                        "flex: 1; display: grid; grid-template-columns: repeat(18, 1fr); gap: 2px; text-align: center;"

                    for dayIdx in days do
                        let targetDate = baseDate.AddDays(float dayIdx)

                        let isWeekend =
                            targetDate.DayOfWeek = DayOfWeek.Saturday || targetDate.DayOfWeek = DayOfWeek.Sunday

                        let isCurrent = dayIdx = 0

                        div {
                            attr.style(
                                if isCurrent then
                                    "padding: 3px 0; font-size: 10.5px; font-weight: 800; border-radius: 4px; background: rgba(var(--rz-primary-rgb), 0.15); color: var(--rz-primary); border: 1px solid var(--rz-primary);"
                                elif isWeekend then
                                    "padding: 3px 0; font-size: 10.5px; font-weight: 500; border-radius: 4px; background: rgba(0,0,0,0.03); color: var(--rz-text-secondary-color);"
                                else
                                    "padding: 3px 0; font-size: 10.5px; font-weight: 600; color: var(--rz-text-color);"
                            )

                            targetDate.ToString("MMM dd")
                        }
                }
            }

            // Hierarchical Swimlanes: Resource Group -> Physical Resource
            div {
                attr.style "display: flex; flex-direction: column; gap: 6px; overflow-y: auto; flex: 1; min-height: 0;"

                for grp in grouped do
                    let isGroupCollapsed = collapsedGroups.Contains grp.GroupId

                    // 1. Resource Group Header Row (Compact ~30px)
                    div {
                        attr.style
                            "display: flex; align-items: center; justify-content: space-between; padding: 4px 10px; background: rgba(var(--rz-primary-rgb), 0.05); border-left: 3px solid var(--rz-primary); border-radius: 4px; cursor: pointer; user-select: none; transition: background 0.15s ease;"
                        on.click(fun _ -> onToggleGroup grp.GroupId)

                        div {
                            attr.style "display: flex; align-items: center; gap: 8px;"

                            span {
                                attr.style "font-size: 11px; font-weight: 800; color: var(--rz-primary); width: 12px; display: inline-block;"
                                if isGroupCollapsed then "▶" else "▼"
                            }

                            span {
                                attr.``class`` "rzi"
                                attr.style "font-size: 15px; color: var(--rz-primary);"
                                grp.Icon
                            }

                            span {
                                attr.style "font-weight: 700; font-size: 12.5px; color: var(--rz-text-color);"
                                sprintf "%s (%d Work Centers)" grp.GroupName grp.Resources.Length
                            }

                            if grp.HasBottleneck then
                                span {
                                    attr.style "padding: 1px 6px; border-radius: 3px; font-size: 9.5px; font-weight: 700; background: var(--rz-danger-lighter); color: var(--rz-danger); border: 1px solid var(--rz-danger-light);"
                                    "⚠ GROUP BOTTLENECK"
                                }
                        }

                        div {
                            attr.style "display: flex; align-items: center; gap: 10px; font-size: 11px;"

                            span {
                                attr.style "color: var(--rz-text-secondary-color);"
                                sprintf "Group Load: %.0fh / %.0fh" grp.TotalLoadHours grp.TotalAvailableHours
                            }

                            span {
                                let badgeColor =
                                    if grp.UtilizationPct > 100.0m then "var(--rz-danger)"
                                    elif grp.UtilizationPct >= 85.0m then "var(--rz-warning-dark)"
                                    else "var(--rz-success)"
                                let badgeBg =
                                    if grp.UtilizationPct > 100.0m then "var(--rz-danger-lighter)"
                                    elif grp.UtilizationPct >= 85.0m then "var(--rz-warning-lighter)"
                                    else "var(--rz-success-lighter)"
                                attr.style (sprintf "font-size: 10.5px; font-weight: 700; padding: 2px 7px; border-radius: 10px; color: %s; background: %s;" badgeColor badgeBg)
                                sprintf "%.1f%% Util" grp.UtilizationPct
                            }
                        }
                    }

                    // 2. Child Physical Resources (Compact ~40px Swimlane)
                    if not isGroupCollapsed then
                        for wc in grp.Resources do
                            let isSelected = selectedResource = Some wc.ResourceId

                            div {
                                attr.style(
                                    "display: flex; align-items: stretch; border: 1px solid var(--rz-border-color); border-radius: 6px; margin-bottom: 2px; transition: all 0.15s ease; cursor: pointer; " +
                                    (if isSelected then
                                         "background: rgba(var(--rz-primary-rgb), 0.08); border: 2px solid var(--rz-primary); box-shadow: 0 0 0 1px var(--rz-primary);"
                                     else
                                         "background: var(--rz-base-background-color);")
                                )
                                on.click(fun _ -> onSelectResource wc.ResourceId)

                                // Work Center Metadata Compact Card (Left column: 270px)
                                div {
                                    attr.style
                                        "width: 270px; min-width: 270px; display: flex; flex-direction: column; justify-content: center; padding: 4px 10px 4px 22px; border-right: 1px solid var(--rz-border-color); box-sizing: border-box; position: relative;"

                                    // Line 1: Status Dot, Resource ID, Util badge, Bottleneck tag
                                    div {
                                        attr.style "display: flex; align-items: center; justify-content: space-between; gap: 4px;"

                                        div {
                                            attr.style "display: flex; align-items: center; gap: 6px; overflow: hidden;"

                                            span {
                                                let dotClr = statusColor wc.Status
                                                attr.style (sprintf "width: 8px; height: 8px; border-radius: 50%%; background: %s; display: inline-block; flex-shrink: 0;" dotClr)
                                            }

                                            span {
                                                attr.style "font-weight: 700; font-size: 12px; color: var(--rz-text-color); white-space: nowrap; overflow: hidden; text-overflow: ellipsis;"
                                                wc.ResourceId
                                            }
                                        }

                                        div {
                                            attr.style "display: flex; align-items: center; gap: 4px; flex-shrink: 0;"

                                            if wc.Bottleneck = BottleneckAssessmentStatus.CurrentBottleneck then
                                                span {
                                                    attr.style "color: var(--rz-danger); font-size: 9px; font-weight: 800; background: var(--rz-danger-lighter); padding: 1px 4px; border-radius: 2px;"
                                                    "BOTTLENECK"
                                                }

                                            span {
                                                let badgeStyle = statusBadgeStyle wc.Status
                                                attr.style (sprintf "font-size: 10px; font-weight: 700; padding: 1px 5px; border-radius: 3px; %s" badgeStyle)
                                                sprintf "%.1f%%" wc.UtilizationPct
                                            }
                                        }
                                    }

                                    // Line 2: Load Hours & Click-to-focus prompt
                                    div {
                                        attr.style "display: flex; justify-content: space-between; font-size: 10.5px; color: var(--rz-text-secondary-color); margin-top: 1px;"

                                        span { sprintf "Load: %.0fh / %.0fh" wc.LoadHours wc.AvailableHours }

                                        span {
                                            attr.style (if isSelected then "color: var(--rz-primary); font-weight: 600; font-size: 9.5px;" else "font-size: 9.5px;")
                                            if isSelected then "✓ Focused" else "Click to focus"
                                        }
                                    }

                                    // Line 3: Bottom micro progress bar
                                    div {
                                        attr.style "width: 100%; height: 3px; background: var(--rz-secondary-lighter); border-radius: 2px; overflow: hidden; margin-top: 2px;"

                                        div {
                                            let fillWidth = min 100.0 (float wc.UtilizationPct)
                                            let clr = statusColor wc.Status
                                            attr.style (sprintf "width: %.1f%%; height: 100%%; background: %s;" fillWidth clr)
                                        }
                                    }
                                }

                                // Interactive Timeline Lane (Right: 18 Days Grid)
                                div {
                                    attr.style
                                        "flex: 1; position: relative; min-height: 38px; display: flex; align-items: center; padding-left: 4px;"

                                    // Day guidelines
                                    div {
                                        attr.style
                                            "position: absolute; inset: 0; display: grid; grid-template-columns: repeat(18, 1fr); gap: 2px; pointer-events: none;"

                                        for dayIdx in days do
                                            let targetDate = baseDate.AddDays(float dayIdx)
                                            let isWeekend = targetDate.DayOfWeek = DayOfWeek.Saturday || targetDate.DayOfWeek = DayOfWeek.Sunday

                                            div {
                                                attr.style(
                                                    if isWeekend then "background: rgba(0,0,0,0.02); border-right: 1px dashed rgba(0,0,0,0.05);"
                                                    else "border-right: 1px dashed rgba(0,0,0,0.06);"
                                                )
                                            }
                                    }

                                    // Task bars (compact 22px height)
                                    for task in wc.Tasks do
                                        let leftPct = (float task.StartDay / float horizonDays) * 100.0
                                        let widthPct = (float task.DurationDays / float horizonDays) * 100.0
                                        let isTaskOverload = task.IsOverload

                                        let barBg =
                                            if isTaskOverload then "repeating-linear-gradient(45deg, var(--rz-danger), var(--rz-danger) 6px, #b02a37 6px, #b02a37 12px)"
                                            elif wc.Status = CapacityUtilizationStatus.NearCapacity then "var(--rz-warning)"
                                            else "var(--rz-primary)"

                                        let barColor =
                                            if wc.Status = CapacityUtilizationStatus.NearCapacity && not isTaskOverload then "var(--rz-warning-dark)"
                                            else "white"

                                        div {
                                            attr.style(
                                                sprintf
                                                    "position: absolute; left: %.2f%%; width: %.2f%%; height: 22px; background: %s; border-radius: 4px; color: %s; display: flex; align-items: center; padding: 0 6px; box-shadow: 0 1px 3px rgba(0,0,0,0.15); cursor: pointer; overflow: hidden; box-sizing: border-box; z-index: 2; font-size: 10.5px; font-weight: 600; white-space: nowrap; text-overflow: ellipsis;"
                                                    leftPct
                                                    widthPct
                                                    barBg
                                                    barColor
                                            )
                                            attr.title (sprintf "Task: %s · Item: %s · Planned: %.0fh · Duration: %d days · Status: %s" task.OrderId task.Item task.PlannedHours task.DurationDays task.StatusBadge)
                                            on.click(fun _ -> onSelectResource wc.ResourceId)

                                            span {
                                                attr.style "overflow: hidden; text-overflow: ellipsis; white-space: nowrap;"
                                                sprintf "%s · %.0fh" task.OrderId task.PlannedHours
                                            }
                                        }
                                }
                            }
            }

            // Bottom Aggregate Workload Histogram
            div {
                attr.style
                    "margin-top: 12px; padding: 12px; background: rgba(0,0,0,0.02); border: 1px solid var(--rz-border-color); border-radius: 8px;"

                div {
                    attr.style "display: flex; align-items: center; justify-content: space-between; margin-bottom: 8px;"

                    span {
                        attr.style "font-weight: 700; font-size: 12px; color: var(--rz-text-color);"
                        "Daily Multi-Resource Workload Histogram (Planned Load vs Fleet Safe Capacity)"
                    }

                    div {
                        attr.style
                            "display: flex; align-items: center; gap: 12px; font-size: 11px; color: var(--rz-text-secondary-color);"

                        span {
                            attr.style "display: flex; align-items: center; gap: 4px;"

                            span {
                                attr.style
                                    "display: inline-block; width: 10px; height: 10px; border-radius: 2px; background: var(--rz-success);"
                            }

                            "Optimal Load (<85%)"
                        }

                        span {
                            attr.style "display: flex; align-items: center; gap: 4px;"

                            span {
                                attr.style
                                    "display: inline-block; width: 10px; height: 10px; border-radius: 2px; background: var(--rz-danger);"
                            }

                            "Bottleneck Spike (>100%)"
                        }
                    }
                }

                div {
                    attr.style
                        "display: grid; grid-template-columns: repeat(18, 1fr); gap: 4px; height: 50px; align-items: flex-end;"

                    let fleetDailyCapacity =
                        let totalAvail = schedules |> List.sumBy(fun wc -> wc.AvailableHours)
                        max 24.0m (totalAvail / decimal horizonDays)

                    for dayIdx in days do
                        let targetDate = baseDate.AddDays(float dayIdx)

                        let loadOnDay =
                            schedules
                            |> List.sumBy(fun wc ->
                                wc.Tasks
                                |> List.filter(fun t -> dayIdx >= t.StartDay && dayIdx < t.StartDay + t.DurationDays)
                                |> List.sumBy(fun t -> t.PlannedHours / decimal t.DurationDays))

                        let fillPct = min 100.0 (float(loadOnDay / fleetDailyCapacity * 100.0m))
                        let isSpike = loadOnDay > fleetDailyCapacity

                        div {
                            attr.style
                                "display: flex; flex-direction: column; align-items: center; height: 100%; justify-content: flex-end;"

                            div {
                                let bg = if isSpike then "var(--rz-danger)" else "var(--rz-primary)"
                                let dateStr = targetDate.ToString("MMM dd")

                                attr.style(
                                    sprintf
                                        "width: 100%%; height: %.0f%%; background: %s; border-radius: 3px 3px 0 0; transition: height 0.3s ease;"
                                        fillPct
                                        bg
                                )

                                attr.title(
                                    sprintf
                                        "%s: %.1fh Load / %.0fh Daily Fleet Shift"
                                        dateStr
                                        loadOnDay
                                        fleetDailyCapacity
                                )
                            }
                        }
                }
            }
        }

let renderOperatingEnvelopes (schedules: WorkCenterSchedule list) : Node =
    if schedules.IsEmpty then
        div {
            attr.style
                "display: flex; flex-direction: column; align-items: center; justify-content: center; height: 320px; color: var(--rz-text-secondary-color); font-size: 13px; gap: 8px;"

            span {
                attr.``class`` "rzi"
                attr.style "font-size: 32px; color: var(--rz-text-disabled-color);"
                "bar_chart"
            }

            "No active capacity positions available. Ingest telemetry or run assessment to populate envelopes."
        }
    else
        div {
            attr.style
                "display: grid; grid-template-columns: repeat(auto-fit, minmax(360px, 1fr)); gap: 16px; padding: 8px 0;"

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
                        attr.style
                            "display: flex; align-items: center; justify-content: space-between; border-bottom: 1px solid var(--rz-border-color); padding-bottom: 8px;"

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
                            let bStyle = statusBadgeStyle wc.Status

                            attr.style(
                                sprintf
                                    "font-size: 11px; font-weight: 700; padding: 2px 8px; border-radius: 4px; %s"
                                    bStyle
                            )

                            string wc.Status
                        }
                    }

                    // Envelope Stats Cards
                    div {
                        attr.style "display: grid; grid-template-columns: repeat(3, 1fr); gap: 8px; text-align: center;"

                        div {
                            attr.style "padding: 8px; background: rgba(0,0,0,0.02); border-radius: 6px;"

                            div {
                                attr.style "font-size: 10px; color: var(--rz-text-secondary-color);"
                                "Target Upper"
                            }

                            div {
                                attr.style "font-size: 13px; font-weight: 700; color: var(--rz-text-color);"
                                sprintf "%.0fh" wc.Envelope.TargetUpperHours
                            }
                        }

                        div {
                            attr.style "padding: 8px; background: rgba(0,0,0,0.02); border-radius: 6px;"

                            div {
                                attr.style "font-size: 10px; color: var(--rz-text-secondary-color);"
                                "Max Planned"
                            }

                            div {
                                attr.style "font-size: 13px; font-weight: 700; color: var(--rz-text-color);"
                                sprintf "%.0fh" wc.Envelope.MaxPlannedHours
                            }
                        }

                        div {
                            attr.style "padding: 8px; background: rgba(0,0,0,0.02); border-radius: 6px;"

                            div {
                                attr.style "font-size: 10px; color: var(--rz-text-secondary-color);"
                                "Absolute Cap"
                            }

                            div {
                                attr.style "font-size: 13px; font-weight: 700; color: var(--rz-text-color);"
                                sprintf "%.0fh" wc.Envelope.AbsoluteCapacityHours
                            }
                        }
                    }

                    // Operating Envelope Bar Visualizer
                    div {
                        attr.style "display: flex; flex-direction: column; gap: 4px; margin-top: 4px;"

                        div {
                            attr.style "display: flex; justify-content: space-between; font-size: 11px;"

                            span {
                                attr.style "color: var(--rz-text-secondary-color);"
                                "Current Scheduled Load"
                            }

                            span {
                                let clr = statusColor wc.Status
                                attr.style(sprintf "font-weight: 700; color: %s;" clr)
                                sprintf "%.1fh (%.1f%%)" wc.LoadHours wc.UtilizationPct
                            }
                        }

                        div {
                            attr.style
                                "width: 100%; height: 22px; background: var(--rz-secondary-lighter); border-radius: 4px; position: relative; overflow: hidden; border: 1px solid var(--rz-border-color);"

                            let safeCap = max 1.0m wc.Envelope.AbsoluteCapacityHours
                            let targetPct = (float(wc.Envelope.TargetUpperHours / safeCap)) * 100.0

                            div {
                                attr.style(
                                    sprintf
                                        "position: absolute; left: %.1f%%; top: 0; bottom: 0; width: 2px; background: var(--rz-warning); z-index: 2;"
                                        targetPct
                                )

                                attr.title(sprintf "Target Upper Buffer: %.0fh" wc.Envelope.TargetUpperHours)
                            }

                            let maxPlannedPct = (float(wc.Envelope.MaxPlannedHours / safeCap)) * 100.0

                            div {
                                attr.style(
                                    sprintf
                                        "position: absolute; left: %.1f%%; top: 0; bottom: 0; width: 2px; background: var(--rz-danger); z-index: 2;"
                                        maxPlannedPct
                                )

                                attr.title(sprintf "Max Planned Capacity: %.0fh" wc.Envelope.MaxPlannedHours)
                            }

                            let loadFillPct = min 100.0 ((float(wc.LoadHours / safeCap)) * 100.0)

                            div {
                                let clr = statusColor wc.Status

                                attr.style(
                                    sprintf
                                        "height: 100%%; width: %.1f%%; background: %s; opacity: 0.85; transition: width 0.3s ease;"
                                        loadFillPct
                                        clr
                                )
                            }
                        }

                        div {
                            attr.style
                                "display: flex; justify-content: space-between; font-size: 9px; color: var(--rz-text-secondary-color);"

                            span { "0h (Idle)" }
                            span { sprintf "Safe: %.0fh" wc.Envelope.TargetUpperHours }
                            span { sprintf "Max: %.0fh" wc.Envelope.MaxPlannedHours }
                            span { sprintf "Ceiling: %.0fh" wc.Envelope.AbsoluteCapacityHours }
                        }
                    }

                    // Bottleneck & Risk Diagnostics
                    div {
                        attr.style
                            "padding: 8px; border-radius: 6px; font-size: 11px; display: flex; flex-direction: column; gap: 4px; background: rgba(0,0,0,0.02);"

                        div {
                            attr.style "display: flex; justify-content: space-between;"

                            span {
                                attr.style "color: var(--rz-text-secondary-color);"
                                "Constraint Assessment:"
                            }

                            span {
                                attr.style "font-weight: 600;"
                                string wc.Constraint
                            }
                        }

                        div {
                            attr.style "display: flex; justify-content: space-between;"

                            span {
                                attr.style "color: var(--rz-text-secondary-color);"
                                "Bottleneck Role:"
                            }

                            span {
                                attr.style "font-weight: 600;"
                                string wc.Bottleneck
                            }
                        }

                        div {
                            attr.style "display: flex; justify-content: space-between;"

                            span {
                                attr.style "color: var(--rz-text-secondary-color);"
                                "Single Point of Failure:"
                            }

                            span {
                                attr.style(
                                    if wc.OverloadRisk = CapacityRiskLevel.Critical then
                                        "color: var(--rz-danger); font-weight: 700;"
                                    else
                                        "color: var(--rz-text-color);"
                                )

                                if wc.OverloadRisk = CapacityRiskLevel.Critical then
                                    "YES (High Vulnerability)"
                                else
                                    "No (Alternate Resource Available)"
                            }
                        }
                    }
                }
        }

let render (ctx: ComponentRenderContext) (dispatch: ComponentEvent -> unit) : Node =
    let activeTab =
        match ctx.Configuration.Parameters |> Map.tryFind "ActiveStudioTab" with
        | Some(StringValue "Envelopes") -> OperatingEnvelopes
        | Some(StringValue "Table") -> PositionTable
        | _ -> GanttTimeline

    let selectedResource = ctx.Selection

    let collapsedGroupsStr =
        match ctx.Configuration.Parameters |> Map.tryFind "CollapsedGroups" with
        | Some (StringValue s) -> s
        | _ -> ""

    let collapsedGroups =
        if String.IsNullOrWhiteSpace collapsedGroupsStr then Set.empty
        else collapsedGroupsStr.Split(';', StringSplitOptions.RemoveEmptyEntries) |> Set.ofArray

    let onToggleGroup (groupId: string) =
        let newSet =
            if collapsedGroups.Contains groupId then collapsedGroups.Remove groupId
            else collapsedGroups.Add groupId
        let updatedStr = String.Join(";", newSet)
        let updatedParams = ctx.Configuration.Parameters |> Map.add "CollapsedGroups" (StringValue updatedStr)
        dispatch (ConfigurationChangeRequested { ctx.Configuration with Parameters = updatedParams })

    let onSelectResource (resId: string) =
        let nextSel = if selectedResource = Some resId then None else Some resId
        dispatch (SelectionChanged nextSel)

    let schedules: WorkCenterSchedule list =
        match ctx.Data with
        | Tabular rows -> rows |> List.map parseRowToSchedule
        | _ -> []

    let bottleneckList =
        schedules
        |> List.filter(fun s ->
            s.Bottleneck = BottleneckAssessmentStatus.CurrentBottleneck
            || s.Status = CapacityUtilizationStatus.Overloaded)

    let bottleneckSummary =
        match bottleneckList with
        | [] -> "No Active Bottlenecks"
        | [ b ] -> sprintf "1 Bottleneck Active: %s (%.1f%%)" b.ResourceId b.UtilizationPct
        | head :: _ ->
            sprintf "%d Bottlenecks Active (e.g. %s: %.1f%%)" bottleneckList.Length head.ResourceId head.UtilizationPct

    let avgFleetUtil =
        if schedules.IsEmpty then
            0.0m
        else
            (schedules |> List.sumBy(fun s -> s.UtilizationPct)) / decimal schedules.Length

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
            attr.style
                "display: flex; align-items: center; justify-content: space-between; border-bottom: 1px solid var(--rz-border-color); padding-bottom: 10px; margin-bottom: 12px; flex-wrap: wrap; gap: 8px;"

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

                        sprintf
                            "Active Fleet: %d Work Centers · Planning Scope: PS-GLOBAL-SUPPLY-CHAIN"
                            schedules.Length
                    }
                }
            }

            // Middle: Quick Fleet Badges & Focused Resource
            div {
                attr.style "display: flex; align-items: center; gap: 8px; flex-wrap: wrap;"

                span {
                    let hasBottleneck = not bottleneckList.IsEmpty
                    let badgeColor = if hasBottleneck then "var(--rz-danger)" else "var(--rz-success)"
                    let badgeBg = if hasBottleneck then "rgba(220, 53, 69, 0.12)" else "rgba(40, 167, 69, 0.12)"
                    let badgeBorder = if hasBottleneck then "rgba(220, 53, 69, 0.3)" else "rgba(40, 167, 69, 0.3)"

                    attr.style(
                        sprintf
                            "padding: 3px 8px; border-radius: 12px; font-size: 11px; font-weight: 600; background: %s; color: %s; border: 1px solid %s;"
                            badgeBg
                            badgeColor
                            badgeBorder
                    )

                    bottleneckSummary
                }

                span {
                    let utilColor =
                        if avgFleetUtil > 100.0m then "var(--rz-danger)"
                        elif avgFleetUtil >= 85.0m then "var(--rz-warning)"
                        else "var(--rz-success)"

                    let utilBg =
                        if avgFleetUtil > 100.0m then "rgba(220, 53, 69, 0.12)"
                        elif avgFleetUtil >= 85.0m then "rgba(255, 193, 7, 0.15)"
                        else "rgba(40, 167, 69, 0.12)"

                    let utilBorder =
                        if avgFleetUtil > 100.0m then "rgba(220, 53, 69, 0.3)"
                        elif avgFleetUtil >= 85.0m then "rgba(255, 193, 7, 0.4)"
                        else "rgba(40, 167, 69, 0.3)"

                    attr.style(
                        sprintf
                            "padding: 3px 8px; border-radius: 12px; font-size: 11px; font-weight: 600; background: %s; color: %s; border: 1px solid %s;"
                            utilBg
                            utilColor
                            utilBorder
                    )

                    sprintf "Fleet Utilization: %.1f%%" avgFleetUtil
                }

                match selectedResource with
                | Some selId when not (String.IsNullOrWhiteSpace selId) ->
                    div {
                        attr.style "display: flex; align-items: center; gap: 6px; padding: 2px 8px; border-radius: 12px; background: rgba(var(--rz-primary-rgb), 0.15); border: 1px solid var(--rz-primary); font-size: 11px; font-weight: 600; color: var(--rz-primary);"
                        span { text $"Focused: {selId}" }
                        button {
                            attr.style "cursor: pointer; background: transparent; border: none; color: var(--rz-primary); font-size: 10.5px; text-decoration: underline; padding: 0;"
                            on.click (fun _ -> onSelectResource selId)
                            text "Clear Focus"
                        }
                    }
                | _ -> ()
            }

            // Right: View Tabs & Actions
            div {
                attr.style "display: flex; align-items: center; gap: 6px;"

                Rz.button(
                    "Gantt Timeline",
                    onClick =
                        (fun _ ->
                            let updated =
                                { ctx.Configuration with
                                    Parameters =
                                        ctx.Configuration.Parameters |> Map.add "ActiveStudioTab" (StringValue "Gantt")
                                }

                            dispatch(ConfigurationChangeRequested updated)),
                    style = (if activeTab = GanttTimeline then ButtonStyle.Primary else ButtonStyle.Light),
                    icon = "view_timeline"
                )

                Rz.button(
                    "Operating Envelopes",
                    onClick =
                        (fun _ ->
                            let updated =
                                { ctx.Configuration with
                                    Parameters =
                                        ctx.Configuration.Parameters
                                        |> Map.add "ActiveStudioTab" (StringValue "Envelopes")
                                }

                            dispatch(ConfigurationChangeRequested updated)),
                    style =
                        (if activeTab = OperatingEnvelopes then
                             ButtonStyle.Primary
                         else
                             ButtonStyle.Light),
                    icon = "bar_chart"
                )

                Rz.button(
                    "Position Grid",
                    onClick =
                        (fun _ ->
                            let updated =
                                { ctx.Configuration with
                                    Parameters =
                                        ctx.Configuration.Parameters |> Map.add "ActiveStudioTab" (StringValue "Table")
                                }

                            dispatch(ConfigurationChangeRequested updated)),
                    style = (if activeTab = PositionTable then ButtonStyle.Primary else ButtonStyle.Light),
                    icon = "table_rows"
                )

                Rz.iconButton("refresh", onClick = (fun _ -> dispatch RefreshRequested), tooltip = "Refresh telemetry")
            }
        }

        // Main Studio Body
        div {
            attr.style "flex: 1; min-height: 0; overflow-y: auto;"

            match activeTab with
            | GanttTimeline ->
                renderGanttTimeline schedules selectedResource collapsedGroups onToggleGroup onSelectResource
            | OperatingEnvelopes -> renderOperatingEnvelopes schedules
            | PositionTable -> UniversalEnterpriseGrid.render ctx dispatch
        }
    }

let runtime: ComponentRuntime = { Render = render }

let registration: ComponentRegistration =
    {
        Definition = definition
        Schema = CapacityGrid.schema
        Runtime = runtime
    }
