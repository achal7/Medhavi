/// Supply Generation Step — Generates PO/WO/TO proposals from net requirements
/// FP Pattern: Railway-Oriented Programming (ROP) with async pipelines
module Medhavi.Planning.Mrp.Steps.SupplyGenerationStep

open System
open Medhavi.SharedKernel
open Medhavi.Planning.Mrp.Domain.Types
open Medhavi.Planning.Mrp.Domain.Errors
open Medhavi.Planning.Mrp.Domain.Policies
open Medhavi.Planning.Mrp.Pipeline.PipelineTypes

// ============================================================================
// TYPES & QUERY SIGNATURES
// ============================================================================

type ProductType =
    | Manufactured
    | Purchased
    | Transferred
    | Unknown

type ProductTypeQuery = SkuId -> Async<ProductType>
type SupplierQuery = SkuId -> StockingPointId -> Async<SupplierId option>
type RoutingQuery = SkuId -> StockingPointId -> Async<RoutingId option>
type TransferSourceQuery = SkuId -> StockingPointId -> Async<StockingPointId option>

// ============================================================================
// PRIORITY & EXPEDITE HELPERS
// ============================================================================

let private calculatePriority (dueDate: Timestamp) (currentDate: Timestamp) (isExpedite: bool) : int =
    let due = Timestamp.value dueDate
    let current = Timestamp.value currentDate
    let daysUntilDue = (due - current).Days

    let basePriority =
        if daysUntilDue <= 0 then 10
        elif daysUntilDue <= 3 then 9
        elif daysUntilDue <= 7 then 7
        elif daysUntilDue <= 14 then 5
        elif daysUntilDue <= 30 then 3
        else 1

    if isExpedite then basePriority + 2 else basePriority

let private shouldExpedite (policy: ExpeditePolicy) (dueDate: Timestamp) (currentDate: Timestamp) : bool =
    let due = Timestamp.value dueDate
    let current = Timestamp.value currentDate
    let daysUntilDue = (due - current).Days

    match policy with
    | AlwaysExpedite -> true
    | NeverExpedite -> false
    | ExpediteIfUrgent -> daysUntilDue <= 7
    | ExpediteIfShortLeadTime days -> daysUntilDue <= days

// ============================================================================
// PROPOSAL GENERATION
// ============================================================================

let private generateProposal
    (productTypeQuery: ProductTypeQuery)
    (supplierQuery: SupplierQuery)
    (routingQuery: RoutingQuery)
    (transferSourceQuery: TransferSourceQuery)
    (runId: string)
    (policy: MrpPolicy)
    (netReq: NetRequirement)
    : Async<Result<SupplyProposal, SupplyGenerationError>> =
    async {
        let currentDate = Timestamp.now

        // Determine product procurement type
        let! prodType = productTypeQuery netReq.SkuId

        // Query source (routing or supplier or transfer source) based on product type
        let! (propType, routingIdOpt, supplierIdOpt) =
            async {
                match prodType with
                | Manufactured ->
                    let! routingOpt = routingQuery netReq.SkuId netReq.StockingPointId
                    match routingOpt with
                    | Some rid -> return (PlannedWorkOrder, Some rid, None)
                    | None ->
                        // Fallback to purchase if no routing defined
                        let! supplierOpt = supplierQuery netReq.SkuId netReq.StockingPointId
                        return (PlannedPurchaseOrder, None, supplierOpt)
                | Transferred ->
                    let! transferSourceOpt = transferSourceQuery netReq.SkuId netReq.StockingPointId
                    match transferSourceOpt with
                    | Some sourceSpId ->
                        let supplierId =
                            SupplierId.create (StockingPointId.value sourceSpId)
                            |> Result.defaultWith (fun _ -> failwith "Invalid ID mapping")
                        return (PlannedTransferOrder, None, Some supplierId)
                    | None ->
                        // Fallback to purchase if no transfer source defined
                        let! supplierOpt = supplierQuery netReq.SkuId netReq.StockingPointId
                        return (PlannedPurchaseOrder, None, supplierOpt)
                | Purchased | Unknown ->
                    let! supplierOpt = supplierQuery netReq.SkuId netReq.StockingPointId
                    return (PlannedPurchaseOrder, None, supplierOpt)
            }

        // Check if a source was successfully identified
        match propType, routingIdOpt, supplierIdOpt with
        | PlannedWorkOrder, None, None -> 
            return Error (NoRoutingFound (SkuId.value netReq.SkuId))
        | PlannedPurchaseOrder, None, None -> 
            return Error (NoSupplierFound (SkuId.value netReq.SkuId))
        | _ ->
            let isExpedite = shouldExpedite policy.ExpeditePolicy netReq.RequiredDate currentDate
            let priority = calculatePriority netReq.RequiredDate currentDate isExpedite
            
            // Deterministic proposal ID (idempotent run logic)
            let proposalId = SupplyProposalId.createDeterministic "mrp" (SkuId.value netReq.SkuId) (Timestamp.value netReq.RequiredDate)

            // Determine status via firming policy
            let status =
                match policy.Firming with
                | None -> Planned
                | Some firmingPolicy ->
                    if Firming.shouldAutoFirm firmingPolicy currentDate netReq.RequiredDate then Firmed
                    else Planned

            return
                Ok {
                    Id = proposalId
                    ProposalType = propType
                    SkuId = netReq.SkuId
                    NodeId = netReq.NodeId
                    StockingPointId = netReq.StockingPointId
                    Quantity = netReq.NetRequirement
                    DueDate = netReq.RequiredDate
                    StartDate = None
                    RoutingId = routingIdOpt
                    SupplierId = supplierIdOpt
                    Priority = priority
                    IsExpedite = isExpedite
                    Status = status
                    PeggingRefs = []
                    CapacityCheckedDate = None
                    CreatedAt = currentDate
                }
    }

/// Check frozen horizon and emit warn action messages if needed
let private checkFrozenHorizon
    (policy: FrozenHorizonPolicy option)
    (proposal: SupplyProposal)
    (currentDate: Timestamp)
    : ActionMessageRecord option =
    match policy with
    | None -> None
    | Some horizonPolicy ->
        match FrozenHorizon.getZone horizonPolicy currentDate proposal.DueDate with
        | Frozen ->
            Some {
                Id = $"action-{SupplyProposalId.value proposal.Id}-frozen"
                Message = ActionMessage.Expedite (SupplyProposalId.value proposal.Id, "Proposal falls in frozen horizon", 0)
                SkuId = proposal.SkuId
                StockingPointId = proposal.StockingPointId
                Severity = Critical
                CreatedAt = currentDate
                AcknowledgedAt = None
            }
        | Slushy ->
            Some {
                Id = $"action-{SupplyProposalId.value proposal.Id}-slushy"
                Message = ActionMessage.Reschedule (SupplyProposalId.value proposal.Id, proposal.DueDate, proposal.DueDate, "Proposal in slushy zone - requires approval")
                SkuId = proposal.SkuId
                StockingPointId = proposal.StockingPointId
                Severity = Warning
                CreatedAt = currentDate
                AcknowledgedAt = None
            }
        | Free -> None

// ============================================================================
// STEP CREATION
// ============================================================================

/// Create supply generation step
let createStep
    (productTypeQuery: ProductTypeQuery)
    (supplierQuery: SupplierQuery)
    (routingQuery: RoutingQuery)
    (transferSourceQuery: TransferSourceQuery)
    : MrpStepAsync<NetRequirement list, SupplyProposal list> =
    fun netRequirements ctx ->
        async {
            let startTime = Timestamp.now

            // Generate proposals in parallel
            let! results =
                netRequirements
                |> List.map (generateProposal productTypeQuery supplierQuery routingQuery transferSourceQuery (MrpRunId.value ctx.RunId) ctx.Policy)
                |> Async.Parallel

            let proposals = results |> Array.choose (function Ok p -> Some p | _ -> None) |> List.ofArray
            let errors = results |> Array.choose (function Error e -> Some e | _ -> None) |> List.ofArray

            // Action messages for frozen horizon checks
            let actionMessages =
                proposals
                |> List.choose (fun p -> checkFrozenHorizon ctx.Policy.FrozenHorizon p startTime)

            if not (List.isEmpty errors) && List.isEmpty proposals then
                return Error (SupplyGeneration errors)
            else
                let updatedCtx =
                    proposals
                    |> List.fold (fun c p -> MrpContext.addEvent (SupplyProposalCreated p) c) ctx
                    |> (fun c ->
                        actionMessages
                        |> List.fold (fun c' am ->
                            c'
                            |> MrpContext.addActionMessage am
                            |> MrpContext.addEvent (ActionMessageGenerated am))
                            c)
                    |> MrpContext.updateTelemetry (fun t ->
                        { t with ProposalsGenerated = t.ProposalsGenerated + List.length proposals })
                    |> (fun c ->
                        errors
                        |> List.fold (fun c' err ->
                            match err with
                            | NoSupplierFound sku -> MrpContext.addWarning $"No supplier found for SKU {sku}" c'
                            | NoRoutingFound sku -> MrpContext.addWarning $"No routing found for SKU {sku}" c'
                            | _ -> c')
                            c)

                return Ok (proposals, updatedCtx)
        }
