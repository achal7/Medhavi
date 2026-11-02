namespace Medhavi.Promise.LearnFromPromises.PromiseCalibrationModel

/// CA-P-011 Learn & Calibrate Promising Models — Projections
/// Architectural Lineage: ARS §3, §14; Table A.3 EV-P-027, EV-P-028

open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model

module Projections =
    let mapParamToDto (p: CalibratedParameter) : CalibratedParameterDto =
        { ParameterName = p.ParameterName
          TargetEntityType = p.TargetEntityType
          EntityIdentifier = p.EntityIdentifier
          BaselineValue = p.BaselineValue
          EmpiricalObservedValue = p.EmpiricalObservedValue
          RecommendedCalibratedValue = p.RecommendedCalibratedValue
          DampenedStepDelta = p.DampenedStepDelta }

    let toDto (entity: PromiseCalibrationModel) : PromiseCalibrationModelDto =
        let stateStr =
            match entity.LifecycleState with
            | DriftDetected -> "DriftDetected"
            | CalibrationProposed _ -> "CalibrationProposed"
            | Ratified _ -> "Ratified"
            | Applied _ -> "Applied"
            | Dismissed _ -> "Dismissed"

        let avgDelta =
            if entity.Parameters.IsEmpty then 0.0m
            else
                let sum = entity.Parameters |> List.sumBy (fun p -> p.DampenedStepDelta)
                sum / decimal entity.Parameters.Length

        { ModelId = PromiseLearningId.value entity.ModelId
          PlanningScopeId = PlanningScopeId.value entity.ScopeId
          ObservationWindowDays = entity.ObservationWindowDays
          SampleSizeCount = entity.SampleSizeCount
          AdjustedParametersCount = entity.Parameters.Length
          AverageBufferDeltaDays = avgDelta
          Parameters = entity.Parameters |> List.map mapParamToDto
          LifecycleState = stateStr
          DecisionTraceability = entity.DecisionTraceability }

    type State = Map<PromiseLearningId, PromiseCalibrationModelDto>

    let initial: State = Map.empty

    let apply (state: State) (event: PromiseCalibrationEvent) : State =
        match event with
        | PromiseCalibrationProposed e ->
            state
            |> Map.change e.ModelId (function
                | Some dto ->
                    Some
                        { dto with
                            LifecycleState = "CalibrationProposed" }
                | None ->
                    Some
                        { ModelId = PromiseLearningId.value e.ModelId
                          PlanningScopeId = PlanningScopeId.value e.ScopeId
                          ObservationWindowDays = 90
                          SampleSizeCount = 50
                          AdjustedParametersCount = e.AdjustedParametersCount
                          AverageBufferDeltaDays = 0.0m
                          Parameters = []
                          LifecycleState = "CalibrationProposed"
                          DecisionTraceability = None })

        | PromiseCalibrationApplied e ->
            state
            |> Map.change e.ModelId (Option.map (fun dto ->
                { dto with
                    LifecycleState = "Applied" }))

    let seedFromAggregates (aggregates: PromiseCalibrationModel list) : State =
        aggregates
        |> List.map (fun a -> a.ModelId, toDto a)
        |> Map.ofList

