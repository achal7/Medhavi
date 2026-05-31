namespace Medhavi.Transport

open System
open Medhavi.SharedKernel

/// ID for a transport reservation (tentative slot on a leg)
type TransportReservationId = private TransportReservationId of string

module TransportReservationId =
    let create (s: string) =
        if String.IsNullOrWhiteSpace s then
            Error(DomainError.validation "TransportReservationId cannot be empty")
        else
            Ok(TransportReservationId s)
    let value (TransportReservationId id) = id
    let generate () =
        TransportReservationId(Guid.NewGuid().ToString("N"))

/// ID for an itinerary (a specific multi-hop path)
type ItineraryId = private ItineraryId of string

module ItineraryId =
    let create (s: string) =
        if String.IsNullOrWhiteSpace s then
            Error(DomainError.validation "ItineraryId cannot be empty")
        else
            Ok(ItineraryId s)
    let value (ItineraryId id) = id
    let generate () =
        ItineraryId(Guid.NewGuid().ToString("N"))

/// Transport reservation status
type TransportReservationStatus =
    | Tentative
    | Confirmed
    | Released
    | Expired

/// A transport leg reference (lightweight, used inside pathfinding)
type TransportLegRef =
    { LegId: string
      Origin: string
      Destination: string
      Mode: string
      LeadTimeMinutes: decimal
      Capacity: decimal option
      CapacityUnit: string option
      Reliability: decimal option
      CO2PerUnit: decimal option
      FixedCost: decimal
      VariableCostPerUnit: decimal option
      Status: bool }

/// A single hop in a multi-hop route
type ItineraryHop =
    { LegId: string
      Origin: string
      Destination: string
      Mode: string
      LeadTimeMinutes: decimal
      DepartureDateOffset: decimal // minutes from route start
      ArrivalDateOffset: decimal   // minutes from route start
      FixedCost: decimal
      VariableCostPerUnit: decimal option }

/// A complete transport route (may be single-hop or multi-hop)
type Itinerary =
    { Id: ItineraryId
      SkuId: string option
      FromNode: string
      ToNode: string
      Hops: ItineraryHop list
      TotalLeadTimeMinutes: decimal
      TotalFixedCost: decimal
      TotalVariableCostPerUnit: decimal option
      TotalCO2: decimal option
      TotalReliability: decimal  // product of hop reliabilities
      HopCount: int }

/// Request to find transport options
type GetTransportOptionsReq =
    { FromNode: string
      ToNode: string
      SkuId: string option
      RequiredQuantity: decimal option
      NeedByDate: DateTimeOffset
      MaxHops: int option
      MaxItineraries: int option }

/// Result of transport option finding
type TransportOption =
    { Itinerary: Itinerary
      EarliestDeparture: DateTimeOffset
      EarliestArrival: DateTimeOffset
      EstimatedCost: decimal
      CostBreakdown: string
      ReliabilityScore: decimal
      CO2Estimate: decimal option
      IsPreferred: bool }

/// Transport caching key
type TransportCacheKey =
    { FromNode: string
      ToNode: string
      SkuId: string option }
