namespace Medhavi.SemanticModel

type BatchIdentifier = private BatchIdentifier of string

module BatchIdentifier =
    let create (id: string) = Invariants.createStringId BatchIdentifier "BatchIdentifier" id
    let value (BatchIdentifier id) = id

/// Inventory is identified by Item + Location + Batch Identifier.
/// This is a composite identity, not a surrogate ID.
type InventoryIdentity =
    { Item: ItemId
      Location: LocationId
      Batch: BatchIdentifier }

module InventoryIdentity =
    let create
        (item: ItemId)
        (location: LocationId)
        (batch: BatchIdentifier)
        : Result<InventoryIdentity, SemanticValidationError> =

        if System.String.IsNullOrWhiteSpace(BatchIdentifier.value batch) then
            Error(InvalidCompositeIdentity "InventoryIdentity requires a non-empty BatchIdentifier.")
        else
            Ok
                { Item = item
                  Location = location
                  Batch = batch }

    let item (identity: InventoryIdentity) = identity.Item
    let location (identity: InventoryIdentity) = identity.Location
    let batch (identity: InventoryIdentity) = identity.Batch

    let parse (identityRef: string) =
        let parts = identityRef.Split('-')

        if parts.Length <> 3 then
            Error(InvalidCompositeIdentity "InventoryIdentity requires a non-empty BatchIdentifier.")
        else
            ItemId.create parts.[0]
            |> Result.bind(fun item ->
                LocationId.create parts.[1]
                |> Result.bind(fun location ->
                    BatchIdentifier.create parts.[2] |> Result.bind(fun batch -> create item location batch)))

    let toString (inventoryIdentity: InventoryIdentity) =
        sprintf
            "%s-%s-%s"
            (inventoryIdentity.Item |> ItemId.value)
            (inventoryIdentity.Location |> LocationId.value)
            (inventoryIdentity.Batch |> BatchIdentifier.value)

/// SE-C-015 Inventory
type Inventory =
    { Identity: InventoryIdentity
      OnHandQuantity: Quantity
      ObservationTime: Timestamp }

module Inventory =
    let validate (inventory: Inventory) : Result<unit, SemanticValidationError> =
        Invariants.firstError
            [ Invariants.nonEmptyIdentifier "InventoryIdentity.Batch" (BatchIdentifier.value inventory.Identity.Batch)
              Quantity.nonNegativeQuantity "Inventory.OnHandQuantity" inventory.OnHandQuantity ]
