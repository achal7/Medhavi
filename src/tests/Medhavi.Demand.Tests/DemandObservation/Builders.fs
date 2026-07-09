module Medhavi.Demand.Tests.DemandObservation.Builders

open Medhavi.Contracts.Demand.DemandObservation
open System

let defaultEstablish: EstablishObservationReq =
    { ObservationId = "OBS-TEST-001"
      SkuId = "SKU-001"
      StockingPointId = "SP-001"
      Quantity = 100m
      UnitOfMeasure = "EA"
      ObservationType = ObservationType.SalesOrder
      BusinessTime = DateTimeOffset.UtcNow.AddHours(-1.0)
      CustomerId = None
      PromotionRef = None
      CampaignRef = None
      ContractRef = None
      SourceSystem = "ERP"
      ExternalRef = "ORD-001"
      MessageId = Guid.NewGuid().ToString()
      Revision = 1 }

let withId id (req: EstablishObservationReq) = { req with ObservationId = id }
let withQuantity q (req: EstablishObservationReq) = { req with Quantity = q }
let withSku s (req: EstablishObservationReq) = { req with SkuId = s }
let withObservationType t (req: EstablishObservationReq) = { req with ObservationType = t }
