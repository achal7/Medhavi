namespace Medhavi.Demand

open System
open Medhavi.SharedKernel
open Medhavi.MasterData

/// Customer Order aggregate
type CustomerOrder = {
    OrderId: OrderId
    LineId: string
    SkuId: SkuId
    NodeId: NodeId
    Quantity: Qty
    DueDate: DateTimeOffset
    Priority: int // E.g., Gold = 1, Silver = 2, Bronze = 3
    IsExpedited: bool
}

/// Demand Forecast aggregate
type Forecast = {
    ForecastId: string
    SkuId: SkuId
    NodeId: NodeId
    Quantity: Qty
    PeriodStart: DateTimeOffset
    PeriodEnd: DateTimeOffset
}
