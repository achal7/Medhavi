/// SE-S-070 — Production Schedule Anti-Corruption Layer (ACL)
/// Traces to: Supply Intelligence Specification §4.3.1 (SE-S-070, FS-S-070, EV-S-060)
module Medhavi.Supply.ScheduleProduction.ProductionSchedule.ACL

open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Model

let mapScheduleType (t: Medhavi.Contracts.Supply.ScheduleType) : Medhavi.Supply.ScheduleType =
    match t with
    | Medhavi.Contracts.Supply.ScheduleType.Primary -> Medhavi.Supply.ScheduleType.Primary
    | Medhavi.Contracts.Supply.ScheduleType.Recovery -> Medhavi.Supply.ScheduleType.Recovery
    | Medhavi.Contracts.Supply.ScheduleType.Frozen -> Medhavi.Supply.ScheduleType.Frozen
    | Medhavi.Contracts.Supply.ScheduleType.Scenario -> Medhavi.Supply.ScheduleType.Scenario

/// Inbound ACL: translates external request into domain CreateProductionScheduleCmd using Applicative validation.
let toCreateCmd (req: CreateProductionScheduleReq) : Validation<CreateProductionScheduleCmd, DomainError> =
    let schedType = mapScheduleType req.ScheduleType

    let create rid pid ts =
        {
            ResourceId = rid
            PlanningPeriodId = pid
            ScheduleType = schedType
            SourceSupplyPlanVersion = 1
            SchedulingPolicyVersion = 1
            Timestamp = ts
        }

    create <!> validatePhysicalResourceId req.ResourceId
    <*> validatePlanningPeriodId req.PlanningPeriodId
    <*> validateTimestamp req.Timestamp

/// Outbound ACL: maps domain production schedule events to canonical ARS Enterprise Events (EV-S-060).
let toArsEventId =
    function
    | ProductionScheduleActivated _ -> ArsIdentifiers.EnterpriseEvents.productionScheduleActivated.Id
    | ProductionScheduleArchived _ -> ArsIdentifiers.EnterpriseEvents.productionScheduleActivated.Id
