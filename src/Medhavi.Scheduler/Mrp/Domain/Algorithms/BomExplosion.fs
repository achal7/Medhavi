/// BOM Explosion — Recursive multi-level BOM explosion with cycle detection
/// Phase 9.1: Multi-level BOM explosion
/// FP Pattern: Recursive pure functions with explicit error handling
module Medhavi.Planning.Mrp.Domain.Algorithms.BomExplosion

open System
open Medhavi.SharedKernel
open Medhavi.Planning.Mrp.Domain.Types
open Medhavi.Planning.Mrp.Domain.Errors
open Medhavi.Planning.Mrp.Domain.Policies

// ============================================================================
// BOM DATA TYPES (for dependency injection)
// ============================================================================

/// BOM component from master data
type BomComponent =
    { ComponentSkuId: SkuId
      QuantityPer: Quantity          // Quantity per parent unit
      UnitOfMeasureId: UomId
      Sequence: int
      IsPhantom: bool }             // Phantom items pass-through to children

/// BOM lookup result from master data
type BomRecord =
    { BomId: string
      ParentSkuId: SkuId
      Components: BomComponent list
      IsActive: bool }

/// BOM lookup function — injected dependency
/// Given a SKU and an optional selection policy, return the applicable BOM
type BomLookup = SkuId -> BomSelectionPolicy -> BomRecord option

// ============================================================================
// CONSTANTS
// ============================================================================

/// Maximum recursion depth to prevent runaway explosions
[<Literal>]
let MaxBomDepth = 50

// ============================================================================
// EXPLOSION ENGINE
// ============================================================================

/// Collect a list of Results into a Result of list, short-circuiting on first error
let private collectResults (results: Result<'a, BomExplosionError> list) : Result<'a list, BomExplosionError> =
    results
    |> List.fold (fun acc r ->
        match acc, r with
        | Error e, _ -> Error e
        | _, Error e -> Error e
        | Ok items, Ok item -> Ok (items @ [item]))
        (Ok [])

/// Explode a single demand through the BOM hierarchy
/// Pure recursive function with explicit cycle detection via path tracking
let explode
    (bomLookup: BomLookup)
    (selectionPolicy: BomSelectionPolicy)
    (demand: MrpDemand)
    : Result<ExplodedComponent list, BomExplosionError> =

    let rec explodeRecursive
        (skuId: SkuId)
        (nodeId: NodeId)
        (stockingPointId: StockingPointId)
        (requiredQty: Quantity)
        (requiredDate: Timestamp)
        (level: int)
        (path: SkuId list)
        : Result<ExplodedComponent list, BomExplosionError> =

        // Guard: max depth
        if level > MaxBomDepth then
            Error (MaxDepthExceeded (SkuId.value skuId, level))
        else

        // Guard: cycle detection
        let skuVal = SkuId.value skuId
        if path |> List.exists (fun p -> SkuId.value p = skuVal) then
            Error (CycleDetected (path |> List.map SkuId.value |> List.append [skuVal]))
        else

        // Guard: invalid quantity
        if Quantity.isZero requiredQty then
            Ok []
        else

        let updatedPath = path @ [skuId]

        // Lookup BOM for this SKU
        match bomLookup skuId selectionPolicy with
        | None ->
            // Leaf item (raw material / purchased item) — no BOM, create requirement
            Ok [{ SkuId = skuId
                  NodeId = nodeId
                  StockingPointId = stockingPointId
                  RequiredQuantity = requiredQty
                  RequiredDate = requiredDate
                  BomLevel = level
                  BomPath = updatedPath
                  ParentSkuId = if level > 0 then path |> List.tryLast else None
                  IsPhantom = false }]

        | Some bom ->
            if not bom.IsActive then
                Error (BomNotActive (SkuId.value skuId))
            else

            // Explode each component
            bom.Components
            |> List.sortBy (fun c -> c.Sequence)
            |> List.map (fun comp ->
                // Calculate component quantity: parent quantity × quantity per
                let componentQty = requiredQty * (Quantity.value comp.QuantityPer)

                if comp.IsPhantom then
                    // Phantom: explode through to children (don't create a requirement for the phantom itself)
                    explodeRecursive comp.ComponentSkuId nodeId stockingPointId componentQty requiredDate (level + 1) updatedPath
                else
                    // Normal component: create requirement AND check if it has children
                    let thisComponent =
                        { SkuId = comp.ComponentSkuId
                          NodeId = nodeId
                          StockingPointId = stockingPointId
                          RequiredQuantity = componentQty
                          RequiredDate = requiredDate
                          BomLevel = level + 1
                          BomPath = updatedPath @ [comp.ComponentSkuId]
                          ParentSkuId = Some skuId
                          IsPhantom = false }

                    // Try to explode further (sub-assemblies)
                    match bomLookup comp.ComponentSkuId selectionPolicy with
                    | None ->
                        // Leaf component
                        Ok [thisComponent]
                    | Some _childBom ->
                        // Has children — explode recursively
                        explodeRecursive comp.ComponentSkuId nodeId stockingPointId componentQty requiredDate (level + 1) updatedPath
                        |> Result.map (fun children -> thisComponent :: children))
            |> collectResults
            |> Result.map List.concat

    // Start explosion from the demand's root SKU
    explodeRecursive
        demand.SkuId
        demand.NodeId
        demand.StockingPointId
        demand.Quantity
        demand.RequiredDate
        0
        []



// ============================================================================
// BATCH EXPLOSION
// ============================================================================

/// Explode all demands through BOM hierarchy
/// Returns exploded components and any errors encountered
let explodeAll
    (bomLookup: BomLookup)
    (selectionPolicy: BomSelectionPolicy)
    (demands: MrpDemand list)
    : Result<ExplodedComponent list, BomExplosionError list> =

    let results =
        demands
        |> List.map (fun demand -> explode bomLookup selectionPolicy demand)

    let components, errors =
        results
        |> List.fold (fun (comps, errs) r ->
            match r with
            | Ok cs -> (comps @ cs, errs)
            | Error e -> (comps, errs @ [e]))
            ([], [])

    if not (List.isEmpty errors) then
        Error errors
    else
        Ok components

/// Group exploded components by SKU + stocking point for netting
let groupBySkuAndLocation (components: ExplodedComponent list) : (SkuId * StockingPointId * Quantity * Timestamp) list =
    components
    |> List.groupBy (fun c -> (c.SkuId, c.StockingPointId))
    |> List.map (fun ((skuId, spId), group) ->
        let totalQty = group |> List.map (fun c -> c.RequiredQuantity) |> Quantity.sum
        let earliestDate = group |> List.map (fun c -> c.RequiredDate) |> List.min
        (skuId, spId, totalQty, earliestDate))
