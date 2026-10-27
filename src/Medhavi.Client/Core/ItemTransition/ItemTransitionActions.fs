module Medhavi.Client.Core.ItemTransition.ItemTransitionActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Core.ItemTransition
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ActionExecution
open Medhavi.Client
open Medhavi.Client.Core.ItemTransition

let private getApi (ctx: ActionExecutionContext) : ItemTransitionApi option =
    match ctx.Resolve<ItemTransitionApi>() with
    | Some api -> Some api
    | None -> ctx.Resolve<DomainServices>() |> Option.map(fun ds -> ds.Core.ItemTransitionManagement.Commands)

let private getQueries (ctx: ActionExecutionContext) : ItemTransitionQueries option =
    match ctx.Resolve<ItemTransitionQueries>() with
    | Some q -> Some q
    | None -> ctx.Resolve<DomainServices>() |> Option.map(fun ds -> ds.Core.ItemTransitionManagement.Queries)

let private getTransitionDto (ctx: ActionExecutionContext) (transitionId: string) : Task<ItemTransitionDto option> =
    task {
        match getQueries ctx with
        | Some q -> return! q.GetById transitionId
        | None -> return None
    }

/// Records a new supersession relationship between an old item and a superseding item.
let recordSupersession: ActionHandler =
    fun ctx ->
        task {
            match getApi ctx with
            | Some api ->
                match ctx.TargetKey with
                | Some transitionId ->
                    let! dtoOpt = getTransitionDto ctx transitionId

                    match dtoOpt with
                    | Some dto ->
                        let req: RecognizeItemTransitionReq =
                            {
                                SupersededItem = dto.SupersededItem
                                SupersedingItem = dto.SupersedingItem
                                TransitionType = "Supersession"
                                EffectiveDate = DateTimeOffset.Now
                                EndDate = None
                            }

                        match! api.Recognize req with
                        | Ok resDto ->
                            return
                                Ok(
                                    ActionExecutionOutcome.Completed(
                                        $"Item supersession '{resDto.TransitionId}' ({resDto.SupersededItem} -> {resDto.SupersedingItem}) recorded.",
                                        [ ItemTransitionStore.storeId; DataStoreId.literal "demo-demand-store" ]
                                    )
                                )
                        | Error err -> return Error err.Message
                    | None -> return Error $"Item transition '{transitionId}' not found in query projection."
                | None ->
                    // Global registration default
                    let req: RecognizeItemTransitionReq =
                        {
                            SupersededItem = "A100-BASE"
                            SupersedingItem = "H100-UPGRADE"
                            TransitionType = "Supersession"
                            EffectiveDate = DateTimeOffset.Now
                            EndDate = None
                        }

                    match! api.Recognize req with
                    | Ok resDto ->
                        return
                            Ok(
                                ActionExecutionOutcome.Completed(
                                    $"Item supersession '{resDto.TransitionId}' ({resDto.SupersededItem} -> {resDto.SupersedingItem}) recorded.",
                                    [ ItemTransitionStore.storeId; DataStoreId.literal "demo-demand-store" ]
                                )
                            )
                    | Error err -> return Error err.Message
            | None -> return Error "ItemTransitionApi capability is not connected."
        }

/// Activates phase-out lifecycle for an item transition.
let activatePhaseOut: ActionHandler =
    fun ctx ->
        task {
            match getApi ctx with
            | Some api ->
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
                                        [ ItemTransitionStore.storeId; DataStoreId.literal "demo-demand-store" ]
                                    )
                                )
                        | Error err -> return Error err.Message
                    | None -> return Error $"Item transition '{transitionId}' not found in query projection."
                | None -> return Error "No item transition selected to activate phase-out."
            | None -> return Error "ItemTransitionApi capability is not connected."
        }

/// Modifies the effective date of an existing item transition.
let modifyEffectiveDate: ActionHandler =
    fun ctx ->
        task {
            match getApi ctx with
            | Some api ->
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
                                        [ ItemTransitionStore.storeId ]
                                    )
                                )
                        | Error err -> return Error err.Message
                    | None -> return Error $"Item transition '{transitionId}' not found in query projection."
                | None -> return Error "No item transition selected to modify effective date."
            | None -> return Error "ItemTransitionApi capability is not connected."
        }

/// Suspends an active item transition.
let suspendTransition: ActionHandler =
    fun ctx ->
        task {
            match getApi ctx with
            | Some api ->
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
                                        [ ItemTransitionStore.storeId; DataStoreId.literal "demo-demand-store" ]
                                    )
                                )
                        | Error err -> return Error err.Message
                    | None -> return Error $"Item transition '{transitionId}' not found in query projection."
                | None -> return Error "No item transition selected to suspend."
            | None -> return Error "ItemTransitionApi capability is not connected."
        }

/// Reinstates a suspended item transition to active state.
let reinstateTransition: ActionHandler =
    fun ctx ->
        task {
            match getApi ctx with
            | Some api ->
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
                                        [ ItemTransitionStore.storeId; DataStoreId.literal "demo-demand-store" ]
                                    )
                                )
                        | Error err -> return Error err.Message
                    | None -> return Error $"Item transition '{transitionId}' not found in query projection."
                | None -> return Error "No item transition selected to reinstate."
            | None -> return Error "ItemTransitionApi capability is not connected."
        }

let all: (ActionDefId * ActionHandler) list =
    [
        ItemTransitionGridSchema.recordSupersession.Id, recordSupersession
        ItemTransitionGridSchema.activatePhaseOut.Id, activatePhaseOut
        ItemTransitionGridSchema.modifyEffectiveDate.Id, modifyEffectiveDate
        ItemTransitionGridSchema.suspend.Id, suspendTransition
        ItemTransitionGridSchema.reinstate.Id, reinstateTransition
    ]
