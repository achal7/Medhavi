module Medhavi.Scheduler.Mrp.Domain.Algorithms.Netting

open Medhavi.SharedKernel
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Mrp.Domain.Policies

type NettingSupplyProposal =
    { SkuId: SkuId
      NodeId: NodeId
      StockingPointId: StockingPointId
      Quantity: Quantity
      DueDate: Timestamp
      PeggingRefs: string list }

/// Calculate net available quantity for a simple snapshot
let calculateNetAvailable
    (onHand: Quantity)
    (inbound: Quantity)
    (reservations: Quantity)
    (safetyStock: Quantity)
    : Quantity =
    let totalAvailable = onHand + inbound
    let totalDeductions = reservations + safetyStock
    Quantity.subtract totalAvailable totalDeductions

/// Calculate time-phased netting and generate planned proposals for a SKU at a stocking point
let netDemands
    (skuId: SkuId)
    (nodeId: NodeId)
    (stockingPointId: StockingPointId)
    (initialOnHand: Quantity)
    (inboundSupplies: (Timestamp * Quantity * bool) list)
    (reservations: (Timestamp * Quantity) list)
    (safetyStock: Quantity)
    (demands: MrpDemand list)
    (policy: NettingPolicy)
    : NetRequirement list * NettingSupplyProposal list =

    let sortedDemands = demands |> List.sortBy (fun d -> d.RequiredDate)

    let sortedInbound =
        inboundSupplies
        |> List.sortBy (fun (t, _, _) -> t)

    let sortedReservations = reservations |> List.sortBy (fun (t, _) -> t)

    let rec loop
        (currentOnHand: Quantity)
        (inboundLeft: (Timestamp * Quantity * bool) list)
        (resLeft: (Timestamp * Quantity) list)
        (demandsLeft: MrpDemand list)
        (accNetReqs: NetRequirement list)
        (accProposals: NettingSupplyProposal list)
        =

        match demandsLeft with
        | [] -> (List.rev accNetReqs, List.rev accProposals)
        | demand :: restDemands ->
            let targetDate = demand.RequiredDate

            // 1. Accrue inbound supplies up to targetDate
            let (inboundToApply, inboundRemaining) =
                inboundLeft
                |> List.partition (fun (t, _, _) -> t <= targetDate)

            let inboundQty =
                inboundToApply
                |> List.map (fun (_, q, _) -> q)
                |> Quantity.sum

            // 2. Accrue reservations up to targetDate
            let (resToApply, resRemaining) =
                resLeft
                |> List.partition (fun (t, _) -> t <= targetDate)

            let resQty = resToApply |> List.map snd |> Quantity.sum

            // Calculate projected on-hand before satisfying this demand
            let onHandBeforeDemand =
                let withInbound = currentOnHand + inboundQty
                Quantity.subtract withInbound resQty

            // 3. Shortfall calculation: gross + safetyStock - onHandBeforeDemand
            let gross = demand.Quantity

            let shortfallVal =
                let grossVal = Quantity.value gross
                let safetyVal = Quantity.value safetyStock
                let ohVal = Quantity.value onHandBeforeDemand
                max 0m (grossVal + safetyVal - ohVal)

            let shortfall = Quantity.clampToZero shortfallVal

            if Quantity.isPositive shortfall then
                // Gather future demand quantities for lot sizing (like Silver-Meal / POQ)
                let futureDemands = restDemands |> List.map (fun d -> d.Quantity)

                // Apply lot sizing and constraints
                let lotSizedNet =
                    LotSizing.applyWithConstraints
                        policy.LotSizing
                        policy.MinOrderQty
                        policy.MaxOrderQty
                        shortfall
                        futureDemands

                let finalNet = lotSizedNet

                let proposal =
                    if Quantity.isPositive finalNet then
                        Some
                            { SkuId = skuId
                              NodeId = nodeId
                              StockingPointId = stockingPointId
                              Quantity = finalNet
                              DueDate = targetDate
                              PeggingRefs = [ demand.DemandId ] }
                    else
                        None

                let netReq =
                    { SkuId = skuId
                      NodeId = nodeId
                      StockingPointId = stockingPointId
                      GrossRequirement = gross
                      OnHand = onHandBeforeDemand
                      Inbound = inboundQty
                      Reservations = resQty
                      SafetyStock = safetyStock
                      NetRequirement = finalNet
                      RequiredDate = targetDate
                      BomPath = None
                      PeggingRefs = [ demand.DemandId ] }

                let newOnHand =
                    let withSupply = onHandBeforeDemand + finalNet
                    Quantity.subtract withSupply gross

                let newAccProposals =
                    match proposal with
                    | Some p -> p :: accProposals
                    | None -> accProposals

                loop newOnHand inboundRemaining resRemaining restDemands (netReq :: accNetReqs) newAccProposals
            else
                // No shortfall
                let netReq =
                    { SkuId = skuId
                      NodeId = nodeId
                      StockingPointId = stockingPointId
                      GrossRequirement = gross
                      OnHand = onHandBeforeDemand
                      Inbound = inboundQty
                      Reservations = resQty
                      SafetyStock = safetyStock
                      NetRequirement = Quantity.Zero
                      RequiredDate = targetDate
                      BomPath = None
                      PeggingRefs = [ demand.DemandId ] }

                let newOnHand = Quantity.subtract onHandBeforeDemand gross
                loop newOnHand inboundRemaining resRemaining restDemands (netReq :: accNetReqs) accProposals

    loop initialOnHand sortedInbound sortedReservations sortedDemands [] []
