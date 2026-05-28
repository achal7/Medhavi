module Medhavi.Integration.Adapters.SupplyOrder

open System
open System.Threading
open Medhavi.Common.Patterns
open Medhavi.Contracts
open Medhavi.Contracts.Integration
open Medhavi.Integration
open Medhavi.Infrastructure.IO
open Medhavi.Infrastructure.Stores.EnvelopeStore
open Medhavi.Infrastructure

module ACL =
    let parse (csvText: string) : Result<SupplyOrderStatusPayload list, string> =
        try
            let rows = CsvHelper.parseCsv csvText

            let parseRow (row: CsvHelper.CsvRow) =
                let id = row.Get "SupplyOrderId" |> Option.defaultValue ""
                let prod = row.Get "ProductId" |> Option.defaultValue ""

                let sp =
                    row.Get "StockingPointId"
                    |> Option.defaultValue ""

                let qty =
                    row.GetDecimal "Quantity"
                    |> Option.defaultValue 0.0m

                let dt =
                    row.GetDateTimeOffset "ExpectedDeliveryUtc"
                    |> Option.defaultValue DateTimeOffset.UtcNow

                let status = row.Get "Status" |> Option.defaultValue ""

                { SupplyOrderId = id
                  ProductId = prod
                  StockingPointId = sp
                  Quantity = qty
                  ExpectedDeliveryUtc = dt
                  Status = status }

            rows |> Array.toList |> List.map parseRow |> Ok
        with ex ->
            Error ex.Message

let ingestSupplyOrders (file: string) : TaskResult<SupplyOrderStatusPayload list, IntegrationError> =
    task {
        try
            return
                file
                |> readCsvFile
                |> ACL.parse
                |> Result.mapError IngestionError
        with ex ->
            return Error(IngestionError ex.Message)
    }

let publishSupplyOrders (store: EnvelopeStoreOps) (orders: SupplyOrderStatusPayload list) : TaskResult<Envelope, IntegrationError> =
    task {
        try
            let tenantId = "tenant-mountain-bike"
            let correlationId = Guid.NewGuid()
            let event = SupplyOrdersImported orders

            match IntegrationEventEnvelope.create tenantId correlationId event with
            | Error err -> return Error(IngestionError(sprintf "Serialization failed: %A" err))
            | Ok envelope ->
                let! publishRes =
                    store.PublishSingle
                        "supply-orders-stream"
                        envelope
                        ExpectedRevision.Any
                        CancellationToken.None

                match publishRes with
                | Error err -> return Error(IngestionError(sprintf "Failed to write to EnvelopeStore: %A" err))
                | Ok _ -> return Ok envelope
        with ex ->
            return Error(IngestionError ex.Message)
    }

let ingestAndPublishSupplyOrders (file: string) (store: EnvelopeStoreOps) : TaskResult<Envelope, IntegrationError> =
    taskResult {
        let! orders = ingestSupplyOrders file
        return! publishSupplyOrders store orders
    }
