module Medhavi.Integration.Adapters.Demand

open System
open System.Threading
open Medhavi.Common.Patterns
open Medhavi.Contracts.Demand
open Medhavi.Integration
open Medhavi.Infrastructure.IO
open Medhavi.Infrastructure.Stores.EnvelopeStore
open Medhavi.Infrastructure

module ACL =
    let parse (demandCsv: string) : Result<DemandDefineReq list, string> =
        try
            let rows = CsvHelper.parseCsv demandCsv
            let demands =
                rows
                |> Array.toList
                |> List.map (fun row ->
                    let demandLineId = row.Get "DemandLineId" |> Option.defaultValue ""
                    let demandOrderId = row.Get "DemandOrderId" |> Option.defaultValue ""
                    let skuId = row.Get "SkuId" |> Option.defaultValue ""
                    let stockingPointId = row.Get "StockingPointId" |> Option.defaultValue ""
                    let customerId = row.Get "CustomerId" |> Option.defaultValue ""
                    let quantity = row.GetDecimal "Quantity" |> Option.defaultValue 0.0m
                    let uom = row.Get "UnitOfMeasure" |> Option.defaultValue ""
                    let orderDate = row.GetDateTimeOffset "OrderDate" |> Option.defaultValue DateTimeOffset.UtcNow
                    let earliestDelivery = row.GetDateTimeOffset "EarliestDeliveryDate"
                    let requestedDelivery = row.GetDateTimeOffset "RequestedDeliveryDate" |> Option.defaultValue DateTimeOffset.UtcNow
                    let latestDelivery = row.GetDateTimeOffset "LatestDeliveryDate"
                    let confirmedDelivery = row.GetDateTimeOffset "ConfirmedDeliveryDate"
                    let actualDelivery = row.GetDateTimeOffset "ActualDeliveryDate"
                    let priority = row.GetInt "Priority" |> Option.defaultValue 1
                    let category = row.Get "DemandCategory" |> Option.defaultValue "CustomerOrderDemand"
                    let isFirm = row.GetBool "IsFirm" |> Option.defaultValue true
                    let isFrozen = row.GetBool "IsFrozen" |> Option.defaultValue false
                    { DemandDefineReq.DemandLineId = demandLineId
                      DemandOrderId = demandOrderId
                      SkuId = skuId
                      StockingPointId = stockingPointId
                      CustomerId = customerId
                      Quantity = quantity
                      UnitOfMeasure = uom
                      OrderDate = orderDate
                      EarliestDeliveryDate = earliestDelivery
                      RequestedDeliveryDate = requestedDelivery
                      LatestDeliveryDate = latestDelivery
                      ConfirmedDeliveryDate = confirmedDelivery
                      ActualDeliveryDate = actualDelivery
                      Priority = priority
                      DemandCategory = category
                      IsFirm = isFirm
                      IsFrozen = isFrozen })
            Ok demands
        with ex ->
            Error ex.Message

let ingestDemands (file: string) : TaskResult<DemandDefineReq list, IntegrationError> =
    task {
        try
            let csvText = readCsvFile file
            return
                ACL.parse csvText
                |> Result.mapError IngestionError
        with ex ->
            return Error(IngestionError ex.Message)
    }

let publishDemands (store: EnvelopeStoreOps) (demands: DemandDefineReq list) : TaskResult<Envelope, IntegrationError> =
    task {
        try
            let tenantId = "tenant-mountain-bike"
            let correlationId = Guid.NewGuid()
            let event = DemandsImported demands

            match IntegrationEventEnvelope.create tenantId correlationId event with
            | Error err -> return Error(IngestionError(sprintf "Serialization failed: %A" err))
            | Ok envelope ->
                let! publishRes =
                    store.PublishSingle "master-data-stream" envelope ExpectedRevision.Any CancellationToken.None

                match publishRes with
                | Error err -> return Error(IngestionError(sprintf "Failed to write to EnvelopeStore: %A" err))
                | Ok _ -> return Ok envelope
        with ex ->
            return Error(IngestionError ex.Message)
    }

let ingestAndPublishDemands (file: string) (store: EnvelopeStoreOps) : TaskResult<Envelope, IntegrationError> =
    taskResult {
        let! demands = ingestDemands file
        return! publishDemands store demands
    }
