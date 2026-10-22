/// SE-S-071 — Published Production Schedule Domain Model
/// Traces to: Supply Intelligence Specification §4.3.1 (SE-S-071, AB-S-061, EV-S-061)
module Medhavi.Supply.ScheduleProduction.PublishedProductionSchedule.Model

open Medhavi.SemanticModel

/// Baseline publication trigger classification (Supply Intelligence Specification §6, DE-S-063)
[<RequireQualifiedAccess>]
type PublicationTriggerType =
    | ScheduledPublication
    | EmergencyReplan
    | GovernanceOverride
    | CadenceScheduled
    | ManualRelease
    | ExceptionDriven of Reason: string

/// Publication authorization outcome (Supply Intelligence Specification §6, DE-S-063)
[<RequireQualifiedAccess>]
type PublicationAuthorization =
    | Authorized
    | AuthorizedWithFlags of Flags: string list
    | Rejected of Reasons: string list

[<RequireQualifiedAccess>]
type PublishedScheduleLifecycleState =
    | Draft
    | Published
    | Superseded

type AggregateScheduleMetrics =
    {
        TotalChangeoverHours: decimal
        ResourceUtilization: decimal
        OvertimeRequiredHours: decimal
        ScheduleAdherenceProjection: decimal
    }

type FeasibilitySummary =
    {
        FeasibleCount: int
        ConditionallyFeasibleCount: int
        InfeasibleCount: int
        TopConstraints: string list
    }

type StabilitySummary =
    {
        AverageStabilityScore: decimal
        HighVolatilityResources: PhysicalResourceId list
    }

type RiskSignals =
    {
        ResourcesAtRiskOfDelay: PhysicalResourceId list
        MaterialsAtRiskOfUnavailability: PhysicalResourceId list
        BottleneckResources: PhysicalResourceId list
        OvertimeDependentSchedules: PhysicalResourceId list
    }

type RecoveryOpportunities =
    {
        RecoverableResources: PhysicalResourceId list
        EstimatedRecoverableHours: decimal
        RecoveryStrategy: string
    }

/// SE-S-071 Aggregate Root
type PublishedProductionScheduleAggregate =
    {
        PlanningScopeId: PlanningScopeId
        PlanningPeriodId: PlanningPeriodId
        VersionNumber: int
        LifecycleState: PublishedScheduleLifecycleState
        ActiveScheduleIds: string list
        Metrics: AggregateScheduleMetrics
        Feasibility: FeasibilitySummary
        Stability: StabilitySummary
        Risks: RiskSignals
        Recovery: RecoveryOpportunities
        TriggerType: PublicationTriggerType
        PublicationTimestamp: Timestamp option
        SupersededVersionNumber: int option
        SupersededTimestamp: Timestamp option
    }

type PublishProductionScheduleCmd =
    {
        PlanningScopeId: PlanningScopeId
        PlanningPeriodId: PlanningPeriodId
        TriggerType: PublicationTriggerType
        Timestamp: Timestamp
    }

type PublishedProductionScheduleEvent =
    | ProductionSchedulePublished of baseline: PublishedProductionScheduleAggregate * supersededVersionOpt: int option

let evolve
    (stateOpt: PublishedProductionScheduleAggregate option)
    (event: PublishedProductionScheduleEvent)
    : PublishedProductionScheduleAggregate option =
    match event with
    | ProductionSchedulePublished(baseline, _) -> Some baseline

let replay (events: PublishedProductionScheduleEvent seq) : PublishedProductionScheduleAggregate option =
    Seq.fold evolve None events
