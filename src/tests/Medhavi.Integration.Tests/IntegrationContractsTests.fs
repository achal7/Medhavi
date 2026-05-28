namespace Medhavi.Integration.Tests

open System
open Expecto
open Swensen.Unquote
open Medhavi.SharedKernel
open Medhavi.Infrastructure
open Medhavi.Contracts
open Medhavi.Contracts.Integration
open Medhavi.Integration
open Medhavi.Supply
open Medhavi.Common.Serialization

module IntegrationContractsTests =

    [<Tests>]
    let tests =
        testList
            "Integration Contracts Tests"
            [ testCase "should create Envelope with correct integration metadata and payload" (fun () ->
                  let tenantId = "tenant-test"
                  let correlationId = Guid.NewGuid()

                  let payload: MasterDataImportedPayload =
                      { SkuRequests = []
                        BomRequests = []
                        StockingPointRequests = []
                        NodeRequests = []
                        RoutingRequests = []
                        TransportLegRequests = []
                        UomRequests = []
                        InventoryTargetRequests = []
                        SupplierOfferRequests = []
                        PlantRequests = []
                        UnitConversionRequests = [] }

                  let event = MasterDataImported payload

                  let createResult = IntegrationEventEnvelope.create tenantId correlationId event

                  match createResult with
                  | Error err -> failwithf "Failed to create envelope: %A" err
                  | Ok envelope ->
                      test <@ envelope.TenantId = Some tenantId @>
                      test <@ envelope.CorrelationId = Some correlationId @>
                      test <@ envelope.EventType = "IntegrationEvent" @>
                      test <@ envelope.EventId <> Guid.Empty @>
                      let timeDiff = DateTimeOffset.UtcNow - envelope.CreatedUtc
                      let totalSeconds = timeDiff.TotalSeconds
                      test <@ totalSeconds < 5.0 @>

                      let payloadResult = IntegrationEventEnvelope.tryGetPayload envelope

                      match payloadResult with
                      | Error err -> failwithf "Failed to extract payload: %A" err
                      | Ok extractedEvent -> test <@ extractedEvent = event @>)

              testCase "should serialize and deserialize Envelope containing IntegrationEvent successfully" (fun () ->
                  let tenantId = "tenant-test-2"
                  let correlationId = Guid.NewGuid()

                  let skuReq: SkuDefineReq =
                      { Id = "SKU-999"
                        Code = "SKU-999"
                        Name = "Widget"
                        Group = "Simulation"
                        Created = DateTimeOffset.UtcNow }

                  let payload: MasterDataImportedPayload =
                      { SkuRequests = [ skuReq ]
                        BomRequests = []
                        StockingPointRequests = []
                        NodeRequests = []
                        RoutingRequests = []
                        TransportLegRequests = []
                        InventoryTargetRequests = []
                        SupplierOfferRequests = []
                        UomRequests = []
                        PlantRequests = []
                        UnitConversionRequests = [] }

                  let event = MasterDataImported payload

                  let createResult = IntegrationEventEnvelope.create tenantId correlationId event

                  match createResult with
                  | Error err -> failwithf "Failed to create envelope: %A" err
                  | Ok envelope ->
                      let serializedResult = serialize envelope

                      match serializedResult with
                      | Error err -> failwithf "Serialization failed: %A" err
                      | Ok json ->
                          let deserializedResult = deserialize<Envelope> json

                          match deserializedResult with
                          | Error err -> failwithf "Deserialization failed: %A" err
                          | Ok desEnv ->
                              test <@ desEnv.TenantId = Some tenantId @>
                              test <@ desEnv.CorrelationId = Some correlationId @>
                              test <@ desEnv.EventId = envelope.EventId @>

                              let payloadResult = IntegrationEventEnvelope.tryGetPayload desEnv

                              match payloadResult with
                              | Error err -> failwithf "Failed to extract payload from deserialized envelope: %A" err
                              | Ok extractedEvent ->
                                  match extractedEvent with
                                  | MasterDataImported md ->
                                      test <@ md.SkuRequests.Length = 1 @>
                                      test <@ md.SkuRequests.[0].Id = "SKU-999" @>
                                  | _ -> failwith "Expected MasterDataImported payload")

              testCase "should preserve metadata when converting roundtrip for demand signals" (fun () ->
                  let tenantId = "tenant-test-3"
                  let correlationId = Guid.NewGuid()

                  let order =
                      { OrderId = "ORD-001"
                        SkuId = "SKU-100"
                        NodeId = "NODE-1"
                        Quantity = 50m
                        RequestedDateUtc = DateTimeOffset.UtcNow }

                  let payload =
                      { CustomerOrders = [ order ]
                        Forecasts = [] }

                  let event = DemandSignalsImported payload

                  let createResult = IntegrationEventEnvelope.create tenantId correlationId event

                  match createResult with
                  | Error err -> failwithf "Failed to create envelope: %A" err
                  | Ok envelope ->
                      let payloadResult = IntegrationEventEnvelope.tryGetPayload envelope

                      match payloadResult with
                      | Error err -> failwithf "Failed to extract payload: %A" err
                      | Ok extractedEvent ->
                          match extractedEvent with
                          | DemandSignalsImported ds ->
                              test <@ ds.CustomerOrders.Length = 1 @>
                              test <@ ds.CustomerOrders.[0].OrderId = "ORD-001" @>
                              test <@ ds.CustomerOrders.[0].Quantity = 50m @>
                          | _ -> failwith "Expected DemandSignalsImported payload")

              testCase "Envelope first-class TenantId and withTenantId" (fun () ->
                  let env = Envelope.createEnvelope "TestEvent" "{\"value\":42}" 1

                  // CreatedUtc must be UTC time (offset zero)
                  let envCreatedOffset = env.CreatedUtc.Offset
                  test <@ envCreatedOffset = TimeSpan.Zero @>
                  test <@ env.TenantId = None @>

                  let envWithTenant = env |> Envelope.withTenantId "tenant-123"
                  test <@ envWithTenant.TenantId = Some "tenant-123" @>
                  test <@ Map.tryFind "tenantId" envWithTenant.Metadata = Some "tenant-123" @>
                  test <@ Envelope.tryGetTenantId envWithTenant = Some "tenant-123" @>

                  // Rehydrate/re-extract should work
                  let envelopeRehydrated = Envelope.withMetadataMap envWithTenant.Metadata env
                  test <@ envelopeRehydrated.TenantId = Some "tenant-123" @>)

              testCase "should calculate correct MaterialProvider net and time-phased availability" (fun () ->
                  let supplyBC = Medhavi.Supply.BoundedContext.create ()
                  supplyBC.Initialize().Wait()

                  let targetReq: InventoryTargetDefineReq =
                      { SkuId = "SKU-BIKE"
                        StockingPointId = "SP-WAREHOUSE"
                        ReplenishmentPolicy = None
                        SafetyStockQty = Some 10.0m
                        MinQty = None
                        MaxQty = None
                        TargetServiceLevel = None
                        CoverDays = None
                        SeasonalAdjustments = []
                        EffectiveStart = None
                        EffectiveEnd = None
                        IsActive = true }

                  let defineTargetRes = supplyBC.InventoryTarget.Define(targetReq).Result
                  test <@ Result.isOk defineTargetRes @>

                  let invReq: InventoryDefineReq =
                      { Id = "INV-SKU-BIKE-SP-WAREHOUSE"
                        SkuId = "SKU-BIKE"
                        StockingPointId = "SP-WAREHOUSE"
                        Quantity = 50.0m
                        UnitOfMeasure = "UOM-PCS" }

                  let defineInvRes = supplyBC.Inventory.Define(invReq).Result
                  test <@ Result.isOk defineInvRes @>

                  let now = DateTimeOffset.UtcNow

                  let poReq: SupplyOrderCreateReq =
                      { Id = "PO-001"
                        OrderType = "PurchaseOrder"
                        SkuId = "SKU-BIKE"
                        StockingPointId = "SP-WAREHOUSE"
                        Quantity = 30.0m
                        UnitOfMeasure = "UOM-PCS"
                        RoutingId = None
                        SupplierId = None
                        IsFirm = true
                        IsExpedited = false
                        IsLocked = false
                        UsesLeadTimeQuantity = false
                        RequiredDeliveryDate = Some(now.AddDays(2.0))
                        CreatedDate = now }

                  let woReq: SupplyOrderCreateReq =
                      { Id = "WO-001"
                        OrderType = "WorkOrder"
                        SkuId = "SKU-BIKE"
                        StockingPointId = "SP-WAREHOUSE"
                        Quantity = 20.0m
                        UnitOfMeasure = "UOM-PCS"
                        RoutingId = None
                        SupplierId = None
                        IsFirm = true
                        IsExpedited = false
                        IsLocked = false
                        UsesLeadTimeQuantity = false
                        RequiredDeliveryDate = Some(now.AddDays(5.0))
                        CreatedDate = now }

                  let createPoRes = supplyBC.SupplyOrder.Create(poReq).Result
                  let createWoRes = supplyBC.SupplyOrder.Create(woReq).Result
                  test <@ Result.isOk createPoRes @>
                  test <@ Result.isOk createWoRes @>

                  System.Threading.Thread.Sleep(200)

                  let snapshotRes =
                      Medhavi.Supply.Application.MaterialProvider.getSnapshot supplyBC "SKU-BIKE" "SP-WAREHOUSE" now
                      |> Async.RunSynchronously

                  match snapshotRes with
                  | Error err -> failwithf "Failed to query snapshot: %A" err
                  | Ok snap ->
                      test <@ snap.OnHand = 50.0m @>
                      test <@ snap.Safety = 10.0m @>
                      test <@ snap.Inbound.Length = 2 @>
                      test <@ snap.Inbound.[0] |> snd = 30.0m @>
                      test <@ snap.Inbound.[1] |> snd = 20.0m @>

                      let netAvailable =
                          Medhavi.Supply.Application.MaterialProvider.calculateNetAvailable snap

                      test <@ netAvailable = 90.0m @>

                  let timePhasedRes =
                      Medhavi.Supply.Application.MaterialProvider.getTimePhasedAvailability
                          supplyBC
                          "SKU-BIKE"
                          "SP-WAREHOUSE"
                          now
                          1
                          10
                      |> Async.RunSynchronously

                  match timePhasedRes with
                  | Error err -> failwithf "Failed time phased: %A" err
                  | Ok list ->
                      test <@ list.Length = 10 @>
                      test <@ list.[0] |> snd = 40.0m @>
                      test <@ list.[2] |> snd = 70.0m @>
                      test <@ list.[5] |> snd = 90.0m @>

                  supplyBC.Dispose()) ]
