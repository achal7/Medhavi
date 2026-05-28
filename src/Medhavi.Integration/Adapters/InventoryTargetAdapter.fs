module Medhavi.Integration.Adapters.InventoryTarget

open System
open System.Threading
open Medhavi.Common.Patterns
open Medhavi.Contracts.Integration
open Medhavi.Integration
open Medhavi.Infrastructure.IO
open Medhavi.Infrastructure.Stores.EnvelopeStore
open Medhavi.Infrastructure

module ACL =
    let parseInventoryTargetCsv csv =
        let rows = CsvHelper.parseCsv csv

        let parseRow (row: CsvHelper.CsvRow) =
            let sku = row.Get "SkuId" |> Option.defaultValue ""

            let sp =
                row.Get "StockingPointId"
                |> Option.defaultValue ""

            let safety = row.GetDecimal "SafetyStockQty"
            let minQty = row.GetDecimal "MinQty"
            let maxQty = row.GetDecimal "MaxQty"
            let serviceLevel = row.GetDecimal "TargetServiceLevel"
            let coverDays = row.GetDecimal "CoverDays"
            let active = row.GetBool "IsActive" |> Option.defaultValue true

            { SkuId = sku
              StockingPointId = sp
              ReplenishmentPolicy = None
              SafetyStockQty = safety
              MinQty = minQty
              MaxQty = maxQty
              TargetServiceLevel = serviceLevel
              CoverDays = coverDays
              SeasonalAdjustments = []
              EffectiveStart = None
              EffectiveEnd = None
              IsActive = active }

        rows |> Array.toList |> List.map parseRow |> Ok

    let parse (csvText: string) : Result<InventoryTargetDefineReq list, string> =
        try
            parseInventoryTargetCsv csvText
        with ex ->
            Error ex.Message

let ingestInventoryTargets (file: string) : TaskResult<InventoryTargetDefineReq list, IntegrationError> =
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

let publishInventoryTargets (store: EnvelopeStoreOps) (targets: InventoryTargetDefineReq list) : TaskResult<Envelope, IntegrationError> =
    task {
        try
            let tenantId = "tenant-mountain-bike"
            let correlationId = Guid.NewGuid()
            let event = InventoryTargetsImported targets

            match IntegrationEventEnvelope.create tenantId correlationId event with
            | Error err -> return Error(IngestionError(sprintf "Serialization failed: %A" err))
            | Ok envelope ->
                let! publishRes =
                    store.PublishSingle
                        "inventory-targets-stream"
                        envelope
                        ExpectedRevision.Any
                        CancellationToken.None

                match publishRes with
                | Error err -> return Error(IngestionError(sprintf "Failed to write to EnvelopeStore: %A" err))
                | Ok _ -> return Ok envelope
        with ex ->
            return Error(IngestionError ex.Message)
    }

let ingestAndPublishInventoryTargets (file: string) (store: EnvelopeStoreOps) : TaskResult<Envelope, IntegrationError> =
    taskResult {
        let! targets = ingestInventoryTargets file
        return! publishInventoryTargets store targets
    }
