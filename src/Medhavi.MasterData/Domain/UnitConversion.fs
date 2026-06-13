module Medhavi.MasterData.Domain.UnitConversionAgg

open System.Text.Json.Serialization
open Medhavi.Common.Validation
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Aggregate

[<JsonFSharpConverter>]
type UnitConversionId = private UnitConversionId of string

module UnitConversionId =
    let create = IdsFactory.createExplicitId UnitConversionId "UnitConversionId"
    let value (UnitConversionId id) = id

// Core UnitConversion aggregate - defines conversion relationships between units
type UnitConversion =
    { Id: UnitConversionId
      ProductId: SkuId option
      FromUom: UomId
      ToUom: UomId
      Ratio: PositiveDecimal // multiply source by Ratio to get target
      Status: Status
      Created: Timestamp
      Modified: Timestamp }

// Commands
type DefineUnitConversionCmd =
    { Id: string
      ProductId: SkuId option
      FromUom: UomId
      ToUom: UomId
      Ratio: decimal
      Created: Timestamp }

type UpdateUnitConversionCmd =
    { Id: UnitConversionId; Ratio: decimal }

// Commands
type UnitConversionCommand =
    | DefineUnitConversion of DefineUnitConversionCmd
    | RetireUnitConversion of UnitConversionId
    | UpdateRatio of UpdateUnitConversionCmd
    | UpdateStatus of UnitConversionId * Status

// Events
type UnitConversionDefinedEvt = UnitConversion

type RatioUpdatedEvt =
    { Id: UnitConversionId
      NewRatio: PositiveDecimal
      Modified: Timestamp }

type StatusUpdatedEvt =
    { Id: UnitConversionId
      NewStatus: Status
      Modified: Timestamp }

type UnitConversionRetiredEvt =
    { Id: UnitConversionId
      Modified: Timestamp }

type UnitConversionEvent =
    | UnitConversionDefined of UnitConversionDefinedEvt
    | RatioUpdated of RatioUpdatedEvt
    | StatusUpdated of StatusUpdatedEvt
    | UnitConversionRetired of UnitConversionRetiredEvt

// Decision function signature
type DecideUnitConversion = Decide<UnitConversion, UnitConversionCommand, UnitConversionEvent>

// Evolution function signature
type EvolveUnitConversion = Evolve<UnitConversion, UnitConversionEvent>

let defineUnitConversion now pid fromUom toUom id ratio =
    { Id = id
      ProductId = pid
      FromUom = fromUom
      ToUom = toUom
      Ratio = ratio
      Status = Active
      Created = now
      Modified = Timestamp.minValue }

let validateAndDefine (cmd: DefineUnitConversionCmd) =
    defineUnitConversion cmd.Created cmd.ProductId cmd.FromUom cmd.ToUom
    <!> (UnitConversionId.create cmd.Id |> fromResult)
    <*> (PositiveDecimal.create cmd.Ratio |> fromResult)

let decide: DecideUnitConversion =
    fun command state ->
        match command, state with
        | DefineUnitConversion cmd, None ->
            createAggregate validateAndDefine (fun uconv -> [ UnitConversionDefined uconv ]) cmd

        | UpdateRatio cmd, Some s ->
            PositiveDecimal.create cmd.Ratio
            |> Result.map (fun ratio ->
                let update =
                    { s with
                        Ratio = ratio
                        Modified = Timestamp.now }

                { NewState = update
                  Events =
                    [ RatioUpdated(
                          { Id = update.Id
                            NewRatio = update.Ratio
                            Modified = update.Modified }
                      ) ] })

        | UpdateStatus(_, status), Some s ->
            match status with
            | Active when s.Status = Active -> Error(DomainError.invariant "UnitCoversion is already active")
            | Inactive when s.Status = Inactive -> Error(DomainError.invariant "UnitCoversion is already retired")
            | _ ->
                let updated =
                    { s with
                        Status = status
                        Modified = Timestamp.now }

                { NewState = updated
                  Events =
                    [ StatusUpdated(
                          { Id = updated.Id
                            NewStatus = status
                            Modified = updated.Modified }
                      ) ] }
                |> Ok

        | _ -> Error(DomainError.validation "Invalid command and state combination")

let evolve (state: UnitConversion option) (event: UnitConversionEvent) : UnitConversion option =
    match event, state with
    | UnitConversionDefined e, None -> Some e
    | RatioUpdated e, Some s ->
        { s with
            Ratio = e.NewRatio
            Modified = e.Modified }
        |> Some
    | StatusUpdated e, Some s ->
        { s with
            Status = e.NewStatus
            Modified = e.Modified }
        |> Some
    | _, _ -> state // Idempotent - conversion already exists
