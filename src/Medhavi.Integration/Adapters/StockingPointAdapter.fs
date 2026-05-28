module Medhavi.Integration.Adapters.StockingPoint

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
    let parse (csvText: string) : Result<(StockingPointDefineReq list * NodeDefineReq list), string> =
        try
            let rows = CsvHelper.parseCsv csvText
            let sps =
                rows
                |> Array.toList
                |> List.map (fun row ->
                    let id = row.Get "StockingPointId" |> Option.defaultValue ""
                    let name = row.Get "Name" |> Option.defaultValue ""
                    let active = row.GetBool "IsActive" |> Option.defaultValue true
                    { StockingPointId = id; Name = name; IsActive = active })

            let spReqs =
                sps
                |> List.map (fun sp ->
                    { Id = sp.StockingPointId
                      PlantId = "PLANT-DEFAULT"
                      Code = sp.StockingPointId
                      Name = sp.Name
                      Type = "Warehouse"
                      Location = None
                      Level = None
                      PlanningLevel = None
                      SupplyCanBeSplit = false })
            let nodeReqs =
                sps
                |> List.map (fun sp ->
                    { Id = sp.StockingPointId
                      Code = sp.StockingPointId
                      Name = sp.Name
                      Type = "StockingPoint"
                      Attributes = { LocationCode = None; PlanningLevel = None; StockingPointRef = Some sp.StockingPointId }
                      Created = DateTimeOffset.UtcNow })
            Ok (spReqs, nodeReqs)
        with ex ->
            Error ex.Message

let ingestStockingPoints (file: string) : TaskResult<StockingPointDefineReq list * NodeDefineReq list, IntegrationError> =
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

let publishStockingPoints (store: EnvelopeStoreOps) (sps: StockingPointDefineReq list, nodes: NodeDefineReq list) : TaskResult<Envelope, IntegrationError> =
    task {
        try
            let tenantId = "tenant-mountain-bike"
            let correlationId = Guid.NewGuid()
            let event = StockingPointsImported sps

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

let ingestAndPublishStockingPoints (file: string) (store: EnvelopeStoreOps) : TaskResult<Envelope, IntegrationError> =
    taskResult {
        let! (sps, nodes) = ingestStockingPoints file
        return! publishStockingPoints store (sps, nodes)
    }
