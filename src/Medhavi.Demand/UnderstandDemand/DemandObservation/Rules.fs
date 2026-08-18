module Medhavi.Demand.UnderstandDemand.DemandObservation.Rules

open System
open Medhavi.SemanticModel
open Medhavi.Foundation.Contracts
open Medhavi.Demand
open Medhavi.Demand.ArsIdentifiers
open Model
open Policies

type EvaluateInput =
    { Cmd: EvaluateObservationCmd
      CurrentState: DemandObservation option
      Policy: DemandDataAcceptancePolicy
      SourceReliability: decimal
      HasDuplicateInWindow: bool }

let demandSignalTimeliness: Rule<EvaluateInput> =
    Rule.create
        Rules.demandSignalTimeliness.Id
        Rules.demandSignalTimeliness.Explanation
        (fun input ->
            match input.CurrentState with
            | Some obs ->
                let latency = (Timestamp.value obs.ObservationTime) - (Timestamp.value obs.BusinessTime)
                latency.TotalMinutes <= float input.Policy.MaxDataLatencyMinutes
            | None -> false)
        (fun input ->
            let latency =
                input.CurrentState
                |> Option.map(fun o -> (Timestamp.value o.ObservationTime) - (Timestamp.value o.BusinessTime))
                |> Option.defaultValue TimeSpan.Zero

            sprintf "Latency: %f mins, Max: %d mins" latency.TotalMinutes input.Policy.MaxDataLatencyMinutes)

let demandQuantityRangeValidity: Rule<EvaluateInput> =
    Rule.create
        Rules.demandQuantityRangeValidity.Id
        Rules.demandQuantityRangeValidity.Explanation
        (fun input ->
            input.CurrentState |> Option.map(fun o -> (Quantity.value o.Quantity) >= 0m) |> Option.defaultValue false)
        (fun input ->
            let qty = input.CurrentState |> Option.map(fun o -> Quantity.value o.Quantity) |> Option.defaultValue 0m
            sprintf "Quantity: %M" qty)

let sourceReliabilityThreshold: Rule<EvaluateInput> =
    Rule.create
        Rules.sourceReliabilityThreshold.Id
        Rules.sourceReliabilityThreshold.Explanation
        (fun input -> input.SourceReliability >= input.Policy.MinSourceReliability)
        (fun input -> sprintf "Reliability: %M, Min: %M" input.SourceReliability input.Policy.MinSourceReliability)

let duplicateDataDetection: Rule<EvaluateInput> =
    Rule.create
        Rules.duplicateDataDetection.Id
        Rules.duplicateDataDetection.Explanation
        (fun input -> not input.HasDuplicateInWindow)
        (fun input -> sprintf "HasDuplicate: %b" input.HasDuplicateInWindow)

let receivedStatePrerequisite: Rule<EvaluateInput> =
    Rule.create
        Rules.receivedStatePrerequisite.Id
        Rules.receivedStatePrerequisite.Explanation
        (fun input ->
            input.CurrentState |> Option.map(fun o -> o.LifecycleState = Received) |> Option.defaultValue false)
        (fun input ->
            let state =
                input.CurrentState |> Option.map(fun o -> sprintf "%A" o.LifecycleState) |> Option.defaultValue "None"

            sprintf "CurrentState: %s" state)

let observationExistencePrerequisite: Rule<EvaluateInput> =
    Rule.create
        Rules.observationExistencePrerequisite.Id
        Rules.observationExistencePrerequisite.Explanation
        (fun input -> input.CurrentState.IsSome)
        (fun input -> sprintf "ObservationId: %s" (DemandObservationId.value input.Cmd.ObservationId))

let evaluationRules: Rule<EvaluateInput> list =
    [ observationExistencePrerequisite
      receivedStatePrerequisite
      demandSignalTimeliness
      demandQuantityRangeValidity
      sourceReliabilityThreshold
      duplicateDataDetection ]
