/// CA-S-010 — Evaluate Supply Quality Capability API
/// Traces to: CR-S-022, FS-S-090, BN-S-090
module Medhavi.Supply.EvaluateSupplyQuality.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Contracts
open Medhavi.Contracts.Supply
open Medhavi.Supply.AclHelpers
open SupplyQualityAssessment.Capabilities

type EvaluateSupplyQualityApis =
    {
        Evaluate: EvaluateSupplyQualityReq -> Task<Result<SupplyQualityAssessmentDto, ApiError>>
    }

let create
    (aggApi: SupplyQualityAssessmentAggregateApi)
    : EvaluateSupplyQualityApis =

    let evaluate (req: EvaluateSupplyQualityReq) : Task<Result<SupplyQualityAssessmentDto, ApiError>> =
        taskResult {
            let! domainAgg = aggApi.Evaluate req |> TaskResult.mapError mapAppErrorToApiError

            match SupplyQualityAssessment.Projections.mapToDto domainAgg with
            | Some dto ->
                return dto
            | None ->
                return! TaskResult.fail(ApiError.infrastructureError "Failed to map SupplyQualityAssessment to DTO")
        }

    { Evaluate = evaluate }
