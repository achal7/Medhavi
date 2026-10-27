namespace Medhavi.Contracts.Traceability

open System

[<CLIMutable>]
type TelemetryItemDto =
    {
        EventId: string
        Timestamp: DateTimeOffset
        Severity: string
        BoundedContext: string
        AggregateType: string
        AggregateId: string
        CommandType: string
        Message: string
        CorrelationId: string
        CausationId: string
    }

[<CLIMutable>]
type RuleEvaluationDto =
    {
        RuleId: string
        Passed: bool
        ReasonCode: string
        Evidence: string list
    }

[<CLIMutable>]
type DecisionDossierDto =
    {
        DecisionId: string
        CapabilityId: string
        PolicyId: string
        PolicyVersion: string
        Outcome: string
        RulesEvaluated: RuleEvaluationDto list
        RationaleSummary: string
        Alternatives: (string * string) list
    }

[<CLIMutable>]
type DomainTransitionDto =
    {
        SourceContext: string
        TargetContext: string
        EventCount: int
        AverageLatencyMs: float
    }

[<CLIMutable>]
type StageExecutionDto =
    { StageName: string; DurationMs: float }

[<CLIMutable>]
type TraceDossierDto =
    {
        CorrelationId: string
        RootCommandType: string
        BoundedContextsInvolved: string list
        TotalDurationMs: float
        EventCount: int
        Events: TelemetryItemDto list
        Decisions: DecisionDossierDto list
        Transitions: DomainTransitionDto list
        StageTimings: StageExecutionDto list
    }

[<CLIMutable>]
type TelemetryFilterQuery =
    {
        SearchTerm: string option
        CorrelationId: string option
        AggregateId: string option
        BoundedContext: string option
        MinimumSeverity: string option
        StartTime: DateTimeOffset option
        EndTime: DateTimeOffset option
        Limit: int
        Skip: int
    }

    static member Default =
        {
            SearchTerm = None
            CorrelationId = None
            AggregateId = None
            BoundedContext = None
            MinimumSeverity = None
            StartTime = None
            EndTime = None
            Limit = 100
            Skip = 0
        }

[<CLIMutable>]
type TelemetryFilterResult =
    {
        Items: TelemetryItemDto list
        TotalCount: int
    }

type TraceabilityQueryService =
    {
        QueryTelemetry: TelemetryFilterQuery -> System.Threading.Tasks.Task<TelemetryFilterResult>
        GetTraceDossier: string -> System.Threading.Tasks.Task<TraceDossierDto option>
        GetRecentTransitions: unit -> System.Threading.Tasks.Task<DomainTransitionDto list>
    }
