/// CA-S-001 Understand Supply — Capability Parent API
/// Traces to: CR-S-001, CR-S-002, CR-S-003, CR-S-004, FS-S-001, FS-S-002, FS-S-003, FS-S-004
module Medhavi.Supply.UnderstandSupply.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Contracts
open Medhavi.Contracts.Supply
open Medhavi.Supply.AclHelpers

type UnderstandSupplyApis =
    {
        Records: SupplyDataRecordApi
        Understanding: SupplyUnderstandingApi
    }

let create
    (recordAggApi: SupplyDataRecord.Capabilities.SupplyDataRecordAggregateApi)
    (understandingAggApi: SupplyUnderstanding.Capabilities.SupplyUnderstandingAggregateApi)
    : UnderstandSupplyApis =

    let receiveRecord (req: ReceiveSupplyDataReq) : Task<Result<SupplyDataRecordDto, ApiError>> =
        recordAggApi.Receive req
        |> TaskResult.map SupplyDataRecord.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let evaluateRecord (req: EvaluateSupplyDataReq) : Task<Result<SupplyDataRecordDto, ApiError>> =
        recordAggApi.Evaluate req
        |> TaskResult.map SupplyDataRecord.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let recordApi: SupplyDataRecordApi =
        {
            Receive = receiveRecord
            Evaluate = evaluateRecord
        }

    let reviseUnderstanding (req: ReviseSupplyUnderstandingReq) : Task<Result<SupplyUnderstandingDto, ApiError>> =
        understandingAggApi.Revise req
        |> TaskResult.map SupplyUnderstanding.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let publishUnderstanding (req: PublishSupplyUnderstandingReq) : Task<Result<SupplyUnderstandingDto, ApiError>> =
        understandingAggApi.Publish req
        |> TaskResult.map SupplyUnderstanding.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let understandingApi: SupplyUnderstandingApi =
        {
            Revise = reviseUnderstanding
            Publish = publishUnderstanding
        }

    {
        Records = recordApi
        Understanding = understandingApi
    }
