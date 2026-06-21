module Medhavi.Contracts.MasterData.Transport

open System
open System.Threading.Tasks
open Medhavi.Contracts

type TransportLeg =
    { Id: string
      Origin: string
      Destination: string
      Mode: string
      LeadTimeMinutes: decimal
      Capacity: decimal option
      CapacityUnit: string option
      Status: bool }

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

/// A single hop in a multi-hop route
type ItineraryHop =
    { LegId: string
      Origin: string
      Destination: string
      Mode: string
      LeadTimeMinutes: decimal
      DepartureDateOffset: decimal // minutes from route start
      ArrivalDateOffset: decimal // minutes from route start
      FixedCost: decimal
      VariableCostPerUnit: decimal option }

/// A complete transport route (may be single-hop or multi-hop)
type Itinerary =
    { Id: string
      SkuId: string option
      FromNode: string
      ToNode: string
      Hops: ItineraryHop list
      TotalLeadTimeMinutes: decimal
      TotalFixedCost: decimal
      TotalVariableCostPerUnit: decimal option
      TotalCO2: decimal option
      TotalReliability: decimal // product of hop reliabilities
      HopCount: int }

type TransportLegApi =
    { Define: TransportLegDefineReq -> Task<Result<TransportLeg, ApiError>>
      DefineBulk: TransportLegDefineReq list -> Task<Result<TransportLeg list, ApiError>>
      Update: TransportLegUpdateReq -> Task<Result<TransportLeg, ApiError>>
      Deactivate: TransportLegDeactivateReq -> Task<Result<TransportLeg, ApiError>> }

type TransportLegQueryService = QueryService<TransportLeg, string>
