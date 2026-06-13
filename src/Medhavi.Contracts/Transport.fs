module Medhavi.Contracts.Transport

open System
open Medhavi.Contracts.Analytics

/// Shipment lifecycle status
type ShipmentStatus =
    | PlannedShipment
    | BookedShipment
    | InTransitShipment
    | DeliveredShipment

/// A single shipment on a transport leg — drill-down detail
type ShipmentView =
    { ShipmentId: string
      TransportLegId: string
      SkuId: string
      SkuCode: string
      FromPlantId: string
      ToPlantId: string
      Quantity: decimal
      DepartureDate: DateOnly
      ArrivalDate: DateOnly
      TransitDays: decimal
      Mode: string // "Road" | "Rail" | "Sea" | "Air"
      CarrierId: string option
      Status: ShipmentStatus
      IsFirm: bool }

/// Aggregated transport view for a single PlanningPeriod on a transport leg.
type TransportPeriodView =
    { Period: PlanningPeriod
      TransportLegId: string
      FromPlantId: string
      ToPlantId: string
      TotalOutboundQty: decimal
      TotalInboundQty: decimal
      LegCapacity: decimal option
      CapacityUtilizPct: decimal option
      EstimatedCost: decimal option
      Shipments: ShipmentView list }


type TransportLegDefineReq =
    { Id: string
      Origin: string
      Destination: string
      Mode: string
      Schedule: string
      LeadTimeMinutes: decimal
      Capacity: decimal option
      CapacityUnit: string option
      CutoffMinutes: decimal option
      Constraints: string list
      Reliability: decimal option
      CO2PerUnit: decimal option
      EffectiveStart: DateTimeOffset
      EffectiveEnd: DateTimeOffset option
      Created: DateTimeOffset }

type TransportLegUpdateReq =
    { Id: string
      Mode: string option
      Schedule: string option
      LeadTimeMinutes: decimal option
      Capacity: decimal option
      CapacityUnit: string option
      CutoffMinutes: decimal option
      Constraints: string list option
      Reliability: decimal option
      CO2PerUnit: decimal option
      EffectiveEnd: DateTimeOffset option
      Modified: DateTimeOffset }

type TransportLegDeactivateReq =
    { Id: string
      DeactivatedAt: DateTimeOffset }
