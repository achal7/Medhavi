namespace Medhavi.Supply.ScheduleProduction

open Medhavi.Common
open Medhavi.Contracts.Supply
open Medhavi.Supply.AclHelpers

/// High-level Capability Composition and Command API for Schedule Production
/// Traces to: Supply Intelligence Specification §4.3.1 (CA-S-007, BW-S-070, BW-S-071, BN-S-060, BN-S-061)
module Capabilities =

    let create
        (scheduleAggApi: ProductionSchedule.Capabilities.ProductionScheduleAggregateApi)
        (publishedAggApi: PublishedProductionSchedule.Capabilities.PublishedProductionScheduleAggregateApi)
        : ScheduleProductionApis =

        let scheduleApi: ProductionScheduleApi =
            {
                Create =
                    fun req ->
                        scheduleAggApi.Create req
                        |> TaskResult.map ProductionSchedule.Projections.toDto
                        |> TaskResult.mapError mapAppErrorToApiError
            }

        let publishedBaselineApi: PublishedProductionScheduleApi =
            {
                Publish =
                    fun req ->
                        publishedAggApi.Publish req
                        |> TaskResult.map PublishedProductionSchedule.Projections.toDto
                        |> TaskResult.mapError mapAppErrorToApiError
            }

        {
            Schedule = scheduleApi
            PublishedBaseline = publishedBaselineApi
        }
