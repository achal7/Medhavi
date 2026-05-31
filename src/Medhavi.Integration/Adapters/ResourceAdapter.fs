module Medhavi.Integration.Adapters.Resource

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
    let parseResourceGroups (csvText: string) : Result<ResourceGroupDefineReq list, string> =
        try
            let rows = CsvHelper.parseCsv csvText

            let parseRow (row: CsvHelper.CsvRow) =
                { Id =
                    row.Get "ResourceGroupId"
                    |> Option.defaultValue ""
                  PlantId = row.Get "PlantId"
                  Name = row.Get "Name" |> Option.defaultValue ""
                  Description = row.Get "Description"
                  DefaultCalendarId = row.Get "DefaultCalendarId"
                  IsActive = row.GetBool "IsActive" |> Option.defaultValue true
                  Created = DateTimeOffset.UtcNow }

            rows |> Array.toList |> List.map parseRow |> Ok
        with ex ->
            Error ex.Message

    let parseStandardResources (csvText: string) : Result<StandardResourceDefineReq list, string> =
        try
            let rows = CsvHelper.parseCsv csvText

            let parseRow (row: CsvHelper.CsvRow) =
                { Id =
                    row.Get "StandardResourceId"
                    |> Option.defaultValue ""
                  ResourceGroupId =
                    row.Get "ResourceGroupId"
                    |> Option.defaultValue ""
                  Name = row.Get "Name" |> Option.defaultValue ""
                  Description = row.Get "Description"
                  DefaultEfficiency =
                    row.GetDecimal "DefaultEfficiency"
                    |> Option.defaultValue 1.0M
                  DefaultCostRateAmount = row.GetDecimal "DefaultCostRateAmount"
                  DefaultCostRateCurrency = row.Get "DefaultCostRateCurrency"
                  IsActive = row.GetBool "IsActive" |> Option.defaultValue true
                  Created = DateTimeOffset.UtcNow }

            rows |> Array.toList |> List.map parseRow |> Ok
        with ex ->
            Error ex.Message

    let parsePhysicalResources (csvText: string) : Result<PhysicalResourceDefineReq list, string> =
        try
            let rows = CsvHelper.parseCsv csvText

            let parseRow (row: CsvHelper.CsvRow) =
                { Id =
                    row.Get "PhysicalResourceId"
                    |> Option.defaultValue ""
                  StandardResourceId =
                    row.Get "StandardResourceId"
                    |> Option.defaultValue ""
                  Name = row.Get "Name" |> Option.defaultValue ""
                  SerialNumber = row.Get "SerialNumber"
                  Location = row.Get "Location"
                  EfficiencyOverride = row.GetDecimal "EfficiencyOverride"
                  CostRateOverrideAmount = row.GetDecimal "CostRateOverrideAmount"
                  CostRateOverrideCurrency = row.Get "CostRateOverrideCurrency"
                  CalendarId = row.Get "CalendarId"
                  IsActive = row.GetBool "IsActive" |> Option.defaultValue true
                  Created = DateTimeOffset.UtcNow }

            rows |> Array.toList |> List.map parseRow |> Ok
        with ex ->
            Error ex.Message

let ingestResourceGroups (file: string) : TaskResult<ResourceGroupDefineReq list, IntegrationError> =
    task {
        try
            return
                file
                |> readCsvFile
                |> ACL.parseResourceGroups
                |> Result.mapError IngestionError
        with ex ->
            return Error(IngestionError ex.Message)
    }

let publishResourceGroups
    (store: EnvelopeStoreOps)
    (payloads: ResourceGroupDefineReq list)
    : TaskResult<Envelope, IntegrationError> =
    task {
        try
            let tenantId = "tenant-mountain-bike"
            let correlationId = Guid.NewGuid()
            let event = ResourceGroupsImported payloads

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

let ingestAndPublishResourceGroups (file: string) (store: EnvelopeStoreOps) : TaskResult<Envelope, IntegrationError> =
    taskResult {
        let! payloads = ingestResourceGroups file
        return! publishResourceGroups store payloads
    }

let ingestStandardResources (file: string) : TaskResult<StandardResourceDefineReq list, IntegrationError> =
    task {
        try
            return
                file
                |> readCsvFile
                |> ACL.parseStandardResources
                |> Result.mapError IngestionError
        with ex ->
            return Error(IngestionError ex.Message)
    }

let publishStandardResources
    (store: EnvelopeStoreOps)
    (payloads: StandardResourceDefineReq list)
    : TaskResult<Envelope, IntegrationError> =
    task {
        try
            let tenantId = "tenant-mountain-bike"
            let correlationId = Guid.NewGuid()
            let event = StandardResourcesImported payloads

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

let ingestAndPublishStandardResources
    (file: string)
    (store: EnvelopeStoreOps)
    : TaskResult<Envelope, IntegrationError> =
    taskResult {
        let! payloads = ingestStandardResources file
        return! publishStandardResources store payloads
    }

let ingestPhysicalResources (file: string) : TaskResult<PhysicalResourceDefineReq list, IntegrationError> =
    task {
        try
            return
                file
                |> readCsvFile
                |> ACL.parsePhysicalResources
                |> Result.mapError IngestionError
        with ex ->
            return Error(IngestionError ex.Message)
    }

let publishPhysicalResources
    (store: EnvelopeStoreOps)
    (payloads: PhysicalResourceDefineReq list)
    : TaskResult<Envelope, IntegrationError> =
    task {
        try
            let tenantId = "tenant-mountain-bike"
            let correlationId = Guid.NewGuid()
            let event = PhysicalResourcesImported payloads

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

let ingestAndPublishPhysicalResources
    (file: string)
    (store: EnvelopeStoreOps)
    : TaskResult<Envelope, IntegrationError> =
    taskResult {
        let! payloads = ingestPhysicalResources file
        return! publishPhysicalResources store payloads
    }
