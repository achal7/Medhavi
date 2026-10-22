/// SE-S-031 — Inventory Policy Anti-Corruption Layer
module Medhavi.Supply.ManageInventory.InventoryPolicy.ACL

open Medhavi.Common.Validation
open Medhavi.Contracts.Supply
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model
open Medhavi.Supply.ManageInventory

let toDomainReplanishmentStrategy (value: string) : Validation<ReplenishmentStrategy, DomainError> =
    match value.ToLower() with
    | "minmax" -> Valid MinMax
    | "periodicreview" -> Valid PeriodicReview
    | "reorderpoint" -> Valid ReorderPoint
    | _ -> Invalid [ DomainError.validation "Invalid replanishment strategy" ]

let private validateOptional (validator: 'a -> Validation<'b, 'e>) (opt: 'a option) : Validation<'b option, 'e> =
    match opt with
    | None -> Valid None
    | Some x -> validator x |> map Some

let toDomainParameters (dto: InventoryPolicyParametersDto) : Validation<InventoryPolicyParameters, DomainError> =
    let strategyV = toDomainReplanishmentStrategy dto.ReplenishmentStrategy
    let ssQtyV = validateQty dto.SafetyStockQuantity
    let ropQtyV = dto.ReorderPointQuantity |> validateOptional validateQty
    let minQtyV = dto.MinimumQuantity |> validateOptional validateQty
    let maxQtyV = dto.MaximumQuantity |> validateOptional validateQty
    let targetQtyV = validateQty dto.TargetStockLevel

    (fun strategy ssQty ropQty minQty maxQty targetQty ->
        {
            ReplenishmentStrategy = strategy
            SafetyStockQuantity = ssQty
            ReorderPointQuantity = ropQty
            MinimumQuantity = minQty
            MaximumQuantity = maxQty
            TargetStockLevel = targetQty
            TargetServiceLevel = dto.TargetServiceLevel
            ReviewFrequencyDays = dto.ReviewFrequencyDays
            LeadTimeDays = dto.LeadTimeDays
            DemandVariability = dto.DemandVariability
        })
    <!> strategyV
    <*> ssQtyV
    <*> ropQtyV
    <*> minQtyV
    <*> maxQtyV
    <*> targetQtyV

let toUpdateCmd (req: UpdateInventoryPolicyReq) : Validation<UpdateInventoryPolicyCmd, DomainError> =
    let itemV = validateItemId req.ItemId
    let locV = validateLocationId req.LocationId
    let manualOverrideV = req.ManualOverrideParameters |> validateOptional toDomainParameters

    (fun item loc manualOverride ->
        {
            Item = item
            Location = loc
            TargetServiceLevel = req.TargetServiceLevel
            ManualOverrideParameters = manualOverride
            Timestamp = Timestamp.now()
        })
    <!> itemV
    <*> locV
    <*> manualOverrideV
