/// CA-P-009 Resolve Exceptions — Behaviors
/// Traces to: AB-P-023 Raise Exception, AB-P-025 Authorize Mitigation
module Medhavi.Promise.ResolveExceptions.PromiseExceptionAssessment.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Model
open Rules
open Decisions
open Policies

let buildTrace policy decisionId decisionOutcome state events summary =
    let policyId = policy |> Option.map (fun (p: DisruptionMitigationPolicy) -> p.PolicyId)
    let policyVersion = policy |> Option.map (fun (p: DisruptionMitigationPolicy) -> p.Version)

    buildDecisionWithTrace
        evolve
        state
        events
        decisionId
        []
        Capabilities.resolvePromiseExceptionsAndManageDisruptions.Id
        decisionOutcome
        policyId
        policyVersion
        [ SemanticObjects.promiseExceptionAssessment.Id ]
        (Some summary)

/// AB-P-023: Raise Promise Exception
let raise
    (policy: DisruptionMitigationPolicy)
    (commitmentExists: bool)
    : Decide<PromiseExceptionAssessment, RaisePromiseExceptionCmd, PromiseExceptionEvent> =
    fun (cmd: RaisePromiseExceptionCmd) (state: PromiseExceptionAssessment option) ->
        result {
            match state with
            | Some _ ->
                return! Error(DomainError.conflict ($"PromiseException already exists: {PromiseExceptionId.value cmd.ExceptionId}", Capabilities.resolvePromiseExceptionsAndManageDisruptions.Id))
            | None ->
                let input: RaiseExceptionInput =
                    { Cmd = cmd
                      CommitmentExists = commitmentExists
                      Policy = policy }

                let! decision = evaluateRaise raiseRules input
                let traceId = Guid.NewGuid().ToString()

                match decision.Outcome with
                | RaiseAuthorized ->
                    // Synthesize default candidate mitigation plans (Expedite vs Alternate vs RePromise)
                    let baseTime = Timestamp.value cmd.IncidentTime
                    let plan1: MitigationActionPlan =
                        { PlanId = "PLAN-EXPEDITE"
                          ActionType = "AirFreightExpedite"
                          EstimatedExpediteCost = 1200.0m
                          NewProjectedDeliveryDate =
                            let fin = baseTime.AddDays(2.0)
                            Timestamp.create fin |> Result.defaultWith (fun _ -> cmd.IncidentTime)
                          ProjectedGrossMarginPct = 22.5m
                          Authorized = false }

                    let plan2: MitigationActionPlan =
                        { PlanId = "PLAN-REROUTE"
                          ActionType = "AlternateNodeFulfillment"
                          EstimatedExpediteCost = 450.0m
                          NewProjectedDeliveryDate =
                            let fin = baseTime.AddDays(4.0)
                            Timestamp.create fin |> Result.defaultWith (fun _ -> cmd.IncidentTime)
                          ProjectedGrossMarginPct = 28.0m
                          Authorized = false }

                    let assessment: PromiseExceptionAssessment =
                        { ExceptionId = cmd.ExceptionId
                          CommitmentId = cmd.CommitmentId
                          OrderLineId = cmd.OrderLineId
                          DisruptionType = cmd.DisruptionType
                          SeverityTier = PromiseRiskTier.High
                          RootCauseDescription = cmd.Description
                          CandidatePlans = [ plan1; plan2 ]
                          SelectedPlanId = None
                          LifecycleState = Detected
                          DetectedTime = cmd.IncidentTime
                          ResolvedTime = None
                          DecisionTraceability = Some traceId }

                    let events = [ DisruptionCaseOpened assessment ]
                    return buildTrace (Some policy) traceId decision state events $"Disruption incident opened: {cmd.Description}"

                | RaiseRejected reason ->
                    return! Error(DomainError.rule (reason, rule = Rules.promiseExceptionIncidentIdentity.Id))
        }

/// AB-P-025: Authorize Mitigation
let resolve
    (policy: DisruptionMitigationPolicy)
    : Decide<PromiseExceptionAssessment, ResolvePromiseExceptionCmd, PromiseExceptionEvent> =
    fun (cmd: ResolvePromiseExceptionCmd) (state: PromiseExceptionAssessment option) ->
        result {
            match state with
            | None ->
                return! Error(DomainError.notFound (SemanticObjects.promiseExceptionAssessment.Id, PromiseExceptionId.value cmd.ExceptionId))
            | Some assessment ->
                let input: ResolveExceptionInput =
                    { Cmd = cmd
                      CurrentAssessment = state
                      Policy = policy }

                let! decision = evaluateResolve resolveRules input
                let traceId = Guid.NewGuid().ToString()

                match decision.Outcome with
                | MitigationPlanAuthorized planId ->
                    let events =
                        [ DisruptionRecoveryExecuted(
                            cmd.ExceptionId,
                            assessment.CommitmentId,
                            planId,
                            cmd.AuthorizationTime,
                            traceId) ]
                    return buildTrace (Some policy) traceId decision state events $"Mitigation plan {planId} authorized"

                | MitigationRejected reason ->
                    return! Error(DomainError.rule (reason, rule = Rules.disruptionMitigationExpediteRule.Id))
        }

