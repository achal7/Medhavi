/// CA-D-004 — Segment Demand Parent Capability API
/// Maps child aggregate domain entities to public DTOs and dispatches BN-D-017
module Medhavi.Demand.SegmentDemand.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Contracts
open Medhavi.Contracts.Demand
open Medhavi.SharedKernel.BusinessNotifications
open Medhavi.Demand
open Medhavi.Demand.SegmentDemand.PlanningClassificationAssignment

let create
    (aggregateApi: Capabilities.AggregateApi)
    (dispatchEnvelope: Envelope -> Task<unit>)
    : PlanningClassificationApi =

    let classifyEntity (req: ClassifyPlanningEntityReq) : Task<Result<PlanningClassificationDto, ApiError>> =
        taskResult {
            let! domainAss = aggregateApi.Classify req |> TaskResult.mapError mapAppErrorToApiError
            let dto = Projections.mapToDto domainAss
            let lastChange = domainAss.ChangeEvents |> List.head

            let notif: PlanningClassificationChangedNotification =
                { AssignmentId = dto.AssignmentId
                  EntityType = dto.EntityType
                  EntityId = dto.EntityId
                  ClassificationType = dto.ClassificationType
                  PreviousClassification = lastChange.FromClassification |> Option.map(fun c -> c.AsString)
                  NewClassification = dto.CurrentClassification
                  ClassificationScore = dto.ClassificationScore
                  ClassificationConfidence = dto.ClassificationConfidence
                  Rationale = dto.AssignmentRationale
                  Timestamp = domainAss.LastUpdated }

            do!
                dispatchNotification
                    dispatchEnvelope
                    "BN-D-017"
                    "CA-D-004"
                    "PlanningClassificationAssignment"
                    dto.AssignmentId
                    notif

            return dto
        }

    let overrideClassification
        (req: OverridePlanningClassificationReq)
        : Task<Result<PlanningClassificationDto, ApiError>> =
        taskResult {
            let! domainAss = aggregateApi.Override req |> TaskResult.mapError mapAppErrorToApiError
            let dto = Projections.mapToDto domainAss
            let lastChange = domainAss.ChangeEvents |> List.head

            let notif: PlanningClassificationChangedNotification =
                { AssignmentId = dto.AssignmentId
                  EntityType = dto.EntityType
                  EntityId = dto.EntityId
                  ClassificationType = dto.ClassificationType
                  PreviousClassification = lastChange.FromClassification |> Option.map(fun c -> c.AsString)
                  NewClassification = dto.CurrentClassification
                  ClassificationScore = dto.ClassificationScore
                  ClassificationConfidence = dto.ClassificationConfidence
                  Rationale = dto.AssignmentRationale
                  Timestamp = domainAss.LastUpdated }

            do!
                dispatchNotification
                    dispatchEnvelope
                    "BN-D-017"
                    "CA-D-004"
                    "PlanningClassificationAssignment"
                    dto.AssignmentId
                    notif

            return dto
        }

    { ClassifyEntity = classifyEntity
      OverrideClassification = overrideClassification }
