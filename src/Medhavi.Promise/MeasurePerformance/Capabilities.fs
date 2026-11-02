/// CA-P-010 Measure Promise Performance & Service Quality — Parent Capabilities
module Medhavi.Promise.MeasurePerformance.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Contracts
open Medhavi.Contracts.Promise
open Medhavi.Promise.AclHelpers
open Medhavi.Promise.MeasurePerformance.PromiseQualityAssessment

let createApi (aggregateApi: PromiseQualityAssessment.Capabilities.PromiseQualityAggregateApi) : PromiseQualityApi =
    { AuditQuality = fun req ->
        taskResult {
            let! assessment =
                aggregateApi.AuditQuality req
                |> TaskResult.mapError mapAppErrorToApiError
            return ACL.toDto assessment
        }
      RecordDispute = fun req ->
        taskResult {
            let! assessment =
                aggregateApi.RecordDispute req
                |> TaskResult.mapError mapAppErrorToApiError
            return ACL.toDto assessment
        } }

