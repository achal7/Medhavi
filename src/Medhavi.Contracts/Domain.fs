namespace Medhavi.Contracts.Domain

open System

type UnitOfMeasure =
    { Id: string
      Code: string
      Name: string
      Status: bool
      ConversionFactor: decimal
      IsBase: bool }

type Plant =
    { Id: string
      Code: string
      Name: string
      Status: bool }

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

type Sku =
    { Id: string
      Code: string
      Name: string
      Group: string
      Status: bool }

type StockingPoint =
    { Id: string
      PlantId: string
      Code: string
      Name: string
      Type: string
      Status: bool }

type BomItem =
    { ComponentSkuId: string
      Quantity: decimal
      Sequence: int }

type Bom =
    { Id: string
      SkuId: string
      Items: BomItem list
      Status: bool }

type RoutingStep =
    { StepId: string
      Sequence: int
      ResourceGroupId: string option
      Yield: decimal option
      DurationPerUnitMinutes: decimal option }

type Routing =
    { Id: string
      Name: string
      Type: string
      Steps: RoutingStep list
      Status: bool }

type TransportLeg =
    { Id: string
      Origin: string
      Destination: string
      Mode: string
      LeadTimeMinutes: decimal
      Capacity: decimal option
      CapacityUnit: string option
      Status: bool }
