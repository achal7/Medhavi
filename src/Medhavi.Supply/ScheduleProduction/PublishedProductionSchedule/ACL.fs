/// SE-S-071 — Published Production Schedule Anti-Corruption Layer (ACL)
/// Traces to: Supply Intelligence Specification §4.3.1 (SE-S-071, FS-S-071, EV-S-061)
module Medhavi.Supply.ScheduleProduction.PublishedProductionSchedule.ACL

open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Model

let parsePublicationTriggerType (s: string) : PublicationTriggerType =
    match s.Trim().ToLowerInvariant() with
    | "scheduledpublication" -> PublicationTriggerType.ScheduledPublication
    | "emergencyreplan" -> PublicationTriggerType.EmergencyReplan
    | "governanceoverride" -> PublicationTriggerType.GovernanceOverride
    | "manualrelease" -> PublicationTriggerType.ManualRelease
    | "exceptiondriven" -> PublicationTriggerType.ExceptionDriven "Exception driven publication"
    | _ -> PublicationTriggerType.CadenceScheduled

/// Inbound ACL: translates external request into domain PublishProductionScheduleCmd using Applicative validation.
let toPublishCmd (req: PublishProductionScheduleReq) : Validation<PublishProductionScheduleCmd, DomainError> =
    let trigger = parsePublicationTriggerType req.TriggerType

    let create scope pid ts =
        {
            PlanningScopeId = scope
            PlanningPeriodId = pid
            TriggerType = trigger
            Timestamp = ts
        }

    create <!> validatePlanningScopeId req.PlanningScopeId
    <*> validatePlanningPeriodId req.PlanningPeriodId
    <*> validateTimestamp req.Timestamp

/// Outbound ACL: maps domain published baseline events to canonical ARS Enterprise Events (EV-S-061).
let toArsEventId =
    function
    | ProductionSchedulePublished _ -> ArsIdentifiers.EnterpriseEvents.productionSchedulePublished.Id
