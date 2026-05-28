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
    let parse (resourceCsv: string) : Result<ResourceImportedPayload list, string> =
        try
            let rows = CsvHelper.parseCsv resourceCsv
            let parseRow (row: CsvHelper.CsvRow) =
                let id = row.Get "ResourceId" |> Option.defaultValue ""
                let name = row.Get "Name" |> Option.defaultValue ""
                let nodeId = row.Get "NodeId" |> Option.defaultValue ""
                let active = row.GetBool "IsActive" |> Option.defaultValue true
                { ResourceId = id
                  Name = name
                  NodeId = nodeId
                  IsActive = active }
            rows |> Array.toList |> List.map parseRow |> Ok
        with ex ->
            Error ex.Message

let ingestResources (file: string) : TaskResult<ResourceImportedPayload list, IntegrationError> =
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

let publishResources (store: EnvelopeStoreOps) (resources: ResourceImportedPayload list) : TaskResult<Envelope, IntegrationError> =
    task {
        try
            let tenantId = "tenant-mountain-bike"
            let correlationId = Guid.NewGuid()
            let event = ResourcesImported resources

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

let ingestAndPublishResources (file: string) (store: EnvelopeStoreOps) : TaskResult<Envelope, IntegrationError> =
    taskResult {
        let! resources = ingestResources file
        return! publishResources store resources
    }
