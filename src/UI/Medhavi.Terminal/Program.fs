namespace Medhavi.Terminal

open System
open System.Threading.Tasks
open Medhavi.SharedKernel
open Medhavi.SharedKernel.BoundedContexts
open Medhavi.MasterData
open Medhavi.MasterData.Application
open Medhavi.Integration
open Medhavi.Supply
open Medhavi.Supply.Application
open Medhavi.Infrastructure.Stores.EnvelopeStore
open Medhavi.Infrastructure.Stores.EnvelopeStoreMem
open Medhavi.Contracts.Integration
open Medhavi.Capacity
open Medhavi.Capacity.Application
open Medhavi.Capacity.Domain.CapacityResourceAgg
open Medhavi.Capacity.Domain.CapacityAgg
open Medhavi.Infrastructure.Projections
open Medhavi.Transport
open Medhavi.Transport.Application
open Medhavi.Scheduler.Mrp.Domain
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Mrp.Domain.MrpRunAggregate
open Medhavi.Scheduler.Mrp.Domain.Policies
open Medhavi.Scheduler.Mrp.Domain.Algorithms
open Medhavi.Scheduler.Mrp.Domain.Errors
open Medhavi.Scheduler.Mrp.Steps
open Medhavi.Scheduler.Mrp.Application
open Medhavi.Scheduler.Mrp

[<AutoOpen>]
module ResultExtensions =
    module Result =
        let get = function
            | Ok x -> x
            | Error e -> failwithf "Expected Ok, got Error: %A" e

module Program =
    open Medhavi.Common.Patterns

    // ANSI Colors for Rich Typography
    let printColor color text =
        let code =
            match color with
            | "green" -> "\u001b[32m"
            | "cyan" -> "\u001b[36m"
            | "yellow" -> "\u001b[33m"
            | "red" -> "\u001b[31m"
            | "bold" -> "\u001b[1m"
            | "reset" -> "\u001b[0m"
            | _ -> ""

        printf "%s%s\u001b[0m" code text

    let printColorLine color text =
        printColor color text
        printfn ""

    // Initialize Bounded Contexts via modular composition roots
    let masterDataContext = Medhavi.MasterData.BoundedContext.create ()
    let supplyContext = Medhavi.Supply.BoundedContext.create ()
    let capacityContext = Medhavi.Capacity.BoundedContext.create ()

    // Transport context: legs are loaded from MasterData's projection on demand
    let getTransportLegs () =
        async {
            let! legs =
                masterDataContext.Queries.TransportLeg.GetAll()
                |> Async.AwaitTask

            return
                legs
                |> List.filter (fun l -> l.Status)
                |> List.map (fun l ->
                    { LegId = l.Id
                      Origin = l.Origin
                      Destination = l.Destination
                      Mode = l.Mode
                      LeadTimeMinutes = l.LeadTimeMinutes
                      Capacity = l.Capacity
                      CapacityUnit = l.CapacityUnit
                      Reliability = None // enrichable from full domain leg
                      CO2PerUnit = None
                      FixedCost = 0.0m
                      VariableCostPerUnit = None
                      Status = l.Status }
                    : Medhavi.Transport.TransportLegRef)
        }

    let transportContext = Medhavi.Transport.BoundedContext.create getTransportLegs

    // Real EnvelopeStore instance (Mailbox-agent based)
    let envelopeStore = createEnvelopeStoreMem ()
    let mutable subscriptionHandle: SubscriptionHandle option = None
    let mutable latestMrpRun: MrpRunResult option = None

    let bomLookup : BomExplosion.BomLookup =
        fun skuId _ ->
            let boms = masterDataContext.Queries.Bom.GetAll().Result
            boms
            |> List.tryFind (fun b -> b.SkuId = SkuId.value skuId && b.Status)
            |> Option.map (fun b ->
                { BomExplosion.BomRecord.BomId = b.Id
                  ParentSkuId = skuId
                  Components =
                      b.Items
                      |> List.map (fun item ->
                          { BomExplosion.BomComponent.ComponentSkuId = SkuId.create item.ComponentSkuId |> Result.get
                            QuantityPer = Quantity.create item.Quantity |> Result.get
                            UnitOfMeasureId = UomId.create "UOM-PCS" |> Result.get
                            Sequence = item.Sequence
                            IsPhantom = false })
                  IsActive = b.Status })

    let onHandQuery : NettingStep.OnHandQuery =
        fun skuId spId ->
            task {
                let! invs = supplyContext.Queries.Inventory.GetAll()
                let matchOpt = invs |> List.tryFind (fun i -> i.SkuId = SkuId.value skuId && i.StockingPointId = StockingPointId.value spId)
                return matchOpt |> Option.map (fun i -> Quantity.create i.Quantity |> Result.get) |> Option.defaultValue Quantity.Zero
            }

    let inboundQuery : NettingStep.InboundQuery =
        fun skuId spId start endT ->
            task {
                let! orders = supplyContext.Queries.SupplyOrder.GetAll()
                let matched =
                    orders
                    |> List.filter (fun o ->
                        o.SkuId = SkuId.value skuId &&
                        o.StockingPointId = StockingPointId.value spId &&
                        not (o.State.Equals("Completed", StringComparison.OrdinalIgnoreCase)) &&
                        not (o.State.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)))
                return matched |> List.map (fun o ->
                    let dueDate =
                        o.RequiredDeliveryDate
                        |> Option.map Timestamp.create
                        |> Option.defaultValue Timestamp.now
                    let qty = Quantity.create o.Quantity |> Result.get
                    let isFirm = o.IsFirm || o.IsLocked
                    (dueDate, qty, isFirm, o.Id))
            }

    let reservationsQuery : NettingStep.ReservationsQuery =
        fun skuId spId start endT ->
            task {
                let! resvs = supplyContext.Queries.MaterialReservation.GetAll()
                let matched =
                    resvs
                    |> List.filter (fun r ->
                        r.SkuId = SkuId.value skuId &&
                        r.StockingPointId = StockingPointId.value spId &&
                        r.State.Equals("Tentative", StringComparison.OrdinalIgnoreCase))
                return matched |> List.map (fun r ->
                    (Timestamp.create r.RequiredDate, Quantity.create r.Quantity |> Result.get, r.Id))
            }

    let safetyStockQuery : NettingStep.SafetyStockQuery =
        fun skuId spId ->
            task {
                let! targets = supplyContext.Queries.InventoryTarget.GetAll()
                let matchOpt = targets |> List.tryFind (fun t -> t.SkuId = SkuId.value skuId && t.StockingPointId = StockingPointId.value spId)
                return matchOpt |> Option.bind (fun t -> t.SafetyStockQty) |> Option.map (fun q -> Quantity.create q |> Result.get) |> Option.defaultValue Quantity.Zero
            }

    let productTypeQuery : SupplyGenerationStep.ProductTypeQuery =
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

    let supplierQuery : SupplyGenerationStep.SupplierQuery =
        fun skuId spId ->
            task {
                let! offers = supplyContext.Queries.SupplierOffer.GetAll()
                let offerOpt = offers |> List.tryFind (fun o -> o.SkuId = SkuId.value skuId && o.IsActive)
                return offerOpt |> Option.map (fun o -> SupplierId.create o.SupplierId |> Result.get)
            }

    let routingQuery : SupplyGenerationStep.RoutingQuery =
        fun skuId spId ->
            task {
                let! routings = masterDataContext.Queries.Routing.GetAll()
                let matchOpt =
                    routings
                    |> List.tryFind (fun r ->
                        r.Status &&
                        match r.Details with
                        | Medhavi.Contracts.Domain.RoutingDetails.Work w ->
                            w.ProductId.Equals(SkuId.value skuId, StringComparison.OrdinalIgnoreCase)
                        | _ -> false)
                return matchOpt |> Option.map (fun r -> RoutingId.create r.Id |> Result.get)
            }

    let transferSourceQuery : SupplyGenerationStep.TransferSourceQuery =
        fun skuId spId ->
            task {
                let! legs = masterDataContext.Queries.TransportLeg.GetAll()
                let matchOpt =
                    legs
                    |> List.tryFind (fun l ->
                        l.Status &&
                        l.Destination.Equals(StockingPointId.value spId, StringComparison.OrdinalIgnoreCase))
                return matchOpt |> Option.map (fun l -> StockingPointId.create l.Origin |> Result.get)
            }

    let capacityCheckQuery : CapacityCheckStep.CapacityCheckQuery =
        fun spId skuId routingIdOpt qty due policy ->
            task {
                let resources = capacityContext.CapacityResourceAgent.GetStateAsync().Result
                let calendars = capacityContext.CalendarAgent.GetStateAsync().Result
                let buckets = capacityContext.CapacityAgent.GetStateAsync().Result
                let getRoutings productId =
                    task {
                        let! list = masterDataContext.Queries.Routing.GetAll()
                        let filtered =
                            list
                            |> List.filter (fun r ->
                                match r.Details with
                                | Medhavi.Contracts.Domain.RoutingDetails.Work work -> work.ProductId = productId
                                | _ -> false)
                        return Ok filtered
                    }
                let mode = if policy.Finite then CapacityPlanningMode.Finite else CapacityPlanningMode.Infinite
                let! res = SchedulerApp.checkCapacity (SkuId.value skuId) (Quantity.value qty) (Timestamp.value due) mode resources calendars buckets getRoutings
                match res with
                | Error err -> return Error (CapacityCheckError.AllocationFailed (sprintf "Capacity query failed: %A" err))
                | Ok result ->
                    if result.IsFeasible then
                        return Ok due
                    else
                        return Ok (Timestamp.create result.SuggestedDate)
            }

    let alternateRoutingsQuery : CapacityCheckStep.AlternateRoutingsQuery =
        fun skuId spId ->
            task {
                let! list = masterDataContext.Queries.Routing.GetAll()
                let filtered =
                    list
                    |> List.filter (fun r ->
                        match r.Details with
                        | Medhavi.Contracts.Domain.RoutingDetails.Work work -> work.ProductId = SkuId.value skuId
                        | _ -> false)
                    |> List.map (fun r -> RoutingId.create r.Id |> Result.get)
                return filtered
            }

    let reservationCreator : PostprocessStep.ReservationCreator =
        fun skuId spId qty reqDate ->
            async {
                let resId = $"RES-{Guid.NewGuid()}"
                let req = {
                    Id = resId
                    IdempotencyKey = $"idem-{resId}"
                    SkuId = SkuId.value skuId
                    StockingPointId = StockingPointId.value spId
                    Quantity = Quantity.value qty
                    RequiredDate = Timestamp.value reqDate
                    ExpiryTime = (Timestamp.value reqDate).AddDays(30.0)
                }
                let! res = supplyContext.Commands.MaterialReservation.CreateTentative req |> Async.AwaitTask
                match res with
                | Ok _ -> return Ok ()
                | Error err -> return Error (sprintf "%A" err)
            }

    let createSupplyOrders : CreateSupplyOrders =
        fun runId proposals ->
            async {
                let reqs =
                    proposals
                    |> List.map (fun p ->
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
                          RequiredDeliveryDate = Some (Timestamp.value p.DueDate)
                          CreatedDate = DateTimeOffset.UtcNow })

                let! res = supplyContext.Commands.SupplyOrder.CreateBulk reqs |> Async.AwaitTask
                match res with
                | Ok _ -> return Ok ()
                | Error err -> return Error (sprintf "%A" err)
            }

    let buildMrpDependencies () =
        { BomLookup = bomLookup
          OnHandQuery = onHandQuery
          InboundQuery = inboundQuery
          ReservationsQuery = reservationsQuery
          SafetyStockQuery = safetyStockQuery
          ProductTypeQuery = productTypeQuery
          SupplierQuery = supplierQuery
          RoutingQuery = routingQuery
          TransferSourceQuery = transferSourceQuery
          CapacityQuery = capacityCheckQuery
          AlternateRoutingsQuery = alternateRoutingsQuery
          PeggingCreator = None
          ReservationCreator = Some reservationCreator
          CreateSupplyOrders = createSupplyOrders }

    let runBaselineMrp () =
        task {
            printColorLine "bold" "\n--- [EXECUTING BASELINE MRP RUN] ---"

            let now = DateTimeOffset.UtcNow
            let bikeSku = SkuId.create "SKU-BIKE" |> Result.get
            let sp = StockingPointId.create "SP-WAREHOUSE" |> Result.get
            let node = NodeId.create "SP-WAREHOUSE" |> Result.get

            let demands =
                [ { MrpDemand.DemandId = "DEMAND-BIKE-1"
                    SkuId = bikeSku
                    NodeId = node
                    StockingPointId = sp
                    Quantity = Quantity.create 10.0m |> Result.get
                    RequiredDate = Timestamp.create (now.AddDays(10.0))
                    Source = CustomerOrder("ORDER-1", "1")
                    Priority = Some 1 } ]

            let deps = buildMrpDependencies ()
            let mrpServiceInstance = Medhavi.Scheduler.Mrp.MrpService.create deps

            let! runRes =
                mrpServiceInstance.ExecuteRun
                    "MRP-RUN-BASELINE"
                    (Timestamp.create now)
                    (Timestamp.create (now.AddDays(30.0)))
                    sp
                    MrpPolicy.defaults
                    demands
                    []

            match runRes with
            | Error err ->
                printColorLine "red" (sprintf "   [ ERR ] Baseline MRP Run failed: %A" err)
            | Ok result ->
                latestMrpRun <- Some result
                printColorLine "green" "   [ OK ] Baseline MRP Run executed and cached."
                printfn "   Generated Proposals: %d" (List.length result.Proposals)
                for p in result.Proposals do
                    printfn "     - PropId: %s | Sku: %s | Qty: %M | Due: %s | Type: %A"
                        (SupplyProposalId.value p.Id)
                        (SkuId.value p.SkuId)
                        (Quantity.value p.Quantity)
                        ((Timestamp.value p.DueDate).ToString("yyyy-MM-dd HH:mm"))
                        p.ProposalType
        }

    // Initialize Integration Capabilities injecting the store
    let integrationCaps = IntegrationService.createCapabilities envelopeStore

    // -------------------------------------------------------------
    // Table Helper Printer
    // -------------------------------------------------------------
    let printTable (title: string) (headers: string[]) (rows: string[][]) =
        printColorLine "cyan" ("\n┌" + String.replicate 78 "─" + "┐")
        printfn "│ %-76s │" title
        printColorLine "cyan" ("├" + String.replicate 78 "─" + "┤")

        let colCount = headers.Length

        let colWidths =
            Array.init colCount (fun i ->
                let headerWidth = headers.[i].Length

                let rowMax =
                    if rows.Length = 0 then
                        0
                    else
                        rows |> Seq.map (fun r -> r.[i].Length) |> Seq.max

                max headerWidth rowMax + 2)

        // Render headers
        printf "│"

        for i in 0 .. colCount - 1 do
            printf " %-*s │" (colWidths.[i] - 1) headers.[i]

        printfn ""

        // Render header separator
        printf "├"

        for i in 0 .. colCount - 1 do
            printf "%s%s" (String.replicate colWidths.[i] "─") (if i = colCount - 1 then "┤" else "┼")

        printfn ""

        // Render rows
        if rows.Length = 0 then
            let totalWidth = (Array.sum colWidths) + colCount - 1
            printfn "│ %-*s │" (totalWidth - 1) "No data found."
        else
            for row in rows do
                printf "│"

                for i in 0 .. colCount - 1 do
                    printf " %-*s │" (colWidths.[i] - 1) row.[i]

                printfn ""

        // Render bottom border
        printf "└"

        for i in 0 .. colCount - 1 do
            printf "%s%s" (String.replicate colWidths.[i] "─") (if i = colCount - 1 then "┘" else "┴")

        printfn ""

    // -------------------------------------------------------------
    // Subscription Setup (Subscribes Bounded Contexts)
    // -------------------------------------------------------------
    let subscribeBoundedContexts () =
        printColorLine "bold" "\n--- [INITIALIZING EVENT BOUNDED CONTEXT SUBSCRIPTIONS] ---"

        let logger =
            { LogInfo = fun msg -> printColorLine "cyan" msg
              LogSuccess = fun msg -> printColorLine "green" msg
              LogError = fun msg -> printColorLine "red" msg }

        let handler (envelopedEvent: EnvelopedEvent) : Task<unit> =
            task {
                let envelope = envelopedEvent.Envelope

                match IntegrationEventEnvelope.tryGetPayload envelope with
                | Error err ->
                    printColorLine "red" (sprintf "   [ ERR ] [Subscription] Failed to deserialize envelope: %A" err)
                | Ok event ->
                    printColorLine
                        "cyan"
                        $"\n>>> [Subscription Broker] Received {event.GetType().Name} Event! Delegating processing..."

                    match event with
                    | UomImported uoms ->
                        let! _ = masterDataContext.Commands.Uom.DefineBulk(uoms)
                        ()
                    | UnitConversionsImported unitConversions ->
                        let! _ = masterDataContext.Commands.UnitConversion.DefineBulk(unitConversions)
                        ()
                    | TransportLegsImported transportLegs ->
                        let! _ = masterDataContext.Commands.TransportLeg.DefineBulk(transportLegs)
                        ()
                    | RoutingsImported routings ->
                        let! _ = masterDataContext.Commands.Routing.DefineBulk(routings)
                        ()
                    | BomImported boms ->
                        let! _ = masterDataContext.Commands.Bom.DefineBulk(boms)
                        ()
                    | SkusImported skus ->
                        let! _ = masterDataContext.Commands.Sku.DefineBulk(skus)
                        ()
                    | StockingPointsImported stockingPoints ->
                        let! _ = masterDataContext.Commands.StockingPoint.DefineBulk(stockingPoints)
                        ()
                    | PlantsImported plants ->
                        let! _ = masterDataContext.Commands.Plant.DefineBulk(plants)
                        ()
                    | ResourceGroupsImported groups ->
                        let! _ = masterDataContext.Commands.ResourceGroup.DefineBulk(groups)
                        ()
                    | StandardResourcesImported reqs ->
                        let! _ = masterDataContext.Commands.StandardResource.DefineBulk(reqs)
                        ()
                    | PhysicalResourcesImported reqs ->
                        let! _ = masterDataContext.Commands.PhysicalResource.DefineBulk(reqs)
                        ()
                    | SupplyOffersImported supplyOffers ->
                        let! _ = supplyContext.Commands.SupplierOffer.DefineBulk(supplyOffers)
                        ()
                    | InventoryPositionsImported inventoryPositions ->
                        let! res = supplyContext.Commands.Inventory.DefineBulk(inventoryPositions)

                        match res with
                        | Ok items ->
                            for item in items do
                                logger.LogSuccess(
                                    sprintf
                                        "    - Inventory Position Updated: Product=%s, SP=%s, Qty=%M [ OK ]"
                                        item.SkuId
                                        item.StockingPointId
                                        item.Quantity
                                )
                        | Error err -> logger.LogError(sprintf "    - Inventory Ingestion Error: %A" err)

                        ()
                    | InventoryTargetsImported inventoryTargets ->
                        let! _ = supplyContext.Commands.InventoryTarget.DefineBulk(inventoryTargets)
                        ()
                    | SupplyOrdersImported supplyOrders ->
                        let! _ = supplyContext.Commands.SupplyOrder.ProcessStatusUpdates(supplyOrders)
                        ()
                    | MaterialReservationsImported reservations ->
                        let! res =
                            reservations
                            |> List.map supplyContext.Commands.MaterialReservation.CreateTentative
                            |> TaskResult.sequence

                        match res with
                        | Ok items ->
                            for item in items do
                                logger.LogSuccess(
                                    sprintf
                                        "    - Material Reservation Created: Id=%s, Sku=%s, Qty=%M [ OK ]"
                                        item.Id
                                        item.SkuId
                                        item.Quantity
                                )
                        | Error err -> logger.LogError(sprintf "    - Reservation Ingestion Error: %A" err)

                        ()
                    | ResourceCalendarsImported resourceCalendars ->
                        // do! supplyContext.ResourceCalendar.DefineBulk(resourceCalendars)
                        ()
                    | WorkOrdersCompleted workOrdersCompleted ->
                        for item in workOrdersCompleted do
                            let! existingOpt = supplyContext.Queries.SupplyOrder.GetById item.WorkOrderId
                            match existingOpt with
                            | None ->
                                logger.LogError(sprintf "    - Work Order Ingestion Error: Work Order %s not found" item.WorkOrderId)
                            | Some order ->
                                if order.State.Equals("Completed", StringComparison.OrdinalIgnoreCase) then
                                    logger.LogInfo(sprintf "    - Work Order %s already Completed. Skipping (Idempotent)." item.WorkOrderId)
                                else
                                    // 1. Complete the work order (passing the reported scrap)
                                    let completeReq : SupplyOrderCompleteReq =
                                        { Id = item.WorkOrderId
                                          ScrapQuantity = item.QuantityScrapped
                                          CompletedDate = item.CompletedAtUtc
                                          FeedbackId = None }
                                    let! completeRes = supplyContext.Commands.SupplyOrder.Complete completeReq
                                    match completeRes with
                                    | Error err ->
                                        logger.LogError(sprintf "    - Failed to complete Work Order %s: %A" item.WorkOrderId err)
                                    | Ok _ ->
                                        logger.LogSuccess(sprintf "    - Work Order %s Completed successfully [ OK ]" item.WorkOrderId)
                                        
                                        // 2. Increase Finished Goods inventory by QuantityCompleted
                                        let! allInvs = supplyContext.Queries.Inventory.GetAll()
                                        let fgInvOpt =
                                            allInvs
                                            |> List.tryFind (fun inv ->
                                                inv.SkuId.Equals(order.SkuId, StringComparison.OrdinalIgnoreCase) &&
                                                inv.StockingPointId.Equals(order.StockingPointId, StringComparison.OrdinalIgnoreCase))
                                        
                                        match fgInvOpt with
                                        | Some fgInv ->
                                            let newQty = fgInv.Quantity + item.QuantityCompleted
                                            let! fgRes = supplyContext.Commands.Inventory.Define {
                                                Id = fgInv.Id
                                                SkuId = fgInv.SkuId
                                                StockingPointId = fgInv.StockingPointId
                                                Quantity = newQty
                                                UnitOfMeasure = fgInv.UnitOfMeasure
                                            }
                                            match fgRes with
                                            | Ok _ -> logger.LogSuccess(sprintf "      -> Finished Goods Inventory increased for %s to %M" order.SkuId newQty)
                                            | Error e -> logger.LogError(sprintf "      -> FG Inv update failed: %A" e)
                                        | None ->
                                            let fgId = $"INV-{order.SkuId}-{order.StockingPointId}"
                                            let! fgRes = supplyContext.Commands.Inventory.Define {
                                                Id = fgId
                                                SkuId = order.SkuId
                                                StockingPointId = order.StockingPointId
                                                Quantity = item.QuantityCompleted
                                                UnitOfMeasure = "UOM-PCS"
                                            }
                                            match fgRes with
                                            | Ok _ -> logger.LogSuccess(sprintf "      -> Finished Goods Inventory created for %s with %M" order.SkuId item.QuantityCompleted)
                                            | Error e -> logger.LogError(sprintf "      -> FG Inv create failed: %A" e)

                                        // 3. Deduct BOM components inventory (Backflushing)
                                        let! boms = masterDataContext.Queries.Bom.GetAll()
                                        let skuBomOpt =
                                            boms
                                            |> List.tryFind (fun b ->
                                                b.SkuId.Equals(order.SkuId, StringComparison.OrdinalIgnoreCase) &&
                                                b.Status)
                                        
                                        match skuBomOpt with
                                        | None -> ()
                                        | Some bom ->
                                            for bomItem in bom.Items do
                                                let consumedQty = bomItem.Quantity * (item.QuantityCompleted + item.QuantityScrapped)
                                                let compInvOpt =
                                                    allInvs
                                                    |> List.tryFind (fun inv ->
                                                        inv.SkuId.Equals(bomItem.ComponentSkuId, StringComparison.OrdinalIgnoreCase) &&
                                                        inv.StockingPointId.Equals(order.StockingPointId, StringComparison.OrdinalIgnoreCase))
                                                
                                                match compInvOpt with
                                                | Some compInv ->
                                                    let newCompQty = compInv.Quantity - consumedQty
                                                    let! compRes = supplyContext.Commands.Inventory.Define {
                                                        Id = compInv.Id
                                                        SkuId = compInv.SkuId
                                                        StockingPointId = compInv.StockingPointId
                                                        Quantity = newCompQty
                                                        UnitOfMeasure = compInv.UnitOfMeasure
                                                    }
                                                    match compRes with
                                                    | Ok _ -> logger.LogSuccess(sprintf "      -> Component Stock reduced: Component=%s, consumed=%M, newBalance=%M" bomItem.ComponentSkuId consumedQty newCompQty)
                                                    | Error e -> logger.LogError(sprintf "      -> Component Stock reduction failed: %A" e)
                                                | None ->
                                                    let compId = $"INV-{bomItem.ComponentSkuId}-{order.StockingPointId}"
                                                    let! compRes = supplyContext.Commands.Inventory.Define {
                                                        Id = compId
                                                        SkuId = bomItem.ComponentSkuId
                                                        StockingPointId = order.StockingPointId
                                                        Quantity = -consumedQty
                                                        UnitOfMeasure = "UOM-PCS"
                                                    }
                                                    match compRes with
                                                    | Ok _ -> logger.LogSuccess(sprintf "      -> Component Stock created with negative balance: Component=%s, consumed=%M" bomItem.ComponentSkuId consumedQty)
                                                    | Error e -> logger.LogError(sprintf "      -> Component Stock create failed: %A" e)
                    | MaterialsReceived materialsReceived ->
                        // do! supplyContext.MaterialReceipt.DefineBulk(materialsReceived)
                        ()
                    | ResourceDowntimes resourceDowntimes ->
                        for payload in resourceDowntimes do
                            printColorLine "yellow" (sprintf ">>> [Disruption Ingest] Resource downtime reported: Resource=%s, Start=%s, End=%s, Reason=%s" 
                                payload.ResourceId 
                                (payload.StartUtc.ToString("yyyy-MM-dd HH:mm"))
                                (payload.EndUtc.ToString("yyyy-MM-dd HH:mm"))
                                payload.Reason)
                            
                            match latestMrpRun with
                            | None -> 
                                printColorLine "cyan" "   - No baseline MRP run cached. Skipping heuristic reactive repair."
                            | Some baseline ->
                                printColorLine "cyan" "   - Evaluating blast radius and triggering reactive repair..."
                                let event = Medhavi.Scheduler.Mrp.Domain.ResourceBreakdown(payload.ResourceId, Timestamp.create payload.StartUtc, Timestamp.create payload.EndUtc)
                                
                                let severityMap = Map.ofList [
                                    "fullReplanDurationHrs", 24.0
                                    "ignoreDurationHrs", 1.0
                                ]
                                
                                let deps = buildMrpDependencies ()
                                let! replanResult = ReplanService.executeReplan deps baseline event severityMap
                                match replanResult with
                                | Error err ->
                                    printColorLine "red" (sprintf "   [ ERR ] Reactive repair failed: %A" err)
                                | Ok newRun ->
                                        let delta = Replan.PlanDeltaCalculator.calculate baseline newRun
                                        printColorLine "green" "   [ OK ] Reactive repair complete."
                                        printfn "     - Churn (Rescheduled): %d" (List.length delta.RescheduledProposals)
                                        printfn "     - Added Proposals: %d" (List.length delta.AddedProposals)
                                        printfn "     - Cancelled Proposals: %d" (List.length delta.CancelledProposals)
                                        
                                        // Update cache and persist new proposals
                                        latestMrpRun <- Some newRun
                                        let! (persistRes: Result<unit, string>) = createSupplyOrders newRun.RunId newRun.Proposals |> Async.StartAsTask
                                        match persistRes with
                                        | Ok _ -> printColorLine "green" "     - Repaired plan successfully persisted to database."
                                        | Error err -> printColorLine "red" (sprintf "     - Failed to persist repaired proposals: %s" err)
                        ()

                    | TransportDelays transportDelays ->
                        for payload in transportDelays do
                            printColorLine "yellow" (sprintf ">>> [Disruption Ingest] Transport delay reported: Leg=%s, DelayMins=%.1f, NewArrival=%s, Reason=%s"
                                payload.TransportLegId
                                payload.EstimatedDelayMinutes
                                (payload.NewArrivalUtc.ToString("yyyy-MM-dd HH:mm"))
                                payload.Reason)

                            match latestMrpRun with
                            | None ->
                                printColorLine "cyan" "   - No baseline MRP run cached. Skipping heuristic reactive repair."
                            | Some baseline ->
                                // Look up the transport leg details
                                let! legOpt = masterDataContext.Queries.TransportLeg.GetById(payload.TransportLegId)
                                match legOpt with
                                | None ->
                                    printColorLine "red" (sprintf "   - Transport leg %s not found in database. Skipping." payload.TransportLegId)
                                | Some leg ->
                                    // Search for matching proposal in baseline
                                    let matchedPropOpt =
                                        baseline.Proposals
                                        |> List.filter (fun p -> p.ProposalType = PlannedTransferOrder)
                                        |> List.tryFind (fun p ->
                                            (StockingPointId.value p.StockingPointId).Equals(leg.Destination, StringComparison.OrdinalIgnoreCase) &&
                                            (p.SupplierId |> Option.map SupplierId.value |> Option.defaultValue "").Equals(leg.Origin, StringComparison.OrdinalIgnoreCase))
                                            
                                    match matchedPropOpt with
                                    | None ->
                                        printColorLine "yellow" (sprintf "   - No active transfer order proposal matches leg %s (Origin=%s, Dest=%s). Skipping." payload.TransportLegId leg.Origin leg.Destination)
                                    | Some prop ->
                                        printColorLine "cyan" (sprintf "   - Found matching Transfer Order proposal %s. Triggering reactive repair..." (SupplyProposalId.value prop.Id))
                                        let event = Medhavi.Scheduler.Mrp.Domain.MaterialDelay(prop.SkuId, prop.StockingPointId, Timestamp.create payload.NewArrivalUtc, SupplyProposalId.value prop.Id)
                                        
                                        let severityMap = Map.ofList [
                                            "fullReplanDelayHrs", 48.0
                                            "ignoreDelayHrs", 2.0
                                        ]
                                        
                                        let deps = buildMrpDependencies ()
                                        let! replanResult = ReplanService.executeReplan deps baseline event severityMap
                                        match replanResult with
                                        | Error err ->
                                            printColorLine "red" (sprintf "   [ ERR ] Reactive repair failed: %A" err)
                                        | Ok newRun ->
                                            let delta = Replan.PlanDeltaCalculator.calculate baseline newRun
                                            printColorLine "green" "   [ OK ] Reactive repair complete."
                                            printfn "     - Churn (Rescheduled): %d" (List.length delta.RescheduledProposals)
                                            printfn "     - Added Proposals: %d" (List.length delta.AddedProposals)
                                            printfn "     - Cancelled Proposals: %d" (List.length delta.CancelledProposals)
                                            
                                            // Update cache and persist new proposals
                                            latestMrpRun <- Some newRun
                                            let! (persistRes: Result<unit, string>) = createSupplyOrders newRun.RunId newRun.Proposals |> Async.StartAsTask
                                            match persistRes with
                                            | Ok _ -> printColorLine "green" "     - Repaired plan successfully persisted to database."
                                            | Error err -> printColorLine "red" (sprintf "     - Failed to persist repaired proposals: %s" err)
                        ()
            }

        let subscribeTask =
            envelopeStore.Subscribe SubscriptionMode.All None handler System.Threading.CancellationToken.None

        match subscribeTask.Result with
        | Error err -> printfn "   [ ERR ] Failed to subscribe bounded context handlers: %A" err
        | Ok handle ->
            subscriptionHandle <- Some handle

            printColorLine
                "green"
                "   [ OK ] Bounded Context subscriptions established with EnvelopeStore. Standing by..."

    // -------------------------------------------------------------
    // Menu Actions
    // -------------------------------------------------------------
    let loadAndValidateCsv () =
        printColorLine "bold" "\n--- [STEP 1: TRIGGER CSV INGESTION & PUBLISH via IntegrationService] ---"
        let ingestTask = integrationCaps.IngestAndPublishMasterData()

        match ingestTask.Result with
        | Ok _ -> printColorLine "green" "   [ OK ] Master data successfully published."
        | Error(ValidationError errors) ->
            printColor "red" "   [ ERR ] Validation failed with the following errors:\n"

            for err in errors do
                printfn "     - %s" err
        | Error(IngestionError err) -> printColorLine "red" (sprintf "   [ ERR ] Ingestion failed: %s" err)

    let viewEnvelopesInStore () =
        printColorLine "bold" "\n--- [STEP 2: VIEW OUTBOX ENVELOPES IN STORE] ---"

        let readTask =
            envelopeStore.ReadAll None None System.Threading.CancellationToken.None

        match readTask.Result with
        | Error err -> printColorLine "red" (sprintf "   [ ERR ] Failed to read from EnvelopeStore: %A" err)
        | Ok envelopes ->
            if envelopes.Length = 0 then
                printColorLine
                    "yellow"
                    "   [ WARN ] EnvelopeStore is currently empty. Please load CSV data first (Option 1)."
            else
                printfn "Current Envelopes in Store:"

                for env in envelopes do
                    printfn
                        "  - Envelope ID: %s | Type: %s | Stream: %s | GlobalPos: %A"
                        (env.Envelope.EventId.ToString())
                        env.Envelope.EventType
                        (env.ReadFrom |> Option.defaultValue "Unknown")
                        (env.Position.GlobalPosition
                         |> Option.map string
                         |> Option.defaultValue "None")

    let showDashboard () =
        printColorLine "bold" "\n================================================================================"
        printColorLine "bold" "                     MEDHĀVĪ AGGREGATE DB SNAPSHOT DASHBOARD                     "
        printColorLine "bold" "================================================================================"

        // 1. SKUs Table
        let skus = masterDataContext.Queries.Sku.GetAll().Result

        let skuRows =
            skus
            |> List.map (fun s -> [| s.Id; s.Code; s.Name |])
            |> List.toArray

        printTable "SKUs IN DATABASE" [| "SKU ID"; "CODE"; "NAME" |] skuRows

        // 2. Stocking Points Table
        let sps =
            masterDataContext.Queries.StockingPoint
                .GetAll()
                .Result

        let spRows =
            sps
            |> List.map (fun s -> [| s.Id; s.PlantId; s.Name; s.Type |])
            |> List.toArray

        printTable "STOCKING POINTS IN DATABASE" [| "STOCKING POINT ID"; "PLANT ID"; "NAME"; "TYPE" |] spRows

        // 3. BOM Table
        let boms = masterDataContext.Queries.Bom.GetAll().Result

        let bomRows =
            boms
            |> List.collect (fun b ->
                b.Items
                |> List.map (fun i -> [| b.Id; b.SkuId; i.ComponentSkuId; i.Quantity.ToString() |]))
            |> List.toArray

        printTable
            "BILL OF MATERIALS (BOM) RELATIONSHIPS"
            [| "BOM ID"; "PARENT SKU ID"; "COMPONENT SKU ID"; "QTY REQUIRED" |]
            bomRows

        // 4. Routings Table
        let routings =
            masterDataContext.Queries.Routing
                .GetAll()
                .Result

        let routingRows =
            routings
            |> List.collect (fun r ->
                match r.Details with
                | Medhavi.Contracts.Domain.RoutingDetails.Work work ->
                    work.Steps
                    |> List.map (fun s ->
                        let resIdStr =
                            s.ResourceRequirements
                            |> List.tryHead
                            |> Option.map (fun req ->
                                req.Options
                                |> List.tryHead
                                |> Option.map (fun o -> o.ResourceGroupId)
                                |> Option.defaultValue req.RequirementId)
                            |> Option.defaultValue ""

                        let yieldStr =
                            match s.YieldPolicy with
                            | Medhavi.Contracts.Domain.StepYieldPolicy.NoYieldLoss -> "1.0"
                            | Medhavi.Contracts.Domain.StepYieldPolicy.ExpectedYield y -> y.ToString()

                        [| r.Id
                           $"WORK: {work.ProductId}"
                           s.StepId
                           s.Sequence.ToString()
                           resIdStr
                           yieldStr |])
                | Medhavi.Contracts.Domain.RoutingDetails.Transport trans ->
                    [ [| r.Id
                         $"TRANSPORT: {trans.SkuId}"
                         "Move"
                         "-"
                         $"{trans.FromNodeId} -> {trans.ToNodeId}"
                         $"Lead: {trans.TransitLeadTime}m" |] ]
                | Medhavi.Contracts.Domain.RoutingDetails.Purchase pur ->
                    [ [| r.Id
                         $"PURCHASE: {pur.SkuId}"
                         "Buy"
                         "-"
                         $"Supplier: {pur.SupplierId}"
                         $"Lead: {pur.SupplierLeadTime}m" |] ])
            |> List.toArray

        printTable
            "ROUTINGS AND PRODUCTION/LOGISTICS PATHS"
            [| "ROUTING ID"
               "TYPE/SKU"
               "STEP/OP"
               "SEQ"
               "RESOURCE GROUP / PATH"
               "YIELD / LEAD" |]
            routingRows

        // 5. Transport Legs Table
        let legs =
            masterDataContext.Queries.TransportLeg
                .GetAll()
                .Result

        let legRows =
            legs
            |> List.map (fun l ->
                let capStr =
                    l.Capacity
                    |> Option.map (fun c -> (float c).ToString())
                    |> Option.defaultValue "Uncapped"

                [| l.Id
                   l.Origin
                   l.Destination
                   l.Mode
                   l.LeadTimeMinutes.ToString() + "m"
                   capStr |])
            |> List.toArray

        printTable
            "TRANSPORT LEGS (LOGISTICS LANES)"
            [| "LEG ID"; "ORIGIN SP"; "DESTINATION SP"; "MODE"; "LEAD TIME"; "CAPACITY" |]
            legRows

        // 6. Plants Table
        let plants =
            masterDataContext.Queries.Plant
                .GetAll()
                .Result

        let plantRows =
            plants
            |> List.map (fun p -> [| p.Id; p.Code; p.Name; (if p.Status then "Active" else "Inactive") |])
            |> List.toArray

        printTable "PLANTS IN DATABASE" [| "PLANT ID"; "CODE"; "NAME"; "STATUS" |] plantRows

        // 7. Unit Conversions Table
        let conversions =
            masterDataContext.Queries.UnitConversion
                .GetAll()
                .Result

        let convRows =
            conversions
            |> List.map (fun c ->
                [| c.Id
                   c.FromUnitCode
                   c.ToUnitCode
                   c.Ratio.ToString()
                   (if c.Status then "Active" else "Inactive") |])
            |> List.toArray

        printTable
            "UNIT CONVERSIONS IN DATABASE"
            [| "CONVERSION ID"; "FROM UNIT"; "TO UNIT"; "RATIO"; "STATUS" |]
            convRows

        // 8. Inventories Table
        let inventories =
            supplyContext.Queries.Inventory
                .GetAll()
                .Result

        let invRows =
            inventories
            |> List.map (fun i ->
                [| i.Id
                   i.SkuId
                   i.StockingPointId
                   i.Quantity.ToString()
                   i.InTransitInbound.ToString()
                   i.AvailableToPromise.ToString() |])
            |> List.toArray

        printTable
            "INVENTORIES IN DATABASE"
            [| "INVENTORY ID"
               "SKU ID"
               "STOCKING POINT ID"
               "ON-HAND QTY"
               "IN-TRANSIT"
               "ATP QTY" |]
            invRows

        // 9. Inventory Targets Table
        let targets =
            supplyContext.Queries.InventoryTarget
                .GetAll()
                .Result

        let targetRows =
            targets
            |> List.map (fun t ->
                let safetyStr =
                    t.SafetyStockQty
                    |> Option.map (fun q -> q.ToString())
                    |> Option.defaultValue "0"

                let minStr =
                    t.MinQty
                    |> Option.map (fun q -> q.ToString())
                    |> Option.defaultValue "None"

                let maxStr =
                    t.MaxQty
                    |> Option.map (fun q -> q.ToString())
                    |> Option.defaultValue "None"

                [| t.Id; t.SkuId; t.StockingPointId; safetyStr; minStr; maxStr |])
            |> List.toArray

        printTable
            "INVENTORY TARGETS IN DATABASE"
            [| "TARGET ID"
               "SKU ID"
               "STOCKING POINT ID"
               "SAFETY STOCK"
               "MIN QTY"
               "MAX QTY" |]
            targetRows

        // 10. Supplier Offers Table
        let offers =
            supplyContext.Queries.SupplierOffer
                .GetAll()
                .Result

        let offerRows =
            offers
            |> List.map (fun o ->
                let moqStr =
                    o.Moq
                    |> Option.map (fun q -> q.ToString())
                    |> Option.defaultValue "None"

                let lotStr =
                    o.LotSize
                    |> Option.map (fun q -> q.ToString())
                    |> Option.defaultValue "None"

                [| o.Id
                   o.SupplierId
                   o.SkuId
                   moqStr
                   lotStr
                   (if o.IsActive then "Active" else "Inactive") |])
            |> List.toArray

        printTable
            "SUPPLIER OFFERS IN DATABASE"
            [| "OFFER ID"; "SUPPLIER ID"; "SKU ID"; "MOQ"; "LOT SIZE"; "STATUS" |]
            offerRows

        // 11. Supply Orders Table
        let orders =
            supplyContext.Queries.SupplyOrder
                .GetAll()
                .Result

        let orderRows =
            orders
            |> List.map (fun o ->
                [| o.Id
                   o.OrderType
                   o.SkuId
                   o.StockingPointId
                   o.Quantity.ToString()
                   o.State |])
            |> List.toArray

        printTable
            "SUPPLY ORDERS IN DATABASE"
            [| "ORDER ID"; "TYPE"; "SKU ID"; "STOCKING POINT ID"; "QTY"; "STATE" |]
            orderRows

        // 11.5 Material Reservations In Database
        let reservationsList =
            supplyContext.Queries.MaterialReservation
                .GetAll()
                .Result

        let resvRows =
            reservationsList
            |> List.map (fun r ->
                [| r.Id
                   r.SkuId
                   r.StockingPointId
                   r.Quantity.ToString()
                   r.State
                   r.RequiredDate.ToString("yyyy-MM-dd")
                   r.ExpiryTime.ToString("yyyy-MM-dd HH:mm:ss") |])
            |> List.toArray

        printTable
            "MATERIAL RESERVATIONS IN DATABASE"
            [| "RESERVATION ID"
               "SKU ID"
               "STOCKING POINT ID"
               "QTY"
               "STATE"
               "REQUIRED DATE"
               "EXPIRY TIME" |]
            resvRows

        // 11.6 Capacity Resources in Database
        let capResources: CapacityResource list =
            QueryServiceBase.getAll capacityContext.CapacityResourceAgent
            |> fun t -> t.Result

        let capResRows =
            capResources
            |> List.map (fun (r: CapacityResource) ->
                let costStr =
                    r.EffectiveCostRate
                    |> Option.map (fun c -> c.ToString())
                    |> Option.defaultValue "-"

                let calStr =
                    r.EffectiveCalendarId
                    |> Option.map CalendarId.value
                    |> Option.defaultValue "-"

                [| PhysicalResourceId.value r.Id
                   StandardResourceId.value r.StandardResourceId
                   ResourceGroupId.value r.ResourceGroupId
                   r.Name
                   (if r.IsActive then "Active" else "Inactive")
                   (Percent.value r.EffectiveEfficiency).ToString()
                   + "%"
                   costStr
                   calStr |])
            |> List.toArray

        printTable
            "CAPACITY RESOURCES (CLEAN BOUNDED VIEW WITH HIERARCHICAL FALLBACKS)"
            [| "RESOURCE ID"
               "STD RESOURCE ID"
               "GROUP ID"
               "NAME"
               "STATUS"
               "EFFICIENCY"
               "COST RATE"
               "CALENDAR ID" |]
            capResRows

        // 11.7 Capacity Buckets in Database
        let capBuckets: CapacityBucket list =
            QueryServiceBase.getAll capacityContext.CapacityAgent
            |> fun t -> t.Result

        let bucketRows =
            capBuckets
            |> List.map (fun (b: CapacityBucket) ->
                let startStr =
                    (Timestamp.value b.Window.Start)
                        .ToString("yyyy-MM-dd HH:mm")

                let endStr =
                    (Timestamp.value b.Window.End)
                        .ToString("yyyy-MM-dd HH:mm")

                [| CapacityBucketId.value b.Id
                   PhysicalResourceId.value b.ResourceId
                   $"{startStr} to {endStr}"
                   (DurationMinutes.value b.AvailableMinutes)
                       .ToString()
                   + "m"
                   (DurationMinutes.value b.PlannedMinutes)
                       .ToString()
                   + "m"
                   (DurationMinutes.value b.FreeMinutes).ToString()
                   + "m"
                   b.Status.ToString() |])
            |> List.toArray

        printTable
            "CAPACITY BUCKETS IN DATABASE"
            [| "BUCKET ID"
               "RESOURCE ID"
               "WINDOW"
               "AVAILABLE"
               "PLANNED"
               "FREE"
               "STATUS" |]
            bucketRows

        // 12. Live Material Availability ATP Snapshots & Projections
        printColorLine "bold" "\n================================================================================"
        printColorLine "bold" "                     LIVE MATERIAL AVAILABILITY SNAPSHOTS                       "
        printColorLine "bold" "================================================================================"

        let sampleProducts = [ "SKU-BIKE"; "SKU-FRAME"; "SKU-WHEEL" ]
        let stockingPoints = [ "SP-WAREHOUSE"; "SP-FACTORY" ]
        let now = DateTimeOffset.UtcNow

        let atpRows =
            [ for p in sampleProducts do
                  for sp in stockingPoints do
                      let snapRes =
                          MaterialProvider.getSnapshot supplyContext p sp now
                          |> Async.RunSynchronously

                      match snapRes with
                      | Ok snap ->
                          let net = MaterialProvider.calculateNetAvailable snap
                          let totalInbound = snap.Inbound |> List.sumBy snd
                          let totalReservations = snap.Reservations |> List.sumBy snd

                          yield
                              [| p
                                 sp
                                 snap.OnHand.ToString()
                                 totalInbound.ToString()
                                 snap.Safety.ToString()
                                 totalReservations.ToString()
                                 net.ToString() |]
                      | Error _ -> () ]
            |> List.toArray

        printTable
            "MATERIAL AVAILABILITY (ATP) SUMMARY"
            [| "SKU ID"
               "STOCKING POINT"
               "ON-HAND QTY"
               "INBOUND QTY"
               "SAFETY QTY"
               "RESERVATIONS"
               "NET AVAILABLE (ATP)" |]
            atpRows

        // 13. Time-Phased Availability Projections
        printColorLine "bold" "\n================================================================================"
        printColorLine "bold" "             TIME-PHASED AVAILABILITY PROJECTIONS (90-DAY HORIZON)               "
        printColorLine "bold" "================================================================================"

        let timePhasedRows =
            [ for p in [ "SKU-BIKE"; "SKU-FRAME" ] do
                  let sp =
                      if p = "SKU-BIKE" then
                          "SP-WAREHOUSE"
                      else
                          "SP-FACTORY"

                  let tpRes =
                      MaterialProvider.getTimePhasedAvailability supplyContext p sp now 10 90
                      |> Async.RunSynchronously

                  match tpRes with
                  | Ok list ->
                      for (date, qty) in list do
                          yield [| p; sp; date.ToString("yyyy-MM-dd"); qty.ToString() |]
                  | Error _ -> () ]
            |> List.toArray

        printTable
            "TIME-PHASED NET AVAILABILITY BUCKETS (10-DAY BUCKETS)"
            [| "SKU ID"; "STOCKING POINT"; "BUCKET START DATE"; "NET AVAILABLE" |]
            timePhasedRows

        // 13.5 Daily Date-Wise Step-Curve Projections
        printColorLine "bold" "\n================================================================================"
        printColorLine "bold" "             DAILY DATE-WISE STEP-CURVE AVAILABILITY PROJECTIONS                "
        printColorLine "bold" "================================================================================"

        let dailyRows =
            [ for p in [ "SKU-BIKE"; "SKU-FRAME" ] do
                  let sp =
                      if p = "SKU-BIKE" then
                          "SP-WAREHOUSE"
                      else
                          "SP-FACTORY"

                  let dailyRes =
                      MaterialProvider.getDateWiseAvailability supplyContext p sp now 90
                      |> Async.RunSynchronously

                  match dailyRes with
                  | Ok list ->
                      for (date, qty) in list do
                          yield [| p; sp; date.ToString("yyyy-MM-dd"); qty.ToString() |]
                  | Error _ -> () ]
            |> List.toArray

        printTable
            "DAILY STEP-CURVE NET AVAILABILITY"
            [| "SKU ID"; "STOCKING POINT"; "DATE"; "NET AVAILABLE" |]
            dailyRows

    let runCapacityCheckDemo () =
        printColorLine "bold" "\n--- [CTP CAPACITY CHECK DEMO] ---"

        let productId = "SKU-FRAME"
        let quantity = 10.0m
        let needDate = DateTimeOffset.UtcNow.AddDays(5.0)

        printfn
            "Running check for Product=%s, Qty=%M, NeedDate=%s"
            productId
            quantity
            (needDate.ToString("yyyy-MM-dd HH:mm"))

        let getRoutings productId =
            task {
                let! list = masterDataContext.Queries.Routing.GetAll()

                let filtered =
                    list
                    |> List.filter (fun r ->
                        match r.Details with
                        | Medhavi.Contracts.Domain.RoutingDetails.Work work -> work.ProductId = productId
                        | _ -> false)

                return Ok filtered
            }

        let resources =
            capacityContext.CapacityResourceAgent
                .GetStateAsync()
                .Result

        let calendars =
            capacityContext.CalendarAgent
                .GetStateAsync()
                .Result

        let buckets =
            capacityContext.CapacityAgent
                .GetStateAsync()
                .Result

        // 1. Run Infinite check
        let checkInfinite =
            SchedulerApp.checkCapacity
                productId
                quantity
                needDate
                CapacityPlanningMode.Infinite
                resources
                calendars
                buckets
                getRoutings
            |> Async.AwaitTask
            |> Async.RunSynchronously

        match checkInfinite with
        | Error err -> printColorLine "red" (sprintf "Infinite capacity check failed: %A" err)
        | Ok res ->
            printColorLine "green" "\n--- INFINITE CAPACITY CHECK RESULT ---"
            printfn "  Is Feasible: %b" res.IsFeasible
            printfn "  Suggested Date: %s" (res.SuggestedDate.ToString("yyyy-MM-dd HH:mm"))

            res.LatenessReason
            |> Option.iter (fun r -> printfn "  Reason: %s" r)

            printfn "  Required Loads:"

            for KeyValue(resId, dm) in res.RequiredLoads do
                printfn "    - %s: %Mm" resId (DurationMinutes.value dm)

        // 2. Run Finite check
        let checkFinite =
            SchedulerApp.checkCapacity
                productId
                quantity
                needDate
                CapacityPlanningMode.Finite
                resources
                calendars
                buckets
                getRoutings
            |> Async.AwaitTask
            |> Async.RunSynchronously

        match checkFinite with
        | Error err -> printColorLine "red" (sprintf "Finite capacity check failed: %A" err)
        | Ok res ->
            printColorLine "green" "\n--- FINITE CAPACITY CHECK RESULT ---"
            printfn "  Is Feasible: %b" res.IsFeasible
            printfn "  Suggested Date: %s" (res.SuggestedDate.ToString("yyyy-MM-dd HH:mm"))

            res.LatenessReason
            |> Option.iter (fun r -> printfn "  Reason: %s" r)

            printfn "  Bottleneck Resource: %A" res.BottleneckResourceId
            printfn "  Required Loads:"

            for KeyValue(resId, dm) in res.RequiredLoads do
                printfn "    - %s: %Mm" resId (DurationMinutes.value dm)

    let runTransportAtpDemo () =
        printColorLine "bold" "\n--- [TRANSPORT ATP DEMO — K-SHORTEST PATHS] ---"

        let fromNode = "SP-FACTORY"
        let toNode = "SP-CUSTOMER"
        let needDate = DateTimeOffset.UtcNow.AddDays(3.0)
        let qty = 50.0m

        printfn
            "Finding transport routes: %s → %s | NeedBy: %s | Qty: %M"
            fromNode
            toNode
            (needDate.ToString("yyyy-MM-dd"))
            qty

        let req: Medhavi.Transport.GetTransportOptionsReq =
            { FromNode = fromNode
              ToNode = toNode
              SkuId = Some "SKU-FRAME"
              RequiredQuantity = Some qty
              NeedByDate = needDate
              MaxHops = Some 4
              MaxItineraries = Some 5 }

        let result =
            transportContext.Atp.GetOptions req
            |> Async.RunSynchronously

        match result with
        | Error err -> printColorLine "red" (sprintf "Transport ATP failed: %s" err)
        | Ok options ->
            printColorLine "green" (sprintf "\nFound %d feasible transport itineraries:" options.Length)

            for i, opt in options |> List.indexed do
                printColorLine
                    "cyan"
                    (sprintf "\n  Route #%d %s" (i + 1) (if opt.IsPreferred then "★ PREFERRED" else ""))

                printfn "    Hops:         %d" opt.Itinerary.HopCount
                printfn "    Lead Time:    %.1f hours" (float opt.Itinerary.TotalLeadTimeMinutes / 60.0)
                printfn "    Est. Cost:    %M" opt.EstimatedCost
                printfn "    Reliability:  %.1f%%" (float opt.ReliabilityScore * 100.0)
                printfn "    Earliest Dep: %s" (opt.EarliestDeparture.ToString("yyyy-MM-dd HH:mm"))
                printfn "    Earliest Arr: %s" (opt.EarliestArrival.ToString("yyyy-MM-dd HH:mm"))

                opt.CO2Estimate
                |> Option.iter (fun co2 -> printfn "    CO₂ Estimate: %M kg" co2)

                printfn "    Hops detail:"

                for hop in opt.Itinerary.Hops do
                    printfn
                        "      [%s] %s → %s  (%.0f min)"
                        hop.Mode
                        hop.Origin
                        hop.Destination
                        (float hop.LeadTimeMinutes)

    [<EntryPoint>]
    let main argv =
        printColorLine "bold" "========================================================="
        printColorLine "cyan" "     MEDHĀVĪ CONTROL TOWER - INTEGRATION SIMULATOR       "
        printColorLine "bold" "========================================================="

        subscribeBoundedContexts ()

        // Bootstrap projections
        masterDataContext.Initialize().Wait()
        supplyContext.Initialize().Wait()
        capacityContext.Initialize().Wait()
        transportContext.Initialize().Wait()

        let mutable exit = false

        while not exit do
            printColorLine "bold" "\nMAIN MENU"
            printfn "1. Publish Master Data CSV"
            printfn "2. View Aggregate Database Snapshot Dashboard"
            printfn "3. Run End-to-End Automated Demo"
            printfn "4. Run CTP Capacity Check Demo"
            printfn "5. Run Transport ATP Demo (K-Shortest Paths)"
            printfn "6. Run Baseline MRP Plan"
            printfn "7. Exit"

            printf "Select option (1-7): "
            let choice = Console.ReadLine()

            match choice with
            | "1" -> loadAndValidateCsv ()
            | "2" -> viewEnvelopesInStore ()
            | "3" -> showDashboard ()
            | "4" -> runCapacityCheckDemo ()
            | "5" ->
                // Invalidate transport cache so fresh legs are used after CSV load
                transportContext.Atp.InvalidateCache()
                runTransportAtpDemo ()
            | "6" ->
                (runBaselineMrp ()).Wait()
            | "7" ->
                exit <- true
                printColorLine "cyan" "\nExiting Medhāvī Simulator. Goodbye!"
            | _ -> printColorLine "red" "Invalid choice. Please enter 1-7."

        masterDataContext.Dispose()
        supplyContext.Dispose()
        capacityContext.Dispose()
        transportContext.Dispose()

        0
