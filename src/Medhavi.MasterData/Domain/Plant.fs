module Medhavi.MasterData.Domain.PlantAgg

open Medhavi.Common.Validation
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Validations
open Medhavi.SharedKernel.Aggregate

type Plant =
    { Id: PlantId
      Code: string
      Name: string
      Created: Timestamp
      Modified: Timestamp
      Status: Status }

// Commands
type DefinePlantCmd =
    { Id: string
      Code: string
      Name: string }

type RenamePlantCmd = { Id: PlantId; NewName: string }

type RetirePlantCmd = { Id: PlantId }

type PlantCommand =
    | DefinePlant of DefinePlantCmd
    | RenamePlant of RenamePlantCmd
    | RetirePlant of RetirePlantCmd

// Events
type PlantDefinedEvt =
    { Id: PlantId
      Code: string
      Name: string
      Created: Timestamp }

type PlantRenamedEvt =
    { Id: PlantId
      NewName: string
      Modified: Timestamp }

type PlantRetiredEvt = { Id: PlantId; RetiredAt: Timestamp }

type PlantEvent =
    | PlantDefined of PlantDefinedEvt
    | PlantRenamed of PlantRenamedEvt
    | PlantRetired of PlantRetiredEvt

// Signatures
type DecidePlant = Decide<Plant, PlantCommand, PlantEvent>
type EvolvePlant = Evolve<Plant, PlantEvent>

let createPlant id code name =
    { Id = id
      Code = code
      Name = name
      Created = Timestamp.now
      Modified = Timestamp.now
      Status = Active }

let validateDefinedPlant (cmd: DefinePlantCmd) =
    createPlant
    <!> (PlantId.create cmd.Id |> fromResult)
    <*> required "Plant code" cmd.Code
    <*> required "Plant name" cmd.Name

let decide: DecidePlant =
    fun command state ->
        match command, state with
        | DefinePlant _, Some _ -> Error(DomainError.invariant ("Plant already defined"))
        | DefinePlant cmd, None ->
            createAggregate
                validateDefinedPlant
                (fun plant ->
                    [ PlantDefined
                          { Id = plant.Id
                            Code = plant.Code
                            Name = plant.Name
                            Created = Timestamp.now } ])
                cmd
        | RenamePlant cmd, Some plant ->
            match plant.Status with
            | Inactive -> Error(DomainError.invariant ("Cannot rename an inactive plant"))
            | Active ->
                { NewState =
                    { plant with
                        Name = cmd.NewName
                        Modified = Timestamp.now }
                  Events =
                    [ PlantRenamed
                          { Id = plant.Id
                            NewName = cmd.NewName
                            Modified = Timestamp.now } ] }
                |> Ok

        | RetirePlant cmd, Some plant ->
            match plant.Status with
            | Inactive -> Error(DomainError.invariant ("Plant is already inactive"))
            | Active ->
                { NewState =
                    { plant with
                        Status = Inactive
                        Modified = Timestamp.now }
                  Events =
                    [ PlantRetired
                          { Id = plant.Id
                            RetiredAt = Timestamp.now } ] }
                |> Ok
        | _, None -> Error(DomainError.invariant ("Plant is missing"))

let applyDefined (evt: PlantDefinedEvt) : Plant =
    { Id = evt.Id
      Code = evt.Code
      Name = evt.Name
      Created = evt.Created
      Modified = evt.Created
      Status = Active }

let applyRenamed (evt: PlantRenamedEvt) (state: Plant) : Plant =
    { state with
        Name = evt.NewName
        Modified = evt.Modified }

let applyRetired (evt: PlantRetiredEvt) (state: Plant) : Plant =
    { state with
        Status = Inactive
        Modified = evt.RetiredAt }

let evolve (state: Plant option) (event: PlantEvent) : Plant option =
    match event, state with
    | PlantDefined e, None -> Some(applyDefined e)
    | PlantRenamed e, Some s -> Some(applyRenamed e s)
    | PlantRetired e, Some s -> Some(applyRetired e s)
    | PlantRetired _, None -> None
    | _, current -> current
