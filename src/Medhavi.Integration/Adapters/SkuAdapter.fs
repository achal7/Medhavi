module Medhavi.Integration.Adapters.Sku

open System
open System.Threading
open Medhavi.Common.Patterns
open Medhavi.Contracts.Integration
open Medhavi.Integration
open Medhavi.Infrastructure.IO
open Medhavi.Infrastructure.Stores.EnvelopeStore
open Medhavi.Infrastructure

module ACL =
    let parse (productsCsv: string) : Result<SkuDefineReq list, string> =
        try
            let rows = CsvHelper.parseCsv productsCsv
            let products =
                rows
                |> Array.toList
                |> List.map (fun row ->
                    let id = row.Get "SkuId" |> Option.defaultValue ""
                    let name = row.Get "Name" |> Option.defaultValue ""
                    let uom = row.Get "UoM" |> Option.defaultValue ""
                    let active = row.GetBool "IsActive" |> Option.defaultValue true
                    (id, name, uom, active))

            products
            |> List.map (fun (skuId, name, _, _) ->
                { Id = skuId
                  Code = skuId
                  Name = name
                  Group = "Simulation"
                  Created = DateTimeOffset.UtcNow })
            |> Ok
        with ex ->
            Error ex.Message

let ingestSkus (file: string) : TaskResult<SkuDefineReq list, IntegrationError> =
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

let publishSkus (store: EnvelopeStoreOps) (skus: SkuDefineReq list) : TaskResult<Envelope, IntegrationError> =
    task {
        try
            let tenantId = "tenant-mountain-bike"
            let correlationId = Guid.NewGuid()
            let event = SkusImported skus

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

let ingestAndPublishSkus (file: string) (store: EnvelopeStoreOps) : TaskResult<Envelope, IntegrationError> =
    taskResult {
        let! skus = ingestSkus file
        return! publishSkus store skus
    }
