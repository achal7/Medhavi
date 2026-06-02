/// Postprocess Step — Aggregates final results, builds MRP run outputs, and triggers reservations
/// FP Pattern: Railway-Oriented Programming (ROP) with async pipelines
module Medhavi.Planning.Mrp.Steps.PostprocessStep

open System
open Medhavi.SharedKernel
open Medhavi.Planning.Mrp.Domain.Types
open Medhavi.Planning.Mrp.Domain.Errors
open Medhavi.Planning.Mrp.Pipeline.PipelineTypes

// ============================================================================
// SIGNATURES & DEPENDENCIES
// ============================================================================

/// Injected reservation creator function.
/// Takes (SkuId, StockingPointId, Quantity, DueDate) -> returns Ok or Error msg.
type ReservationCreator = SkuId -> StockingPointId -> Quantity -> Timestamp -> Async<Result<unit, string>>

// ============================================================================
// HELPERS
// ============================================================================

let private createPeggingLinks (proposals: SupplyProposal list) : PeggingLink list =
    proposals
    |> List.collect (fun p ->
        p.PeggingRefs
        |> List.map (fun pegId ->
            { DemandId = pegId
              SupplyId = SupplyProposalId.value p.Id
              Quantity = p.Quantity
              SkuId = p.SkuId }))

let private createReservations
    (reservationCreatorOpt: ReservationCreator option)
    (proposals: SupplyProposal list)
    : Async<Result<unit, string list>> =
    async {
        match reservationCreatorOpt with
        | None -> return Ok ()
        | Some creator ->
            let! results =
                proposals
                |> List.map (fun p -> creator p.SkuId p.StockingPointId p.Quantity p.DueDate)
                |> Async.Parallel
                
            let errors =
                results
                |> Array.choose (function Error e -> Some e | _ -> None)
                |> List.ofArray
                
            if List.isEmpty errors then return Ok ()
            else return Error errors
    }

let private buildRunResult (proposals: SupplyProposal list) (ctx: MrpContext) (endTime: Timestamp) : MrpRunResult =
    let pegLinks = createPeggingLinks proposals
    { RunId = ctx.RunId
      StartTime = ctx.StartDate
      EndTime = endTime
      Status = MrpRunStatus.Completed
      BomExplosionCount = ctx.Telemetry.ComponentsProcessed
      NetRequirements = [] // Will be mapped in Orchestrator
      Proposals = proposals
      ActionMessages = ctx.ActionMessages
      PeggingLinks = pegLinks
      Errors = []
      Warnings = ctx.Warnings }

// ============================================================================
// STEP EXECUTION
// ============================================================================

/// Postprocess step execution
let execute (reservationCreatorOpt: ReservationCreator option) : MrpStepAsync<SupplyProposal list, MrpRunResult> =
    fun proposals ctx ->
        async {
            let endTime = Timestamp.now

            // Call reservation creator to reserve material for proposals
            let! resResult = createReservations reservationCreatorOpt proposals

            match resResult with
            | Error errs ->
                let baseResult = buildRunResult proposals ctx endTime
                let failedResult = { baseResult with Status = MrpRunStatus.Failed "Reservation creation failed"; Errors = errs }
                return Error (Postprocess errs)
            | Ok _ ->
                let runResult = buildRunResult proposals ctx endTime
                let totalDuration = Timestamp.value endTime - Timestamp.value ctx.StartDate

                let finalCtx =
                    ctx
                    |> MrpContext.addEvent (MrpRunCompleted (MrpRunId.value ctx.RunId, endTime))
                    |> MrpContext.updateTelemetry (fun t ->
                        { t with TotalDuration = totalDuration })

                return Ok (runResult, finalCtx)
        }
