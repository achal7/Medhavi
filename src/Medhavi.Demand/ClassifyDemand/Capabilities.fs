/// CA-D-005 — Classify Demand Parent Capability API
/// Maps child aggregate domain entities to public DTOs and dispatches BN-D-019
module Medhavi.Demand.ClassifyDemand.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Contracts
open Medhavi.Contracts.Demand
open Medhavi.SharedKernel.BusinessNotifications
open Medhavi.Demand
open Medhavi.Demand.ClassifyDemand.DemandBehaviorAssignment

let create
    (aggregateApi: Capabilities.AggregateApi)
    (dispatchEnvelope: Envelope -> Task<unit>)
    : DemandBehaviorClassificationApi =

    let classifyBehavior (req: ClassifyDemandBehaviorReq) : Task<Result<DemandBehaviorAssignmentDto, ApiError>> =
        taskResult {
            let! domainAss = aggregateApi.Classify req |> TaskResult.mapError mapAppErrorToApiError
            let dto = Projections.mapToDto domainAss
            let lastChange = domainAss.ChangeEvents |> List.head

            let notif: DemandBehaviorClassificationChangedNotification =
                { AssignmentId = dto.AssignmentId
                  Item = domainAss.Item
                  Location = domainAss.Location
                  Dimension = dto.Dimension
                  PreviousClassification = lastChange.FromClassification |> Option.map(fun c -> c.AsString)
                  NewClassification = dto.CurrentClassification
                  ClassificationConfidence = dto.ClassificationConfidence
                  Rationale = dto.AssignmentRationale
                  Timestamp = domainAss.LastUpdated }

            do!
                dispatchNotification
                    dispatchEnvelope
                    "BN-D-019"
                    "CA-D-005"
                    "DemandBehaviorAssignment"
                    dto.AssignmentId
                    notif

            return dto
        }

    let overrideBehavior (req: OverrideDemandBehaviorReq) : Task<Result<DemandBehaviorAssignmentDto, ApiError>> =
        taskResult {
            let! domainAss = aggregateApi.Override req |> TaskResult.mapError mapAppErrorToApiError
            let dto = Projections.mapToDto domainAss
            let lastChange = domainAss.ChangeEvents |> List.head

            let notif: DemandBehaviorClassificationChangedNotification =
                { AssignmentId = dto.AssignmentId
                  Item = domainAss.Item
                  Location = domainAss.Location
                  Dimension = dto.Dimension
                  PreviousClassification = lastChange.FromClassification |> Option.map(fun c -> c.AsString)
                  NewClassification = dto.CurrentClassification
                  ClassificationConfidence = dto.ClassificationConfidence
                  Rationale = dto.AssignmentRationale
                  Timestamp = domainAss.LastUpdated }

            do!
                dispatchNotification
                    dispatchEnvelope
                    "BN-D-019"
                    "CA-D-005"
                    "DemandBehaviorAssignment"
                    dto.AssignmentId
                    notif

            return dto
        }

    { ClassifyBehavior = classifyBehavior
      OverrideBehavior = overrideBehavior }
