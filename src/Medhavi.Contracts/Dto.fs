namespace Medhavi.Contracts

open System
open Medhavi.SharedKernel

/// DTO for unit conversion upsert requests
type UnitConversionDto = {
    Id: string
    ProductId: string option
    FromUnitCode: string
    ToUnitCode: string
    Ratio: float
    IsActive: bool
}

/// Request DTO for requesting order promising (ATP/CTP)
type PromiseRequest = {
    OrderId: string
    SkuId: string
    NodeId: string
    Quantity: decimal
    RequestedDate: DateTimeOffset
}

/// Response DTO for order promising (ATP/CTP)
type PromiseResponse = {
    OrderId: string
    SkuId: string
    PromiseDate: DateTimeOffset
    IsFeasible: bool
    LimiterReason: string
}
