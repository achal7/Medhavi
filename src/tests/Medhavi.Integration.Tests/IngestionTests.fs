namespace Medhavi.Integration.Tests

open System
open Expecto
open Swensen.Unquote
open Medhavi.Contracts
open Medhavi.Contracts.Integration
open Medhavi.Integration
open Medhavi.Common.Validation
open Medhavi.Common.Serialization

module IngestionTests =

    [<Tests>]
    let tests =
        testList
            "Ingestion Parsing and Validation Tests"
            [ testCase "should parse UOM JSON and CSV completely" (fun () ->
                  let json = """[{"id":"UOM-1","code":"PCS","name":"Pieces","isBase":true,"toBaseFactor":1.0,"created":"2026-05-27T00:00:00Z"}]"""
                  let uomJsonResult = InboundAdapter.parseUomJson json
                  match uomJsonResult with
                  | Error err -> failwithf "Failed JSON: %s" err
                  | Ok list ->
                      test <@ list.Length = 1 @>
                      test <@ list.[0].Code = "PCS" @>
                      test <@ list.[0].IsBase @>

                  let csv = "Id,Code,Name,IsBase,ToBaseFactor,Created\nUOM-1,PCS,Pieces,true,1.0,2026-05-27T00:00:00Z"
                  let uomCsvResult = InboundAdapter.parseUomCsv csv
                  match uomCsvResult with
                  | Error err -> failwithf "Failed CSV: %s" err
                  | Ok list ->
                      test <@ list.Length = 1 @>
                      test <@ list.[0].Code = "PCS" @>
                      test <@ list.[0].IsBase @>
              )

              testCase "should parse UnitConversion JSON and CSV completely" (fun () ->
                  let json = """[{"sourceUom":"KG","targetUom":"G","conversionFactor":1000.0,"created":"2026-05-27T00:00:00Z"}]"""
                  let jsonRes = InboundAdapter.parseUnitConversionJson json
                  match jsonRes with
                  | Error err -> failwithf "Failed JSON: %s" err
                  | Ok list ->
                      test <@ list.Length = 1 @>
                      test <@ list.[0].SourceUom = "KG" @>
                      test <@ list.[0].ConversionFactor = 1000.0m @>

                  let csv = "SourceUom,TargetUom,ConversionFactor,Created\nKG,G,1000.0,2026-05-27T00:00:00Z"
                  let csvRes = InboundAdapter.parseUnitConversionCsv csv
                  match csvRes with
                  | Error err -> failwithf "Failed CSV: %s" err
                  | Ok list ->
                      test <@ list.Length = 1 @>
                      test <@ list.[0].SourceUom = "KG" @>
                      test <@ list.[0].ConversionFactor = 1000.0m @>
              )

              testCase "should parse Products CSV completely" (fun () ->
                  let csv = "SkuId,Name,UoM,IsActive\nSKU-1,Widget,PCS,true"
                  let res = InboundAdapter.parseProductCsv csv
                  match res with
                  | Error err -> failwithf "Failed CSV: %s" err
                  | Ok list ->
                      test <@ list.Length = 1 @>
                      test <@ list.[0].SkuId = "SKU-1" @>
                      test <@ list.[0].Name = "Widget" @>
                      test <@ list.[0].IsActive @>
              )

              testCase "should parse BOM Lines CSV completely" (fun () ->
                  let csv = "ParentSkuId,ComponentSkuId,QuantityRequired\nSKU-1,SKU-2,2.5"
                  let res = InboundAdapter.parseBomLineCsv csv
                  match res with
                  | Error err -> failwithf "Failed CSV: %s" err
                  | Ok list ->
                      test <@ list.Length = 1 @>
                      test <@ list.[0].ParentSkuId = "SKU-1" @>
                      test <@ list.[0].ComponentSkuId = "SKU-2" @>
                      test <@ list.[0].QuantityRequired = 2.5m @>
              )

              testCase "should parse StockingPoints CSV completely" (fun () ->
                  let csv = "StockingPointId,Name,IsActive\nSP-1,Warehouse A,true"
                  let res = InboundAdapter.parseStockingPointCsv csv
                  match res with
                  | Error err -> failwithf "Failed CSV: %s" err
                  | Ok list ->
                      test <@ list.Length = 1 @>
                      test <@ list.[0].StockingPointId = "SP-1" @>
                      test <@ list.[0].IsActive @>
              )

              testCase "should parse Resources CSV completely" (fun () ->
                  let csv = "ResourceId,Name,NodeId,IsActive\nRES-1,Assembly Line,SP-1,true"
                  let res = InboundAdapter.parseResourceCsv csv
                  match res with
                  | Error err -> failwithf "Failed CSV: %s" err
                  | Ok list ->
                      test <@ list.Length = 1 @>
                      test <@ list.[0].ResourceId = "RES-1" @>
                      test <@ list.[0].NodeId = "SP-1" @>
              )

              testCase "should parse Routings and Steps from CSV completely (grouping steps)" (fun () ->
                  let csv = "SkuId,Sequence,ResourceId,SetupHours,RunHoursPerUnit\nSKU-1,10,RES-1,1.5,0.25\nSKU-1,20,RES-2,0.5,0.1"
                  let res = InboundAdapter.parseRoutingCsv csv
                  match res with
                  | Error err -> failwithf "Failed CSV: %s" err
                  | Ok list ->
                      test <@ list.Length = 1 @>
                      test <@ list.[0].SkuId = "SKU-1" @>
                      test <@ list.[0].Steps.Length = 2 @>
                      test <@ list.[0].Steps.[0].Sequence = 10 @>
                      test <@ list.[0].Steps.[0].ResourceId = "RES-1" @>
              )

              testCase "should parse TransportLegs CSV completely (handling constraints splitting)" (fun () ->
                  let csv = "Id,Origin,Destination,Mode,Schedule,LeadTimeMinutes,Capacity,CapacityUnit,CutoffMinutes,Constraints,Reliability,CO2PerUnit,EffectiveStart,EffectiveEnd,Created\nLEG-1,SP-1,SP-2,Road,Daily,180.0,500.0,PCS,60.0,\"Hazmat|Fragile\",0.95,0.05,2026-05-27T00:00:00Z,,2026-05-27T00:00:00Z"
                  let res = InboundAdapter.parseTransportLegCsv csv
                  match res with
                  | Error err -> failwithf "Failed CSV: %s" err
                  | Ok list ->
                      test <@ list.Length = 1 @>
                      test <@ list.[0].Id = "LEG-1" @>
                      test <@ list.[0].Constraints = ["Hazmat"; "Fragile"] @>
                      test <@ list.[0].Capacity = Some 500.0m @>
                      test <@ list.[0].CutoffMinutes = Some 60.0 @>
              )

              testCase "should validate master data cross-reference integrity successfully" (fun () ->
                  let payload = {
                      Products = [
                          { SkuId = "SKU-1"; Name = "Finished Good"; UoM = "PCS"; IsActive = true }
                          { SkuId = "SKU-2"; Name = "Component Sku"; UoM = "PCS"; IsActive = true }
                      ]
                      Boms = [
                          { ParentSkuId = "SKU-1"; ComponentSkuId = "SKU-2"; QuantityRequired = 2m }
                      ]
                      StockingPoints = [
                          { StockingPointId = "SP-1"; Name = "Factory Point"; IsActive = true }
                      ]
                      Resources = [
                          { ResourceId = "RES-1"; Name = "Assembly Machine"; NodeId = "SP-1"; IsActive = true }
                      ]
                      Routings = [
                          { SkuId = "SKU-1"; Steps = [ { Sequence = 10; ResourceId = "RES-1"; SetupHours = 1.0; RunHoursPerUnit = 0.1 } ] }
                      ]
                      Suppliers = []
                  }
                  let validation = MasterDataValidator.validate payload
                  match validation with
                  | Invalid errs -> failwithf "Expected valid master data, got: %A" errs
                  | Valid _ -> () // Success
              )

              testCase "should flag missing dependencies in master data reference checks" (fun () ->
                  let payload = {
                      Products = [
                          { SkuId = "SKU-1"; Name = "Finished Good"; UoM = "PCS"; IsActive = true }
                      ]
                      Boms = [
                          { ParentSkuId = "SKU-1"; ComponentSkuId = "MISSING-SKU"; QuantityRequired = 2m }
                      ]
                      StockingPoints = []
                      Resources = [
                          { ResourceId = "RES-1"; Name = "Assembly Machine"; NodeId = "MISSING-SP"; IsActive = true }
                      ]
                      Routings = [
                          { SkuId = "MISSING-SKU-2"; Steps = [ { Sequence = 10; ResourceId = "MISSING-RES"; SetupHours = 1.0; RunHoursPerUnit = 0.1 } ] }
                      ]
                      Suppliers = []
                  }
                  let validation = MasterDataValidator.validate payload
                  match validation with
                  | Valid _ -> failwith "Expected validation errors, but succeeded"
                  | Invalid errs ->
                      test <@ errs.Length = 3 @>
                      test <@ errs.[0].Contains("BOM Lines refer to missing Product IDs") @>
                      test <@ errs.[1].Contains("Resources refer to missing Node/StockingPoint IDs") @>
                      test <@ errs.[2].Contains("Routings refer to missing Sku IDs") @>
              )

              testCase "should validate duplicate IDs" (fun () ->
                  let payload = {
                      Products = [
                          { SkuId = "SKU-1"; Name = "FG A"; UoM = "PCS"; IsActive = true }
                          { SkuId = "SKU-1"; Name = "FG B"; UoM = "PCS"; IsActive = true }
                      ]
                      Boms = []
                      StockingPoints = [
                          { StockingPointId = "SP-1"; Name = "SP A"; IsActive = true }
                          { StockingPointId = "SP-1"; Name = "SP B"; IsActive = true }
                      ]
                      Resources = []
                      Routings = []
                      Suppliers = []
                  }
                  let validation = MasterDataValidator.validate payload
                  match validation with
                  | Valid _ -> failwith "Expected validation errors, but succeeded"
                  | Invalid errs ->
                      test <@ errs.Length = 2 @>
                      test <@ errs.[0].Contains("Duplicate Product IDs found") @>
                      test <@ errs.[1].Contains("Duplicate Stocking Point IDs found") @>
              )

              testCase "should parse and roundtrip new telemetry integration events successfully" (fun () ->
                  let tenantId = "telemetry-tenant"
                  let correlationId = Guid.NewGuid()
                  let payload = [
                      { ProductId = "SKU-1"; StockingPointId = "SP-1"; Quantity = 100m; AsOfUtc = DateTimeOffset.UtcNow }
                  ]
                  let event = InventoryPositionsImported payload

                  let envelopeResult = IntegrationEventEnvelope.create tenantId correlationId event
                  match envelopeResult with
                  | Error err -> failwithf "Failed to create envelope: %A" err
                  | Ok envelope ->
                      test <@ envelope.TenantId = Some tenantId @>
                      test <@ envelope.CorrelationId = Some correlationId @>

                      let extractionResult = IntegrationEventEnvelope.tryGetPayload envelope
                      match extractionResult with
                      | Error err -> failwithf "Failed to extract payload: %A" err
                      | Ok extractedEvent ->
                          match extractedEvent with
                          | InventoryPositionsImported list ->
                              test <@ list.Length = 1 @>
                              test <@ list.[0].ProductId = "SKU-1" @>
                          | _ -> failwith "Expected InventoryPositionsImported payload"
              )

              testCase "should parse Plant, Uom, and UnitConversion adapters correctly" (fun () ->
                  // 1. PlantAdapter
                  let spCsv = "StockingPointId,Name,IsActive\nSP-FACTORY,Assembly Plant,true"
                  let plantRes = Medhavi.Integration.Adapters.PlantAdapter.parse spCsv
                  match plantRes with
                  | Error err -> failwithf "PlantAdapter failed: %s" err
                  | Ok plants ->
                      test <@ plants.Length = 1 @>
                      test <@ plants.[0].Id = "PLANT-DEFAULT" @>

                  // 2. UomAdapter
                  let prodCsv = "SkuId,Name,UoM,IsActive\nSKU-1,Widget,UOM-PCS,true"
                  let legCsv = "Id,Origin,Destination,Mode,Schedule,LeadTimeMinutes,Capacity,CapacityUnit,CutoffMinutes,Constraints,Reliability,CO2PerUnit,EffectiveStart\nLEG-1,SP-1,SP-2,Road,Daily,180.0,500.0,UOM-BAG,60.0,,0.95,0.05,2026-05-27T00:00:00Z"
                  let uomRes = Medhavi.Integration.Adapters.UomAdapter.parse prodCsv legCsv
                  match uomRes with
                  | Error err -> failwithf "UomAdapter failed: %s" err
                  | Ok uoms ->
                      test <@ uoms |> List.exists (fun u -> u.Id = "UOM-PCS") @>
                      test <@ uoms |> List.exists (fun u -> u.Id = "UOM-BAG") @>
                      test <@ uoms |> List.exists (fun u -> u.Id = "UOM-BOX") @>

                  // 3. UnitConversionAdapter
                  let ucCsv = "SourceUom,TargetUom,ConversionFactor,Created\nUOM-BOX,UOM-PCS,10.0,2026-05-28T00:00:00Z"
                  let ucRes = Medhavi.Integration.Adapters.UnitConversionAdapter.parse ucCsv
                  match ucRes with
                  | Error err -> failwithf "UnitConversionAdapter failed: %s" err
                  | Ok conversions ->
                      test <@ conversions.Length = 1 @>
                      test <@ conversions.[0].SourceUom = "UOM-BOX" @>
                      test <@ conversions.[0].TargetUom = "UOM-PCS" @>
                      test <@ conversions.[0].ConversionFactor = 10.0m @>

                  let emptyUcRes = Medhavi.Integration.Adapters.UnitConversionAdapter.parse ""
                  match emptyUcRes with
                  | Error err -> failwithf "UnitConversionAdapter empty failed: %s" err
                  | Ok conversions ->
                      test <@ conversions.Length = 1 @>
                      test <@ conversions.[0].SourceUom = "UOM-BOX" @>
                      test <@ conversions.[0].TargetUom = "UOM-PCS" @>
                      test <@ conversions.[0].ConversionFactor = 10.0m @>
              )
            ]
