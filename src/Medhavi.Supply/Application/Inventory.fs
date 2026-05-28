module Medhavi.Supply.Application.Inventory

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
open Medhavi.Supply.Domain
open Medhavi.Supply.Domain.InventoryAgg

module ACL =

    let toDefineCommand (req: InventoryDefineReq) : Validation<DefineInventoryCmd, DomainError> =
        let make (skuId: SkuId) (spId: StockingPointId) (uomId: UomId) : DefineInventoryCmd =
            { Id = req.Id
              SkuId = skuId
              StockingPointId = spId
              Quantity = req.Quantity
              UnitOfMeasure = uomId
              LastUpdated = None }

        make <!> (SkuId.create req.SkuId |> fromResult)
        <*> (StockingPointId.create req.StockingPointId
             |> fromResult)
        <*> (UomId.create req.UnitOfMeasure |> fromResult)

    let toRemoveCommand (inventoryId: string) : Result<InventoryId, DomainError> = InventoryId.create inventoryId

    let toContract (inv: InventoryAgg.Inventory) : Contracts.Domain.Inventory =
        { Id = InventoryId.value inv.Id
          SkuId = SkuId.value inv.SkuId
          StockingPointId = StockingPointId.value inv.StockingPointId
          Quantity = Quantity.value inv.Quantity
          UnitOfMeasure = UomId.value inv.UnitOfMeasure
          InTransitInbound = Quantity.value inv.InTransitInbound
          InTransitOutbound = Quantity.value inv.InTransitOutbound
          QualityHold = Quantity.value inv.QualityHold
          Damaged = Quantity.value inv.Damaged
          AvailableToPromise = Quantity.value inv.AvailableToPromise
          Created = Timestamp.value inv.Created
          Modified = Timestamp.value inv.Modified }

type Decision = Decision<Inventory, InventoryEvent>

type InventoryCapabilities =
    { Define: InventoryDefineReq -> TaskResult<Decision, ApplicationError>
      Remove: string -> TaskResult<Decision, ApplicationError> }

let createCapabilities (repo: Repository<Inventory, string, InventoryEvent>) =
    { Define =
        liftCmdValidation ACL.toDefineCommand
        >=> handleCommand (fun cmd -> cmd.Id) repo Create decide

      Remove =
        liftCmdResult ACL.toRemoveCommand
        >=> handleCommand InventoryId.value repo Remove decide }

let evolveProjection (state: Map<string, Contracts.Domain.Inventory>) (evt: InventoryEvent) =
    match evt with
    | InventoryCreated inv -> Map.add (InventoryId.value inv.Id) (ACL.toContract inv) state
    | InventoryRemoved e -> Map.remove (InventoryId.value e.Id) state

let createProjectionAgent () =
    ProjectionAgent<Map<string, Contracts.Domain.Inventory>, InventoryEvent>(
        evolveProjection,
        Map.empty,
        "InventoryReadModel"
    )

let createQueryService agent = QueryServiceBase.getQueryService agent id

let createInventoryApi (capabilities: InventoryCapabilities) agent =
    { Define =
        fun req ->
            capabilities.Define req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map ACL.toContract
      Remove =
        fun reqId ->
            capabilities.Remove reqId
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map ACL.toContract
      QueryService = QueryServiceBase.getQueryService agent id }
    : InventoryApi
