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
    open Medhavi.Supply.Application

    [<Tests>]
    let tests =
        testList
            "Integration Contracts Tests"
            [ testCase "should create Envelope with correct integration metadata and payload" (fun () ->
                  let tenantId = "tenant-test"
                  let correlationId = Guid.NewGuid()

                  let skuReq: SkuDefineReq =
                      { Id = "SKU-999"
                        Code = "SKU-999"
                        Name = "Widget"
                        Group = "Simulation"
                        Created = DateTimeOffset.UtcNow }

                  let event = SkusImported [ skuReq ]

                  let createResult = IntegrationEventEnvelope.create tenantId correlationId event

                  match createResult with
                  | Error err -> failwithf "Failed to create envelope: %A" err
                  | Ok envelope ->
                      test <@ envelope.TenantId = Some tenantId @>
                      test <@ envelope.CorrelationId = Some(CorrelationId correlationId) @>
                      test <@ envelope.EventType = "IntegrationEvent" @>

                      test <@ envelope.EventId <> (EventId Guid.Empty) @>

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

                  let event = SkusImported [ skuReq ]

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
                              test <@ desEnv.CorrelationId = Some(CorrelationId correlationId) @>
                              test <@ desEnv.EventId = envelope.EventId @>

                              let payloadResult = IntegrationEventEnvelope.tryGetPayload desEnv

                              match payloadResult with
                              | Error err -> failwithf "Failed to extract payload from deserialized envelope: %A" err
                              | Ok extractedEvent ->
                                  match extractedEvent with
                                  | SkusImported skus ->
                                      test <@ skus.Length = 1 @>
                                      test <@ skus.[0].Id = "SKU-999" @>
                                  | _ -> failwith "Expected SkusImported payload")

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

                  let defineTargetRes =
                      supplyBC.Commands.InventoryTarget
                          .Define(targetReq)
                          .Result

                  test <@ Result.isOk defineTargetRes @>

                  let invReq: InventoryDefineReq =
                      { Id = "INV-SKU-BIKE-SP-WAREHOUSE"
                        SkuId = "SKU-BIKE"
                        StockingPointId = "SP-WAREHOUSE"
                        Quantity = 50.0m
                        UnitOfMeasure = "UOM-PCS" }

                  let defineInvRes = supplyBC.Commands.Inventory.Define(invReq).Result
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

                  let createPoRes = supplyBC.Commands.SupplyOrder.Create(poReq).Result
                  let createWoRes = supplyBC.Commands.SupplyOrder.Create(woReq).Result
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

                  supplyBC.Dispose())

              testCase "should enforce strict SupplyOrder state machine transitions" (fun () ->
                  let supplyBC = Medhavi.Supply.BoundedContext.create ()
                  supplyBC.Initialize().Wait()

                  let now = DateTimeOffset.UtcNow

                  // Create order (Created)
                  let req: SupplyOrderCreateReq =
                      { Id = "WO-STM"
                        OrderType = "WorkOrder"
                        SkuId = "SKU-STM"
                        StockingPointId = "SP-STM"
                        Quantity = 100.0m
                        UnitOfMeasure = "UOM-PCS"
                        RoutingId = None
                        SupplierId = None
                        IsFirm = false
                        IsExpedited = false
                        IsLocked = false
                        UsesLeadTimeQuantity = false
                        RequiredDeliveryDate = Some(now.AddDays(5.0))
                        CreatedDate = now }

                  let createRes = supplyBC.Commands.SupplyOrder.Create(req).Result
                  test <@ Result.isOk createRes @>

                  // Try invalid transition Planned -> InProgress directly (should fail)
                  let startResInvalid =
                      supplyBC.Commands.SupplyOrder
                          .Start({ Id = "WO-STM"; StartedDate = now })
                          .Result

                  test <@ Result.isError startResInvalid @>

                  // Valid transitions: Created -> Planned -> Confirmed -> Released -> InProgress -> Completed
                  let planRes =
                      supplyBC.Commands.SupplyOrder
                          .Plan(
                              { Id = "WO-STM"
                                PlannedDeliveryDate = now }
                          )
                          .Result

                  test <@ Result.isOk planRes @>

                  let confirmRes =
                      supplyBC.Commands.SupplyOrder
                          .Confirm({ Id = "WO-STM"; ConfirmedDate = now })
                          .Result

                  test <@ Result.isOk confirmRes @>

                  let releaseRes =
                      supplyBC.Commands.SupplyOrder
                          .Release({ Id = "WO-STM"; ReleasedDate = now })
                          .Result

                  test <@ Result.isOk releaseRes @>

                  let startRes =
                      supplyBC.Commands.SupplyOrder
                          .Start({ Id = "WO-STM"; StartedDate = now })
                          .Result

                  test <@ Result.isOk startRes @>

                  let completeRes =
                      supplyBC.Commands.SupplyOrder
                          .Complete(
                              { Id = "WO-STM"
                                ScrapQuantity = 0.0m
                                CompletedDate = now
                                FeedbackId = None }
                          )
                          .Result

                  test <@ Result.isOk completeRes @>

                  // Completed is final, transition should fail
                  let cancelResInvalid =
                      supplyBC.Commands.SupplyOrder
                          .Cancel({ Id = "WO-STM"; CancelledDate = now })
                          .Result

                  test <@ Result.isError cancelResInvalid @>

                  supplyBC.Dispose())

              testCase "should reconcile MES progress idempotently suppressing duplicate payloads" (fun () ->
                  let supplyBC = Medhavi.Supply.BoundedContext.create ()
                  supplyBC.Initialize().Wait()

                  let now = DateTimeOffset.UtcNow

                  // Create, plan, confirm, release
                  let req: SupplyOrderCreateReq =
                      { Id = "WO-IDEMP"
                        OrderType = "WorkOrder"
                        SkuId = "SKU-IDEMP"
                        StockingPointId = "SP-IDEMP"
                        Quantity = 100.0m
                        UnitOfMeasure = "UOM-PCS"
                        RoutingId = None
                        SupplierId = None
                        IsFirm = false
                        IsExpedited = false
                        IsLocked = false
                        UsesLeadTimeQuantity = false
                        RequiredDeliveryDate = Some(now.AddDays(5.0))
                        CreatedDate = now }

                  let _ = supplyBC.Commands.SupplyOrder.Create(req).Result

                  let _ =
                      supplyBC.Commands.SupplyOrder
                          .Plan(
                              { Id = "WO-IDEMP"
                                PlannedDeliveryDate = now }
                          )
                          .Result

                  let _ =
                      supplyBC.Commands.SupplyOrder
                          .Confirm({ Id = "WO-IDEMP"; ConfirmedDate = now })
                          .Result

                  let _ =
                      supplyBC.Commands.SupplyOrder
                          .Release({ Id = "WO-IDEMP"; ReleasedDate = now })
                          .Result

                  // 1. First partial completion feedback
                  let partReq =
                      { Id = "WO-IDEMP"
                        CompletedQuantity = 15.0m
                        ScrapQuantity = 5.0m
                        CompletedDate = now
                        FeedbackId = Some "MES-FEEDBACK-123" }

                  let partRes =
                      supplyBC.Commands.SupplyOrder
                          .PartialComplete(partReq)
                          .Result

                  test <@ Result.isOk partRes @>

                  let orderAfter1 =
                      supplyBC.Queries.SupplyOrder
                          .GetById("WO-IDEMP")
                          .Result
                      |> Option.get

                  test <@ orderAfter1.CompletedQuantity = 15.0m @>
                  test <@ orderAfter1.ScrapQuantity = 5.0m @>

                  // 2. Duplicate partial completion feedback (should be ignored)
                  let partRes2 =
                      supplyBC.Commands.SupplyOrder
                          .PartialComplete(partReq)
                          .Result

                  test <@ Result.isOk partRes2 @>

                  let orderAfter2 =
                      supplyBC.Queries.SupplyOrder
                          .GetById("WO-IDEMP")
                          .Result
                      |> Option.get

                  test <@ orderAfter2.CompletedQuantity = 15.0m @> // Still 15.0, not 30.0!
                  test <@ orderAfter2.ScrapQuantity = 5.0m @> // Still 5.0, not 10.0!

                  // 3. Complete feedback
                  let completeReq =
                      { Id = "WO-IDEMP"
                        ScrapQuantity = 10.0m
                        CompletedDate = now
                        FeedbackId = Some "MES-FEEDBACK-456" }

                  let completeRes =
                      supplyBC.Commands.SupplyOrder
                          .Complete(completeReq)
                          .Result

                  test <@ Result.isOk completeRes @>

                  let orderAfter3 =
                      supplyBC.Queries.SupplyOrder
                          .GetById("WO-IDEMP")
                          .Result
                      |> Option.get

                  test <@ orderAfter3.State = "Completed" @>
                  // Scrap: 5.0 + 10.0 = 15.0. Completed: 100.0 - 15.0 = 85.0
                  test <@ orderAfter3.ScrapQuantity = 15.0m @>
                  test <@ orderAfter3.CompletedQuantity = 85.0m @>

                  // 4. Duplicate complete feedback (should be ignored)
                  let completeRes2 =
                      supplyBC.Commands.SupplyOrder
                          .Complete(completeReq)
                          .Result

                  test <@ Result.isOk completeRes2 @>

                  let orderAfter4 =
                      supplyBC.Queries.SupplyOrder
                          .GetById("WO-IDEMP")
                          .Result
                      |> Option.get

                  test <@ orderAfter4.ScrapQuantity = 15.0m @>
                  test <@ orderAfter4.CompletedQuantity = 85.0m @>

                  supplyBC.Dispose())

              testCase
                  "should deduct scrap/completions from remaining open quantity in MaterialProvider netting"
                  (fun () ->
                      let supplyBC = Medhavi.Supply.BoundedContext.create ()
                      supplyBC.Initialize().Wait()

                      let now = DateTimeOffset.UtcNow

                      // Seed inventory safety stock target
                      let targetReq: InventoryTargetDefineReq =
                          { SkuId = "SKU-NET"
                            StockingPointId = "SP-NET"
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

                      let _ =
                          supplyBC.Commands.InventoryTarget
                              .Define(targetReq)
                              .Result

                      // Seed inventory on-hand = 50
                      let invReq: InventoryDefineReq =
                          { Id = "INV-SKU-NET-SP-NET"
                            SkuId = "SKU-NET"
                            StockingPointId = "SP-NET"
                            Quantity = 50.0m
                            UnitOfMeasure = "UOM-PCS" }

                      let _ = supplyBC.Commands.Inventory.Define(invReq).Result

                      // Seed work order (qty = 100)
                      let woReq: SupplyOrderCreateReq =
                          { Id = "WO-NET"
                            OrderType = "WorkOrder"
                            SkuId = "SKU-NET"
                            StockingPointId = "SP-NET"
                            Quantity = 100.0m
                            UnitOfMeasure = "UOM-PCS"
                            RoutingId = None
                            SupplierId = None
                            IsFirm = true
                            IsExpedited = false
                            IsLocked = false
                            UsesLeadTimeQuantity = false
                            RequiredDeliveryDate = Some(now.AddDays(2.0))
                            CreatedDate = now }

                      let _ = supplyBC.Commands.SupplyOrder.Create(woReq).Result

                      // Move to InProgress
                      let _ =
                          supplyBC.Commands.SupplyOrder
                              .Confirm({ Id = "WO-NET"; ConfirmedDate = now })
                              .Result

                      let _ =
                          supplyBC.Commands.SupplyOrder
                              .Release({ Id = "WO-NET"; ReleasedDate = now })
                              .Result

                      let _ =
                          supplyBC.Commands.SupplyOrder
                              .Start({ Id = "WO-NET"; StartedDate = now })
                              .Result

                      // Get initial snapshot: open quantity is 100.0
                      let snap1 =
                          MaterialProvider.getSnapshot supplyBC "SKU-NET" "SP-NET" now
                          |> Async.RunSynchronously
                          |> Result.defaultWith (fun e -> failwithf "Failed: %A" e)

                      test <@ snap1.OnHand = 50.0m @>
                      test <@ snap1.Inbound.Length = 1 @>
                      test <@ snap1.Inbound.[0] |> snd = 100.0m @>

                      // Report partial completion of 40.0 and scrap of 10.0 -> remaining open quantity should be 100 - 40 - 10 = 50
                      let partReq =
                          { Id = "WO-NET"
                            CompletedQuantity = 40.0m
                            ScrapQuantity = 10.0m
                            CompletedDate = now
                            FeedbackId = Some "FEEDBACK-NET-1" }

                      let _ =
                          supplyBC.Commands.SupplyOrder
                              .PartialComplete(partReq)
                              .Result

                      let snap2 =
                          MaterialProvider.getSnapshot supplyBC "SKU-NET" "SP-NET" now
                          |> Async.RunSynchronously
                          |> Result.defaultWith (fun e -> failwithf "Failed: %A" e)

                      test <@ snap2.OnHand = 50.0m @>
                      test <@ snap2.Inbound.Length = 1 @>
                      test <@ snap2.Inbound.[0] |> snd = 50.0m @> // open inbound is now 50.0

                      // Complete the rest with 10.0 final scrap -> remaining open quantity should be 0.0 (removed from inbound)
                      let completeReq =
                          { Id = "WO-NET"
                            ScrapQuantity = 10.0m
                            CompletedDate = now
                            FeedbackId = Some "FEEDBACK-NET-2" }

                      let _ =
                          supplyBC.Commands.SupplyOrder
                              .Complete(completeReq)
                              .Result

                      let snap3 =
                          MaterialProvider.getSnapshot supplyBC "SKU-NET" "SP-NET" now
                          |> Async.RunSynchronously
                          |> Result.defaultWith (fun e -> failwithf "Failed: %A" e)

                      test <@ snap3.Inbound.Length = 0 @> // completed order is removed from inbound

                      supplyBC.Dispose())

              testCase "should automatically firm planned supply orders inside the firming horizon" (fun () ->
                  let supplyBC = Medhavi.Supply.BoundedContext.create ()
                  supplyBC.Initialize().Wait()

                  let now = DateTimeOffset.UtcNow

                  // 1. Order due in 5 days (inside firming horizon of 7 days)
                  let req1: SupplyOrderCreateReq =
                      { Id = "WO-FIRM-1"
                        OrderType = "WorkOrder"
                        SkuId = "SKU-FIRM"
                        StockingPointId = "SP-FIRM"
                        Quantity = 100.0m
                        UnitOfMeasure = "UOM-PCS"
                        RoutingId = None
                        SupplierId = None
                        IsFirm = false
                        IsExpedited = false
                        IsLocked = false
                        UsesLeadTimeQuantity = false
                        RequiredDeliveryDate = Some(now.AddDays(5.0))
                        CreatedDate = now }

                  let _ = supplyBC.Commands.SupplyOrder.Create(req1).Result

                  // 2. Order due in 15 days (outside firming horizon of 7 days)
                  let req2: SupplyOrderCreateReq =
                      { Id = "WO-FIRM-2"
                        OrderType = "WorkOrder"
                        SkuId = "SKU-FIRM"
                        StockingPointId = "SP-FIRM"
                        Quantity = 100.0m
                        UnitOfMeasure = "UOM-PCS"
                        RoutingId = None
                        SupplierId = None
                        IsFirm = false
                        IsExpedited = false
                        IsLocked = false
                        UsesLeadTimeQuantity = false
                        RequiredDeliveryDate = Some(now.AddDays(15.0))
                        CreatedDate = now }

                  let _ = supplyBC.Commands.SupplyOrder.Create(req2).Result

                  // Move both to Planned
                  let _ =
                      supplyBC.Commands.SupplyOrder
                          .Plan(
                              { Id = "WO-FIRM-1"
                                PlannedDeliveryDate = now }
                          )
                          .Result

                  let _ =
                      supplyBC.Commands.SupplyOrder
                          .Plan(
                              { Id = "WO-FIRM-2"
                                PlannedDeliveryDate = now }
                          )
                          .Result

                  // Verify initially they are Planned and not Firm
                  let order1Init =
                      supplyBC.Queries.SupplyOrder
                          .GetById("WO-FIRM-1")
                          .Result
                      |> Option.get

                  let order2Init =
                      supplyBC.Queries.SupplyOrder
                          .GetById("WO-FIRM-2")
                          .Result
                      |> Option.get

                  test
                      <@
                          order1Init.State = "Planned"
                          && not order1Init.IsFirm
                      @>

                  test
                      <@
                          order2Init.State = "Planned"
                          && not order2Init.IsFirm
                      @>

                  // Trigger auto-firming with a 7-day horizon
                  let firmRes =
                      supplyBC.Commands.SupplyOrder.AutoFirmOrders now 7
                      |> Async.AwaitTask
                      |> Async.RunSynchronously

                  test <@ Result.isOk firmRes @>

                  // Verify order1 is now Confirmed/Firm and order2 is still Planned
                  let order1Final =
                      supplyBC.Queries.SupplyOrder
                          .GetById("WO-FIRM-1")
                          .Result
                      |> Option.get

                  let order2Final =
                      supplyBC.Queries.SupplyOrder
                          .GetById("WO-FIRM-2")
                          .Result
                      |> Option.get

                  test
                      <@
                          order1Final.State = "Confirmed"
                          && order1Final.IsFirm
                      @>

                  test
                      <@
                          order2Final.State = "Planned"
                          && not order2Final.IsFirm
                      @>

                  supplyBC.Dispose()) ]
