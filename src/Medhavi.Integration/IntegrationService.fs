namespace Medhavi.Integration

open Medhavi.Common.Patterns
open Medhavi.Infrastructure
open Medhavi.Infrastructure.Stores.EnvelopeStore
open Medhavi.Integration.Adapters

type IntegrationCapabilities =
    { IngestAndPublishMasterData: unit -> TaskResult<Envelope list, IntegrationError> }

module IntegrationService =

    let collect steps =
        task {
            let mutable envelopes = []
            let mutable error = None

            for step in steps do
                match error with
                | Some _ -> ()
                | None ->
                    let! result = step ()

                    match result with
                    | Ok envs -> envelopes <- envelopes @ [ envs ]

                    | Error e -> error <- Some e

            match error with
            | Some e -> return Error e
            | None -> return Ok envelopes
        }

    /// Orchestrates ingestion of CSV data, validates via Anti-Corruption Layer,
    /// and publishes the resulting IntegrationEvent envelope to the provided EnvelopeStore.
    let ingestAndPublishMasterData (store: EnvelopeStoreOps) : TaskResult<Envelope list, IntegrationError> =
        collect
            [ fun () -> Uom.ingestAndPublishUoms "uoms.csv" store
              fun () -> Sku.ingestAndPublishSkus "products.csv" store
              fun () -> Demand.ingestAndPublishDemands "demands.csv" store
              fun () -> UnitConversion.ingestAndPublishUnitConversions "unit_conversions.csv" store
              fun () -> Plant.ingestAndPublishPlants "plants.csv" store
              fun () -> StockingPoint.ingestAndPublishStockingPoints "stocking_points.csv" store
              fun () -> Resource.ingestAndPublishResourceGroups "resource_groups.csv" store
              fun () -> Resource.ingestAndPublishStandardResources "standard_resources.csv" store
              fun () -> Resource.ingestAndPublishPhysicalResources "physical_resources.csv" store
              fun () -> Bom.ingestAndPublishBoms "boms.csv" store
              fun () -> Routing.ingestAndPublishRoutings "routings.csv" store
              fun () -> TransportLeg.ingestAndPublishTransportLegs "transport_legs.csv" store
              fun () -> Inventory.ingestAndPublishInventoryPositions "inventory_positions.csv" store
              fun () -> InventoryTarget.ingestAndPublishInventoryTargets "inventory_targets.csv" store
              fun () -> SupplyOrder.ingestAndPublishSupplyOrders "supply_orders.csv" store
              fun () -> SupplierOffer.ingestAndPublishSupplierOffers "supplier_offers.csv" store
              fun () -> MaterialReservation.ingestAndPublishReservations "reservations.csv" store ]

    let createCapabilities (store: EnvelopeStoreOps) =
        // store.Subscribe SubscriptionMode.All None handler CancellationToken.None
        {
            IngestAndPublishMasterData = fun () -> ingestAndPublishMasterData store
        }

