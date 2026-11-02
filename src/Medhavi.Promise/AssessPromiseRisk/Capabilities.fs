/// CA-P-007 Assess Promise Risk and Resilience — Parent Capabilities
module Medhavi.Promise.AssessPromiseRisk.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Contracts
open Medhavi.Contracts.Promise
open Medhavi.Promise.AclHelpers

type AssessPromiseRiskApis =
    { Risk: PromiseRiskApi }

let create
    (riskAggApi: PromiseRiskAssessment.Capabilities.PromiseRiskAggregateApi)
    : AssessPromiseRiskApis =

    let assess (req: AssessCommitmentRiskReq) : Task<Result<PromiseRiskAssessmentDto, ApiError>> =
        riskAggApi.Assess req
        |> TaskResult.map PromiseRiskAssessment.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let escalate (req: EscalateRiskBreachReq) : Task<Result<PromiseRiskAssessmentDto, ApiError>> =
        riskAggApi.Escalate req
        |> TaskResult.map PromiseRiskAssessment.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let riskApi: PromiseRiskApi =
        { Assess = assess
          Escalate = escalate }

    { Risk = riskApi }

