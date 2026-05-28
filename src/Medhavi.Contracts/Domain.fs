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

type Inventory =
    { Id: string
      SkuId: string
      StockingPointId: string
      Quantity: decimal
      UnitOfMeasure: string
      InTransitInbound: decimal
      InTransitOutbound: decimal
      QualityHold: decimal
      Damaged: decimal
      AvailableToPromise: decimal
      Created: DateTimeOffset
      Modified: DateTimeOffset }

type SeasonalAdjustment =
    { PeriodStart: DateTimeOffset
      PeriodEnd: DateTimeOffset
      AdjustmentFactor: decimal }

type ReplenishmentPolicy =
    { Safety: decimal
      MinQty: decimal option
      MaxQty: decimal option
      CoverDays: decimal option
      LotSize: decimal option
      Expedite: bool }

type InventoryTarget =
    { Id: string
      SkuId: string
      StockingPointId: string
      ReplenishmentPolicy: ReplenishmentPolicy option
      SafetyStockQty: decimal option
      MinQty: decimal option
      MaxQty: decimal option
      TargetServiceLevel: decimal option
      CoverDays: decimal option
      SeasonalAdjustments: SeasonalAdjustment list
      EffectiveStart: DateTimeOffset option
      EffectiveEnd: DateTimeOffset option
      IsActive: bool }

type PriceTier =
    { TierNumber: int
      MinQuantity: decimal
      MaxQuantity: decimal option
      PricePerUnit: decimal
      Currency: string }

type SupplierCapacityWindow =
    { WindowId: string
      StartDate: DateTimeOffset
      EndDate: DateTimeOffset
      MaxQuantity: decimal
      AvailableQuantity: decimal }

type SupplierOffer =
    { Id: string
      SupplierId: string
      SkuId: string
      StockingPointId: string option
      Moq: decimal option
      LotSize: decimal option
      LeadTimeP50Minutes: float option
      LeadTimeP95Minutes: float option
      PriceTiers: PriceTier list
      Reliability: decimal option
      Incoterm: string option
      CapacityWindows: SupplierCapacityWindow list
      IsActive: bool }

type SupplyOrder =
    { Id: string
      OrderType: string
      SkuId: string
      StockingPointId: string
      Quantity: decimal
      UnitOfMeasure: string
      State: string
      RoutingId: string option
      SupplierId: string option
      IsFirm: bool
      IsExpedited: bool
      IsLocked: bool
      UsesLeadTimeQuantity: bool
      RequiredDeliveryDate: DateTimeOffset option
      CreatedDate: DateTimeOffset
      ModifiedDate: DateTimeOffset }

type MaterialSnapshot =
    { OnHand: decimal
      Inbound: (DateTimeOffset * decimal) list
      Reservations: (DateTimeOffset * decimal) list
      Safety: decimal }

type ProviderError =
    | UnknownError of string

type MaterialProvider =
    { GetSnapshot: string -> string -> DateTimeOffset -> Async<Result<MaterialSnapshot, ProviderError>>
      GetSupplierOptions: string -> string option -> decimal -> DateTimeOffset -> Async<Result<SupplierOffer list, ProviderError>>
      GetDateWiseAvailability: string -> string -> DateTimeOffset -> int -> Async<Result<(DateTimeOffset * decimal) list, ProviderError>> }

type MaterialReservation =
    { Id: string
      IdempotencyKey: string
      SkuId: string
      StockingPointId: string
      Quantity: decimal
      State: string // "Tentative", "Confirmed", "Released", "Expired", "Reduced"
      RequiredDate: DateTimeOffset
      ExpiryTime: DateTimeOffset
      Created: DateTimeOffset
      Modified: DateTimeOffset }