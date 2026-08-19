module Medhavi.Core.EnterprisePictureManagement.ACL

open Medhavi.SemanticModel
open Medhavi.Common.Validation
open Medhavi.Contracts.Core
open Medhavi.Foundation.Failure
open Model
open Medhavi.Core

/// Validates and translates a ComposePictureVersionReq into a domain command.
let toComposeCmd (req: ComposePictureVersionReq) : Validation<ComposePictureVersionCmd, DomainError> =

    let validateDemandRefs =
        let results =
            req.DemandReferences
            |> List.map(fun id ->
                match DemandId.create id with
                | Ok demandId -> Valid demandId
                | Error err -> Invalid [ DomainError.validation(sprintf "DemandId: %A" err) ])

        // Accumulate all results
        let errors =
            results
            |> List.collect (function
                | Invalid errs -> errs
                | Valid _ -> [])

        let values =
            results
            |> List.choose (function
                | Valid v -> Some v
                | Invalid _ -> None)

        if errors.IsEmpty then Valid values else Invalid errors

    let validateSupplyRefs =
        let results =
            req.SupplyReferences
            |> List.map(fun id ->
                match SupplyId.create id with
                | Ok supplyId -> Valid supplyId
                | Error err -> Invalid [ DomainError.validation(sprintf "SupplyId: %A" err) ])

        let errors =
            results
            |> List.collect (function
                | Invalid errs -> errs
                | Valid _ -> [])

        let values =
            results
            |> List.choose (function
                | Valid v -> Some v
                | Invalid _ -> None)

        if errors.IsEmpty then Valid values else Invalid errors

    let validateInventoryReferences =
        req.InventoryReferences
        |> List.map(fun id ->
            match InventoryIdentity.parse id with
            | Ok invId -> Valid invId
            | Error err -> Invalid [ DomainError.validation(sprintf "InventoryId: %A" err) ])
        |> sequence

    // Applicative combination: accumulate ALL errors
    let create scopeId demandRefs supplyRefs compositionTime invReferences =
        { PlanningScopeId = scopeId
          DemandReferences = demandRefs
          SupplyReferences = supplyRefs
          InventoryReferences = invReferences
          CompositionTriggerTime = compositionTime }

    create <!> validateScopeId req.PlanningScopeId
    <*> validateDemandRefs
    <*> validateSupplyRefs
    <*> validateTimestamp req.CompositionTime
    <*> validateInventoryReferences

/// Validates and translates a PublishPictureVersionReq into a domain command.
let toPublishCmd (req: PublishPictureVersionReq) : Validation<PublishPictureVersionCmd, DomainError> =

    let validateVersionNumber =
        match PictureVersionId.create req.VersionNumber with
        | Ok id -> Valid id
        | Error err -> Invalid [ DomainError.validation(sprintf "VersionNumber: %A" err) ]

    let create scopeId versionNumber publicationTime =
        { PlanningScopeId = scopeId
          VersionNumber = versionNumber
          PublicationTime = publicationTime }

    create <!> validateScopeId req.PlanningScopeId <*> validateVersionNumber <*> validateTimestamp req.PublicationTime
