module Medhavi.Core.EnterprisePictureManagement.Projection

open Medhavi
open Medhavi.SemanticModel
open Model

type EnterprisePictureDto = Contracts.Core.EnterprisePicture

let mapVersion (v: PictureVersion) : Contracts.Core.PictureVersion =
    { VersionNumber = Identities.pictureVersionIdValue v.VersionNumber
      DemandReferences = v.DemandReferences |> List.map Identities.demandIdValue
      SupplyReferences = v.SupplyReferences |> List.map Identities.supplyIdValue
      InventoryReferences = v.InventoryReferences |> List.map Identities.InventoryIdentity.toString
      CompositionTime = Timestamp.value v.CompositionTime
      PublicationTime = v.PublicationTime |> Option.map Timestamp.value
      LifecycleState = v.LifecycleState.ToString() }

/// Map domain aggregate to contract DTO
let mapToDto (aggregate: EnterprisePicture) : Contracts.Core.EnterprisePicture =
    let currentPublished =
        aggregate.Versions
        |> List.tryFind(fun v -> v.LifecycleState = PictureVersionLifecycleState.Published)
        |> Option.map(fun v -> Identities.pictureVersionIdValue v.VersionNumber)

    { PlanningScopeId = Identities.planningScopeIdValue aggregate.PlanningScopeIdentifier
      Versions = aggregate.Versions |> List.map mapVersion
      CurrentPublishedVersion = currentPublished }

/// Projection agent state type
type State = Map<PlanningScopeId, Contracts.Core.EnterprisePicture>
let initial: State = Map.empty

/// Evolve projection based on events
let evolveProjection
    (state: Map<PlanningScopeId, Contracts.Core.EnterprisePicture>)
    (evt: EnterprisePictureEvent)
    : Map<PlanningScopeId, Contracts.Core.EnterprisePicture> =
    match evt with
    | PictureVersionComposed(scopeId, version) ->
        let scopeKey = Identities.planningScopeIdValue scopeId
        let versionDto = mapVersion version

        state
        |> Map.change scopeId (fun existing ->
            match existing with
            | Some dto ->
                Some
                    { dto with
                        Versions = versionDto :: dto.Versions }
            | None ->
                Some
                    { PlanningScopeId = scopeKey
                      Versions = [ versionDto ]
                      CurrentPublishedVersion = None })

    | PictureVersionPublished(scopeId, versionNumber, publicationTime) ->
        let scopeKey = Identities.planningScopeIdValue scopeId
        let versionNum = Identities.pictureVersionIdValue versionNumber

        state
        |> Map.change scopeId (fun existing ->
            match existing with
            | Some dto ->
                let updatedVersions =
                    dto.Versions
                    |> List.map(fun v ->
                        if v.VersionNumber = versionNum then
                            { v with
                                LifecycleState = PictureVersionLifecycleState.Published.ToString()
                                PublicationTime = Some(Timestamp.value publicationTime) }
                        else
                            v)

                Some
                    { dto with
                        Versions = updatedVersions
                        CurrentPublishedVersion = Some versionNum }
            | None -> None)

    | PictureVersionSuperseded(scopeId, versionNumber) ->
        let scopeKey = Identities.planningScopeIdValue scopeId
        let versionNum = Identities.pictureVersionIdValue versionNumber

        state
        |> Map.change scopeId (fun existing ->
            match existing with
            | Some dto ->
                let updatedVersions =
                    dto.Versions
                    |> List.map(fun v ->
                        if v.VersionNumber = versionNum then
                            { v with
                                LifecycleState = PictureVersionLifecycleState.Superseded.ToString() }
                        else
                            v)

                Some { dto with Versions = updatedVersions }
            | None -> None)

/// Seed the projection from existing aggregates
let seedFromAggregates (aggregates: EnterprisePicture list) : Map<PlanningScopeId, Contracts.Core.EnterprisePicture> =
    aggregates
    |> List.fold
        (fun state agg ->
            let dto = mapToDto agg
            //let scopeKey = Identities.planningScopeIdValue agg.PlanningScopeIdentifier
            Map.add agg.PlanningScopeIdentifier dto state)
        initial
