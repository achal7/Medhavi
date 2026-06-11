module Medhavi.MasterData.Domain.BoMAgg

open System.Text.Json.Serialization
open Medhavi.Common.Validation
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Validations
open UomAgg
open Medhavi.SharedKernel.Aggregate

[<JsonFSharpConverter>]
type BillOfMaterialId = BillOfMaterialId of string

module BillOfMaterialId =
    let create = IdsFactory.createExplicitId BillOfMaterialId "BillOfMaterialId"
    let value (BillOfMaterialId id) = id

type BomItem =
    { ComponentSkuId: SkuId
      Quantity: Quantity
      UnitOfMeasureId: UomId
      Sequence: int }

type BillOfMaterial =
    { Id: BillOfMaterialId
      SkuId: SkuId
      Version: Version
      Items: BomItem list
      Status: Status
      CreatedAt: Timestamp
      ModifiedAt: Timestamp }

type DefineBomItemCmd =
    { ComponentSkuId: SkuId
      Quantity: Quantity
      UnitOfMeasureId: UomId
      Sequence: int }

type DefineBillOfMaterialCmd =
    { Id: BillOfMaterialId
      SkuId: SkuId
      Items: DefineBomItemCmd list }

type BomCommand =
    | DefineBom of DefineBillOfMaterialCmd
    | ActivateBom of id: BillOfMaterialId
    | DeactivateBom of id: BillOfMaterialId

type BomEvent =
    | BomDefined of BillOfMaterial
    | BomActivated of BillOfMaterialId * Timestamp
    | BomDeactivated of BillOfMaterialId * Timestamp

type DecideBom = Decide<BillOfMaterial, BomCommand, BomEvent>
type EvolveBom = Evolve<BillOfMaterial, BomEvent>

let validateAndBomItem (cmd: DefineBomItemCmd) =
    let mk (level: int) : BomItem =
        { ComponentSkuId = cmd.ComponentSkuId
          Quantity = cmd.Quantity
          UnitOfMeasureId = cmd.UnitOfMeasureId
          Sequence = level }

    mk <!> positive "Sequence" cmd.Sequence

let validateAndMakeBoM now (cmd: DefineBillOfMaterialCmd) =
    let mk (lines: BomItem list) : BillOfMaterial =
        { Id = cmd.Id
          SkuId = cmd.SkuId
          Version = Version.initial
          Items = lines
          Status = Active
          CreatedAt = now
          ModifiedAt = Timestamp.minValue }

    mk
    <!> (cmd.Items
         |> List.map validateAndBomItem
         |> sequence)

let private validateItems (items: DefineBomItemCmd list) =
    if List.isEmpty items then
        Error(DomainError.validation "BOM must contain at least one item")
    else
        let duplicatedSeq =
            items
            |> List.groupBy (fun item -> item.Sequence)
            |> List.exists (fun (_, grouped) -> List.length grouped > 1)

        if duplicatedSeq then
            Error(DomainError.validation "BOM item sequence must be unique")
        else
            Ok items

let decide: DecideBom =
    fun command stateOpt ->
        match command, stateOpt with
        | DefineBom cmd, None ->
            match validateItems cmd.Items with
            | Error e -> Error e
            | Ok _ -> createAggregate (validateAndMakeBoM Timestamp.now) (fun bom -> [ BomDefined bom ]) cmd

        | DefineBom _, Some _ -> Error(DomainError.validation "BOM already exists")

        | ActivateBom(id), Some state when state.Id = id ->
            match state.Status with
            | Active -> Error(DomainError.invariant "BOM is already active")
            | Inactive ->
                let updated =
                    { state with
                        Status = Active
                        ModifiedAt = Timestamp.now }

                Ok(
                    { NewState = updated
                      Events = [ BomActivated(id, Timestamp.now) ] }
                )
        | ActivateBom _, Some _ -> Error(DomainError.validation "BOM not found")

        | DeactivateBom(id), Some state when state.Id = id ->
            match state.Status with
            | Inactive -> Error(DomainError.invariant "BOM is already inactive")
            | Active ->
                let updated =
                    { state with
                        Status = Inactive
                        ModifiedAt = Timestamp.now }

                Ok(
                    { NewState = updated
                      Events = [ BomDeactivated(id, Timestamp.now) ] }
                )
        | DeactivateBom _, Some _ -> Error(DomainError.validation "BOM not found")

        | _, None -> Error(DomainError.validation "BOM not found")

let evolve: EvolveBom =
    fun event stateOpt ->
        match event, stateOpt with
        | BomDefined state, None -> Some state
        | BomActivated(id, modifiedAt), Some state when state.Id = id ->
            Some
                { state with
                    Status = Active
                    ModifiedAt = modifiedAt }
        | BomDeactivated(id, modifiedAt), Some state when state.Id = id ->
            Some
                { state with
                    Status = Inactive
                    ModifiedAt = modifiedAt }
        | BomDefined _, Some state -> Some state
        | _, current -> current
