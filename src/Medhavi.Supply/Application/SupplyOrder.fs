module Medhavi.Supply.Application.SupplyOrder

open Medhavi
open Medhavi.Common.Patterns
open Medhavi.Common.Validation
open Medhavi.Contracts.Domain
open Medhavi.Contracts.Integration
open Medhavi.Infrastructure
open Medhavi.Infrastructure.Projections
open Medhavi.SharedKernel
open Medhavi.SharedKernel.API
open Medhavi.SharedKernel.Aggregate
open Medhavi.Domain.Material.SupplyOrder

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
              RequiredDeliveryDate =
                req.RequiredDeliveryDate
                |> Option.map Timestamp.create
              CreatedDate = Timestamp.create req.CreatedDate }

        make
        <!> (parseOrderType req.OrderType |> fromResult)
        <*> (SkuId.create req.SkuId |> fromResult)
        <*> (StockingPointId.create req.StockingPointId
             |> fromResult)
        <*> (Quantity.create req.Quantity |> fromResult)
        <*> (UomId.create req.UnitOfMeasure |> fromResult)
        <*> (match req.RoutingId with
             | None -> Valid None
             | Some id ->
                 RoutingId.create id
                 |> Result.map Some
                 |> fromResult)

    let toStartCommand (req: SupplyOrderStartReq) : Result<StartSupplyOrderCmd, DomainError> =
        SupplyOrderId.create req.Id
        |> Result.map (fun id ->
            { Id = id
              StartedDate = Timestamp.create req.StartedDate })

    let toPartialCompleteCommand
        (req: SupplyOrderPartialCompleteReq)
        : Validation<PartialCompleteSupplyOrderCmd, DomainError> =
        let make (orderId: SupplyOrderId) (qty: Quantity) : PartialCompleteSupplyOrderCmd =
            { Id = orderId
              CompletedQuantity = qty
              CompletedDate = Timestamp.create req.CompletedDate }

        make
        <!> (SupplyOrderId.create req.Id |> fromResult)
        <*> (Quantity.create req.CompletedQuantity
             |> fromResult)

    let toCompleteCommand (req: SupplyOrderCompleteReq) : Result<CompleteSupplyOrderCmd, DomainError> =
        SupplyOrderId.create req.Id
        |> Result.map (fun id ->
            { Id = id
              CompletedDate = Timestamp.create req.CompletedDate })

    let toPlanCommand (req: SupplyOrderPlanReq) : Result<PlanSupplyOrderCmd, DomainError> =
        SupplyOrderId.create req.Id
        |> Result.map (fun id ->
            { Id = id
              PlannedDeliveryDate = Timestamp.create req.PlannedDeliveryDate })

    let toConfirmCommand (req: SupplyOrderConfirmReq) : Result<ConfirmSupplyOrderCmd, DomainError> =
        SupplyOrderId.create req.Id
        |> Result.map (fun id ->
            { Id = id
              ConfirmedDate = Timestamp.create req.ConfirmedDate })

    let toReleaseCommand (req: SupplyOrderReleaseReq) : Result<ReleaseSupplyOrderCmd, DomainError> =
        SupplyOrderId.create req.Id
        |> Result.map (fun id ->
            { Id = id
              ReleasedDate = Timestamp.create req.ReleasedDate })

    let toCancelCommand (req: SupplyOrderCancelReq) : Result<CancelSupplyOrderCmd, DomainError> =
        SupplyOrderId.create req.Id
        |> Result.map (fun id ->
            { Id = id
              CancelledDate = Timestamp.create req.CancelledDate })

    let toLockCommand (req: SupplyOrderLockReq) : Result<LockSupplyOrderCmd, DomainError> =
        SupplyOrderId.create req.Id
        |> Result.map (fun id ->
            { Id = id
              Locked = req.Locked
              ModifiedDate = Timestamp.create req.ModifiedDate })

    let toContract (o: SupplyOrder) : Contracts.Domain.SupplyOrder =
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
          RequiredDeliveryDate =
            o.RequiredDeliveryDate
            |> Option.map Timestamp.value
          CreatedDate = Timestamp.value o.CreatedDate
          ModifiedDate = Timestamp.value o.ModifiedDate }

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

let createCapabilities (repo: Repository<SupplyOrder, string, SupplyOrderEvent>) =
    { Create =
        liftCmdValidation ACL.toCreateCommand
        >=> handleCommand (fun cmd -> cmd.Id) repo CreateSupplyOrder decide

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

let evolveProjection (state: Map<string, Contracts.Domain.SupplyOrder>) (evt: SupplyOrderEvent) =
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
                    ModifiedDate = Timestamp.value e.StartedDate }
                state
        | None -> state
    | SupplyOrderCompleted e ->
        let key = SupplyOrderId.value e.Id

        match Map.tryFind key state with
        | Some existing ->
            Map.add
                key
                { existing with
                    State = "Completed"
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
    | SupplyOrderPriorityUpdated e -> state

let createProjectionAgent () =
    ProjectionAgent<Map<string, Contracts.Domain.SupplyOrder>, SupplyOrderEvent>(
        evolveProjection,
        Map.empty,
        "SupplyOrderReadModel"
    )

let createSupplyOrderApi (capabilities: SupplyOrderCapabilities) agent =
    { Create =
        fun req ->
            capabilities.Create req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map ACL.toContract
      Start =
        fun req ->
            capabilities.Start req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map ACL.toContract
      PartialComplete =
        fun req ->
            capabilities.PartialComplete req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map ACL.toContract
      Complete =
        fun req ->
            capabilities.Complete req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map ACL.toContract
      Plan =
        fun req ->
            capabilities.Plan req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map ACL.toContract
      Confirm =
        fun req ->
            capabilities.Confirm req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map ACL.toContract
      Release =
        fun req ->
            capabilities.Release req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map ACL.toContract
      Cancel =
        fun req ->
            capabilities.Cancel req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map ACL.toContract
      Lock =
        fun req ->
            capabilities.Lock req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map ACL.toContract
      QueryService = QueryServiceBase.getQueryService agent id }
    : SupplyOrderApi
