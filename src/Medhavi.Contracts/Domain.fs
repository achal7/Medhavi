namespace Medhavi.Contracts.Domain

open System

type UnitOfMeasure =
    { Id: string
      Code: string
      Name: string
      Status: bool
      ConversionFactor: decimal
      IsBase: bool }

type UnitConversion =
    { Id: string
      ProductId: string option
      FromUnitCode: string
      ToUnitCode: string
      Ratio: decimal
      Status: bool }

/// Request DTO for requesting order promising (ATP/CTP)
type PromiseRequest =
    { OrderId: string
      SkuId: string
      NodeId: string
      Quantity: decimal
      RequestedDate: DateTimeOffset }

/// Response DTO for order promising (ATP/CTP)
type PromiseResponse =
    { OrderId: string
      SkuId: string
      PromiseDate: DateTimeOffset
      IsFeasible: bool
      LimiterReason: string }
