module Medhavi.Integration.Adapters.Routing

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
    let parse (routingCsv: string) (resourceCsv: string) (bomLinesCsv: string) : Result<RoutingDefineReq list, string> =
        try
            // Parse routingCsv
            let routingRows = CsvHelper.parseCsv routingCsv
            let parseRoutingRow (row: CsvHelper.CsvRow) =
                let skuId = row.Get "SkuId" |> Option.defaultValue ""
                let seq = row.GetInt "Sequence" |> Option.defaultValue 0
                let resId = row.Get "ResourceId" |> Option.defaultValue ""
                let setup = row.GetFloat "SetupHours" |> Option.defaultValue 0.0
                let run = row.GetFloat "RunHoursPerUnit" |> Option.defaultValue 0.0
                (skuId, { Sequence = seq; ResourceId = resId; SetupHours = setup; RunHoursPerUnit = run })

            let routingsFlat = routingRows |> Array.toList |> List.map parseRoutingRow
            let routings =
                routingsFlat
                |> List.groupBy fst
                |> List.map (fun (skuId, items) ->
                    { SkuId = skuId
                      Steps = items |> List.map snd })

            // Parse resourceCsv
            let resourceRows = CsvHelper.parseCsv resourceCsv
            let resources =
                resourceRows
                |> Array.toList
                |> List.map (fun row ->
                    let id = row.Get "ResourceId" |> Option.defaultValue ""
                    let name = row.Get "Name" |> Option.defaultValue ""
                    let nodeId = row.Get "NodeId" |> Option.defaultValue ""
                    let active = row.GetBool "IsActive" |> Option.defaultValue true
                    { ResourceId = id; Name = name; NodeId = nodeId; IsActive = active })

            // Parse bomLinesCsv
            let bomRows = CsvHelper.parseCsv bomLinesCsv
            let bomLines =
                bomRows
                |> Array.toList
                |> List.map (fun row ->
                    let parent = row.Get "ParentSkuId" |> Option.defaultValue ""
                    let comp = row.Get "ComponentSkuId" |> Option.defaultValue ""
                    let qty = row.GetDecimal "QuantityRequired" |> Option.defaultValue 0.0m
                    { ParentSkuId = parent; ComponentSkuId = comp; QuantityRequired = qty })

            routings
            |> List.map (fun r ->
                let steps = 
                    r.Steps 
                    |> List.map (fun s -> 
                        { StepId = $"STEP-{r.SkuId}-{s.Sequence}"
                          Sequence = s.Sequence
                          ResourceGroupId = Some s.ResourceId
                          Yield = None })

                let stepResources =
                    r.Steps
                    |> List.map (fun s ->
                        { StepId = $"STEP-{r.SkuId}-{s.Sequence}"
                          ResourceId = s.ResourceId
                          IsAllowed = true
                          Sequence = s.Sequence
                          DurationPerUnitMinutes = Some (decimal (s.RunHoursPerUnit * 60.0)) })

                let firstStepOpt = r.Steps |> List.sortBy (fun s -> s.Sequence) |> List.tryHead
                let lastStepOpt = r.Steps |> List.sortBy (fun s -> s.Sequence) |> List.tryLast

                let firstStepId = 
                    match firstStepOpt with
                    | Some s -> $"STEP-{r.SkuId}-{s.Sequence}"
                    | None -> $"STEP-{r.SkuId}-10"
                let lastStepId = 
                    match lastStepOpt with
                    | Some s -> $"STEP-{r.SkuId}-{s.Sequence}"
                    | None -> $"STEP-{r.SkuId}-10"

                let getStepNodeId (stepOpt: RoutingStepImportedPayload option) =
                    match stepOpt with
                    | None -> "SP-FACTORY"
                    | Some s ->
                        resources 
                        |> List.tryFind (fun res -> res.ResourceId = s.ResourceId)
                        |> Option.map (fun res -> res.NodeId)
                        |> Option.defaultValue "SP-FACTORY"

                let firstStepNodeId = getStepNodeId firstStepOpt
                let lastStepNodeId = getStepNodeId lastStepOpt

                let relatedBoms = bomLines |> List.filter (fun b -> b.ParentSkuId = r.SkuId)

                let inputs =
                    if List.isEmpty relatedBoms then
                        [ { StepId = firstStepId
                            SkuId = "SKU-FRAME"
                            NodeId = firstStepNodeId
                            ConversionRate = Some 1.0m } ]
                    else
                        relatedBoms |> List.map (fun b ->
                            { StepId = firstStepId
                              SkuId = b.ComponentSkuId
                              NodeId = firstStepNodeId
                              ConversionRate = Some b.QuantityRequired })

                let outputs =
                    [ { StepId = lastStepId
                        SkuId = r.SkuId
                        NodeId = lastStepNodeId
                        ConversionRate = Some 1.0m
                        IsCoSku = false } ]

                { Id = $"ROUTING-{r.SkuId}"
                  Name = $"Routing for {r.SkuId}"
                  Type = "Work"
                  EffectiveStart = DateTimeOffset.UtcNow
                  EffectiveEnd = None
                  Steps = steps
                  Inputs = inputs
                  Outputs = outputs
                  StepResources = stepResources
                  Created = DateTimeOffset.UtcNow })
            |> Ok
        with ex ->
            Error ex.Message

let ingestRoutings (file: string) : TaskResult<RoutingDefineReq list, IntegrationError> =
    task {
        try
            let routingCsv = readCsvFile file
            let resourceCsv = readCsvFile "resources.csv"
            let bomLinesCsv = readCsvFile "boms.csv"
            return
                ACL.parse routingCsv resourceCsv bomLinesCsv
                |> Result.mapError IngestionError
        with ex ->
            return Error(IngestionError ex.Message)
    }

let publishRoutings (store: EnvelopeStoreOps) (routings: RoutingDefineReq list) : TaskResult<Envelope, IntegrationError> =
    task {
        try
            let tenantId = "tenant-mountain-bike"
            let correlationId = Guid.NewGuid()
            let event = RoutingsImported routings

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

let ingestAndPublishRoutings (file: string) (store: EnvelopeStoreOps) : TaskResult<Envelope, IntegrationError> =
    taskResult {
        let! routings = ingestRoutings file
        return! publishRoutings store routings
    }
