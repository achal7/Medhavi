/// SE-S-001 — Supply Understanding ACL
/// Traces to: FS-S-003 (Revise), FS-S-004 (Publish)
module Medhavi.Supply.UnderstandSupply.SupplyUnderstanding.ACL

open Medhavi.Common.Validation
open Medhavi.Contracts.Supply
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model

/// FS-S-003 — Translate the external request into the domain ReviseCmd.
let toReviseCmd (req: ReviseSupplyUnderstandingReq) : Validation<ReviseCmd, DomainError> =
    (fun scopeId ->
        { PlanningScopeId = scopeId
          PictureSupplyFacts =
            { PictureVersion = req.EvidencePictureVersion
              SupplyFacts = [] }
          SupplyChangeAssessments = []
          TransactionTime = Timestamp.now() })
    <!> validatePlanningScopeId req.PlanningScopeId

/// FS-S-004 — Translate the external request into the domain PublishCmd.
let toPublishCmd (req: PublishSupplyUnderstandingReq) : Validation<PublishCmd, DomainError> =
    (fun scopeId ->
        { PlanningScopeId = scopeId
          IsPeriodicRefresh = req.IsPeriodicRefresh
          PublicationTime = Timestamp.now() })
    <!> validatePlanningScopeId req.PlanningScopeId

/// Outbound ACL: maps domain understanding events to canonical ARS Enterprise Events (EV-S-003, EV-S-004).
let toArsEventId = function
    | SupplyUnderstandingRevised _ -> ArsIdentifiers.EnterpriseEvents.supplyUnderstandingRevised.Id
    | SupplyUnderstandingPublished _ -> ArsIdentifiers.EnterpriseEvents.supplyUnderstandingPublished.Id
