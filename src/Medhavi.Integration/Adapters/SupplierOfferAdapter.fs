module Medhavi.Integration.Adapters.SupplierOffer

open System
open System.Threading
open Medhavi.Common.Patterns
open Medhavi.Contracts.Integration
open Medhavi.Integration
open Medhavi.Infrastructure.IO
open Medhavi.Infrastructure.Stores.EnvelopeStore
open Medhavi.Infrastructure

module ACL =
    let parseSupplierOfferCsv csv =
        let rows = CsvHelper.parseCsv csv

        let parseRow (row: CsvHelper.CsvRow) =
            let id = row.Get "Id" |> Option.defaultValue ""
            let supplier = row.Get "SupplierId" |> Option.defaultValue ""
            let sku = row.Get "SkuId" |> Option.defaultValue ""
            let sp = row.Get "StockingPointId"
            let moq = row.GetDecimal "Moq"
            let lot = row.GetDecimal "LotSize"
            let ltP50 = row.GetDecimal "LeadTimeP50Minutes"
            let ltP95 = row.GetDecimal "LeadTimeP95Minutes"
            let rel = row.GetDecimal "Reliability"
            let incoterm = row.Get "Incoterm"

            let defaultPriceTier: PriceTierReq =
                { TierNumber = 1
                  MinQuantity = 0.0m
                  MaxQuantity = None
                  PricePerUnit = 1.0m
                  Currency = "USD" }

            { Id = id
              SupplierId = supplier
              SkuId = sku
              StockingPointId = sp
              Moq = moq
              LotSize = lot
              LeadTimeP50Minutes = ltP50
              LeadTimeP95Minutes = ltP95
              PriceTiers = [ defaultPriceTier ]
              Reliability = rel
              Incoterm = incoterm
              CapacityWindows = []
              CreatedDate = DateTimeOffset.UtcNow }

        rows |> Array.toList |> List.map parseRow |> Ok

    let parse (csvText: string) : Result<SupplierOfferDefineReq list, string> =
        try
            parseSupplierOfferCsv csvText
        with ex ->
            Error ex.Message

let ingestSupplierOffers (file: string) : TaskResult<SupplierOfferDefineReq list, IntegrationError> =
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

let publishSupplierOffers
    (store: EnvelopeStoreOps)
    (offers: SupplierOfferDefineReq list)
    : TaskResult<Envelope, IntegrationError> =
    task {
        try
            let tenantId = "tenant-mountain-bike"
            let correlationId = Guid.NewGuid()
            let event = SupplyOffersImported offers

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

let ingestAndPublishSupplierOffers (file: string) (store: EnvelopeStoreOps) : TaskResult<Envelope, IntegrationError> =
    taskResult {
        let! offers = ingestSupplierOffers file
        return! publishSupplierOffers store offers
    }
