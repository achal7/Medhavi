/// SE-D-002 — Demand Understanding ACL
/// Traces to: FS-D-003 (Revise), FS-D-004 (Publish)
module Medhavi.Demand.UnderstandDemand.DemandUnderstanding.ACL

open System
open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Model

/// FS-D-003 — Translate the external request into the domain ReviseCmd.
let toReviseCmd (req: ReviseDemandUnderstandingReq) : Validation<ReviseCmd, DomainError> =

    (fun scopeId ->
        { PlanningScopeId = scopeId
          PictureFacts =
            { PictureVersion = int req.EvidencePictureVersion
              DemandFacts = [] }
          // Transaction Time is stamped at the ACL boundary (imperative shell); the aggregate stays pure.
          TransactionTime = Timestamp.now() })
    <!> validatePlanningScopeId req.PlanningScopeId

/// FS-D-004 — Translate the external request into the domain PublishCmd.
let toPublishCmd (req: PublishDemandUnderstandingReq) : Validation<PublishCmd, DomainError> =
    (fun scopeId ->
        { PlanningScopeId = scopeId
          IsPeriodicRefresh = req.IsPeriodicRefresh
          // Publication Time is stamped at the ACL boundary (imperative shell).
          PublicationTime = Timestamp.now() })
    <!> validatePlanningScopeId req.PlanningScopeId

/// Outbound ACL: maps domain understanding events to canonical ARS Enterprise Events (EV-D-003, EV-D-004).
let toArsEventId = function
    | DemandUnderstandingRevised _ -> ArsIdentifiers.EnterpriseEvents.demandUnderstandingRevised.Id
    | DemandUnderstandingPublished _ -> ArsIdentifiers.EnterpriseEvents.demandUnderstandingPublished.Id
