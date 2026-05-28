module Medhavi.Domain.Material.SupplyOrder

open System
open System.Text.Json.Serialization
open Medhavi.SharedKernel
open Medhavi.Common

[<JsonFSharpConverter>]
type SupplyOrderType =
    | WorkOrder
    | PurchaseOrder
    | TransportOrder

[<JsonFSharpConverter>]
type SupplyOrderState =
    | Created
    | Planned
    | Confirmed
    | Released
    | InProgress
    | Completed
    | Cancelled

[<JsonFSharpConverter>]
type SupplyOrderId = private SupplyOrderId of string

module SupplyOrderId =
    let create = IdsFactory.createExplicitId SupplyOrderId "SupplyOrderId"
    let value (SupplyOrderId id) = id

type SupplyOrder =
    { Id: SupplyOrderId
      OrderType: SupplyOrderType
      SkuId: SkuId
      StockingPointId: StockingPointId
      Quantity: Quantity
      UnitOfMeasure: UomId
      State: SupplyOrderState
      RoutingId: RoutingId option
      SupplierId: string option
      IsFirm: bool
      IsExpedited: bool
      IsLocked: bool
      UsesLeadTimeQuantity: bool
      RequiredDeliveryDate: Timestamp option
      CreatedDate: Timestamp
      ModifiedDate: Timestamp }

// Commands
type CreateSupplyOrderCmd =
    { Id: string
      OrderType: SupplyOrderType
      SkuId: SkuId
      StockingPointId: StockingPointId
      Quantity: Quantity
      UnitOfMeasure: UomId
      RoutingId: RoutingId option
      SupplierId: string option
      IsFirm: bool
      IsExpedited: bool
      IsLocked: bool
      UsesLeadTimeQuantity: bool
      RequiredDeliveryDate: Timestamp option
      CreatedDate: Timestamp }

type PlanSupplyOrderCmd =
    { Id: SupplyOrderId
      PlannedDeliveryDate: Timestamp }

type ConfirmSupplyOrderCmd =
    { Id: SupplyOrderId
      ConfirmedDate: Timestamp }

type ReleaseSupplyOrderCmd =
    { Id: SupplyOrderId
      ReleasedDate: Timestamp }

type StartSupplyOrderCmd =
    { Id: SupplyOrderId
      StartedDate: Timestamp }

type CompleteSupplyOrderCmd =
    { Id: SupplyOrderId
      CompletedDate: Timestamp }

type PartialCompleteSupplyOrderCmd =
    { Id: SupplyOrderId
      CompletedQuantity: Quantity
      CompletedDate: Timestamp }

type CancelSupplyOrderCmd =
    { Id: SupplyOrderId
      CancelledDate: Timestamp }

type LockSupplyOrderCmd =
    { Id: SupplyOrderId
      Locked: bool
      ModifiedDate: Timestamp }

type UpdateSupplyOrderPriorityCmd =
    { Id: SupplyOrderId
      ModifiedDate: Timestamp }

type SupplyOrderCommand =
    | CreateSupplyOrder of CreateSupplyOrderCmd
    | StartSupplyOrder of StartSupplyOrderCmd
    | PartialCompleteSupplyOrder of PartialCompleteSupplyOrderCmd
    | CompleteSupplyOrder of CompleteSupplyOrderCmd
    | PlanSupplyOrder of PlanSupplyOrderCmd
    | ConfirmSupplyOrder of ConfirmSupplyOrderCmd
    | ReleaseSupplyOrder of ReleaseSupplyOrderCmd
    | CancelSupplyOrder of CancelSupplyOrderCmd
    | LockSupplyOrder of LockSupplyOrderCmd
    | UpdateSupplyOrderPriority of UpdateSupplyOrderPriorityCmd

// Events
type SupplyOrderCreatedEvt = SupplyOrder

type SupplyOrderPlannedEvt =
    { Id: SupplyOrderId
      PlannedDeliveryDate: Timestamp }

type SupplyOrderConfirmedEvt =
    { Id: SupplyOrderId
      ConfirmedDate: Timestamp }

type SupplyOrderReleasedEvt =
    { Id: SupplyOrderId
      ReleasedDate: Timestamp }

type SupplyOrderStartedEvt =
    { Id: SupplyOrderId
      StartedDate: Timestamp }

type SupplyOrderCompletedEvt =
    { Id: SupplyOrderId
      CompletedDate: Timestamp }

type SupplyOrderPartiallyCompletedEvt =
    { Id: SupplyOrderId
      CompletedQuantity: Quantity
      CompletedDate: Timestamp }

type SupplyOrderCancelledEvt =
    { Id: SupplyOrderId
      CancelledDate: Timestamp }

type SupplyOrderLockedEvt =
    { Id: SupplyOrderId
      Locked: bool
      ModifiedDate: Timestamp }

type SupplyOrderPriorityUpdatedEvt =
    { Id: SupplyOrderId
      ModifiedDate: Timestamp }

type SupplyOrderEvent =
    | SupplyOrderCreated of SupplyOrderCreatedEvt
    | SupplyOrderPlanned of SupplyOrderPlannedEvt
    | SupplyOrderConfirmed of SupplyOrderConfirmedEvt
    | SupplyOrderReleased of SupplyOrderReleasedEvt
    | SupplyOrderStarted of SupplyOrderStartedEvt
    | SupplyOrderCompleted of SupplyOrderCompletedEvt
    | SupplyOrderPartiallyCompleted of SupplyOrderPartiallyCompletedEvt
    | SupplyOrderCancelled of SupplyOrderCancelledEvt
    | SupplyOrderLocked of SupplyOrderLockedEvt
    | SupplyOrderPriorityUpdated of SupplyOrderPriorityUpdatedEvt

// Signatures
type DecideSupplyOrder = Decide<SupplyOrder, SupplyOrderCommand, SupplyOrderEvent>
type EvolveSupplyOrder = Evolve<SupplyOrder, SupplyOrderEvent>

let applyCreated (evt: SupplyOrderCreatedEvt) : SupplyOrder =
    { Id = evt.Id
      OrderType = evt.OrderType
      SkuId = evt.SkuId
      StockingPointId = evt.StockingPointId
      Quantity = evt.Quantity
      UnitOfMeasure = evt.UnitOfMeasure
      State = SupplyOrderState.Created
      RoutingId = evt.RoutingId
      SupplierId = evt.SupplierId
      IsFirm = evt.IsFirm
      IsExpedited = evt.IsExpedited
      IsLocked = evt.IsLocked
      UsesLeadTimeQuantity = evt.UsesLeadTimeQuantity
      RequiredDeliveryDate = evt.RequiredDeliveryDate
      CreatedDate = evt.CreatedDate
      ModifiedDate = evt.CreatedDate }

let applyPlanned (evt: SupplyOrderPlannedEvt) (state: SupplyOrder) =
    { state with
        State = SupplyOrderState.Planned
        ModifiedDate = evt.PlannedDeliveryDate }

let applyConfirmed (evt: SupplyOrderConfirmedEvt) (state: SupplyOrder) =
    { state with
        State = SupplyOrderState.Confirmed
        ModifiedDate = evt.ConfirmedDate }

let applyReleased (evt: SupplyOrderReleasedEvt) (state: SupplyOrder) =
    { state with
        State = SupplyOrderState.Released
        ModifiedDate = evt.ReleasedDate }

let applyStarted (evt: SupplyOrderStartedEvt) (state: SupplyOrder) =
    { state with
        State = SupplyOrderState.InProgress
        ModifiedDate = evt.StartedDate }

let applyCompleted (evt: SupplyOrderCompletedEvt) (state: SupplyOrder) =
    { state with
        State = SupplyOrderState.Completed
        ModifiedDate = evt.CompletedDate }

let applyPartiallyCompleted (evt: SupplyOrderPartiallyCompletedEvt) (state: SupplyOrder) =
    { state with
        ModifiedDate = evt.CompletedDate
    // State remains InProgress for partial completion
    }

let applyCancelled (evt: SupplyOrderCancelledEvt) (state: SupplyOrder) =
    { state with
        State = SupplyOrderState.Cancelled
        ModifiedDate = evt.CancelledDate }

let applyLocked (evt: SupplyOrderLockedEvt) (state: SupplyOrder) =
    { state with
        IsLocked = evt.Locked
        ModifiedDate = evt.ModifiedDate }

let applyPriorityUpdated (evt: SupplyOrderPriorityUpdatedEvt) (state: SupplyOrder) =
    { state with
        ModifiedDate = evt.ModifiedDate }

let evolve (state: SupplyOrder option) (event: SupplyOrderEvent) : SupplyOrder option =
    match event, state with
    | SupplyOrderCreated e, None -> Some(applyCreated e)
    | SupplyOrderPlanned e, Some s -> Some(applyPlanned e s)
    | SupplyOrderConfirmed e, Some s -> Some(applyConfirmed e s)
    | SupplyOrderReleased e, Some s -> Some(applyReleased e s)
    | SupplyOrderStarted e, Some s -> Some(applyStarted e s)
    | SupplyOrderCompleted e, Some s -> Some(applyCompleted e s)
    | SupplyOrderPartiallyCompleted e, Some s -> Some(applyPartiallyCompleted e s)
    | SupplyOrderCancelled e, Some s -> Some(applyCancelled e s)
    | SupplyOrderLocked e, Some s -> Some(applyLocked e s)
    | SupplyOrderPriorityUpdated e, Some s -> Some(applyPriorityUpdated e s)
    | SupplyOrderCreated _, Some _ -> state
    | _, current -> current

let validateCreate (cmd: CreateSupplyOrderCmd) = SupplyOrderId.create cmd.Id

let decide: DecideSupplyOrder =
    fun cmd stateOpt ->
        match cmd, stateOpt with
        | CreateSupplyOrder c, None ->
            match validateCreate c with
            | Error e -> Error(DomainError.validation $"Failed to create supply order {c.Id}")
            | Ok sid ->
                let order =
                    { Id = sid
                      OrderType = c.OrderType
                      SkuId = c.SkuId
                      StockingPointId = c.StockingPointId
                      Quantity = c.Quantity
                      UnitOfMeasure = c.UnitOfMeasure
                      State = SupplyOrderState.Created
                      RoutingId = c.RoutingId
                      SupplierId = c.SupplierId
                      IsFirm = c.IsFirm
                      IsExpedited = c.IsExpedited
                      IsLocked = c.IsLocked
                      UsesLeadTimeQuantity = c.UsesLeadTimeQuantity
                      RequiredDeliveryDate = c.RequiredDeliveryDate
                      CreatedDate = c.CreatedDate
                      ModifiedDate = c.CreatedDate }

                let evt: SupplyOrderCreatedEvt = order

                Ok
                    { NewState = order
                      Events = [ SupplyOrderCreated evt ] }

        | StartSupplyOrder c, Some state ->
            let evt =
                SupplyOrderStarted
                    { Id = c.Id
                      StartedDate = c.StartedDate }

            evolve stateOpt evt
            |> Option.map (fun supplier ->
                Ok
                    { NewState = supplier
                      Events = [ evt ] })
            |> Option.defaultWith (fun () -> Error(DomainError.validation $"Failed to start supply order {c.Id}"))
        | PartialCompleteSupplyOrder cmd, Some s ->
            let evt =
                SupplyOrderPartiallyCompleted
                    { Id = cmd.Id
                      CompletedQuantity = cmd.CompletedQuantity
                      CompletedDate = cmd.CompletedDate }

            evolve stateOpt evt
            |> Option.map (fun supplier ->
                Ok
                    { NewState = supplier
                      Events = [ evt ] })
            |> Option.defaultWith (fun () ->
                Error(DomainError.validation $"Failed to partially complete supply order {cmd.Id}"))
        | CompleteSupplyOrder cmd, Some s ->
            let evt =
                SupplyOrderCompleted
                    { Id = cmd.Id
                      CompletedDate = cmd.CompletedDate }

            evolve stateOpt evt
            |> Option.map (fun supplier ->
                Ok
                    { NewState = supplier
                      Events = [ evt ] })
            |> Option.defaultWith (fun () ->
                Error(DomainError.validation $"Failed to mark supply order {cmd.Id} completed"))
        | PlanSupplyOrder cmd, Some s ->
            let evt =
                SupplyOrderPlanned
                    { Id = cmd.Id
                      PlannedDeliveryDate = cmd.PlannedDeliveryDate }

            evolve stateOpt evt
            |> Option.map (fun supplier ->
                Ok
                    { NewState = supplier
                      Events = [ evt ] })
            |> Option.defaultWith (fun () -> Error(DomainError.validation $"Failed to plan Supply order {cmd.Id}"))
        | ConfirmSupplyOrder cmd, Some s ->
            let evt =
                SupplyOrderConfirmed
                    { Id = cmd.Id
                      ConfirmedDate = cmd.ConfirmedDate }

            evolve stateOpt evt
            |> Option.map (fun supplier ->
                Ok
                    { NewState = supplier
                      Events = [ evt ] })
            |> Option.defaultWith (fun () -> Error(DomainError.validation $"Failed to confirm Supply order {cmd.Id}"))
        | ReleaseSupplyOrder cmd, Some s ->
            let evt =
                SupplyOrderReleased
                    { Id = cmd.Id
                      ReleasedDate = cmd.ReleasedDate }

            evolve stateOpt evt
            |> Option.map (fun supplier ->
                Ok
                    { NewState = supplier
                      Events = [ evt ] })
            |> Option.defaultWith (fun () -> Error(DomainError.validation $"Failed to release Supply order {cmd.Id}"))
        | CancelSupplyOrder cmd, Some s ->
            let evt =
                SupplyOrderCancelled
                    { Id = cmd.Id
                      CancelledDate = cmd.CancelledDate }

            evolve stateOpt evt
            |> Option.map (fun supplier ->
                Ok
                    { NewState = supplier
                      Events = [ evt ] })
            |> Option.defaultWith (fun () -> Error(DomainError.validation $"Failed to cancel Supply order {cmd.Id}"))
        | LockSupplyOrder cmd, Some s ->
            let evt =
                SupplyOrderLocked
                    { Id = cmd.Id
                      Locked = cmd.Locked
                      ModifiedDate = cmd.ModifiedDate }

            evolve stateOpt evt
            |> Option.map (fun supplier ->
                Ok
                    { NewState = supplier
                      Events = [ evt ] })
            |> Option.defaultWith (fun () -> Error(DomainError.validation $"Failed to lock Supply order {cmd.Id}"))
        | UpdateSupplyOrderPriority cmd, Some s ->
            let evt =
                SupplyOrderPriorityUpdated
                    { Id = cmd.Id
                      ModifiedDate = cmd.ModifiedDate }

            evolve stateOpt evt
            |> Option.map (fun supplier ->
                Ok
                    { NewState = supplier
                      Events = [ evt ] })
            |> Option.defaultWith (fun () ->
                Error(DomainError.validation $"Failed to update priority for Supply order {cmd.Id}"))

        | _ -> Error(DomainError.validation $"Invalid command for supply order")
