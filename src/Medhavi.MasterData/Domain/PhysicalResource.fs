module Medhavi.MasterData.Domain.PhysicalResourceAgg

open Medhavi.Common.Validation
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Validations
open Medhavi.SharedKernel.Aggregate

type PhysicalResource =
    { Id: PhysicalResourceId
      StandardResourceId: StandardResourceId
      Name: string
      SerialNumber: string option
      Location: string option
      EfficiencyOverride: Percent option
      CostRateOverride: Money option
      CalendarId: CalendarId option
      Created: Timestamp
      Modified: Timestamp
      Status: Status }

// Commands
type DefinePhysicalResourceCmd =
    { Id: string
      StandardResourceId: string
      Name: string
      SerialNumber: string option
      Location: string option
      EfficiencyOverride: decimal option
      CostRateOverrideAmount: decimal option
      CostRateOverrideCurrency: string option
      CalendarId: string option }

type RenamePhysicalResourceCmd = { Id: PhysicalResourceId; NewName: string }
type RetirePhysicalResourceCmd = { Id: PhysicalResourceId }

type PhysicalResourceCommand =
    | DefinePhysicalResource of DefinePhysicalResourceCmd
    | RenamePhysicalResource of RenamePhysicalResourceCmd
    | RetirePhysicalResource of RetirePhysicalResourceCmd

// Events
type PhysicalResourceDefinedEvt =
    { Id: PhysicalResourceId
      StandardResourceId: StandardResourceId
      Name: string
      SerialNumber: string option
      Location: string option
      EfficiencyOverride: Percent option
      CostRateOverride: Money option
      CalendarId: CalendarId option
      Created: Timestamp }

type PhysicalResourceRenamedEvt =
    { Id: PhysicalResourceId
      NewName: string
      Modified: Timestamp }

type PhysicalResourceRetiredEvt = { Id: PhysicalResourceId; RetiredAt: Timestamp }

type PhysicalResourceEvent =
    | PhysicalResourceDefined of PhysicalResourceDefinedEvt
    | PhysicalResourceRenamed of PhysicalResourceRenamedEvt
    | PhysicalResourceRetired of PhysicalResourceRetiredEvt

type DecidePhysicalResource = Decide<PhysicalResource, PhysicalResourceCommand, PhysicalResourceEvent>
type EvolvePhysicalResource = Evolve<PhysicalResource, PhysicalResourceEvent>

let optionalStringId create idOpt =
    match idOpt with
    | None -> Ok None
    | Some id -> create id |> Result.map Some

let optionalPercent percentOpt =
    match percentOpt with
    | None -> Ok None
    | Some p -> Percent.create p |> Result.map Some

let createPhysicalResource id standardResourceId name serialNumber location efficiencyOverride costRateOverride calendarId =
    { Id = id
      StandardResourceId = standardResourceId
      Name = name
      SerialNumber = serialNumber
      Location = location
      EfficiencyOverride = efficiencyOverride
      CostRateOverride = costRateOverride
      CalendarId = calendarId
      Created = Timestamp.now
      Modified = Timestamp.now
      Status = Active }

let validateDefinedPhysicalResource (cmd: DefinePhysicalResourceCmd) =
    let money =
        match cmd.CostRateOverrideAmount, cmd.CostRateOverrideCurrency with
        | Some amt, Some cur -> Some { Amount = amt; Currency = cur }
        | _ -> None

    createPhysicalResource
    <!> (PhysicalResourceId.create cmd.Id |> fromResult)
    <*> (StandardResourceId.create cmd.StandardResourceId |> fromResult)
    <*> required "Physical resource name" cmd.Name
    <*> (Ok cmd.SerialNumber |> fromResult)
    <*> (Ok cmd.Location |> fromResult)
    <*> (optionalPercent cmd.EfficiencyOverride |> fromResult)
    <*> (Ok money |> fromResult)
    <*> (optionalStringId CalendarId.create cmd.CalendarId |> fromResult)

let decide: DecidePhysicalResource =
    fun command state ->
        match command, state with
        | DefinePhysicalResource _, Some _ -> Error(DomainError.invariant ("Physical resource already defined"))
        | DefinePhysicalResource cmd, None ->
            createAggregate
                validateDefinedPhysicalResource
                (fun pr ->
                    [ PhysicalResourceDefined
                          { Id = pr.Id
                            StandardResourceId = pr.StandardResourceId
                            Name = pr.Name
                            SerialNumber = pr.SerialNumber
                            Location = pr.Location
                            EfficiencyOverride = pr.EfficiencyOverride
                            CostRateOverride = pr.CostRateOverride
                            CalendarId = pr.CalendarId
                            Created = Timestamp.now } ])
                cmd
        | RenamePhysicalResource cmd, Some pr ->
            match pr.Status with
            | Inactive -> Error(DomainError.invariant ("Cannot rename an inactive physical resource"))
            | Active ->
                { NewState =
                    { pr with
                        Name = cmd.NewName
                        Modified = Timestamp.now }
                  Events =
                    [ PhysicalResourceRenamed
                          { Id = pr.Id
                            NewName = cmd.NewName
                            Modified = Timestamp.now } ] }
                |> Ok
        | RetirePhysicalResource cmd, Some pr ->
            match pr.Status with
            | Inactive -> Error(DomainError.invariant ("Physical resource is already inactive"))
            | Active ->
                { NewState =
                    { pr with
                        Status = Inactive
                        Modified = Timestamp.now }
                  Events =
                    [ PhysicalResourceRetired
                          { Id = pr.Id
                            RetiredAt = Timestamp.now } ] }
                |> Ok
        | _, None -> Error(DomainError.invariant ("Physical resource is missing"))

let applyDefined (evt: PhysicalResourceDefinedEvt) : PhysicalResource =
    { Id = evt.Id
      StandardResourceId = evt.StandardResourceId
      Name = evt.Name
      SerialNumber = evt.SerialNumber
      Location = evt.Location
      EfficiencyOverride = evt.EfficiencyOverride
      CostRateOverride = evt.CostRateOverride
      CalendarId = evt.CalendarId
      Created = evt.Created
      Modified = evt.Created
      Status = Active }

let applyRenamed (evt: PhysicalResourceRenamedEvt) (state: PhysicalResource) : PhysicalResource =
    { state with
        Name = evt.NewName
        Modified = evt.Modified }

let applyRetired (evt: PhysicalResourceRetiredEvt) (state: PhysicalResource) : PhysicalResource =
    { state with
        Status = Inactive
        Modified = evt.RetiredAt }

let evolve (state: PhysicalResource option) (event: PhysicalResourceEvent) : PhysicalResource option =
    match event, state with
    | PhysicalResourceDefined e, None -> Some(applyDefined e)
    | PhysicalResourceRenamed e, Some s -> Some(applyRenamed e s)
    | PhysicalResourceRetired e, Some s -> Some(applyRetired e s)
    | PhysicalResourceRetired _, None -> None
    | _, current -> current
