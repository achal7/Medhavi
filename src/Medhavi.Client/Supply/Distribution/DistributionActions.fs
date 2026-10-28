module Medhavi.Client.Supply.Distribution.DistributionActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Supply
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ActionExecution
open Medhavi.Client

let createRecommendation: ActionHandler =
    fun ctx ->
        task {
            let apis = ctx.Domain.Supply.ManageDistribution.ManageDistributionApis
            let reqRef =
                match ctx.TargetKey with
                | Some k -> k
                | None -> $"DIST-REQ-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}"

            let req: CreateDistributionRecommendationReq =
                { RequirementReference = reqRef
                  Timestamp = DateTimeOffset.UtcNow }

            match! apis.Recommendation.Create req with
            | Ok dto ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Distribution recommendation created for '{dto.ItemId}' (Requirement: {dto.RequirementReference}, Destination: {dto.DestinationLocationId}).",
                            [ DistributionGrid.storeId
                              DataStoreId.literal "supply-store" ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let publishPlan: ActionHandler =
    fun ctx ->
        task {
            let apis = ctx.Domain.Supply.ManageDistribution.ManageDistributionApis
            let req: PublishDistributionPlanReq =
                { PlanningScopeId = "PS-GLOBAL-SUPPLY-CHAIN"
                  PlanningCycle = "CYCLE-2026-W37"
                  PublicationReason = PublicationReasonType.ScheduledCadence
                  Timestamp = DateTimeOffset.UtcNow }

            match! apis.Plan.Publish req with
            | Ok planDto ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Distribution plan published (Plan: {planDto.PlanId}, Movements: {planDto.TotalMovementCount}).",
                            [ DistributionGrid.storeId
                              DataStoreId.literal "supply-store" ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let all: (ActionDefId * ActionHandler) list =
    [ DistributionGrid.DistributionActions.createRecommendation.Id, createRecommendation
      DistributionGrid.DistributionActions.publishPlan.Id, publishPlan ]
