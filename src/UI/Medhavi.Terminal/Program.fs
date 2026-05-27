namespace Medhavi.Terminal

open System
open System.Threading.Tasks
open Medhavi.SharedKernel
open Medhavi.SharedKernel.BoundedContexts
open Medhavi.MasterData
open Medhavi.MasterData.Application
open Medhavi.Integration
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

    // Initialize MasterData Bounded Context via modular composition root
    let masterDataContext = BoundedContext.create ()

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

        let handler (envelopedEvent: EnvelopedEvent) : Task<unit> =
            task {
                let envelope = envelopedEvent.Envelope

                match IntegrationEventEnvelope.tryGetPayload envelope with
                | Error err ->
                    printColorLine "red" (sprintf "   [ ERR ] [Subscription] Failed to deserialize envelope: %A" err)
                | Ok event ->
                    match event with
                    | MasterDataImported payload ->
                        printColorLine
                            "cyan"
                            "\n>>> [Subscription Broker] Received MasterDataImported Event! Delegating processing to MasterData Bounded Context..."

                        let logger =
                            { LogInfo = fun msg -> printColorLine "cyan" msg
                              LogSuccess = fun msg -> printColorLine "green" msg
                              LogError = fun msg -> printColorLine "red" msg }

                        do! MasterDataImportedHandler.handle masterDataContext payload logger
                    | _ -> printfn "   [ INFO ] [Subscription] Received event: %A" event
            }

        let subscribeTask =
            envelopeStore.Subscribe SubscriptionMode.All None handler System.Threading.CancellationToken.None

        match Async.RunSynchronously subscribeTask with
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
        | Success(evtId, corrId) ->
            printColorLine "green" "   [ OK ] Master data successfully validated and published."
            printfn "          Envelope ID: %s" (evtId.ToString())
            printfn "          Correlation ID: %s" (corrId.ToString())
        | ValidationError errors ->
            printColor "red" "   [ ERR ] Validation failed with the following errors:\n"

            for err in errors do
                printfn "     - %s" err
        | IngestionError err -> printColorLine "red" (sprintf "   [ ERR ] Ingestion failed: %s" err)

    let viewEnvelopesInStore () =
        printColorLine "bold" "\n--- [STEP 2: VIEW OUTBOX ENVELOPES IN STORE] ---"

        let readTask =
            envelopeStore.ReadAll None None System.Threading.CancellationToken.None

        match Async.RunSynchronously readTask with
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
        let sps = masterDataContext.StockingPoint.QueryService.GetAll().Result

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
        let routings = masterDataContext.Routing.QueryService.GetAll().Result

        let routingRows =
            routings
            |> List.collect (fun r ->
                r.Steps
                |> List.map (fun s ->
                    let resIdStr = s.ResourceGroupId |> Option.defaultValue ""

                    let yieldStr =
                        s.Yield
                        |> Option.map (fun y -> y.ToString())
                        |> Option.defaultValue "1.0"

                    [| r.Id; s.StepId; s.Sequence.ToString(); resIdStr; yieldStr |]))
            |> List.toArray

        printTable
            "ROUTINGS AND PRODUCTION STEPS"
            [| "ROUTING ID"; "STEP ID"; "SEQ"; "RESOURCE GROUP"; "YIELD" |]
            routingRows

        // 5. Transport Legs Table
        let legs = masterDataContext.TransportLeg.QueryService.GetAll().Result

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
        let plants = masterDataContext.Plant.QueryService.GetAll().Result
        let plantRows =
            plants
            |> List.map (fun p -> [| p.Id; p.Code; p.Name; (if p.Status then "Active" else "Inactive") |])
            |> List.toArray
        printTable "PLANTS IN DATABASE" [| "PLANT ID"; "CODE"; "NAME"; "STATUS" |] plantRows

        // 7. Unit Conversions Table
        let conversions = masterDataContext.UnitConversion.QueryService.GetAll().Result
        let convRows =
            conversions
            |> List.map (fun c -> [| c.Id; c.FromUnitCode; c.ToUnitCode; c.Ratio.ToString(); (if c.Status then "Active" else "Inactive") |])
            |> List.toArray
        printTable "UNIT CONVERSIONS IN DATABASE" [| "CONVERSION ID"; "FROM UNIT"; "TO UNIT"; "RATIO"; "STATUS" |] convRows

    [<EntryPoint>]
    let main argv =
        printColorLine "bold" "========================================================="
        printColorLine "cyan" "     MEDHĀVĪ CONTROL TOWER - INTEGRATION SIMULATOR       "
        printColorLine "bold" "========================================================="

        subscribeBoundedContexts ()

        // Bootstrap projections
        masterDataContext.Initialize().Wait()

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
            | "1" -> loadAndValidateCsv ()
            | "2" -> viewEnvelopesInStore ()
            | "3" -> showDashboard ()
            | "4" ->
                printColorLine "cyan" "\n>>> RUNNING END-TO-END AUTOMATED DEMO <<<"
                loadAndValidateCsv ()
                // Wait briefly for the background subscriber thread to process and commit
                System.Threading.Thread.Sleep(1000)
                showDashboard ()
            | "5" ->
                exit <- true
                printColorLine "cyan" "\nExiting Medhāvī Simulator. Goodbye!"
            | _ -> printColorLine "red" "Invalid choice. Please enter 1-5."

        0
