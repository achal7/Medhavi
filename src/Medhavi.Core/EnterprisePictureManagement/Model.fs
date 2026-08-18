/// CA-C-019 Enterprise Picture Management Model
module Medhavi.Core.EnterprisePictureManagement.Model

open Medhavi.SemanticModel

/// AB-C-001 input
type ComposePictureVersionCmd =
    { PlanningScopeId: PlanningScopeId
      DemandReferences: DemandId list
      SupplyReferences: SupplyId list
      InventoryReferences: InventoryIdentity list
      CompositionTriggerTime: Timestamp }

/// AB-C-002 input
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

/// EV-C-001 carries CompositionTriggerTime (provenance), NOT stored on the entity.
/// EV-C-002 publication implicitly supersedes the prior Published version in evolve.
type EnterprisePictureEvent =
    | PictureVersionComposed of PlanningScopeId * PictureVersion * CompositionTriggerTime: Timestamp
    | PictureVersionPublished of PlanningScopeId * PictureVersionId * PublicationTime: Timestamp

/// Pure state evolution (catamorphism).
let evolve (state: EnterprisePicture option) (event: EnterprisePictureEvent) : EnterprisePicture option =
    match event with
    | PictureVersionComposed(scopeId, version, _) ->
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
            let updated =
                picture.Versions
                |> List.map(fun v ->
                    if v.VersionNumber = versionNumber then
                        { v with
                            LifecycleState = PictureVersionLifecycleState.Published
                            PublicationTime = Some publicationTime }
                    elif v.LifecycleState = PictureVersionLifecycleState.Published then
                        // Atomic supersede: prior Published becomes Superseded (no separate event)
                        { v with
                            LifecycleState = PictureVersionLifecycleState.Superseded }
                    else
                        v)

            { picture with Versions = updated })

let replay (events: EnterprisePictureEvent seq) : EnterprisePicture option = Seq.fold evolve None events
