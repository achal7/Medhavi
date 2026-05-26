namespace Medhavi.Contracts

open System

/// Integration event payloads for Master Data elements
type ProductImportedPayload = {
    SkuId: string
    Name: string
    UoM: string
    IsActive: bool
}

type BomLineImportedPayload = {
    ParentSkuId: string
    ComponentSkuId: string
    QuantityRequired: decimal
}

type StockingPointImportedPayload = {
    StockingPointId: string
    Name: string
    IsActive: bool
}

type ResourceImportedPayload = {
    ResourceId: string
    Name: string
    NodeId: string
    IsActive: bool
}

type RoutingStepImportedPayload = {
    Sequence: int
    ResourceId: string
    SetupHours: float
    RunHoursPerUnit: float
}

type RoutingImportedPayload = {
    SkuId: string
    Steps: RoutingStepImportedPayload list
}

type SupplierImportedPayload = {
    SupplierId: string
    Name: string
    IsActive: bool
}

/// Integration event payloads for Demand Signals
type CustomerOrderReceivedPayload = {
    OrderId: string
    SkuId: string
    NodeId: string
    Quantity: decimal
    RequestedDateUtc: DateTimeOffset
}

type ForecastReceivedPayload = {
    ForecastId: string
    SkuId: string
    NodeId: string
    Quantity: decimal
    ForecastDateUtc: DateTimeOffset
}

/// Bulk container for imported Master Data records.
type MasterDataPayload = {
    Products: ProductImportedPayload list
    Boms: BomLineImportedPayload list
    StockingPoints: StockingPointImportedPayload list
    Resources: ResourceImportedPayload list
    Routings: RoutingImportedPayload list
    Suppliers: SupplierImportedPayload list
}

/// Bulk container for processed Demand Signals.
type DemandSignalsPayload = {
    CustomerOrders: CustomerOrderReceivedPayload list
    Forecasts: ForecastReceivedPayload list
}
