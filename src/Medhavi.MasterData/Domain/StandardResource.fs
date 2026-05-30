module Medhavi.MasterData.Domain.StandardResourceAgg

open Medhavi.Common.Validation
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Validations
open Medhavi.SharedKernel.Aggregate

type StandardResource =
    { Id: StandardResourceId
      ResourceGroupId: ResourceGroupId
      Name: string
      Description: string option
      DefaultEfficiency: Percent
      DefaultCostRate: Money option
      Created: Timestamp
      Modified: Timestamp
      Status: Status }

// Commands
type DefineStandardResourceCmd =
    { Id: string
      ResourceGroupId: string
      Name: string
      Description: string option
      DefaultEfficiency: decimal
      DefaultCostRateAmount: decimal option
      DefaultCostRateCurrency: string option }

type RenameStandardResourceCmd = { Id: StandardResourceId; NewName: string }
type RetireStandardResourceCmd = { Id: StandardResourceId }

type StandardResourceCommand =
    | DefineStandardResource of DefineStandardResourceCmd
    | RenameStandardResource of RenameStandardResourceCmd
    | RetireStandardResource of RetireStandardResourceCmd

// Events
type StandardResourceDefinedEvt =
    { Id: StandardResourceId
      ResourceGroupId: ResourceGroupId
      Name: string
      Description: string option
      DefaultEfficiency: Percent
      DefaultCostRate: Money option
      Created: Timestamp }

type StandardResourceRenamedEvt =
    { Id: StandardResourceId
      NewName: string
      Modified: Timestamp }

type StandardResourceRetiredEvt = { Id: StandardResourceId; RetiredAt: Timestamp }

type StandardResourceEvent =
    | StandardResourceDefined of StandardResourceDefinedEvt
    | StandardResourceRenamed of StandardResourceRenamedEvt
    | StandardResourceRetired of StandardResourceRetiredEvt

type DecideStandardResource = Decide<StandardResource, StandardResourceCommand, StandardResourceEvent>
type EvolveStandardResource = Evolve<StandardResource, StandardResourceEvent>

let createStandardResource id resourceGroupId name description defaultEfficiency defaultCostRate =
    { Id = id
      ResourceGroupId = resourceGroupId
      Name = name
      Description = description
      DefaultEfficiency = defaultEfficiency
      DefaultCostRate = defaultCostRate
      Created = Timestamp.now
      Modified = Timestamp.now
      Status = Active }

let validateDefinedStandardResource (cmd: DefineStandardResourceCmd) =
    let money =
        match cmd.DefaultCostRateAmount, cmd.DefaultCostRateCurrency with
        | Some amt, Some cur -> Some { Amount = amt; Currency = cur }
        | _ -> None

    createStandardResource
    <!> (StandardResourceId.create cmd.Id |> fromResult)
    <*> (ResourceGroupId.create cmd.ResourceGroupId |> fromResult)
    <*> required "Standard resource name" cmd.Name
    <*> (Ok cmd.Description |> fromResult)
    <*> (Percent.create cmd.DefaultEfficiency |> fromResult)
    <*> (Ok money |> fromResult)

let decide: DecideStandardResource =
    fun command state ->
        match command, state with
        | DefineStandardResource _, Some _ -> Error(DomainError.invariant ("Standard resource already defined"))
        | DefineStandardResource cmd, None ->
            createAggregate
                validateDefinedStandardResource
                (fun sr ->
                    [ StandardResourceDefined
                          { Id = sr.Id
                            ResourceGroupId = sr.ResourceGroupId
                            Name = sr.Name
                            Description = sr.Description
                            DefaultEfficiency = sr.DefaultEfficiency
                            DefaultCostRate = sr.DefaultCostRate
                            Created = Timestamp.now } ])
                cmd
        | RenameStandardResource cmd, Some sr ->
            match sr.Status with
            | Inactive -> Error(DomainError.invariant ("Cannot rename an inactive standard resource"))
            | Active ->
                { NewState =
                    { sr with
                        Name = cmd.NewName
                        Modified = Timestamp.now }
                  Events =
                    [ StandardResourceRenamed
                          { Id = sr.Id
                            NewName = cmd.NewName
                            Modified = Timestamp.now } ] }
                |> Ok
        | RetireStandardResource cmd, Some sr ->
            match sr.Status with
            | Inactive -> Error(DomainError.invariant ("Standard resource is already inactive"))
            | Active ->
                { NewState =
                    { sr with
                        Status = Inactive
                        Modified = Timestamp.now }
                  Events =
                    [ StandardResourceRetired
                          { Id = sr.Id
                            RetiredAt = Timestamp.now } ] }
                |> Ok
        | _, None -> Error(DomainError.invariant ("Standard resource is missing"))

let applyDefined (evt: StandardResourceDefinedEvt) : StandardResource =
    { Id = evt.Id
      ResourceGroupId = evt.ResourceGroupId
      Name = evt.Name
      Description = evt.Description
      DefaultEfficiency = evt.DefaultEfficiency
      DefaultCostRate = evt.DefaultCostRate
      Created = evt.Created
      Modified = evt.Created
      Status = Active }

let applyRenamed (evt: StandardResourceRenamedEvt) (state: StandardResource) : StandardResource =
    { state with
        Name = evt.NewName
        Modified = evt.Modified }

let applyRetired (evt: StandardResourceRetiredEvt) (state: StandardResource) : StandardResource =
    { state with
        Status = Inactive
        Modified = evt.RetiredAt }

let evolve (state: StandardResource option) (event: StandardResourceEvent) : StandardResource option =
    match event, state with
    | StandardResourceDefined e, None -> Some(applyDefined e)
    | StandardResourceRenamed e, Some s -> Some(applyRenamed e s)
    | StandardResourceRetired e, Some s -> Some(applyRetired e s)
    | StandardResourceRetired _, None -> None
    | _, current -> current
