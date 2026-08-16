// =============================================================================
// Medhavi.Contracts.Core.Customer
// Traceability: SE‑C‑003 Customer contracts
// Contains: DTO, Commands, Notifications, API gateway record, Queries alias
// =============================================================================
namespace Medhavi.Contracts.Core

open System.Threading.Tasks
open Medhavi.Contracts

// ---------- Governed Enums ----------
type CustomerClass =
    | A
    | B
    | C
    | D

// ---------- DTO ----------
type Customer =
    { Id: string
      Name: string
      CustomerType: CustomerClass
      State: string }

// ---------- Command Payloads ----------
type CreateCustomerReq =
    { Id: string
      Name: string
      CustomerType: CustomerClass }

type ActivateCustomerReq = { Id: string }

type InactivateCustomerReq = { Id: string; Reason: string }

type RetireCustomerReq = { Id: string; Reason: string }

// ---------- Business Notifications ----------
type CustomerCreatedNotification =
    { Id: string
      Name: string
      CustomerType: string }

type CustomerActivatedNotification = { Id: string }

type CustomerInactivatedNotification = { Id: string; Reason: string }

type CustomerRetiredNotification = { Id: string; Reason: string }

// ---------- API Record & Query Service ----------
type CustomerApi =
    { Create: CreateCustomerReq -> Task<Result<Customer, ApiError>>
      Activate: ActivateCustomerReq -> Task<Result<Customer, ApiError>>
      Inactivate: InactivateCustomerReq -> Task<Result<Customer, ApiError>>
      Retire: RetireCustomerReq -> Task<Result<Customer, ApiError>> }

type CustomerQueries = QueryService<Customer, string>
