module Medhavi.Client.Core.ItemTransition.ItemTransitionActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Core.ItemTransition
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ActionExecution
open Medhavi.Client
open Medhavi.Client.Core.ItemTransition

let private getTransitionDto (ctx: ActionExecutionContext) (transitionId: string) : Task<ItemTransitionDto option> =
    ctx.Domain.Core.ItemTransitionManagement.Queries.GetById transitionId

let private extractItemParams (payload: ActionPayload) =
    match payload with
    | CustomParameters p ->
        let findKey (key: string) =
            p
            |> Map.tryFind key
            |> Option.orElseWith (fun () ->
                p
                |> Map.toSeq
                |> Seq.tryFind (fun (k, _) -> String.Equals(k, key, StringComparison.OrdinalIgnoreCase))
                |> Option.map snd)
            |> Option.bind (function
                | StringValue s when not (String.IsNullOrWhiteSpace s) -> Some (s.Trim())
                | _ -> None)

        match findKey "SupersededItem", findKey "SupersedingItem" with
        | Some superseded, Some superseding -> Some (superseded, superseding)
        | _ -> None
    | _ -> None

let private extractEffectiveDate (payload: ActionPayload) =
    match payload with
    | CustomParameters p ->
        p
        |> Map.tryFind "EffectiveDate"
        |> Option.bind (function DateTimeValue d -> Some d | _ -> None)
        |> Option.defaultValue DateTimeOffset.Now
    | _ -> DateTimeOffset.Now

let private extractEndDate (payload: ActionPayload) =
    match payload with
    | CustomParameters p ->
        p
        |> Map.tryFind "EndDate"
        |> Option.bind (function DateTimeValue d -> Some d | _ -> None)
    | _ -> None

/// Records a new supersession relationship between an old item and a superseding item.
let recordSupersession: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Core.ItemTransitionManagement.Commands
            let! itemsOpt =
                task {
                    match extractItemParams ctx.Payload with
                    | Some (superseded, superseding) ->
                        return Some (superseded, superseding)
                    | None ->
                        match ctx.TargetKey with
                        | Some transitionId ->
                            let! dtoOpt = getTransitionDto ctx transitionId
                            return dtoOpt |> Option.map (fun d -> d.SupersededItem, d.SupersedingItem)
                        | None ->
                            return None
                }

            match itemsOpt with
            | Some (superseded, superseding) ->
                let req: RecognizeItemTransitionReq =
                    {
                        SupersededItem = superseded
                        SupersedingItem = superseding
                        TransitionType = "Supersession"
                        EffectiveDate = extractEffectiveDate ctx.Payload
                        EndDate = extractEndDate ctx.Payload
                    }

                match! api.Recognize req with
                | Ok resDto ->
                    return
                        Ok(
                            ActionExecutionOutcome.Completed(
                                $"Item supersession '{resDto.TransitionId}' ({resDto.SupersededItem} -> {resDto.SupersedingItem}) recorded.",
                                [ ItemTransitionGrid.storeId; DataStoreId.literal "demo-demand-store" ]
                            )
                        )
                | Error err -> return Error err.Message
            | None ->
                return Error "Required item transition parameters (SupersededItem, SupersedingItem) were not provided in action payload."
        }

/// Activates phase-out lifecycle for an item transition.
let activatePhaseOut: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Core.ItemTransitionManagement.Commands
            match ctx.TargetKey with
            | Some transitionId ->
                let! dtoOpt = getTransitionDto ctx transitionId

                match dtoOpt with
                | Some dto ->
                    let req: RecognizeItemTransitionReq =
                        {
                            SupersededItem = dto.SupersededItem
                            SupersedingItem = dto.SupersedingItem
                            TransitionType = "PhaseOut"
                            EffectiveDate = dto.EffectiveDate
                            EndDate = dto.EndDate |> Option.orElse(Some(DateTimeOffset.Now.AddMonths 3))
                        }

                    match! api.Recognize req with
                    | Ok resDto ->
                        return
                            Ok(
                                ActionExecutionOutcome.Completed(
                                    $"Phase-out activated for transition '{resDto.TransitionId}' ({resDto.SupersededItem} -> {resDto.SupersedingItem}).",
                                    [ ItemTransitionGrid.storeId; DataStoreId.literal "demo-demand-store" ]
                                )
                            )
                    | Error err -> return Error err.Message
                | None -> return Error $"Item transition '{transitionId}' not found in query projection."
            | None -> return Error "No item transition selected to activate phase-out."
        }

/// Modifies the effective date of an existing item transition.
let modifyEffectiveDate: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Core.ItemTransitionManagement.Commands
            match ctx.TargetKey with
            | Some transitionId ->
                let! dtoOpt = getTransitionDto ctx transitionId

                match dtoOpt with
                | Some dto ->
                    let req: RecognizeItemTransitionReq =
                        {
                            SupersededItem = dto.SupersededItem
                            SupersedingItem = dto.SupersedingItem
                            TransitionType = dto.TransitionType
                            EffectiveDate = DateTimeOffset.Now
                            EndDate = dto.EndDate
                        }

                    match! api.Recognize req with
                    | Ok resDto ->
                        let dateStr = resDto.EffectiveDate.ToString("yyyy-MM-dd")

                        return
                            Ok(
                                ActionExecutionOutcome.Completed(
                                    $"Effective date for transition '{resDto.TransitionId}' updated to {dateStr}.",
                                    [ ItemTransitionGrid.storeId ]
                                )
                            )
                    | Error err -> return Error err.Message
                | None -> return Error $"Item transition '{transitionId}' not found in query projection."
            | None -> return Error "No item transition selected to modify effective date."
        }

/// Suspends an active item transition.
let suspendTransition: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Core.ItemTransitionManagement.Commands
            match ctx.TargetKey with
            | Some transitionId ->
                let! dtoOpt = getTransitionDto ctx transitionId

                match dtoOpt with
                | Some dto ->
                    let req: SuspendItemTransitionReq =
                        {
                            SupersededItem = dto.SupersededItem
                            SupersedingItem = dto.SupersedingItem
                            SuspensionTime = DateTimeOffset.Now
                        }

                    match! api.Suspend req with
                    | Ok resDto ->
                        return
                            Ok(
                                ActionExecutionOutcome.Completed(
                                    $"Item transition '{resDto.TransitionId}' suspended.",
                                    [ ItemTransitionGrid.storeId; DataStoreId.literal "demo-demand-store" ]
                                )
                            )
                    | Error err -> return Error err.Message
                | None -> return Error $"Item transition '{transitionId}' not found in query projection."
            | None -> return Error "No item transition selected to suspend."
        }

/// Reinstates a suspended item transition to active state.
let reinstateTransition: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Core.ItemTransitionManagement.Commands
            match ctx.TargetKey with
            | Some transitionId ->
                let! dtoOpt = getTransitionDto ctx transitionId

                match dtoOpt with
                | Some dto ->
                    let req: ReinstateItemTransitionReq =
                        {
                            SupersededItem = dto.SupersededItem
                            SupersedingItem = dto.SupersedingItem
                            ReinstatementTime = DateTimeOffset.Now
                        }

                    match! api.Reinstate req with
                    | Ok resDto ->
                        return
                            Ok(
                                ActionExecutionOutcome.Completed(
                                    $"Item transition '{resDto.TransitionId}' reinstated to active.",
                                    [ ItemTransitionGrid.storeId; DataStoreId.literal "demo-demand-store" ]
                                )
                            )
                    | Error err -> return Error err.Message
                | None -> return Error $"Item transition '{transitionId}' not found in query projection."
            | None -> return Error "No item transition selected to reinstate."
        }

/// Permanently retires an item transition.
let retireTransition: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Core.ItemTransitionManagement.Commands
            match ctx.TargetKey with
            | Some transitionId ->
                let! dtoOpt = getTransitionDto ctx transitionId

                match dtoOpt with
                | Some dto ->
                    let req: RetireItemTransitionReq =
                        {
                            SupersededItem = dto.SupersededItem
                            SupersedingItem = dto.SupersedingItem
                            RetirementTime = DateTimeOffset.Now
                        }

                    match! api.Retire req with
                    | Ok resDto ->
                        return
                            Ok(
                                ActionExecutionOutcome.Completed(
                                    $"Item transition '{resDto.TransitionId}' retired.",
                                    [ ItemTransitionGrid.storeId; DataStoreId.literal "demo-demand-store" ]
                                )
                            )
                    | Error err -> return Error err.Message
                | None -> return Error $"Item transition '{transitionId}' not found in query projection."
            | None -> return Error "No item transition selected to retire."
        }

let all: (ActionDefId * ActionHandler) list =
    [
        ItemTransitionGrid.ItemTransitionActions.recordSupersession.Id, recordSupersession
        ItemTransitionGrid.ItemTransitionActions.activatePhaseOut.Id, activatePhaseOut
        ItemTransitionGrid.ItemTransitionActions.modifyEffectiveDate.Id, modifyEffectiveDate
        ItemTransitionGrid.ItemTransitionActions.suspend.Id, suspendTransition
        ItemTransitionGrid.ItemTransitionActions.reinstate.Id, reinstateTransition
        ItemTransitionGrid.ItemTransitionActions.retire.Id, retireTransition
    ]
