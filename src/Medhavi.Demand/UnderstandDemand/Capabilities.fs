/// CA-D-001 Understand Demand — Capability Parent API
/// Traces to: CR-D-001, CR-D-002, CR-D-003, CR-D-004, FS-D-001, FS-D-002, FS-D-003, FS-D-004
module Medhavi.Demand.UnderstandDemand.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Contracts
open Medhavi.Contracts.Demand
open Medhavi.SharedKernel.BusinessNotifications
open Medhavi.Demand
open Medhavi.Foundation.Failure

type UnderstandDemandApis =
    { Observations: DemandObservationApi
      Understanding: DemandUnderstandingApi }

let create
    (observationAggApi: DemandObservation.Capabilities.DemandObservationAggregateApi)
    (understandingAggApi: DemandUnderstanding.Capabilities.DemandUnderstandingAggregateApi)
    (dispatchEnvelope: Envelope -> Task<unit>)
    : UnderstandDemandApis =

    let receiveObservation (req: ReceiveObservationReq) : Task<Result<DemandObservationDto, ApiError>> =
        observationAggApi.Receive req
        |> TaskResult.map DemandObservation.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let evaluateObservation (req: EvaluateObservationReq) : Task<Result<DemandObservationDto, ApiError>> =
        taskResult {
            let! domainObs = observationAggApi.Evaluate req |> TaskResult.mapError mapAppErrorToApiError
            let dto = DemandObservation.Projections.mapToDto domainObs

            do!
                if domainObs.LifecycleState = DemandObservation.Model.ObservationLifecycleState.Accepted then
                    let notif: DemandObservationAcceptedNotification =
                        { ObservationId = dto.ObservationId
                          Item = domainObs.Item
                          Location = domainObs.Location
                          Quantity = domainObs.Quantity
                          BusinessTime = domainObs.BusinessTime
                          ObservationTime = domainObs.ObservationTime
                          ObservationType = domainObs.ObservationType
                          SourceSystemProvenance = domainObs.SourceSystemProvenance
                          Customer = None
                          Confidence = Some "High" }

                    dispatchNotification
                        dispatchEnvelope
                        "BN-D-006"
                        "CA-D-001"
                        "DemandObservation"
                        dto.ObservationId
                        notif
                else
                    TaskResult.return' ()

            return dto
        }

    let observationApi: DemandObservationApi =
        { Receive = receiveObservation
          Evaluate = evaluateObservation }

    let reviseUnderstanding (req: ReviseDemandUnderstandingReq) : Task<Result<DemandUnderstandingDto, ApiError>> =
        understandingAggApi.Revise req
        |> TaskResult.map DemandUnderstanding.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let publishUnderstanding (req: PublishDemandUnderstandingReq) : Task<Result<DemandUnderstandingDto, ApiError>> =
        taskResult {
            let! domainAgg = understandingAggApi.Publish req |> TaskResult.mapError mapAppErrorToApiError
            let dto = DemandUnderstanding.Projections.mapToDto domainAgg

            let! scopeId =
                validatePlanningScopeId dto.PlanningScopeId
                |> Medhavi.Common.Validation.toResult
                |> TaskResult.ofResult
                |> TaskResult.mapError(
                    DomainError.combineValidationErrors >> ApplicationError.fromDomainError >> mapAppErrorToApiError
                )

            let now = Timestamp.now()

            let notif: DemandUnderstandingPublishedNotification =
                { PlanningScopeId = scopeId
                  Version = dto.VersionNumber
                  EvidencePictureVersion = 1
                  PublicationTime = now
                  MaterialChangeDetected = true }

            do!
                dispatchNotification
                    dispatchEnvelope
                    "BN-D-010"
                    "CA-D-001"
                    "DemandUnderstanding"
                    dto.PlanningScopeId
                    notif

            return dto
        }

    let understandingApi: DemandUnderstandingApi =
        { Revise = reviseUnderstanding
          Publish = publishUnderstanding }

    { Observations = observationApi
      Understanding = understandingApi }
