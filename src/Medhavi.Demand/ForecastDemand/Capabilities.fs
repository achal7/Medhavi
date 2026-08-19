/// CA-D-002 Forecast Demand — Capability Parent API
/// Traces to: CA-D-002, FS-D-005, FS-D-006, FS-D-007, FS-D-008
module Medhavi.Demand.ForecastDemand.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.CommandCapabilities
open Medhavi.SemanticModel
open Medhavi.Contracts
open Medhavi.Contracts.Demand
open Medhavi.SharedKernel.BusinessNotifications
open Medhavi.Demand
open Medhavi.Demand.ForecastDemand.ForecastPublication
open Medhavi.Foundation.Failure

let create
    (aggregateApi: Capabilities.AggregateApi)
    (dispatchEnvelope: Envelope -> Task<unit>)
    : ForecastPublicationApi =

    let initiateCycle (req: InitiateForecastCycleReq) : Task<Result<ForecastPublicationDto, ApiError>> =
        aggregateApi.InitiateCycle req
        |> TaskResult.map Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let selectChampionModel (req: SelectChampionModelReq) : Task<Result<ForecastPublicationDto, ApiError>> =
        aggregateApi.SelectChampionModel req
        |> TaskResult.map Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let produceProjection (req: ProduceForecastProjectionReq) : Task<Result<ForecastPublicationDto, ApiError>> =
        aggregateApi.ProduceProjection req
        |> TaskResult.map Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let applyOverride (req: ApplyPlannerOverrideReq) : Task<Result<ForecastPublicationDto, ApiError>> =
        taskResult {
            let! dto =
                aggregateApi.ApplyOverride req
                |> TaskResult.map Projections.mapToDto
                |> TaskResult.mapError mapAppErrorToApiError

            let overrideDto = dto.Overrides |> List.head

            let! item =
                ItemId.create req.ItemId
                |> Result.mapError(
                    mapSemanticValidationToDomainError >> ApplicationError.fromDomainError >> mapAppErrorToApiError
                )
                |> TaskResult.ofResult

            let! location =
                LocationId.create req.LocationId
                |> Result.mapError(
                    mapSemanticValidationToDomainError >> ApplicationError.fromDomainError >> mapAppErrorToApiError
                )
                |> TaskResult.ofResult

            let! bucketStart =
                Timestamp.create req.BucketStart
                |> Result.mapError(
                    mapSemanticValidationToDomainError >> ApplicationError.fromDomainError >> mapAppErrorToApiError
                )
                |> TaskResult.ofResult

            let! overrideTs =
                Timestamp.create overrideDto.Timestamp
                |> Result.mapError(
                    mapSemanticValidationToDomainError >> ApplicationError.fromDomainError >> mapAppErrorToApiError
                )
                |> TaskResult.ofResult

            let overrideNotif: ForecastOverrideAppliedNotification =
                { PublicationId = dto.PublicationId
                  ItemId = item
                  LocationId = location
                  BucketStart = bucketStart
                  OriginalValue = overrideDto.OriginalValue
                  OverrideValue = overrideDto.OverrideValue
                  PlannerId = req.PlannerId
                  Justification = req.Justification
                  Timestamp = overrideTs }

            do!
                dispatchNotification
                    dispatchEnvelope
                    "BN-D-012"
                    "CA-D-002"
                    "ForecastPublication"
                    dto.PublicationId
                    overrideNotif

            return dto
        }

    let publish (req: PublishForecastPublicationReq) : Task<Result<ForecastPublicationDto, ApiError>> =
        taskResult {
            let! fpub = aggregateApi.Publish req |> TaskResult.mapError mapAppErrorToApiError
            let dto = Projections.mapToDto fpub

            let pubTime =
                fpub.Versions |> List.head |> (fun v -> v.PublicationTime |> Option.defaultValue fpub.HorizonStart)

            let pubNotif: ForecastPublishedNotification =
                { PublicationId = dto.PublicationId
                  PlanningScopeId = fpub.PlanningScope
                  VersionNumber = dto.VersionNumber
                  HorizonStart = fpub.HorizonStart
                  HorizonEnd = fpub.HorizonEnd
                  PublicationTime = pubTime
                  LineCount = dto.Lines.Length
                  ChampionModelId = dto.ChampionModelId }

            do!
                dispatchNotification
                    dispatchEnvelope
                    "BN-D-011"
                    "CA-D-002"
                    "ForecastPublication"
                    dto.PublicationId
                    pubNotif

            return dto
        }

    { InitiateCycle = initiateCycle
      SelectChampionModel = selectChampionModel
      ProduceProjection = produceProjection
      ApplyOverride = applyOverride
      Publish = publish }
