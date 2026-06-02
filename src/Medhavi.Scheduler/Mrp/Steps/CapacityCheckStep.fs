/// Capacity Check Step — Check and reserve capacity for work orders via Capacity CTP
/// FP Pattern: Railway-Oriented Programming (ROP) with async pipelines
module Medhavi.Planning.Mrp.Steps.CapacityCheckStep

open System
open Medhavi.SharedKernel
open Medhavi.Planning.Mrp.Domain.Types
open Medhavi.Planning.Mrp.Domain.Errors
open Medhavi.Planning.Mrp.Domain.Policies
open Medhavi.Planning.Mrp.Pipeline.PipelineTypes

// ============================================================================
// SIGNATURES
// ============================================================================

/// Query to check capacity availability. Returns the end timestamp of allocation.
type CapacityCheckQuery =
    StockingPointId
        -> SkuId
        -> RoutingId option
        -> Quantity
        -> Timestamp
        -> CapacityPolicy
        -> Async<Result<Timestamp, CapacityCheckError>>

/// Query to look up alternate routings for a SKU at a stocking point.
type AlternateRoutingsQuery = SkuId -> StockingPointId -> Async<RoutingId list>

// ============================================================================
// STEP CREATION
// ============================================================================

/// Create capacity check step
let createStep
    (capacityQuery: CapacityCheckQuery)
    (alternateRoutingsQuery: AlternateRoutingsQuery)
    : MrpStepAsync<SupplyProposal list, SupplyProposal list> =
    fun proposals ctx ->
        async {
            let startTime = DateTimeOffset.UtcNow

            // Partition work orders from other supply proposals
            let (workOrders, otherProposals) =
                proposals
                |> List.partition (fun p ->
                    match p.ProposalType with
                    | PlannedWorkOrder -> true
                    | _ -> false)

            // Execute capacity check on work orders in parallel
            let! checkedWorkOrders =
                workOrders
                |> List.map (fun proposal ->
                    async {
                        if ctx.Policy.CapacityPolicy.Finite then
                            let! result =
                                capacityQuery
                                    proposal.StockingPointId
                                    proposal.SkuId
                                    proposal.RoutingId
                                    proposal.Quantity
                                    proposal.DueDate
                                    ctx.Policy.CapacityPolicy

                            match result with
                            | Ok allocatedDate ->
                                if allocatedDate <= proposal.DueDate then
                                    // Feasible on time
                                    return Ok { proposal with CapacityCheckedDate = Some allocatedDate }
                                else
                                    // Delayed: Try alternate routings
                                    if ctx.Policy.CapacityPolicy.MaxAlternateAttempts > 0 then
                                        let! alternates = alternateRoutingsQuery proposal.SkuId proposal.StockingPointId
                                        let otherAlternates =
                                            alternates
                                            |> List.filter (fun r -> Some r <> proposal.RoutingId)
                                            |> List.truncate ctx.Policy.CapacityPolicy.MaxAlternateAttempts

                                        match otherAlternates with
                                        | [] ->
                                            // No alternates available: shift original forward
                                            return Ok { proposal with DueDate = allocatedDate; CapacityCheckedDate = Some allocatedDate }
                                        | _ ->
                                            let! alternateResults =
                                                otherAlternates
                                                |> List.map (fun altRouting ->
                                                    async {
                                                        let! res =
                                                            capacityQuery
                                                                proposal.StockingPointId
                                                                proposal.SkuId
                                                                (Some altRouting)
                                                                proposal.Quantity
                                                                proposal.DueDate
                                                                ctx.Policy.CapacityPolicy
                                                        match res with
                                                        | Ok date -> return Some (altRouting, date)
                                                        | Error _ -> return None
                                                    })
                                                |> Async.Parallel

                                            let candidates = alternateResults |> Array.choose id |> List.ofArray
                                            match candidates with
                                            | [] ->
                                                // No alternates had capacity: shift primary forward
                                                return Ok { proposal with DueDate = allocatedDate; CapacityCheckedDate = Some allocatedDate }
                                            | _ ->
                                                let bestCandidate =
                                                    candidates
                                                    |> List.sortBy (fun (_, date) ->
                                                        let delay = (Timestamp.value date - Timestamp.value proposal.DueDate).TotalSeconds
                                                        if delay <= 0.0 then 0.0 else delay)
                                                    |> List.head

                                                let (bestRouting, bestDate) = bestCandidate
                                                if bestDate < allocatedDate then
                                                    // Alternate is faster than primary shift! Fallback onto alternate
                                                    return Ok { proposal with RoutingId = Some bestRouting; DueDate = bestDate; CapacityCheckedDate = Some bestDate }
                                                else
                                                    // Primary shift is still best
                                                    return Ok { proposal with DueDate = allocatedDate; CapacityCheckedDate = Some allocatedDate }
                                    else
                                        // MaxAlternateAttempts is 0: just shift primary forward
                                        return Ok { proposal with DueDate = allocatedDate; CapacityCheckedDate = Some allocatedDate }
                            | Error err ->
                                // Fallback to due date if allocation fails entirely
                                return Error (proposal, err)
                        else
                            // Infinite capacity fallback
                            return Ok { proposal with CapacityCheckedDate = Some proposal.DueDate }
                    })
                |> Async.Parallel

            // Separate successful and failed capacity allocations
            let successes =
                checkedWorkOrders
                |> Array.choose (function Ok p -> Some p | _ -> None)
                |> List.ofArray

            let failures =
                checkedWorkOrders
                |> Array.choose (function Error (p, e) -> Some (p, e) | _ -> None)
                |> List.ofArray

            // Update proposals with failures as fallback-allocated
            let failedProposalsWithFallback =
                failures
                |> List.map (fun (p, _) -> { p with CapacityCheckedDate = Some p.DueDate })

            let allCheckedWorkOrders = successes @ failedProposalsWithFallback
            let allProposals = allCheckedWorkOrders @ otherProposals

            let endTime = DateTimeOffset.UtcNow
            let duration = endTime - startTime

            let updatedCtx =
                ctx
                |> MrpContext.addEvent (CapacityCheckCompleted (List.length workOrders))
                |> MrpContext.updateTelemetry (fun t ->
                    { t with CapacityCheckDuration = duration })
                |> (fun c ->
                    failures
                    |> List.fold (fun acc (p, err) ->
                        let msg =
                            match err with
                            | CapacityUnavailable (res, req, avail) ->
                                $"Capacity overload on {res} for SKU {SkuId.value p.SkuId}: needed {req}, available {avail}"
                            | AllocationFailed reason ->
                                $"Capacity check allocation failed for SKU {SkuId.value p.SkuId}: {reason}"
                            | _ -> $"Capacity check error for SKU {SkuId.value p.SkuId}"
                        MrpContext.addWarning msg acc)
                        c)

            return Ok (allProposals, updatedCtx)
        }

/// Step utilizing infinite capacity (always allocates on due date)
let createInfiniteCapacityStep : MrpStepAsync<SupplyProposal list, SupplyProposal list> =
    fun proposals ctx ->
        async {
            let updatedProposals =
                proposals
                |> List.map (fun p ->
                    match p.ProposalType with
                    | PlannedWorkOrder -> { p with CapacityCheckedDate = Some p.DueDate }
                    | _ -> p)

            let updatedCtx =
                ctx
                |> MrpContext.addEvent (CapacityCheckCompleted (List.length proposals))
                |> MrpContext.addWarning "Using infinite capacity planning (finite capacity checks disabled)"

            return Ok (updatedProposals, updatedCtx)
        }
