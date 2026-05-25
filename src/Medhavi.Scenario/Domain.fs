namespace Medhavi.Scenario

open System
open Medhavi.SharedKernel
open Medhavi.MasterData
open Medhavi.Demand
open Medhavi.Supply

/// Pegging Link connecting a demand requirements to supply orders
type PeggingLink = {
    PegId: string
    DemandOrderId: OrderId
    DemandLineId: string
    SupplyRefId: string // E.g. PlannedOrderId or purchase order id
    PeggedQty: Qty
    IsFixed: bool
}

/// Scenario metadata and sandboxing state
type Scenario = {
    ScenarioId: string
    Name: string
    BaseScenarioId: string option
    Version: int
    CreatedAt: DateTimeOffset
    IsActive: bool
}
