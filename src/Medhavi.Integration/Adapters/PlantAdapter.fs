module Medhavi.Integration.Adapters.Plant

open System
open System.Threading
open Medhavi.Common.Patterns
open Medhavi.Contracts.Integration
open Medhavi.Integration
open Medhavi.Infrastructure.IO
open Medhavi.Infrastructure.Stores.EnvelopeStore
open Medhavi.Infrastructure

module ACL =
    let parse (stockingPointsCsv: string) : Result<PlantDefineReq list, string> =
        try
            let rows = CsvHelper.parseCsv stockingPointsCsv
            let sps =
                rows
                |> Array.toList
                |> List.map (fun row ->
                    let id = row.Get "StockingPointId" |> Option.defaultValue ""
                    let name = row.Get "Name" |> Option.defaultValue ""
                    let active = row.GetBool "IsActive" |> Option.defaultValue true
                    (id, name, active))

            // Stocking points are currently mapped to PLANT-DEFAULT.
            // We map unique plant IDs from them.
            let plantIds =
                sps
                |> List.map (fun _ -> "PLANT-DEFAULT")
                |> List.distinct

            let plantsList =
                plantIds
                |> List.map (fun plantId ->
                    { Id = plantId
                      Code =
                        plantId
                            .Replace("PLANT-", "")
                            .Replace("-DEFAULT", "DEF")
                      Name = "Plant: " + plantId })

            Ok plantsList
        with ex ->
            Error ex.Message

let ingestPlants (file: string) : TaskResult<PlantDefineReq list, IntegrationError> =
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

let publishPlants (store: EnvelopeStoreOps) (plants: PlantDefineReq list) : TaskResult<Envelope, IntegrationError> =
    task {
        try
            let tenantId = "tenant-mountain-bike"
            let correlationId = Guid.NewGuid()
            let event = PlantsImported plants

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

let ingestAndPublishPlants (file: string) (store: EnvelopeStoreOps) : TaskResult<Envelope, IntegrationError> =
    taskResult {
        let! plants = ingestPlants file
        return! publishPlants store plants
    }

