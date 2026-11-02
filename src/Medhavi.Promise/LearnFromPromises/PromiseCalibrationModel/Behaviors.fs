/// CA-P-011 Learn & Calibrate Promising Models — Behaviors
/// Architectural Lineage: ARS §3, §4, §14; AB-P-026, AB-P-027
module Medhavi.Promise.LearnFromPromises.PromiseCalibrationModel.Behaviors

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

let evolve (state: PromiseCalibrationModel option) (event: PromiseCalibrationEvent) : PromiseCalibrationModel option =
    match event with
    | PromiseCalibrationProposed _ -> state
    | PromiseCalibrationApplied e ->
        match state with
        | Some s ->
            Some
                { s with
                    LifecycleState = Applied e.ActivationTime }
        | None -> None

let buildTrace policy decisionId decisionOutcome state events summary =
    let policyId = policy |> Option.map (fun (p: PromisingModelCalibrationPolicy) -> p.PolicyId)
    let policyVersion = policy |> Option.map (fun (p: PromisingModelCalibrationPolicy) -> p.Version)

    buildDecisionWithTrace
        evolve
        state
        events
        decisionId
        []
        Capabilities.learnAndCalibratePromisingModels.Id
        decisionOutcome
        policyId
        policyVersion
        [ SemanticObjects.promiseCalibrationModel.Id ]
        (Some summary)

/// AB-P-026: Propose Model Calibration
let propose
    (policy: PromisingModelCalibrationPolicy)
    (historicalSampleSize: int)
    : Decide<PromiseCalibrationModel, ProposeModelCalibrationCmd, PromiseCalibrationEvent> =
    fun (cmd: ProposeModelCalibrationCmd) (state: PromiseCalibrationModel option) ->
        result {
            match state with
            | Some _ ->
                return!
                    Error(
                        DomainError.conflict
                            ($"Model already exists: {PromiseLearningId.value cmd.ModelId}",
                             Capabilities.learnAndCalibratePromisingModels.Id)
                    )
            | None ->
                let input: ProposeInput =
                    { Cmd = cmd
                      HistoricalSampleSize = historicalSampleSize
                      Policy = policy }

                let! decision = evaluatePropose proposeRules input
                let traceId = Guid.NewGuid().ToString()

                match decision.Outcome with
                | ProposeAuthorized ->
                    let transitLaneParam =
                        Algorithms.calibrateParameter
                            policy
                            "TransitLaneSafetyBufferHours"
                            "TransportationLane"
                            "LANE-CHI-NYC"
                            48.0m
                            58.0m

                    let mfgBufferParam =
                        Algorithms.calibrateParameter
                            policy
                            "AssemblyThroughputBufferHours"
                            "WorkCenter"
                            "PLANT-01-MACH-01"
                            24.0m
                            27.5m

                    let calibratedParams = [ transitLaneParam; mfgBufferParam ]

                    let model: PromiseCalibrationModel =
                        { ModelId = cmd.ModelId
                          ScopeId = cmd.ScopeId
                          ObservationWindowDays = cmd.ObservationWindowDays
                          SampleSizeCount = historicalSampleSize
                          Parameters = calibratedParams
                          LifecycleState = CalibrationProposed cmd.EvaluationTime
                          DecisionTraceability = Some traceId }

                    let events =
                        [ PromiseCalibrationProposed
                            {| ModelId = cmd.ModelId
                               ScopeId = cmd.ScopeId
                               AdjustedParametersCount = calibratedParams.Length
                               EvaluationTime = cmd.EvaluationTime |} ]

                    return buildTrace (Some policy) traceId decision state events $"Promising model calibration proposed for {PromiseLearningId.value cmd.ModelId}"

                | ProposeRejected reason ->
                    return! Error(DomainError.rule (reason, rule = Rules.promiseModelCalibrationRecordIdentity.Id))
        }

/// AB-P-027: Apply Model Calibration
let apply
    (policy: PromisingModelCalibrationPolicy)
    : Decide<PromiseCalibrationModel, ApplyModelCalibrationCmd, PromiseCalibrationEvent> =
    fun (cmd: ApplyModelCalibrationCmd) (state: PromiseCalibrationModel option) ->
        result {
            match state with
            | None ->
                return!
                    Error(
                        DomainError.notFound
                            (SemanticObjects.promiseCalibrationModel.Id, PromiseLearningId.value cmd.ModelId)
                    )
            | Some model ->
                let input: ApplyInput =
                    { Cmd = cmd
                      CurrentModel = state
                      Policy = policy }

                let! decision = evaluateApply applyRules input
                let traceId = Guid.NewGuid().ToString()

                match decision.Outcome with
                | ApplyAuthorized ->
                    let events =
                        [ PromiseCalibrationApplied
                            {| ModelId = cmd.ModelId
                               ScopeId = model.ScopeId
                               ApprovedBy = cmd.ApprovedBySteward
                               ActivationTime = cmd.ActivationTime |} ]

                    return buildTrace (Some policy) traceId decision state events $"Promising model calibration applied for {PromiseLearningId.value cmd.ModelId}"

                | ApplyRejected reason ->
                    return! Error(DomainError.rule (reason, rule = Rules.stewardAuthorizationInvariant.Id))
        }
