module Medhavi.Integration.Adapters.Bom

open System
open System.Threading
open Medhavi.Common.Patterns
open Medhavi.Contracts.MasterData.Bom
open Medhavi.Integration
open Medhavi.Infrastructure.IO
open Medhavi.Infrastructure.Stores.EnvelopeStore
open Medhavi.Infrastructure

module ACL =
    let parse (csvText: string) : Result<BomDefineReq list, string> =
        try
            let rows = CsvHelper.parseCsv csvText

            let bomLines =
                rows
                |> Array.toList
                |> List.map (fun row ->
                    let parent = row.Get "ParentSkuId" |> Option.defaultValue ""
                    let comp = row.Get "ComponentSkuId" |> Option.defaultValue ""

                    let qty =
                        row.GetDecimal "QuantityRequired"
                        |> Option.defaultValue 0.0m

                    (parent, comp, qty))

            let groupedBoms = bomLines |> List.groupBy (fun (parent, _, _) -> parent)

            groupedBoms
            |> List.map (fun (parentSkuId, lines) ->
                let items =
                    lines
                    |> List.mapi (fun idx (_, comp, qty) ->
                        { ComponentSkuId = comp
                          Quantity = qty
                          UnitOfMeasureId = "UOM-DEFAULT"
                          Sequence = (idx + 1) * 10 })

                { Id = $"BOM-{parentSkuId}"
                  SkuId = parentSkuId
                  Items = items })
            |> Ok
        with ex ->
            Error ex.Message

let ingestBoms (file: string) : TaskResult<BomDefineReq list, IntegrationError> =
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

let publishBoms (store: EnvelopeStoreOps) (boms: BomDefineReq list) : TaskResult<Envelope, IntegrationError> =
    task {
        try
            let tenantId = "tenant-mountain-bike"
            let correlationId = Guid.NewGuid()
            let event = BomImported boms

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

let ingestAndPublishBoms (file: string) (store: EnvelopeStoreOps) : TaskResult<Envelope, IntegrationError> =
    taskResult {
        let! boms = ingestBoms file
        return! publishBoms store boms
    }
