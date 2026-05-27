module Medhavi.MasterData.Domain.StockingPointAgg

open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Validations
open System.Text.Json.Serialization
open Medhavi.SharedKernel.Aggregate

[<JsonFSharpConverter>]
type StockingPointType =
    | Plant
    | DistributionCenter
    | Warehouse

type StockingPoint =
    { Id: StockingPointId
      PlantId: PlantId
      Code: string
      Name: string
      Type: StockingPointType
      Location: string option
      Level: int option
      PlanningLevel: int option
      SupplyCanBeSplit: bool
      Created: Timestamp
      Modified: Timestamp
      Status: Status }

// Commands
type DefineStockingPointCmd =
    { Id: StockingPointId
      PlantId: PlantId
      Code: string
      Name: string
      Type: StockingPointType
      Location: string option
      Level: int option
      PlanningLevel: int option
      SupplyCanBeSplit: bool }

type RenameStockingPointCmd =
    { Id: StockingPointId; NewName: string }

type StockingPointCommand =
    | DefineStockingPoint of DefineStockingPointCmd
    | RenameStockingPoint of RenameStockingPointCmd
    | RetireStockingPoint of id: StockingPointId

// Events
type StockingPointDefinedEvt = StockingPoint

type StockingPointRenamedEvt =
    { Id: StockingPointId
      NewName: string
      Modified: Timestamp }

type StockingPointRetiredEvt =
    { Id: StockingPointId
      RetiredAt: Timestamp }

type StockingPointEvent =
    | StockingPointDefined of StockingPointDefinedEvt
    | StockingPointRenamed of StockingPointRenamedEvt
    | StockingPointRetired of StockingPointRetiredEvt

// Signatures
type DecideStockingPoint = Decide<StockingPoint, StockingPointCommand, StockingPointEvent>
type EvolveStockingPoint = Evolve<StockingPoint, StockingPointEvent>

let createStockingPoint now plantId stype loc level plevel canSplit id code name =
    { Id = id
      PlantId = plantId
      Code = code
      Name = name
      Type = stype
      Location = loc
      Level = level
      PlanningLevel = plevel
      SupplyCanBeSplit = canSplit
      Created = now
      Modified = Timestamp.minValue
      Status = Active }

let validateStockingPoint now cmd =
    createStockingPoint now cmd.PlantId cmd.Type cmd.Location cmd.Level cmd.PlanningLevel cmd.SupplyCanBeSplit cmd.Id
    <!> required "Code" cmd.Code
    <*> required "Name" cmd.Name

let decide: DecideStockingPoint =
    fun command stateOpt ->
        match command, stateOpt with
        | DefineStockingPoint cmd, None ->
            let now = Timestamp.now

            createAggregate (validateStockingPoint now) (fun sp -> [ StockingPointDefined sp ]) cmd
        | DefineStockingPoint _, Some _ -> Error(DomainError.validation "StockingPoint already exists")

        | RenameStockingPoint cmd, Some state when state.Id = cmd.Id ->
            match state.Status with
            | Inactive -> Error(DomainError.invariant "Cannot rename an inactive StockingPoint")
            | Active ->
                { NewState =
                    { state with
                        Name = cmd.NewName
                        Modified = Timestamp.now }
                  Events =
                    [ StockingPointRenamed
                          { Id = state.Id
                            NewName = cmd.NewName
                            Modified = Timestamp.now } ] }
                |> Ok

        | RetireStockingPoint id, Some state when state.Id = id ->
            match state.Status with
            | Inactive -> Error(DomainError.invariant "StockingPoint is already inactive")
            | Active ->
                let updated =
                    { state with
                        Status = Inactive
                        Modified = Timestamp.now }

                { NewState = updated
                  Events =
                    [ StockingPointRetired
                          { Id = state.Id
                            RetiredAt = updated.Modified } ] }
                |> Ok
        | _, None -> Error(DomainError.validation "StockingPoint not found")
        | _, _ -> Error(DomainError.validation "Invalid command/state combination")

let applyDefined (evt: StockingPointDefinedEvt) : StockingPoint =
    { Id = evt.Id
      PlantId = evt.PlantId
      Code = evt.Code
      Name = evt.Name
      Type = evt.Type
      Location = evt.Location
      Level = evt.Level
      PlanningLevel = evt.PlanningLevel
      SupplyCanBeSplit = evt.SupplyCanBeSplit
      Created = evt.Created
      Modified = evt.Created
      Status = Active }

let applyRenamed (evt: StockingPointRenamedEvt) (state: StockingPoint) : StockingPoint =
    { state with
        Name = evt.NewName
        Modified = evt.Modified }

let applyRetired (evt: StockingPointRetiredEvt) (state: StockingPoint) : StockingPoint =
    { state with
        Status = Inactive
        Modified = evt.RetiredAt }

let evolve (state: StockingPoint option) (event: StockingPointEvent) : StockingPoint option =
    match event, state with
    | StockingPointDefined e, None -> Some(applyDefined e)
    | StockingPointRenamed e, Some s -> Some(applyRenamed e s)
    | StockingPointRetired e, Some s -> Some(applyRetired e s)
    | StockingPointRetired _, None -> None
    | _, current -> current
