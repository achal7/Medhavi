// =============================================================================
// Medhavi.Contracts.Core.Location
// Traceability: SE‑C‑002 Location contracts
// Contains: DTO, Commands, Notifications, API gateway record, Queries alias
// =============================================================================
namespace Medhavi.Contracts.Core

open System.Threading.Tasks
open Medhavi.Contracts

// ---------- Governed Enums ----------
type LocationType =
    | Plant
    | DistributionCenter
    | Warehouse
    | Store
    | CustomerSite
    | SupplierSite
    | Port
    | Depot
    | Terminal
    | Other

// ---------- DTO ----------
type Location =
    { Id: string
      Name: string
      LocationType: LocationType
      TimeZoneId: string
      State: string }

// ---------- Command Payloads ----------
type CreateLocationReq =
    { Id: string
      Name: string
      LocationType: LocationType
      TimeZoneId: string }

type ActivateLocationReq = { Id: string }

type InactivateLocationReq = { Id: string; Reason: string }

type CloseLocationReq = { Id: string; Reason: string }

// ---------- Business Notifications ----------
type LocationCreatedNotification =
    { Id: string
      Name: string
      LocationType: string
      TimeZoneId: string }

type LocationActivatedNotification = { Id: string }

type LocationInactivatedNotification = { Id: string; Reason: string }

type LocationClosedNotification = { Id: string; Reason: string }

// ---------- API Record & Query Service ----------
type LocationApi =
    { Create: CreateLocationReq -> Task<Result<Location, ApiError>>
      Activate: ActivateLocationReq -> Task<Result<Location, ApiError>>
      Inactivate: InactivateLocationReq -> Task<Result<Location, ApiError>>
      Close: CloseLocationReq -> Task<Result<Location, ApiError>> }

type LocationQueries = QueryService<Location, string>
