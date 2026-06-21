module Medhavi.Integration.Adapters.UnitConversion

open System
open System.Threading
open Medhavi.Common.Patterns
open Medhavi.Contracts.MasterData.Uom
open Medhavi.Integration
open Medhavi.Infrastructure.IO
open Medhavi.Infrastructure.Stores.EnvelopeStore
open Medhavi.Infrastructure

module ACL =
    let parse (csvText: string) : Result<UnitConversionDefineReq list, string> =
        try
            if String.IsNullOrWhiteSpace(csvText) then
                [ { SourceUom = "UOM-BOX"
                    TargetUom = "UOM-PCS"
                    ConversionFactor = 10.0m
                    Created = DateTimeOffset.UtcNow } ]
                |> Ok
            else
                let rows = CsvHelper.parseCsv csvText
                let parseRow (row: CsvHelper.CsvRow) =
                    let src = row.Get "SourceUom" |> Option.defaultValue ""
                    let target = row.Get "TargetUom" |> Option.defaultValue ""
                    let factor = row.GetDecimal "ConversionFactor" |> Option.defaultValue 1.0m
                    let created = row.GetDateTimeOffset "Created" |> Option.defaultValue DateTimeOffset.UtcNow
                    { SourceUom = src
                      TargetUom = target
                      ConversionFactor = factor
                      Created = created }
                let conversions = rows |> Array.toList |> List.map parseRow
                if List.isEmpty conversions then
                    [ { SourceUom = "UOM-BOX"
                        TargetUom = "UOM-PCS"
                        ConversionFactor = 10.0m
                        Created = DateTimeOffset.UtcNow } ]
                        |> Ok
                else
                    Ok conversions
        with ex ->
            Error ex.Message

let ingestUnitConversions (file: string) : TaskResult<UnitConversionDefineReq list, IntegrationError> =
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

let publishUnitConversions (store: EnvelopeStoreOps) (conversions: UnitConversionDefineReq list) : TaskResult<Envelope, IntegrationError> =
    task {
        try
            let tenantId = "tenant-mountain-bike"
            let correlationId = Guid.NewGuid()
            let event = UnitConversionsImported conversions

            match IntegrationEventEnvelope.create tenantId correlationId event with
            | Error err -> return Error(IngestionError(sprintf "Serialization failed: %A" err))
            | Ok envelope ->
                let! publishRes =
                    store.PublishSingle
                        "master-data-stream"
                        envelope
                        ExpectedRevision.Any
                        CancellationToken.None

                match publishRes with
                | Error err -> return Error(IngestionError(sprintf "Failed to write to EnvelopeStore: %A" err))
                | Ok _ -> return Ok envelope
        with ex ->
            return Error(IngestionError ex.Message)
    }

let ingestAndPublishUnitConversions (file: string) (store: EnvelopeStoreOps) : TaskResult<Envelope, IntegrationError> =
    taskResult {
        let! conversions = ingestUnitConversions file
        return! publishUnitConversions store conversions
    }
