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
type ResourceId = private ResourceId of string

module ResourceId =
    let create = IdsFactory.createExplicitId ResourceId "ResourceId"
    let value (ResourceId id) = id

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
type RoutingId = private RoutingId of string

module RoutingId =
    let create = IdsFactory.createExplicitId RoutingId "RoutingId"
    let value (RoutingId id) = id

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
