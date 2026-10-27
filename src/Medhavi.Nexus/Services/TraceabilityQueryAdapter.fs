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

        GetRecentTransitions =
            fun () ->
                System.Threading.Tasks.Task.FromResult(ops.GetRecentTransitions() |> List.map toDomainTransitionDto)
    }
