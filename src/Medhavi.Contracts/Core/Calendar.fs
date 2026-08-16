// =============================================================================
// Medhavi.Contracts.Core.Calendar
// Traceability: SE‑C‑033 Calendar contracts
// Contains: DTO, Commands, Notifications, API gateway record, Queries alias
// =============================================================================
namespace Medhavi.Contracts.Core

open System.Threading.Tasks
open Medhavi.Contracts

// ---------- DTO ----------
// Traceability: SE-C-033 Calendar read-side DTO
type Calendar =
    { Id: string
      Name: string
      TimeZoneId: string
      IntervalsCount: int
      State: string }

type CalendarIntervalDto =
    { StartIso: string
      FinishIso: string }

// ---------- Command Payloads ----------
type CreateCalendarReq =
    { Id: string
      Name: string
      TimeZoneId: string
      Intervals: CalendarIntervalDto list }

type SupersedeCalendarReq =
    { Id: string
      NewCalendarId: string
      Reason: string }

// ---------- Business Notifications ----------
// Traceability: BN-C-050..BN-C-051 Calendar notifications
type CalendarCreatedNotification =
    { Id: string
      Name: string
      TimeZoneId: string }

type CalendarSupersededNotification =
    { Id: string
      NewCalendarId: string
      Reason: string }

// ---------- API Record & Query Service ----------
type CalendarApi =
    { Create: CreateCalendarReq -> Task<Result<Calendar, ApiError>>
      Supersede: SupersedeCalendarReq -> Task<Result<Calendar, ApiError>> }

type CalendarQueries = QueryService<Calendar, string>
