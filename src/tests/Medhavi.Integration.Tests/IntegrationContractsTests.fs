namespace Medhavi.Integration.Tests

open System
open Expecto
open Swensen.Unquote
open Medhavi.SharedKernel
open Medhavi.Infrastructure
open Medhavi.Contracts
open Medhavi.Contracts.Integration
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
                  let payload : MasterDataImportedPayload = { 
                      SkuRequests = []
                      BomRequests = []
                      StockingPointRequests = []
                      NodeRequests = []
                      RoutingRequests = []
                      TransportLegRequests = [] 
                      UomRequests = []
                      PlantRequests = []
                      UnitConversionRequests = []
                  }
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
                  let skuReq : SkuDefineReq = { 
                      Id = "SKU-999"
                      Code = "SKU-999"
                      Name = "Widget"
                      Group = "Simulation"
                      Created = DateTimeOffset.UtcNow 
                  }
                  let payload : MasterDataImportedPayload = { 
                      SkuRequests = [skuReq]
                      BomRequests = []
                      StockingPointRequests = []
                      NodeRequests = []
                      RoutingRequests = []
                      TransportLegRequests = [] 
                      UomRequests = []
                      PlantRequests = []
                      UnitConversionRequests = []
                  }
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
              )

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
                  test <@ envelopeRehydrated.TenantId = Some "tenant-123" @>) ]
