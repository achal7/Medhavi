module Medhavi.Integration.Adapters.MaterialReservation

open System
open System.Threading
open Medhavi.Common.Patterns
open Medhavi.Contracts.Supply
open Medhavi.Integration
open Medhavi.Infrastructure.IO
open Medhavi.Infrastructure.Stores.EnvelopeStore
open Medhavi.Infrastructure

module ACL =
    let parse (csvText: string) : Result<MaterialReservationCreateReq list, string> =
        try
            let rows = CsvHelper.parseCsv csvText
            let reqs =
                rows
                |> Array.toList
                |> List.map (fun row ->
                    let id = row.Get "Id" |> Option.defaultValue ""
                    let key = row.Get "IdempotencyKey" |> Option.defaultValue ""
                    let skuId = row.Get "SkuId" |> Option.defaultValue ""
                    let spId = row.Get "StockingPointId" |> Option.defaultValue ""
                    let qty = row.GetDecimal "Quantity" |> Option.defaultValue 0.0m
                    let reqDate = row.GetDateTimeOffset "RequiredDate" |> Option.defaultValue DateTimeOffset.UtcNow
                    let expTime = row.GetDateTimeOffset "ExpiryTime" |> Option.defaultValue DateTimeOffset.UtcNow
                    { Id = id
                      IdempotencyKey = key
                      SkuId = skuId
                      StockingPointId = spId
                      Quantity = qty
                      RequiredDate = reqDate
                      ExpiryTime = expTime })
            Ok reqs
        with ex ->
            Error ex.Message

let ingestReservations (file: string) : TaskResult<MaterialReservationCreateReq list, IntegrationError> =
    task {
        try
            let csvText = readCsvFile file
            return
                ACL.parse csvText
                |> Result.mapError IngestionError
        with ex ->
            return Error(IngestionError ex.Message)
    }

let publishReservations (store: EnvelopeStoreOps) (reqs: MaterialReservationCreateReq list) : TaskResult<Envelope, IntegrationError> =
    task {
        try
            let tenantId = "tenant-mountain-bike"
            let correlationId = Guid.NewGuid()
            let event = MaterialReservationsImported reqs

            match IntegrationEventEnvelope.create tenantId correlationId event with
            | Error err -> return Error(IngestionError(sprintf "Serialization failed: %A" err))
            | Ok envelope ->
                let! publishRes =
                    store.PublishSingle "reservations-stream" envelope ExpectedRevision.Any CancellationToken.None

                match publishRes with
                | Error err -> return Error(IngestionError(sprintf "Failed to write to EnvelopeStore: %A" err))
                | Ok _ -> return Ok envelope
        with ex ->
            return Error(IngestionError ex.Message)
    }

let ingestAndPublishReservations (file: string) (store: EnvelopeStoreOps) : TaskResult<Envelope, IntegrationError> =
    taskResult {
        let! reqs = ingestReservations file
        return! publishReservations store reqs
    }
