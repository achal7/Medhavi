/// CA-C-020 Anti-Corruption Layer
module Medhavi.Core.ExceptionManagement.ACL

open Medhavi.SemanticModel
open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.Foundation.IdsFactory
open Medhavi.Contracts.Core.Exception
open Model

/// BR-C-007: ExceptionId is deterministically derived from business identity.
/// Same violation always routes to the same aggregate stream -> natural deduplication.
let deriveExceptionId (constraintRef: string) (scopeType: string) (scopeId: string) : string =
    generalDeterministicId "exception" [ constraintRef; scopeType; scopeId ]

let private invalid (message: string) : Validation<'T, DomainError> = Invalid [ DomainError.validation message ]

/// Translates detection evidence into a domain command. No external ExceptionId accepted.
let toProcessEvidenceCmd (req: ExceptionEvidenceReq) : Validation<ProcessExceptionEvidenceCmd, DomainError> =
    let validateExceptionId =
        let derived = deriveExceptionId req.ConstraintReference req.AffectedScopeType req.AffectedScopeIdentifier

        match ExceptionId.create derived with
        | Ok id -> Valid id
        | Error err -> invalid(sprintf "ExceptionId: %A" err)

    let validateClassification =
        match VocabularyEntryId.create req.Classification with
        | Ok id -> Valid id
        | Error err -> invalid(sprintf "Classification: %A" err)

    let validateScopeType =
        match VocabularyEntryId.create req.AffectedScopeType with
        | Ok id -> Valid id
        | Error err -> invalid(sprintf "AffectedScopeType: %A" err)

    let validateSeverity =
        match req.Severity with
        | None -> Valid None
        | Some s ->
            match VocabularyEntryId.create s with
            | Ok id -> Valid(Some id)
            | Error err -> invalid(sprintf "Severity: %A" err)

    let validateEvidenceTime =
        match Timestamp.create req.EvidenceTime with
        | Ok ts -> Valid ts
        | Error err -> invalid(sprintf "EvidenceTime: %s" err)

    let create id classification scopeType severity evidenceTime =
        { ExceptionId = id
          ConstraintReference = req.ConstraintReference
          Classification = classification
          AffectedScopeType = scopeType
          AffectedScopeIdentifier = req.AffectedScopeIdentifier
          EvidenceReference = req.EvidenceReference
          Severity = severity
          EvidenceTime = evidenceTime }

    create <!> validateExceptionId
    <*> validateClassification
    <*> validateScopeType
    <*> validateSeverity
    <*> validateEvidenceTime

let toResolveCmd (req: ResolveExceptionReq) : Validation<ResolveExceptionCmd, DomainError> =
    let validateExceptionId =
        let derived = deriveExceptionId req.ConstraintReference req.AffectedScopeType req.AffectedScopeIdentifier

        match ExceptionId.create derived with
        | Ok id -> Valid id
        | Error err -> invalid(sprintf "ExceptionId: %A" err)

    let validateResolutionTime =
        match Timestamp.create req.ResolutionTime with
        | Ok ts -> Valid ts
        | Error err -> invalid(sprintf "ResolutionTime: %s" err)

    let create id time =
        { ExceptionId = id
          ResolutionTime = time
          ResolutionEvidence = req.ResolutionEvidence }

    create <!> validateExceptionId <*> validateResolutionTime
