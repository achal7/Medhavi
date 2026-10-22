/// Anti-Corruption Layer (ACL) for Procurement Recommendation Commands
/// Translates Medhavi.Contracts.Supply DTOs into pure domain commands using applicative validation
module Medhavi.Supply.ProcureMaterials.ProcurementRecommendation.ACL

open System
open Medhavi.Common.Validation
open Medhavi.Contracts.Supply
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model

let validateRequirementId (reqId: string) : Validation<ProcurementRequirementId, DomainError> =
    ProcurementRequirementId.create reqId |> fromResult

let toCreateRecommendationCmd (req: CreateProcurementRecommendationReq) : Validation<CreateProcurementRecommendationCmd, DomainError> =
    let reqIdV = validateRequirementId req.RequirementId
    let itemV = validateItemId req.ItemId
    let locV = validateLocationId req.LocationId
    let reqDateV = validateTimestamp req.RequiredDate
    let reqQtyV = validateQty req.RequiredQuantity
    let evalTimestampV = validateTimestamp req.EvaluationTimestamp

    let planIdV =
        if String.IsNullOrWhiteSpace req.SourceSupplyPlanId then
            Invalid [DomainError.validation "SourceSupplyPlanId cannot be empty"]
        else
            Valid req.SourceSupplyPlanId

    (fun r s i l rd rq et ->
        {   RequirementId = r
            SourceSupplyPlanId = s
            ItemId = i
            LocationId = l
            RequiredDate = rd
            RequiredQuantity = rq
            EvaluationTimestamp = et })
    <!> reqIdV
    <*> planIdV
    <*> itemV
    <*> locV
    <*> reqDateV
    <*> reqQtyV
    <*> evalTimestampV
