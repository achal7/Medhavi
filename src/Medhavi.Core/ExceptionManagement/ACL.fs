/// CA-C-020 Exception Management Anti-Corruption Layer
module Medhavi.Core.ExceptionManagement.ACL

open Medhavi.SemanticModel
open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.Contracts.Core.Exception
open Model

let private invalidMessage (message: string) : Validation<'T, DomainError> = Invalid [ DomainError.validation message ]

/// Translates external registration request to domain command
let toRegisterCmd (req: RegisterExceptionReq) : Validation<RegisterExceptionCmd, DomainError> =
    let validateExceptionId =
        match Identities.exceptionIdCreate req.ExceptionId with
        | Ok id -> Valid id
        | Error err -> invalidMessage(sprintf "ExceptionId: %A" err)

    let validateClassification =
        match Identities.vocabularyEntryIdCreate req.Classification with
        | Ok id -> Valid id
        | Error err -> invalidMessage(sprintf "Classification: %A" err)

    let validateAffectedScopeType =
        match Identities.vocabularyEntryIdCreate req.AffectedScopeType with
        | Ok id -> Valid id
        | Error err -> invalidMessage(sprintf "AffectedScopeType: %A" err)

    let validateSeverity =
        match req.Severity with
        | None -> Valid None
        | Some s ->
            match Identities.vocabularyEntryIdCreate s with
            | Ok id -> Valid(Some id)
            | Error err -> invalidMessage(sprintf "Severity: %A" err)

    let validateRegistrationTime =
        match Timestamp.create req.RegistrationTime with
        | Ok ts -> Valid ts
        | Error err -> invalidMessage(sprintf "RegistrationTime: %s" err)

    let create id classif scopeType sev time =
        { ExceptionId = id
          ConstraintReference = req.ConstraintReference
          Classification = classif
          AffectedScopeType = scopeType
          AffectedScopeIdentifier = req.AffectedScopeIdentifier
          EvidenceReference = req.EvidenceReference
          Severity = sev
          RegistrationTime = time }

    create <!> validateExceptionId
    <*> validateClassification
    <*> validateAffectedScopeType
    <*> validateSeverity
    <*> validateRegistrationTime

/// Translates external resolution request to domain command
let toResolveCmd (req: ResolveExceptionReq) : Validation<ResolveExceptionCmd, DomainError> =
    let validateExceptionId =
        match Identities.exceptionIdCreate req.ExceptionId with
        | Ok id -> Valid id
        | Error err -> invalidMessage(sprintf "ExceptionId: %A" err)

    let validateResolutionTime =
        match Timestamp.create req.ResolutionTime with
        | Ok ts -> Valid ts
        | Error err -> invalidMessage(sprintf "ResolutionTime: %s" err)

    let create id time =
        { ExceptionId = id
          ResolutionTime = time
          ResolutionEvidence = req.ResolutionEvidence }

    create <!> validateExceptionId <*> validateResolutionTime
