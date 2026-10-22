module Medhavi.Supply.UnderstandSupply.SupplyDataRecord.Rules

open System
open Medhavi.SemanticModel
open Medhavi.Foundation.Contracts
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers
open Model
open Policies

type EvaluateInput =
    { Cmd: EvaluateRecordCmd
      CurrentState: SupplyDataRecord option
      Policy: SupplyDataAcceptancePolicy
      SourceReliability: decimal
      HasDuplicateInWindow: bool }

/// Precondition: Record must exist
let recordExistencePrerequisite: Rule<EvaluateInput> =
    Rule.create
        Rules.supplyDataRecordIdentity.Id
        Rules.supplyDataRecordIdentity.Explanation
        (fun input -> input.CurrentState.IsSome)
        (fun input -> $"RecordId: %s{SupplyDataRecordId.value input.Cmd.RecordId}")

/// BR-S-101: Supply data record must be in Received state for evaluation
let receivedStatePrerequisite: Rule<EvaluateInput> =
    Rule.create
        Rules.supplyDataEvaluationEligibility.Id
        Rules.supplyDataEvaluationEligibility.Explanation
        (fun input ->
            input.CurrentState |> Option.map(fun r -> r.LifecycleState = Received) |> Option.defaultValue false)
        (fun input ->
            let state =
                input.CurrentState |> Option.map(fun r -> $"%A{r.LifecycleState}") |> Option.defaultValue "None"
            $"CurrentState: %s{state}")

/// BR-S-010: Supply data timestamp must be within maximum allowed latency
let supplyDataTimeliness: Rule<EvaluateInput> =
    Rule.create
        Rules.supplyDataTimeliness.Id
        Rules.supplyDataTimeliness.Explanation
        (fun input ->
            match input.CurrentState with
            | Some r ->
                let latency = (Timestamp.value r.ObservationTime) - (Timestamp.value r.BusinessTime)
                latency <= input.Policy.MaximumDataLatency
            | None -> false)
        (fun input ->
            let latency =
                input.CurrentState
                |> Option.map(fun r -> (Timestamp.value r.ObservationTime) - (Timestamp.value r.BusinessTime))
                |> Option.defaultValue TimeSpan.Zero
            $"Latency: %f{latency.TotalMinutes} mins, Max: %f{input.Policy.MaximumDataLatency.TotalMinutes} mins")

/// BR-S-011: Supply quantities must be positive and within valid range
let supplyQuantityRangeValidity: Rule<EvaluateInput> =
    Rule.create
        Rules.supplyQuantityRangeValidity.Id
        Rules.supplyQuantityRangeValidity.Explanation
        (fun input ->
            input.CurrentState
            |> Option.map(fun r ->
                let q = Quantity.value r.Quantity
                q > 0m && q <= input.Policy.MaxAllowedQuantity)
            |> Option.defaultValue false)
        (fun input ->
            let qty = input.CurrentState |> Option.map(fun r -> Quantity.value r.Quantity) |> Option.defaultValue 0m
            $"Quantity: %M{qty}, MaxAllowed: %M{input.Policy.MaxAllowedQuantity}")

/// BR-S-012: Source reliability must meet minimum acceptance threshold
let sourceReliabilityThreshold: Rule<EvaluateInput> =
    Rule.create
        Rules.sourceReliabilityThreshold.Id
        Rules.sourceReliabilityThreshold.Explanation
        (fun input -> input.SourceReliability >= input.Policy.MinimumSourceReliability)
        (fun input -> $"Reliability: %M{input.SourceReliability}, Min: %M{input.Policy.MinimumSourceReliability}")

/// BR-S-013: Duplicate data within duplicate detection window is rejected
let duplicateDataDetection: Rule<EvaluateInput> =
    Rule.create
        Rules.duplicateDataDetection.Id
        Rules.duplicateDataDetection.Explanation
        (fun input -> not input.HasDuplicateInWindow)
        (fun input -> $"HasDuplicate: %b{input.HasDuplicateInWindow}")

let evaluationRules: Rule<EvaluateInput> list =
    [ recordExistencePrerequisite
      receivedStatePrerequisite
      supplyDataTimeliness
      supplyQuantityRangeValidity
      sourceReliabilityThreshold
      duplicateDataDetection ]
