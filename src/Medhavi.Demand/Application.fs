module Medhavi.Demand.Application

open Medhavi.Common.Patterns
open Medhavi.Common.Validation
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Aggregate
open Medhavi.Demand.Domain
open Medhavi.Demand.Domain.DemandLineAgg
open Medhavi.Contracts.Demand

type DemandLineApi =
    { Define: DemandDefineReq -> TaskResult<Domain.DemandLine, ApplicationError>
      DefineBulk: DemandDefineReq list -> TaskResult<Domain.DemandLine list, ApplicationError>
      Fulfill: FulfillDemandLineReq -> TaskResult<Domain.DemandLine, ApplicationError> }

module ACL =
    let toDefineCommand (req: DemandDefineReq) : Validation<DefineDemandLineCmd, DomainError> =
        let cat =
            match req.DemandCategory.ToLower() with
            | "customerorderdemand"
            | "customerorder" -> DemandCategory.CustomerOrderDemand
            | "salesorderforecast"
            | "forecast" -> DemandCategory.SalesOrderForecast
            | "interplanttransfer"
            | "transfer" -> DemandCategory.InterplantTransfer
            | "servicepart"
            | "sparepart" -> DemandCategory.ServicePart
            | "internalconsumption"
            | "consumption" -> DemandCategory.InternalConsumption
            | _ -> DemandCategory.CustomerOrderDemand

        let make (skuId: SkuId) (spId: StockingPointId) (qty: Quantity) : DefineDemandLineCmd =
            { DemandLineId = req.DemandLineId
              DemandOrderId = req.DemandOrderId
              SkuId = skuId
              StockingPointId = spId
              CustomerId = req.CustomerId
              Quantity = qty
              UnitOfMeasure = req.UnitOfMeasure
              OrderDate = req.OrderDate
              EarliestDeliveryDate = req.EarliestDeliveryDate
              RequestedDeliveryDate = req.RequestedDeliveryDate
              LatestDeliveryDate = req.LatestDeliveryDate
              ConfirmedDeliveryDate = req.ConfirmedDeliveryDate
              ActualDeliveryDate = req.ActualDeliveryDate
              Priority = req.Priority
              DemandCategory = cat
              IsFirm = req.IsFirm
              IsFrozen = req.IsFrozen }

        make <!> (SkuId.create req.SkuId |> fromResult)
        <*> (StockingPointId.create req.StockingPointId
             |> fromResult)
        <*> (Quantity.create req.Quantity |> fromResult)

    let toFulfillCommand (req: FulfillDemandLineReq) : Validation<FulfillDemandLineCmd, DomainError> =
        let make qty : FulfillDemandLineCmd =
            { DemandLineId = req.DemandLineId
              Quantity = qty }

        make
        <!> (Quantity.create req.Quantity |> fromResult)

type Decision = Decision<Domain.DemandLine, DemandLineEvent>

type DemandLineCapabilities =
    { Define: DemandDefineReq -> TaskResult<Decision, ApplicationError>
      Fulfill: FulfillDemandLineReq -> TaskResult<Decision, ApplicationError> }

let createCapabilities (repo: Repository<Domain.DemandLine, string, DemandLineEvent>) =
    { Define =
        liftCmdValidation ACL.toDefineCommand
        >=> handleCommand (fun cmd -> cmd.DemandLineId) repo Create decide

      Fulfill =
        liftCmdValidation ACL.toFulfillCommand
        >=> handleCommand (fun cmd -> cmd.DemandLineId) repo Fulfill decide }

let createDemandLineApi (capabilities: DemandLineCapabilities) =
    { Define =
        fun req ->
            capabilities.Define req
            |> TaskResult.map (fun d -> d.NewState)
      DefineBulk =
        fun reqs ->
            reqs
            |> List.map capabilities.Define
            |> TaskResult.sequence
            |> TaskResult.map (fun decisions -> decisions |> List.map (fun d -> d.NewState))
      Fulfill =
        fun req ->
            capabilities.Fulfill req
            |> TaskResult.map (fun d -> d.NewState) }
    : DemandLineApi
