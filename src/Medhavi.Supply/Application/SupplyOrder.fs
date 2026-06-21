module Medhavi.Supply.Application.SupplyOrder

open Medhavi
open Medhavi.Common.Patterns
open Medhavi.Common.Validation
open Medhavi.Contracts.Supply
open Medhavi.Infrastructure.Projections
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Aggregate
open Medhavi.Supply.Domain.SupplyOrderAgg
open System

module ACL =

    let parseOrderType (s: string) : Result<SupplyOrderType, DomainError> =
        match s.Trim().ToLowerInvariant() with
        | "workorder"
        | "work-order"
        | "work_order" -> Ok WorkOrder
        | "purchaseorder"
        | "purchase-order"
        | "purchase_order" -> Ok PurchaseOrder
        | "transportorder"
        | "transport-order"
        | "transport_order" -> Ok TransportOrder
        | other -> Error(DomainError.validation $"Invalid supply order type: '{other}'")

    let toCreateCommand (req: SupplyOrderCreateReq) : Validation<CreateSupplyOrderCmd, DomainError> =
        let make
            (orderType: SupplyOrderType)
            (skuId: SkuId)
            (spId: StockingPointId)
            (qty: Quantity)
            (uomId: UomId)
            (routingId: RoutingId option)
            : CreateSupplyOrderCmd =
            { Id = req.Id
              OrderType = orderType
              SkuId = skuId
              StockingPointId = spId
              Quantity = qty
              UnitOfMeasure = uomId
              RoutingId = routingId
              SupplierId = req.SupplierId
              IsFirm = req.IsFirm
              IsExpedited = req.IsExpedited
              IsLocked = req.IsLocked
              UsesLeadTimeQuantity = req.UsesLeadTimeQuantity
              RequiredDeliveryDate = req.RequiredDeliveryDate |> Option.map Timestamp.create
              CreatedDate = Timestamp.create req.CreatedDate }

        make <!> (parseOrderType req.OrderType |> fromResult)
        <*> (SkuId.create req.SkuId |> fromResult)
        <*> (StockingPointId.create req.StockingPointId |> fromResult)
        <*> (Quantity.create req.Quantity |> fromResult)
        <*> (UomId.create req.UnitOfMeasure |> fromResult)
        <*> (match req.RoutingId with
             | None -> Valid None
             | Some id -> RoutingId.create id |> Result.map Some |> fromResult)

    let toStartCommand (req: SupplyOrderStartReq) : Result<StartSupplyOrderCmd, DomainError> =
        SupplyOrderId.create req.Id
        |> Result.map(fun id ->
            { Id = id
              StartedDate = Timestamp.create req.StartedDate })

    let toPartialCompleteCommand
        (req: SupplyOrderPartialCompleteReq)
        : Validation<PartialCompleteSupplyOrderCmd, DomainError> =
        let make (orderId: SupplyOrderId) (qty: Quantity) (scrapQty: Quantity) : PartialCompleteSupplyOrderCmd =
            { Id = orderId
              CompletedQuantity = qty
              ScrapQuantity = scrapQty
              CompletedDate = Timestamp.create req.CompletedDate
              FeedbackId = req.FeedbackId }

        make <!> (SupplyOrderId.create req.Id |> fromResult)
        <*> (Quantity.create req.CompletedQuantity |> fromResult)
        <*> (Quantity.create req.ScrapQuantity |> fromResult)

    let toCompleteCommand (req: SupplyOrderCompleteReq) : Result<CompleteSupplyOrderCmd, DomainError> =
        match SupplyOrderId.create req.Id, Quantity.create req.ScrapQuantity with
        | Ok id, Ok scrapQty ->
            Ok
                { Id = id
                  ScrapQuantity = scrapQty
                  CompletedDate = Timestamp.create req.CompletedDate
                  FeedbackId = req.FeedbackId }
        | Error e, _ -> Error e
        | _, Error e -> Error e

    let toPlanCommand (req: SupplyOrderPlanReq) : Result<PlanSupplyOrderCmd, DomainError> =
        SupplyOrderId.create req.Id
        |> Result.map(fun id ->
            { Id = id
              PlannedDeliveryDate = Timestamp.create req.PlannedDeliveryDate })

    let toConfirmCommand (req: SupplyOrderConfirmReq) : Result<ConfirmSupplyOrderCmd, DomainError> =
        SupplyOrderId.create req.Id
        |> Result.map(fun id ->
            { Id = id
              ConfirmedDate = Timestamp.create req.ConfirmedDate })

    let toReleaseCommand (req: SupplyOrderReleaseReq) : Result<ReleaseSupplyOrderCmd, DomainError> =
        SupplyOrderId.create req.Id
        |> Result.map(fun id ->
            { Id = id
              ReleasedDate = Timestamp.create req.ReleasedDate })

    let toCancelCommand (req: SupplyOrderCancelReq) : Result<CancelSupplyOrderCmd, DomainError> =
        SupplyOrderId.create req.Id
        |> Result.map(fun id ->
            { Id = id
              CancelledDate = Timestamp.create req.CancelledDate })

    let toLockCommand (req: SupplyOrderLockReq) : Result<LockSupplyOrderCmd, DomainError> =
        SupplyOrderId.create req.Id
        |> Result.map(fun id ->
            { Id = id
              Locked = req.Locked
              ModifiedDate = Timestamp.create req.ModifiedDate })

    let toContract (o: SupplyOrder) : Contracts.Supply.SupplyOrder =
        let orderTypeStr =
            match o.OrderType with
            | WorkOrder -> "WorkOrder"
            | PurchaseOrder -> "PurchaseOrder"
            | TransportOrder -> "TransportOrder"

        let stateStr =
            match o.State with
            | SupplyOrderState.Created -> "Created"
            | SupplyOrderState.Planned -> "Planned"
            | SupplyOrderState.Confirmed -> "Confirmed"
            | SupplyOrderState.Released -> "Released"
            | SupplyOrderState.InProgress -> "InProgress"
            | SupplyOrderState.Completed -> "Completed"
            | SupplyOrderState.Cancelled -> "Cancelled"

        { Id = SupplyOrderId.value o.Id
          OrderType = orderTypeStr
          SkuId = SkuId.value o.SkuId
          StockingPointId = StockingPointId.value o.StockingPointId
          Quantity = Quantity.value o.Quantity
          UnitOfMeasure = UomId.value o.UnitOfMeasure
          State = stateStr
          RoutingId = o.RoutingId |> Option.map RoutingId.value
          SupplierId = o.SupplierId
          IsFirm = o.IsFirm
          IsExpedited = o.IsExpedited
          IsLocked = o.IsLocked
          UsesLeadTimeQuantity = o.UsesLeadTimeQuantity
          RequiredDeliveryDate = o.RequiredDeliveryDate |> Option.map Timestamp.value
          CreatedDate = Timestamp.value o.CreatedDate
          ModifiedDate = Timestamp.value o.ModifiedDate
          CompletedQuantity = Quantity.value o.CompletedQuantity
          ScrapQuantity = Quantity.value o.ScrapQuantity }

type Decision = Decision<SupplyOrder, SupplyOrderEvent>

type SupplyOrderCapabilities =
    { Create: SupplyOrderCreateReq -> TaskResult<Decision, ApplicationError>
      Start: SupplyOrderStartReq -> TaskResult<Decision, ApplicationError>
      PartialComplete: SupplyOrderPartialCompleteReq -> TaskResult<Decision, ApplicationError>
      Complete: SupplyOrderCompleteReq -> TaskResult<Decision, ApplicationError>
      Plan: SupplyOrderPlanReq -> TaskResult<Decision, ApplicationError>
      Confirm: SupplyOrderConfirmReq -> TaskResult<Decision, ApplicationError>
      Release: SupplyOrderReleaseReq -> TaskResult<Decision, ApplicationError>
      Cancel: SupplyOrderCancelReq -> TaskResult<Decision, ApplicationError>
      Lock: SupplyOrderLockReq -> TaskResult<Decision, ApplicationError> }

module Service =
    open Medhavi.Contracts

    let private createIfMissing capabilities (item: SupplyOrderUpdateReq) (existingOpt: Supply.SupplyOrder option) =
        taskResult {
            match existingOpt with
            | Some order -> return order
            | None ->
                let orderType =
                    if item.SupplyOrderId.StartsWith("WO", StringComparison.OrdinalIgnoreCase) then
                        "WorkOrder"
                    elif item.SupplyOrderId.StartsWith("TO", StringComparison.OrdinalIgnoreCase) then
                        "TransportOrder"
                    else
                        "PurchaseOrder"

                let createReq: SupplyOrderCreateReq =
                    { Id = item.SupplyOrderId
                      OrderType = orderType
                      SkuId = item.ProductId
                      StockingPointId = item.StockingPointId
                      Quantity = item.Quantity
                      UnitOfMeasure = "UOM-PCS"
                      RoutingId = None
                      SupplierId = None
                      IsFirm =
                        (item.Status.Equals("Firm", StringComparison.OrdinalIgnoreCase)
                         || item.Status.Equals("InTransit", StringComparison.OrdinalIgnoreCase))
                      IsExpedited = false
                      IsLocked = false
                      UsesLeadTimeQuantity = false
                      RequiredDeliveryDate = Some item.ExpectedDeliveryUtc
                      CreatedDate = DateTimeOffset.UtcNow }

                let! decision = capabilities.Create createReq
                return ACL.toContract decision.NewState
        }

    let private transitionState capabilities (item: SupplyOrderUpdateReq) (order: Supply.SupplyOrder) =
        let rec transitionTo (targetState: string) (currentOrder: Supply.SupplyOrder) =
            taskResult {
                let currentStatus = currentOrder.State.Trim().ToLowerInvariant()
                let target = targetState.Trim().ToLowerInvariant()
                if currentStatus = target then
                    return currentOrder
                else
                    match target with
                    | "inprogress" | "intransit" ->
                        match currentStatus with
                        | "created" | "planned" ->
                            let! res = capabilities.Confirm { Id = item.SupplyOrderId; ConfirmedDate = DateTimeOffset.UtcNow }
                            return! transitionTo targetState (ACL.toContract res.NewState)
                        | "confirmed" ->
                            let! res = capabilities.Release { Id = item.SupplyOrderId; ReleasedDate = DateTimeOffset.UtcNow }
                            return! transitionTo targetState (ACL.toContract res.NewState)
                        | "released" ->
                            let! res = capabilities.Start { Id = item.SupplyOrderId; StartedDate = DateTimeOffset.UtcNow }
                            return ACL.toContract res.NewState
                        | _ -> return currentOrder
                    | "completed" | "received" ->
                        if currentStatus <> "inprogress" then
                            let! inProgressOrder = transitionTo "inprogress" currentOrder
                            return! transitionTo targetState inProgressOrder
                        else
                            let! res =
                                capabilities.Complete
                                    { Id = item.SupplyOrderId
                                      ScrapQuantity = 0.0m
                                      CompletedDate = DateTimeOffset.UtcNow
                                      FeedbackId = None }
                            return ACL.toContract res.NewState
                    | "cancelled" ->
                        let! res =
                            capabilities.Cancel
                                { Id = item.SupplyOrderId
                                  CancelledDate = DateTimeOffset.UtcNow }
                        return ACL.toContract res.NewState
                    | "confirmed" | "firm" ->
                        match currentStatus with
                        | "created" | "planned" ->
                            let! res = capabilities.Confirm { Id = item.SupplyOrderId; ConfirmedDate = DateTimeOffset.UtcNow }
                            return ACL.toContract res.NewState
                        | _ -> return currentOrder
                    | "planned" ->
                        match currentStatus with
                        | "created" ->
                            let! res = capabilities.Plan { Id = item.SupplyOrderId; PlannedDeliveryDate = DateTimeOffset.UtcNow }
                            return ACL.toContract res.NewState
                        | _ -> return currentOrder
                    | _ -> return currentOrder
            }
        transitionTo item.Status order

    let processSingleUpdate capabilities query (item: SupplyOrderUpdateReq) =
        taskResult {
            let! existing =
                task {
                    let! res = query.GetById item.SupplyOrderId
                    return Ok res
                }

            let! order = createIfMissing capabilities item existing
            return! transitionState capabilities item order
        }

    let processStatusUpdates
        (capabilities: SupplyOrderCapabilities)
        (query: QueryService<Supply.SupplyOrder, string>)
        (statusUpdates: SupplyOrderUpdateReq list)
        : TaskResult<Supply.SupplyOrder list, ApplicationError> =
        statusUpdates |> List.map(processSingleUpdate capabilities query) |> TaskResult.sequence

    let autoFirmOrders
        (capabilities: SupplyOrderCapabilities)
        (query: QueryService<Supply.SupplyOrder, string>)
        (asOf: DateTimeOffset)
        (firmingDays: int)
        : TaskResult<unit, ApplicationError> =
        taskResult {
            let! (allOrders: Supply.SupplyOrder list) =
                task {
                    let! res = query.GetAll()
                    return Ok res
                }

            let plannedOrders =
                allOrders
                |> List.filter(fun (o: Supply.SupplyOrder) ->
                    String.Equals(o.State, "Planned", StringComparison.OrdinalIgnoreCase) && not o.IsFirm)

            let firmIfInside (order: Supply.SupplyOrder) =
                match order.RequiredDeliveryDate with
                | Some dueDate ->
                    let days = (dueDate - asOf).Days

                    if days <= firmingDays then
                        capabilities.Confirm { Id = order.Id; ConfirmedDate = asOf } |> TaskResult.ignore
                    else
                        TaskResult.return_()
                | None -> TaskResult.return_()

            let firmTask = plannedOrders |> List.map firmIfInside |> TaskResult.sequence

            return! firmTask |> TaskResult.map(fun _ -> ())
        }

let createCapabilities (repo: Repository<SupplyOrder, string, SupplyOrderEvent>) =
    { Create = liftCmdValidation ACL.toCreateCommand >=> handleCommand (fun cmd -> cmd.Id) repo CreateSupplyOrder decide

      Start =
        liftCmdResult ACL.toStartCommand
        >=> handleCommand (fun cmd -> SupplyOrderId.value cmd.Id) repo StartSupplyOrder decide

      PartialComplete =
        liftCmdValidation ACL.toPartialCompleteCommand
        >=> handleCommand (fun cmd -> SupplyOrderId.value cmd.Id) repo PartialCompleteSupplyOrder decide

      Complete =
        liftCmdResult ACL.toCompleteCommand
        >=> handleCommand (fun cmd -> SupplyOrderId.value cmd.Id) repo CompleteSupplyOrder decide

      Plan =
        liftCmdResult ACL.toPlanCommand
        >=> handleCommand (fun cmd -> SupplyOrderId.value cmd.Id) repo PlanSupplyOrder decide

      Confirm =
        liftCmdResult ACL.toConfirmCommand
        >=> handleCommand (fun cmd -> SupplyOrderId.value cmd.Id) repo ConfirmSupplyOrder decide

      Release =
        liftCmdResult ACL.toReleaseCommand
        >=> handleCommand (fun cmd -> SupplyOrderId.value cmd.Id) repo ReleaseSupplyOrder decide

      Cancel =
        liftCmdResult ACL.toCancelCommand
        >=> handleCommand (fun cmd -> SupplyOrderId.value cmd.Id) repo CancelSupplyOrder decide

      Lock =
        liftCmdResult ACL.toLockCommand
        >=> handleCommand (fun cmd -> SupplyOrderId.value cmd.Id) repo LockSupplyOrder decide }

let evolveProjection (state: Map<string, Medhavi.Contracts.Supply.SupplyOrder>) (evt: SupplyOrderEvent) =
    match evt with
    | SupplyOrderCreated e -> Map.add (SupplyOrderId.value e.Id) (ACL.toContract e) state
    | SupplyOrderPlanned e ->
        let key = SupplyOrderId.value e.Id

        match Map.tryFind key state with
        | Some existing ->
            Map.add
                key
                { existing with
                    State = "Planned"
                    ModifiedDate = Timestamp.value e.PlannedDeliveryDate }
                state
        | None -> state
    | SupplyOrderConfirmed e ->
        let key = SupplyOrderId.value e.Id

        match Map.tryFind key state with
        | Some existing ->
            Map.add
                key
                { existing with
                    State = "Confirmed"
                    IsFirm = true
                    ModifiedDate = Timestamp.value e.ConfirmedDate }
                state
        | None -> state
    | SupplyOrderReleased e ->
        let key = SupplyOrderId.value e.Id

        match Map.tryFind key state with
        | Some existing ->
            Map.add
                key
                { existing with
                    State = "Released"
                    ModifiedDate = Timestamp.value e.ReleasedDate }
                state
        | None -> state
    | SupplyOrderStarted e ->
        let key = SupplyOrderId.value e.Id

        match Map.tryFind key state with
        | Some existing ->
            Map.add
                key
                { existing with
                    State = "InProgress"
                    ModifiedDate = Timestamp.value e.ReleasedDate }
                state
        | None -> state
    | SupplyOrderCompleted e ->
        let key = SupplyOrderId.value e.Id

        match Map.tryFind key state with
        | Some existing ->
            let scrapVal = Quantity.value e.ScrapQuantity
            let newScrap = existing.ScrapQuantity + scrapVal
            let newCompleted = existing.Quantity - newScrap |> max 0m

            Map.add
                key
                { existing with
                    State = "Completed"
                    CompletedQuantity = newCompleted
                    ScrapQuantity = newScrap
                    ModifiedDate = Timestamp.value e.CompletedDate }
                state
        | None -> state
    | SupplyOrderPartiallyCompleted e ->
        let key = SupplyOrderId.value e.Id

        match Map.tryFind key state with
        | Some existing ->
            Map.add
                key
                { existing with
                    State = "InProgress"
                    CompletedQuantity = existing.CompletedQuantity + Quantity.value e.CompletedQuantity
                    ScrapQuantity = existing.ScrapQuantity + Quantity.value e.ScrapQuantity
                    ModifiedDate = Timestamp.value e.CompletedDate }
                state
        | None -> state
    | SupplyOrderCancelled e ->
        let key = SupplyOrderId.value e.Id

        match Map.tryFind key state with
        | Some existing ->
            Map.add
                key
                { existing with
                    State = "Cancelled"
                    ModifiedDate = Timestamp.value e.CancelledDate }
                state
        | None -> state
    | SupplyOrderLocked e ->
        let key = SupplyOrderId.value e.Id

        match Map.tryFind key state with
        | Some existing ->
            Map.add
                key
                { existing with
                    IsLocked = e.Locked
                    ModifiedDate = Timestamp.value e.ModifiedDate }
                state
        | None -> state
    | SupplyOrderPriorityUpdated _ -> state

let createProjectionAgent () =
    ProjectionAgent<Map<string, Medhavi.Contracts.Supply.SupplyOrder>, SupplyOrderEvent>(
        evolveProjection,
        Map.empty,
        "SupplyOrderReadModel"
    )

let createSupplyOrderApi (capabilities: SupplyOrderCapabilities) agent =
    let query = QueryServiceBase.getQueryService agent id

    { Create =
        fun req -> capabilities.Create req |> TaskResult.map(fun d -> d.NewState) |> TaskResult.map ACL.toContract |> TaskResult.mapError ApplicationError.mapToApiError
      CreateBulk =
        fun reqs ->
            reqs
            |> List.map capabilities.Create
            |> TaskResult.sequence
            |> TaskResult.map(fun decisions ->
                decisions
                |> List.map(fun d -> d.NewState)
                |> List.map ACL.toContract)
            |> TaskResult.mapError ApplicationError.mapToApiError
      ProcessStatusUpdates = Service.processStatusUpdates capabilities query >> TaskResult.mapError ApplicationError.mapToApiError
      Start = fun req -> capabilities.Start req |> TaskResult.map(fun d -> d.NewState) |> TaskResult.map ACL.toContract  |> TaskResult.mapError ApplicationError.mapToApiError
      PartialComplete =
        fun req ->
            capabilities.PartialComplete req |> TaskResult.map(fun d -> d.NewState) |> TaskResult.map ACL.toContract  |> TaskResult.mapError ApplicationError.mapToApiError
      Complete =
        fun req -> capabilities.Complete req |> TaskResult.map(fun d -> d.NewState) |> TaskResult.map ACL.toContract  |> TaskResult.mapError ApplicationError.mapToApiError
      Plan = fun req -> capabilities.Plan req |> TaskResult.map(fun d -> d.NewState) |> TaskResult.map ACL.toContract  |> TaskResult.mapError ApplicationError.mapToApiError
      Confirm =
        fun req -> capabilities.Confirm req |> TaskResult.map(fun d -> d.NewState) |> TaskResult.map ACL.toContract  |> TaskResult.mapError ApplicationError.mapToApiError
      Release =
        fun req -> capabilities.Release req |> TaskResult.map(fun d -> d.NewState) |> TaskResult.map ACL.toContract  |> TaskResult.mapError ApplicationError.mapToApiError
      Cancel =
        fun req -> capabilities.Cancel req |> TaskResult.map(fun d -> d.NewState) |> TaskResult.map ACL.toContract  |> TaskResult.mapError ApplicationError.mapToApiError
      Lock = fun req -> capabilities.Lock req |> TaskResult.map(fun d -> d.NewState) |> TaskResult.map ACL.toContract  |> TaskResult.mapError ApplicationError.mapToApiError
      AutoFirmOrders = fun asOf str -> task {
        let! res = Service.autoFirmOrders capabilities query asOf str
        return res |> Result.mapError ApplicationError.mapToApiError
        }
    }
    : SupplyOrderApi
