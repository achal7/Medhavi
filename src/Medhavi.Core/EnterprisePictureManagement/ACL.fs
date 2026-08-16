module Medhavi.Core.EnterprisePictureManagement.ACL

open Medhavi.SemanticModel
open Medhavi.Common.Validation
open Medhavi.Contracts.Core
open Medhavi.Foundation.Failure
open Model

/// Validates and translates a ComposePictureVersionReq into a domain command.
let toComposeCmd (req: ComposePictureVersionReq) : Validation<ComposePictureVersionCmd, DomainError> =

    let validateScopeId =
        match Identities.planningScopeIdCreate req.PlanningScopeId with
        | Ok id -> Valid id
        | Error err -> Invalid [ DomainError.validation(sprintf "PlanningScopeId: %A" err) ]

    let validateDemandRefs =
        let results =
            req.DemandReferences
            |> List.map(fun id ->
                match Identities.demandIdCreate id with
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
                match Identities.supplyIdCreate id with
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

    let validateCompositionTime =
        match Timestamp.create req.CompositionTime with
        | Ok ts -> Valid ts
        | Error err -> Invalid [ DomainError.validation(sprintf "CompositionTime: %s" err) ]

    let validateInventoryReferences =
        req.InventoryReferences
        |> List.map(fun id ->
            match Identities.InventoryIdentity.parse id with
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

    create <!> validateScopeId
    <*> validateDemandRefs
    <*> validateSupplyRefs
    <*> validateCompositionTime
    <*> validateInventoryReferences

/// Validates and translates a PublishPictureVersionReq into a domain command.
let toPublishCmd (req: PublishPictureVersionReq) : Validation<PublishPictureVersionCmd, DomainError> =

    let validateScopeId =
        match Identities.planningScopeIdCreate req.PlanningScopeId with
        | Ok id -> Valid id
        | Error err -> Invalid [ DomainError.validation(sprintf "PlanningScopeId: %A" err) ]

    let validateVersionNumber =
        match Identities.pictureVersionIdCreate req.VersionNumber with
        | Ok id -> Valid id
        | Error err -> Invalid [ DomainError.validation(sprintf "VersionNumber: %A" err) ]

    let validatePublicationTime =
        match Timestamp.create req.PublicationTime with
        | Ok ts -> Valid ts
        | Error err -> Invalid [ DomainError.validation(sprintf "PublicationTime: %s" err) ]

    let create scopeId versionNumber publicationTime =
        { PlanningScopeId = scopeId
          VersionNumber = versionNumber
          PublicationTime = publicationTime }

    create <!> validateScopeId <*> validateVersionNumber <*> validatePublicationTime
