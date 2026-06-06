namespace Medhavi.Scheduler.Mrp.Domain

open System
open Medhavi.SharedKernel
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Mrp.Domain.MrpRunAggregate

type DisruptionEvent =
    | ResourceBreakdown of resourceId: string * startTime: Timestamp * endTime: Timestamp
    | MaterialDelay of skuId: SkuId * stockingPointId: StockingPointId * newArrival: Timestamp * affectedSupplyId: string
    | MesScrapVariance of workOrderId: string * skuId: SkuId * scrapQty: Quantity

type PlanningMode =
    | ReactiveRepair
    | IncrementalInsert
    | FullReplan
    | Ignore

type PlanDelta =
    { AddedProposals: SupplyProposal list
      RescheduledProposals: (string * Timestamp) list // ProposalId * NewDueDate
      CancelledProposals: string list
      UpdatedPeggings: PeggingLink list }

type ReplanKpis =
    { TotalLatenessMinutes: float
      LateOrdersCount: int
      ScheduleChurnCount: int }

// Component Lookup function: SkuId -> (componentSkuId * Quantity) list
type ComponentLookup = SkuId -> (SkuId * Quantity) list

module Replan =

    module ImpactAssessment =
        
        let evaluateBlastRadius
            (baseline: MrpRunResult)
            (event: DisruptionEvent)
            (componentLookup: ComponentLookup)
            : string list * string list = // affectedDemandIds * affectedProposalIds
            
            // Helper to check if a proposal overlaps with resource breakdown
            let isAffectedByResource resourceId start endT (p: SupplyProposal) =
                match p.ProposalType with
                | PlannedWorkOrder ->
                    let pStart = p.StartDate |> Option.defaultValue p.DueDate
                    let overlap = (pStart <= endT) && (p.DueDate >= start)
                    let matchesRouting = 
                        p.RoutingId 
                        |> Option.map (RoutingId.value >> (fun r -> r.Equals(resourceId, StringComparison.OrdinalIgnoreCase))) 
                        |> Option.defaultValue false
                    let matchesSku = 
                        SkuId.value p.SkuId |> (fun s -> s.Equals(resourceId, StringComparison.OrdinalIgnoreCase))
                    overlap && (matchesRouting || matchesSku)
                | _ -> false

            // Identify initial direct affected proposals
            let initialProposals =
                match event with
                | ResourceBreakdown(resId, start, endT) ->
                    baseline.Proposals
                    |> List.filter (isAffectedByResource resId start endT)
                    |> List.map (fun p -> SupplyProposalId.value p.Id)
                | MaterialDelay(_, _, _, supplyId) ->
                    [ supplyId ]
                | MesScrapVariance(woId, _, _) ->
                    [ woId ]

            let rec findDownstream
                (currentAffectedProposals: string list)
                (visitedProposals: Set<string>)
                (visitedDemands: Set<string>) =
                
                if List.isEmpty currentAffectedProposals then
                    (visitedDemands, visitedProposals)
                else
                    // Find peggings where Target is a Supply matching any of the currentAffectedProposals
                    let directPegs =
                        baseline.Peggings
                        |> List.filter (fun p ->
                            match p.Target with
                            | Supply s -> List.contains s.SupplyId currentAffectedProposals
                            | Reservation _ -> false)

                    let directAffectedDemands =
                        directPegs
                        |> List.map (fun p -> p.Demand.DemandId)
                        |> List.filter (fun id -> not (visitedDemands.Contains id))
                        |> List.distinct

                    let nextVisitedDemands = visitedDemands |> Set.union (Set.ofList directAffectedDemands)
                    let nextVisitedProposals = visitedProposals |> Set.union (Set.ofList currentAffectedProposals)

                    // Find parent proposals that consume the outputs of currentAffectedProposals (via BOM lookup)
                    let affectedSkus =
                        baseline.Proposals
                        |> List.filter (fun p -> List.contains (SupplyProposalId.value p.Id) currentAffectedProposals)
                        |> List.map (fun p -> p.SkuId)
                        |> List.distinct

                    let parentProposals =
                        baseline.Proposals
                        |> List.filter (fun parent ->
                            match parent.ProposalType with
                            | PlannedWorkOrder ->
                                let parentComponents = componentLookup parent.SkuId
                                parentComponents
                                |> List.exists (fun (compSku, _) -> List.contains compSku affectedSkus)
                            | _ -> false)
                        |> List.map (fun p -> SupplyProposalId.value p.Id)
                        |> List.filter (fun id -> not (nextVisitedProposals.Contains id))
                    
                    let nextProposalsToEvaluate = List.distinct parentProposals
                    findDownstream nextProposalsToEvaluate nextVisitedProposals nextVisitedDemands

            let (affectedDemands, affectedProposals) =
                findDownstream initialProposals Set.empty Set.empty

            (Set.toList affectedDemands, Set.toList affectedProposals)

    module ReplanDispatcher =
        
        let determineMode
            (event: DisruptionEvent)
            (severityThresholds: Map<string, float>)
            : PlanningMode =
            
            match event with
            | ResourceBreakdown(_, start, endT) ->
                let durationHrs = (Timestamp.value endT - Timestamp.value start).TotalHours
                let fullReplanThreshold = Map.tryFind "fullReplanDurationHrs" severityThresholds |> Option.defaultValue 24.0
                let ignoreThreshold = Map.tryFind "ignoreDurationHrs" severityThresholds |> Option.defaultValue 1.0
                
                if durationHrs >= fullReplanThreshold then
                    FullReplan
                elif durationHrs <= ignoreThreshold then
                    Ignore
                else
                    ReactiveRepair
                    
            | MaterialDelay(_, _, newArrival, _) ->
                let delayHrs = (Timestamp.value newArrival - DateTimeOffset.UtcNow).TotalHours
                let fullReplanThreshold = Map.tryFind "fullReplanDelayHrs" severityThresholds |> Option.defaultValue 48.0
                let ignoreThreshold = Map.tryFind "ignoreDelayHrs" severityThresholds |> Option.defaultValue 2.0
                
                if delayHrs >= fullReplanThreshold then
                    FullReplan
                elif delayHrs <= ignoreThreshold then
                    Ignore
                else
                    ReactiveRepair
                    
            | MesScrapVariance(_, _, scrapQty) ->
                let qtyVal = Quantity.value scrapQty
                let fullReplanThreshold = Map.tryFind "fullReplanScrapQty" severityThresholds |> Option.defaultValue 500.0 |> decimal
                let ignoreThreshold = Map.tryFind "ignoreScrapQty" severityThresholds |> Option.defaultValue 5.0 |> decimal
                
                if qtyVal >= fullReplanThreshold then
                    FullReplan
                elif qtyVal <= ignoreThreshold then
                    Ignore
                else
                    ReactiveRepair

    module KPIEvaluator =
        
        let evaluate (result: MrpRunResult) (baseline: MrpRunResult option) : ReplanKpis =
            let customerPegs =
                result.Peggings
                |> List.filter (fun p -> p.Status = PegStatus.Active && not (p.Demand.DemandId.StartsWith("comp-")))

            let latePegs =
                customerPegs
                |> List.filter (fun p ->
                    match p.Target with
                    | Supply s -> s.DeliveryDate > p.Demand.NeedDate
                    | Reservation _ -> false)

            let totalLateness =
                latePegs
                |> List.map (fun p ->
                    match p.Target with
                    | Supply s -> (Timestamp.value s.DeliveryDate - Timestamp.value p.Demand.NeedDate).TotalMinutes
                    | Reservation _ -> 0.0)
                |> List.sum

            let scheduleChurn =
                match baseline with
                | None -> 0
                | Some baseRun ->
                    let baseProposals = baseRun.Proposals |> List.map (fun p -> SupplyProposalId.value p.Id, p.DueDate) |> Map.ofList
                    result.Proposals
                    |> List.filter (fun p ->
                        let pId = SupplyProposalId.value p.Id
                        match Map.tryFind pId baseProposals with
                        | Some baseDueDate -> baseDueDate <> p.DueDate
                        | None -> true)
                    |> List.length

            { TotalLatenessMinutes = totalLateness
              LateOrdersCount = List.length latePegs
              ScheduleChurnCount = scheduleChurn }

    module PlanDeltaCalculator =
        
        let calculate (before: MrpRunResult) (after: MrpRunResult) : PlanDelta =
            let beforeMap = before.Proposals |> List.map (fun p -> SupplyProposalId.value p.Id, p) |> Map.ofList
            let afterMap = after.Proposals |> List.map (fun p -> SupplyProposalId.value p.Id, p) |> Map.ofList

            let added =
                after.Proposals
                |> List.filter (fun p -> not (Map.containsKey (SupplyProposalId.value p.Id) beforeMap))

            let rescheduled =
                after.Proposals
                |> List.choose (fun p ->
                    let pId = SupplyProposalId.value p.Id
                    match Map.tryFind pId beforeMap with
                    | Some beforeP when beforeP.DueDate <> p.DueDate -> Some(pId, p.DueDate)
                    | _ -> None)

            let cancelled =
                before.Proposals
                |> List.filter (fun p -> not (Map.containsKey (SupplyProposalId.value p.Id) afterMap))
                |> List.map (fun p -> SupplyProposalId.value p.Id)

            let updatedPeggings =
                after.Peggings
                |> List.filter (fun p ->
                    let matchedBefore = before.Peggings |> List.tryFind (fun bp -> bp.Id = p.Id)
                    match matchedBefore with
                    | Some bp -> bp.PeggedQty <> p.PeggedQty || bp.Status <> p.Status
                    | None -> true)

            { AddedProposals = added
              RescheduledProposals = rescheduled
              CancelledProposals = cancelled
              UpdatedPeggings = updatedPeggings }
