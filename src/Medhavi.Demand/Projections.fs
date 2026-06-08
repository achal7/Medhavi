namespace Medhavi.Demand

open System
open System.Threading.Tasks

// =============================================================================
// Demand Query Service — BC-scoped read model
// =============================================================================

/// Read-model query service for the Demand BC.
/// Medhavi.Nexus composition root wires a concrete implementation into
/// Medhavi.Analytics.PlanningHorizon.DemandDataSource.
type DemandQueryService =
    {
        /// Get all demand lines for a plant within a date range (by RequestedDeliveryDate)
        GetDemandLines: string -> DateOnly -> DateOnly -> Task<DemandLine list>
        /// Get all demand lines belonging to a specific order
        GetByOrderId: string -> Task<DemandLine list>
        /// Get all open (unfulfilled) demand for a SKU at a specific stocking point
        GetOpenDemand: string -> string -> Task<DemandLine list>
        /// Get demand lines filtered by fulfillment status within a plant
        GetByStatus: DemandStatus -> string -> Task<DemandLine list>
    }
