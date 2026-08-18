module Medhavi.Demand.UnderstandDemand.DemandObservation.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Foundation.Failure
open Medhavi.Demand
open Medhavi.Demand.ArsIdentifiers
open Model
open Rules
open Decisions
open Policies

let buildTrace policy decisionId decisionOutcome state events summary =
    let policyId = policy |> Option.map(fun p -> p.PolicyId)
    let policyVersion = policy |> Option.map(fun p -> p.Version)

    buildDecisionWithTrace
        evolve
        state
        events
        decisionId
        []
        ArsIdentifiers.Capabilities.understandDemand.Id
        decisionOutcome
        policyId
        policyVersion
        [ ArsIdentifiers.SemanticObjects.demandObservation.Id ]
        (Some summary)

/// AB-D-001: Receive Demand Observation
let receive: Decide<DemandObservation, ReceiveObservationCmd, ObservationEvent> =
    fun (cmd: ReceiveObservationCmd) (state: DemandObservation option) ->
        result {
            match state with
            | Some _ ->
                return! Error(DomainError.conflict("Observation already exists", Capabilities.understandDemand.Id))
            | None ->
                let newObs: DemandObservation =
                    { ObservationId = cmd.ObservationId
                      Item = cmd.Item
                      Location = cmd.Location
                      Quantity = cmd.Quantity
                      ObservationType = cmd.ObservationType
                      BusinessTime = cmd.BusinessTime
                      ObservationTime = cmd.ObservationTime
                      SourceSystemProvenance = cmd.SourceSystemProvenance
                      LifecycleState = Received
                      DecisionTraceability = None }

                let events = [ ObservationReceived newObs ]
                let decision = { Outcome = Receive; Evaluations = [] }

                return buildTrace None Capabilities.understandDemand.Id decision None events "Observation received"
        }

/// AB-D-002: Evaluate Demand Observation
let evaluate
    (policy: DemandDataAcceptancePolicy)
    (sourceReliability: decimal)
    (hasDuplicate: bool)
    : Decide<DemandObservation, EvaluateObservationCmd, ObservationEvent> =
    fun (cmd: EvaluateObservationCmd) (state: DemandObservation option) ->
        result {
            let input: EvaluateInput =
                { Cmd = cmd
                  CurrentState = state
                  Policy = policy
                  SourceReliability = sourceReliability
                  HasDuplicateInWindow = hasDuplicate }

            let! decision = Decisions.evaluateObservation Rules.evaluationRules input

            let traceId = Guid.NewGuid().ToString()

            match decision.Outcome with
            | Accept ->
                let events = [ ObservationAccepted(cmd.ObservationId, cmd.EvaluationTime, traceId) ]
                return buildTrace (Some policy) traceId decision state events "Observation accepted"

            | Quarantine reasons ->
                let events = [ ObservationQuarantined(cmd.ObservationId, cmd.EvaluationTime, traceId) ]
                return buildTrace (Some policy) traceId decision state events (String.concat "; " reasons)

            | Reject reasons ->
                let events = [ ObservationRejected(cmd.ObservationId, cmd.EvaluationTime, traceId) ]
                return buildTrace (Some policy) traceId decision state events (String.concat "; " reasons)
        }
