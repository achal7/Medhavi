module Medhavi.MasterData.Domain.ResourceGroupAgg

open Medhavi.Common.Validation
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Validations
open Medhavi.SharedKernel.Aggregate

type ResourceGroup =
    { Id: ResourceGroupId
      PlantId: PlantId option
      Name: string
      Description: string option
      DefaultCalendarId: CalendarId option
      Created: Timestamp
      Modified: Timestamp
      Status: Status }

// Commands
type DefineResourceGroupCmd =
    { Id: string
      PlantId: string option
      Name: string
      Description: string option
      DefaultCalendarId: string option }

type RenameResourceGroupCmd = { Id: ResourceGroupId; NewName: string }
type RetireResourceGroupCmd = { Id: ResourceGroupId }

type ResourceGroupCommand =
    | DefineResourceGroup of DefineResourceGroupCmd
    | RenameResourceGroup of RenameResourceGroupCmd
    | RetireResourceGroup of RetireResourceGroupCmd

// Events
type ResourceGroupDefinedEvt =
    { Id: ResourceGroupId
      PlantId: PlantId option
      Name: string
      Description: string option
      DefaultCalendarId: CalendarId option
      Created: Timestamp }

type ResourceGroupRenamedEvt =
    { Id: ResourceGroupId
      NewName: string
      Modified: Timestamp }

type ResourceGroupRetiredEvt = { Id: ResourceGroupId; RetiredAt: Timestamp }

type ResourceGroupEvent =
    | ResourceGroupDefined of ResourceGroupDefinedEvt
    | ResourceGroupRenamed of ResourceGroupRenamedEvt
    | ResourceGroupRetired of ResourceGroupRetiredEvt

type DecideResourceGroup = Decide<ResourceGroup, ResourceGroupCommand, ResourceGroupEvent>
type EvolveResourceGroup = Evolve<ResourceGroup, ResourceGroupEvent>

let optionalStringId create idOpt =
    match idOpt with
    | None -> Ok None
    | Some id -> create id |> Result.map Some

let createResourceGroup id plantId name description defaultCalendarId =
    { Id = id
      PlantId = plantId
      Name = name
      Description = description
      DefaultCalendarId = defaultCalendarId
      Created = Timestamp.now
      Modified = Timestamp.now
      Status = Active }

let validateDefinedResourceGroup (cmd: DefineResourceGroupCmd) =
    createResourceGroup
    <!> (ResourceGroupId.create cmd.Id |> fromResult)
    <*> (optionalStringId PlantId.create cmd.PlantId |> fromResult)
    <*> required "Resource group name" cmd.Name
    <*> (Ok cmd.Description |> fromResult)
    <*> (optionalStringId CalendarId.create cmd.DefaultCalendarId |> fromResult)

let decide: DecideResourceGroup =
    fun command state ->
        match command, state with
        | DefineResourceGroup _, Some _ -> Error(DomainError.invariant ("Resource group already defined"))
        | DefineResourceGroup cmd, None ->
            createAggregate
                validateDefinedResourceGroup
                (fun rg ->
                    [ ResourceGroupDefined
                          { Id = rg.Id
                            PlantId = rg.PlantId
                            Name = rg.Name
                            Description = rg.Description
                            DefaultCalendarId = rg.DefaultCalendarId
                            Created = Timestamp.now } ])
                cmd
        | RenameResourceGroup cmd, Some rg ->
            match rg.Status with
            | Inactive -> Error(DomainError.invariant ("Cannot rename an inactive resource group"))
            | Active ->
                { NewState =
                    { rg with
                        Name = cmd.NewName
                        Modified = Timestamp.now }
                  Events =
                    [ ResourceGroupRenamed
                          { Id = rg.Id
                            NewName = cmd.NewName
                            Modified = Timestamp.now } ] }
                |> Ok
        | RetireResourceGroup cmd, Some rg ->
            match rg.Status with
            | Inactive -> Error(DomainError.invariant ("Resource group is already inactive"))
            | Active ->
                { NewState =
                    { rg with
                        Status = Inactive
                        Modified = Timestamp.now }
                  Events =
                    [ ResourceGroupRetired
                          { Id = rg.Id
                            RetiredAt = Timestamp.now } ] }
                |> Ok
        | _, None -> Error(DomainError.invariant ("Resource group is missing"))

let applyDefined (evt: ResourceGroupDefinedEvt) : ResourceGroup =
    { Id = evt.Id
      PlantId = evt.PlantId
      Name = evt.Name
      Description = evt.Description
      DefaultCalendarId = evt.DefaultCalendarId
      Created = evt.Created
      Modified = evt.Created
      Status = Active }

let applyRenamed (evt: ResourceGroupRenamedEvt) (state: ResourceGroup) : ResourceGroup =
    { state with
        Name = evt.NewName
        Modified = evt.Modified }

let applyRetired (evt: ResourceGroupRetiredEvt) (state: ResourceGroup) : ResourceGroup =
    { state with
        Status = Inactive
        Modified = evt.RetiredAt }

let evolve (state: ResourceGroup option) (event: ResourceGroupEvent) : ResourceGroup option =
    match event, state with
    | ResourceGroupDefined e, None -> Some(applyDefined e)
    | ResourceGroupRenamed e, Some s -> Some(applyRenamed e s)
    | ResourceGroupRetired e, Some s -> Some(applyRetired e s)
    | ResourceGroupRetired _, None -> None
    | _, current -> current
