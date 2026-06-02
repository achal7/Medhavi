/// MRP Application Service — Coordinates pipeline execution and triggers supply order creation
/// FP/DDD Pattern: Application service orchestrating domain flows, completely decoupled from infrastructure details
module Medhavi.Planning.Mrp.Application.MrpService

open System
open Medhavi.SharedKernel
open Medhavi.Planning.Mrp.Domain.Types
open Medhavi.Planning.Mrp.Domain.Errors
open Medhavi.Planning.Mrp.Domain.Policies
open Medhavi.Planning.Mrp.Pipeline.PipelineTypes
open Medhavi.Planning.Mrp.Pipeline.Orchestrator

/// Application service signature for executing a planning run
type MrpService =
    { /// Run the complete MRP pipeline, including generating PO/WO/TO supply proposals in Supply BC
      ExecuteRun:
          string
              -> Timestamp
              -> Timestamp
              -> StockingPointId
              -> MrpPolicy
              -> MrpDemand list
              -> Async<Result<MrpRunResult, MrpApplicationError>>

      /// Run in dry-run mode (returns proposals without creating them in Supply BC)
      ExecuteDryRun:
          string
              -> Timestamp
              -> Timestamp
              -> StockingPointId
              -> MrpPolicy
              -> MrpDemand list
              -> Async<Result<MrpRunResult, MrpApplicationError>> }

/// Create a concrete instance of MrpService
let create (deps: MrpDependencies) : MrpService =
    
    // Wire the steps into the orchestrator pipeline
    let pipelineDeps : MrpPipelineDependencies =
        { BomLookup = deps.BomLookup
          OnHandQuery = deps.OnHandQuery
          InboundQuery = deps.InboundQuery
          ReservationsQuery = deps.ReservationsQuery
          SafetyStockQuery = deps.SafetyStockQuery
          ProductTypeQuery = deps.ProductTypeQuery
          SupplierQuery = deps.SupplierQuery
          RoutingQuery = deps.RoutingQuery
          TransferSourceQuery = deps.TransferSourceQuery
          CapacityQuery = deps.CapacityQuery
          AlternateRoutingsQuery = deps.AlternateRoutingsQuery
          PeggingCreator = deps.PeggingCreator
          ReservationCreator = deps.ReservationCreator }

    let pipeline = createPipeline pipelineDeps

    { ExecuteRun =
        fun runId startDate endDate spId policy demands ->
            async {
                // 1. Run the pipeline
                let! pipeResult = execute pipeline runId startDate endDate spId policy demands
                match pipeResult with
                | Error err -> return Error err
                | Ok runResult ->
                    // 2. Commit the generated proposals to the Supply Bounded Context
                    let runIdObj = MrpRunId.create runId |> Result.defaultWith (fun _ -> failwith "Invalid RunId")
                    let! supplyOrderResult = deps.CreateSupplyOrders runIdObj runResult.Proposals
                    match supplyOrderResult with
                    | Error errStr ->
                        return Error (MrpApplicationError.UnexpectedError (Exception $"Failed to persist generated supply orders: {errStr}"))
                    | Ok _ ->
                        return Ok runResult
            }

      ExecuteDryRun =
        fun runId startDate endDate spId policy demands ->
            execute pipeline runId startDate endDate spId policy demands }
