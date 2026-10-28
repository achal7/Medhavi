module Medhavi.Client.Core.MasterData.MasterDataActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Core
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ActionExecution
open Medhavi.Client
open Medhavi.Client.Core.MasterData

let private getItemApi (ctx: ActionExecutionContext) : ItemApi option =
    ctx.Domain.Core.MasterData |> Option.bind (fun md -> md.ItemApi)

let private getLocationApi (ctx: ActionExecutionContext) : LocationApi option =
    ctx.Domain.Core.MasterData |> Option.bind (fun md -> md.LocationApi)

let private extractStringParam (key: string) (payload: ActionPayload) : string option =
    match payload with
    | CustomParameters p ->
        p
        |> Map.tryFind key
        |> Option.orElseWith (fun () ->
            p
            |> Map.toSeq
            |> Seq.tryFind (fun (k, _) -> String.Equals(k, key, StringComparison.OrdinalIgnoreCase))
            |> Option.map snd)
        |> Option.bind (function
            | ParameterValue.StringValue s when not (String.IsNullOrWhiteSpace s) -> Some (s.Trim())
            | _ -> None)
    | _ -> None

let private extractReason (payload: ActionPayload) : string option =
    match payload with
    | ReasonedAction(code, commentOpt) ->
        let comment = commentOpt |> Option.defaultValue ""
        Some ($"{code}: {comment}".TrimEnd())
    | CustomParameters _ ->
        extractStringParam "Reason" payload
        |> Option.orElseWith (fun () -> extractStringParam "reason" payload)
        |> Option.orElseWith (fun () -> extractStringParam "RetirementReason" payload)
    | _ -> None

/// Activates a master data item in the core ledger.
let activateItem: ActionHandler =
    fun ctx ->
        task {
            match getItemApi ctx with
            | Some api ->
                let targetId =
                    ctx.TargetKey
                    |> Option.orElseWith (fun () -> extractStringParam "Id" ctx.Payload)
                    |> Option.orElseWith (fun () -> extractStringParam "ItemId" ctx.Payload)

                match targetId with
                | Some itemId ->
                    let req: ActivateItemReq = { Id = itemId }
                    match! api.Activate req with
                    | Ok item ->
                        return
                            Ok(
                                ActionExecutionOutcome.Completed(
                                    $"Master data item '{item.Id}' ({item.Name}) activated.",
                                    [ MasterDataGrid.storeId; DataStoreId.literal "core-item-transition-store" ]
                                )
                            )
                    | Error err -> return Error err.Message
                | None ->
                    return Error "No master data item selected to activate."
            | None ->
                return Error "ItemApi capability is not connected."
        }

/// Permanently retires a master data item with governed justification evidence.
let retireItem: ActionHandler =
    fun ctx ->
        task {
            match getItemApi ctx with
            | Some api ->
                let targetId =
                    ctx.TargetKey
                    |> Option.orElseWith (fun () -> extractStringParam "Id" ctx.Payload)
                    |> Option.orElseWith (fun () -> extractStringParam "ItemId" ctx.Payload)

                match targetId with
                | Some itemId ->
                    match extractReason ctx.Payload with
                    | Some reason ->
                        let req: RetireItemReq = { Id = itemId; Reason = reason }
                        match! api.Retire req with
                        | Ok item ->
                            return
                                Ok(
                                    ActionExecutionOutcome.Completed(
                                        $"Master data item '{item.Id}' ({item.Name}) retired. Reason: {reason}",
                                        [ MasterDataGrid.storeId; DataStoreId.literal "core-item-transition-store" ]
                                    )
                                )
                        | Error err -> return Error err.Message
                    | None ->
                        return Error "Required parameter 'Reason' was not provided in action payload."
                | None ->
                    return Error "No master data item selected to retire."
            | None ->
                return Error "ItemApi capability is not connected."
        }

/// Activates a master data location in the core network ledger.
let activateLocation: ActionHandler =
    fun ctx ->
        task {
            match getLocationApi ctx with
            | Some api ->
                let targetId =
                    ctx.TargetKey
                    |> Option.orElseWith (fun () -> extractStringParam "Id" ctx.Payload)
                    |> Option.orElseWith (fun () -> extractStringParam "LocationId" ctx.Payload)

                match targetId with
                | Some locId ->
                    let req: ActivateLocationReq = { Id = locId }
                    match! api.Activate req with
                    | Ok loc ->
                        return
                            Ok(
                                ActionExecutionOutcome.Completed(
                                    $"Master data location '{loc.Id}' ({loc.Name}) activated.",
                                    [ MasterDataGrid.storeId ]
                                )
                            )
                    | Error err -> return Error err.Message
                | None ->
                    return Error "No master data location selected to activate."
            | None ->
                return Error "LocationApi capability is not connected."
        }

let all: (ActionDefId * ActionHandler) list =
    [ MasterDataGrid.MasterDataActions.activateItem.Id, activateItem
      MasterDataGrid.MasterDataActions.retireItem.Id, retireItem
      MasterDataGrid.MasterDataActions.activateLocation.Id, activateLocation ]
