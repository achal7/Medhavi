/// CA-P-011 Learn & Calibrate Promising Models — Parent Capabilities
module Medhavi.Promise.LearnFromPromises.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Contracts
open Medhavi.Contracts.Promise
open Medhavi.Promise.AclHelpers
open Medhavi.Promise.LearnFromPromises.PromiseCalibrationModel

let createApi (aggregateApi: PromiseCalibrationModel.Capabilities.PromiseCalibrationAggregateApi) : PromiseLearningApi =
    { ProposeCalibration = fun req ->
        taskResult {
            let! model =
                aggregateApi.ProposeCalibration req
                |> TaskResult.mapError mapAppErrorToApiError
            return Projections.toDto model
        }
      ApplyCalibration = fun req ->
        taskResult {
            let! model =
                aggregateApi.ApplyCalibration req
                |> TaskResult.mapError mapAppErrorToApiError
            return Projections.toDto model
        } }

