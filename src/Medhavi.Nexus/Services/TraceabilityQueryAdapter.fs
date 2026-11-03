module Medhavi.Nexus.Services.TraceabilityQueryAdapter

open System
open Medhavi.Common
open Medhavi.Foundation.ExecutionContext
open Medhavi.Foundation.Observation
open Medhavi.Foundation.Contracts
open Medhavi.Contracts.Traceability
open Medhavi.Infrastructure.Observation

/// Anti-Corruption Layer (ACL) adapting internal infrastructure observation types
/// to external public client and API wire contracts.
let toTelemetryItemDto (evt: TelemetryEvent) : TelemetryItemDto =
    let bc = evt.Properties.TryFind "BoundedContext" |> Option.map string |> Option.defaultValue ""
    let aggType = evt.Properties.TryFind "AggregateType" |> Option.map string |> Option.defaultValue ""
    let aggId = evt.Properties.TryFind "AggregateId" |> Option.map string |> Option.defaultValue ""
    let cmdType = evt.Properties.TryFind "CommandType" |> Option.map string |> Option.defaultValue ""
    let corrId = evt.CorrelationId |> Option.map CorrelationId.toString |> Option.defaultValue ""
    let causId = evt.CausationId |> Option.map CorrelationId.toString |> Option.defaultValue ""

    {
        EventId = evt.EventId.ToString()
        Timestamp = evt.Timestamp
        Severity = string evt.Severity
        BoundedContext = bc
        AggregateType = aggType
        AggregateId = aggId
        CommandType = cmdType
        Message = evt.Message
        CorrelationId = corrId
        CausationId = causId
    }

let toRuleEvaluationDto (r: RuleEvaluation) : RuleEvaluationDto =
    {
        RuleId = r.RuleId
        Passed = r.Passed
        ReasonCode = r.ReasonCode |> Option.defaultValue ""
        Evidence = r.Evidence
    }

let toDecisionDossierDto (dt: DecisionTrace) : DecisionDossierDto =
    {
        DecisionId = dt.DecisionId
        CapabilityId = dt.CapabilityId
        PolicyId = dt.PolicyId |> Option.defaultValue ""
        PolicyVersion = dt.PolicyVersion |> Option.map string |> Option.defaultValue ""
        Outcome = dt.Outcome
        RulesEvaluated = dt.RulesEvaluated |> List.map toRuleEvaluationDto
        RationaleSummary = dt.Rationale.Summary
        Alternatives = dt.Rationale.Alternatives
    }

let toDomainTransitionDto (t: DomainTransition) : DomainTransitionDto =
    {
        SourceContext = t.SourceContext
        TargetContext = t.TargetContext
        EventCount = t.EventCount
        AverageLatencyMs = t.AverageLatencyMs
    }

let toStageExecutionDto (s: StageExecutionDetail) : StageExecutionDto =
    {
        StageName = s.StageName
        DurationMs = s.DurationMs
    }

let toTraceDossierDto (d: TraceDossier) : TraceDossierDto =
    {
        CorrelationId = d.CorrelationId
        RootCommandType = d.RootCommandType |> Option.defaultValue ""
        BoundedContextsInvolved = d.BoundedContextsInvolved
        TotalDurationMs = d.TotalDurationMs
        EventCount = d.EventCount
        Events = d.Events |> List.map toTelemetryItemDto
        Decisions = d.DecisionTraces |> List.map toDecisionDossierDto
        Transitions = d.Transitions |> List.map toDomainTransitionDto
        StageTimings = d.StageTimings |> List.map toStageExecutionDto
    }

let toHighFidelityTraceDossierDto (d: TraceDossier) : HighFidelityTraceDossierDto =
    let sortedEvents = d.Events |> List.sortBy (fun e -> e.Timestamp)
    let minTime = if List.isEmpty sortedEvents then DateTimeOffset.UtcNow else sortedEvents.Head.Timestamp
    let maxTime = if List.isEmpty sortedEvents then DateTimeOffset.UtcNow else (List.last sortedEvents).Timestamp

    let decisionBlocks =
        d.DecisionTraces
        |> List.map (fun dt ->
            let ruleTraces =
                dt.RulesEvaluated
                |> List.map (fun r ->
                    {
                        RuleId = r.RuleId
                        RuleName = r.RuleId
                        Passed = r.Passed
                        ReasonCode = r.ReasonCode |> Option.defaultValue ""
                        EvaluatedValue = ""
                        ThresholdValue = ""
                        Evidence = r.Evidence
                    })
            {
                DecisionId = dt.DecisionId
                CapabilityId = dt.CapabilityId
                PolicyId = dt.PolicyId |> Option.defaultValue ""
                PolicyVersion = dt.PolicyVersion |> Option.map string |> Option.defaultValue ""
                Outcome = dt.Outcome
                OutcomeCategory = if dt.Outcome.Equals("Accepted", StringComparison.OrdinalIgnoreCase) then "Approved" else "Warning"
                RationaleSummary = dt.Rationale.Summary
                RulesEvaluated = ruleTraces
                AlternativesConsidered = dt.Rationale.Alternatives
                ExecutionDurationMs = 0.0
            })

    let processingBlocks =
        sortedEvents
        |> List.groupBy (fun e ->
            let bc = e.Properties.TryFind "BoundedContext" |> Option.map string |> Option.defaultValue "Unknown"
            let aggId = e.Properties.TryFind "AggregateId" |> Option.map string |> Option.defaultValue "Unknown"
            bc, aggId)
        |> List.map (fun ((bc, aggId), evts) ->
            let firstEvt = evts |> List.minBy (fun e -> e.Timestamp)
            let lastEvt = evts |> List.maxBy (fun e -> e.Timestamp)
            let aggType = firstEvt.Properties.TryFind "AggregateType" |> Option.map string |> Option.defaultValue ""
            let cmdType = firstEvt.Properties.TryFind "CommandType" |> Option.map string |> Option.defaultValue firstEvt.Message
            let hasErr = evts |> List.exists (fun e -> e.Severity = TelemetrySeverity.Errors || e.Severity = TelemetrySeverity.Critical)
            let hasWarn = evts |> List.exists (fun e -> e.Severity = TelemetrySeverity.Warning)
            let matchedDecs = decisionBlocks |> List.filter (fun dec -> dec.CapabilityId = bc || dec.DecisionId = aggId)
            {
                BoundedContext = bc
                AggregateType = aggType
                AggregateId = aggId
                CommandType = cmdType
                InitialLifecycleState = ""
                ResultingLifecycleState = ""
                ExecutionStart = firstEvt.Timestamp
                ExecutionEnd = lastEvt.Timestamp
                DurationMs = (lastEvt.Timestamp - firstEvt.Timestamp).TotalMilliseconds
                Decisions = matchedDecs
                EmittedEvents = evts |> List.map (fun e -> e.EventId.ToString())
                EmittedNotifications = []
                HasErrors = hasErr
                HasWarnings = hasWarn
            })

    let hops =
        sortedEvents
        |> List.choose (fun e ->
            e.Properties.TryFind "BoundedContext"
            |> Option.map (fun bc -> (string bc, e.Timestamp, e.Properties.TryFind "CommandType" |> Option.map string |> Option.defaultValue e.Message)))
        |> Seq.pairwise
        |> Seq.mapi (fun idx ((srcBc, srcTime, srcMsg), (tgtBc, tgtTime, tgtMsg)) ->
            let isLoop = srcBc = tgtBc || (tgtBc = "Demand" && srcBc = "Supply") || (tgtBc = "Promise" && srcBc = "Supply")
            let hopKind = if isLoop then "FeedbackLoopBack" else "ForwardHop"
            let lat = max 0.0 (tgtTime - srcTime).TotalMilliseconds
            {
                HopId = sprintf "HOP-%s-%s-%d" srcBc tgtBc (idx + 1)
                SourceContext = srcBc
                TargetContext = tgtBc
                TriggeringNotificationId = ""
                HopKind = hopKind
                CorrelationId = d.CorrelationId
                CausationId = ""
                DispatchedTimestamp = srcTime
                ReceivedTimestamp = tgtTime
                TransportLatencyMs = lat
                PayloadSummary = sprintf "%s -> %s" srcMsg tgtMsg
                IsLoopBack = isLoop
                IterationIndex = if isLoop then 2 else 1
            })
        |> Seq.toList

    let loopIndicators =
        hops
        |> List.filter (fun h -> h.IsLoopBack)
        |> List.mapi (fun idx h ->
            {
                LoopIdentifier = sprintf "LOOP-%s-%s-%d" h.SourceContext h.TargetContext (idx + 1)
                OriginatingContext = h.SourceContext
                ConstrainedResourceOrEntity = h.TargetContext
                TotalIterations = 2
                ConvergenceAchieved = true
                ConvergenceReason = "Constraint satisfied on subsequent iteration"
                Iterations = [ h ]
            })

    let alerts =
        sortedEvents
        |> List.filter (fun e -> e.Severity = TelemetrySeverity.Errors || e.Severity = TelemetrySeverity.Critical || e.Severity = TelemetrySeverity.Warning)
        |> List.mapi (fun idx e ->
            let bc = e.Properties.TryFind "BoundedContext" |> Option.map string |> Option.defaultValue ""
            let aggId = e.Properties.TryFind "AggregateId" |> Option.map string |> Option.defaultValue ""
            {
                AlertId = sprintf "ALT-%d" (idx + 1)
                Severity = string e.Severity
                BoundedContext = bc
                AggregateId = aggId
                FailedRuleId = None
                BreachType = "ExecutionAnomaly"
                Description = e.Message
                FinancialOrSlaExposure = None
                MitigationActionTaken = None
                EscalatedToCoreExceptionId = None
            })

    let overallStatus =
        if alerts |> List.exists (fun a -> a.Severity = "Errors" || a.Severity = "Critical") then "Failed"
        elif alerts |> List.exists (fun a -> a.Severity = "Warning") then "Warning"
        else "Succeeded"

    {
        CorrelationId = d.CorrelationId
        RootTransactionId = d.CorrelationId
        RootCommandType = d.RootCommandType |> Option.defaultValue "Unknown"
        OverallStatus = overallStatus
        StartTime = minTime
        EndTime = maxTime
        TotalLatencyMs = d.TotalDurationMs
        BoundedContextsInvolved = d.BoundedContextsInvolved
        ProcessingBlocks = processingBlocks
        CrossContextHops = hops
        FeedbackLoops = loopIndicators
        AlertsAndWarnings = alerts
    }

let toTraceabilityQueryService (ops: ObservationQueryOps) : TraceabilityQueryService =
    {
        QueryTelemetry =
            fun (q) ->
                System.Threading.Tasks.Task.FromResult(
                    let internalQuery =
                        {
                            SearchTerm = q.SearchTerm
                            CorrelationId = q.CorrelationId
                            AggregateId = q.AggregateId
                            BoundedContext = q.BoundedContext
                            MinimumSeverity =
                                q.MinimumSeverity
                                |> Option.bind(fun s ->
                                    match s.ToLowerInvariant() with
                                    | "trace" -> Some TelemetrySeverity.Trace
                                    | "debug" -> Some TelemetrySeverity.Debug
                                    | "information"
                                    | "info" -> Some TelemetrySeverity.Information
                                    | "warning"
                                    | "warn" -> Some TelemetrySeverity.Warning
                                    | "errors"
                                    | "error" -> Some TelemetrySeverity.Errors
                                    | "critical" -> Some TelemetrySeverity.Critical
                                    | _ -> None)
                            StartTime = q.StartTime
                            EndTime = q.EndTime
                            Limit = q.Limit
                            Skip = q.Skip
                        }

                    let result = ops.QueryEvents(internalQuery)

                    {
                        Items = result.Events |> List.map toTelemetryItemDto
                        TotalCount = result.TotalMatches
                    }
                )

        GetTraceDossier =
            fun (cid) ->
                System.Threading.Tasks.Task.FromResult(ops.GetTraceDossier(cid) |> Option.map toTraceDossierDto)

        GetHighFidelityTraceDossier =
            fun (cid) ->
                System.Threading.Tasks.Task.FromResult(ops.GetTraceDossier(cid) |> Option.map toHighFidelityTraceDossierDto)

        GetRecentTransitions =
            fun () ->
                System.Threading.Tasks.Task.FromResult(ops.GetRecentTransitions() |> List.map toDomainTransitionDto)
    }
