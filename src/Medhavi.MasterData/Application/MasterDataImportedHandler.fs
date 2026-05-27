namespace Medhavi.MasterData.Application

open System
open System.Threading.Tasks
open Medhavi.Contracts
open Medhavi.Contracts.Integration
open Medhavi.SharedKernel.BoundedContexts
open Medhavi.MasterData

module MasterDataImportedHandler =

    let handle
        (caps: MasterData)
        (payload: MasterDataImportedPayload)
        (logger: IngestionLogger)
        : Task<unit> =
        task {
            // 1. Ingest Plants
            logger.LogInfo "\n  [MasterData BC] Ingesting Plants..."
            for req in payload.PlantRequests do
                let! res = caps.Plant.Define req
                match res with
                | Ok _ -> logger.LogSuccess(sprintf "    - Plant Ingested: %s [ OK ]" req.Id)
                | Error err -> logger.LogError(sprintf "    - Plant Ingestion Error: %s [ ERR: %A ]" req.Id err)

            // 2. Ingest UOMs
            logger.LogInfo "\n  [MasterData BC] Ingesting Units of Measure (UOMs)..."
            for req in payload.UomRequests do
                let! res = caps.Uom.Define req
                match res with
                | Ok _ -> logger.LogSuccess(sprintf "    - UOM Ingested: %s [ OK ]" req.Id)
                | Error err -> logger.LogError(sprintf "    - UOM Ingestion Error: %s [ ERR: %A ]" req.Id err)

            // 3. Ingest Unit Conversions
            logger.LogInfo "\n  [MasterData BC] Ingesting Unit Conversions..."
            for req in payload.UnitConversionRequests do
                let! res = caps.UnitConversion.Define req
                match res with
                | Ok _ -> logger.LogSuccess(sprintf "    - Unit Conversion Ingested: %s -> %s [ OK ]" req.SourceUom req.TargetUom)
                | Error err -> logger.LogError(sprintf "    - Unit Conversion Ingestion Error: %s -> %s [ ERR: %A ]" req.SourceUom req.TargetUom err)

            // 4. Ingest SKUs
            logger.LogInfo "\n  [MasterData BC] Ingesting SKUs..."
            for req in payload.SkuRequests do
                let! res = caps.Sku.Define req
                match res with
                | Ok _ -> logger.LogSuccess(sprintf "    - SKU Ingested: %s [ OK ]" req.Id)
                | Error err -> logger.LogError(sprintf "    - SKU Ingestion Error: %s [ ERR: %A ]" req.Id err)

            // 5. Ingest Stocking Points & Nodes
            logger.LogInfo "\n  [MasterData BC] Ingesting Stocking Points..."

            // Stocking points and nodes have a 1-to-1 relationship in the request
            for spReq in payload.StockingPointRequests do
                let! spRes = caps.StockingPoint.Define spReq

                // Find matching node definition request
                let nodeReqOpt =
                    payload.NodeRequests
                    |> List.tryFind (fun n -> n.Id = spReq.Id)

                match nodeReqOpt with
                | Some nodeReq ->
                    let! nodeRes = caps.Node.Define nodeReq

                    match spRes, nodeRes with
                    | Ok _, Ok _ ->
                        logger.LogSuccess(sprintf "    - Stocking Point & Node Ingested: %s [ OK ]" spReq.Id)
                    | _ -> logger.LogError(sprintf "    - Stocking Point Ingestion Error: %s [ ERR ]" spReq.Id)
                | None ->
                    match spRes with
                    | Ok _ -> logger.LogSuccess(sprintf "    - Stocking Point Ingested (No Node): %s [ OK ]" spReq.Id)
                    | _ -> logger.LogError(sprintf "    - Stocking Point Ingestion Error: %s [ ERR ]" spReq.Id)

            // 4. BOMs
            logger.LogInfo "\n  [MasterData BC] Ingesting Bill of Materials..."

            for req in payload.BomRequests do
                let! res = caps.Bom.Define req

                match res with
                | Ok _ -> logger.LogSuccess(sprintf "    - BOM Ingested for Parent Sku: %s [ OK ]" req.SkuId)
                | Error err -> logger.LogError(sprintf "    - BOM Ingestion Error: %s [ ERR: %A ]" req.SkuId err)

            // 5. Routings
            logger.LogInfo "\n  [MasterData BC] Ingesting Routings & Resources..."

            for req in payload.RoutingRequests do
                let! res = caps.Routing.Define req

                match res with
                | Ok _ -> logger.LogSuccess(sprintf "    - Routing & Steps Ingested for Sku: %s [ OK ]" req.Id)
                | Error err -> logger.LogError(sprintf "    - Routing Ingestion Error: %s [ ERR: %A ]" req.Id err)

            // 6. Transport Legs Ingest
            logger.LogInfo "\n  [MasterData BC] Ingesting Transport Legs..."

            for req in payload.TransportLegRequests do
                let! res = caps.TransportLeg.Define req

                match res with
                | Ok _ ->
                    logger.LogSuccess(
                        sprintf "    - Transport Leg Ingested: %s (%s -> %s) [ OK ]" req.Id req.Origin req.Destination
                    )
                | Error err -> logger.LogError(sprintf "    - Transport Leg Ingestion Error: %s [ ERR: %A ]" req.Id err)

            logger.LogSuccess "\n   [ SUCCESS ] All events processed and committed to Bounded Context DBs."
        }
