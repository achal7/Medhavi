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

// High-Fidelity Causal Dossier Contracts

[<CLIMutable>]
type RuleEvaluationTraceDto =
    {
        RuleId: string
        RuleName: string
        Passed: bool
        ReasonCode: string
        EvaluatedValue: string
        ThresholdValue: string
        Evidence: string list
    }

[<CLIMutable>]
type DecisionTraceBlockDto =
    {
        DecisionId: string
        CapabilityId: string
        PolicyId: string
        PolicyVersion: string
        Outcome: string
        OutcomeCategory: string
        RationaleSummary: string
        RulesEvaluated: RuleEvaluationTraceDto list
        AlternativesConsidered: (string * string) list
        ExecutionDurationMs: float
    }

[<CLIMutable>]
type AggregateProcessingBlockDto =
    {
        BoundedContext: string
        AggregateType: string
        AggregateId: string
        CommandType: string
        InitialLifecycleState: string
        ResultingLifecycleState: string
        ExecutionStart: DateTimeOffset
        ExecutionEnd: DateTimeOffset
        DurationMs: float
        Decisions: DecisionTraceBlockDto list
        EmittedEvents: string list
        EmittedNotifications: string list
        HasErrors: bool
        HasWarnings: bool
    }

[<CLIMutable>]
type CrossContextHopDto =
    {
        HopId: string
        SourceContext: string
        TargetContext: string
        TriggeringNotificationId: string
        HopKind: string
        CorrelationId: string
        CausationId: string
        DispatchedTimestamp: DateTimeOffset
        ReceivedTimestamp: DateTimeOffset
        TransportLatencyMs: float
        PayloadSummary: string
        IsLoopBack: bool
        IterationIndex: int
    }

[<CLIMutable>]
type FeedbackLoopIndicatorDto =
    {
        LoopIdentifier: string
        OriginatingContext: string
        ConstrainedResourceOrEntity: string
        TotalIterations: int
        ConvergenceAchieved: bool
        ConvergenceReason: string
        Iterations: CrossContextHopDto list
    }

[<CLIMutable>]
type TraceAlertDto =
    {
        AlertId: string
        Severity: string
        BoundedContext: string
        AggregateId: string
        FailedRuleId: string option
        BreachType: string
        Description: string
        FinancialOrSlaExposure: string option
        MitigationActionTaken: string option
        EscalatedToCoreExceptionId: string option
    }

[<CLIMutable>]
type HighFidelityTraceDossierDto =
    {
        CorrelationId: string
        RootTransactionId: string
        RootCommandType: string
        OverallStatus: string
        StartTime: DateTimeOffset
        EndTime: DateTimeOffset
        TotalLatencyMs: float
        BoundedContextsInvolved: string list
        ProcessingBlocks: AggregateProcessingBlockDto list
        CrossContextHops: CrossContextHopDto list
        FeedbackLoops: FeedbackLoopIndicatorDto list
        AlertsAndWarnings: TraceAlertDto list
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
        GetHighFidelityTraceDossier: string -> System.Threading.Tasks.Task<HighFidelityTraceDossierDto option>
        GetRecentTransitions: unit -> System.Threading.Tasks.Task<DomainTransitionDto list>
    }
