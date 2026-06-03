module Medhavi.Scheduler.Mrp.Pipeline.Orchestrator

open System
open Medhavi.Common.Patterns
open Medhavi.SharedKernel
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Mrp.Domain.Errors
open Medhavi.Scheduler.Mrp.Domain.Policies
open Medhavi.Scheduler.Mrp.Domain.MrpRunAggregate
open Medhavi.Scheduler.Mrp.Pipeline
open Medhavi.Scheduler.Mrp.Domain.Algorithms
open Medhavi.Scheduler.Mrp.Steps
open Medhavi.Planning.Mrp.Steps
open Medhavi.Scheduler.Mrp.Application

// ============================================================================
// PIPELINE COMPOSITION
// ============================================================================

/// Create full MRP pipeline
let createPipeline (deps: MrpDependencies) : MrpStepAsync<MrpDemand list, MrpRunResult> =
    let preprocessStep = PreprocessStep.execute
    let bomStep = BomExplosionStep.createStep deps.BomLookup

    let nettingStep =
        NettingStep.createStep deps.OnHandQuery deps.InboundQuery deps.ReservationsQuery deps.SafetyStockQuery

    let supplyStep =
        SupplyGenerationStep.createStep
            deps.ProductTypeQuery
            deps.SupplierQuery
            deps.RoutingQuery
            deps.TransferSourceQuery

    let capacityStep =
        CapacityCheckStep.createStep deps.CapacityQuery deps.AlternateRoutingsQuery

    let peggingStep = PeggingStep.createStep deps.PeggingCreator
    let postprocessStep = PostprocessStep.execute deps.ReservationCreator

    fun demands ctx ->
        task {
            // Step 1: Preprocess (Validate inputs, consume forecast, group)
            let! step1 = preprocessStep demands ctx

            match step1 with
            | Error e -> return Error e
            | Ok(processedDemands, ctx1) ->

                // Step 2: Multi-level BOM Explosion
                let! step2 = bomStep processedDemands ctx1

                match step2 with
                | Error e -> return Error e
                | Ok(components, ctx2) ->

                    // Step 3: Material Netting
                    let! step3 = nettingStep components ctx2

                    match step3 with
                    | Error e -> return Error e
                    | Ok(netReqs, ctx3) ->

                        // Step 4: Supply Order Proposals
                        let! step4 = supplyStep netReqs ctx3

                        match step4 with
                        | Error e -> return Error e
                        | Ok(proposals, ctx4) ->

                            // Step 5: Capacity check CTP
                            let! step5 = capacityStep proposals ctx4

                            match step5 with
                            | Error e -> return Error e
                            | Ok(checkedProposals, ctx5) ->

                                // Step 6: Pegging (demand -> supply links)
                                let! step6 = peggingStep checkedProposals ctx5

                                match step6 with
                                | Error e -> return Error e
                                | Ok(peggedProposals, ctx6) ->

                                    // Step 7: Postprocess (Finalize run details, reserve materials)
                                    let! step7 = postprocessStep peggedProposals ctx6

                                    match step7 with
                                    | Error e -> return Error e
                                    | Ok(runResult, ctx7) ->
                                        // Inject netting calculations back into the final result
                                        let finalResult =
                                            { runResult with
                                                NetRequirements = netReqs }

                                        return Ok(finalResult, ctx7)
        }

// ============================================================================
// EXECUTION LAYER
// ============================================================================

/// Execute the MRP pipeline with context initialization
let execute
    (pipeline: MrpStepAsync<MrpDemand list, MrpRunResult>)
    (runId: string)
    (startDate: Timestamp)
    (endDate: Timestamp)
    (stockingPointId: StockingPointId)
    (policy: MrpPolicy)
    (demands: MrpDemand list)
    : TaskResult<MrpRunResult, MrpApplicationError> =
    task {
        let runIdObj =
            MrpRunId.create runId
            |> Result.defaultWith (fun _ -> failwith "Invalid RunId")

        let ctx = MrpContext.create runIdObj startDate endDate stockingPointId policy

        try
            let! result = pipeline demands ctx

            match result with
            | Ok(mrpResult, _) -> return Ok mrpResult
            | Error stepError -> return Error(MrpApplicationError.PipelineError stepError)
        with ex ->
            return Error(MrpApplicationError.UnexpectedError ex)
    }

/// Execute MRP pipeline with timeout safety
let executeWithTimeout
    (timeout: TimeSpan)
    (pipeline: MrpStepAsync<MrpDemand list, MrpRunResult>)
    (runId: string)
    (startDate: Timestamp)
    (endDate: Timestamp)
    (stockingPointId: StockingPointId)
    (policy: MrpPolicy)
    (demands: MrpDemand list)
    : Async<Result<MrpRunResult, MrpApplicationError>> =
    async {
        let runIdObj =
            MrpRunId.create runId
            |> Result.defaultWith (fun _ -> failwith "Invalid RunId")

        let ctx = MrpContext.create runIdObj startDate endDate stockingPointId policy

        try
            let! child = Async.StartChild(Async.AwaitTask(pipeline demands ctx), int timeout.TotalMilliseconds)
            let! result = child

            match result with
            | Ok(mrpResult, _) -> return Ok mrpResult
            | Error stepError -> return Error(MrpApplicationError.PipelineError stepError)
        with
        | :? TimeoutException -> return Error(MrpApplicationError.Timeout timeout)
        | ex -> return Error(MrpApplicationError.UnexpectedError ex)
    }
