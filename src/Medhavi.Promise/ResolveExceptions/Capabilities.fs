/// CA-P-009 Resolve Exceptions — Parent Capabilities
module Medhavi.Promise.ResolveExceptions.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Contracts
open Medhavi.Contracts.Promise
open Medhavi.Promise.AclHelpers

type ResolveExceptionsApis =
    { Exception: PromiseExceptionApi }

let create
    (exceptionAggApi: PromiseExceptionAssessment.Capabilities.PromiseExceptionAggregateApi)
    : ResolveExceptionsApis =

    let raise (req: RaisePromiseExceptionReq) : Task<Result<PromiseExceptionAssessmentDto, ApiError>> =
        exceptionAggApi.RaiseException req
        |> TaskResult.map PromiseExceptionAssessment.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let resolve (req: ResolvePromiseExceptionReq) : Task<Result<PromiseExceptionAssessmentDto, ApiError>> =
        exceptionAggApi.ResolveException req
        |> TaskResult.map PromiseExceptionAssessment.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let exceptionApi: PromiseExceptionApi =
        { RaiseException = raise
          ResolveException = resolve }

    { Exception = exceptionApi }

