module Medhavi.Integration.Adapters.Uom

open System
open System.Threading
open Medhavi.Common.Patterns
open Medhavi.Contracts.MasterData.Uom
open Medhavi.Integration
open Medhavi.Infrastructure.IO
open Medhavi.Infrastructure.Stores.EnvelopeStore
open Medhavi.Infrastructure

module ACL =
    let parse (uomCsv: string) : Result<UomDefineReq list, string> =
        try
            let rows = CsvHelper.parseCsv uomCsv
            let uoms =
                rows
                |> Array.toList
                |> List.map (fun row ->
                    let id = row.Get "Id" |> Option.defaultValue ""
                    let code = row.Get "Code" |> Option.defaultValue ""
                    let name = row.Get "Name" |> Option.defaultValue ""
                    let isBase = row.GetBool "IsBase" |> Option.defaultValue true
                    let toBase = row.GetDecimal "ToBaseFactor" |> Option.defaultValue 1.0m
                    { Id = id
                      Code = code
                      Name = name
                      IsBase = isBase
                      ToBaseFactor = toBase
                      Created = DateTimeOffset.UtcNow })
            Ok uoms
        with ex ->
            Error ex.Message

let ingestUoms (file: string) : TaskResult<UomDefineReq list, IntegrationError> =
    task {
        try
            let uomCsv = readCsvFile file
            return
                ACL.parse uomCsv
                |> Result.mapError IngestionError
        with ex ->
            return Error(IngestionError ex.Message)
    }

let publishUoms (store: EnvelopeStoreOps) (uoms: UomDefineReq list) : TaskResult<Envelope, IntegrationError> =
    task {
        try
            let tenantId = "tenant-mountain-bike"
            let correlationId = Guid.NewGuid()
            let event = UomImported uoms

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

let ingestAndPublishUoms (file: string) (store: EnvelopeStoreOps) : TaskResult<Envelope, IntegrationError> =
    taskResult {
        let! uoms = ingestUoms file
        return! publishUoms store uoms
    }
