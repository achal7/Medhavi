/// SE-S-030 — Inventory Position Anti-Corruption Layer
module Medhavi.Supply.ManageInventory.InventoryPosition.ACL

open Medhavi.Common.Validation
open Medhavi.Contracts.Supply
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model

let toUpdateCmd (req: UpdateInventoryPositionReq) : Validation<UpdateInventoryPositionCmd, DomainError> =
    let itemV = validateItemId req.ItemId
    let locV = validateLocationId req.LocationId

    (fun item loc ->
        { Item = item
          Location = loc
          TriggeringEvidence = req.TriggeringEvidence
          Timestamp = Timestamp.now () })
    <!> itemV
    <*> locV
