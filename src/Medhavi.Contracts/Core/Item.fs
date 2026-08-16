namespace Medhavi.Contracts.Core

open System.Threading.Tasks
open Medhavi.Contracts

// ---------- Governed Enums ----------
type ItemType =
    | FinishedGood
    | RawMaterial
    | Component
    | Packaging
    | Service
    | TradingGood
    | Other

// ---------- DTO ----------
type Item =
    { Id: string
      EnterpriseBusinessIdentifier: string option
      Name: string
      ItemType: ItemType
      ItemRoles: string list
      UnitOfMeasureId: string
      State: string }

// ---------- Command Payloads ----------
type CreateItemReq =
    { Id: string
      EnterpriseBusinessIdentifier: string option
      Name: string
      ItemType: ItemType // optional per spec
      UnitOfMeasureId: string }

type ActivateItemReq = { Id: string }
type InactivateItemReq = { Id: string; Reason: string }
type RetireItemReq = { Id: string; Reason: string }
type AddItemRoleReq = { Id: string; Role: string }
type RemoveItemRoleReq = { Id: string; Role: string }

// ---------- Business Notifications ----------
type ItemCreatedNotification =
    { Id: string
      Name: string
      ItemType: string // we still send the string representation for notifications
      UnitOfMeasureId: string }

type ItemActivatedNotification = { Id: string }
type ItemInactivatedNotification = { Id: string; Reason: string }
type ItemRetiredNotification = { Id: string; Reason: string }

// ---------- API Record & Query Service ----------
type ItemApi =
    { Create: CreateItemReq -> Task<Result<Item, ApiError>>
      Activate: ActivateItemReq -> Task<Result<Item, ApiError>>
      Inactivate: InactivateItemReq -> Task<Result<Item, ApiError>>
      Retire: RetireItemReq -> Task<Result<Item, ApiError>>
      AddRole: AddItemRoleReq -> Task<Result<Item, ApiError>>
      RemoveRole: RemoveItemRoleReq -> Task<Result<Item, ApiError>> }

type ItemQueries = QueryService<Item, string>
