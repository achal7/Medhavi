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
                masterDataContext.TransportLeg.QueryService.GetAll()
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
                        let! _ = masterDataContext.Uom.DefineBulk(uoms)
                        ()
                    | UnitConversionsImported unitConversions ->
                        let! _ = masterDataContext.UnitConversion.DefineBulk(unitConversions)
                        ()
                    | TransportLegsImported transportLegs ->
                        let! _ = masterDataContext.TransportLeg.DefineBulk(transportLegs)
                        ()
                    | RoutingsImported routings ->
                        let! _ = masterDataContext.Routing.DefineBulk(routings)
                        ()
                    | BomImported boms ->
                        let! _ = masterDataContext.Bom.DefineBulk(boms)
                        ()
                    | SkusImported skus ->
                        let! _ = masterDataContext.Sku.DefineBulk(skus)
                        ()
                    | StockingPointsImported stockingPoints ->
                        let! _ = masterDataContext.StockingPoint.DefineBulk(stockingPoints)
                        ()
                    | PlantsImported plants ->
                        let! _ = masterDataContext.Plant.DefineBulk(plants)
                        ()
                    | ResourceGroupsImported groups ->
                        let! _ = masterDataContext.ResourceGroup.DefineBulk(groups)
                        ()
                    | StandardResourcesImported reqs ->
                        let! _ = masterDataContext.StandardResource.DefineBulk(reqs)
                        ()
                    | PhysicalResourcesImported reqs ->
                        let! _ = masterDataContext.PhysicalResource.DefineBulk(reqs)
                        ()
                    | SupplyOffersImported supplyOffers ->
                        let! _ = supplyContext.SupplierOffer.DefineBulk(supplyOffers)
                        ()
                    | InventoryPositionsImported inventoryPositions ->
                        let! res = supplyContext.Inventory.DefineBulk(inventoryPositions)

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
                        let! _ = supplyContext.InventoryTarget.DefineBulk(inventoryTargets)
                        ()
                    | SupplyOrdersImported supplyOrders ->
                        let! _ = supplyContext.SupplyOrder.ProcessStatusUpdates(supplyOrders)
                        ()
                    | MaterialReservationsImported reservations ->
                        let! res =
                            reservations
                            |> List.map supplyContext.MaterialReservation.CreateTentative
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
                        // do! supplyContext.WorkOrder.DefineBulk(workOrdersCompleted)
                        ()
                    | MaterialsReceived materialsReceived ->
                        // do! supplyContext.MaterialReceipt.DefineBulk(materialsReceived)
                        ()
                    | ResourceDowntimes resourceDowntimes ->
                        // do! supplyContext.ResourceDowntime.DefineBulk(resourceDowntimes)
                        ()
                    | TransportDelays transportDelays ->
                        // do! supplyContext.TransportDelay.DefineBulk(transportDelays)
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
        let skus = masterDataContext.Sku.QueryService.GetAll().Result

        let skuRows =
            skus
            |> List.map (fun s -> [| s.Id; s.Code; s.Name |])
            |> List.toArray

        printTable "SKUs IN DATABASE" [| "SKU ID"; "CODE"; "NAME" |] skuRows

        // 2. Stocking Points Table
        let sps =
            masterDataContext.StockingPoint.QueryService
                .GetAll()
                .Result

        let spRows =
            sps
            |> List.map (fun s -> [| s.Id; s.PlantId; s.Name; s.Type |])
            |> List.toArray

        printTable "STOCKING POINTS IN DATABASE" [| "STOCKING POINT ID"; "PLANT ID"; "NAME"; "TYPE" |] spRows

        // 3. BOM Table
        let boms = masterDataContext.Bom.QueryService.GetAll().Result

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
            masterDataContext.Routing.QueryService
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
            masterDataContext.TransportLeg.QueryService
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
            masterDataContext.Plant.QueryService
                .GetAll()
                .Result

        let plantRows =
            plants
            |> List.map (fun p -> [| p.Id; p.Code; p.Name; (if p.Status then "Active" else "Inactive") |])
            |> List.toArray

        printTable "PLANTS IN DATABASE" [| "PLANT ID"; "CODE"; "NAME"; "STATUS" |] plantRows

        // 7. Unit Conversions Table
        let conversions =
            masterDataContext.UnitConversion.QueryService
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
            supplyContext.Inventory.QueryService
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
            supplyContext.InventoryTarget.QueryService
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
            supplyContext.SupplierOffer.QueryService
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
            supplyContext.SupplyOrder.QueryService
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
            supplyContext.MaterialReservation.QueryService
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
                let! list = masterDataContext.Routing.QueryService.GetAll()

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
            printfn "6. Exit"

            printf "Select option (1-8): "
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
                exit <- true
                printColorLine "cyan" "\nExiting Medhāvī Simulator. Goodbye!"
            | _ -> printColorLine "red" "Invalid choice. Please enter 1-8."

        masterDataContext.Dispose()
        supplyContext.Dispose()
        capacityContext.Dispose()
        transportContext.Dispose()

        0
