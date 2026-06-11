module Medhavi.Terminal.Demand

open System
open Medhavi.Terminal
open Medhavi.SharedKernel
open Medhavi.Demand

let seedDemands (context: DemandContext) =
    let now = DateTimeOffset.UtcNow

    let req =
        { DemandLineId = "DEMAND-1"
          DemandOrderId = "ORDER-1"
          SkuId = "SKU-BIKE"
          StockingPointId = "SP-WAREHOUSE"
          CustomerId = "CUST-1"
          Quantity = 100.0m
          UnitOfMeasure = "UOM-PCS"
          OrderDate = now
          EarliestDeliveryDate = Some(now.AddDays(8.0))
          RequestedDeliveryDate = now.AddDays(10.0)
          LatestDeliveryDate = Some(now.AddDays(12.0))
          ConfirmedDeliveryDate = Some(now.AddDays(10.0))
          ActualDeliveryDate = None
          Priority = 1
          DemandCategory = DemandCategory.CustomerOrderDemand
          IsFirm = true
          IsFrozen = false }

    let _ = (context.Commands.DemandLine.Define req).Result
    ()

let showData (context: DemandContext) printer =
    let allDemands =
        context.DemandAgent.GetStateAsync()
        |> fun t -> t.Result
        |> Map.values
        |> Seq.toList

    let headers =
        [ "DemandLineId"
          "DemandOrderId"
          "SkuId"
          "StockingPointId"
          "Quantity"
          "UnitOfMeasure"
          "OrderDate"
          "EarliestDeliveryDate"
          "RequestedDeliveryDate"
          "LatestDeliveryDate"
          "ConfirmedDeliveryDate"
          "ActualDeliveryDate"
          "Priority"
          "DemandCategory"
          "IsFirm"
          "IsFrozen"
          "OpenQuantity"
          "FulfilledQuantity"
          "Status" ]
        |> List.toArray

    let optStr opt =
        match opt with
        | Some x -> x.ToString()
        | None -> "-"

    let rows =
        allDemands
        |> List.map (fun d ->
            [ d.DemandLineId
              d.DemandOrderId
              SkuId.value d.SkuId
              StockingPointId.value d.StockingPointId
              (Quantity.value d.Quantity).ToString()
              d.UnitOfMeasure
              d.OrderDate.ToString()
              optStr d.EarliestDeliveryDate
              d.RequestedDeliveryDate.ToString()
              optStr d.LatestDeliveryDate
              optStr d.ConfirmedDeliveryDate
              optStr d.ActualDeliveryDate
              d.Priority.ToString()
              d.DemandCategory.ToString()
              d.IsFirm.ToString()
              d.IsFrozen.ToString()
              (Quantity.value d.OpenQuantity).ToString()
              (Quantity.value d.FulfilledQuantity).ToString()
              d.Status.ToString() ]
            |> List.toArray)
        |> List.toArray

    Printer.printTable printer "Demands" headers rows
