module Medhavi.Client.Core.Traceability.TraceCanvasLayout

open System
open Medhavi.Contracts.Traceability

let private contextOrder = [ "Core"; "Demand"; "Supply"; "Promise" ]

let getContextColIndex (bc: string) =
    match contextOrder |> List.tryFindIndex (fun c -> c.Equals(bc, StringComparison.OrdinalIgnoreCase)) with
    | Some idx -> idx
    | None ->
        if bc.StartsWith("Core", StringComparison.OrdinalIgnoreCase) || bc.Contains("Picture") || bc.Contains("Exception") then 0
        elif bc.StartsWith("Demand", StringComparison.OrdinalIgnoreCase) then 1
        elif bc.StartsWith("Supply", StringComparison.OrdinalIgnoreCase) || bc.Contains("Inventory") || bc.Contains("Capacity") then 2
        elif bc.StartsWith("Promise", StringComparison.OrdinalIgnoreCase) then 3
        else contextOrder.Length

let private groupContiguous (events: TelemetryItemDto list) : TelemetryItemDto list list =
    let folder (acc: TelemetryItemDto list list) (evt: TelemetryItemDto) : TelemetryItemDto list list =
        match acc with
        | [] -> [ [ evt ] ]
        | (currentGroup :: rest) ->
            let (head: TelemetryItemDto) = currentGroup.Head
            let isSameTx =
                head.BoundedContext.Equals(evt.BoundedContext, StringComparison.OrdinalIgnoreCase)
                && head.AggregateId = evt.AggregateId
                && head.CommandType = evt.CommandType
                && not (String.IsNullOrWhiteSpace head.CommandType)
            if isSameTx then
                (evt :: currentGroup) :: rest
            else
                [ evt ] :: (currentGroup :: rest)

    events
    |> List.fold folder []
    |> List.map List.rev
    |> List.rev

let computeLayout (dossier: TraceDossierDto) : TraceCanvasNode list * TraceCanvasEdge list * (float * float) =
    let colWidth = 320.0
    let nodeWidth = 270.0
    let nodeHeight = 108.0
    let rowSpacing = 150.0
    let startX = 50.0
    let startY = 70.0

    let sortedEvents = dossier.Events |> List.sortBy (fun e -> e.Timestamp)
    let transactionGroups = groupContiguous sortedEvents

    let nodes =
        transactionGroups
        |> List.mapi (fun idx txEvents ->
            let firstEvt = txEvents.Head
            let lastEvt = List.last txEvents
            let duration = max 0.1 (lastEvt.Timestamp - firstEvt.Timestamp).TotalMilliseconds
            let stages = txEvents |> List.map (fun e -> e.Message) |> List.distinct

            let colIdx = getContextColIndex firstEvt.BoundedContext
            let x = startX + (float colIdx * colWidth)
            let y = startY + (float idx * rowSpacing)

            let hasErr =
                txEvents
                |> List.exists (fun e ->
                    e.Severity.Equals("Error", StringComparison.OrdinalIgnoreCase)
                    || e.Severity.Equals("Errors", StringComparison.OrdinalIgnoreCase)
                    || e.Severity.Equals("Critical", StringComparison.OrdinalIgnoreCase))

            let hasDec =
                txEvents
                |> List.exists (fun e ->
                    e.Message.Contains("Decision")
                    || dossier.Decisions
                       |> List.exists (fun d ->
                           d.CapabilityId.Equals(e.BoundedContext, StringComparison.OrdinalIgnoreCase)
                           || d.DecisionId.Equals(e.AggregateId, StringComparison.OrdinalIgnoreCase)))

            let cmdTitle =
                if not (String.IsNullOrWhiteSpace firstEvt.CommandType) then firstEvt.CommandType
                elif txEvents.Length > 1 then sprintf "%s (%d stages)" firstEvt.Message txEvents.Length
                else firstEvt.Message

            { NodeId = firstEvt.EventId
              EventId = firstEvt.EventId
              BoundedContext = firstEvt.BoundedContext
              AggregateType = firstEvt.AggregateType
              AggregateId = firstEvt.AggregateId
              CommandType = cmdTitle
              Message = String.concat " → " stages
              Severity = if hasErr then "Error" else firstEvt.Severity
              Timestamp = firstEvt.Timestamp
              HasDecision = hasDec
              HasErrors = hasErr
              StageCount = txEvents.Length
              Stages = stages
              TotalDurationMs = duration
              UnderlyingEvents = txEvents
              X = x
              Y = y
              Width = nodeWidth
              Height = nodeHeight })

    let nodeMap = nodes |> List.map (fun n -> n.NodeId, n) |> Map.ofList

    let edges =
        nodes
        |> List.pairwise
        |> List.choose (fun (src, tgt) ->
            let srcCol = getContextColIndex src.BoundedContext
            let tgtCol = getContextColIndex tgt.BoundedContext
            let isLoop = tgtCol <= srcCol
            let latency = max 0.0 (tgt.Timestamp - src.Timestamp).TotalMilliseconds
            let hasErr = tgt.HasErrors

            Some
                { EdgeId = sprintf "%s->%s" src.NodeId tgt.NodeId
                  SourceNodeId = src.NodeId
                  TargetNodeId = tgt.NodeId
                  SourceContext = src.BoundedContext
                  TargetContext = tgt.BoundedContext
                  IsLoopback = isLoop
                  LatencyMs = latency
                  HasErrors = hasErr })

    let totalCols = max 1 (contextOrder.Length + 1)
    let totalRows = max 1 (List.length nodes)
    let totalWidth = startX + (float totalCols * colWidth) + 80.0
    let totalHeight = startY + (float totalRows * rowSpacing) + 80.0

    nodes, edges, (totalWidth, totalHeight)
