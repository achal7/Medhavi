/// SE-S-081 — Distribution Plan ACL
/// Traces to: Supply Intelligence Specification §4.3.1 (SE-S-081)
module Medhavi.Supply.ManageDistribution.DistributionPlan.ACL

open Medhavi.Common.Validation
open Medhavi.Contracts.Supply
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model

/// Translates external publish request to domain command
let toPublishCmd (req: PublishDistributionPlanReq) : Validation<PublishDistributionPlanCmd, DomainError> =
    (fun scopeId timestamp ->
        let planId = DistributionPlanId.ofScopeAndCycle scopeId req.PlanningCycle
        let reason =
            match req.PublicationReason with
            | PublicationReasonType.ScheduledCadence -> PublicationReason.ScheduledCadence
            | PublicationReasonType.MaterialChange -> PublicationReason.MaterialChange
            | PublicationReasonType.GovernanceOverride -> PublicationReason.GovernanceOverride
            | PublicationReasonType.EmergencyReplan -> PublicationReason.EmergencyReplan
        { PlanId = planId
          PlanningScopeId = scopeId
          PlanningCycle = req.PlanningCycle
          PublicationReason = reason
          Timestamp = timestamp })
    <!> validatePlanningScopeId req.PlanningScopeId
    <*> validateTimestamp req.Timestamp

/// Outbound event mapping to canonical ARS Enterprise Event (EV-S-071)
let toArsEventId (event: DistributionPlanEvent) : string =
    match event with
    | DistributionPlanPublished _ -> ArsIdentifiers.EnterpriseEvents.distributionPlanPublished.Id
