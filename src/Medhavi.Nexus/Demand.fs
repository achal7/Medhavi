namespace Medhavi.Nexus

open System
open Medhavi.SharedKernel
open Medhavi.Demand

module Demand =

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
