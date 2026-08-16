// =============================================================================
// Medhavi.Contracts.Core.Demand
// Traceability: SE‑C‑013 Demand contracts
// Contains: DTO, Commands, Notifications, API gateway record, Queries alias
// =============================================================================
namespace Medhavi.Contracts.Core

open System
open System.Threading.Tasks
open Medhavi.Contracts

// ---------- DTO ----------
type DemandOrigin =
    | CustomerOrder
    | Forecast
    | ProductionRequirement
    | Transfer
    | Other

type Demand =
    { Id: string
      ItemId: string
      Quantity: decimal
      LocationId: string
      EarliestAcceptable: DateTimeOffset option
      Preferred: DateTimeOffset option
      LatestAcceptable: DateTimeOffset
      Origin: DemandOrigin
      CustomerId: string option
      ParentDemandId: string option
      State: string }

// ---------- Command Payloads ----------
type CreateDemandReq =
    { Id: string
      ItemId: string
      Quantity: decimal
      LocationId: string
      EarliestAcceptable: DateTimeOffset option
      Preferred: DateTimeOffset option
      LatestAcceptable: DateTimeOffset
      Origin: DemandOrigin
      BusinessTime: DateTimeOffset
      CustomerId: string option
      ParentDemandId: string option }

type SatisfyDemandReq = { Id: string }
type CancelDemandReq = { Id: string; Reason: string }
type ExpireDemandReq = { Id: string }

// ---------- Business Notifications ----------
type DemandCreatedNotification =
    { Id: string
      ItemId: string
      Quantity: decimal
      LocationId: string }

type DemandSatisfiedNotification = { Id: string }
type DemandCancelledNotification = { Id: string; Reason: string }

// ---------- API Record & Query Service ----------
type DemandApi =
    { Create: CreateDemandReq -> Task<Result<Demand, ApiError>>
      Satisfy: SatisfyDemandReq -> Task<Result<Demand, ApiError>>
      Cancel: CancelDemandReq -> Task<Result<Demand, ApiError>>
      Expire: ExpireDemandReq -> Task<Result<Demand, ApiError>> }

type DemandQueries = QueryService<Demand, string>
