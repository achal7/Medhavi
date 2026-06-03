module Medhavi.Scheduler.Mrp.MrpService

open System
open Medhavi.Common.Patterns
open Medhavi.SharedKernel
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Mrp.Domain.MrpRunAggregate
open Medhavi.Scheduler.Mrp.Domain.Errors
open Medhavi.Scheduler.Mrp.Domain.Policies
open Medhavi.Scheduler.Mrp.Pipeline.Orchestrator
open Medhavi.Scheduler.Mrp.Application

/// Application service signature for executing a planning run
type MrpService =
    {
        /// Run the complete MRP pipeline, including generating PO/WO/TO supply proposals in Supply BC
        ExecuteRun:
            string
                -> Timestamp
                -> Timestamp
                -> StockingPointId
                -> MrpPolicy
                -> MrpDemand list
                -> PeggingLink list
                -> TaskResult<MrpRunResult, MrpApplicationError>

        /// Run in dry-run mode (returns proposals without creating them in Supply BC)
        ExecuteDryRun:
            string
                -> Timestamp
                -> Timestamp
                -> StockingPointId
                -> MrpPolicy
                -> MrpDemand list
                -> PeggingLink list
                -> TaskResult<MrpRunResult, MrpApplicationError>
    }

/// Create a concrete instance of MrpService
let create (deps: MrpDependencies) : MrpService =

    let pipeline = createPipeline deps

    { ExecuteRun =
        fun runId startDate endDate spId policy demands firmedPegs ->
            task {
                // 1. Run the pipeline
                let! pipeResult = execute pipeline runId startDate endDate spId policy demands firmedPegs

                match pipeResult with
                | Error err -> return Error err
                | Ok runResult ->
                    // 2. Commit the generated proposals to the Supply Bounded Context
                    let runIdObj =
                        MrpRunId.create runId
                        |> Result.defaultWith (fun _ -> failwith "Invalid RunId")

                    let! supplyOrderResult = deps.CreateSupplyOrders runIdObj runResult.Proposals

                    match supplyOrderResult with
                    | Error errStr ->
                        return
                            Error(
                                MrpApplicationError.UnexpectedError(
                                    Exception $"Failed to persist generated supply orders: {errStr}"
                                )
                            )
                    | Ok _ -> return Ok runResult
            }

      ExecuteDryRun =
        fun runId startDate endDate spId policy demands firmedPegs ->
            execute pipeline runId startDate endDate spId policy demands firmedPegs }
