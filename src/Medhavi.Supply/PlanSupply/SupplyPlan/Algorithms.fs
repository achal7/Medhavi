/// SE-S-020 — Supply Planning Business Algorithms (Chapter 10)
/// Traces to: BA-S-015 (Balance Supply and Demand)
module Medhavi.Supply.PlanSupply.SupplyPlan.Algorithms

open System
open Medhavi.SemanticModel
open Medhavi.Supply
open Model
open Policies


/// Input contract for BA-S-015
type BalancingInput =
    { DemandForecast: DemandForecastFact list
      InitialInventory: Map<ItemId * LocationId, decimal>
      SafetyStockBuffers: Map<ItemId * LocationId, decimal>
      BillOfMaterials: BomComponentFact list
      SourcingAllocations: SourcingAllocationFact list
      ResourceCapacities: Map<PhysicalResourceId * int, decimal>
      DefaultUnitOfMeasure: UnitOfMeasureId
      CurrentTimestamp: Timestamp }

/// Output contract of BA-S-015
type BalancingResult =
    { PlannedOrders: PlannedSupplyOrder list
      ProjectedShortfalls: ShortfallRecord list
      ConstraintViolations: ConstraintViolationRecord list
      FeasibilityStatus: PlanFeasibilityStatus
      ServiceLevelAchieved: decimal
      MaxCapacityUtilization: decimal
      PlanStabilityScore: decimal
      PlanConfidenceScore: decimal }

type private SimulationState =
    { PlannedOrders: PlannedSupplyOrder list
      Shortfalls: ShortfallRecord list
      OnHandTracking: Map<ItemId * LocationId, decimal>
      OrderSeq: int }

/// BA-S-015: Balance Supply and Demand (Pure Stage 5 Constructive Functional Algorithm)
let balanceSupplyAndDemand
    (_: ConstraintRelaxationPolicy)
    (previousVersionOpt: SupplyPlanVersion option)
    (input: BalancingInput)
    : BalancingResult =

    // 1. Topological multi-echelon demand explosion
    let dependentDemands =
        input.DemandForecast
        |> List.collect (fun df ->
            let parentItem = df.Item
            let components =
                input.BillOfMaterials
                |> List.filter (fun bom -> bom.ParentItem = parentItem)

            components
            |> List.map (fun comp ->
                let grossQty =
                    (Quantity.value df.Quantity) * comp.UsageQuantity * comp.ScrapFactor

                let childNeedTime =
                    let span = TimeSpan.FromDays(float comp.ProductionLeadTimeDays)
                    Timestamp.create ((Timestamp.value df.NeedDate).Subtract span)
                    |> Result.defaultWith (fun _ -> input.CurrentTimestamp)

                let childQty =
                    Quantity.create grossQty
                    |> Result.defaultWith (fun _ -> Quantity.zero)

                { Item = comp.ComponentItem
                  Location = df.Location
                  PeriodBucket = max 1 (df.PeriodBucket - 1)
                  Quantity = childQty
                  NeedDate = childNeedTime }))

    let allGrossDemands =
        input.DemandForecast @ dependentDemands

    // 2. Multi-echelon netting and planned order generation
    let demandsByItemLocPeriod =
        allGrossDemands
        |> List.groupBy (fun d -> (d.Item, d.Location, d.PeriodBucket))

    let sortedBuckets =
        demandsByItemLocPeriod
        |> List.sortBy (fun ((_, _, bucket), _) -> bucket)

    let initialSimState: SimulationState =
        { PlannedOrders = []
          Shortfalls = []
          OnHandTracking = input.InitialInventory
          OrderSeq = 1 }

    let finalSimState =
        sortedBuckets
        |> List.fold (fun state ((item, loc, period), demands) ->
            let grossDemandQty =
                demands |> List.sumBy (fun d -> Quantity.value d.Quantity)

            let currentOnHand =
                state.OnHandTracking |> Map.tryFind (item, loc) |> Option.defaultValue 0m

            let safetyStock =
                input.SafetyStockBuffers |> Map.tryFind (item, loc) |> Option.defaultValue 0m

            let availableBeforeNetting = currentOnHand
            let netRequirement = max 0m (grossDemandQty + safetyStock - availableBeforeNetting)

            let needDate =
                demands
                |> List.tryHead
                |> Option.map (fun d -> d.NeedDate)
                |> Option.defaultValue input.CurrentTimestamp

            if netRequirement > 0m then
                let isManufactured =
                    input.BillOfMaterials |> List.exists (fun b -> b.ParentItem = item)

                let sourcingOptions =
                    input.SourcingAllocations |> List.filter (fun s -> s.Item = item)

                if isManufactured then
                    let orderId =
                        PlannedSupplyOrderId.create (sprintf "ORD-PROD-%d" state.OrderSeq)
                        |> Result.defaultValue PlannedSupplyOrderId.defaultId

                    let order: PlannedSupplyOrder =
                        { OrderId = orderId
                          Item = item
                          Location = loc
                          SupplyType = PlannedSupplyType.Production
                          Quantity = Quantity.create netRequirement |> Result.defaultWith (fun _ -> Quantity.zero)
                          UnitOfMeasure = input.DefaultUnitOfMeasure
                          ReleaseDate = input.CurrentTimestamp
                          DueDate = needDate
                          AssignedResource = None
                          AssignedSupplier = None
                          SourceLocation = None }

                    { state with
                        PlannedOrders = order :: state.PlannedOrders
                        OnHandTracking = Map.add (item, loc) (currentOnHand + netRequirement - grossDemandQty) state.OnHandTracking
                        OrderSeq = state.OrderSeq + 1 }

                elif not sourcingOptions.IsEmpty then
                    let newOrders, nextSeq =
                        sourcingOptions
                        |> List.fold (fun (orders, seq) s ->
                            let allocatedQty = netRequirement * s.AllocationRatio
                            if allocatedQty > 0m then
                                let orderId =
                                    PlannedSupplyOrderId.create (sprintf "ORD-PROC-%d" seq)
                                    |> Result.defaultValue PlannedSupplyOrderId.defaultId

                                let order: PlannedSupplyOrder =
                                    { OrderId = orderId
                                      Item = item
                                      Location = loc
                                      SupplyType = PlannedSupplyType.Procurement
                                      Quantity = Quantity.create allocatedQty |> Result.defaultWith (fun _ -> Quantity.zero)
                                      UnitOfMeasure = input.DefaultUnitOfMeasure
                                      ReleaseDate = input.CurrentTimestamp
                                      DueDate = needDate
                                      AssignedResource = None
                                      AssignedSupplier = Some s.Supplier
                                      SourceLocation = None }

                                (order :: orders, seq + 1)
                            else
                                (orders, seq)
                        ) ([], state.OrderSeq)

                    { state with
                        PlannedOrders = newOrders @ state.PlannedOrders
                        OnHandTracking = Map.add (item, loc) (currentOnHand + netRequirement - grossDemandQty) state.OnHandTracking
                        OrderSeq = nextSeq }
                else
                    let shortfall: ShortfallRecord =
                        { Item = item
                          Location = loc
                          PeriodBucket = period
                          ShortfallQuantity = Quantity.create netRequirement |> Result.defaultWith (fun _ -> Quantity.zero)
                          UnitOfMeasure = input.DefaultUnitOfMeasure
                          Reason = "No qualified BOM or sourcing allocation available" }

                    { state with
                        Shortfalls = shortfall :: state.Shortfalls
                        OnHandTracking = Map.add (item, loc) 0m state.OnHandTracking }
            else
                { state with
                    OnHandTracking = Map.add (item, loc) (currentOnHand - grossDemandQty) state.OnHandTracking }
        ) initialSimState

    let plannedOrders = List.rev finalSimState.PlannedOrders
    let shortfalls = List.rev finalSimState.Shortfalls

    // 3. Capacity verification
    let constraintViolations =
        input.ResourceCapacities
        |> Map.toList
        |> List.choose (fun ((res, bucket), maxCap) ->
            let totalPeriodProduction =
                plannedOrders
                |> List.filter (fun o -> o.SupplyType = PlannedSupplyType.Production)
                |> List.sumBy (fun o -> Quantity.value o.Quantity)

            let requiredLoad = totalPeriodProduction * 0.5m
            if requiredLoad > maxCap then
                Some { Resource = res
                       PeriodBucket = bucket
                       RequiredLoad = requiredLoad
                       MaxPlannedCapacity = maxCap
                       OverloadQuantity = requiredLoad - maxCap
                       UnitOfMeasure = input.DefaultUnitOfMeasure
                       IsRelaxationApproved = false
                       RelaxationApprovalReference = None }
            else
                None)

    // 4. Performance metrics
    let totalGrossVolume =
        allGrossDemands |> List.sumBy (fun d -> Quantity.value d.Quantity)

    let totalShortfallVolume =
        shortfalls |> List.sumBy (fun s -> Quantity.value s.ShortfallQuantity)

    let serviceLevel =
        if totalGrossVolume = 0m then 1.0m
        else max 0m (min 1.0m ((totalGrossVolume - totalShortfallVolume) / totalGrossVolume))

    let maxCapacityUtilization =
        if input.ResourceCapacities.IsEmpty then 0.75m
        else
            input.ResourceCapacities
            |> Map.toList
            |> List.map (fun ((_, _), maxCap) ->
                let totalProd =
                    plannedOrders
                    |> List.filter (fun o -> o.SupplyType = PlannedSupplyType.Production)
                    |> List.sumBy (fun o -> Quantity.value o.Quantity)
                let req = totalProd * 0.5m
                if maxCap > 0m then req / maxCap else 0m)
            |> List.max

    let planStabilityScore =
        match previousVersionOpt with
        | None -> 1.0m // First publication is 100% stable per BR-S-131
        | Some prev ->
            let prevCount = decimal prev.PlannedOrders.Length
            let curCount = decimal plannedOrders.Length
            if prevCount = 0m && curCount = 0m then 1.0m
            elif prevCount = 0m || curCount = 0m then 0.5m
            else
                let ratio = min prevCount curCount / max prevCount curCount
                max 0m (min 1.0m ratio)

    let hasViolations = not constraintViolations.IsEmpty || not shortfalls.IsEmpty

    let feasibilityStatus =
        if hasViolations then PlanFeasibilityStatus.ConstrainedInfeasible
        else PlanFeasibilityStatus.FullyFeasible

    let planConfidenceScore =
        let baseScore = serviceLevel * 0.6m + (min 1.0m (1.0m / (max 1.0m maxCapacityUtilization))) * 0.2m + planStabilityScore * 0.2m
        max 0.10m (min 1.0m baseScore)

    { PlannedOrders = plannedOrders
      ProjectedShortfalls = shortfalls
      ConstraintViolations = constraintViolations
      FeasibilityStatus = feasibilityStatus
      ServiceLevelAchieved = serviceLevel
      MaxCapacityUtilization = maxCapacityUtilization
      PlanStabilityScore = planStabilityScore
      PlanConfidenceScore = planConfidenceScore }
