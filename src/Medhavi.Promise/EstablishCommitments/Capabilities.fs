/// CA-P-006 Establish Commitments Parent Capabilities
module Medhavi.Promise.EstablishCommitments.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Contracts
open Medhavi.Contracts.Promise
open Medhavi.Promise.AclHelpers

type EstablishCommitmentsApis =
    { Commitment: CommitmentApi
      Explanation: PromiseExplanationApi }

let create
    (commitmentAggApi: OrderCommitment.Capabilities.CommitmentAggregateApi)
    (explanationAggApi: PromiseExplanation.Capabilities.PromiseExplanationAggregateApi)
    : EstablishCommitmentsApis =

    let establish (req: EstablishCommitmentReq) : Task<Result<OrderCommitmentDto, ApiError>> =
        commitmentAggApi.Establish req
        |> TaskResult.map OrderCommitment.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let revise (req: ReviseCommitmentReq) : Task<Result<OrderCommitmentDto, ApiError>> =
        commitmentAggApi.Revise req
        |> TaskResult.map OrderCommitment.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let confirmFulfillment (req: ConfirmFulfillmentReq) : Task<Result<OrderCommitmentDto, ApiError>> =
        commitmentAggApi.ConfirmFulfillment req
        |> TaskResult.map OrderCommitment.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let cancel (req: CancelCommitmentReq) : Task<Result<OrderCommitmentDto, ApiError>> =
        commitmentAggApi.Cancel req
        |> TaskResult.map OrderCommitment.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let commitmentApi: CommitmentApi =
        { Establish = establish
          Revise = revise
          ConfirmFulfillment = confirmFulfillment
          Cancel = cancel }

    let generateExplanation (req: GeneratePromiseExplanationReq) : Task<Result<PromiseExplanationDto, ApiError>> =
        explanationAggApi.Generate req
        |> TaskResult.map PromiseExplanation.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let explanationApi: PromiseExplanationApi =
        { GenerateExplanation = generateExplanation }

    { Commitment = commitmentApi
      Explanation = explanationApi }

