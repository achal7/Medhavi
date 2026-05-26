namespace Medhavi.Integration.Tests

open System
open Expecto
open Swensen.Unquote
open Medhavi.SharedKernel
open Medhavi.Contracts
open Medhavi.Integration
open Medhavi.Common.Serialization

module IntegrationContractsTests =

    [<Tests>]
    let tests =
        testList
            "Integration Contracts Tests"
            [ testCase "should create Envelope with correct integration metadata and payload" (fun () ->
                  let tenantId = "tenant-test"
                  let correlationId = Guid.NewGuid()
                  let payload = { Products = []; Boms = []; StockingPoints = []; Resources = []; Routings = []; Suppliers = [] }
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
                      | Ok extractedEvent ->
                          test <@ extractedEvent = event @>
              )

              testCase "should serialize and deserialize Envelope containing IntegrationEvent successfully" (fun () ->
                  let tenantId = "tenant-test-2"
                  let correlationId = Guid.NewGuid()
                  let product = { SkuId = "SKU-999"; Name = "Widget"; UoM = "PCS"; IsActive = true }
                  let payload = { Products = [product]; Boms = []; StockingPoints = []; Resources = []; Routings = []; Suppliers = [] }
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
                                      test <@ md.Products.Length = 1 @>
                                      test <@ md.Products.[0].SkuId = "SKU-999" @>
                                  | _ -> failwith "Expected MasterDataImported payload"
              )

              testCase "should preserve metadata when converting roundtrip for demand signals" (fun () ->
                  let tenantId = "tenant-test-3"
                  let correlationId = Guid.NewGuid()
                  let order = { OrderId = "ORD-001"; SkuId = "SKU-100"; NodeId = "NODE-1"; Quantity = 50m; RequestedDateUtc = DateTimeOffset.UtcNow }
                  let payload = { CustomerOrders = [order]; Forecasts = [] }
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
                          | _ -> failwith "Expected DemandSignalsImported payload"
              ) ]
