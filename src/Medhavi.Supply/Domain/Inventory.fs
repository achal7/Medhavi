module Medhavi.Supply.Domain.InventoryAgg

open Medhavi.Common.Validation
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Aggregate

type Inventory =
    { Id: InventoryId
      SkuId: SkuId
      StockingPointId: StockingPointId
      Quantity: Quantity
      UnitOfMeasure: UomId
      InTransitInbound: Quantity
      InTransitOutbound: Quantity
      QualityHold: Quantity
      Damaged: Quantity
      AvailableToPromise: Quantity
      Created: Timestamp
      Modified: Timestamp }

type DefineInventoryCmd =
    { Id: string
      SkuId: SkuId
      StockingPointId: StockingPointId
      Quantity: decimal
      UnitOfMeasure: UomId
      LastUpdated: Timestamp option }

type InventoryCommand =
    | Create of DefineInventoryCmd
    | Remove of InventoryId

// Events
type InventoryCreatedEvt = Inventory

type InventoryRemovedEvt = { Id: InventoryId }

type InventoryEvent =
    | InventoryCreated of InventoryCreatedEvt
    | InventoryRemoved of InventoryRemovedEvt

// Signatures
type DecideInventory = Decide<Inventory, InventoryCommand, InventoryEvent>
type EvolveInventory = Evolve<Inventory, InventoryEvent>

let validateAndDefineInventory now cmd =
    let makeInventory now cmd id qty =
        { Id = id
          SkuId = cmd.SkuId
          StockingPointId = cmd.StockingPointId
          Quantity = qty
          UnitOfMeasure = cmd.UnitOfMeasure
          InTransitInbound = Quantity.Zero
          InTransitOutbound = Quantity.Zero
          QualityHold = Quantity.Zero
          Damaged = Quantity.Zero
          AvailableToPromise = Quantity.Zero
          Created = now
          Modified = Timestamp.minValue }

    makeInventory now cmd
    <!> (InventoryId.create cmd.Id |> fromResult)
    <*> (Quantity.create cmd.Quantity |> fromResult)

let decide: DecideInventory =
    fun command stateOpt ->
        match command, stateOpt with
        | Create cmd, None ->
            createAggregate (validateAndDefineInventory Timestamp.now) (fun inv -> [ InventoryCreated inv ]) cmd
        | Remove id, Some state ->
            Ok(
                { NewState = state
                  Events = [ InventoryRemoved({ Id = id }) ] }
            )
        | _, _ -> Error(DomainError.validation "Not Implemented")

let applyCreated (evt: InventoryCreatedEvt) : Inventory = evt

let evolve (_: Inventory option) (event: InventoryEvent) : Inventory option =
    match event with
    | InventoryCreated e -> Some(applyCreated e)
    | InventoryRemoved _ -> failwith "Not Implemented"
