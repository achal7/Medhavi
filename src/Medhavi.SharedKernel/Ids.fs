namespace Medhavi.SharedKernel

open System.Text.Json.Serialization
open Medhavi.SharedKernel

[<JsonFSharpConverter>]
type SkuId = private SkuId of string

module SkuId =
    let create = IdsFactory.createExplicitId SkuId "SkuId"
    let value (SkuId id) = id

[<JsonFSharpConverter>]
type NodeId = private NodeId of string

module NodeId =
    let create = IdsFactory.createExplicitId NodeId "NodeId"
    let value (NodeId id) = id

[<JsonFSharpConverter>]
type SupplierId = private SupplierId of string

module SupplierId =
    let create = IdsFactory.createExplicitId SupplierId "SupplierId"
    let value (SupplierId id) = id

[<JsonFSharpConverter>]
type OrderId = private OrderId of string

module OrderId =
    let create = IdsFactory.createExplicitId OrderId "OrderId"
    let value (OrderId id) = id

[<JsonFSharpConverter>]
type UomId = private UomId of string

module UomId =
    let create = IdsFactory.createExplicitId UomId "UomId"
    let value (UomId id) = id
