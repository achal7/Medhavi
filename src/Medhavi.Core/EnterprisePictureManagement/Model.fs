/// CA-C-019 Enterprise Picture Management Model
module Medhavi.Core.EnterprisePictureManagement.Model

open Medhavi.SemanticModel

/// AB-C-019a: Compose a new Enterprise Picture Version
type ComposePictureVersionCmd =
    { PlanningScopeId: PlanningScopeId
      DemandReferences: DemandId list
      SupplyReferences: SupplyId list
      InventoryReferences: InventoryIdentity list
      CompositionTime: Timestamp }

/// AB-C-019b: Publish an Enterprise Picture Version
type PublishPictureVersionCmd =
    { PlanningScopeId: PlanningScopeId
      VersionNumber: PictureVersionId
      PublicationTime: Timestamp }

type EnterprisePictureCmd =
    | Compose of ComposePictureVersionCmd
    | Publish of PublishPictureVersionCmd

    member this.PlanningScopeId =
        match this with
        | Compose c -> c.PlanningScopeId
        | Publish c -> c.PlanningScopeId

type EnterprisePictureEvent =
    | PictureVersionComposed of PlanningScopeId * PictureVersion
    | PictureVersionPublished of PlanningScopeId * PictureVersionId * PublicationTime: Timestamp
    | PictureVersionSuperseded of PlanningScopeId * PictureVersionId

/// Layer E: Pure state evolution (Catamorphism)
let evolve (state: EnterprisePicture option) (event: EnterprisePictureEvent) : EnterprisePicture option =
    match event with
    | PictureVersionComposed(scopeId, version) ->
        match state with
        | Some picture ->
            Some
                { picture with
                    Versions = picture.Versions @ [ version ] }
        | None ->
            Some
                { PlanningScopeIdentifier = scopeId
                  Versions = [ version ] }

    | PictureVersionPublished(_, versionNumber, publicationTime) ->
        state
        |> Option.map(fun picture ->
            let updatedVersions =
                picture.Versions
                |> List.map(fun v ->
                    if v.VersionNumber = versionNumber then
                        { v with
                            LifecycleState = PictureVersionLifecycleState.Published
                            PublicationTime = Some publicationTime }
                    else
                        v)

            { picture with
                Versions = updatedVersions })

    | PictureVersionSuperseded(_, versionNumber) ->
        state
        |> Option.map(fun picture ->
            let updatedVersions =
                picture.Versions
                |> List.map(fun v ->
                    if v.VersionNumber = versionNumber then
                        { v with
                            LifecycleState = PictureVersionLifecycleState.Superseded }
                    else
                        v)

            { picture with
                Versions = updatedVersions })

let replay (events: EnterprisePictureEvent seq) : EnterprisePicture option = Seq.fold evolve None events
