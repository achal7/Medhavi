namespace Medhavi.Integration.Tests

open System
open Expecto
open Swensen.Unquote
open Medhavi.Contracts
open Medhavi.Contracts.Integration
open Medhavi.Integration
open Medhavi.Common.Validation
open Medhavi.Common.Serialization
open Medhavi.Infrastructure
open Medhavi.Supply
open Medhavi.Supply.Domain
open Medhavi.Supply.Domain.MaterialReservationAgg
open Medhavi.Supply.Application

module IngestionTests =
    open Medhavi.SharedKernel
    open Medhavi.Supply.Application

    [<Tests>]
    let tests =
        testList
            "Ingestion Parsing and Validation Tests"
            [ testCase "should parse UnitConversion CSV completely" (fun () ->
                  let csv = "SourceUom,TargetUom,ConversionFactor,Created\nKG,G,1000.0,2026-05-27T00:00:00Z"
                  let csvRes = Medhavi.Integration.Adapters.UnitConversion.ACL.parse csv
                  match csvRes with
                  | Error err -> failwithf "Failed CSV: %s" err
                  | Ok list ->
                      test <@ list.Length = 1 @>
                      test <@ list.[0].SourceUom = "KG" @>
                      test <@ list.[0].ConversionFactor = 1000.0m @>
              )

              testCase "should parse Products CSV completely" (fun () ->
                  let csv = "SkuId,Name,UoM,IsActive\nSKU-1,Widget,PCS,true"
                  let res = Medhavi.Integration.Adapters.Sku.ACL.parse csv
                  match res with
                  | Error err -> failwithf "Failed CSV: %s" err
                  | Ok list ->
                      test <@ list.Length = 1 @>
                      test <@ list.[0].Id = "SKU-1" @>
                      test <@ list.[0].Name = "Widget" @>
              )

              testCase "should parse BOM Lines CSV completely" (fun () ->
                  let csv = "ParentSkuId,ComponentSkuId,QuantityRequired\nSKU-1,SKU-2,2.5"
                  let res = Medhavi.Integration.Adapters.Bom.ACL.parse csv
                  match res with
                  | Error err -> failwithf "Failed CSV: %s" err
                  | Ok list ->
                      test <@ list.Length = 1 @>
                      test <@ list.[0].SkuId = "SKU-1" @>
                      test <@ list.[0].Items.Length = 1 @>
                      test <@ list.[0].Items.[0].ComponentSkuId = "SKU-2" @>
                      test <@ list.[0].Items.[0].Quantity = 2.5m @>
              )

              testCase "should parse StockingPoints CSV completely" (fun () ->
                  let csv = "StockingPointId,Name,IsActive\nSP-1,Warehouse A,true"
                  let res = Medhavi.Integration.Adapters.StockingPoint.ACL.parse csv
                  match res with
                  | Error err -> failwithf "Failed CSV: %s" err
                  | Ok (sps, nodes) ->
                      test <@ sps.Length = 1 @>
                      test <@ sps.[0].Id = "SP-1" @>
                      test <@ nodes.Length = 1 @>
                      test <@ nodes.[0].Id = "SP-1" @>
              )

              testCase "should parse Resource Groups CSV completely" (fun () ->
                  let csv = "ResourceGroupId,PlantId,Name,Description,DefaultCalendarId,IsActive\nRG-1,PLANT-1,Group 1,Desc,CAL-1,true"
                  let res = Medhavi.Integration.Adapters.Resource.ACL.parseResourceGroups csv
                  match res with
                  | Error err -> failwithf "Failed CSV: %s" err
                  | Ok list ->
                      test <@ list.Length = 1 @>
                      test <@ list.[0].ResourceGroupId = "RG-1" @>
                      test <@ list.[0].PlantId = Some "PLANT-1" @>
              )

              testCase "should parse Standard Resources CSV completely" (fun () ->
                  let csv = "StandardResourceId,ResourceGroupId,Name,Description,DefaultEfficiency,DefaultCostRateAmount,DefaultCostRateCurrency,IsActive\nSR-1,RG-1,Standard 1,Desc,0.85,150.0,USD,true"
                  let res = Medhavi.Integration.Adapters.Resource.ACL.parseStandardResources csv
                  match res with
                  | Error err -> failwithf "Failed CSV: %s" err
                  | Ok list ->
                      test <@ list.Length = 1 @>
                      test <@ list.[0].StandardResourceId = "SR-1" @>
                      test <@ list.[0].ResourceGroupId = "RG-1" @>
                      test <@ list.[0].DefaultEfficiency = 0.85M @>
              )

              testCase "should parse Physical Resources CSV completely" (fun () ->
                  let csv = "PhysicalResourceId,StandardResourceId,Name,SerialNumber,Location,EfficiencyOverride,CostRateOverrideAmount,CostRateOverrideCurrency,CalendarId,IsActive\nPR-1,SR-1,Physical 1,SN123,Room A,0.90,120.0,USD,CAL-2,true"
                  let res = Medhavi.Integration.Adapters.Resource.ACL.parsePhysicalResources csv
                  match res with
                  | Error err -> failwithf "Failed CSV: %s" err
                  | Ok list ->
                      test <@ list.Length = 1 @>
                      test <@ list.[0].PhysicalResourceId = "PR-1" @>
                      test <@ list.[0].StandardResourceId = "SR-1" @>
                      test <@ list.[0].EfficiencyOverride = Some 0.90M @>
              )

              testCase "should parse Routings and Steps from CSV completely (grouping steps)" (fun () ->
                  let routingCsv = "RoutingId,Name,RoutingType,SkuId,StockingPointId,EffectiveStart,PreferencePriority,IsPreferred,CostPolicyType\nROUTING-SKU-1,Routing for SKU-1,Work,SKU-1,SP-FACTORY,2026-01-01T00:00:00Z,1,true,NoRoutingCost"
                  let stepsCsv = "RoutingId,StepId,Sequence,OperationCode,Name,ResourceRequirementId,ResourceKind,ResourceLoadBasis,ResourceRequiredUnits,ResourceSelectionRule,OptionId,ResourceGroupId,ResourceId,SetupTimeMinutes,RunTimePerBaseQuantityMinutes\nROUTING-SKU-1,STEP-SKU-1-10,10,OP-10,Step 10,REQ-10,WorkCenter,PerUnit,1.0,AnyAllowed,OPT-10,RG-1,RES-1,90.0,15.0\nROUTING-SKU-1,STEP-SKU-1-20,20,OP-20,Step 20,REQ-20,WorkCenter,PerUnit,1.0,AnyAllowed,OPT-20,RG-2,RES-2,30.0,6.0"
                  let res = Medhavi.Integration.Adapters.Routing.ACL.parse routingCsv stepsCsv "" ""
                  match res with
                  | Error err -> failwithf "Failed CSV: %s" err
                  | Ok list ->
                      test <@ list.Length = 1 @>
                      test <@ list.[0].Id = "ROUTING-SKU-1" @>
                      match list.[0].Details with
                      | WorkDetails work ->
                          test <@ work.Steps.Length = 2 @>
                          test <@ work.Steps.[0].Sequence = 10 @>
                          test <@ work.Steps.[0].ResourceRequirements.[0].Options.[0].ResourceId = Some "RES-1" @>
                      | _ -> failwith "Expected WorkDetails"
              )

              testCase "should parse TransportLegs CSV completely (handling constraints splitting)" (fun () ->
                  let csv = "Id,Origin,Destination,Mode,Schedule,LeadTimeMinutes,Capacity,CapacityUnit,CutoffMinutes,Constraints,Reliability,CO2PerUnit,EffectiveStart,EffectiveEnd,Created\nLEG-1,SP-1,SP-2,Road,Daily,180.0,500.0,PCS,60.0,\"Hazmat|Fragile\",0.95,0.05,2026-05-27T00:00:00Z,,2026-05-27T00:00:00Z"
                  let res = Medhavi.Integration.Adapters.TransportLeg.ACL.parse csv
                  match res with
                  | Error err -> failwithf "Failed CSV: %s" err
                  | Ok list ->
                      test <@ list.Length = 1 @>
                      test <@ list.[0].Id = "LEG-1" @>
                      test <@ list.[0].Constraints = ["Hazmat"; "Fragile"] @>
                      test <@ list.[0].Capacity = Some 500.0m @>
                      test <@ list.[0].CutoffMinutes = Some 60.0m @>
              )

              testCase "should parse InventoryTargets CSV completely" (fun () ->
                  let csv = "SkuId,StockingPointId,SafetyStockQty,MinQty,MaxQty,TargetServiceLevel,CoverDays,IsActive\nSKU-BIKE,SP-WAREHOUSE,10.0,5.0,50.0,0.95,5.0,true"
                  let res = Medhavi.Integration.Adapters.InventoryTarget.ACL.parse csv
                  match res with
                  | Error err -> failwithf "Failed CSV: %s" err
                  | Ok list ->
                      test <@ list.Length = 1 @>
                      test <@ list.[0].SkuId = "SKU-BIKE" @>
                      test <@ list.[0].StockingPointId = "SP-WAREHOUSE" @>
                      test <@ list.[0].SafetyStockQty = Some 10.0m @>
                      test <@ list.[0].CoverDays = Some 5.0m @>
                      test <@ list.[0].IsActive @>
              )

              testCase "should parse SupplierOffers CSV completely" (fun () ->
                  let csv = "Id,SupplierId,SkuId,StockingPointId,Moq,LotSize,LeadTimeP50Minutes,LeadTimeP95Minutes,Reliability,Incoterm\nOFFER-1,SUP-1,SKU-1,SP-1,50.0,10.0,1440.0,2880.0,0.95,DDP"
                  let res = Medhavi.Integration.Adapters.SupplierOffer.ACL.parseSupplierOfferCsv csv
                  match res with
                  | Error err -> failwithf "Failed CSV: %s" err
                  | Ok list ->
                      test <@ list.Length = 1 @>
                      test <@ list.[0].Id = "OFFER-1" @>
                      test <@ list.[0].SupplierId = "SUP-1" @>
                      test <@ list.[0].SkuId = "SKU-1" @>
                      test <@ list.[0].Moq = Some 50.0m @>
                      test <@ list.[0].LeadTimeP50Minutes = Some 1440.0m @>
                      test <@ list.[0].Reliability = Some 0.95m @>
                      test <@ list.[0].Incoterm = Some "DDP" @>
              )

              testCase "should parse and roundtrip new telemetry integration events successfully" (fun () ->
                  let tenantId = "telemetry-tenant"
                  let correlationId = Guid.NewGuid()
                  let payload : InventoryDefineReq list = [
                      { Id = "INV-1"; SkuId = "SKU-1"; StockingPointId = "SP-1"; Quantity = 100m; UnitOfMeasure = "UOM-PCS" }
                  ]
                  let event = InventoryPositionsImported payload

                  let envelopeResult = IntegrationEventEnvelope.create tenantId correlationId event
                  match envelopeResult with
                  | Error err -> failwithf "Failed to create envelope: %A" err
                  | Ok envelope ->
                      test <@ envelope.TenantId = Some tenantId @>
                      test <@ envelope.CorrelationId = Some (CorrelationId correlationId) @>

                      let extractionResult = IntegrationEventEnvelope.tryGetPayload envelope
                      match extractionResult with
                      | Error err -> failwithf "Failed to extract payload: %A" err
                      | Ok extractedEvent ->
                          match extractedEvent with
                          | InventoryPositionsImported list ->
                              test <@ list.Length = 1 @>
                              test <@ list.[0].SkuId = "SKU-1" @>
                          | _ -> failwith "Expected InventoryPositionsImported payload"
              )

              testCase "should parse Plant, Uom, and UnitConversion adapters correctly" (fun () ->
                  // 1. PlantAdapter
                  let spCsv = "StockingPointId,Name,IsActive\nSP-FACTORY,Assembly Plant,true"
                  let plantRes = Medhavi.Integration.Adapters.Plant.ACL.parse spCsv
                  match plantRes with
                  | Error err -> failwithf "PlantAdapter failed: %s" err
                  | Ok plants ->
                      test <@ plants.Length = 1 @>
                      test <@ plants.[0].Id = "PLANT-DEFAULT" @>

                  // 2. UomAdapter
                  let uomCsv = "Id,Code,Name,IsBase,ToBaseFactor\nUOM-PCS,PCS,Pieces,true,1.0\nUOM-BAG,BAG,Bags,false,50.0\nUOM-BOX,BOX,Box of 10,false,10.0"
                  let uomRes = Medhavi.Integration.Adapters.Uom.ACL.parse uomCsv
                  match uomRes with
                  | Error err -> failwithf "UomAdapter failed: %s" err
                  | Ok uoms ->
                      test <@ uoms |> List.exists (fun u -> u.Id = "UOM-PCS") @>
                      test <@ uoms |> List.exists (fun u -> u.Id = "UOM-BAG") @>
                      test <@ uoms |> List.exists (fun u -> u.Id = "UOM-BOX") @>


                  // 3. UnitConversionAdapter
                  let ucCsv = "SourceUom,TargetUom,ConversionFactor,Created\nUOM-BOX,UOM-PCS,10.0,2026-05-28T00:00:00Z"
                  let ucRes = Medhavi.Integration.Adapters.UnitConversion.ACL.parse ucCsv
                  match ucRes with
                  | Error err -> failwithf "UnitConversionAdapter failed: %s" err
                  | Ok conversions ->
                      test <@ conversions.Length = 1 @>
                      test <@ conversions.[0].SourceUom = "UOM-BOX" @>
                      test <@ conversions.[0].TargetUom = "UOM-PCS" @>
                      test <@ conversions.[0].ConversionFactor = 10.0m @>

                  let emptyUcRes = Medhavi.Integration.Adapters.UnitConversion.ACL.parse ""
                  match emptyUcRes with
                  | Error err -> failwithf "UnitConversionAdapter empty failed: %s" err
                  | Ok conversions ->
                      test <@ conversions.Length = 1 @>
                      test <@ conversions.[0].SourceUom = "UOM-BOX" @>
                      test <@ conversions.[0].TargetUom = "UOM-PCS" @>
                      test <@ conversions.[0].ConversionFactor = 10.0m @>
              )

              testCase "should parse MaterialReservations CSV completely" (fun () ->
                  let csv = "Id,IdempotencyKey,SkuId,StockingPointId,Quantity,RequiredDate,ExpiryTime\nRES-TEST,key-1,SKU-BIKE,SP-WAREHOUSE,15.5,2026-06-15T00:00:00Z,2026-06-05T00:00:00Z"
                  let res = Medhavi.Integration.Adapters.MaterialReservation.ACL.parse csv
                  match res with
                  | Error err -> failwithf "MaterialReservation adapter failed: %s" err
                  | Ok list ->
                      test <@ list.Length = 1 @>
                      test <@ list.[0].Id = "RES-TEST" @>
                      test <@ list.[0].IdempotencyKey = "key-1" @>
                      test <@ list.[0].SkuId = "SKU-BIKE" @>
                      test <@ list.[0].Quantity = 15.5m @>
              )

              testCase "should enforce MaterialReservation aggregate lifecycle transitions" (fun () ->
                  let now = Timestamp.now
                  let cmd = {
                      Id = "RES-XYZ"
                      IdempotencyKey = "key-xyz"
                      SkuId = SkuId.create "SKU-BIKE" |> function Ok x -> x | Error _ -> failwith "invalid SkuId"
                      StockingPointId = StockingPointId.create "SP-WAREHOUSE" |> function Ok x -> x | Error _ -> failwith "invalid StockingPointId"
                      Quantity = 20.0m
                      RequiredDate = DateTimeOffset.UtcNow.AddDays(10.0)
                      ExpiryTime = DateTimeOffset.UtcNow.AddDays(5.0)
                  }
                  
                  // 1. Create Tentative
                  let decRes = decide (CreateTentative cmd) None
                  match decRes with
                  | Error err -> failwithf "Failed to create reservation: %A" err
                  | Ok decision ->
                      let state = Some decision.NewState
                      test <@ decision.NewState.State = "Tentative" @>
                      test <@ decision.NewState.Quantity = Quantity.clampToZero 20.0m @>

                      // 2. Try to create again on existing state -> should fail (reject duplicates)
                      let decDuplicate = decide (CreateTentative cmd) state
                      test <@ Result.isError decDuplicate @>

                      // 3. Confirm from Tentative -> should succeed
                      let decConfirm = decide (Confirm { Id = "RES-XYZ" }) state
                      match decConfirm with
                      | Error err -> failwithf "Failed to confirm: %A" err
                      | Ok confirmDec ->
                          let confirmedState = Some confirmDec.NewState
                          test <@ confirmDec.NewState.State = "Confirmed" @>

                          // 4. Reduce from Confirmed -> should succeed
                          let decReduce = decide (Reduce { Id = "RES-XYZ"; NewQuantity = 12.0m }) confirmedState
                          match decReduce with
                          | Error err -> failwithf "Failed to reduce: %A" err
                          | Ok reduceDec ->
                              test <@ reduceDec.NewState.State = "Reduced" @>
                              test <@ reduceDec.NewState.Quantity = Quantity.clampToZero 12.0m @>

                          // 5. Expire from Confirmed -> should fail
                          let decExpire = decide (Expire { Id = "RES-XYZ" }) confirmedState
                          test <@ Result.isError decExpire @>

                      // 6. Expire from Tentative -> should succeed
                      let decExpireTentative = decide (Expire { Id = "RES-XYZ" }) state
                      match decExpireTentative with
                      | Error err -> failwithf "Failed to expire: %A" err
                      | Ok expireDec ->
                          test <@ expireDec.NewState.State = "Expired" @>
              )

              testCase "should calculate date-wise ATP projections with active reservations correctly" (fun () ->
                  let supply = BoundedContext.create ()
                  supply.Initialize().Wait()

                  // Seed inventory (on-hand = 100)
                  let invReq = {
                      Id = "INV-SKU-BIKE-PROJ-SP-WAREHOUSE-PROJ"
                      SkuId = "SKU-BIKE-PROJ"
                      StockingPointId = "SP-WAREHOUSE-PROJ"
                      Quantity = 100m
                      UnitOfMeasure = "UOM-PCS"
                  }
                  let! _ = supply.Inventory.Define invReq

                  // Seed inbound supply order (qty = 50 on Day 10)
                  let orderReq : SupplyOrderCreateReq = {
                      Id = "ORDER-1"
                      OrderType = "purchaseorder"
                      SkuId = "SKU-BIKE-PROJ"
                      StockingPointId = "SP-WAREHOUSE-PROJ"
                      Quantity = 50m
                      UnitOfMeasure = "UOM-PCS"
                      RoutingId = None
                      SupplierId = None
                      IsFirm = true
                      IsExpedited = false
                      IsLocked = false
                      UsesLeadTimeQuantity = false
                      RequiredDeliveryDate = Some (DateTimeOffset.UtcNow.AddDays(10.0))
                      CreatedDate = DateTimeOffset.UtcNow
                  }
                  let! _ = supply.SupplyOrder.Create orderReq

                  // Seed active reservation (qty = 30 on Day 20)
                  let resvReq: MaterialReservationCreateReq = {
                      Id = "RES-1"
                      IdempotencyKey = "idem-key-1"
                      SkuId = "SKU-BIKE-PROJ"
                      StockingPointId = "SP-WAREHOUSE-PROJ"
                      Quantity = 30m
                      RequiredDate = DateTimeOffset.UtcNow.AddDays(20.0)
                      ExpiryTime = DateTimeOffset.UtcNow.AddDays(5.0)
                  }
                  let! _ = supply.MaterialReservation.CreateTentative resvReq

                  // Query time-phased availability
                  let startDate = DateTimeOffset.UtcNow
                  let dailyRes =
                      MaterialProvider.getDateWiseAvailability supply "SKU-BIKE-PROJ" "SP-WAREHOUSE-PROJ" startDate 30
                      |> Async.RunSynchronously

                  match dailyRes with
                  | Error err -> failwithf "Date-wise availability failed: %A" err
                  | Ok list ->
                      // Day 0 to Day 9: OnHand = 100
                      let day1Val = list |> List.find (fun (d, _) -> d.Date = startDate.Date) |> snd
                      test <@ day1Val = 100m @>

                      // Day 10 to Day 19: OnHand + Inbound = 150
                      let day11Date = startDate.AddDays(10.0).Date
                      let day11Val = list |> List.find (fun (d, _) -> d.Date = day11Date) |> snd
                      test <@ day11Val = 150m @>

                      // Day 20 onwards: OnHand + Inbound - Reservation = 120
                      let day21Date = startDate.AddDays(20.0).Date
                      let day21Val = list |> List.find (fun (d, _) -> d.Date = day21Date) |> snd
                      test <@ day21Val = 120m @>
              )
            ]
