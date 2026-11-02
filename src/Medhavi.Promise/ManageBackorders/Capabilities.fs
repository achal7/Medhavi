/// CA-P-008 Manage Backorders — Parent Capabilities
module Medhavi.Promise.ManageBackorders.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Contracts
open Medhavi.Contracts.Promise
open Medhavi.Promise.AclHelpers

type ManageBackordersApis =
    { Backorders: BackorderApi }

let create
    (backorderAggApi: BackorderRecord.Capabilities.BackorderAggregateApi)
    : ManageBackordersApis =

    let enqueue (req: EnqueueBackorderReq) : Task<Result<BackorderRecordDto, ApiError>> =
        backorderAggApi.Enqueue req
        |> TaskResult.map BackorderRecord.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let resolve (req: ResolveBackorderReq) : Task<Result<BackorderRecordDto, ApiError>> =
        backorderAggApi.Resolve req
        |> TaskResult.map BackorderRecord.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let backorderApi: BackorderApi =
        { Enqueue = enqueue
          Resolve = resolve }

    { Backorders = backorderApi }

