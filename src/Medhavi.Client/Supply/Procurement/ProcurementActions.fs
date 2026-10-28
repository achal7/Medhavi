module Medhavi.Client.Supply.Procurement.ProcurementActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Supply
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ActionExecution
open Medhavi.Client

let createRecommendation: ActionHandler =
    fun ctx ->
        task {
            let apis = ctx.Domain.Supply.ProcureMaterials.ProcureMaterialsApis
            let reqId =
                match ctx.TargetKey with
                | Some k -> k
                | None -> $"PR-REQ-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}"

            let itemId =
                match ctx.Payload with
                | CustomParameters p ->
                    match Map.tryFind "ItemId" p with
                    | Some (StringValue s) -> s
                    | _ -> "SKU-DRIVE-500GB"
                | _ -> "SKU-DRIVE-500GB"

            let locId =
                match ctx.Payload with
                | CustomParameters p ->
                    match Map.tryFind "LocationId" p with
                    | Some (StringValue s) -> s
                    | _ -> "LOC-CENTRAL-WH"
                | _ -> "LOC-CENTRAL-WH"

            let qty =
                match ctx.Payload with
                | QuantityValue q -> q
                | CustomParameters p ->
                    match Map.tryFind "Quantity" p with
                    | Some (DecimalValue d) -> d
                    | _ -> 100m
                | _ -> 100m

            let reqDate =
                match ctx.Payload with
                | DateValue d -> d
                | CustomParameters p ->
                    match Map.tryFind "RequiredDate" p with
                    | Some (DateTimeValue d) -> d
                    | _ -> DateTimeOffset.UtcNow.AddDays 14.0
                | _ -> DateTimeOffset.UtcNow.AddDays 14.0

            let req: CreateProcurementRecommendationReq =
                { RequirementId = reqId
                  SourceSupplyPlanId = "PLAN-2026-Q3"
                  ItemId = itemId
                  LocationId = locId
                  RequiredDate = reqDate
                  RequiredQuantity = qty
                  EvaluationTimestamp = DateTimeOffset.UtcNow }

            match! apis.Recommendation.Create req with
            | Ok dto ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Procurement recommendation created for '{dto.ItemId}' (Requirement: {dto.RequirementId}, Qty: {dto.RequiredQuantity}).",
                            [ ProcurementGrid.storeId
                              DataStoreId.literal "supply-store" ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let publishPlan: ActionHandler =
    fun ctx ->
        task {
            let apis = ctx.Domain.Supply.ProcureMaterials.ProcureMaterialsApis
            let req: PublishProcurementPlanReq =
                { PlanningScopeId = "PS-GLOBAL-SUPPLY-CHAIN"
                  PlanningCycleId = "CYCLE-2026-W37"
                  PublicationAuthority = "AUTOMATED-APS"
                  PublicationTimestamp = DateTimeOffset.UtcNow }

            match! apis.Plan.Publish req with
            | Ok planDto ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Procurement plan published to enterprise picture (Version {planDto.VersionNumber}, Scope '{planDto.PlanningScopeId}').",
                            [ ProcurementGrid.storeId
                              DataStoreId.literal "supply-store" ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let all: (ActionDefId * ActionHandler) list =
    [ ProcurementGrid.ProcurementActions.createRecommendation.Id, createRecommendation
      ProcurementGrid.ProcurementActions.publishPlan.Id, publishPlan ]
