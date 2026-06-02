namespace Medhavi.Promise

open System
open Medhavi.SharedKernel
open Medhavi.Promise.PromiseTypes
open Medhavi.Transport

/// Material provider interface
type MaterialProvider =
    { GetSnapshot: SkuId * StockingPointId * DateTimeOffset -> Async<Result<MaterialSnapshot, ProviderError>>
      GetSupplierOptions:
          SkuId * StockingPointId * decimal * DateTimeOffset -> Async<Result<SupplierOption list, ProviderError>> }

/// Capacity provider interface - aligns with Capacity CTP interface from PDD
type CapacityProvider =
    { CheckCapacity:
        SkuId * decimal * DateTimeOffset -> Async<Result<CapacityCheckResult, ProviderError>> }

/// Transport provider interface - aligns with Transport ATP interface from PDD
type TransportProvider =
    { GetOptions:
        string * string * DateTimeOffset -> Async<Result<Itinerary list, ProviderError>> }

/// Routing provider interface
type RoutingProvider =
    { Select: SkuId * StockingPointId -> Async<Result<RoutingSelection, ProviderError>> }

/// Reservation provider interface
type ReservationProvider =
    { CreateTentative: ReservationRequest list -> Async<Result<ReservationId list, ProviderError>>
      Confirm: ReservationId list -> Async<Result<unit, ProviderError>>
      Release: ReservationId list -> Async<Result<unit, ProviderError>> }

/// FX provider interface
type FxProvider =
    { GetRate: string * string * DateTimeOffset -> Async<Result<decimal option, ProviderError>> }

/// Tenant provider interface
type TenantProvider =
    { GetTenant: unit -> string * TimeZoneInfo * string option }
