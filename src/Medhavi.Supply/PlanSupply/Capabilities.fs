/// CA-S-002 Plan Supply — Capability Parent API
/// Traces to: CR-S-005, CR-S-006, CR-S-007, FS-S-010, FS-S-011, FS-S-012, BW-S-010, BW-S-011, BW-S-012
module Medhavi.Supply.PlanSupply.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Contracts
open Medhavi.Contracts.Supply
open Medhavi.Supply.AclHelpers

type PlanSupplyApis =
    { Plan: SupplyPlanApi }

let create
    (planAggApi: SupplyPlan.Capabilities.SupplyPlanAggregateApi)
    : PlanSupplyApis =

    let generatePlan (req: GenerateSupplyPlanReq) : Task<Result<SupplyPlanDto, ApiError>> =
        planAggApi.Generate req
        |> TaskResult.map SupplyPlan.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let evaluatePlan (req: EvaluateSupplyPlanReq) : Task<Result<SupplyPlanDto, ApiError>> =
        planAggApi.Evaluate req
        |> TaskResult.map SupplyPlan.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let publishPlan (req: PublishSupplyPlanReq) : Task<Result<SupplyPlanDto, ApiError>> =
        planAggApi.Publish req
        |> TaskResult.map SupplyPlan.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let planApi: SupplyPlanApi =
        { Generate = generatePlan
          Evaluate = evaluatePlan
          Publish = publishPlan }

    { Plan = planApi }
