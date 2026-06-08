namespace Medhavi.Analytics.PlanningHorizon

open System
open System.Threading.Tasks

/// Demand data — reads from Medhavi.Demand query store.
/// Concrete implementation created in Medhavi.Nexus composition root.
type DemandDataSource =
    {
        /// Get demand lines for a plant within a date range (filtered by RequestedDeliveryDate)
        GetDemandLines  : string -> DateOnly -> DateOnly -> PlanContext -> Task<DemandLineView list>
        /// Get supply orders pegged to a specific demand line
        GetPeggedSupply : string -> PlanContext -> Task<PeggedSupplySummary list>
    }

/// Material/supply data — reads from Medhavi.Supply + inventory stores.
/// Concrete implementation created in Medhavi.Nexus composition root.
type MaterialDataSource =
    {
        /// Get all supply elements for a SKU at a plant within a date range
        GetSupplyElements    : string -> string -> DateOnly -> DateOnly -> PlanContext -> Task<SupplyElementView list>
        /// Get the current inventory snapshot for a SKU at a stocking point
        GetInventorySnapshot : string -> string -> DateOnly -> PlanContext -> Task<InventorySnapshot>
    }

/// Capacity data — reads from Medhavi.Capacity + Medhavi.Scheduler.
/// Concrete implementation created in Medhavi.Nexus composition root.
type CapacityDataSource =
    {
        /// Get all scheduled operations for a plant within a date range
        GetOperations         : string -> DateOnly -> DateOnly -> PlanContext -> Task<OperationView list>
        /// Get capacity buckets (available/calendar/maintenance hours) for a resource group
        GetCapacityBuckets    : string -> DateOnly -> DateOnly -> Task<CapacityBucketView list>
        /// Get maintenance windows for a plant
        GetMaintenanceWindows : string -> DateOnly -> DateOnly -> Task<MaintenanceView list>
    }

/// Transport data — reads from Medhavi.Transport.
/// Concrete implementation created in Medhavi.Nexus composition root.
type TransportDataSource =
    {
        /// Get shipments, optionally filtered by transport leg
        GetShipments : string option -> DateOnly -> DateOnly -> PlanContext -> Task<ShipmentView list>
    }

/// All data sources as a single record.
/// Created by Medhavi.Nexus (composition root), injected into projection builders and query services.
/// This is the key decoupling point — Analytics has zero project references to any BC.
type ProjectionDataSources =
    {
        Demand    : DemandDataSource
        Material  : MaterialDataSource
        Capacity  : CapacityDataSource
        Transport : TransportDataSource
    }
