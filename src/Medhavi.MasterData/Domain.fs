namespace Medhavi.MasterData

open System
open Medhavi.SharedKernel

/// Product Master Data
type Product = {
    SkuId: SkuId
    Name: string
    UoM: string
    IsActive: bool
}

/// Bill of Materials (BOM) Line definition
type BomLine = {
    ParentSkuId: SkuId
    ComponentSkuId: SkuId
    QuantityRequired: Qty
}

/// Resource definition (Work Center / Machine)
type Resource = {
    ResourceId: string
    Name: string
    NodeId: NodeId
    IsActive: bool
}

/// Routing Step definition
type RoutingStep = {
    Sequence: int
    ResourceId: string
    SetupHours: float
    RunHoursPerUnit: float
}

/// Product Process Routing definition
type Routing = {
    SkuId: SkuId
    Steps: RoutingStep list
}
