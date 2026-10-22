/// CA-S-004 Manage Capacity — Capability Parent API
/// Traces to: CR-S-011, CR-S-012, FS-S-030, FS-S-031, BW-S-030, BW-S-031
module Medhavi.Supply.ManageCapacity.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Contracts
open Medhavi.Contracts.Supply
open Medhavi.Supply.AclHelpers

let create
    (positionAggApi: CapacityPosition.Capabilities.CapacityPositionAggregateApi)
    (healthAggApi: CapacityHealth.Capabilities.CapacityHealthAggregateApi)
    : ManageCapacityApis =

    let updatePosition (req: UpdateCapacityPositionReq) : Task<Result<CapacityPositionAssessmentDto, ApiError>> =
        positionAggApi.Update req
        |> TaskResult.map CapacityPosition.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let publishHealth (req: PublishCapacityHealthReq) : Task<Result<CapacityHealthAssessmentDto, ApiError>> =
        healthAggApi.Publish req
        |> TaskResult.map CapacityHealth.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    {
        Position = { Update = updatePosition }
        Health = { Publish = publishHealth }
    }
