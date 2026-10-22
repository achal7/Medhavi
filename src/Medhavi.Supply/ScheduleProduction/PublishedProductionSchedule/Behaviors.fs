/// AB-S-061 — Publish Production Schedule Aggregate Behavior
/// Traces to: Supply Intelligence Specification §4.3.1 (AB-S-061, EV-S-061)
module Medhavi.Supply.ScheduleProduction.PublishedProductionSchedule.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model
open Policies
open Algorithms
open Rules
open Decisions

type PublishScheduleContext =
    {
        ScopeId: PlanningScopeId
        PeriodId: PlanningPeriodId
        ActiveSchedules: ActiveScheduleFact list
        TriggerType: PublicationTriggerType
        ExistingPublishedBaseline: PublishedProductionScheduleAggregate option
        Policy: PublicationPolicyConfig
        Timestamp: Timestamp
    }

/// AB-S-061: Publish Production Schedule
/// Evaluates algorithms, decisions, and rules, transitions baseline state, and publishes EV-S-061
let publishProductionSchedule
    (ctx: PublishScheduleContext)
    : Result<PublishedProductionScheduleAggregate * PublishedProductionScheduleEvent list, DomainError> =

    result {
        // Step 1: Precondition validations (BR-S-071, BR-S-221)
        do! validateAggregateIdentity ctx.ScopeId ctx.PeriodId
        do! validatePublicationEligibility (not ctx.ActiveSchedules.IsEmpty) true

        // Step 2: Algorithmic Composition
        let composition = composeBaseline ctx.ActiveSchedules ctx.Policy

        // Step 3: Publication Decision (DE-S-063)
        let! decisionOutcome =
            evaluatePublicationDecision
                composition.Feasibility
                composition.Stability
                ctx.ActiveSchedules.Length
                ctx.TriggerType
                ctx.Policy

        match decisionOutcome with
        | PublicationDecisionOutcome.DoNotPublish reasons ->
            let reasonStr = String.concat "; " reasons
            return! Error(ruleViolationWithDetail ArsIdentifiers.Rules.productionSchedulePublicationGovernance $"Publication not authorized: {reasonStr}")

        | PublicationDecisionOutcome.Publish
        | PublicationDecisionOutcome.PublishWithFlags _ ->
            // Step 4: Versioning and Atomic Supersession
            let newVersion =
                match ctx.ExistingPublishedBaseline with
                | Some prior -> prior.VersionNumber + 1
                | None -> 1

            let supersededVersionOpt = ctx.ExistingPublishedBaseline |> Option.map(fun b -> b.VersionNumber)

            let publishedBaseline: PublishedProductionScheduleAggregate =
                {
                    PlanningScopeId = ctx.ScopeId
                    PlanningPeriodId = ctx.PeriodId
                    VersionNumber = newVersion
                    LifecycleState = PublishedScheduleLifecycleState.Published
                    ActiveScheduleIds = composition.ActiveScheduleIds
                    Metrics = composition.Metrics
                    Feasibility = composition.Feasibility
                    Stability = composition.Stability
                    Risks = composition.Risks
                    Recovery = composition.Recovery
                    TriggerType = ctx.TriggerType
                    PublicationTimestamp = Some ctx.Timestamp
                    SupersededVersionNumber = supersededVersionOpt
                    SupersededTimestamp = None
                }

            // Emit Enterprise Event EV-S-061
            let event =
                PublishedProductionScheduleEvent.ProductionSchedulePublished(publishedBaseline, supersededVersionOpt)

            return (publishedBaseline, [ event ])
    }

/// Pure decision function wrapping publishProductionSchedule for CommandPipeline execution
let decide
    (activeSchedules: ActiveScheduleFact list)
    (policy: PublicationPolicyConfig)
    (cmd: PublishProductionScheduleCmd)
    (stateOpt: PublishedProductionScheduleAggregate option)
    : Result<Decision<PublishedProductionScheduleAggregate, PublishedProductionScheduleEvent>, DomainError> =
    let ctx: PublishScheduleContext =
        { ScopeId = cmd.PlanningScopeId
          PeriodId = cmd.PlanningPeriodId
          ActiveSchedules = activeSchedules
          TriggerType = cmd.TriggerType
          ExistingPublishedBaseline = stateOpt
          Policy = policy
          Timestamp = cmd.Timestamp }

    match publishProductionSchedule ctx with
    | Error err -> Error err
    | Ok(aggregate, events) ->
        let traceOutcome = { Outcome = "SchedulePublished"; Evaluations = [] }
        let decision =
            buildDecisionWithTrace
                evolve
                stateOpt
                events
                ArsIdentifiers.Decisions.publishProductionScheduleDecision.Id
                []
                ArsIdentifiers.Capabilities.scheduleProduction.Id
                traceOutcome
                (Some ArsIdentifiers.Policies.schedulingPolicy.Id)
                (Some 1)
                [ ArsIdentifiers.SemanticObjects.publishedProductionSchedule.Id ]
                (Some $"Published production schedule baseline composed for {PlanningScopeId.value cmd.PlanningScopeId}")
        Ok decision
