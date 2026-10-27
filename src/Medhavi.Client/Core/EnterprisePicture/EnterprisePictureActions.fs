module Medhavi.Client.Core.EnterprisePicture.EnterprisePictureActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Core
open Medhavi.Contracts.Core.Demand
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ActionExecution
open Medhavi.Client
open Medhavi.Client.Core.EnterprisePicture

let private getApi (ctx: ActionExecutionContext) : EnterprisePictureApi option =
    ctx.Resolve<EnterprisePictureApi>()

let private parseTargetKey (key: string) : string * int =
    if key.Contains("#") then
        let parts = key.Split('#')
        let ver = match Int32.TryParse parts.[1] with true, v -> v | _ -> 1
        parts.[0], ver
    else
        let ver = match Int32.TryParse key with true, v -> v | _ -> 1
        "GLOBAL", ver

/// Freezes and publishes an Enterprise Picture Version as the authoritative planning baseline.
let freezeBaseline: ActionHandler =
    fun ctx ->
        task {
            match getApi ctx with
            | Some api ->
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
                                    [ EnterprisePictureStore.storeId
                                      DataStoreId.literal "demo-demand-store"
                                      DataStoreId.literal "core-exception-store" ]
                                )
                            )
                    | Error err -> return Error err.Message
                | None -> return Error "No picture version selected to freeze and publish."
            | None -> return Error "EnterprisePictureApi capability is not connected."
        }

/// Composes and refreshes a new draft version of the Enterprise Picture.
let refreshPicture: ActionHandler =
    fun ctx ->
        task {
            match getApi ctx with
            | Some api ->
                let scopeId =
                    match ctx.TargetKey with
                    | Some key when key.Contains("#") -> (key.Split('#')).[0]
                    | _ -> "GLOBAL"

                let! demandRefs =
                    task {
                        match ctx.Resolve<DemandQueries>() with
                        | Some dq ->
                            let! dtos = dq.GetAll()
                            return dtos |> List.map (fun d -> d.Id)
                        | None ->
                            match ctx.Resolve<DomainServices>() with
                            | Some ds ->
                                let! dtos = ds.Core.DemandManagement.Queries.GetAll()
                                return dtos |> List.map (fun d -> d.Id)
                            | None ->
                                return []
                    }

                let req: ComposePictureVersionReq =
                    { PlanningScopeId = scopeId
                      DemandReferences = demandRefs
                      SupplyReferences = []
                      InventoryReferences = []
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
                                $"New Draft Enterprise Picture version {draftVer} composed for scope '{pic.PlanningScopeId}'.",
                                [ EnterprisePictureStore.storeId ]
                            )
                        )
                | Error err -> return Error err.Message
            | None -> return Error "EnterprisePictureApi capability is not connected."
        }

let all: (ActionDefId * ActionHandler) list =
    [ EnterprisePictureGridSchema.freezeBaseline.Id, freezeBaseline
      EnterprisePictureGridSchema.refreshPicture.Id, refreshPicture
      EnterprisePictureGridSchema.publish.Id, freezeBaseline
      EnterprisePictureGridSchema.compose.Id, refreshPicture ]
