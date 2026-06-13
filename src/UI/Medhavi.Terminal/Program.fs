namespace Medhavi.Terminal

open System
open System.Threading.Tasks
open Medhavi.Infrastructure.Stores.EnvelopeStore
open Medhavi.Infrastructure.Stores.EnvelopeStoreMem
open Medhavi.Infrastructure.Stores.InMemRepository
open Medhavi.Integration
open Medhavi.Capacity
open Medhavi.MasterData
open Medhavi.SharedKernel.BoundedContexts
open Medhavi.Supply
open Medhavi.Transport
open Medhavi.Terminal.Demand
open Medhavi.Scenario.Domain

module Program =
    let printer = Printer.create ()
    // Initialize Bounded Contexts via modular composition roots
    let masterDataContext = Medhavi.MasterData.BoundedContext.create ()
    let supplyContext = Medhavi.Supply.BoundedContext.create ()
    let capacityContext = Medhavi.Capacity.BoundedContext.create ()
    let demandContext = Medhavi.Demand.BoundedContext.create ()

    let scenarioRepo = createInMemoryRepository<Scenario, string, ScenarioEvent> ()

    let configRepo =
        createInMemoryRepository<ScenarioConfiguration, string, ScenarioConfigurationEvent> ()

    let overlayRepo =
        createInMemoryRepository<ScenarioOverlaySet, string, ScenarioOverlayEvent> ()

    let scenarioContext =
        Medhavi.Scenario.BoundedContext.create scenarioRepo configRepo overlayRepo

    let transportContext =
        Medhavi.Transport.BoundedContext.create (MasterData.getTransportLegs masterDataContext)

    // Real EnvelopeStore instance (Mailbox-agent based)
    let envelopeStore = createEnvelopeStoreMem ()
    let mutable subscriptionHandle: SubscriptionHandle option = None

    let integrationCaps = IntegrationService.createCapabilities envelopeStore

    let subscribeBoundedContexts mrpDep =
        printer.PrintLine Bold "\n--- [INITIALIZING EVENT BOUNDED CONTEXT SUBSCRIPTIONS] ---"

        let logger =
            { LogInfo = fun msg -> printer.PrintLine Cyan msg
              LogSuccess = fun msg -> printer.PrintLine PrinterColor.Green msg
              LogError = fun msg -> printer.PrintLine PrinterColor.Red msg }

        let handler (envelopedEvent: EnvelopedEvent) : Task<unit> =
            task {
                let envelope = envelopedEvent.Envelope

                match IntegrationEventEnvelope.tryGetPayload envelope with
                | Error err ->
                    printer.PrintLine
                        PrinterColor.Red
                        (sprintf "   [ ERR ] [Subscription] Failed to deserialize envelope: %A" err)
                | Ok event ->
                    printer.PrintLine
                        Cyan
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
                    | MaterialsReceived _ ->
                        Supply.handleRequest supplyContext masterDataContext logger printer event
                        |> ignore
                    | SupplyOffersImported _ ->
                        Supply.handleRequest supplyContext masterDataContext logger printer event
                        |> ignore
                    | InventoryPositionsImported _ ->
                        Supply.handleRequest supplyContext masterDataContext logger printer event
                        |> ignore
                    | InventoryTargetsImported _ ->
                        Supply.handleRequest supplyContext masterDataContext logger printer event
                        |> ignore
                    | SupplyOrdersImported _ ->
                        Supply.handleRequest supplyContext masterDataContext logger printer event
                        |> ignore
                    | MaterialReservationsImported _ ->
                        Supply.handleRequest supplyContext masterDataContext logger printer event
                        |> ignore
                    | ResourceCalendarsImported _ ->
                        Supply.handleRequest supplyContext masterDataContext logger printer event
                        |> ignore
                    | WorkOrdersCompleted _ ->
                        Supply.handleRequest supplyContext masterDataContext logger printer event
                        |> ignore
                    | ResourceDowntimes resourceDowntimes ->
                        Mrp.handleRequest mrpDep masterDataContext printer event
                        |> ignore
                    | TransportDelays transportDelays ->
                        Mrp.handleRequest mrpDep masterDataContext printer event
                        |> ignore
                    | DemandsImported req -> failwith "Not Implemented"
            }

        let subscribeTask =
            envelopeStore.Subscribe SubscriptionMode.All None handler System.Threading.CancellationToken.None

        match subscribeTask.Result with
        | Error err -> printfn "   [ ERR ] Failed to subscribe bounded context handlers: %A" err
        | Ok handle ->
            subscriptionHandle <- Some handle

            printer.PrintLine
                PrinterColor.Green
                "   [ OK ] Bounded Context subscriptions established with EnvelopeStore. Standing by..."

    let viewEnvelopesInStore () =
        printer.PrintLine Bold "\n--- [STEP 2: VIEW OUTBOX ENVELOPES IN STORE] ---"

        let readTask =
            envelopeStore.ReadAll None None System.Threading.CancellationToken.None

        match readTask.Result with
        | Error err ->
            printer.PrintLine PrinterColor.Red (sprintf "   [ ERR ] Failed to read from EnvelopeStore: %A" err)
        | Ok envelopes ->
            if envelopes.Length = 0 then
                printer.PrintLine
                    Yellow
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
        printer.PrintLine Bold "==================================================================="
        printer.PrintLine Bold "                  MEDHĀVĪ AGGREGATE DASHBOARD                      "
        printer.PrintLine Bold "==================================================================="

        MasterData.printData masterDataContext printer
        Supply.showData supplyContext printer
        Demand.showData demandContext printer
        Capacity.showData capacityContext printer

    [<EntryPoint>]
    let main argv =
        printer.PrintLine Bold "========================================================="
        printer.PrintLine Cyan "     MEDHĀVĪ TERMINAL APPLICATION                        "
        printer.PrintLine Bold "========================================================="

        // Bootstrap projections
        masterDataContext.Initialize().Wait()
        supplyContext.Initialize().Wait()
        capacityContext.Initialize().Wait()
        transportContext.Initialize().Wait()
        scenarioContext.Initialize().Wait()
        demandContext.Initialize().Wait()

        let mrpDep =
            SchedulerWiring.buildMrpDependencies masterDataContext supplyContext capacityContext demandContext

        subscribeBoundedContexts mrpDep

        seedDemands demandContext

        let kpiQueryService =
            Medhavi.Nexus.AnalyticsWiring.bootstrapAnalytics
                demandContext.Queries.DemandLine
                supplyContext.Queries
                capacityContext
                transportContext
                scenarioContext.Queries

        let mutable exit = false

        while not exit do
            printer.PrintLine Bold "\nMAIN MENU"
            printfn "1. Publish Master Data CSV"
            printfn "2. Show Dashboard"
            printfn "3. View Event store"
            printfn "4. Run CTP Capacity Check Demo"
            printfn "5. Run Transport ATP Demo (K-Shortest Paths)"
            printfn "6. Run Baseline MRP Plan"
            printfn "7. View Planning Projections & KPIs"
            printfn "8. Exit"

            printf "Select option (1-8): "
            let choice = Console.ReadLine()

            match choice with
            | "1" -> MasterData.loadAndValidateCsv integrationCaps printer
            | "2" -> showDashboard ()
            | "3" -> viewEnvelopesInStore ()
            | "4" -> Capacity.runCapacityCheckDemo capacityContext masterDataContext printer
            | "5" ->
                // Invalidate transport cache so fresh legs are used after CSV load
                transportContext.Atp.InvalidateCache()
                Transport.runDemo transportContext printer
            | "6" ->
                (Mrp.runBaselineMrp mrpDep demandContext printer)
                    .Wait()
            | "7" ->
                (task {
                    printer.PrintLine Bold "\n--- [STEP 7: VIEW PLANNING PROJECTIONS & KPIs] ---"

                    let request: Medhavi.Analytics.KPI.KpiQueryRequest =
                        { PlantId = "PLANT-DEFAULT"
                          Context = Medhavi.Contracts.Analytics.PlanContext.Live
                          Periods =
                            [ for i in -5 .. 25 do
                                  Medhavi.Contracts.Analytics.PlanningPeriod.PlanningDay(
                                      DateOnly.FromDateTime(DateTime.UtcNow.AddDays(float i))
                                  ) ]
                          SkuFilter = None }

                    let! snapshots = kpiQueryService.GetKpiSnapshots(request)
                    printer.PrintLine Bold "\nKPI SUMMARY STATUS:"

                    for s in snapshots do
                        let statusColor =
                            match s.Status with
                            | Medhavi.Analytics.KPI.Good -> PrinterColor.Green
                            | Medhavi.Analytics.KPI.Warning -> Yellow
                            | Medhavi.Analytics.KPI.Critical -> PrinterColor.Red
                            | Medhavi.Analytics.KPI.NoTarget -> PrinterColor.Cyan

                        printer.PrintLine
                            statusColor
                            (sprintf
                                "  - %s: Value = %.2f%s | Target = %.2f | Status = %A"
                                s.Name
                                s.Value
                                s.Unit
                                (s.Target |> Option.defaultValue 0.0m)
                                s.Status)
                })
                    .Wait()
            | "8" ->
                exit <- true
                printer.PrintLine Cyan "\nExiting Medhāvī Simulator. Goodbye!"
            | _ -> printer.PrintLine PrinterColor.Red "Invalid choice. Please enter 1-8."

        masterDataContext.Dispose()
        supplyContext.Dispose()
        capacityContext.Dispose()
        transportContext.Dispose()
        scenarioContext.Dispose()
        demandContext.Dispose()

        0
