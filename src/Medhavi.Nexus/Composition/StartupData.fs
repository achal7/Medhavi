module Medhavi.Nexus.Composition.StartupData

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open System.Threading.Tasks
open Medhavi.Contracts
open Medhavi.Contracts.Core
open Medhavi.Contracts.Core.Demand
open Medhavi.Contracts.Core.Exception
open Medhavi.Contracts.Core.ItemTransition
open Medhavi.Contracts.Demand
open Medhavi.Contracts.Supply
open Medhavi.Contracts.Promise
open Medhavi.Foundation.ExecutionContext
open Medhavi.Nexus.Composition.ComposeNexus

type EnterprisePictureSeed =
    { PlanningScopeId: string
      DemandReferences: string list
      SupplyReferences: string list
      InventoryReferences: string list
      CompositionTime: DateTimeOffset
      Publish: bool
      VersionNumber: int
      PublicationTime: DateTimeOffset }

type CoreSeedDocument =
    { Demands: RecordDemandReq list
      Exceptions: ExceptionEvidenceReq list
      ItemTransitions: RecognizeItemTransitionReq list
      EnterprisePictures: EnterprisePictureSeed list }

type UnderstandingSeed =
    { PlanningScopeId: string
      EvidencePictureVersion: int64
      Publish: bool
      IsPeriodicRefresh: bool }

type DemandSeedDocument =
    { Observations: ReceiveObservationReq list
      Understanding: UnderstandingSeed list }

type SupplySeedDocument =
    { Records: ReceiveSupplyDataReq list
      Understanding: UnderstandingSeed list }

type PromiseSeedDocument =
    { CustomerProfiles: ConfigureCustomerPromiseProfileReq list
      AllocationPools: InitializeAllocationPoolReq list
      OrderBookSnapshots: IngestOrderRequest list }

let private jsonOptions =
    let opt = JsonSerializerOptions(PropertyNameCaseInsensitive = true)
    opt.Converters.Add(JsonFSharpConverter())
    opt

let private resolveDataFilePath (filename: string) : string =
    let candidates =
        [ Path.Combine(AppContext.BaseDirectory, "Data", filename)
          Path.Combine(AppContext.BaseDirectory, filename)
          Path.Combine(Directory.GetCurrentDirectory(), "Data", filename)
          Path.Combine(Directory.GetCurrentDirectory(), "src", "Medhavi.Nexus", "Data", filename)
          Path.Combine(@"d:\RnD\Medhavi\src\Medhavi.Nexus\Data", filename) ]

    candidates
    |> List.tryFind File.Exists
    |> Option.defaultValue (Path.Combine(AppContext.BaseDirectory, "Data", filename))

let private loadJson<'T> (filename: string) : 'T option =
    try
        let filePath = resolveDataFilePath filename
        if File.Exists filePath then
            let json = File.ReadAllText filePath
            let doc = JsonSerializer.Deserialize<'T>(json, jsonOptions)
            Some doc
        else
            printfn $"[StartupData] [WARN] Seed data file not found: {filePath}"
            None
    with ex ->
        printfn $"[StartupData] [ERROR] Failed to read or parse {filename}: {ex.Message}"
        None

let private seedCoreData
    (demandApi: DemandApi)
    (exceptionApi: ExceptionApi)
    (transitionApi: ItemTransitionApi)
    (epicApi: EnterprisePictureApi)
    (doc: CoreSeedDocument)
    : Task<unit> =
    task {
        printfn "[StartupData] Seeding Core Demands..."
        for req in doc.Demands do
            let! res = demandApi.Record req
            match res with
            | Ok dto -> printfn $"  [OK] Demand recorded: {dto.Id} ({dto.Item}, Qty: {dto.Quantity})"
            | Error err -> printfn $"  [FAIL] Demand record error for {req.DemandId}: {err.Message}"

        printfn "[StartupData] Seeding Core Exceptions..."
        for req in doc.Exceptions do
            let! res = exceptionApi.ProcessEvidence req
            match res with
            | Ok dto -> printfn $"  [OK] Exception registered: {dto.ExceptionId} ({dto.Classification}, Scope: {dto.AffectedScopeIdentifier})"
            | Error err -> printfn $"  [FAIL] Exception evidence error for {req.ConstraintReference}: {err.Message}"

        printfn "[StartupData] Seeding Core Item Transitions..."
        for req in doc.ItemTransitions do
            let! res = transitionApi.Recognize req
            match res with
            | Ok dto -> printfn $"  [OK] Item transition recognized: {dto.TransitionId} ({dto.SupersededItem} -> {dto.SupersedingItem})"
            | Error err -> printfn $"  [FAIL] Item transition error for {req.SupersededItem}: {err.Message}"

        printfn "[StartupData] Seeding Core Enterprise Pictures..."
        for pic in doc.EnterprisePictures do
            let composeReq: ComposePictureVersionReq =
                { PlanningScopeId = pic.PlanningScopeId
                  DemandReferences = pic.DemandReferences
                  SupplyReferences = pic.SupplyReferences
                  InventoryReferences = pic.InventoryReferences
                  CompositionTime = pic.CompositionTime }

            let! composeRes = epicApi.Compose composeReq
            match composeRes with
            | Ok epDto ->
                printfn $"  [OK] Enterprise Picture composed: {epDto.PlanningScopeId} (Versions: {epDto.Versions.Length})"
                if pic.Publish then
                    do! Task.Delay 300
                    let publishReq: PublishPictureVersionReq =
                        { PlanningScopeId = pic.PlanningScopeId
                          VersionNumber = pic.VersionNumber
                          PublicationTime = pic.PublicationTime }
                    let! pubRes = epicApi.Publish publishReq
                    match pubRes with
                    | Ok pubDto ->
                        printfn $"  [OK] Enterprise Picture published: {pubDto.PlanningScopeId} (Version: {pic.VersionNumber})"
                    | Error err ->
                        printfn $"  [FAIL] Enterprise Picture publish error for {pic.PlanningScopeId}: {err.Message}"
            | Error err ->
                printfn $"  [FAIL] Enterprise Picture compose error for {pic.PlanningScopeId}: {err.Message}"
    }

let private seedDemandData
    (obsApi: DemandObservationApi)
    (undApi: DemandUnderstandingApi)
    (doc: DemandSeedDocument)
    : Task<unit> =
    task {
        printfn "[StartupData] Seeding Demand Observations..."
        for req in doc.Observations do
            let! res = obsApi.Receive req
            match res with
            | Ok dto -> printfn $"  [OK] Observation received: {dto.ObservationId} ({dto.Item}, Qty: {dto.Quantity} {dto.UnitOfMeasure})"
            | Error err -> printfn $"  [FAIL] Observation receive error for {req.ObservationId}: {err.Message}"

        printfn "[StartupData] Seeding Demand Understanding..."
        for und in doc.Understanding do
            let revReq: ReviseDemandUnderstandingReq =
                { PlanningScopeId = und.PlanningScopeId
                  EvidencePictureVersion = und.EvidencePictureVersion }
            let! revRes = undApi.Revise revReq
            match revRes with
            | Ok dto ->
                printfn $"  [OK] Demand Understanding revised: {dto.PlanningScopeId} (Version: {dto.VersionNumber}, State: {dto.State})"
                if und.Publish then
                    do! Task.Delay 500
                    let pubReq: PublishDemandUnderstandingReq =
                        { PlanningScopeId = und.PlanningScopeId
                          IsPeriodicRefresh = und.IsPeriodicRefresh }
                    let! pubRes = undApi.Publish pubReq
                    match pubRes with
                    | Ok pubDto ->
                        printfn $"  [OK] Demand Understanding published: {pubDto.PlanningScopeId} (State: {pubDto.State})"
                    | Error err ->
                        printfn $"  [FAIL] Demand Understanding publish error: {err.Message}"
            | Error err ->
                printfn $"  [FAIL] Demand Understanding revise error: {err.Message}"
    }

let private seedSupplyData
    (recApi: SupplyDataRecordApi)
    (undApi: SupplyUnderstandingApi)
    (doc: SupplySeedDocument)
    : Task<unit> =
    task {
        printfn "[StartupData] Seeding Supply Data Records..."
        for req in doc.Records do
            let! res = recApi.Receive req
            match res with
            | Ok dto -> printfn $"  [OK] Supply record received: {dto.RecordId} ({dto.Item} @ {dto.Location}, Qty: {dto.Quantity} {dto.UnitOfMeasure}, Type: {dto.SupplyType})"
            | Error err -> printfn $"  [FAIL] Supply record receive error for {req.RecordId}: {err.Message}"

        printfn "[StartupData] Seeding Supply Understanding..."
        for und in doc.Understanding do
            let revReq: ReviseSupplyUnderstandingReq =
                { PlanningScopeId = und.PlanningScopeId
                  EvidencePictureVersion = und.EvidencePictureVersion }
            let! revRes = undApi.Revise revReq
            match revRes with
            | Ok dto ->
                printfn $"  [OK] Supply Understanding revised: {dto.PlanningScopeId} (Version: {dto.VersionNumber}, State: {dto.State})"
                if und.Publish then
                    do! Task.Delay 500
                    let pubReq: PublishSupplyUnderstandingReq =
                        { PlanningScopeId = und.PlanningScopeId
                          IsPeriodicRefresh = und.IsPeriodicRefresh }
                    let! pubRes = undApi.Publish pubReq
                    match pubRes with
                    | Ok pubDto ->
                        printfn $"  [OK] Supply Understanding published: {pubDto.PlanningScopeId} (State: {pubDto.State})"
                    | Error err ->
                        printfn $"  [FAIL] Supply Understanding publish error: {err.Message}"
            | Error err ->
                printfn $"  [FAIL] Supply Understanding revise error: {err.Message}"
    }

let private seedPromiseData
    (profileApi: CustomerPromiseProfileApi)
    (allocApi: AllocationApi)
    (orderBookApi: OrderBookApi)
    (doc: PromiseSeedDocument)
    : Task<unit> =
    task {
        printfn "[StartupData] Seeding Customer Promise Profiles..."
        for req in doc.CustomerProfiles do
            let! res = profileApi.Configure req
            match res with
            | Ok dto -> printfn $"  [OK] Customer profile configured: {dto.CustomerId} (Tier: {dto.CustomerTier}, SLA: {dto.DefaultSlaDays}d)"
            | Error err -> printfn $"  [FAIL] Customer profile error for {req.CustomerId}: {err.Message}"

        printfn "[StartupData] Seeding Allocation Pools..."
        for req in doc.AllocationPools do
            let! res = allocApi.InitializePool req
            match res with
            | Ok dto -> printfn $"  [OK] Allocation pool initialized: {dto.PoolId} ({dto.Item} @ {dto.Location}, Planned: {dto.TotalPlannedSupply})"
            | Error err -> printfn $"  [FAIL] Allocation pool error for {req.PoolId}: {err.Message}"

        printfn "[StartupData] Seeding Order Book Snapshots..."
        for req in doc.OrderBookSnapshots do
            let! res = orderBookApi.Ingest req
            match res with
            | Ok dto ->
                printfn $"  [OK] Order book ingested: {dto.SnapshotId} (Order: {dto.CustomerOrderIdentifier}, Lines: {dto.Lines.Length})"
                for line in req.Lines do
                    let valReq: ValidateOrderLineReq =
                        { SnapshotId = req.SnapshotId
                          OrderLineId = line.OrderLineId
                          EvaluationTime = DateTimeOffset.UtcNow }
                    let! valRes = orderBookApi.Validate valReq
                    match valRes with
                    | Ok _ -> printfn $"  [OK] Order line validated: {line.OrderLineId} ({line.Item}, Qty: {line.RequestedQuantity})"
                    | Error err -> printfn $"  [FAIL] Order line validation error for {line.OrderLineId}: {err.Message}"
            | Error err ->
                printfn $"  [FAIL] Order book ingestion error for {req.SnapshotId}: {err.Message}"
    }

let private seedExemplarCausalChains (nexusCtx: NexusContext) : Task<unit> =
    task {
        printfn "[StartupData] Seeding Multi-Domain End-to-End Exemplar Causal Chains..."

        // CHAIN 1: Strategic Customer Order Fulfillment (Demand -> Core -> Supply -> Promise)
        let chain1Guid = Guid.Parse("00000001-aaaa-bbbb-cccc-000000000001")
        let corr1 =
            match CorrelationId.fromString (chain1Guid.ToString()) with
            | Some c -> c
            | None -> CorrelationId.create()

        let ctx1 = ExecutionContext.fromCorrelationAndCausation corr1 None
        ExecutionContextHolder.Set(ctx1)

        try
            // Step 1: Demand Observation Ingested (Demand Domain)
            let demObsReq: ReceiveObservationReq =
                { ObservationId = "DEM-ORD-8001"
                  Item = "SKU-DRIVE-1TB"
                  Location = "LOC-CENTRAL-WH"
                  Quantity = 500.0m
                  UnitOfMeasure = "EA"
                  ObservationType = "CustomerOrder"
                  BusinessTime = DateTimeOffset.UtcNow
                  ObservationTime = DateTimeOffset.UtcNow
                  SourceSystemProvenance = "ERP-SEED" }
            let! _ = nexusCtx.Demand.UnderstandDemand.Commands.Observations.Receive demObsReq

            // Step 2: Enterprise Demand Fact Recorded (Core Domain)
            let ctx1_2 = ExecutionContext.asCausation ctx1
            ExecutionContextHolder.Set(ctx1_2)

            let coreDemReq: RecordDemandReq =
                { DemandId = "DEM-AUTH-8001"
                  Item = "SKU-DRIVE-1TB"
                  Location = "LOC-CENTRAL-WH"
                  Customer = Some "CUST-STRATEGIC-01"
                  Quantity = 500.0m
                  NeedWindowLatest = DateTimeOffset.UtcNow.AddDays(14.0)
                  NeedWindowEarliest = Some DateTimeOffset.UtcNow
                  NeedWindowPreferred = Some (DateTimeOffset.UtcNow.AddDays(10.0))
                  DemandOrigin = "CustomerOrder"
                  ParentDemand = Some "DEM-ORD-8001" }
            let! _ = nexusCtx.Core.DemandManagement.Commands.Record coreDemReq

            // Step 3: Supply Inventory Position Evaluated (Supply Domain)
            let ctx1_3 = ExecutionContext.asCausation ctx1_2
            ExecutionContextHolder.Set(ctx1_3)

            let invReq: UpdateInventoryPositionReq =
                { ItemId = "SKU-DRIVE-1TB"
                  LocationId = "LOC-CENTRAL-WH"
                  TriggeringEvidence = [ "DEM-AUTH-8001"; "BUFFER_ALLOCATED" ] }
            let! _ = nexusCtx.Supply.ManageInventory.Commands.Position.Update invReq

            // Step 4: Customer Order Book Ingested & Validated (Promise Domain)
            let ctx1_4 = ExecutionContext.asCausation ctx1_3
            ExecutionContextHolder.Set(ctx1_4)

            let snapReq: IngestOrderRequest =
                { SnapshotId = "SNAP-CHAIN-8001"
                  PlanningScopeId = "SCOPE-GLOBAL"
                  CustomerOrderIdentifier = "ORD-GLOBAL-8001"
                  CustomerId = "CUST-STRATEGIC-01"
                  OrderChannel = "DirectEnterprise"
                  OrderPriorityClass = "Tier-1 Strategic"
                  ObservationTimestamp = DateTimeOffset.UtcNow
                  Lines =
                    [ { OrderLineId = "ORD-LINE-8001"
                        Item = "SKU-DRIVE-1TB"
                        RequestedQuantity = 500.0m
                        UnitOfMeasure = "EA"
                        RequestedDeliveryDate = DateTimeOffset.UtcNow.AddDays(14.0)
                        EarliestAcceptableDate = Some DateTimeOffset.UtcNow
                        LatestAcceptableDate = Some (DateTimeOffset.UtcNow.AddDays(20.0))
                        DestinationLocation = "LOC-CENTRAL-WH"
                        ExternalLineReference = "PO-LINE-8001" } ] }
            let! snapRes = nexusCtx.Promise.UnderstandOrders.Commands.OrderBook.Ingest snapReq
            match snapRes with
            | Ok _ ->
                let valReq: ValidateOrderLineReq =
                    { SnapshotId = "SNAP-CHAIN-8001"
                      OrderLineId = "ORD-LINE-8001"
                      EvaluationTime = DateTimeOffset.UtcNow }
                let! _ = nexusCtx.Promise.UnderstandOrders.Commands.OrderBook.Validate valReq
                printfn "  [OK] Causal Chain E2E-CHAIN-SOP-001 seeded across Demand -> Core -> Supply -> Promise."
            | Error err ->
                printfn $"  [FAIL] Order book ingestion error in E2E-CHAIN-SOP-001: {err.Message}"
        finally
            ExecutionContextHolder.Clear()

        // CHAIN 2: Regional DC Replenishment (Demand -> Core -> Supply -> Promise)
        let chain2Guid = Guid.Parse("00000002-aaaa-bbbb-cccc-000000000002")
        let corr2 =
            match CorrelationId.fromString (chain2Guid.ToString()) with
            | Some c -> c
            | None -> CorrelationId.create()

        let ctx2 = ExecutionContext.fromCorrelationAndCausation corr2 None
        ExecutionContextHolder.Set(ctx2)

        try
            // Step 1: Regional Demand Replenishment Signal (Demand Domain)
            let demRplReq: ReceiveObservationReq =
                { ObservationId = "DEM-RPL-9002"
                  Item = "SKU-BATTERY-PACK-PRO"
                  Location = "LOC-WEST-PLANT"
                  Quantity = 250.0m
                  UnitOfMeasure = "EA"
                  ObservationType = "ReplenishmentOrder"
                  BusinessTime = DateTimeOffset.UtcNow
                  ObservationTime = DateTimeOffset.UtcNow
                  SourceSystemProvenance = "HUB-SEED" }
            let! _ = nexusCtx.Demand.UnderstandDemand.Commands.Observations.Receive demRplReq

            // Step 2: Authoritative Enterprise Demand Recorded (Core Domain)
            let ctx2_2 = ExecutionContext.asCausation ctx2
            ExecutionContextHolder.Set(ctx2_2)

            let coreRplReq: RecordDemandReq =
                { DemandId = "DEM-AUTH-9002"
                  Item = "SKU-BATTERY-PACK-PRO"
                  Location = "LOC-WEST-PLANT"
                  Customer = Some "CUST-MUNICH-HUB"
                  Quantity = 250.0m
                  NeedWindowLatest = DateTimeOffset.UtcNow.AddDays(7.0)
                  NeedWindowEarliest = Some DateTimeOffset.UtcNow
                  NeedWindowPreferred = Some (DateTimeOffset.UtcNow.AddDays(5.0))
                  DemandOrigin = "ReplenishmentOrder"
                  ParentDemand = Some "DEM-RPL-9002" }
            let! _ = nexusCtx.Core.DemandManagement.Commands.Record coreRplReq

            // Step 3: Production Capacity Evaluated (Supply Domain)
            let ctx2_3 = ExecutionContext.asCausation ctx2_2
            ExecutionContextHolder.Set(ctx2_3)

            let capReq: UpdateCapacityPositionReq =
                { PhysicalResourceId = "WC-ASSEMBLY-LINE-01"
                  TriggeringEvidence = [ "DEM-AUTH-9002"; "EXPEDITE_SHIFT_1" ] }
            let! _ = nexusCtx.Supply.ManageCapacity.Commands.Position.Update capReq

            // Step 4: Order Commitment Validated (Promise Domain)
            let ctx2_4 = ExecutionContext.asCausation ctx2_3
            ExecutionContextHolder.Set(ctx2_4)

            let snapRplReq: IngestOrderRequest =
                { SnapshotId = "SNAP-CHAIN-9002"
                  PlanningScopeId = "SCOPE-MUNICH"
                  CustomerOrderIdentifier = "ORD-REPLENISH-9002"
                  CustomerId = "CUST-MUNICH-HUB"
                  OrderChannel = "HubReplenish"
                  OrderPriorityClass = "Expedited"
                  ObservationTimestamp = DateTimeOffset.UtcNow
                  Lines =
                    [ { OrderLineId = "ORD-LINE-9002"
                        Item = "SKU-BATTERY-PACK-PRO"
                        RequestedQuantity = 250.0m
                        UnitOfMeasure = "EA"
                        RequestedDeliveryDate = DateTimeOffset.UtcNow.AddDays(7.0)
                        EarliestAcceptableDate = Some DateTimeOffset.UtcNow
                        LatestAcceptableDate = Some (DateTimeOffset.UtcNow.AddDays(10.0))
                        DestinationLocation = "LOC-WEST-PLANT"
                        ExternalLineReference = "HUB-LINE-9002" } ] }
            let! snapRes2 = nexusCtx.Promise.UnderstandOrders.Commands.OrderBook.Ingest snapRplReq
            match snapRes2 with
            | Ok _ ->
                let valReq2: ValidateOrderLineReq =
                    { SnapshotId = "SNAP-CHAIN-9002"
                      OrderLineId = "ORD-LINE-9002"
                      EvaluationTime = DateTimeOffset.UtcNow }
                let! _ = nexusCtx.Promise.UnderstandOrders.Commands.OrderBook.Validate valReq2
                printfn "  [OK] Causal Chain E2E-CHAIN-REPLENISH-002 seeded across Demand -> Core -> Supply -> Promise."
            | Error err ->
                printfn $"  [FAIL] Order book ingestion error in E2E-CHAIN-REPLENISH-002: {err.Message}"
        finally
            ExecutionContextHolder.Clear()

        // CHAIN 3: Supply Disruption & Quota Review (Supply -> Core -> Promise)
        let chain3Guid = Guid.Parse("00000003-aaaa-bbbb-cccc-000000000003")
        let corr3 =
            match CorrelationId.fromString (chain3Guid.ToString()) with
            | Some c -> c
            | None -> CorrelationId.create()

        let ctx3 = ExecutionContext.fromCorrelationAndCausation corr3 None
        ExecutionContextHolder.Set(ctx3)

        try
            // Step 1: Supply Capacity Bottleneck Sensed (Supply Domain)
            let capDisruptReq: UpdateCapacityPositionReq =
                { PhysicalResourceId = "WC-PACKAGING-01"
                  TriggeringEvidence = [ "TOOLING_FAILURE"; "THROUGHPUT_DEGRADED" ] }
            let! _ = nexusCtx.Supply.ManageCapacity.Commands.Position.Update capDisruptReq

            // Step 2: Planning Exception Registered (Core Domain)
            let ctx3_2 = ExecutionContext.asCausation ctx3
            ExecutionContextHolder.Set(ctx3_2)

            let excReq: ExceptionEvidenceReq =
                { ConstraintReference = "BR-S-010-MAX-HOURS"
                  Classification = "CapacityBottleneck"
                  AffectedScopeType = "PhysicalResource"
                  AffectedScopeIdentifier = "WC-PACKAGING-01"
                  EvidenceReference = Some "Packaging line down for emergency maintenance. Throughput capacity reduced by 60%."
                  Severity = Some "High"
                  EvidenceTime = DateTimeOffset.UtcNow }
            let! _ = nexusCtx.Core.ExceptionManagement.Commands.ProcessEvidence excReq

            // Step 3: Promise Allocation Evaluated under Exception (Promise Domain)
            let ctx3_3 = ExecutionContext.asCausation ctx3_2
            ExecutionContextHolder.Set(ctx3_3)

            let snapDisruptReq: IngestOrderRequest =
                { SnapshotId = "SNAP-CHAIN-7003"
                  PlanningScopeId = "SCOPE-WEST"
                  CustomerOrderIdentifier = "ORD-EXCEPTION-7003"
                  CustomerId = "CUST-STRATEGIC-01"
                  OrderChannel = "DirectEnterprise"
                  OrderPriorityClass = "Standard Commercial"
                  ObservationTimestamp = DateTimeOffset.UtcNow
                  Lines =
                    [ { OrderLineId = "ORD-LINE-7003"
                        Item = "SKU-POWER-SUPPLY-850W"
                        RequestedQuantity = 800.0m
                        UnitOfMeasure = "EA"
                        RequestedDeliveryDate = DateTimeOffset.UtcNow.AddDays(3.0)
                        EarliestAcceptableDate = Some DateTimeOffset.UtcNow
                        LatestAcceptableDate = Some (DateTimeOffset.UtcNow.AddDays(5.0))
                        DestinationLocation = "LOC-WEST-PLANT"
                        ExternalLineReference = "DISRUPT-LINE-7003" } ] }
            let! snapRes3 = nexusCtx.Promise.UnderstandOrders.Commands.OrderBook.Ingest snapDisruptReq
            match snapRes3 with
            | Ok _ ->
                let valReq3: ValidateOrderLineReq =
                    { SnapshotId = "SNAP-CHAIN-7003"
                      OrderLineId = "ORD-LINE-7003"
                      EvaluationTime = DateTimeOffset.UtcNow }
                let! _ = nexusCtx.Promise.UnderstandOrders.Commands.OrderBook.Validate valReq3
                printfn "  [OK] Causal Chain E2E-CHAIN-DISRUPTION-003 seeded across Supply -> Core -> Promise."
            | Error err ->
                printfn $"  [FAIL] Order book ingestion error in E2E-CHAIN-DISRUPTION-003: {err.Message}"
        finally
            ExecutionContextHolder.Clear()
    }

let seed (nexusCtx: NexusContext) : Task<unit> =
    task {
        printfn "========================================================="
        printfn "  Medhavi.Nexus Startup Data Seeding (JSON-Driven)"
        printfn "========================================================="

        match loadJson<CoreSeedDocument> "CoreSeedData.json" with
        | Some coreDoc ->
            do! seedCoreData
                    nexusCtx.Core.DemandManagement.Commands
                    nexusCtx.Core.ExceptionManagement.Commands
                    nexusCtx.Core.ItemTransitionManagement.Commands
                    nexusCtx.Core.EnterprisePictureManagement.Commands
                    coreDoc
        | None ->
            printfn "[StartupData] Skipping Core seeding due to missing or invalid CoreSeedData.json"

        // Allow projection agents to process mailboxes from Core events
        do! Task.Delay 600

        match loadJson<DemandSeedDocument> "DemandSeedData.json" with
        | Some demandDoc ->
            do! seedDemandData
                    nexusCtx.Demand.UnderstandDemand.Commands.Observations
                    nexusCtx.Demand.UnderstandDemand.Commands.Understanding
                    demandDoc
        | None ->
            printfn "[StartupData] Skipping Demand seeding due to missing or invalid DemandSeedData.json"

        // Allow projection agents to process mailboxes from Demand events
        do! Task.Delay 600

        match loadJson<SupplySeedDocument> "SupplySeedData.json" with
        | Some supplyDoc ->
            do! seedSupplyData
                    nexusCtx.Supply.UnderstandSupply.Commands.Records
                    nexusCtx.Supply.UnderstandSupply.Commands.Understanding
                    supplyDoc
        | None ->
            printfn "[StartupData] Skipping Supply seeding due to missing or invalid SupplySeedData.json"

        // Allow projection agents to process mailboxes from Supply records
        do! Task.Delay 600

        printfn "[StartupData] Seeding Initial Inventory Positions..."
        let invItems =
            [ ("SKU-DRIVE-1TB", "LOC-CENTRAL-WH")
              ("SKU-BATTERY-PACK-PRO", "LOC-WEST-PLANT")
              ("SKU-POWER-SUPPLY-850W", "LOC-WEST-PLANT")
              ("SKU-DRIVE-1TB", "LOC-EAST-HUB") ]

        for (item, loc) in invItems do
            let req: UpdateInventoryPositionReq =
                { ItemId = item
                  LocationId = loc
                  TriggeringEvidence = [ "INIT_SEED" ] }
            let! res = nexusCtx.Supply.ManageInventory.Commands.Position.Update req
            match res with
            | Ok dto -> printfn $"  [OK] Inventory position assessed: {dto.ItemId} @ {dto.LocationId} (Available: {dto.AvailableQuantity}, Health: {dto.HealthStatus})"
            | Error err -> printfn $"  [FAIL] Inventory position update error for {item}: {err.Message}"

        printfn "[StartupData] Seeding Initial Capacity Positions..."
        let capResources = [ "WC-ASSEMBLY-LINE-01"; "WC-PACKAGING-01" ]
        for resId in capResources do
            let req: UpdateCapacityPositionReq =
                { PhysicalResourceId = resId
                  TriggeringEvidence = [ "INIT_SEED" ] }
            let! res = nexusCtx.Supply.ManageCapacity.Commands.Position.Update req
            match res with
            | Ok dto -> printfn $"  [OK] Capacity position assessed: {dto.PhysicalResourceId} (Load: {dto.CurrentLoadHours}/{dto.AvailableCapacityHours} hrs, Status: {dto.UtilizationStatus})"
            | Error err -> printfn $"  [FAIL] Capacity position update error for {resId}: {err.Message}"

        // Allow projection agents to process mailboxes from Inventory and Capacity updates
        do! Task.Delay 600

        match loadJson<PromiseSeedDocument> "PromiseSeedData.json" with
        | Some promiseDoc ->
            do! seedPromiseData
                    nexusCtx.Promise.UnderstandOrders.Commands.CustomerProfiles
                    nexusCtx.Promise.AllocateSupply.Commands.Allocation
                    nexusCtx.Promise.UnderstandOrders.Commands.OrderBook
                    promiseDoc
        | None ->
            printfn "[StartupData] Skipping Promise seeding due to missing or invalid PromiseSeedData.json"

        // Seed multi-domain exemplar causal chains
        do! Task.Delay 600
        do! seedExemplarCausalChains nexusCtx

        // Final drain of projection mailboxes
        do! Task.Delay 600
        printfn "[StartupData] Seeding completed. All projection mailboxes processed."
        printfn "========================================================="
    }
