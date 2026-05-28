module Medhavi.Integration.Adapters.Inventory

open System
open System.Threading
open Medhavi.Common.Patterns
open Medhavi.Contracts.Integration
open Medhavi.Integration
open Medhavi.Infrastructure.IO
open Medhavi.Infrastructure.Stores.EnvelopeStore
open Medhavi.Infrastructure

module ACL =
    let parse (csvText: string) : Result<InventoryDefineReq list, string> =
        try
            let rows = CsvHelper.parseCsv csvText

            let parseRow (row: CsvHelper.CsvRow) =
                let prod = row.Get "ProductId" |> Option.defaultValue ""
                let sp = row.Get "StockingPointId" |> Option.defaultValue ""
                let qty = row.GetDecimal "Quantity" |> Option.defaultValue 0.0m

                { Id = $"INV-{prod}-{sp}"
                  SkuId = prod
                  StockingPointId = sp
                  Quantity = qty
                  UnitOfMeasure = "UOM-PCS" }

            rows |> Array.toList |> List.map parseRow |> Ok
        with ex ->
            Error ex.Message

let ingestInventoryPositions (file: string) : TaskResult<InventoryDefineReq list, IntegrationError> =
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

let publishInventoryPositions (store: EnvelopeStoreOps) (positions: InventoryDefineReq list) : TaskResult<Envelope, IntegrationError> =
    task {
        try
            let tenantId = "tenant-mountain-bike"
            let correlationId = Guid.NewGuid()
            let event = InventoryPositionsImported positions

            match IntegrationEventEnvelope.create tenantId correlationId event with
            | Error err -> return Error(IngestionError(sprintf "Serialization failed: %A" err))
            | Ok envelope ->
                let! publishRes =
                    store.PublishSingle
                        "inventory-positions-stream"
                        envelope
                        ExpectedRevision.Any
                        CancellationToken.None

                match publishRes with
                | Error err -> return Error(IngestionError(sprintf "Failed to write to EnvelopeStore: %A" err))
                | Ok _ -> return Ok envelope
        with ex ->
            return Error(IngestionError ex.Message)
    }

let ingestAndPublishInventoryPositions (file: string) (store: EnvelopeStoreOps) : TaskResult<Envelope, IntegrationError> =
    taskResult {
        let! positions = ingestInventoryPositions file
        return! publishInventoryPositions store positions
    }
