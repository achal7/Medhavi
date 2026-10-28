module Medhavi.Client.Core.EnterprisePicture.EnterprisePictureActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Core
open Medhavi.Contracts.Core.Demand
open Medhavi.Contracts.Supply
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ActionExecution
open Medhavi.Client
open Medhavi.Client.Core.EnterprisePicture

let private parseTargetKey (key: string) : string * int =
    if key.Contains("#") then
        let parts = key.Split('#')
        let ver = match Int32.TryParse parts.[1] with true, v -> v | _ -> 1
        parts.[0], ver
    else
        let ver = match Int32.TryParse key with true, v -> v | _ -> 1
        "PS-GLOBAL-SUPPLY-CHAIN", ver

let private resolveScopeId (ctx: ActionExecutionContext) (queries: EnterprisePictureQueries) : Task<string> =
    task {
        match ctx.TargetKey with
        | Some key when key.Contains("#") ->
            return (key.Split('#')).[0]
        | Some key when not (String.IsNullOrWhiteSpace key) && not (fst (Int32.TryParse key)) ->
            return key
        | _ ->
            // Use governed default planning scope from ctx.PlanningContext
            match ctx.PlanningContext.PlantId with
            | Some plant when not (String.IsNullOrWhiteSpace plant) ->
                return $"PS-{plant}"
            | _ ->
                let! all = queries.GetAll()
                match all with
                | first :: _ -> return first.PlanningScopeId
                | [] -> return "PS-GLOBAL-SUPPLY-CHAIN"
    }

/// Freezes and publishes an Enterprise Picture Version as the authoritative planning baseline.
let freezeBaseline: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Core.EnterprisePicture.Commands
            match ctx.TargetKey with
            | Some key ->
                let scopeId, versionNum = parseTargetKey key

                let req: PublishPictureVersionReq =
                    { PlanningScopeId = scopeId
                      VersionNumber = versionNum
                      PublicationTime = DateTimeOffset.Now }

                match! api.Publish req with
                | Ok pic ->
                    return
                        Ok(
                            ActionExecutionOutcome.Completed(
                                $"Enterprise Picture version {versionNum} frozen and published for scope '{pic.PlanningScopeId}'.",
                                [ EnterprisePictureGrid.storeId
                                  DataStoreId.literal "demo-demand-store"
                                  DataStoreId.literal "core-exception-store" ]
                            )
                        )
                | Error err -> return Error err.Message
            | None -> return Error "No picture version selected to freeze and publish."
        }

/// Composes and refreshes a new draft version of the Enterprise Picture.
let refreshPicture: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Core.EnterprisePicture.Commands
            let queries = ctx.Domain.Core.EnterprisePicture.Queries
            let! scopeId = resolveScopeId ctx queries

            // Query current published version to forward existing references
            let! existingPicOpt = queries.GetById scopeId

            let publishedVersionOpt =
                existingPicOpt
                |> Option.bind (fun pic ->
                    match pic.CurrentPublishedVersion with
                    | Some verNum ->
                        pic.Versions |> List.tryFind (fun v -> v.VersionNumber = verNum)
                    | None ->
                        pic.Versions
                        |> List.tryFind (fun v -> v.LifecycleState = "Published")
                        |> Option.orElseWith (fun () -> pic.Versions |> List.tryLast))

            let forwardedSupplyRefs =
                publishedVersionOpt
                |> Option.map (fun v -> v.SupplyReferences)
                |> Option.defaultValue []

            let forwardedInventoryRefs =
                publishedVersionOpt
                |> Option.map (fun v -> v.InventoryReferences)
                |> Option.defaultValue []

            let publishedDemandRefs =
                publishedVersionOpt
                |> Option.map (fun v -> v.DemandReferences)
                |> Option.defaultValue []

            let! liveDemandRefs =
                task {
                    let! dtos = ctx.Domain.Core.DemandManagement.Queries.GetAll()
                    return dtos |> List.map (fun d -> d.Id)
                }

            let demandRefs =
                if not (List.isEmpty liveDemandRefs) then
                    liveDemandRefs
                else
                    publishedDemandRefs

            let! inventoryRefs =
                task {
                    if not (List.isEmpty forwardedInventoryRefs) then
                        return forwardedInventoryRefs
                    else
                        let! items = ctx.Domain.Supply.ManageInventory.InventoryPositionQueries.GetAll()
                        return items |> List.map (fun i -> $"{i.ItemId}@{i.LocationId}")
                }

            let! supplyRefs =
                task {
                    if not (List.isEmpty forwardedSupplyRefs) then
                        return forwardedSupplyRefs
                    else
                        let! plans = ctx.Domain.Supply.PlanSupply.SupplyPlanQueries.GetAll()
                        return plans |> List.map (fun p -> p.PlanningScopeId)
                }

            let req: ComposePictureVersionReq =
                { PlanningScopeId = scopeId
                  DemandReferences = demandRefs
                  SupplyReferences = supplyRefs
                  InventoryReferences = inventoryRefs
                  CompositionTime = DateTimeOffset.Now }

            match! api.Compose req with
            | Ok pic ->
                let draftVer =
                    pic.Versions
                    |> List.tryLast
                    |> Option.map (fun v -> v.VersionNumber)
                    |> Option.defaultValue 1

                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"New Draft Enterprise Picture version {draftVer} composed for scope '{pic.PlanningScopeId}' ({demandRefs.Length} demands, {supplyRefs.Length} supply, {inventoryRefs.Length} inventory).",
                            [ EnterprisePictureGrid.storeId ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let all: (ActionDefId * ActionHandler) list =
    [ EnterprisePictureGrid.EnterprisePictureActions.freezeBaseline.Id, freezeBaseline
      EnterprisePictureGrid.EnterprisePictureActions.refreshPicture.Id, refreshPicture
      EnterprisePictureGrid.EnterprisePictureActions.publish.Id, freezeBaseline
      EnterprisePictureGrid.EnterprisePictureActions.compose.Id, refreshPicture ]
