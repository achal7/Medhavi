module Medhavi.Supply.Domain.SupplyOrderAgg

open System.Text.Json.Serialization
open Medhavi.SharedKernel

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

    override state.ToString() =
        match state with
        | Created -> "Created"
        | Planned -> "Planned"
        | Confirmed -> "Confirmed"
        | Released -> "Released"
        | InProgress -> "InProgress"
        | Completed -> "Completed"
        | Cancelled -> "Cancelled"

    static member FromString str =
        match str with
        | "Created" -> Ok Created
        | "Planned" -> Ok Planned
        | "Confirmed" -> Ok Confirmed
        | "Released" -> Ok Released
        | "InProgress" -> Ok InProgress
        | "Completed" -> Ok Completed
        | "Cancelled" -> Ok Cancelled
        | _ -> Error(DomainError.validation $"Invalid supply order state: '{str}'")

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
      ModifiedDate: Timestamp
      CompletedQuantity: Quantity
      ScrapQuantity: Quantity
      ProcessedFeedbackIds: string list }

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
      ScrapQuantity: Quantity
      CompletedDate: Timestamp
      FeedbackId: string option }

type PartialCompleteSupplyOrderCmd =
    { Id: SupplyOrderId
      CompletedQuantity: Quantity
      ScrapQuantity: Quantity
      CompletedDate: Timestamp
      FeedbackId: string option }

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
      ReleasedDate: Timestamp } // Keep standard field

type SupplyOrderCompletedEvt =
    { Id: SupplyOrderId
      ScrapQuantity: Quantity
      CompletedDate: Timestamp
      FeedbackId: string option }

type SupplyOrderPartiallyCompletedEvt =
    { Id: SupplyOrderId
      CompletedQuantity: Quantity
      ScrapQuantity: Quantity
      CompletedDate: Timestamp
      FeedbackId: string option }

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
      ModifiedDate = evt.CreatedDate
      CompletedQuantity = Quantity.Zero
      ScrapQuantity = Quantity.Zero
      ProcessedFeedbackIds = [] }

let applyPlanned (evt: SupplyOrderPlannedEvt) (state: SupplyOrder) =
    { state with
        State = SupplyOrderState.Planned
        ModifiedDate = evt.PlannedDeliveryDate }

let applyConfirmed (evt: SupplyOrderConfirmedEvt) (state: SupplyOrder) =
    { state with
        State = SupplyOrderState.Confirmed
        IsFirm = true // Confirmed implies firmed
        ModifiedDate = evt.ConfirmedDate }

let applyReleased (evt: SupplyOrderReleasedEvt) (state: SupplyOrder) =
    { state with
        State = SupplyOrderState.Released
        ModifiedDate = evt.ReleasedDate }

let applyStarted (evt: SupplyOrderStartedEvt) (state: SupplyOrder) =
    { state with
        State = SupplyOrderState.InProgress
        ModifiedDate = evt.ReleasedDate } // Use ReleasedDate as event start date field

let applyCompleted (evt: SupplyOrderCompletedEvt) (state: SupplyOrder) =
    let feedbackIds =
        match evt.FeedbackId with
        | Some fid -> fid :: state.ProcessedFeedbackIds
        | None -> state.ProcessedFeedbackIds
    let netScrap = state.ScrapQuantity + evt.ScrapQuantity
    let netCompleted = state.Quantity - netScrap |> max Quantity.Zero
    { state with
        State = SupplyOrderState.Completed
        CompletedQuantity = netCompleted
        ScrapQuantity = netScrap
        ProcessedFeedbackIds = feedbackIds
        ModifiedDate = evt.CompletedDate }

let applyPartiallyCompleted (evt: SupplyOrderPartiallyCompletedEvt) (state: SupplyOrder) =
    let feedbackIds =
        match evt.FeedbackId with
        | Some fid -> fid :: state.ProcessedFeedbackIds
        | None -> state.ProcessedFeedbackIds
    { state with
        State = SupplyOrderState.InProgress // Transition state to InProgress if partial completes
        CompletedQuantity = state.CompletedQuantity + evt.CompletedQuantity
        ScrapQuantity = state.ScrapQuantity + evt.ScrapQuantity
        ProcessedFeedbackIds = feedbackIds
        ModifiedDate = evt.CompletedDate }

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

let isValidTransition (fromState: SupplyOrderState) (toState: SupplyOrderState) : bool =
    match fromState, toState with
    | current, target when current = target -> true

    | Created, Planned -> true
    | Created, Confirmed -> true
    | Created, Cancelled -> true

    | Planned, Confirmed -> true
    | Planned, Cancelled -> true

    | Confirmed, Released -> true
    | Confirmed, Planned -> true
    | Confirmed, Cancelled -> true

    | Released, InProgress -> true
    | Released, Confirmed -> true
    | Released, Cancelled -> true

    | InProgress, Completed -> true
    | InProgress, Cancelled -> true

    | Completed, _ -> false
    | Cancelled, _ -> false

    | _ -> false

let decide: DecideSupplyOrder =
    fun cmd stateOpt ->
        match cmd, stateOpt with
        | CreateSupplyOrder c, None ->
            match validateCreate c with
            | Error _ -> Error(DomainError.validation $"Failed to create supply order {c.Id}")
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
                      ModifiedDate = c.CreatedDate
                      CompletedQuantity = Quantity.Zero
                      ScrapQuantity = Quantity.Zero
                      ProcessedFeedbackIds = [] }

                let evt: SupplyOrderCreatedEvt = order

                Ok
                    { NewState = order
                      Events = [ SupplyOrderCreated evt ] }

        | StartSupplyOrder c, Some state ->
            if not (isValidTransition state.State SupplyOrderState.InProgress) then
                Error(DomainError.validation $"Invalid state transition from {state.State} to InProgress")
            elif state.State = SupplyOrderState.InProgress then
                Ok { NewState = state; Events = [] }
            else
                let evt =
                    SupplyOrderStarted
                        { Id = c.Id
                          ReleasedDate = c.StartedDate }

                match evolve stateOpt evt with
                | Some s -> Ok { NewState = s; Events = [ evt ] }
                | None -> Error(DomainError.validation $"Failed to start supply order {c.Id}")

        | PartialCompleteSupplyOrder cmd, Some state ->
            let alreadyProcessed =
                match cmd.FeedbackId with
                | Some fid -> List.contains fid state.ProcessedFeedbackIds
                | None -> false

            if alreadyProcessed then
                Ok { NewState = state; Events = [] }
            else
                let targetState = SupplyOrderState.InProgress
                if not (isValidTransition state.State targetState) then
                    Error(DomainError.validation $"Invalid state transition from {state.State} to InProgress on partial completion")
                else
                    let evt =
                        SupplyOrderPartiallyCompleted
                            { Id = cmd.Id
                              CompletedQuantity = cmd.CompletedQuantity
                              ScrapQuantity = cmd.ScrapQuantity
                              CompletedDate = cmd.CompletedDate
                              FeedbackId = cmd.FeedbackId }

                    match evolve stateOpt evt with
                    | Some s -> Ok { NewState = s; Events = [ evt ] }
                    | None -> Error(DomainError.validation $"Failed to partially complete supply order {cmd.Id}")

        | CompleteSupplyOrder cmd, Some state ->
            let alreadyProcessed =
                match cmd.FeedbackId with
                | Some fid -> List.contains fid state.ProcessedFeedbackIds
                | None -> false

            if alreadyProcessed then
                Ok { NewState = state; Events = [] }
            elif state.State = SupplyOrderState.Completed then
                Ok { NewState = state; Events = [] }
            else
                let targetState = SupplyOrderState.Completed
                if not (isValidTransition state.State targetState) then
                    Error(DomainError.validation $"Invalid state transition from {state.State} to Completed")
                else
                    let evt =
                        SupplyOrderCompleted
                            { Id = cmd.Id
                              ScrapQuantity = cmd.ScrapQuantity
                              CompletedDate = cmd.CompletedDate
                              FeedbackId = cmd.FeedbackId }

                    match evolve stateOpt evt with
                    | Some s -> Ok { NewState = s; Events = [ evt ] }
                    | None -> Error(DomainError.validation $"Failed to mark supply order {cmd.Id} completed")

        | PlanSupplyOrder cmd, Some state ->
            if not (isValidTransition state.State SupplyOrderState.Planned) then
                Error(DomainError.validation $"Invalid state transition from {state.State} to Planned")
            elif state.State = SupplyOrderState.Planned then
                Ok { NewState = state; Events = [] }
            else
                let evt =
                    SupplyOrderPlanned
                        { Id = cmd.Id
                          PlannedDeliveryDate = cmd.PlannedDeliveryDate }

                match evolve stateOpt evt with
                | Some s -> Ok { NewState = s; Events = [ evt ] }
                | None -> Error(DomainError.validation $"Failed to plan Supply order {cmd.Id}")

        | ConfirmSupplyOrder cmd, Some state ->
            if not (isValidTransition state.State SupplyOrderState.Confirmed) then
                Error(DomainError.validation $"Invalid state transition from {state.State} to Confirmed")
            elif state.State = SupplyOrderState.Confirmed then
                Ok { NewState = state; Events = [] }
            else
                let evt =
                    SupplyOrderConfirmed
                        { Id = cmd.Id
                          ConfirmedDate = cmd.ConfirmedDate }

                match evolve stateOpt evt with
                | Some s -> Ok { NewState = s; Events = [ evt ] }
                | None -> Error(DomainError.validation $"Failed to confirm Supply order {cmd.Id}")

        | ReleaseSupplyOrder cmd, Some state ->
            if not (isValidTransition state.State SupplyOrderState.Released) then
                Error(DomainError.validation $"Invalid state transition from {state.State} to Released")
            elif state.State = SupplyOrderState.Released then
                Ok { NewState = state; Events = [] }
            else
                let evt =
                    SupplyOrderReleased
                        { Id = cmd.Id
                          ReleasedDate = cmd.ReleasedDate }

                match evolve stateOpt evt with
                | Some s -> Ok { NewState = s; Events = [ evt ] }
                | None -> Error(DomainError.validation $"Failed to release Supply order {cmd.Id}")

        | CancelSupplyOrder cmd, Some state ->
            if not (isValidTransition state.State SupplyOrderState.Cancelled) then
                Error(DomainError.validation $"Invalid state transition from {state.State} to Cancelled")
            elif state.State = SupplyOrderState.Cancelled then
                Ok { NewState = state; Events = [] }
            else
                let evt =
                    SupplyOrderCancelled
                        { Id = cmd.Id
                          CancelledDate = cmd.CancelledDate }

                match evolve stateOpt evt with
                | Some s -> Ok { NewState = s; Events = [ evt ] }
                | None -> Error(DomainError.validation $"Failed to cancel Supply order {cmd.Id}")

        | LockSupplyOrder cmd, Some _ ->
            let evt =
                SupplyOrderLocked
                    { Id = cmd.Id
                      Locked = cmd.Locked
                      ModifiedDate = cmd.ModifiedDate }

            match evolve stateOpt evt with
            | Some s -> Ok { NewState = s; Events = [ evt ] }
            | None -> Error(DomainError.validation $"Failed to lock Supply order {cmd.Id}")

        | UpdateSupplyOrderPriority cmd, Some _ ->
            let evt =
                SupplyOrderPriorityUpdated
                    { Id = cmd.Id
                      ModifiedDate = cmd.ModifiedDate }

            match evolve stateOpt evt with
            | Some s -> Ok { NewState = s; Events = [ evt ] }
            | None -> Error(DomainError.validation $"Failed to update priority for Supply order {cmd.Id}")

        | _ -> Error(DomainError.validation $"Invalid command for supply order")
