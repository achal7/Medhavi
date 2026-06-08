namespace Medhavi.Analytics.PlanningHorizon

open System

// =============================================================================
// Plane 4 — Transport Projection
// =============================================================================

/// Shipment lifecycle status
type ShipmentStatus =
    | PlannedShipment
    | BookedShipment
    | InTransitShipment
    | DeliveredShipment

/// A single shipment on a transport leg — drill-down detail
type ShipmentView =
    {
        ShipmentId     : string
        TransportLegId : string
        SkuId          : string
        SkuCode        : string
        FromPlantId    : string
        ToPlantId      : string
        Quantity       : decimal
        DepartureDate  : DateOnly
        ArrivalDate    : DateOnly
        TransitDays    : decimal
        Mode           : string        // "Road" | "Rail" | "Sea" | "Air"
        CarrierId      : string option
        Status         : ShipmentStatus
        IsFirm         : bool
    }

/// Aggregated transport view for a single PlanningPeriod on a transport leg.
/// Shown as one cell in the "Transport" row of the planning board.
type TransportPeriodView =
    {
        Period            : PlanningPeriod
        TransportLegId    : string
        FromPlantId       : string
        ToPlantId         : string
        TotalOutboundQty  : decimal
        TotalInboundQty   : decimal
        LegCapacity       : decimal option
        CapacityUtilizPct : decimal option
        EstimatedCost     : decimal option
        Shipments         : ShipmentView list
    }
