namespace Medhavi.Contracts.Promise

open System
open System.Threading.Tasks
open Medhavi.Contracts

/// Entity representing machine hours/shifts held at a plant
type CapacityReservationSlotDto =
    { SlotId: string
      PhysicalResourceId: string
      ReservedHours: decimal
      WorkCenterShiftDate: DateTimeOffset }

/// Entity representing physical inventory pallets/lots held at a warehouse
type InventoryReservationHoldDto =
    { HoldId: string
      Location: string
      Item: string
      ReservedQuantity: decimal
      BatchOrLotNumber: string option }

/// SE-P-030 Temporary Reservation Data Transfer Object
type TemporaryReservationDto =
    { ReservationToken: string
      OrderLineId: string
      CustomerId: string
      Item: string
      Location: string
      RequestedQuantity: decimal
      HeldQuantity: decimal
      LeaseCreationTime: DateTimeOffset
      LeaseExpirationTime: DateTimeOffset
      CapacitySlots: CapacityReservationSlotDto list
      InventoryHolds: InventoryReservationHoldDto list
      LifecycleState: string
      DecisionTraceability: string option }

/// External request to place an ephemeral reservation hold during cart checkout
type CreateTemporaryHoldReq =
    { ReservationToken: string
      OrderLineId: string
      CustomerId: string
      Item: string
      Location: string
      Quantity: decimal
      LeaseDurationMinutes: int
      CreationTime: DateTimeOffset }

/// External request to confirm a hold into a binding commitment upon payment
type ConfirmHoldToCommitmentReq =
    { ReservationToken: string
      ConfirmationTime: DateTimeOffset }

/// External request to release a hold when user cancels cart
type ReleaseTemporaryHoldReq =
    { ReservationToken: string
      ReleaseReason: string
      ReleaseTime: DateTimeOffset }

/// External request to expire a hold via TTL reaper
type ExpireTemporaryHoldReq =
    { ReservationToken: string
      HeartbeatTime: DateTimeOffset }

// ---------- Business Notifications ----------

/// BN-P-008: Temporary Reservation Confirmed Notification
type TemporaryReservationConfirmedNotification =
    { ReservationToken: string
      OrderLineId: string
      Item: string
      HeldQuantity: decimal
      LeaseExpirationTime: DateTimeOffset
      NotificationTime: DateTimeOffset }

/// BN-P-009: Reservation Expired & Released Notification
type ReservationExpiredAndReleasedNotification =
    { ReservationToken: string
      OrderLineId: string
      Item: string
      ReleasedQuantity: decimal
      NotificationTime: DateTimeOffset }

/// Public API for Temporary Reservations (CA-P-003)
type ReservationApi =
    { CreateHold: CreateTemporaryHoldReq -> Task<Result<TemporaryReservationDto, ApiError>>
      ConfirmHold: ConfirmHoldToCommitmentReq -> Task<Result<TemporaryReservationDto, ApiError>>
      ReleaseHold: ReleaseTemporaryHoldReq -> Task<Result<TemporaryReservationDto, ApiError>>
      ExpireHold: ExpireTemporaryHoldReq -> Task<Result<TemporaryReservationDto, ApiError>> }

/// Query service alias for Temporary Reservations
type ReservationQueries = QueryService<TemporaryReservationDto, string>

