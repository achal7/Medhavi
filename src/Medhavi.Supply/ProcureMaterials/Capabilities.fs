namespace Medhavi.Supply.ProcureMaterials

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Contracts.Supply
open Medhavi.Supply.AclHelpers

type ProcureMaterialsApisDependencies =
    {
        RecommendationAggregate: ProcurementRecommendation.ProcurementRecommendationAggregateApi
        PlanAggregate: ProcurementPlan.ProcurementPlanAggregateApi
    }

/// High-level Capability Composition and Command API for Procure Materials
/// Traces to: Supply Intelligence Specification §4.3.2 (CA-S-006, BW-S-060, BW-S-061, BN-S-050, BN-S-051)
module Capabilities =

    let create (deps: ProcureMaterialsApisDependencies) : ProcureMaterialsApis =

        let recommendationApi: ProcurementRecommendationApi =
            {
                Create =
                    fun req ->
                        deps.RecommendationAggregate.Create req
                        |> TaskResult.map ProcurementRecommendation.Projections.mapRecommendationToDto
                        |> TaskResult.mapError mapAppErrorToApiError
            }

        let planApi: ProcurementPlanApi =
            {
                Publish =
                    fun req ->
                        deps.PlanAggregate.Publish req
                        |> TaskResult.map ProcurementPlan.Projections.mapPlanToDto
                        |> TaskResult.mapError mapAppErrorToApiError
            }

        {
            Recommendation = recommendationApi
            Plan = planApi
        }
