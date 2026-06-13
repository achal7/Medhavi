module Medhavi.Integration.Adapters.TransportLeg

open System
open System.Threading
open Medhavi.Common.Patterns
open Medhavi.Contracts.Transport
open Medhavi.Integration
open Medhavi.Infrastructure.IO
open Medhavi.Infrastructure.Stores.EnvelopeStore
open Medhavi.Infrastructure

module ACL =
    let parse (csvText: string) : Result<TransportLegDefineReq list, string> =
        try
            let rows = CsvHelper.parseCsv csvText
            let parseRow (row: CsvHelper.CsvRow) =
                let id = row.Get "Id" |> Option.defaultValue ""
                let origin = row.Get "Origin" |> Option.defaultValue ""
                let dest = row.Get "Destination" |> Option.defaultValue ""
                let mode = row.Get "Mode" |> Option.defaultValue ""
                let schedule = row.Get "Schedule" |> Option.defaultValue ""
                let lt = row.GetDecimal "LeadTimeMinutes" |> Option.defaultValue 0.0m
                let cap = row.GetDecimal "Capacity"
                let capUnit = row.Get "CapacityUnit"
                let cutoff = row.GetDecimal "CutoffMinutes"
                let constraints =
                    match row.Get "Constraints" with
                    | None -> []
                    | Some s ->
                        s.Split([| '|'; ';' |], StringSplitOptions.RemoveEmptyEntries)
                        |> Array.toList
                        |> List.map (fun x -> x.Trim())
                let rel = row.GetDecimal "Reliability"
                let co2 = row.GetDecimal "CO2PerUnit"
                let start = row.GetDateTimeOffset "EffectiveStart" |> Option.defaultValue DateTimeOffset.UtcNow
                let end' = row.GetDateTimeOffset "EffectiveEnd"
                let created = row.GetDateTimeOffset "Created" |> Option.defaultValue DateTimeOffset.UtcNow
                { Id = id
                  Origin = origin
                  Destination = dest
                  Mode = mode
                  Schedule = schedule
                  LeadTimeMinutes = lt
                  Capacity = cap
                  CapacityUnit = capUnit
                  CutoffMinutes = cutoff
                  Constraints = constraints
                  Reliability = rel
                  CO2PerUnit = co2
                  EffectiveStart = start
                  EffectiveEnd = end'
                  Created = created }
            rows |> Array.toList |> List.map parseRow |> Ok
        with ex ->
            Error ex.Message

let ingestTransportLegs (file: string) : TaskResult<TransportLegDefineReq list, IntegrationError> =
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

let publishTransportLegs (store: EnvelopeStoreOps) (legs: TransportLegDefineReq list) : TaskResult<Envelope, IntegrationError> =
    task {
        try
            let tenantId = "tenant-mountain-bike"
            let correlationId = Guid.NewGuid()
            let event = TransportLegsImported legs

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

let ingestAndPublishTransportLegs (file: string) (store: EnvelopeStoreOps) : TaskResult<Envelope, IntegrationError> =
    taskResult {
        let! legs = ingestTransportLegs file
        return! publishTransportLegs store legs
    }
