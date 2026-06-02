/// Pegging Step — Creates demand-to-supply traceability links (pegging links)
/// FP Pattern: Railway-Oriented Programming (ROP) with async pipelines
module Medhavi.Planning.Mrp.Steps.PeggingStep

open System
open Medhavi.SharedKernel
open Medhavi.Planning.Mrp.Domain.Types
open Medhavi.Planning.Mrp.Domain.Errors
open Medhavi.Planning.Mrp.Pipeline.PipelineTypes

// ============================================================================
// DEPENDENCIES
// ============================================================================

/// Injected pegging creation function.
/// Takes (SkuId, DemandId, ProposalId, Quantity) -> returns Ok peggingLinkId or Error msg.
type PeggingCreator = SkuId -> string -> string -> Quantity -> Async<Result<string, string>>

// ============================================================================
// STEP CREATION
// ============================================================================

/// Create pegging step
let createStep (peggingCreatorOpt: PeggingCreator option) : MrpStepAsync<SupplyProposal list, SupplyProposal list> =
    fun proposals ctx ->
        async {
            let startTime = DateTimeOffset.UtcNow

            match peggingCreatorOpt with
            | None ->
                // No pegging service injected: use synthetic pegging links
                let proposalsWithPegs =
                    proposals
                    |> List.mapi (fun idx p ->
                        let syntheticPegId = $"peg-{MrpRunId.value ctx.RunId}-{SkuId.value p.SkuId}-{idx}"
                        { p with PeggingRefs = [ syntheticPegId ] })

                let updatedCtx =
                    ctx
                    |> MrpContext.addEvent (PeggingCompleted (List.length proposals))
                    |> MrpContext.addWarning "No pegging service injected — generated synthetic pegging references"

                return Ok (proposalsWithPegs, updatedCtx)

            | Some pegCreator ->
                // Create actual pegging links in parallel
                let! results =
                    proposals
                    |> List.map (fun proposal ->
                        async {
                            // Find the demand references keyed on the proposal
                            let demandId = 
                                match proposal.PeggingRefs with
                                | head :: _ -> head
                                | [] -> $"demand-{MrpRunId.value ctx.RunId}-{SkuId.value proposal.SkuId}"

                            let! pegResult = 
                                pegCreator 
                                    proposal.SkuId 
                                    demandId 
                                    (SupplyProposalId.value proposal.Id) 
                                    proposal.Quantity

                            match pegResult with
                            | Ok pegLinkId ->
                                return Ok ({ proposal with PeggingRefs = [ pegLinkId ] })
                            | Error err ->
                                return Error (proposal, err)
                        })
                    |> Async.Parallel

                let successes = results |> Array.choose (function Ok p -> Some p | _ -> None) |> List.ofArray
                let failures = results |> Array.choose (function Error (p, e) -> Some (p, e) | _ -> None) |> List.ofArray

                // For failures, keep the proposal but add warning
                let failedProposalsPassThrough = failures |> List.map fst
                let allProposals = successes @ failedProposalsPassThrough

                let endTime = DateTimeOffset.UtcNow
                let duration = endTime - startTime

                let updatedCtx =
                    ctx
                    |> MrpContext.addEvent (PeggingCompleted (List.length allProposals))
                    |> (fun c ->
                        failures
                        |> List.fold (fun acc (p, err) ->
                            MrpContext.addWarning $"Pegging creation failed for proposal {SupplyProposalId.value p.Id}: {err}" acc)
                            c)

                return Ok (allProposals, updatedCtx)
        }

/// Simple pegging step utilizing synthetic refs
let createSimpleStep : MrpStepAsync<SupplyProposal list, SupplyProposal list> =
    createStep None
