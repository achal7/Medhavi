namespace Medhavi.Integration

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Contracts
open Medhavi.Contracts.Integration
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Aggregate
open Medhavi.Common.Validation
open Medhavi.Infrastructure.Stores.EnvelopeStore
open Medhavi.Integration.Adapters

type LoadResult =
    | Success of envelopeId: Guid * correlationId: Guid
    | ValidationError of string list
    | IngestionError of string

type IntegrationCapabilities =
    { IngestAndPublishMasterData: unit -> Task<LoadResult>
      IngestAndPublishInventoryPositions: unit -> Task<LoadResult>
      IngestAndPublishSupplyOrders: unit -> Task<LoadResult> }

module IntegrationService =

    let getCsvPath fileName = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "csv", fileName)

    let readCsvFile fileName =
        let path = getCsvPath fileName

        if System.IO.File.Exists(path) then
            System.IO.File.ReadAllText(path)
        else
            // Fallback to project source relative path if running from test or other directory
            let fallbackPath =
                System.IO.Path.Combine("src", "Medhavi.Integration", "csv", fileName)

            if System.IO.File.Exists(fallbackPath) then
                System.IO.File.ReadAllText(fallbackPath)
            else
                let upFallbackPath = System.IO.Path.Combine("..", fallbackPath)

                if System.IO.File.Exists(upFallbackPath) then
                    System.IO.File.ReadAllText(upFallbackPath)
                else
                    let doubleUpFallbackPath = System.IO.Path.Combine("..", "..", fallbackPath)

                    if System.IO.File.Exists(doubleUpFallbackPath) then
                        System.IO.File.ReadAllText(doubleUpFallbackPath)
                    else
                        failwithf "CSV file not found: %s (Checked %s and fallbacks)" fileName path

    /// Orchestrates ingestion of CSV data, validates via Anti-Corruption Layer,
    /// and publishes the resulting IntegrationEvent envelope to the provided EnvelopeStore.
    let ingestAndPublishMasterData (store: EnvelopeStoreOps) : Task<LoadResult> =
        task {
            try
                let productsCsv = readCsvFile "products.csv"
                let stockingPointsCsv = readCsvFile "stocking_points.csv"
                let resourcesCsv = readCsvFile "resources.csv"
                let bomsCsv = readCsvFile "boms.csv"
                let routingsCsv = readCsvFile "routings.csv"
                let transportLegsCsv = readCsvFile "transport_legs.csv"
                let unitConversionsCsv = readCsvFile "unit_conversions.csv"
                let inventoryTargetsCsv = readCsvFile "inventory_targets.csv"
                let supplierOffersCsv = readCsvFile "supplier_offers.csv"

                let products =
                    InboundAdapter.parseProductCsv productsCsv
                    |> Result.defaultWith (fun e -> failwith e)

                let stockingPoints =
                    InboundAdapter.parseStockingPointCsv stockingPointsCsv
                    |> Result.defaultWith (fun e -> failwith e)

                let resources =
                    InboundAdapter.parseResourceCsv resourcesCsv
                    |> Result.defaultWith (fun e -> failwith e)

                let boms =
                    InboundAdapter.parseBomLineCsv bomsCsv
                    |> Result.defaultWith (fun e -> failwith e)

                let routings =
                    InboundAdapter.parseRoutingCsv routingsCsv
                    |> Result.defaultWith (fun e -> failwith e)

                let payload: MasterDataPayload =
                    { Products = products
                      Boms = boms
                      StockingPoints = stockingPoints
                      Resources = resources
                      Routings = routings
                      Suppliers = [] }

                match MasterDataValidator.validate payload with
                | Invalid errors -> return ValidationError errors
                | Valid _ ->
                    // 1. SKUs
                    let skus =
                        SkuAdapter.parse productsCsv
                        |> Result.defaultWith (fun e -> failwith e)
                    // 2. BOMs
                    let bomsList =
                        BomAdapter.parse bomsCsv
                        |> Result.defaultWith (fun e -> failwith e)
                    // 3. Plants & Stocking Points
                    let sps, nodes =
                        StockingPointAdapter.parse stockingPointsCsv
                        |> Result.defaultWith (fun e -> failwith e)
                    // 4. Routings
                    let routingsList =
                        RoutingAdapter.parse routingsCsv resourcesCsv boms
                        |> Result.defaultWith (fun e -> failwith e)
                    // 5. Transport Legs
                    let legs =
                        TransportLegAdapter.parse transportLegsCsv
                        |> Result.defaultWith (fun e -> failwith e)

                    // 6. UOMs
                    let allUoms =
                        UomAdapter.parse productsCsv transportLegsCsv
                        |> Result.defaultWith (fun e -> failwith e)

                    // 7. Plants
                    let plantsList =
                        PlantAdapter.parse stockingPointsCsv
                        |> Result.defaultWith (fun e -> failwith e)

                    // 8. Unit Conversions
                    let conversionList =
                        UnitConversionAdapter.parse unitConversionsCsv
                        |> Result.defaultWith (fun e -> failwith e)

                    // 9. Targets & Offers
                    let targets =
                        InventoryTarget.ACL.parse inventoryTargetsCsv
                        |> Result.defaultWith (fun e -> failwith e)

                    let offers =
                        SupplierOffer.ACL.parse supplierOffersCsv
                        |> Result.defaultWith (fun e -> failwith e)

                    let importedPayload: MasterDataImportedPayload =
                        { SkuRequests = skus
                          BomRequests = bomsList
                          StockingPointRequests = sps
                          NodeRequests = nodes
                          RoutingRequests = routingsList
                          TransportLegRequests = legs
                          UomRequests = allUoms
                          PlantRequests = plantsList
                          UnitConversionRequests = conversionList
                          InventoryTargetRequests = targets
                          SupplierOfferRequests = offers }

                    let tenantId = "tenant-mountain-bike"
                    let correlationId = Guid.NewGuid()
                    let event = MasterDataImported importedPayload

                    match IntegrationEventEnvelope.create tenantId correlationId event with
                    | Error err -> return IngestionError(sprintf "Serialization failed: %A" err)
                    | Ok envelope ->
                        let! publishRes =
                            store.PublishSingle
                                "master-data-stream"
                                envelope
                                ExpectedRevision.Any
                                CancellationToken.None

                        match publishRes with
                        | Error err -> return IngestionError(sprintf "Failed to write to EnvelopeStore: %A" err)
                        | Ok _ -> return Success(envelope.EventId, correlationId)
            with ex ->
                return IngestionError ex.Message
        }

    let ingestAndPublishInventoryPositions (store: EnvelopeStoreOps) : Task<LoadResult> =
        task {
            try
                let csv = readCsvFile "inventory_positions.csv"

                let payload =
                    InboundAdapter.parseInventoryPositionCsv csv
                    |> Result.defaultWith (fun e -> failwith e)

                let tenantId = "tenant-mountain-bike"
                let correlationId = Guid.NewGuid()
                let event = InventoryPositionsImported payload

                match IntegrationEventEnvelope.create tenantId correlationId event with
                | Error err -> return IngestionError(sprintf "Serialization failed: %A" err)
                | Ok envelope ->
                    let! publishRes =
                        store.PublishSingle
                            "inventory-positions-stream"
                            envelope
                            ExpectedRevision.Any
                            CancellationToken.None

                    match publishRes with
                    | Error err -> return IngestionError(sprintf "Failed to write to EnvelopeStore: %A" err)
                    | Ok _ -> return Success(envelope.EventId, correlationId)
            with ex ->
                return IngestionError ex.Message
        }

    let ingestAndPublishSupplyOrders (store: EnvelopeStoreOps) : Task<LoadResult> =
        task {
            try
                let csv = readCsvFile "supply_orders.csv"

                let payload =
                    InboundAdapter.parseSupplyOrderStatusCsv csv
                    |> Result.defaultWith (fun e -> failwith e)

                let tenantId = "tenant-mountain-bike"
                let correlationId = Guid.NewGuid()
                let event = SupplyOrdersImported payload

                match IntegrationEventEnvelope.create tenantId correlationId event with
                | Error err -> return IngestionError(sprintf "Serialization failed: %A" err)
                | Ok envelope ->
                    let! publishRes =
                        store.PublishSingle "supply-orders-stream" envelope ExpectedRevision.Any CancellationToken.None

                    match publishRes with
                    | Error err -> return IngestionError(sprintf "Failed to write to EnvelopeStore: %A" err)
                    | Ok _ -> return Success(envelope.EventId, correlationId)
            with ex ->
                return IngestionError ex.Message
        }

    /// Helper to retrieve the transport legs list directly parsed from the CSV.
    let getTransportLegs () : Result<TransportLegDefineReq list, string> =
        try
            let transportLegsCsv = readCsvFile "transport_legs.csv"
            InboundAdapter.parseTransportLegCsv transportLegsCsv
        with ex ->
            Error ex.Message

    let createCapabilities (store: EnvelopeStoreOps) : IntegrationCapabilities =
        { IngestAndPublishMasterData = fun () -> ingestAndPublishMasterData store
          IngestAndPublishInventoryPositions = fun () -> ingestAndPublishInventoryPositions store
          IngestAndPublishSupplyOrders = fun () -> ingestAndPublishSupplyOrders store }
