module Medhavi.MasterData.Domain.UomAgg

open System.Text.Json.Serialization
open Medhavi.Common.Validation
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Validations
open Medhavi.SharedKernel.Aggregate

[<JsonFSharpConverter>]
type UomId = private UomId of string

module UomId =
    let create = IdsFactory.createExplicitId UomId "UomId"
    let value (UomId id) = id

type ConversionFactor =
    | Base of factor: PositiveDecimal
    | Derived of factor: PositiveDecimal

type UnitOfMeasure =
    { Id: UomId
      Code: string
      Name: string
      Status: Status
      ConversionFactor: ConversionFactor
      Created: Timestamp
      Modified: Timestamp }

// Commands
type DefineUnitOfMeasureCmd =
    { Code: string
      Name: string
      IsBase: bool
      ToBaseFactor: decimal
      Created: Timestamp }

type ChangeConversionFactorCmd =
    { Id: UomId
      NewIsBase: bool
      NewFactor: decimal }

type UnitOfMeasureCommand =
    | Define of DefineUnitOfMeasureCmd
    | ChangeConversionFactor of ChangeConversionFactorCmd
    | Retire of UomId
    | Activate of UomId

// Events
type UnitOfMeasureDefinedEvt = UnitOfMeasure

type UnitOfMeasureEvent =
    | UnitOfMeasureDefined of UnitOfMeasureDefinedEvt
    | ConversionFactorChanged of
        {| Id: UomId
           NewFactor: ConversionFactor
           Modified: Timestamp |}
    | UnitOfMeasureRetired of {| Id: UomId; Modified: Timestamp |}
    | UnitOfMeasureActivated of {| Id: UomId; Modified: Timestamp |}

// Signatures
type DecideUnitOfMeasure = Decide<UnitOfMeasure, UnitOfMeasureCommand, UnitOfMeasureEvent>
type EvolveUnitOfMeasure = Evolve<UnitOfMeasure, UnitOfMeasureEvent>

let createUoM now id code name factor =
    { Id = id
      Code = code
      Name = name
      Status = Active
      ConversionFactor = factor
      Created = now
      Modified = Timestamp.minValue }

let private baseFactorValidation isBase baseFactor =
    match isBase with
    | true -> PositiveDecimal.create 1.0m |> Result.map Base
    | false ->
        PositiveDecimal.create baseFactor
        |> Result.map Derived

let validateAndDefine (cmd: DefineUnitOfMeasureCmd) =
    createUoM cmd.Created
    <!> (UomId.create cmd.Code |> fromResult)
    <*> required "Code" cmd.Code
    <*> required "UnitName" cmd.Name
    <*> (baseFactorValidation cmd.IsBase cmd.ToBaseFactor
         |> fromResult)

let validateChangeFactor (cmd: ChangeConversionFactorCmd) = positiveDecimal "Conversion factor" cmd.NewFactor

let decide: DecideUnitOfMeasure =
    fun command stateOpt ->
        match command, stateOpt with
        | Define cmd, None -> createAggregate validateAndDefine (fun uconv -> [ UnitOfMeasureDefined uconv ]) cmd

        | Define _, Some _ -> Error(DomainError.invariant ("Unit of measure already defined"))
        | ChangeConversionFactor cmd, Some s ->
            baseFactorValidation cmd.NewIsBase cmd.NewFactor
            |> Result.map (fun factor ->
                let newstate =
                    { s with
                        ConversionFactor = factor
                        Modified = Timestamp.now }

                { NewState = newstate
                  Events =
                    [ ConversionFactorChanged
                          {| Id = cmd.Id
                             NewFactor = factor
                             Modified = newstate.Modified |} ] })
        | Activate id, Some s ->
            if s.Status = Active then
                Error(DomainError.invariant ("Unit of measure already active"))
            else
                let newstate =
                    { s with
                        Status = Active
                        Modified = Timestamp.now }

                { NewState = newstate
                  Events =
                    [ UnitOfMeasureActivated
                          {| Id = id
                             Modified = newstate.Modified |} ] }
                |> Ok
        | Retire id, Some s ->
            let newstate =
                { s with
                    Status = Inactive
                    Modified = Timestamp.now }

            { NewState = newstate
              Events =
                [ UnitOfMeasureRetired
                      {| Id = id
                         Modified = newstate.Modified |} ] }
            |> Ok
        | _, None -> Error(DomainError.invariant ("Unit of measure not defined"))

let evolve (state: UnitOfMeasure option) (event: UnitOfMeasureEvent) : UnitOfMeasure option =
    match event, state with
    | UnitOfMeasureDefined e, None -> Some(e)
    | ConversionFactorChanged e, Some s ->
        Some(
            { s with
                ConversionFactor = e.NewFactor
                Modified = e.Modified }
        )
    | UnitOfMeasureRetired e, Some s ->
        Some(
            { s with
                Status = Inactive
                Modified = e.Modified }
        )
    | UnitOfMeasureRetired _, None -> None
    | _, current -> current
