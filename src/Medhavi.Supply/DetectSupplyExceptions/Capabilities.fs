/// CA-S-011 Detect Supply Exceptions — Capability Parent API
/// Traces to: CR-S-023, FS-S-100, BW-S-100, BN-S-100, BN-S-101, BN-S-102
module Medhavi.Supply.DetectSupplyExceptions.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Contracts
open Medhavi.Contracts.Supply
open Medhavi.Supply.AclHelpers
open Medhavi.Supply.DetectSupplyExceptions.SupplyExceptionAssessment.Capabilities
open Medhavi.Supply.DetectSupplyExceptions.SupplyExceptionAssessment.Projections

type DetectSupplyExceptionsApis = { Exceptions: SupplyExceptionApi }

let create
    (aggApi: SupplyExceptionAssessmentAggregateApi)
    : DetectSupplyExceptionsApis =

    let evaluateException (req: EvaluateSupplyExceptionReq) : Task<Result<SupplyExceptionAssessmentDto, ApiError>> =
        aggApi.Evaluate req
        |> TaskResult.map mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    { Exceptions = { EvaluateException = evaluateException } }
