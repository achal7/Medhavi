/// CA-S-009 — Sense Supply Changes Capability API
/// Traces to: CR-S-021, FS-S-085, BN-S-080
module Medhavi.Supply.SenseSupplyChanges.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Contracts
open Medhavi.Contracts.Supply
open Medhavi.Supply.AclHelpers
open SupplyChangeAssessment.Capabilities

type SenseSupplyChangesApis =
    {
        Assess: AssessSupplyChangeReq -> Task<Result<SupplyChangeAssessmentDto, ApiError>>
        Archive: ArchiveSupplyChangeReq -> Task<Result<SupplyChangeAssessmentDto, ApiError>>
    }

/// Pure factory for CA-S-009 Sense Supply Changes Capability API
let create
    (aggApi: SupplyChangeAssessmentAggregateApi)
    : SenseSupplyChangesApis =

    let handleAssess (req: AssessSupplyChangeReq) : Task<Result<SupplyChangeAssessmentDto, ApiError>> =
        aggApi.Assess req
        |> TaskResult.map SupplyChangeAssessment.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let handleArchive (req: ArchiveSupplyChangeReq) : Task<Result<SupplyChangeAssessmentDto, ApiError>> =
        aggApi.Archive req
        |> TaskResult.map SupplyChangeAssessment.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    {
        Assess = handleAssess
        Archive = handleArchive
    }
