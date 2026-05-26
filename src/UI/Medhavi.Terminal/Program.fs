namespace Medhavi.Terminal

open System
open System.Collections.Generic
open System.Threading.Tasks
open Medhavi.Contracts
open Medhavi.Contracts.Integration
open Medhavi.Contracts.Domain
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Aggregate
open Medhavi.Common.Validation
open Medhavi.MasterData.Domain.UomAgg
open Medhavi.MasterData.Domain.SkuAgg
open Medhavi.MasterData.Domain.PlantAgg
open Medhavi.MasterData.Domain.StockingPointAgg
open Medhavi.MasterData.Domain.NodeAgg
open Medhavi.MasterData.Domain.BoMAgg
open Medhavi.MasterData.Domain.RoutingAgg
open Medhavi.MasterData.Domain.UnitConversionAgg
open Medhavi.Domain.Transport
open Medhavi.MasterData.Application
open Medhavi.Integration
open Medhavi.Infrastructure.Stores.InMemRepository
open Medhavi.Infrastructure.Stores.EnvelopeStore
open Medhavi.Infrastructure.Stores.EnvelopeStoreMem

module Program =

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

    // In-memory repositories for database snapshots
    let uomRepo = createInMemoryRepository<UnitOfMeasure, string, UnitOfMeasureEvent> ()
    let conversionRepo = createInMemoryRepository<UnitConversion, string, UnitConversionEvent> ()
    let skuRepo = createInMemoryRepository<Sku, string, SkuEvent> ()
    let plantRepo = createInMemoryRepository<Plant, string, PlantEvent> ()
    let stockingPointRepo = createInMemoryRepository<StockingPoint, string, StockingPointEvent> ()
    let nodeRepo = createInMemoryRepository<Node, string, NodeEvent> ()
    let bomRepo = createInMemoryRepository<BillOfMaterial, string, BomEvent> ()
    let routingRepo = createInMemoryRepository<Routing, string, RoutingEvent> ()
    let transportLegRepo = createInMemoryRepository<TransportLeg, string, TransportLegEvent> ()

    // Initialize Application layer services
    let uomCaps = Uom.createCapabilities uomRepo
    let conversionCaps = UoMConversion.createCapabilities conversionRepo
    let skuCaps = Sku.createCapabilities skuRepo
    let plantCaps = Plant.createCapabilities plantRepo
    let stockingPointCaps = StockingPoint.createCapabilities stockingPointRepo
    let nodeCaps = Node.createCapabilities nodeRepo
    let bomCaps = BillOfMaterials.createCapabilities bomRepo
    let routingCaps = Routing.createCapabilities routingRepo
    let transportLegCaps = TransportLeg.createCapabilities transportLegRepo

    // Real EnvelopeStore instance (Mailbox-agent based)
    let envelopeStore = createEnvelopeStoreMem ()
    let mutable subscriptionHandle : SubscriptionHandle option = None

    // Sample static CSV files contents
    let productsCsv = 
        "SkuId,Name,UoM,IsActive\n" +
        "SKU-BIKE,Mountain Bike,UOM-PCS,true\n" +
        "SKU-FRAME,Alloy Bike Frame,UOM-PCS,true\n" +
        "SKU-WHEEL,Heavy Duty Wheel,UOM-PCS,true"

    let stockingPointsCsv =
        "StockingPointId,Name,IsActive\n" +
        "SP-FACTORY,Assembly Plant,true\n" +
        "SP-WAREHOUSE,Central Warehouse,true"

    let resourcesCsv =
        "ResourceId,Name,NodeId,IsActive\n" +
        "RES-LINE1,Assembly Line 1,SP-FACTORY,true\n" +
        "RES-LINE2,Assembly Line 2,SP-FACTORY,true"

    let bomsCsv =
        "ParentSkuId,ComponentSkuId,QuantityRequired\n" +
        "SKU-BIKE,SKU-FRAME,1.0\n" +
        "SKU-BIKE,SKU-WHEEL,2.0"

    let routingsCsv =
        "SkuId,Sequence,ResourceId,SetupHours,RunHoursPerUnit\n" +
        "SKU-BIKE,10,RES-LINE1,1.0,0.5\n" +
        "SKU-BIKE,20,RES-LINE2,0.5,0.25"

    let transportLegsCsv =
        "Id,Origin,Destination,Mode,Schedule,LeadTimeMinutes,Capacity,CapacityUnit,CutoffMinutes,Constraints,Reliability,CO2PerUnit,EffectiveStart\n" +
        "LEG-FACTORY-WH,SP-FACTORY,SP-WAREHOUSE,Road,Daily,120.0,500.0,UOM-PCS,60.0,,0.98,0.02,2026-05-27T00:00:00Z"

    // -------------------------------------------------------------
    // Table Helper Printer
    // -------------------------------------------------------------
    let printTable (title: string) (headers: string[]) (rows: string[][]) =
        printColorLine "cyan" ("\n┌" + String.replicate 78 "─" + "┐")
        printfn "│ %-76s │" title
        printColorLine "cyan" ("├" + String.replicate 78 "─" + "┤")
        
        let colCount = headers.Length
        let colWidths = Array.init colCount (fun i ->
            let headerWidth = headers.[i].Length
            let rowMax = if rows.Length = 0 then 0 else rows |> Seq.map (fun r -> r.[i].Length) |> Seq.max
            max headerWidth rowMax + 2
        )
        
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
        let handler (envelopedEvent: EnvelopedEvent) : Task<unit> =
            task {
                let envelope = envelopedEvent.Envelope
                match IntegrationEventEnvelope.tryGetPayload envelope with
                | Error err -> printColorLine "red" (sprintf "   [ ERR ] [Subscription] Failed to deserialize envelope: %A" err)
                | Ok event ->
                    match event with
                    | MasterDataImported payload ->
                        printColorLine "cyan" "\n>>> [Subscription Broker] Received MasterDataImported Event! Dispatching to MasterData Bounded Context..."
                        
                        // Setup Default Base UOM
                        let baseUomReq : UomDefineReq = { Id = "UOM-DEFAULT"; Code = "PCS"; Name = "Pieces"; IsBase = true; ToBaseFactor = 1.0m; Created = DateTimeOffset.UtcNow }
                        let _ = (uomCaps.Define baseUomReq).Result

                        // 1. SKUs
                        printColorLine "cyan" "\n  [MasterData BC] Ingesting SKUs..."
                        for p in payload.Products do
                            let req : SkuDefineReq = { Id = p.SkuId; Code = p.SkuId; Name = p.Name; Group = "Simulation"; Created = DateTimeOffset.UtcNow }
                            let res = (skuCaps.Define req).Result
                            match res with
                            | Ok _ -> 
                                printf "    - SKU Ingested: %s " p.SkuId
                                printColorLine "green" "[ OK ]"
                            | Error err -> 
                                printf "    - SKU Ingestion Error: %s " p.SkuId
                                printColorLine "red" (sprintf "[ ERR: %A ]" err)

                        // 2. Stocking Points
                        printColorLine "cyan" "\n  [MasterData BC] Ingesting Plants & Stocking Points..."
                        let plantReq : PlantDefineReq = { Id = "PLANT-DEFAULT"; Code = "PL-DEF"; Name = "Main Production Plant" }
                        let _ = (plantCaps.Define plantReq).Result

                        for sp in payload.StockingPoints do
                            let spReq : StockingPointDefineReq = {
                                Id = sp.StockingPointId
                                PlantId = "PLANT-DEFAULT"
                                Code = sp.StockingPointId
                                Name = sp.Name
                                Type = "Warehouse"
                                Location = None
                                Level = None
                                PlanningLevel = None
                                SupplyCanBeSplit = false
                            }
                            let spRes = (stockingPointCaps.Define spReq).Result
                            
                            // Define corresponding node
                            let nodeReq : NodeDefineReq = {
                                Id = sp.StockingPointId
                                Code = sp.StockingPointId
                                Name = sp.Name
                                Type = "StockingPoint"
                                Attributes = { LocationCode = None; PlanningLevel = None; StockingPointRef = Some sp.StockingPointId }
                                Created = DateTimeOffset.UtcNow
                            }
                            let nodeRes = (nodeCaps.Define nodeReq).Result

                            match spRes, nodeRes with
                            | Ok _, Ok _ -> 
                                printf "    - Stocking Point & Node Ingested: %s " sp.StockingPointId
                                printColorLine "green" "[ OK ]"
                            | _ -> 
                                printf "    - Stocking Point Ingestion Error: %s " sp.StockingPointId
                                printColorLine "red" "[ ERR ]"

                        // 3. BOMs
                        printColorLine "cyan" "\n  [MasterData BC] Ingesting Bill of Materials..."
                        let groupedBoms = payload.Boms |> List.groupBy (fun b -> b.ParentSkuId)
                        for parentSkuId, lines in groupedBoms do
                            let items : BomItemReq list = 
                                lines 
                                |> List.mapi (fun idx b -> 
                                    { ComponentSkuId = b.ComponentSkuId
                                      Quantity = b.QuantityRequired
                                      UnitOfMeasureId = "UOM-DEFAULT"
                                      Sequence = (idx + 1) * 10 })
                            let req : BomDefineReq = { Id = $"BOM-{parentSkuId}"; SkuId = parentSkuId; Items = items }
                            let res = (bomCaps.Define req).Result
                            match res with
                            | Ok _ -> 
                                printf "    - BOM Ingested for Parent Sku: %s " parentSkuId
                                printColorLine "green" "[ OK ]"
                            | Error err -> 
                                printf "    - BOM Ingestion Error: %s " parentSkuId
                                printColorLine "red" (sprintf "[ ERR: %A ]" err)

                        // 4. Routings
                        printColorLine "cyan" "\n  [MasterData BC] Ingesting Routings & Resources..."
                        for r in payload.Routings do
                            let steps : RoutingStepReq list = 
                                r.Steps 
                                |> List.map (fun s -> 
                                    { StepId = $"STEP-{r.SkuId}-{s.Sequence}"
                                      Sequence = s.Sequence
                                      ResourceGroupId = Some s.ResourceId
                                      Yield = None })
                            let stepResources : StepResourceReq list =
                                r.Steps
                                |> List.map (fun s ->
                                    { StepId = $"STEP-{r.SkuId}-{s.Sequence}"
                                      ResourceId = s.ResourceId
                                      IsAllowed = true
                                      Sequence = s.Sequence
                                      DurationPerUnitMinutes = Some (decimal (s.RunHoursPerUnit * 60.0)) })

                            let firstStepOpt : RoutingStepImportedPayload option = r.Steps |> List.sortBy (fun s -> s.Sequence) |> List.tryHead
                            let lastStepOpt : RoutingStepImportedPayload option = r.Steps |> List.sortBy (fun s -> s.Sequence) |> List.tryLast

                            let firstStepId = 
                                match firstStepOpt with
                                | Some s -> $"STEP-{r.SkuId}-{s.Sequence}"
                                | None -> $"STEP-{r.SkuId}-10"
                            let lastStepId = 
                                match lastStepOpt with
                                | Some s -> $"STEP-{r.SkuId}-{s.Sequence}"
                                | None -> $"STEP-{r.SkuId}-10"

                            let getStepNodeId (stepOpt: RoutingStepImportedPayload option) =
                                match stepOpt with
                                | None -> "SP-FACTORY"
                                | Some s ->
                                    payload.Resources 
                                    |> List.tryFind (fun res -> res.ResourceId = s.ResourceId)
                                    |> Option.map (fun res -> res.NodeId)
                                    |> Option.defaultValue "SP-FACTORY"
                            let firstStepNodeId = getStepNodeId firstStepOpt
                            let lastStepNodeId = getStepNodeId lastStepOpt

                            let bomLines = payload.Boms |> List.filter (fun b -> b.ParentSkuId = r.SkuId)

                            let inputs : RoutingInputReq list =
                                if List.isEmpty bomLines then
                                    [ { StepId = firstStepId
                                        SkuId = "SKU-FRAME"
                                        NodeId = firstStepNodeId
                                        ConversionRate = Some 1.0m } ]
                                else
                                    bomLines |> List.map (fun b ->
                                        { StepId = firstStepId
                                          SkuId = b.ComponentSkuId
                                          NodeId = firstStepNodeId
                                          ConversionRate = Some b.QuantityRequired })

                            let outputs : RoutingOutputReq list =
                                [ { StepId = lastStepId
                                    SkuId = r.SkuId
                                    NodeId = lastStepNodeId
                                    ConversionRate = Some 1.0m
                                    IsCoSku = false } ]

                            let req : RoutingDefineReq = {
                                Id = $"ROUTING-{r.SkuId}"
                                Name = $"Routing for {r.SkuId}"
                                Type = "Work"
                                EffectiveStart = DateTimeOffset.UtcNow
                                EffectiveEnd = None
                                Steps = steps
                                Inputs = inputs
                                Outputs = outputs
                                StepResources = stepResources
                                Created = DateTimeOffset.UtcNow
                            }
                            let res = (routingCaps.Define req).Result
                            match res with
                            | Ok _ -> 
                                printf "    - Routing & Steps Ingested for Sku: %s " r.SkuId
                                printColorLine "green" "[ OK ]"
                            | Error err -> 
                                printf "    - Routing Ingestion Error: %s " r.SkuId
                                printColorLine "red" (sprintf "[ ERR: %A ]" err)

                        // 5. Transport Legs Ingest
                        printColorLine "cyan" "\n  [MasterData BC] Ingesting Transport Legs..."
                        let legs = InboundAdapter.parseTransportLegCsv transportLegsCsv |> Result.defaultValue []
                        for leg in legs do
                            let res = (transportLegCaps.Define leg).Result
                            match res with
                            | Ok _ -> 
                                printf "    - Transport Leg Ingested: %s (%s -> %s) " leg.Id leg.Origin leg.Destination
                                printColorLine "green" "[ OK ]"
                            | Error err -> 
                                printf "    - Transport Leg Ingestion Error: %s " leg.Id
                                printColorLine "red" (sprintf "[ ERR: %A ]" err)
                        
                        printColorLine "green" "\n   [ SUCCESS ] All events processed and committed to Bounded Context DBs."

                    | _ -> printfn "   [ INFO ] [Subscription] Received event: %A" event
            }

        let subscribeTask = envelopeStore.Subscribe SubscriptionMode.All None handler System.Threading.CancellationToken.None
        match Async.RunSynchronously subscribeTask with
        | Error err -> printfn "   [ ERR ] Failed to subscribe bounded context handlers: %A" err
        | Ok handle ->
            subscriptionHandle <- Some handle
            printColorLine "green" "   [ OK ] Bounded Context subscriptions established with EnvelopeStore. Standing by..."

    // -------------------------------------------------------------
    // Menu Actions
    // -------------------------------------------------------------
    let loadAndValidateCsv() =
        printColorLine "bold" "\n--- [STEP 1: LOAD, VALIDATE CSV DATA & PUBLISH ENVELOPE] ---"
        try
            printfn "Loading CSV files from virtual ingestion pool..."
            
            let products = InboundAdapter.parseProductCsv productsCsv |> Result.defaultWith (fun e -> failwith e)
            let stockingPoints = InboundAdapter.parseStockingPointCsv stockingPointsCsv |> Result.defaultWith (fun e -> failwith e)
            let resources = InboundAdapter.parseResourceCsv resourcesCsv |> Result.defaultWith (fun e -> failwith e)
            let boms = InboundAdapter.parseBomLineCsv bomsCsv |> Result.defaultWith (fun e -> failwith e)
            let routings = InboundAdapter.parseRoutingCsv routingsCsv |> Result.defaultWith (fun e -> failwith e)
            
            let payload = {
                Products = products
                Boms = boms
                StockingPoints = stockingPoints
                Resources = resources
                Routings = routings
                Suppliers = []
            }

            printfn "Executing Anti-Corruption Layer (ACL) reference checks..."
            match MasterDataValidator.validate payload with
            | Invalid errors ->
                printColor "red" "   [ ERR ] Validation failed with the following errors:\n"
                for err in errors do
                    printfn "     - %s" err
            | Valid validPayload ->
                printColorLine "green" "   [ OK ] MasterDataPayload reference integrity check passed."
                
                // Wrap in integration envelope
                let tenantId = "tenant-mountain-bike"
                let correlationId = Guid.NewGuid()
                let event = MasterDataImported validPayload
                
                match IntegrationEventEnvelope.create tenantId correlationId event with
                | Error err -> printfn "   [ ERR ] Serialization failed: %A" err
                | Ok envelope ->
                    // Publish to EnvelopeStore
                    let publishTask = envelopeStore.PublishSingle "master-data-stream" envelope ExpectedRevision.Any System.Threading.CancellationToken.None
                    match Async.RunSynchronously publishTask with
                    | Error err -> printColorLine "red" (sprintf "   [ ERR ] Failed to write to outbox: %A" err)
                    | Ok _ ->
                        printColor "green" "   [ OK ] "
                        printfn "Integration Envelope successfully written to EnvelopeStore (outbox)."
                        printfn "          Envelope ID: %s" (envelope.EventId.ToString())
                        printfn "          Stream: master-data-stream"

        with ex ->
            printColorLine "red" (sprintf "   [ ERR ] Exception occurred during parsing: %s" ex.Message)

    let viewEnvelopesInStore() =
        printColorLine "bold" "\n--- [STEP 2: VIEW OUTBOX ENVELOPES IN STORE] ---"
        let readTask = envelopeStore.ReadAll None None System.Threading.CancellationToken.None
        match Async.RunSynchronously readTask with
        | Error err -> printColorLine "red" (sprintf "   [ ERR ] Failed to read from EnvelopeStore: %A" err)
        | Ok envelopes ->
            if envelopes.Length = 0 then
                printColorLine "yellow" "   [ WARN ] EnvelopeStore is currently empty. Please load CSV data first (Option 1)."
            else
                printfn "Current Envelopes in Store:"
                for env in envelopes do
                    printfn "  - Envelope ID: %s | Type: %s | Stream: %s | GlobalPos: %A" 
                        (env.Envelope.EventId.ToString()) 
                        env.Envelope.EventType 
                        (env.ReadFrom |> Option.defaultValue "Unknown")
                        (env.Position.GlobalPosition |> Option.map string |> Option.defaultValue "None")

    let showDashboard() =
        printColorLine "bold" "\n================================================================================"
        printColorLine "bold" "                     MEDHĀVĪ AGGREGATE DB SNAPSHOT DASHBOARD                     "
        printColorLine "bold" "================================================================================"

        // 1. SKUs Table
        let skus = (skuRepo.GetAll()).Result |> Result.defaultValue []
        let skuRows = skus |> List.map (fun s -> [| SkuId.value s.Id; s.Code; s.Name |]) |> List.toArray
        printTable "SKUs IN DATABASE" [| "SKU ID"; "CODE"; "NAME" |] skuRows

        // 2. Stocking Points Table
        let sps = (stockingPointRepo.GetAll()).Result |> Result.defaultValue []
        let spRows = sps |> List.map (fun s -> 
            let spTypeStr = 
                match s.Type with
                | StockingPointType.Plant -> "Plant"
                | StockingPointType.DistributionCenter -> "DistributionCenter"
                | StockingPointType.Warehouse -> "Warehouse"
            [| StockingPointId.value s.Id; PlantId.value s.PlantId; s.Name; spTypeStr |]) |> List.toArray
        printTable "STOCKING POINTS IN DATABASE" [| "STOCKING POINT ID"; "PLANT ID"; "NAME"; "TYPE" |] spRows

        // 3. BOM Table
        let boms = (bomRepo.GetAll()).Result |> Result.defaultValue []
        let bomRows = 
            boms 
            |> List.collect (fun b -> 
                b.Items 
                |> List.map (fun i -> [| BillOfMaterialId.value b.Id; SkuId.value b.SkuId; SkuId.value i.ComponentSkuId; (float (Qty.value i.Quantity)).ToString() |]))
            |> List.toArray
        printTable "BILL OF MATERIALS (BOM) RELATIONSHIPS" [| "BOM ID"; "PARENT SKU ID"; "COMPONENT SKU ID"; "QTY REQUIRED" |] bomRows

        // 4. Routings Table
        let routings = (routingRepo.GetAll()).Result |> Result.defaultValue []
        let routingRows = 
            routings
            |> List.collect (fun r ->
                r.Steps
                |> List.map (fun s -> 
                    let resIdStr = s.ResourceGroupId |> Option.map ResourceGroupId.value |> Option.defaultValue ""
                    let yieldStr = s.Yield |> Option.map (fun y -> y.ToString()) |> Option.defaultValue "1.0"
                    [| RoutingId.value r.Id; s.StepId; s.Sequence.ToString(); resIdStr; yieldStr |]))
            |> List.toArray
        printTable "ROUTINGS AND PRODUCTION STEPS" [| "ROUTING ID"; "STEP ID"; "SEQ"; "RESOURCE GROUP"; "YIELD" |] routingRows

        // 5. Transport Legs Table
        let legs = (transportLegRepo.GetAll()).Result |> Result.defaultValue []
        let legRows =
            legs
            |> List.map (fun l ->
                let modeStr = 
                    match l.Mode with
                    | TransportMode.Air -> "Air"
                    | Road -> "Road"
                    | Rail -> "Rail"
                    | Sea -> "Sea"
                    | Pipeline -> "Pipeline"
                    | TransportMode.Other s -> s
                let capStr = l.Capacity |> Option.map (fun c -> PositiveDecimal.value c |> float) |> Option.map (fun f -> f.ToString()) |> Option.defaultValue "Uncapped"
                [| TransportLegId.value l.Id; StockingPointId.value l.Origin; StockingPointId.value l.Destination; modeStr; l.LeadTime.TotalMinutes.ToString() + "m"; capStr |])
            |> List.toArray
        printTable "TRANSPORT LEGS (LOGISTICS LANES)" [| "LEG ID"; "ORIGIN SP"; "DESTINATION SP"; "MODE"; "LEAD TIME"; "CAPACITY" |] legRows

    [<EntryPoint>]
    let main argv =
        printColorLine "bold" "========================================================="
        printColorLine "cyan" "     MEDHĀVĪ CONTROL TOWER - INTEGRATION SIMULATOR       "
        printColorLine "bold" "========================================================="
        
        subscribeBoundedContexts()
        
        let mutable exit = false
        while not exit do
            printColorLine "bold" "\nMAIN MENU"
            printfn "1. Load, Validate and Publish CSV (Generates & Writes Envelope)"
            printfn "2. View Outbox Envelopes inside EnvelopeStore"
            printfn "3. View Aggregate Database Snapshot Dashboard"
            printfn "4. Run End-to-End Automated Demo"
            printfn "5. Exit"
            
            printf "Select option (1-5): "
            let choice = Console.ReadLine()
            match choice with
            | "1" -> loadAndValidateCsv()
            | "2" -> viewEnvelopesInStore()
            | "3" -> showDashboard()
            | "4" -> 
                printColorLine "cyan" "\n>>> RUNNING END-TO-END AUTOMATED DEMO <<<"
                loadAndValidateCsv()
                // Wait briefly for the background subscriber thread to process and commit
                System.Threading.Thread.Sleep(1000)
                showDashboard()
            | "5" -> 
                exit <- true
                printColorLine "cyan" "\nExiting Medhāvī Simulator. Goodbye!"
            | _ -> 
                printColorLine "red" "Invalid choice. Please enter 1-5."

        0
