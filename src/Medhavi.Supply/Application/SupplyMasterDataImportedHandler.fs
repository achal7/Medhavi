namespace Medhavi.Supply.Application

open System
open System.Threading.Tasks
open Medhavi.Contracts
open Medhavi.Contracts.Integration
open Medhavi.SharedKernel.BoundedContexts
open Medhavi.Supply

module SupplyMasterDataImportedHandler =

    let handle
        (caps: Supply)
        (payload: MasterDataImportedPayload)
        (logger: IngestionLogger)
        : Task<unit> =
        task {
            // 1. Ingest Inventory Targets
            logger.LogInfo "\n  [Supply BC] Ingesting Inventory Targets..."
            for req in payload.InventoryTargetRequests do
                let! res = caps.InventoryTarget.Define req
                match res with
                | Ok _ -> logger.LogSuccess(sprintf "    - Inventory Target Ingested: %s-%s [ OK ]" req.SkuId req.StockingPointId)
                | Error err -> logger.LogError(sprintf "    - Inventory Target Ingestion Error: %s-%s [ ERR: %A ]" req.SkuId req.StockingPointId err)

            // 2. Ingest Supplier Offers
            logger.LogInfo "\n  [Supply BC] Ingesting Supplier Offers..."
            for req in payload.SupplierOfferRequests do
                let! res = caps.SupplierOffer.Define req
                match res with
                | Ok _ -> logger.LogSuccess(sprintf "    - Supplier Offer Ingested: %s [ OK ]" req.Id)
                | Error err -> logger.LogError(sprintf "    - Supplier Offer Ingestion Error: %s [ ERR: %A ]" req.Id err)
        }
