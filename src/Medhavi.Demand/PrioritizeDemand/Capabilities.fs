/// CA-D-006 — Prioritize Demand Parent Capability API
module Medhavi.Demand.PrioritizeDemand.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.CommandCapabilities
open Medhavi.Contracts
open Medhavi.Contracts.Demand
open Medhavi.SharedKernel.BusinessNotifications
open Medhavi.Demand
open Medhavi.Demand.PrioritizeDemand.PlanningPriorityAssignment

let create
    (aggregateApi: Capabilities.AggregateApi)
    (ports: DemandPorts)
    (dispatchEnvelope: Envelope -> Task<unit>)
    : PlanningPriorityApi =

    let prioritizeEntity (req: PrioritizePlanningEntityReq) : Task<Result<PlanningPriorityDto, ApiError>> =
        taskResult {
            let! domainAss = aggregateApi.Prioritize req |> TaskResult.mapError mapAppErrorToApiError
            let dto = Projections.mapToDto domainAss
            let lastChange = domainAss.ChangeEvents |> List.head

            let notif: PlanningPriorityChangedNotification =
                { AssignmentId = dto.AssignmentId
                  EntityType = dto.EntityType
                  EntityId = dto.EntityId
                  PreviousPriority = lastChange.FromPriority |> Option.map (fun p -> p.AsString)
                  NewPriority = dto.CurrentPriority
                  PriorityScore = dto.PriorityScore
                  Rationale = dto.DecisionRationale
                  BusinessValidity = dto.BusinessValidity
                  Timestamp = domainAss.LastUpdated }

            do! dispatchNotification
                    dispatchEnvelope
                    "BN-D-020"
                    "CA-D-006"
                    "PlanningPriorityAssignment"
                    dto.AssignmentId
                    notif

            return dto
        }

    let overridePriority (req: OverridePlanningPriorityReq) : Task<Result<PlanningPriorityDto, ApiError>> =
        taskResult {
            let! domainAss = aggregateApi.Override req |> TaskResult.mapError mapAppErrorToApiError
            let dto = Projections.mapToDto domainAss
            let lastChange = domainAss.ChangeEvents |> List.head

            let notif: PlanningPriorityChangedNotification =
                { AssignmentId = dto.AssignmentId
                  EntityType = dto.EntityType
                  EntityId = dto.EntityId
                  PreviousPriority = lastChange.FromPriority |> Option.map (fun p -> p.AsString)
                  NewPriority = dto.CurrentPriority
                  PriorityScore = dto.PriorityScore
                  Rationale = dto.DecisionRationale
                  BusinessValidity = dto.BusinessValidity
                  Timestamp = domainAss.LastUpdated }

            do! dispatchNotification
                    dispatchEnvelope
                    "BN-D-020"
                    "CA-D-006"
                    "PlanningPriorityAssignment"
                    dto.AssignmentId
                    notif

            return dto
        }

    { PrioritizeEntity = prioritizeEntity
      OverridePriority = overridePriority }
