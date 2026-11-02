/// CA-P-009 Resolve Exceptions — Model
/// Traces to: SE-P-080, CR-P-027, CR-P-028
module Medhavi.Promise.ResolveExceptions.PromiseExceptionAssessment.Model

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers

type ExceptionLifecycleState =
    | Detected
    | Triaging
    | MitigationApproved of PlanId: string
    | ExecutedResolved of ResolutionTime: Timestamp * PlanId: string
    | DeCommitted of Reason: string

/// SE-P-080 Promise Exception Assessment Aggregate Root
type PromiseExceptionAssessment =
    { ExceptionId: PromiseExceptionId
      CommitmentId: OrderCommitmentId
      OrderLineId: OrderLineId
      DisruptionType: PromiseDisruptionType
      SeverityTier: PromiseRiskTier
      RootCauseDescription: string
      CandidatePlans: MitigationActionPlan list
      SelectedPlanId: string option
      LifecycleState: ExceptionLifecycleState
      DetectedTime: Timestamp
      ResolvedTime: Timestamp option
      DecisionTraceability: string option }

type RaisePromiseExceptionCmd =
    { ExceptionId: PromiseExceptionId
      CommitmentId: OrderCommitmentId
      OrderLineId: OrderLineId
      DisruptionType: PromiseDisruptionType
      Description: string
      IncidentTime: Timestamp }

type ResolvePromiseExceptionCmd =
    { ExceptionId: PromiseExceptionId
      SelectedPlanId: string
      AuthorizationTime: Timestamp }

type PromiseExceptionCmd =
    | Raise of RaisePromiseExceptionCmd
    | Resolve of ResolvePromiseExceptionCmd

    static member GetId(cmd: PromiseExceptionCmd) =
        match cmd with
        | Raise c -> c.ExceptionId
        | Resolve c -> c.ExceptionId

type PromiseExceptionEvent =
    | DisruptionCaseOpened of Assessment: PromiseExceptionAssessment
    | DisruptionRecoveryExecuted of ExceptionId: PromiseExceptionId * CommitmentId: OrderCommitmentId * PlanId: string * ResolutionTime: Timestamp * TraceId: string

let evolve: Evolve<PromiseExceptionAssessment, PromiseExceptionEvent> =
    fun (state: PromiseExceptionAssessment option) (event: PromiseExceptionEvent) ->
        match event with
        | DisruptionCaseOpened assessment ->
            Some assessment

        | DisruptionRecoveryExecuted(_, _, planId, resTime, traceId) ->
            state
            |> Option.map (fun a ->
                let updatedPlans =
                    a.CandidatePlans
                    |> List.map (fun p -> if p.PlanId = planId then { p with Authorized = true } else p)
                { a with
                    CandidatePlans = updatedPlans
                    SelectedPlanId = Some planId
                    LifecycleState = ExecutedResolved(resTime, planId)
                    ResolvedTime = Some resTime
                    DecisionTraceability = Some traceId })

