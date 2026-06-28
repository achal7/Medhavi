namespace Medhavi.Terminal

open System
open System.Threading.Tasks
open Medhavi.SharedKernel
open Medhavi.MasterData
open Medhavi.MasterData.Application
open Medhavi.Supply
open Medhavi.Supply.Application
open Medhavi.Capacity
open Medhavi.Capacity.Application
open Medhavi.Capacity.Domain.CapacityResourceAgg
open Medhavi.Capacity.Domain.CapacityAgg
open Medhavi.Capacity.Domain.CapacityReservationAgg
open Medhavi.Capacity.Domain.OperationAgg
open Medhavi.Scheduler.Mrp.Domain
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Mrp.Domain.Policies
open Medhavi.Scheduler.Mrp.Domain.Algorithms
open Medhavi.Scheduler.Mrp.Domain.Errors
open Medhavi.Scheduler.Mrp.Domain.MrpRunAggregate
open Medhavi.Scheduler.Mrp.Steps
open Medhavi.Scheduler.Mrp.Application
open Medhavi.Scheduler.Mrp
open Medhavi.Contracts.Integration

module SchedulerWiring =

    let buildMrpDependencies
        (masterDataContext: Medhavi.MasterData.MasterData)
        (supplyContext: Medhavi.Supply.SupplyContext)
        (capacityContext: Medhavi.Capacity.CapacityContext)
        (demandContext: Medhavi.Demand.DemandContext)
        =

        let bomLookup: BomExplosion.BomLookup =
            fun skuId _ ->
                let boms = masterDataContext.Queries.Bom.GetAll().Result

                boms
                |> List.tryFind(fun b -> b.SkuId = SkuId.value skuId && b.Status)
                |> Option.map(fun b ->
                    { BomExplosion.BomRecord.BomId = b.Id
                      ParentSkuId = skuId
                      Components =
                        b.Items
                        |> List.map(fun item ->
                            { BomExplosion.BomComponent.ComponentSkuId =
                                SkuId.create item.ComponentSkuId |> Result.get
                              QuantityPer = Quantity.create item.Quantity |> Result.get
                              UnitOfMeasureId = UomId.create "UOM-PCS" |> Result.get
                              Sequence = item.Sequence
                              IsPhantom = false })
                      IsActive = b.Status })

        let onHandQuery: NettingStep.OnHandQuery =
            fun skuId spId ->
                task {
                    let! invs = supplyContext.Queries.Inventory.GetAll()

                    let matchOpt =
                        invs
                        |> List.tryFind(fun i ->
                            i.SkuId = SkuId.value skuId && i.StockingPointId = StockingPointId.value spId)

                    return
                        matchOpt
                        |> Option.map(fun i -> Quantity.create i.Quantity |> Result.get)
                        |> Option.defaultValue Quantity.Zero
                }

        let inboundQuery: NettingStep.InboundQuery =
            fun skuId spId start endT ->
                task {
                    let! orders = supplyContext.Queries.SupplyOrder.GetAll()

                    let matched =
                        orders
                        |> List.filter(fun o ->
                            o.SkuId = SkuId.value skuId
                            && o.StockingPointId = StockingPointId.value spId
                            && not(o.State.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                            && not(o.State.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)))

                    return
                        matched
                        |> List.map(fun o ->
                            let dueDate =
                                o.RequiredDeliveryDate
                                |> Option.map Timestamp.create
                                |> Option.defaultValue Timestamp.now

                            let qty = Quantity.create o.Quantity |> Result.get
                            let isFirm = o.IsFirm || o.IsLocked
                            (dueDate, qty, isFirm, o.Id))
                }

        let reservationsQuery: NettingStep.ReservationsQuery =
            fun skuId spId start endT ->
                task {
                    let! resvs = supplyContext.Queries.MaterialReservation.GetAll()

                    let matched =
                        resvs
                        |> List.filter(fun r ->
                            r.SkuId = SkuId.value skuId
                            && r.StockingPointId = StockingPointId.value spId
                            && r.State.Equals("Tentative", StringComparison.OrdinalIgnoreCase))

                    return
                        matched
                        |> List.map(fun r ->
                            (Timestamp.create r.RequiredDate, Quantity.create r.Quantity |> Result.get, r.Id))
                }

        let safetyStockQuery: NettingStep.SafetyStockQuery =
            fun skuId spId ->
                task {
                    let! targets = supplyContext.Queries.InventoryTarget.GetAll()

                    let matchOpt =
                        targets
                        |> List.tryFind(fun t ->
                            t.SkuId = SkuId.value skuId && t.StockingPointId = StockingPointId.value spId)

                    return
                        matchOpt
                        |> Option.bind(fun t -> t.SafetyStockQty)
                        |> Option.map(fun q -> Quantity.create q |> Result.get)
                        |> Option.defaultValue Quantity.Zero
                }

        let productTypeQuery: SupplyGenerationStep.ProductTypeQuery =
            fun skuId ->
                task {
                    let skuStr = SkuId.value skuId

                    if skuStr.Equals("SKU-BIKE", StringComparison.OrdinalIgnoreCase) then
                        return SupplyGenerationStep.Manufactured
                    elif skuStr.Equals("SKU-FRAME", StringComparison.OrdinalIgnoreCase) then
                        return SupplyGenerationStep.Manufactured
                    elif skuStr.Equals("SKU-WHEEL", StringComparison.OrdinalIgnoreCase) then
                        return SupplyGenerationStep.Purchased
                    else
                        return SupplyGenerationStep.Purchased
                }

        let supplierQuery: SupplyGenerationStep.SupplierQuery =
            fun skuId spId ->
                task {
                    let! offers = supplyContext.Queries.SupplierOffer.GetAll()

                    let offerOpt = offers |> List.tryFind(fun o -> o.SkuId = SkuId.value skuId && o.IsActive)

                    return offerOpt |> Option.map(fun o -> SupplierId.create o.SupplierId |> Result.get)
                }

        let routingQuery: SupplyGenerationStep.RoutingQuery =
            fun skuId spId ->
                task {
                    let! routings = masterDataContext.Queries.Routing.GetAll()

                    let matchOpt =
                        routings
                        |> List.tryFind(fun r ->
                            r.Status
                            && (r.Applicability.StockingPointId = None
                                || r.Applicability.StockingPointId = Some(StockingPointId.value spId))
                            && match r.Details with
                               | Medhavi.Contracts.Domain.RoutingDetails.Work w ->
                                   w.ProductId.Equals(SkuId.value skuId, StringComparison.OrdinalIgnoreCase)
                               | _ -> false)

                    return matchOpt |> Option.map(fun r -> RoutingId.create r.Id |> Result.get)
                }

        let transferSourceQuery: SupplyGenerationStep.TransferSourceQuery =
            fun skuId spId ->
                task {
                    let! legs = masterDataContext.Queries.TransportLeg.GetAll()

                    let matchOpt =
                        legs
                        |> List.tryFind(fun l ->
                            l.Status
                            && l.Destination.Equals(StockingPointId.value spId, StringComparison.OrdinalIgnoreCase))

                    return matchOpt |> Option.map(fun l -> StockingPointId.create l.Origin |> Result.get)
                }

        let capacityPromiseQuery: CapacityPromiseQuery =
            fun rgId desiredBucket neededDuration tentativeLoad ->
                task {
                    let resources = capacityContext.CapacityResourceAgent.GetStateAsync().Result

                    let calendars = capacityContext.CalendarAgent.GetStateAsync().Result

                    let buckets = capacityContext.CapacityAgent.GetStateAsync().Result

                    // Determine planning start date from buckets
                    let startDate =
                        buckets
                        |> Map.toList
                        |> List.map(fun (_, b) -> Timestamp.value b.Window.Start)
                        |> List.sort
                        |> List.tryHead
                        |> Option.defaultValue DateTimeOffset.UtcNow

                    let targetDateOfBucket (bIdx: BucketIndex) = startDate.AddDays(float bIdx)

                    // Find active resources belonging to the resource group
                    let groupResources =
                        resources
                        |> Map.toList
                        |> List.map snd
                        |> List.filter(fun r -> r.ResourceGroupId = rgId && r.IsActive)

                    // Calculate total available capacity for a specific bucket index (sum of resource group free minutes)
                    let getAvailableCapacity (bIdx: BucketIndex) =
                        let targetDate = targetDateOfBucket bIdx
                        let targetDateOnly = DateOnly.FromDateTime(targetDate.Date)

                        groupResources
                        |> List.map(fun r ->
                            buckets
                            |> Map.toList
                            |> List.map snd
                            |> List.tryFind(fun b ->
                                b.ResourceId = r.Id
                                && DateOnly.FromDateTime((Timestamp.value b.Window.Start).Date) = targetDateOnly)
                            |> Option.map(fun b -> DurationMinutes.value b.FreeMinutes)
                            |> Option.defaultValue 480.0m)
                        |> List.sum

                    let neededMins = DurationMinutes.value neededDuration

                    let getTentativeCommitment (bIdx: BucketIndex) =
                        tentativeLoad
                        |> Map.tryFind(rgId, bIdx)
                        |> Option.map DurationMinutes.value
                        |> Option.defaultValue 0.0m

                    // Scan forward to find the earliest feasible bucket
                    let horizonEndBucket = 365 // limit search to 1 year forward

                    let rec findFeasible bIdx =
                        if bIdx > horizonEndBucket then
                            { EarliestFeasibleBucket = bIdx
                              IsFeasible = false }
                        else
                            let avail = getAvailableCapacity bIdx
                            let tentative = getTentativeCommitment bIdx
                            let remaining = avail - tentative

                            if neededMins <= remaining then
                                { EarliestFeasibleBucket = bIdx
                                  IsFeasible = (bIdx <= desiredBucket) }
                            else
                                findFeasible(bIdx + 1)

                    return findFeasible desiredBucket
                }

        let capacityRoutingQuery: CapacityRoutingQuery =
            fun skuId spId routingIdOpt qty ->
                task {
                    let! routings = masterDataContext.Queries.Routing.GetAll()

                    let matchOpt =
                        match routingIdOpt with
                        | Some rId -> routings |> List.tryFind(fun r -> r.Id = RoutingId.value rId && r.Status)
                        | None ->
                            routings
                            |> List.filter(fun r ->
                                r.Status
                                && (r.Applicability.StockingPointId = None
                                    || r.Applicability.StockingPointId = Some(StockingPointId.value spId))
                                && match r.Details with
                                   | Medhavi.Contracts.Domain.RoutingDetails.Work work ->
                                       work.ProductId = SkuId.value skuId
                                   | _ -> false)
                            |> List.sortBy(fun r -> r.Preference.Priority)
                            |> List.tryHead

                    match matchOpt with
                    | None -> return None
                    | Some routing ->
                        let profile = RoutingAcl.translate routing

                        let allLoads = profile.StepLoads |> List.collect(fun s -> s.Loads)

                        let primaryLoadOpt =
                            allLoads
                            |> List.tryFind(fun l ->
                                match l.Target with
                                | LoadTarget.Resource(_, _) -> true
                                | _ -> false)
                            |> Option.orElse(allLoads |> List.tryHead)

                        match primaryLoadOpt with
                        | None -> return None
                        | Some load ->
                            let rgIdVal =
                                match load.Target with
                                | LoadTarget.Resource(rgId, _) -> rgId
                                | LoadTarget.WorkCenter(wcId, _) -> wcId

                            let rgId = ResourceGroupId.create rgIdVal |> Result.get

                            let setup = load.SetupLoadMinutes |> Option.defaultValue 0.0m
                            let teardown = load.TeardownLoadMinutes |> Option.defaultValue 0.0m
                            let baseQty = if profile.BaseQuantity <= 0.0m then 1.0m else profile.BaseQuantity
                            let runTime = load.RunLoadPerBaseQuantityMinutes * ((Quantity.value qty) / baseQty)
                            let totalMins = setup + runTime + teardown

                            let duration = DurationMinutes.create totalMins |> Result.defaultValue DurationMinutes.zero

                            return
                                Some
                                    { RoutingId = RoutingId.create routing.Id |> Result.get
                                      ResourceGroupId = rgId
                                      NeededDuration = duration }
                }

        let alternateRoutingsQuery: SkuId -> StockingPointId -> Task<RoutingId list> =
            fun skuId spId ->
                task {
                    let! list = masterDataContext.Queries.Routing.GetAll()

                    let filtered =
                        list
                        |> List.filter(fun r ->
                            r.Status
                            && (r.Applicability.StockingPointId = None
                                || r.Applicability.StockingPointId = Some(StockingPointId.value spId))
                            && match r.Details with
                               | Medhavi.Contracts.Domain.RoutingDetails.Work work ->
                                   work.ProductId = SkuId.value skuId
                               | _ -> false)
                        |> List.map(fun r -> RoutingId.create r.Id |> Result.get)

                    return filtered
                }

        let reservationCreator: PostprocessStep.ReservationCreator =
            fun skuId spId qty reqDate ->
                async {
                    let resId = $"RES-{Guid.NewGuid()}"

                    let req =
                        { Id = resId
                          IdempotencyKey = $"idem-{resId}"
                          SkuId = SkuId.value skuId
                          StockingPointId = StockingPointId.value spId
                          Quantity = Quantity.value qty
                          RequiredDate = Timestamp.value reqDate
                          ExpiryTime = (Timestamp.value reqDate).AddDays(30.0) }

                    let! res = supplyContext.Commands.MaterialReservation.CreateTentative req |> Async.AwaitTask

                    match res with
                    | Ok _ -> return Ok()
                    | Error err -> return Error(sprintf "%A" err)
                }

        let printColor (color: ConsoleColor) (msg: string) =
            let original = Console.ForegroundColor
            Console.ForegroundColor <- color
            Console.WriteLine(msg)
            Console.ForegroundColor <- original

        let createSupplyOrders: CreateSupplyOrders =
            fun runId proposals ->
                async {
                    let! activeResources = capacityContext.CapacityResourceAgent.GetStateAsync() |> Async.AwaitTask
                    let! bucketsState = capacityContext.CapacityAgent.GetStateAsync() |> Async.AwaitTask
                    let! allRoutings = masterDataContext.Queries.Routing.GetAll() |> Async.AwaitTask

                    let mutable allocations = Map.empty
                    let mutable scheduledCount = 0
                    let mutable violationCount = 0

                    let workOrders = proposals |> List.filter(fun p -> p.ProposalType = PlannedWorkOrder)

                    for p in workOrders do
                        let matchingRoutings =
                            allRoutings
                            |> List.filter(fun r ->
                                match r.Details with
                                | Medhavi.Contracts.Domain.RoutingDetails.Work work ->
                                    work.ProductId = SkuId.value p.SkuId
                                | _ -> false)

                        let selectedRoutingOpt =
                            match p.RoutingId with
                            | Some rId ->
                                matchingRoutings
                                |> List.filter(fun r -> r.Id = RoutingId.value rId)
                                |> List.map RoutingAcl.translate
                                |> List.tryHead
                            | None ->
                                let loadProfiles = matchingRoutings |> List.map RoutingAcl.translate

                                let preferredOpt =
                                    loadProfiles
                                    |> List.filter(fun r ->
                                        let raw = matchingRoutings |> List.find(fun rr -> rr.Id = r.RoutingId)
                                        raw.Preference.IsPreferred)
                                    |> List.tryHead

                                preferredOpt
                                |> Option.orElse(
                                    loadProfiles |> List.sortBy(fun r -> r.PreferencePriority) |> List.tryHead
                                )

                        match selectedRoutingOpt with
                        | None ->
                            printColor
                                ConsoleColor.Yellow
                                (sprintf
                                    "  [WARNING] No routing found for planned work order %s (Sku: %s)"
                                    (SupplyProposalId.value p.Id)
                                    (SkuId.value p.SkuId))
                        | Some routing ->
                            let req =
                                { WorkOrderId = SupplyProposalId.value p.Id
                                  ProductId = SkuId.value p.SkuId
                                  Quantity = Quantity.value p.Quantity
                                  DueDate = (Timestamp.value p.DueDate) }

                            match
                                FiniteCapacityScheduler.scheduleWorkOrder
                                    req
                                    routing
                                    activeResources
                                    bucketsState
                                    DateTimeOffset.UtcNow
                                    allocations
                            with
                            | Error err ->
                                printColor
                                    ConsoleColor.Red
                                    (sprintf "  [ERROR] Scheduling failed for %s: %A" (SupplyProposalId.value p.Id) err)
                            | Ok(capResult, nextAllocations) ->
                                allocations <- nextAllocations
                                scheduledCount <- scheduledCount + 1

                                let prodOrder = capResult.ProductionOrder
                                printColor ConsoleColor.Cyan "\n========================================="
                                printfn "PRODUCTION ORDER FINITE SCHEDULE"
                                printfn "Work Order ID : %s" prodOrder.WorkOrderId
                                printfn "Product ID    : %s" prodOrder.ProductId
                                printfn "Quantity      : %.2f" prodOrder.Quantity
                                printfn "Due Date      : %s" (prodOrder.DueDate.ToString("yyyy-MM-dd HH:mm"))
                                printfn "Outcome       : %A" capResult.Outcome
                                printfn "-----------------------------------------"

                                for op in prodOrder.Operations do
                                    printfn
                                        "  - Step %s (%s) on resource %s"
                                        op.StepId
                                        op.OperationCode
                                        (PhysicalResourceId.value op.ResourceId)

                                    printfn
                                        "    Window  : %s to %s"
                                        ((Timestamp.value op.Window.Start).ToString("yyyy-MM-dd HH:mm"))
                                        ((Timestamp.value op.Window.End).ToString("yyyy-MM-dd HH:mm"))

                                    printfn "    Duration: %.2f mins" op.DurationMinutes

                                if not capResult.Violations.IsEmpty then
                                    printfn "-----------------------------------------"
                                    printColor ConsoleColor.Red "PLANNING VIOLATIONS:"

                                    for v in capResult.Violations do
                                        violationCount <- violationCount + 1

                                        match v with
                                        | DueDateMiss(woId, start, now) ->
                                            printColor
                                                ConsoleColor.Red
                                                (sprintf
                                                    "  [VIOLATION] Due Date Miss: start time %s is in the past (now is %s)"
                                                    (start.ToString("yyyy-MM-dd HH:mm"))
                                                    (now.ToString("yyyy-MM-dd HH:mm")))
                                        | CapacityOverload(resId, bucketIdOpt, date, reqMins, availMins) ->
                                            let bIdStr = bucketIdOpt |> Option.defaultValue "N/A"
                                            let overload = reqMins - availMins

                                            printColor
                                                ConsoleColor.Red
                                                (sprintf
                                                    "  [VIOLATION] Capacity Overload on resource %s (Bucket: %s, Date: %s): requested %.2f mins, but only %.2f mins remaining (overloaded by %.2f mins)"
                                                    resId
                                                    bIdStr
                                                    (date.ToString("yyyy-MM-dd"))
                                                    reqMins
                                                    availMins
                                                    overload)
                                        | ResourceUnavailable resId ->
                                            printColor
                                                ConsoleColor.Red
                                                (sprintf "  [VIOLATION] Resource %s is unavailable" resId)
                                        | CalendarViolation(calId, date) ->
                                            printColor
                                                ConsoleColor.Red
                                                (sprintf
                                                    "  [VIOLATION] Calendar %s has a violation on date %s"
                                                    calId
                                                    (date.ToString("yyyy-MM-dd")))

                                printColor ConsoleColor.Cyan "=========================================\n"

                                for op in prodOrder.Operations do
                                    let opId = OperationId.create($"OP-{Guid.NewGuid().ToString()}") |> Result.get
                                    let stepId = RoutingStepId.create op.StepId |> Result.get
                                    let duration = (Timestamp.value op.Window.End) - (Timestamp.value op.Window.Start)

                                    let opWindow =
                                        { DateRange.Start = op.Window.Start
                                          End = Some op.Window.End }

                                    let scheduleOpCmd =
                                        { Id = opId
                                          SequenceNumber = op.SequenceNumber
                                          Window = opWindow
                                          RoutingStepId = stepId
                                          ResourceId = op.ResourceId
                                          Duration = duration
                                          IsFixed = false }

                                    let! opRes = capacityContext.Operation.Schedule scheduleOpCmd |> Async.AwaitTask
                                    ()

                                for resv in capResult.Reservations do
                                    let startTime =
                                        match resv.Start with
                                        | Some t -> Timestamp.value t
                                        | None -> DateTimeOffset.UtcNow

                                    let targetDate = DateOnly.FromDateTime(startTime.Date)

                                    let bucketOpt =
                                        bucketsState
                                        |> Map.toList
                                        |> List.map snd
                                        |> List.tryFind(fun b ->
                                            b.ResourceId = resv.ResourceId
                                            && let startVal = Timestamp.value b.Window.Start in
                                               DateOnly.FromDateTime(startVal.Date) = targetDate)

                                    let! _ =
                                        match bucketOpt with
                                        | Some _ -> async { return () }
                                        | None ->
                                            async {
                                                let dayStart =
                                                    DateTimeOffset(
                                                        startTime.Year,
                                                        startTime.Month,
                                                        startTime.Day,
                                                        0,
                                                        0,
                                                        0,
                                                        startTime.Offset
                                                    )

                                                let dayEnd = dayStart.AddDays(1.0)

                                                match TimeWindow.createFromTime dayStart dayEnd with
                                                | Error _ -> ()
                                                | Ok win ->
                                                    let! _ =
                                                        capacityContext.Capacity.DefineBucket(
                                                            resv.ResourceId,
                                                            win,
                                                            DurationMinutes.create 480.0m |> Result.get
                                                        )
                                                        |> Async.AwaitTask

                                                    ()
                                            }

                                    let reservationCmd =
                                        { CreateReservationCmd.Id = resv.Id
                                          RequirementId = resv.RequirementId
                                          ResourceId = resv.ResourceId
                                          BucketId = resv.BucketId
                                          Minutes = resv.Minutes
                                          Start = resv.Start
                                          End = resv.End
                                          Source = resv.Source
                                          Created = resv.CreatedAt }

                                    let! resvRes =
                                        capacityContext.CapacityReservation.Create reservationCmd |> Async.AwaitTask

                                    ()

                    let reqs =
                        proposals
                        |> List.map(fun p ->
                            let orderTypeStr =
                                match p.ProposalType with
                                | PlannedWorkOrder -> "workorder"
                                | PlannedPurchaseOrder -> "purchaseorder"
                                | PlannedTransferOrder -> "transportorder"

                            { SupplyOrderCreateReq.Id = SupplyProposalId.value p.Id
                              OrderType = orderTypeStr
                              SkuId = SkuId.value p.SkuId
                              StockingPointId = StockingPointId.value p.StockingPointId
                              Quantity = Quantity.value p.Quantity
                              UnitOfMeasure = "UOM-PCS"
                              RoutingId = p.RoutingId |> Option.map RoutingId.value
                              SupplierId = p.SupplierId |> Option.map SupplierId.value
                              IsFirm = false
                              IsExpedited = p.IsExpedite
                              IsLocked = false
                              UsesLeadTimeQuantity = false
                              RequiredDeliveryDate = Some(Timestamp.value p.DueDate)
                              CreatedDate = DateTimeOffset.UtcNow })

                    let! res = supplyContext.Commands.SupplyOrder.CreateBulk reqs |> Async.AwaitTask

                    match res with
                    | Ok _ ->
                        printColor
                            ConsoleColor.Green
                            (sprintf
                                "Finite scheduler run complete: %d production orders firmed, %d planning violations registered."
                                scheduledCount
                                violationCount)

                        return Ok()
                    | Error err -> return Error(sprintf "%A" err)
                }

        { BomLookup = bomLookup
          OnHandQuery = onHandQuery
          InboundQuery = inboundQuery
          ReservationsQuery = reservationsQuery
          SafetyStockQuery = safetyStockQuery
          ProductTypeQuery = productTypeQuery
          SupplierQuery = supplierQuery
          RoutingQuery = routingQuery
          TransferSourceQuery = transferSourceQuery
          CapacityPromiseQuery = capacityPromiseQuery
          CapacityRoutingQuery = capacityRoutingQuery
          AlternateRoutingsQuery = alternateRoutingsQuery
          PeggingCreator = None
          ReservationCreator = Some reservationCreator
          CreateSupplyOrders = createSupplyOrders }
