// =============================================================================
// Medhavi.Contracts.Core.Supplier
// Traceability: SE‑C‑004 Supplier contracts
// Contains: DTO, Commands, Notifications, API gateway record, Queries alias
// =============================================================================
namespace Medhavi.Contracts.Core

open System.Threading.Tasks
open Medhavi.Contracts

// ---------- DTO ----------
type Supplier =
    { Id: string
      Name: string
      State: string }

// ---------- Command Payloads ----------
type CreateSupplierReq = { Id: string; Name: string }

type ActivateSupplierReq = { Id: string }
type InactivateSupplierReq = { Id: string; Reason: string }
type RetireSupplierReq = { Id: string; Reason: string }

// ---------- Business Notifications ----------
type SupplierCreatedNotification = { Id: string; Name: string }

type SupplierActivatedNotification = { Id: string }
type SupplierInactivatedNotification = { Id: string; Reason: string }
type SupplierRetiredNotification = { Id: string; Reason: string }

// ---------- API Record & Query Service ----------
type SupplierApi =
    { Create: CreateSupplierReq -> Task<Result<Supplier, ApiError>>
      Activate: ActivateSupplierReq -> Task<Result<Supplier, ApiError>>
      Inactivate: InactivateSupplierReq -> Task<Result<Supplier, ApiError>>
      Retire: RetireSupplierReq -> Task<Result<Supplier, ApiError>> }

type SupplierQueries = QueryService<Supplier, string>
