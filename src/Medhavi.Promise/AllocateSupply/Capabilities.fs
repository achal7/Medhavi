/// CA-P-002 Allocate Supply and Manage Priority Parent API
module Medhavi.Promise.AllocateSupply.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Contracts
open Medhavi.Contracts.Promise
open Medhavi.Promise.AclHelpers

type AllocateSupplyApis =
    { Allocation: AllocationApi }

let create (allocationAggApi: AllocationPool.Capabilities.AllocationAggregateApi) : AllocateSupplyApis =

    let initPool (req: InitializeAllocationPoolReq) : Task<Result<AllocationPoolDto, ApiError>> =
        allocationAggApi.Initialize req
        |> TaskResult.map AllocationPool.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let drawQuota (req: DrawAllocationQuotaReq) : Task<Result<AllocationPoolDto, ApiError>> =
        allocationAggApi.DrawQuota req
        |> TaskResult.map AllocationPool.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let rebalance (req: RebalancePoolsReq) : Task<Result<AllocationPoolDto, ApiError>> =
        allocationAggApi.Rebalance req
        |> TaskResult.map AllocationPool.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let calcPriority (req: CalculateDynamicPriorityReq) : Task<Result<DynamicOrderPriorityDto, ApiError>> =
        allocationAggApi.CalculatePriority req
        |> TaskResult.map AllocationPool.Projections.mapPriorityToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let api: AllocationApi =
        { InitializePool = initPool
          DrawQuota = drawQuota
          Rebalance = rebalance
          CalculatePriority = calcPriority }

    { Allocation = api }

