module Medhavi.Supply.Domain.InventoryTargetAgg

open System.Text.Json.Serialization
open Medhavi.Common
open Medhavi.SharedKernel

type ReplenishmentPolicy =
    { Safety: Quantity
      MinQty: Quantity option
      MaxQty: Quantity option
      CoverDays: decimal option
      LotSize: Quantity option
      Expedite: bool }

/// Seasonal adjustment factor
type SeasonalAdjustment =
    { PeriodStart: Timestamp
      PeriodEnd: Timestamp
      AdjustmentFactor: decimal } // e.g., 1.2 = 20% increase

[<JsonFSharpConverter>]
type InventoryTargetId = private InventoryTargetId of string

module InventoryTargetId =
    let create (skuId: SkuId) (spId: StockingPointId) =
        InventoryTargetId $"{SkuId.value skuId}-{StockingPointId.value spId}"

    let createFromExisting (existingId: string) =
        let parts = existingId.Split('-')

        if parts.Length <> 2 then
            Error(DomainError.validation "Invalid InventoryTargetId format")
        else
            let skuId = parts[0]
            let spId = parts[1]

            match (SkuId.create skuId, StockingPointId.create spId) with
            | Ok skuId, Ok spId -> Ok(create skuId spId)
            | _ -> Error(DomainError.validation "Invalid InventoryTargetId format")

    let value (InventoryTargetId id) = id

type InventoryTarget =
    { Id: InventoryTargetId
      SkuId: SkuId
      StockingPointId: StockingPointId
      ReplenishmentPolicy: ReplenishmentPolicy option
      SafetyStockQty: Quantity option
      MinQty: Quantity option
      MaxQty: Quantity option
      TargetServiceLevel: decimal option
      CoverDays: decimal option
      SeasonalAdjustments: SeasonalAdjustment list // TODO-014: Fixed - Seasonal adjustments
      EffectiveStart: Timestamp option
      EffectiveEnd: Timestamp option
      IsActive: bool
      CreatedDate: Timestamp
      ModifiedDate: Timestamp }

// Commands
type DefineInventoryTargetCmd =
    { Id: InventoryTargetId
      SkuId: SkuId
      StockingPointId: StockingPointId
      ReplenishmentPolicy: ReplenishmentPolicy option
      SafetyStockQty: decimal option
      MinQty: decimal option
      MaxQty: decimal option
      TargetServiceLevel: decimal option
      CoverDays: decimal option
      SeasonalAdjustments: SeasonalAdjustment list // TODO-014: Fixed - Seasonal adjustments
      EffectiveStart: Timestamp option
      EffectiveEnd: Timestamp option
      IsActive: bool }

type UpdateInventoryTargetCmd =
    { Id: InventoryTargetId
      SkuId: SkuId
      StockingPointId: StockingPointId
      ReplenishmentPolicy: ReplenishmentPolicy option
      SafetyStockQty: decimal option
      MinQty: decimal option
      MaxQty: decimal option
      TargetServiceLevel: decimal option
      CoverDays: decimal option
      SeasonalAdjustments: SeasonalAdjustment list option // TODO-014: Fixed - Seasonal adjustments
      EffectiveStart: Timestamp option
      EffectiveEnd: Timestamp option }

type ActivateInventoryTargetCmd =
    { Id: InventoryTargetId
      SkuId: SkuId
      StockingPointId: StockingPointId
      ModifiedDate: Timestamp }

type DeactivateInventoryTargetCmd =
    { Id: InventoryTargetId
      SkuId: SkuId
      StockingPointId: StockingPointId
      ModifiedDate: Timestamp }

type InventoryTargetCommand =
    | DefineInventoryTarget of DefineInventoryTargetCmd
    | UpdateInventoryTarget of UpdateInventoryTargetCmd
    | ActivateInventoryTarget of ActivateInventoryTargetCmd
    | DeactivateInventoryTarget of DeactivateInventoryTargetCmd

// Events
type InventoryTargetDefinedEvt =
    { Id: InventoryTargetId
      SkuId: SkuId
      StockingPointId: StockingPointId
      ReplenishmentPolicy: ReplenishmentPolicy option
      SafetyStockQty: Quantity option
      MinQty: Quantity option
      MaxQty: Quantity option
      TargetServiceLevel: decimal option
      CoverDays: decimal option
      SeasonalAdjustments: SeasonalAdjustment list
      EffectiveStart: Timestamp option
      EffectiveEnd: Timestamp option
      IsActive: bool
      CreatedDate: Timestamp }

type InventoryTargetUpdatedEvt =
    { Id: InventoryTargetId
      SkuId: SkuId
      StockingPointId: StockingPointId
      ReplenishmentPolicy: ReplenishmentPolicy option
      SafetyStockQty: Quantity option
      MinQty: Quantity option
      MaxQty: Quantity option
      TargetServiceLevel: decimal option
      CoverDays: decimal option
      SeasonalAdjustments: SeasonalAdjustment list option
      EffectiveStart: Timestamp option
      EffectiveEnd: Timestamp option
      ModifiedDate: Timestamp }

type InventoryTargetActivatedEvt =
    { Id: InventoryTargetId
      SkuId: SkuId
      StockingPointId: StockingPointId
      ModifiedDate: Timestamp }

type InventoryTargetDeactivatedEvt =
    { Id: InventoryTargetId
      SkuId: SkuId
      StockingPointId: StockingPointId
      ModifiedDate: Timestamp }

type InventoryTargetEvent =
    | InventoryTargetDefined of InventoryTargetDefinedEvt
    | InventoryTargetUpdated of InventoryTargetUpdatedEvt
    | InventoryTargetActivated of InventoryTargetActivatedEvt
    | InventoryTargetDeactivated of InventoryTargetDeactivatedEvt

// Signatures
type DecideInventoryTarget = Decide<InventoryTarget, InventoryTargetCommand, InventoryTargetEvent>

type EvolveInventoryTarget = Evolve<InventoryTarget, InventoryTargetEvent>

// Validation functions (includes business rules)
// let validateDefine (cmd: DefineInventoryTargetCmd) : Result<unit, DomainError> =
//     result {
//         let! _ = required "SkuId" (SkuId.value cmd.SkuId)
//         let! _ = required "StockingPointId" (StockingPointId.value cmd.StockingPointId)

//         // Business rule: If MaxQty is specified, it must be >= MinQty
//         match cmd.MinQty, cmd.MaxQty with
//         | Some min, Some max when max < min ->
//             return! Error(DomainError.validation "MaxQty must be greater than or equal to MinQty")
//         | _ -> return ()
//     }

let makePolicy days expedite safety min max lot : ReplenishmentPolicy =
    { Safety = safety
      MinQty = min
      MaxQty = max
      CoverDays = days
      LotSize = lot
      Expedite = expedite }

let validateActivate (_cmd: ActivateInventoryTargetCmd) : Result<unit, DomainError> =
    // Activation is always allowed
    Ok()

let validateDeactivate (_cmd: DeactivateInventoryTargetCmd) : Result<unit, DomainError> =
    // Deactivation is always allowed
    Ok()

// State evolution functions (pure state transitions)
let applyDefinedEvent (evt: InventoryTargetDefinedEvt) : InventoryTarget =
    { Id = evt.Id
      SkuId = evt.SkuId
      StockingPointId = evt.StockingPointId
      ReplenishmentPolicy = evt.ReplenishmentPolicy
      SafetyStockQty = evt.SafetyStockQty
      MinQty = evt.MinQty
      MaxQty = evt.MaxQty
      TargetServiceLevel = evt.TargetServiceLevel
      CoverDays = evt.CoverDays
      SeasonalAdjustments = evt.SeasonalAdjustments
      EffectiveStart = evt.EffectiveStart
      EffectiveEnd = evt.EffectiveEnd
      IsActive = evt.IsActive
      CreatedDate = evt.CreatedDate
      ModifiedDate = evt.CreatedDate }

let applyUpdatedEvent (existing: InventoryTarget) (evt: InventoryTargetUpdatedEvt) : InventoryTarget =
    { existing with
        ReplenishmentPolicy = evt.ReplenishmentPolicy
        SafetyStockQty = evt.SafetyStockQty
        MinQty = evt.MinQty
        MaxQty = evt.MaxQty
        TargetServiceLevel = evt.TargetServiceLevel
        CoverDays = evt.CoverDays
        SeasonalAdjustments =
            evt.SeasonalAdjustments
            |> Option.defaultValue existing.SeasonalAdjustments
        EffectiveStart = evt.EffectiveStart
        EffectiveEnd = evt.EffectiveEnd
        ModifiedDate = evt.ModifiedDate }

let applyActivatedEvent (existing: InventoryTarget) (evt: InventoryTargetActivatedEvt) : InventoryTarget =
    { existing with
        IsActive = true
        ModifiedDate = evt.ModifiedDate }

let applyDeactivatedEvent (existing: InventoryTarget) (evt: InventoryTargetDeactivatedEvt) : InventoryTarget =
    { existing with
        IsActive = false
        ModifiedDate = evt.ModifiedDate }

let evolve (state: InventoryTarget option) (event: InventoryTargetEvent) : InventoryTarget option =
    match event, state with
    | InventoryTargetDefined e, None -> Some(applyDefinedEvent e)
    | InventoryTargetUpdated e, Some s -> Some(applyUpdatedEvent s e)
    | InventoryTargetActivated e, Some s -> Some(applyActivatedEvent s e)
    | InventoryTargetDeactivated e, Some s -> Some(applyDeactivatedEvent s e)
    | InventoryTargetDefined _, Some _ -> state // Idempotent - target already exists
    | _, None -> None // Can't apply updates to non-existent target

let decide: DecideInventoryTarget =
    fun cmd stateOpt ->
        match cmd, stateOpt with
        | DefineInventoryTarget c, None ->
            let evt =
                { Id = InventoryTargetId.create c.SkuId c.StockingPointId
                  SkuId = c.SkuId
                  StockingPointId = c.StockingPointId
                  ReplenishmentPolicy = c.ReplenishmentPolicy
                  SafetyStockQty = Quantity.tryFromOption c.SafetyStockQty
                  MinQty = Quantity.tryFromOption c.MinQty
                  MaxQty = Quantity.tryFromOption c.MaxQty
                  TargetServiceLevel = c.TargetServiceLevel
                  CoverDays = c.CoverDays
                  SeasonalAdjustments = c.SeasonalAdjustments
                  EffectiveStart = c.EffectiveStart
                  EffectiveEnd = c.EffectiveEnd
                  IsActive = c.IsActive
                  CreatedDate = Timestamp.now }

            let state = applyDefinedEvent evt

            Ok
                { NewState = state
                  Events = [ InventoryTargetDefined evt ] }

        | DefineInventoryTarget _, Some _ -> Error(DomainError.validation "InventoryTarget already exists")

        | UpdateInventoryTarget c, Some state when state.Id = c.Id ->
            let safetyStock =
                c.SafetyStockQty
                |> Option.map Quantity.clampToZero

            let minQty = c.MinQty |> Option.map Quantity.clampToZero
            let maxQty = c.MaxQty |> Option.map Quantity.clampToZero

            let evt =
                { Id = c.Id
                  SkuId = c.SkuId
                  StockingPointId = c.StockingPointId
                  ReplenishmentPolicy = c.ReplenishmentPolicy
                  SafetyStockQty = safetyStock
                  MinQty = minQty
                  MaxQty = maxQty
                  TargetServiceLevel = c.TargetServiceLevel
                  CoverDays = c.CoverDays
                  SeasonalAdjustments = c.SeasonalAdjustments
                  EffectiveStart = c.EffectiveStart
                  EffectiveEnd = c.EffectiveEnd
                  ModifiedDate = Timestamp.now }

            let newState = applyUpdatedEvent state evt

            Ok
                { NewState = newState
                  Events = [ InventoryTargetUpdated evt ] }

        | UpdateInventoryTarget _, Some _ -> Error(DomainError.validation "InventoryTarget not found")

        | ActivateInventoryTarget c, Some state when state.Id = c.Id ->
            let evt: InventoryTargetActivatedEvt =
                { Id = c.Id
                  SkuId = c.SkuId
                  StockingPointId = c.StockingPointId
                  ModifiedDate = c.ModifiedDate }

            let newState = applyActivatedEvent state evt

            Ok
                { NewState = newState
                  Events = [ InventoryTargetActivated evt ] }

        | ActivateInventoryTarget _, Some _ -> Error(DomainError.validation "InventoryTarget not found")

        | DeactivateInventoryTarget c, Some state when state.Id = c.Id ->
            let evt =
                { Id = c.Id
                  SkuId = c.SkuId
                  StockingPointId = c.StockingPointId
                  ModifiedDate = c.ModifiedDate }

            let newState = applyDeactivatedEvent state evt

            Ok
                { NewState = newState
                  Events = [ InventoryTargetDeactivated evt ] }

        | DeactivateInventoryTarget _, Some _ -> Error(DomainError.validation "InventoryTarget not found")
        | _, None -> Error(DomainError.validation "InventoryTarget not found")
