/// CA-S-008 — Manage Distribution Capability Parent API
/// Traces to: CR-S-019, CR-S-020, FS-S-080, FS-S-081, BN-S-070, BN-S-071
module Medhavi.Supply.ManageDistribution.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Contracts
open Medhavi.Contracts.Supply
open Medhavi.Supply.AclHelpers

let create
    (recAggApi: DistributionRecommendation.Capabilities.DistributionRecommendationAggregateApi)
    (planAggApi: DistributionPlan.Capabilities.DistributionPlanAggregateApi)
    : ManageDistributionApis =

    let handleCreateRec
        (req: CreateDistributionRecommendationReq)
        : Task<Result<DistributionRecommendationDto, ApiError>> =
        recAggApi.Create req
        |> TaskResult.map DistributionRecommendation.Projections.toDto
        |> TaskResult.mapError mapAppErrorToApiError

    let handlePublishPlan (req: PublishDistributionPlanReq) : Task<Result<DistributionPlanDto, ApiError>> =
        planAggApi.Publish req
        |> TaskResult.map DistributionPlan.Projections.toDto
        |> TaskResult.mapError mapAppErrorToApiError

    {
        Recommendation = { Create = handleCreateRec }
        Plan = { Publish = handlePublishPlan }
    }
