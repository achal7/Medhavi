module Medhavi.Core.DemandManagement.Rules

open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Core.ArsIdentifiers
open Model

type RecordInput =
    { Cmd: RecordDemandCmd
      CurrentState: Demand option
      AllowDuplicate: bool }

type SatisfyInput =
    { Cmd: SatisfyDemandCmd
      CurrentState: Demand option }

type CancelInput =
    { Cmd: CancelDemandCmd
      CurrentState: Demand option }

/// BR-C-019: Demand must not already exist (unless duplicates allowed)
let demandMustNotExist: Rule<RecordInput> =
    Rule.create
        Rules.demandMustNotExist.Id
        Rules.demandMustNotExist.Explanation
        (fun input -> input.CurrentState.IsNone || input.AllowDuplicate)
        (fun input -> sprintf "DemandId: %s, Exists: %b" (DemandId.value input.Cmd.DemandId) input.CurrentState.IsSome)

/// BR-C-020: Must be Active for Satisfaction
let demandMustBeActiveForSatisfaction: Rule<SatisfyInput> =
    Rule.create
        Rules.demandMustBeActiveForSatisfaction.Id
        Rules.demandMustBeActiveForSatisfaction.Explanation
        (fun input ->
            input.CurrentState
            |> Option.map(fun d -> d.LifecycleState = DemandLifecycleState.Active)
            |> Option.defaultValue false)
        (fun input ->
            let state =
                input.CurrentState |> Option.map(fun d -> sprintf "%A" d.LifecycleState) |> Option.defaultValue "None"

            sprintf "LifecycleState: %s" state)

/// BR-C-021: Must be Active for Cancellation
let demandMustBeActiveForCancellation: Rule<CancelInput> =
    Rule.create
        Rules.demandMustBeActiveForCancellation.Id
        Rules.demandMustBeActiveForCancellation.Explanation
        (fun input ->
            input.CurrentState
            |> Option.map(fun d -> d.LifecycleState = DemandLifecycleState.Active)
            |> Option.defaultValue false)
        (fun input ->
            let state =
                input.CurrentState |> Option.map(fun d -> sprintf "%A" d.LifecycleState) |> Option.defaultValue "None"

            sprintf "LifecycleState: %s" state)

/// BR-C-022: Must exist for Satisfaction
let demandMustExistForSatisfaction: Rule<SatisfyInput> =
    Rule.create
        Rules.demandMustExistForSatisfaction.Id
        Rules.demandMustExistForSatisfaction.Explanation
        (fun input -> input.CurrentState.IsSome)
        (fun input -> sprintf "DemandId: %s" (DemandId.value input.Cmd.DemandId))

/// BR-C-023: Must exist for Cancellation
let demandMustExistForCancellation: Rule<CancelInput> =
    Rule.create
        Rules.demandMustExistForCancellation.Id
        Rules.demandMustExistForCancellation.Explanation
        (fun input -> input.CurrentState.IsSome)
        (fun input -> sprintf "DemandId: %s" (DemandId.value input.Cmd.DemandId))

let recordingRules: Rule<RecordInput> list = [ demandMustNotExist ]
let satisfactionRules: Rule<SatisfyInput> list = [ demandMustExistForSatisfaction; demandMustBeActiveForSatisfaction ]
let cancellationRules: Rule<CancelInput> list = [ demandMustExistForCancellation; demandMustBeActiveForCancellation ]
