module Medhavi.Client.Supply.Pegging.PeggingSchedulerActions

open System.Threading.Tasks
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ActionExecution

let realignPegging: ActionHandler =
    fun _ctx ->
        task {
            return
                Ok(
                    ActionExecutionOutcome.Completed(
                        "Demand-Supply pegging horizon realigned with latest enterprise ledger commitments.",
                        [ PeggingSchedulerStore.storeId
                          DataStoreId.literal "demo-demand-store"
                          DataStoreId.literal "supply-store" ]
                    )
                )
        }

let all: (ActionDefId * ActionHandler) list =
    [ PeggingSchedulerSchema.realignPegging.Id, realignPegging ]
