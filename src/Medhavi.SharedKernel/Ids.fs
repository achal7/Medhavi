namespace Medhavi.SharedKernel

open System.Text.Json.Serialization
open Medhavi.SharedKernel

[<JsonFSharpConverter>]
type UomId = private UomId of string

module UomId =
    let create = IdsFactory.createExplicitId UomId "UomId"
    let value (UomId id) = id

[<JsonFSharpConverter>]
type SkuId = private SkuId of string

module SkuId =
    let create = IdsFactory.createExplicitId SkuId "SkuId"
    let value (SkuId id) = id

[<JsonFSharpConverter>]
type PlantId = private PlantId of string

module PlantId =
    let create = IdsFactory.createExplicitId PlantId "PlantId"
    let value (PlantId id) = id

[<JsonFSharpConverter>]
type CalendarId = CalendarId of string

module CalendarId =
    let create id =
        if System.String.IsNullOrWhiteSpace id then
            Error(DomainError.validation "CalendarId cannot be empty")
        else
            Ok(CalendarId id)

    let value (CalendarId id) = id

[<JsonFSharpConverter>]
type StockingPointId = private StockingPointId of string

module StockingPointId =
    let create = IdsFactory.createExplicitId StockingPointId "StockingPointId"
    let value (StockingPointId id) = id

[<JsonFSharpConverter>]
type NodeId = private NodeId of string

module NodeId =
    let create = IdsFactory.createExplicitId NodeId "NodeId"
    let value (NodeId id) = id

[<JsonFSharpConverter>]
type ResourceGroupId = private ResourceGroupId of string

module ResourceGroupId =
    let create = IdsFactory.createExplicitId ResourceGroupId "ResourceGroupId"
    let value (ResourceGroupId id) = id

[<JsonFSharpConverter>]
type StandardResourceId = private StandardResourceId of string

module StandardResourceId =
    let create = IdsFactory.createExplicitId StandardResourceId "StandardResourceId"
    let value (StandardResourceId id) = id

[<JsonFSharpConverter>]
type PhysicalResourceId = private PhysicalResourceId of string

module PhysicalResourceId =
    let create = IdsFactory.createExplicitId PhysicalResourceId "PhysicalResourceId"
    let value (PhysicalResourceId id) = id

[<JsonFSharpConverter>]
type CombinedResourceId = private CombinedResourceId of string

module CombinedResourceId =
    let create = IdsFactory.createExplicitId CombinedResourceId "CombinedResourceId"
    let value (CombinedResourceId id) = id

[<JsonFSharpConverter>]
type CapacityReservationId = private CapacityReservationId of string

module CapacityReservationId =
    let create = IdsFactory.createExplicitId CapacityReservationId "CapacityReservationId"

    let value (CapacityReservationId id) = id

[<JsonFSharpConverter>]
type CapacityRequirementId = private CapacityRequirementId of string

module CapacityRequirementId =
    let create = IdsFactory.createExplicitId CapacityRequirementId "CapacityRequirementId"

    let value (CapacityRequirementId id) = id

[<JsonFSharpConverter>]
type CapacityBucketId = private CapacityBucketId of string

module CapacityBucketId =
    let create (resId: PhysicalResourceId) (window: TimeWindow) =
        let resVal = PhysicalResourceId.value resId
        let startVal = Timestamp.value window.Start
        let endVal = Timestamp.value window.End
        CapacityBucketId $"{resVal}_{startVal:yyyyMMddHHmmss}_{endVal:yyyyMMddHHmmss}"

    let value (CapacityBucketId id) = id

[<JsonFSharpConverter>]
type WorkCenterId = private WorkCenterId of string

module WorkCenterId =
    let create = IdsFactory.createExplicitId WorkCenterId "WorkCenterId"
    let value (WorkCenterId id) = id

[<JsonFSharpConverter>]
type RoutingId = private RoutingId of string

module RoutingId =
    let create = IdsFactory.createExplicitId RoutingId "RoutingId"
    let value (RoutingId id) = id

[<JsonFSharpConverter>]
type RoutingStepId = private RoutingStepId of string

module RoutingStepId =
    let create = IdsFactory.createExplicitId RoutingStepId "RoutingStepId"
    let value (RoutingStepId id) = id

[<JsonFSharpConverter>]
type OperationId = private OperationId of string

module OperationId =
    let create = IdsFactory.createExplicitId OperationId "OperationId"
    let value (OperationId id) = id

[<JsonFSharpConverter>]
type SupplierId = private SupplierId of string

module SupplierId =
    let create = IdsFactory.createExplicitId SupplierId "SupplierId"
    let value (SupplierId id) = id

type InventoryId = private InventoryId of string

module InventoryId =
    let create = IdsFactory.createExplicitId InventoryId "InventoryId"
    let value (InventoryId id) = id

[<JsonFSharpConverter>]
type OrderId = private OrderId of string

module OrderId =
    let create = IdsFactory.createExplicitId OrderId "OrderId"
    let value (OrderId id) = id

[<JsonFSharpConverter>]
type PeggingId = private PeggingId of string

module PeggingId =
    let create = IdsFactory.createExplicitId PeggingId "Peggings"
    let value (PeggingId id) = id

[<JsonFSharpConverter>]
type ScenarioId = private ScenarioId of string

module ScenarioId =
    let create = IdsFactory.createExplicitId ScenarioId "ScenarioId"
    let value (ScenarioId id) = id
