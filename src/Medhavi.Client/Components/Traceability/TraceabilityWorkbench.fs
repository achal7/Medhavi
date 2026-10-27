module Medhavi.Client.Components.Traceability.TraceabilityWorkbench

open System
open System.Collections.Generic
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor
open Medhavi.Contracts.Traceability
open Medhavi.Client
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client.Rendering
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Data

let definitionId = ComponentDefId.literal "comp-traceability-workbench"
let storeId = DataStoreId.literal "core-traceability-store"

let definition: ComponentDefinition =
    {
        Id = definitionId
        DisplayName = "Traceability & Investigation Studio"
        Icon = "account_tree"
        Category = ComponentCategory.Visualization
        RequiredSchemaCapabilities = [ "Tabular"; "Detail" ]
        DefaultGridSpan = GridSpan.literal 12 8
        DefaultSchemaId = Some TraceabilitySchema.schemaId
        DefaultStoreId = Some storeId
        IsResizable = true
        IsDraggable = true
        SelectionMode = SingleSelection
    }

type TimeRangePreset =
    | AllEvents
    | Last15Minutes
    | LastHour
    | Last24Hours

module TraceabilityTemporal =
    let toTimeInterval (now: DateTimeOffset) =
        function
        | AllEvents -> None, None
        | Last15Minutes -> Some(now.AddMinutes -15.0), Some now
        | LastHour -> Some(now.AddHours -1.0), Some now
        | Last24Hours -> Some(now.AddDays -1.0), Some now

    let presetLabel =
        function
        | AllEvents -> "All Ingested"
        | Last15Minutes -> "Last 15m"
        | LastHour -> "Last 1h"
        | Last24Hours -> "Last 24h"

    let allPresets = [ AllEvents; Last15Minutes; LastHour; Last24Hours ]

let store: DataStore =
    {
        Id = storeId
        Fetch =
            fun (ctx: ApplicationContext) ->
                async {
                    match ctx.Resolve<TraceabilityQueryService>() with
                    | Some queryService ->
                        let query =
                            { TelemetryFilterQuery.Default with
                                Limit = 100
                            }

                        let! result = queryService.QueryTelemetry(query) |> Async.AwaitTask

                        let rows =
                            result.Items
                            |> List.map(fun item ->
                                {
                                    EventId = item.EventId
                                    Timestamp = item.Timestamp
                                    Severity = item.Severity
                                    BoundedContext = item.BoundedContext
                                    AggregateType = item.AggregateType
                                    AggregateId = item.AggregateId
                                    CommandType = item.CommandType
                                    Message = item.Message
                                    CorrelationId = item.CorrelationId
                                    CausationId = item.CausationId
                                    HasDecision =
                                        not(
                                            String.IsNullOrWhiteSpace item.Message
                                            && String.IsNullOrWhiteSpace item.CommandType
                                        )
                                    HasErrors =
                                        item.Severity.Equals("Error", StringComparison.OrdinalIgnoreCase)
                                        || item.Severity.Equals("Errors", StringComparison.OrdinalIgnoreCase)
                                })
                            |> List.map TraceabilitySchema.adapter.ToUniversal

                        return Tabular rows
                    | None -> return Tabular []
                }
    }

let private renderTabContent
    (tabIndex: int)
    (activeDossier: TraceDossierDto)
    (activeEvent: TelemetryItemDto)
    (totalStageLatency: float)
    : Node =
    match tabIndex with
    | 0 ->
        div {
            attr.style "display: flex; flex-direction: column; gap: 10px;"

            div {
                attr.style "font-size: 12px; font-weight: 700; color: var(--rz-text-color); margin-bottom: 4px;"
                "Functional Execution Pipeline Stages (Monoidal Fold)"
            }

            if activeDossier.StageTimings.IsEmpty then
                div {
                    attr.style
                        "padding: 16px; text-align: center; color: var(--rz-text-secondary-color); font-size: 12px;"

                    "No individual stage execution timings recorded for this command."
                }
            else
                for stage in activeDossier.StageTimings do
                    div {
                        attr.style
                            "border: 1px solid var(--rz-border-color); border-radius: 6px; padding: 10px 12px; background: rgba(0,0,0,0.01); display: flex; justify-content: space-between; align-items: center;"

                        div {
                            div {
                                attr.style "font-size: 12px; font-weight: 600; color: var(--rz-text-color);"
                                stage.StageName
                            }

                            div {
                                attr.style "font-size: 11px; color: var(--rz-text-secondary-color);"
                                sprintf "Stage completed in %s context" activeEvent.BoundedContext
                            }
                        }

                        div {
                            attr.style
                                "font-size: 12px; font-weight: 700; color: var(--rz-primary); background: var(--rz-primary-lighter); border-radius: 4px; padding: 2px 8px;"

                            sprintf "%.2f ms" stage.DurationMs
                        }
                    }

                div {
                    attr.style
                        "margin-top: 10px; border-top: 1px dashed var(--rz-border-color); padding-top: 10px; display: flex; justify-content: space-between; font-size: 12px; font-weight: 700;"

                    span { "Summed Pipeline Latency:" }
                    span { sprintf "%.2f ms" totalStageLatency }
                }
        }
    | 1 ->
        div {
            attr.style "display: flex; flex-direction: column; gap: 12px;"

            if activeDossier.Decisions.IsEmpty then
                div {
                    attr.style
                        "padding: 20px; text-align: center; color: var(--rz-text-secondary-color); font-size: 12px;"

                    "No explicit policy decision was recorded for this correlation chain."
                }
            else
                for dec in activeDossier.Decisions do
                    div {
                        attr.style
                            "display: flex; flex-direction: column; gap: 8px; border: 1px solid var(--rz-border-color); border-radius: 6px; padding: 12px;"

                        div {
                            attr.style "display: flex; justify-content: space-between; align-items: center;"

                            span {
                                attr.style "font-size: 13px; font-weight: 700; color: var(--rz-primary);"
                                dec.PolicyId
                            }

                            span {
                                let isPass =
                                    dec.Outcome = "Passed"
                                    || dec.Outcome = "Recorded"
                                    || dec.Outcome = "Registered"
                                    || dec.Outcome = "Recognized"

                                let outcomeBg =
                                    if isPass then "var(--rz-success-lighter)" else "var(--rz-warning-lighter)"

                                let outcomeFg = if isPass then "var(--rz-success)" else "var(--rz-warning)"

                                attr.style
                                    $"padding: 2px 8px; border-radius: 4px; font-size: 10px; font-weight: 700; background: {outcomeBg}; color: {outcomeFg};"

                                dec.Outcome
                            }
                        }

                        div {
                            attr.style "font-size: 11px; color: var(--rz-text-secondary-color);"

                            sprintf
                                "Version: %s · Decision ID: %s · Capability: %s"
                                dec.PolicyVersion
                                dec.DecisionId
                                dec.CapabilityId
                        }

                        div {
                            attr.style "font-size: 11px; color: var(--rz-text-color); font-style: italic;"
                            dec.RationaleSummary
                        }

                        div {
                            attr.style
                                "font-size: 11px; font-weight: 700; color: var(--rz-text-secondary-color); margin-top: 4px;"

                            "Evaluated Rules:"
                        }

                        for rule in dec.RulesEvaluated do
                            let isPass = rule.Passed
                            let outcomeBg = if isPass then "var(--rz-success-lighter)" else "var(--rz-warning-lighter)"
                            let outcomeFg = if isPass then "var(--rz-success)" else "var(--rz-warning)"
                            let outcomeText = if isPass then "PASSED" else "FAILED"

                            div {
                                attr.style
                                    "border: 1px solid var(--rz-border-color); border-radius: 4px; padding: 8px 10px; background: rgba(0,0,0,0.01);"

                                div {
                                    attr.style "display: flex; justify-content: space-between; align-items: center;"

                                    span {
                                        attr.style "font-size: 11px; font-weight: 600; color: var(--rz-text-color);"
                                        rule.RuleId
                                    }

                                    span {
                                        attr.style
                                            $"padding: 1px 6px; border-radius: 3px; font-size: 9px; font-weight: 700; background: {outcomeBg}; color: {outcomeFg};"

                                        outcomeText
                                    }
                                }

                                div {
                                    attr.style
                                        "font-size: 11px; color: var(--rz-text-secondary-color); margin-top: 2px;"

                                    sprintf
                                        "Reason: %s · Evidence: %s"
                                        rule.ReasonCode
                                        (String.concat "; " rule.Evidence)
                                }
                            }
                    }
        }
    | _ ->
        div {
            attr.style "display: flex; flex-direction: column; gap: 12px;"
            // Cross-Domain Transitions
            div {
                attr.style
                    "border: 1px solid var(--rz-border-color); border-radius: 6px; padding: 10px 12px; display: flex; flex-direction: column; gap: 6px;"

                div {
                    attr.style "font-size: 12px; font-weight: 700; color: var(--rz-text-color);"
                    "Cross-Domain Transitions"
                }

                if activeDossier.Transitions.IsEmpty then
                    div {
                        attr.style "font-size: 11px; color: var(--rz-text-secondary-color);"
                        "No inter-domain transitions for this scope."
                    }
                else
                    for tr in activeDossier.Transitions do
                        div {
                            attr.style
                                "display: flex; justify-content: space-between; align-items: center; font-size: 11px;"

                            span { sprintf "%s ➔ %s (%d events)" tr.SourceContext tr.TargetContext tr.EventCount }

                            span {
                                attr.style "font-weight: 600; color: var(--rz-primary);"
                                sprintf "avg %.1f ms" tr.AverageLatencyMs
                            }
                        }
            }
            // Causal Provenance Identifiers
            div {
                attr.style
                    "border: 1px solid var(--rz-border-color); border-radius: 6px; padding: 10px 12px; display: flex; flex-direction: column; gap: 6px;"

                div {
                    attr.style "font-size: 12px; font-weight: 700; color: var(--rz-text-color);"
                    "Causal Provenance"
                }

                div {
                    attr.style "display: flex; justify-content: space-between; font-size: 11px;"

                    span {
                        attr.style "color: var(--rz-text-secondary-color);"
                        "Correlation ID:"
                    }

                    code {
                        attr.style "background: rgba(0,0,0,0.04); padding: 1px 4px; border-radius: 3px;"
                        activeDossier.CorrelationId
                    }
                }

                div {
                    attr.style "display: flex; justify-content: space-between; font-size: 11px;"

                    span {
                        attr.style "color: var(--rz-text-secondary-color);"
                        "Causation ID:"
                    }

                    code {
                        attr.style "background: rgba(0,0,0,0.04); padding: 1px 4px; border-radius: 3px;"
                        activeEvent.CausationId
                    }
                }

                div {
                    attr.style "display: flex; justify-content: space-between; font-size: 11px;"

                    span {
                        attr.style "color: var(--rz-text-secondary-color);"
                        "Root Command:"
                    }

                    span {
                        attr.style "font-weight: 600;"
                        activeDossier.RootCommandType
                    }
                }
            }
        }

// =========================================================================
// Deterministic Dossier Synthesis (from Ingested Events)
// =========================================================================

let private synthesizeDossier (row: TraceabilityRow) (rows: TraceabilityRow list) : TraceDossierDto =
    let chain = rows |> List.filter(fun r -> r.CorrelationId = row.CorrelationId) |> List.sortBy(fun r -> r.Timestamp)

    let contexts =
        chain |> List.map(fun r -> r.BoundedContext) |> List.filter(not << String.IsNullOrWhiteSpace) |> List.distinct

    let duration =
        if chain.Length > 1 then
            (chain.[chain.Length - 1].Timestamp - chain.[0].Timestamp).TotalMilliseconds
        else
            0.0

    let stages =
        if chain.Length > 1 then
            let rec computeDurations acc =
                function
                | (r1: TraceabilityRow) :: ((r2: TraceabilityRow) :: _ as rest) ->
                    let stageName = if not(String.IsNullOrWhiteSpace r1.Message) then r1.Message else r1.CommandType
                    let dur = max 0.01 (r2.Timestamp - r1.Timestamp).TotalMilliseconds

                    computeDurations
                        ({
                            StageName = stageName
                            DurationMs = dur
                         }
                         :: acc)
                        rest
                | [ last ] ->
                    let stageName =
                        if not(String.IsNullOrWhiteSpace last.Message) then
                            last.Message
                        else
                            last.CommandType

                    List.rev(
                        {
                            StageName = stageName
                            DurationMs = 0.01
                        }
                        :: acc
                    )
                | [] -> List.rev acc

            computeDurations [] chain
        else
            chain
            |> List.map(fun r ->
                let stageName = if not(String.IsNullOrWhiteSpace r.Message) then r.Message else r.CommandType

                {
                    StageName = stageName
                    DurationMs = 0.0
                })

    {
        CorrelationId = row.CorrelationId
        RootCommandType = row.CommandType
        BoundedContextsInvolved = if contexts.IsEmpty then [ row.BoundedContext ] else contexts
        TotalDurationMs = duration
        EventCount = chain.Length
        Events =
            chain
            |> List.map(fun r ->
                {
                    EventId = r.EventId
                    Timestamp = r.Timestamp
                    Severity = r.Severity
                    BoundedContext = r.BoundedContext
                    AggregateType = r.AggregateType
                    AggregateId = r.AggregateId
                    CommandType = r.CommandType
                    Message = r.Message
                    CorrelationId = r.CorrelationId
                    CausationId = r.CausationId
                })
        Decisions = []
        Transitions = []
        StageTimings = stages
    }

let private fromUniversalRow (urow: UniversalRow) : TraceabilityRow =
    let getText col =
        match urow.Cells.TryFind col with
        | Some(TextValue s) -> s
        | _ -> ""

    let getDate col =
        match urow.Cells.TryFind col with
        | Some(DateTimeValue dt) -> dt
        | _ -> DateTimeOffset.UtcNow

    let getBool col =
        match urow.Cells.TryFind col with
        | Some(BooleanValue b) -> b
        | _ -> false

    {
        EventId = urow.Key
        Timestamp = getDate "Timestamp"
        Severity = getText "Severity"
        BoundedContext = getText "BoundedContext"
        AggregateType = getText "AggregateType"
        AggregateId = getText "AggregateId"
        CommandType = getText "CommandType"
        Message = getText "Message"
        CorrelationId = getText "CorrelationId"
        CausationId = getText "CausationId"
        HasDecision = getBool "HasDecision"
        HasErrors = getBool "HasErrors"
    }

// =========================================================================
// Interactive Traceability Studio View Component
// =========================================================================

type TraceabilityWorkbenchComponent() =
    inherit Component()

    let mutable searchText = ""
    let mutable selectedDomain = "ALL"
    let mutable selectedTab = 0
    let mutable selectedEventId: string option = None
    let mutable activeDossier: TraceDossierDto option = None
    let mutable lastCorrelationId = ""
    let mutable selectedTimePreset: TimeRangePreset = AllEvents
    let mutable selectedLimit = 100
    let mutable liveRows: TraceabilityRow list option = None
    let mutable isLoading = false

    [<Parameter>]
    member val Context: ComponentRenderContext = Unchecked.defaultof<_> with get, set

    [<Parameter>]
    member val Dispatch: ComponentEvent -> unit = Unchecked.defaultof<_> with get, set

    [<Inject>]
    member val QueryService: TraceabilityQueryService = Unchecked.defaultof<_> with get, set

    member this.RequestRefresh() = this.StateHasChanged()
    member this.RunOnDispatcher(action: Action) = this.InvokeAsync(action) |> ignore

    member this.FetchTelemetry(preset: TimeRangePreset, limit: int) =
        if not(isNull(box this.QueryService)) then
            isLoading <- true
            selectedTimePreset <- preset
            selectedLimit <- limit
            this.StateHasChanged()

            System.Threading.Tasks.Task.Run(fun () ->
                task {
                    try
                        let st, et = TraceabilityTemporal.toTimeInterval DateTimeOffset.UtcNow preset

                        let q =
                            { TelemetryFilterQuery.Default with
                                StartTime = st
                                EndTime = et
                                Limit = limit
                            }

                        let! res = this.QueryService.QueryTelemetry(q)

                        let newRows =
                            res.Items
                            |> List.map(fun item ->
                                {
                                    EventId = item.EventId
                                    Timestamp = item.Timestamp
                                    Severity = item.Severity
                                    BoundedContext = item.BoundedContext
                                    AggregateType = item.AggregateType
                                    AggregateId = item.AggregateId
                                    CommandType = item.CommandType
                                    Message = item.Message
                                    CorrelationId = item.CorrelationId
                                    CausationId = item.CausationId
                                    HasDecision =
                                        not(
                                            String.IsNullOrWhiteSpace item.Message
                                            && String.IsNullOrWhiteSpace item.CommandType
                                        )
                                    HasErrors =
                                        item.Severity.Equals("Error", StringComparison.OrdinalIgnoreCase)
                                        || item.Severity.Equals("Errors", StringComparison.OrdinalIgnoreCase)
                                })

                        this.RunOnDispatcher(
                            Action(fun () ->
                                liveRows <- Some newRows
                                isLoading <- false
                                this.RequestRefresh())
                        )
                    with _ ->
                        this.RunOnDispatcher(
                            Action(fun () ->
                                isLoading <- false
                                this.RequestRefresh())
                        )
                }
                :> System.Threading.Tasks.Task)
            |> ignore

    member this.LoadDossier(corrId: string) =
        if not(isNull(box this.QueryService)) then
            System.Threading.Tasks.Task.Run(fun () ->
                task {
                    try
                        let! dOpt = this.QueryService.GetTraceDossier(corrId)

                        if dOpt.IsSome then
                            activeDossier <- dOpt
                            this.RunOnDispatcher(Action(fun () -> this.RequestRefresh()))
                    with _ ->
                        ()
                }
                :> System.Threading.Tasks.Task)
            |> ignore

    override this.Render() : Node =
        let allRows =
            match liveRows with
            | Some rows -> rows
            | None ->
                match this.Context.Data with
                | Tabular urows when not urows.IsEmpty -> urows |> List.map fromUniversalRow
                | _ -> []

        // Dynamically available domains from ingested stream
        let availableDomains =
            [ "ALL" ]
            @ (allRows
               |> List.map(fun r -> r.BoundedContext)
               |> List.filter(not << String.IsNullOrWhiteSpace)
               |> List.distinct)

        // Filter rows by domain and search term
        let filteredRows =
            allRows
            |> List.filter(fun r ->
                let domainMatch =
                    if selectedDomain = "ALL" then
                        true
                    else
                        String.Equals(r.BoundedContext, selectedDomain, StringComparison.OrdinalIgnoreCase)

                let searchMatch =
                    if String.IsNullOrWhiteSpace searchText then
                        true
                    else
                        let term = searchText.Trim()

                        r.CorrelationId.Contains(term, StringComparison.OrdinalIgnoreCase)
                        || r.AggregateId.Contains(term, StringComparison.OrdinalIgnoreCase)
                        || r.CommandType.Contains(term, StringComparison.OrdinalIgnoreCase)
                        || r.Message.Contains(term, StringComparison.OrdinalIgnoreCase)
                        || r.EventId.Contains(term, StringComparison.OrdinalIgnoreCase)

                domainMatch && searchMatch)

        // Determine active event row (if any)
        let activeRowOpt =
            match selectedEventId with
            | Some eid ->
                filteredRows
                |> List.tryFind(fun r -> r.EventId = eid)
                |> Option.orElseWith(fun () -> allRows |> List.tryFind(fun r -> r.EventId = eid))
                |> Option.orElseWith(fun () -> List.tryHead filteredRows)
                |> Option.orElseWith(fun () -> List.tryHead allRows)
            | None ->
                match this.Context.Selection with
                | Some eid when not(String.IsNullOrWhiteSpace eid) ->
                    filteredRows
                    |> List.tryFind(fun r -> r.EventId = eid)
                    |> Option.orElseWith(fun () -> allRows |> List.tryFind(fun r -> r.EventId = eid))
                    |> Option.orElseWith(fun () -> List.tryHead filteredRows)
                    |> Option.orElseWith(fun () -> List.tryHead allRows)
                | _ -> filteredRows |> List.tryHead |> Option.orElseWith(fun () -> List.tryHead allRows)

        // When an active row exists, manage its dossier
        match activeRowOpt with
        | Some activeRow ->
            if lastCorrelationId <> activeRow.CorrelationId then
                lastCorrelationId <- activeRow.CorrelationId
                activeDossier <- None
                this.LoadDossier(activeRow.CorrelationId)

            let dossier = activeDossier |> Option.defaultWith(fun () -> synthesizeDossier activeRow allRows)

            let totalStageLatency =
                dossier.StageTimings
                |> List.map(fun s -> s.DurationMs)
                |> List.fold (snd TraceabilityOptics.stageDurationMonoid) (fst TraceabilityOptics.stageDurationMonoid)

            let activeEventDto: TelemetryItemDto =
                {
                    EventId = activeRow.EventId
                    Timestamp = activeRow.Timestamp
                    Severity = activeRow.Severity
                    BoundedContext = activeRow.BoundedContext
                    AggregateType = activeRow.AggregateType
                    AggregateId = activeRow.AggregateId
                    CommandType = activeRow.CommandType
                    Message = activeRow.Message
                    CorrelationId = activeRow.CorrelationId
                    CausationId = activeRow.CausationId
                }

            div {
                attr.style
                    "display: flex; flex-direction: column; gap: 14px; height: 100%; box-sizing: border-box; overflow-y: auto; padding: 14px; font-family: Segoe UI, sans-serif;"

                // -------------------------------------------------------------
                // 1. Top Metrics & Ring Buffer Health Deck (Monoidal Folds)
                // -------------------------------------------------------------
                div {
                    attr.style "display: grid; grid-template-columns: repeat(4, 1fr); gap: 12px;"

                    // Card 1: Ring Buffer Depth
                    div {
                        attr.style
                            "background: var(--rz-base-background-color); border: 1px solid var(--rz-border-color); border-radius: 8px; padding: 12px 16px; display: flex; align-items: center; gap: 12px; box-shadow: 0 1px 3px rgba(0,0,0,0.04);"

                        span {
                            attr.``class`` "rzi"
                            attr.style "font-size: 28px; color: var(--rz-primary);"
                            "memory"
                        }

                        div {
                            div {
                                attr.style
                                    "font-size: 11px; text-transform: uppercase; letter-spacing: 0.5px; color: var(--rz-text-secondary-color);"

                                "Active Ingested Events"
                            }

                            div {
                                attr.style "font-size: 18px; font-weight: 700; color: var(--rz-text-color);"
                                sprintf "%s Events" (allRows.Length.ToString("N0"))
                            }
                        }
                    }

                    // Card 2: Active Multi-Domain Correlation Chains
                    let distinctChains =
                        allRows
                        |> List.choose(fun r ->
                            if String.IsNullOrWhiteSpace r.CorrelationId then None else Some r.CorrelationId)
                        |> List.distinct
                        |> List.length

                    div {
                        attr.style
                            "background: var(--rz-base-background-color); border: 1px solid var(--rz-border-color); border-radius: 8px; padding: 12px 16px; display: flex; align-items: center; gap: 12px; box-shadow: 0 1px 3px rgba(0,0,0,0.04);"

                        span {
                            attr.``class`` "rzi"
                            attr.style "font-size: 28px; color: var(--rz-info);"
                            "link"
                        }

                        div {
                            div {
                                attr.style
                                    "font-size: 11px; text-transform: uppercase; letter-spacing: 0.5px; color: var(--rz-text-secondary-color);"

                                "Active Correlation Chains"
                            }

                            div {
                                attr.style "font-size: 18px; font-weight: 700; color: var(--rz-text-color);"
                                sprintf "%d Chains" distinctChains
                            }
                        }
                    }

                    // Card 3: Causal Execution Latency
                    let meanLatency = if totalStageLatency > 0.0 then totalStageLatency else 0.0

                    div {
                        attr.style
                            "background: var(--rz-base-background-color); border: 1px solid var(--rz-border-color); border-radius: 8px; padding: 12px 16px; display: flex; align-items: center; gap: 12px; box-shadow: 0 1px 3px rgba(0,0,0,0.04);"

                        span {
                            attr.``class`` "rzi"
                            attr.style "font-size: 28px; color: var(--rz-success);"
                            "speed"
                        }

                        div {
                            div {
                                attr.style
                                    "font-size: 11px; text-transform: uppercase; letter-spacing: 0.5px; color: var(--rz-text-secondary-color);"

                                "Execution Latency"
                            }

                            div {
                                attr.style "font-size: 18px; font-weight: 700; color: var(--rz-text-color);"
                                sprintf "%.2f ms" meanLatency
                            }
                        }
                    }

                    // Card 4: Governed Rules Status
                    let checkedRules = dossier.Decisions |> List.collect(fun d -> d.RulesEvaluated)
                    let warnRules = checkedRules |> List.filter(fun r -> not r.Passed)
                    let rulesSummary = sprintf "%d Checked (%d Failed)" checkedRules.Length warnRules.Length

                    div {
                        attr.style
                            "background: var(--rz-base-background-color); border: 1px solid var(--rz-border-color); border-radius: 8px; padding: 12px 16px; display: flex; align-items: center; gap: 12px; box-shadow: 0 1px 3px rgba(0,0,0,0.04);"

                        span {
                            attr.``class`` "rzi"
                            attr.style "font-size: 28px; color: var(--rz-warning);"
                            "verified_user"
                        }

                        div {
                            div {
                                attr.style
                                    "font-size: 11px; text-transform: uppercase; letter-spacing: 0.5px; color: var(--rz-text-secondary-color);"

                                "Governed Rule Evaluations"
                            }

                            div {
                                attr.style "font-size: 18px; font-weight: 700; color: var(--rz-text-color);"
                                rulesSummary
                            }
                        }
                    }
                }

                // -------------------------------------------------------------
                // 2. Cross-Domain Causal Flow Progression Banner
                // -------------------------------------------------------------
                let chainEvents =
                    allRows
                    |> List.filter(fun r -> r.CorrelationId = activeRow.CorrelationId)
                    |> List.sortBy(fun r -> r.Timestamp)

                let topology =
                    chainEvents
                    |> List.map(fun e -> e.BoundedContext)
                    |> List.filter(not << String.IsNullOrWhiteSpace)
                    |> List.distinct
                    |> String.concat " ➔ "

                let topologyText =
                    if String.IsNullOrWhiteSpace topology then
                        "Topology: Single Context"
                    else
                        sprintf "Topology: %s" topology

                div {
                    attr.style
                        "background: linear-gradient(135deg, rgba(37,99,235,0.05), rgba(16,185,129,0.05)); border: 1px solid var(--rz-primary-light); border-radius: 8px; padding: 12px 18px; display: flex; flex-direction: column; gap: 8px;"

                    div {
                        attr.style "display: flex; justify-content: space-between; align-items: center;"

                        div {
                            attr.style
                                "display: flex; align-items: center; gap: 8px; font-weight: 600; font-size: 13px; color: var(--rz-text-color);"

                            span {
                                attr.``class`` "rzi"
                                attr.style "font-size: 18px; color: var(--rz-primary);"
                                "account_tree"
                            }

                            span { "Causal Flow Progression: " + dossier.CorrelationId }
                        }

                        span {
                            attr.style
                                "font-size: 11px; background: var(--rz-primary-lighter); color: var(--rz-primary); border-radius: 12px; padding: 2px 10px; font-weight: 600;"

                            topologyText
                        }
                    }

                    // Stepper nodes: Micro Pipeline Stages vs Macro Choreography Flow
                    div {
                        attr.style "display: flex; align-items: center; gap: 12px; padding: 6px 0; overflow-x: auto;"

                        let isSingleCommandPipeline =
                            let distinctCmds =
                                chainEvents
                                |> List.map(fun e -> e.CommandType)
                                |> List.filter(not << String.IsNullOrWhiteSpace)
                                |> List.distinct

                            distinctCmds.Length <= 1

                        let stagesToRender =
                            chainEvents
                            |> List.mapi(fun i e ->
                                let label, icon =
                                    if isSingleCommandPipeline then
                                        match e.Message with
                                        | "AggregateLoaded" -> "Load State", "download"
                                        | "DecisionEvaluated" -> "Decide", "policy"
                                        | "EventsPersisted" -> "Persist Events", "save"
                                        | "CommandCompleted" -> "Completed", "check_circle"
                                        | other when not(String.IsNullOrWhiteSpace other) -> other, "task_alt"
                                        | _ ->
                                            let fallback =
                                                if not(String.IsNullOrWhiteSpace e.CommandType) then
                                                    e.CommandType
                                                else
                                                    e.AggregateId

                                            fallback, "account_tree"
                                    else
                                        let cmdLabel =
                                            if not(String.IsNullOrWhiteSpace e.CommandType) then
                                                e.CommandType
                                            else
                                                e.AggregateId

                                        let ctxIcon =
                                            match e.BoundedContext with
                                            | "Demand" -> "shopping_cart"
                                            | "Supply" -> "precision_manufacturing"
                                            | "Core" -> "photo_library"
                                            | _ -> "account_tree"

                                        cmdLabel, ctxIcon

                                (i + 1, e.BoundedContext, label, e.EventId, icon))

                        for idx, (stepNum, ctx, label, eid, icon) in List.indexed stagesToRender do
                            if idx > 0 then
                                span {
                                    attr.``class`` "rzi"
                                    attr.style "color: var(--rz-text-secondary-color); font-size: 18px;"
                                    "arrow_forward"
                                }

                            let isCurrent = eid = activeRow.EventId
                            let bg = if isCurrent then "var(--rz-primary)" else "var(--rz-base-background-color)"
                            let fg = if isCurrent then "#ffffff" else "var(--rz-text-color)"

                            div {
                                attr.style
                                    $"padding: 6px 14px; border-radius: 6px; border: 1px solid var(--rz-primary); background: {bg}; color: {fg}; font-size: 12px; font-weight: 600; display: flex; align-items: center; gap: 6px; cursor: pointer;"

                                on.click(fun _ ->
                                    selectedEventId <- Some eid

                                    if not(isNull(box this.Dispatch)) then
                                        this.Dispatch(SelectionChanged(Some eid))

                                    this.RequestRefresh())

                                span {
                                    attr.``class`` "rzi"
                                    attr.style "font-size: 14px;"
                                    icon
                                }

                                span { sprintf "%d. %s: %s" stepNum ctx label }
                            }
                    }
                }

                // -------------------------------------------------------------
                // 3. Interactive Splitter Deck (Left: Stream | Right: Dossier)
                // -------------------------------------------------------------
                div {
                    attr.style
                        "display: grid; grid-template-columns: 1.15fr 1fr; gap: 14px; flex: 1; min-height: 480px;"

                    // ---------------- Left Pane: Telemetry Stream ----------------
                    div {
                        attr.style
                            "background: var(--rz-base-background-color); border: 1px solid var(--rz-border-color); border-radius: 8px; display: flex; flex-direction: column; overflow: hidden;"

                        // Search & Filter Toolbar
                        div {
                            attr.style
                                "padding: 10px 14px; border-bottom: 1px solid var(--rz-border-color); display: flex; gap: 10px; align-items: center; background: rgba(0,0,0,0.01);"

                            // Search Box
                            input {
                                attr.``type`` "text"
                                attr.value searchText
                                attr.placeholder "Filter by correlation, aggregate, or command..."

                                attr.style
                                    "flex: 1; padding: 6px 10px; font-size: 12px; border: 1px solid var(--rz-border-color); border-radius: 4px; outline: none; background: var(--rz-base-background-color); color: var(--rz-text-color);"

                                on.input(fun (e: ChangeEventArgs) ->
                                    searchText <- string e.Value
                                    let term = searchText.Trim()

                                    if not(String.IsNullOrWhiteSpace term) then
                                        let matches =
                                            allRows
                                            |> List.filter(fun r ->
                                                r.CorrelationId.Contains(term, StringComparison.OrdinalIgnoreCase)
                                                || r.AggregateId.Contains(term, StringComparison.OrdinalIgnoreCase)
                                                || r.CommandType.Contains(term, StringComparison.OrdinalIgnoreCase))

                                        if not matches.IsEmpty then
                                            selectedEventId <- Some matches.Head.EventId

                                    this.RequestRefresh())
                            }

                            // Domain Chips
                            div {
                                attr.style "display: flex; gap: 4px;"

                                for domain in availableDomains do
                                    let isSel = domain = selectedDomain
                                    let bg = if isSel then "var(--rz-primary)" else "transparent"
                                    let fg = if isSel then "#ffffff" else "var(--rz-text-secondary-color)"

                                    span {
                                        attr.style
                                            $"padding: 4px 8px; border-radius: 4px; font-size: 11px; font-weight: 600; cursor: pointer; border: 1px solid var(--rz-border-color); background: {bg}; color: {fg};"

                                        on.click(fun _ ->
                                            selectedDomain <- domain
                                            this.RequestRefresh())

                                        domain
                                    }
                            }
                        }

                        // Enterprise Trace Control Deck: Temporal Presets & Record Caps
                        div {
                            attr.style
                                "padding: 8px 14px; border-bottom: 1px solid var(--rz-border-color); display: flex; justify-content: space-between; align-items: center; background: rgba(0,0,0,0.02); flex-wrap: wrap; gap: 8px;"

                            // Left: Time Range Presets
                            div {
                                attr.style "display: flex; align-items: center; gap: 6px;"

                                span {
                                    attr.style
                                        "font-size: 11px; font-weight: 600; color: var(--rz-text-secondary-color);"

                                    "Window:"
                                }

                                for preset in TraceabilityTemporal.allPresets do
                                    let isSel = preset = selectedTimePreset
                                    let bg = if isSel then "var(--rz-info)" else "transparent"
                                    let fg = if isSel then "#ffffff" else "var(--rz-text-secondary-color)"

                                    span {
                                        attr.style
                                            $"padding: 2px 8px; border-radius: 4px; font-size: 10px; font-weight: 600; cursor: pointer; border: 1px solid var(--rz-border-color); background: {bg}; color: {fg}; transition: all 0.15s ease;"

                                        on.click(fun _ -> this.FetchTelemetry(preset, selectedLimit))
                                        TraceabilityTemporal.presetLabel preset
                                    }
                            }

                            // Right: Record Limit & Refresh Button
                            div {
                                attr.style "display: flex; align-items: center; gap: 8px;"

                                span {
                                    attr.style
                                        "font-size: 11px; font-weight: 600; color: var(--rz-text-secondary-color);"

                                    "Cap:"
                                }

                                for cap in [ 50; 100; 250; 500 ] do
                                    let isSel = cap = selectedLimit
                                    let bg = if isSel then "var(--rz-primary)" else "transparent"
                                    let fg = if isSel then "#ffffff" else "var(--rz-text-secondary-color)"

                                    span {
                                        attr.style
                                            $"padding: 2px 6px; border-radius: 4px; font-size: 10px; font-weight: 600; cursor: pointer; border: 1px solid var(--rz-border-color); background: {bg}; color: {fg}; transition: all 0.15s ease;"

                                        on.click(fun _ -> this.FetchTelemetry(selectedTimePreset, cap))
                                        string cap
                                    }

                                // Live Refresh Button
                                button {
                                    attr.``type`` "button"

                                    attr.style
                                        "display: flex; align-items: center; gap: 4px; padding: 3px 8px; border-radius: 4px; border: 1px solid var(--rz-border-color); background: var(--rz-base-background-color); color: var(--rz-text-color); font-size: 11px; font-weight: 600; cursor: pointer;"

                                    on.click(fun _ -> this.FetchTelemetry(selectedTimePreset, selectedLimit))

                                    span {
                                        attr.``class`` "rzi"

                                        attr.style(
                                            if isLoading then
                                                "font-size: 13px; color: var(--rz-primary);"
                                            else
                                                "font-size: 13px;"
                                        )

                                        "refresh"
                                    }

                                    span { if isLoading then "Syncing..." else "Refresh" }
                                }

                                // Matches pill
                                span {
                                    attr.style
                                        "font-size: 10px; font-weight: 600; color: var(--rz-text-secondary-color); background: rgba(0,0,0,0.04); padding: 2px 6px; border-radius: 4px;"

                                    sprintf "%d / %d" filteredRows.Length allRows.Length
                                }
                            }
                        }

                        // Event Items List
                        div {
                            attr.style "flex: 1; overflow-y: auto;"

                            if filteredRows.IsEmpty then
                                div {
                                    attr.style
                                        "padding: 30px; text-align: center; color: var(--rz-text-secondary-color); font-size: 12px;"

                                    "No telemetry events matched the filter criteria."
                                }
                            else
                                for row in filteredRows do
                                    let isSelected = row.EventId = activeRow.EventId
                                    let itemBg = if isSelected then "rgba(37,99,235,0.08)" else "transparent"

                                    let borderLeft =
                                        if isSelected then "3px solid var(--rz-primary)" else "3px solid transparent"

                                    div {
                                        attr.style
                                            $"padding: 10px 14px; border-bottom: 1px solid var(--rz-border-color); border-left: {borderLeft}; background: {itemBg}; cursor: pointer; display: flex; flex-direction: column; gap: 4px; transition: background 0.15s ease;"

                                        on.click(fun _ ->
                                            selectedEventId <- Some row.EventId

                                            if not(isNull(box this.Dispatch)) then
                                                this.Dispatch(SelectionChanged(Some row.EventId))

                                            this.RequestRefresh())

                                        // Top Line: Domain + Command + Time
                                        div {
                                            attr.style
                                                "display: flex; justify-content: space-between; align-items: center;"

                                            div {
                                                attr.style "display: flex; align-items: center; gap: 8px;"
                                                // Domain Badge
                                                span {
                                                    let badgeBg =
                                                        match row.BoundedContext with
                                                        | "Demand" -> "rgba(37,99,235,0.12); color: #2563eb;"
                                                        | "Supply" -> "rgba(16,185,129,0.12); color: #059669;"
                                                        | "Core" -> "rgba(139,92,246,0.12); color: #7c3aed;"
                                                        | _ -> "rgba(100,116,139,0.12); color: #475569;"

                                                    attr.style
                                                        $"border-radius: 4px; padding: 2px 6px; font-size: 10px; font-weight: 700; background: {badgeBg}"

                                                    row.BoundedContext
                                                }

                                                // Command Name
                                                span {
                                                    attr.style
                                                        "font-size: 13px; font-weight: 600; color: var(--rz-text-color);"

                                                    row.CommandType
                                                }

                                                // Warning/Error Pill
                                                if row.HasErrors || row.Severity = "Warning" then
                                                    span {
                                                        attr.style
                                                            "background: var(--rz-warning-lighter); color: var(--rz-warning); font-size: 10px; font-weight: 700; padding: 2px 6px; border-radius: 4px;"

                                                        "WARN"
                                                    }
                                            }

                                            // Time
                                            span {
                                                attr.style
                                                    "font-size: 11px; font-weight: 600; color: var(--rz-text-secondary-color);"

                                                row.Timestamp.ToString("HH:mm:ss.fff")
                                            }
                                        }

                                        // Middle Line: Stage & Message
                                        div {
                                            attr.style
                                                "display: flex; align-items: center; gap: 6px; font-size: 12px; font-weight: 600; color: var(--rz-text-color);"

                                            let stageIcon, stageColor =
                                                match row.Message with
                                                | "AggregateLoaded" -> "download", "var(--rz-info)"
                                                | "DecisionEvaluated" -> "policy", "var(--rz-primary)"
                                                | "EventsPersisted" -> "save", "var(--rz-success)"
                                                | "CommandCompleted" -> "check_circle", "var(--rz-success)"
                                                | _ -> "chevron_right", "var(--rz-text-secondary-color)"

                                            span {
                                                attr.``class`` "rzi"
                                                attr.style $"font-size: 14px; color: {stageColor};"
                                                stageIcon
                                            }

                                            span {
                                                if String.IsNullOrWhiteSpace row.Message then
                                                    row.CommandType
                                                else
                                                    row.Message
                                            }
                                        }

                                        // Bottom Line: Aggregate ID & Correlation ID
                                        div {
                                            attr.style
                                                "display: flex; justify-content: space-between; font-size: 11px; color: var(--rz-text-secondary-color);"

                                            span { sprintf "%s: %s" row.AggregateType row.AggregateId }
                                            span { sprintf "%s" row.CorrelationId }
                                        }
                                    }
                        }
                    }

                    // ---------------- Right Pane: Causal Dossier Inspector ----------------
                    div {
                        attr.style
                            "background: var(--rz-base-background-color); border: 1px solid var(--rz-border-color); border-radius: 8px; display: flex; flex-direction: column; overflow: hidden;"

                        // Header with Event Details
                        div {
                            attr.style
                                "padding: 12px 16px; border-bottom: 1px solid var(--rz-border-color); background: rgba(0,0,0,0.01);"

                            div {
                                attr.style "display: flex; justify-content: space-between; align-items: flex-start;"

                                div {
                                    div {
                                        attr.style "font-size: 14px; font-weight: 700; color: var(--rz-text-color);"
                                        sprintf "Dossier: %s" activeRow.CommandType
                                    }

                                    div {
                                        attr.style
                                            "font-size: 11px; color: var(--rz-text-secondary-color); margin-top: 2px;"

                                        sprintf
                                            "Aggregate: %s (%s) · Causation: %s"
                                            activeRow.AggregateId
                                            activeRow.AggregateType
                                            activeRow.CausationId
                                    }
                                }

                                span {
                                    attr.style
                                        "background: var(--rz-primary-lighter); color: var(--rz-primary); border-radius: 12px; padding: 3px 10px; font-size: 11px; font-weight: 600;"

                                    sprintf "Total: %.2f ms" totalStageLatency
                                }
                            }
                        }

                        // Tabs Navigation
                        div {
                            attr.style
                                "display: flex; border-bottom: 1px solid var(--rz-border-color); background: rgba(0,0,0,0.02);"

                            let tabs = [ "Execution Stages"; "Governed Decisions"; "Domain Transitions & Hashes" ]

                            for i in 0 .. tabs.Length - 1 do
                                let isTabSel = selectedTab = i

                                let tabBorder =
                                    if isTabSel then "2px solid var(--rz-primary)" else "2px solid transparent"

                                let tabFg = if isTabSel then "var(--rz-primary)" else "var(--rz-text-secondary-color)"

                                button {
                                    attr.``type`` "button"

                                    attr.style
                                        $"padding: 8px 16px; border: none; background: transparent; border-bottom: {tabBorder}; color: {tabFg}; font-size: 12px; font-weight: 600; cursor: pointer;"

                                    on.click(fun _ ->
                                        selectedTab <- i
                                        this.RequestRefresh())

                                    tabs.[i]
                                }
                        }

                        // Tab Body Content
                        div {
                            attr.style "padding: 14px 16px; flex: 1; overflow-y: auto;"
                            renderTabContent selectedTab dossier activeEventDto totalStageLatency
                        }
                    }
                }
            }
        | None ->
            // Enterprise Empty State View
            div {
                attr.style
                    "display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100%; padding: 40px; box-sizing: border-box; text-align: center; gap: 16px;"

                span {
                    attr.``class`` "rzi"
                    attr.style "font-size: 48px; color: var(--rz-text-secondary-color);"
                    "manage_search"
                }

                div {
                    div {
                        attr.style "font-size: 16px; font-weight: 700; color: var(--rz-text-color);"
                        "No Telemetry Traces Found"
                    }

                    div {
                        attr.style
                            "font-size: 12px; color: var(--rz-text-secondary-color); max-width: 420px; margin-top: 4px;"

                        "No events matched the selected time window, domain, or search criteria. Select a wider time window, increase the record cap, or refresh the query from Nexus."
                    }
                }

                div {
                    attr.style "display: flex; gap: 8px; margin-top: 8px;"

                    button {
                        attr.``type`` "button"

                        attr.style
                            "display: flex; align-items: center; gap: 6px; padding: 6px 16px; border-radius: 6px; border: 1px solid var(--rz-primary); background: var(--rz-primary); color: #ffffff; font-size: 12px; font-weight: 600; cursor: pointer;"

                        on.click(fun _ -> this.FetchTelemetry(AllEvents, 100))

                        span {
                            attr.``class`` "rzi"
                            attr.style "font-size: 14px;"
                            "refresh"
                        }

                        "Reset Filters & Ingest All"
                    }
                }
            }

let render (ctx: ComponentRenderContext) (dispatch: ComponentEvent -> unit) : Node =
    comp<TraceabilityWorkbenchComponent> {
        "Context" => ctx
        "Dispatch" => dispatch
    }

let runtime: ComponentRuntime = { Render = render }

let registration: ComponentRegistration =
    {
        Definition = definition
        Runtime = runtime
    }
