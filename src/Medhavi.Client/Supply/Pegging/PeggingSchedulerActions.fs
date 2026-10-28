module Medhavi.Client.Supply.Pegging.PeggingSchedulerActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Supply
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ActionExecution
open Medhavi.Client

let realignPegging: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Supply.ScheduleProduction.ScheduleProductionApis
            let req: PublishProductionScheduleReq =
                { PlanningScopeId = "PS-GLOBAL-SUPPLY-CHAIN"
                  PlanningPeriodId = "P-2026-09"
                  TriggerType = "PeggingRealignment"
                  Timestamp = DateTimeOffset.UtcNow }

            match! api.PublishedBaseline.Publish req with
            | Ok sched ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Production schedule baseline published ({sched.BaselineId}) and pegging realigned for scope '{sched.PlanningScopeId}'.",
                            [ PeggingScheduler.storeId
                              DataStoreId.literal "supply-store"
                              DataStoreId.literal "capacity-store" ]
                        )
                    )
            | Error err ->
                return Error err.Message
        }

let all: (ActionDefId * ActionHandler) list =
    [ PeggingScheduler.PeggingSchedulerActions.realignPegging.Id, realignPegging ]
