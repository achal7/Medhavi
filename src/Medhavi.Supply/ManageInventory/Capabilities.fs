/// CA-S-003 Manage Inventory — Capability Parent API
/// Traces to: CR-S-008, CR-S-009, CR-S-010, FS-S-020, FS-S-021, FS-S-022, BW-S-020, BW-S-021, BW-S-022
module Medhavi.Supply.ManageInventory.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Contracts
open Medhavi.Contracts.Supply
open Medhavi.Supply.AclHelpers

let create
    (positionAggApi: InventoryPosition.Capabilities.InventoryPositionAggregateApi)
    (policyAggApi: InventoryPolicy.Capabilities.InventoryPolicyAggregateApi)
    (healthAggApi: InventoryHealth.Capabilities.InventoryHealthAggregateApi)
    : ManageInventoryApis =

    let updatePosition (req: UpdateInventoryPositionReq) : Task<Result<InventoryPositionAssessmentDto, ApiError>> =
        positionAggApi.Update req
        |> TaskResult.map InventoryPosition.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let updatePolicy (req: UpdateInventoryPolicyReq) : Task<Result<InventoryPolicyAssignmentDto, ApiError>> =
        policyAggApi.Update req
        |> TaskResult.map InventoryPolicy.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let publishHealth (req: PublishInventoryHealthReq) : Task<Result<InventoryHealthAssessmentDto, ApiError>> =
        healthAggApi.Publish req
        |> TaskResult.map InventoryHealth.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    {
        Position = { Update = updatePosition }
        Policy = { Update = updatePolicy }
        Health = { Publish = publishHealth }
    }
