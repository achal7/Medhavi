module Medhavi.Scheduler.Mrp.Steps.PeggingStep

open System
open System.Threading.Tasks
open Medhavi.Common.Patterns
open Medhavi.SharedKernel
open Medhavi.Scheduler.Mrp.Domain
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Mrp.Pipeline

// ============================================================================
// DEPENDENCIES
// ============================================================================

/// Injected pegging creation function.
/// Takes (SkuId, DemandId, ProposalId, Quantity) -> returns Ok peggingLinkId or Error msg.
type PeggingCreator = SkuId -> string -> string -> Quantity -> TaskResult<string, string>

// ============================================================================
// STEP CREATION
// ============================================================================

/// Create pegging step
let createStep (peggingCreatorOpt: PeggingCreator option) : MrpStepAsync<SupplyProposal list, SupplyProposal list> =
    fun proposals ctx ->
        task {
            let startTime = DateTimeOffset.UtcNow

            match peggingCreatorOpt with
            | None ->
                let resolvedPolicy =
                    PeggingPolicy.resolvePolicy ctx.Policy.PeggingPolicyTier

                let newPeggingLinks =
                    PeggingEngine.pegSuppliesToDemands resolvedPolicy ctx.Demands proposals

                let proposalsWithPegs =
                    proposals
                    |> List.map (fun p ->
                        let pPegs =
                            newPeggingLinks
                            |> List.filter (fun peg ->
                                match peg.Target with
                                | Supply s -> s.SupplyId = SupplyProposalId.value p.Id
                                | _ -> false)
                            |> List.map (fun peg -> PeggingId.value peg.Id)

                        { p with PeggingRefs = pPegs })

                let updatedCtx =
                    { ctx with Peggings = newPeggingLinks }
                    |> MrpContext.addEvent (PeggingCompleted(List.length proposalsWithPegs))

                return Ok(proposalsWithPegs, updatedCtx)

            | Some pegCreator ->
                // Create actual pegging links in parallel
                let peggingTasks =
                    proposals
                    |> List.map (fun proposal ->
                        task {
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
                                let pegId =
                                    PeggingId.create pegLinkId
                                    |> Result.defaultWith (fun _ ->
                                        PeggingId.createDeterministic demandId (SupplyProposalId.value proposal.Id))

                                let link =
                                    { Id = pegId
                                      Demand =
                                        { DemandId = demandId
                                          SkuId = proposal.SkuId
                                          NodeId = proposal.NodeId
                                          StockingPointId = proposal.StockingPointId
                                          NeedDate = proposal.DueDate
                                          Quantity = proposal.Quantity }
                                      Target =
                                        Supply
                                            { SupplyId = SupplyProposalId.value proposal.Id
                                              ProposalType = proposal.ProposalType
                                              SkuId = proposal.SkuId
                                              NodeId = proposal.NodeId
                                              StockingPointId = proposal.StockingPointId
                                              DeliveryDate = proposal.DueDate
                                              Quantity = proposal.Quantity }
                                      PeggedQty = proposal.Quantity
                                      Status = PegStatus.Active
                                      IsLocked = false
                                      Created = DateTimeOffset.UtcNow
                                      Modified = DateTimeOffset.UtcNow }

                                return
                                    Ok(
                                        { proposal with
                                            PeggingRefs = [ pegLinkId ] },
                                        link
                                    )
                            | Error err -> return Error(proposal, err)
                        })

                let! results = Task.WhenAll(peggingTasks)

                let successes =
                    results
                    |> Array.choose (function
                        | Ok(p, _) -> Some p
                        | _ -> None)
                    |> List.ofArray

                let successLinks =
                    results
                    |> Array.choose (function
                        | Ok(_, link) -> Some link
                        | _ -> None)
                    |> List.ofArray

                let failures =
                    results
                    |> Array.choose (function
                        | Error(p, e) -> Some(p, e)
                        | _ -> None)
                    |> List.ofArray

                // For failures, keep the proposal but add warning
                let failedProposalsPassThrough = failures |> List.map fst
                let allProposals = successes @ failedProposalsPassThrough

                let endTime = DateTimeOffset.UtcNow
                let duration = endTime - startTime

                let updatedCtx =
                    { ctx with Peggings = successLinks }
                    |> MrpContext.addEvent (PeggingCompleted(List.length allProposals))
                    |> (fun c ->
                        failures
                        |> List.fold
                            (fun acc (p, err) ->
                                MrpContext.addWarning
                                    $"Pegging creation failed for proposal {SupplyProposalId.value p.Id}: {err}"
                                    acc)
                            c)

                return Ok(allProposals, updatedCtx)
        }

/// Simple pegging step utilizing synthetic refs
let createSimpleStep: MrpStepAsync<SupplyProposal list, SupplyProposal list> =
    createStep None
