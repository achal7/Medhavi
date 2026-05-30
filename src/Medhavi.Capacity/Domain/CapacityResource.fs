module Medhavi.Capacity.Domain.CapacityResourceAgg

open Medhavi.SharedKernel

type CapacityResource =
    { Id: PhysicalResourceId
      StandardResourceId: StandardResourceId
      ResourceGroupId: ResourceGroupId
      Name: string
      IsActive: bool
      EffectiveEfficiency: Percent
      EffectiveCostRate: Money option
      EffectiveCalendarId: CalendarId option
      Created: Timestamp
      Modified: Timestamp }

// Commands
type RegisterCapacityResourceCmd =
    { Id: PhysicalResourceId
      StandardResourceId: StandardResourceId
      ResourceGroupId: ResourceGroupId
      Name: string
      IsActive: bool
      EffectiveEfficiency: Percent
      EffectiveCostRate: Money option
      EffectiveCalendarId: CalendarId option }

type UpdateCapacityResourceCmd =
    { Id: PhysicalResourceId
      Name: string
      IsActive: bool
      EffectiveEfficiency: Percent
      EffectiveCostRate: Money option
      EffectiveCalendarId: CalendarId option }

type CapacityResourceCommand =
    | RegisterCapacityResource of RegisterCapacityResourceCmd
    | UpdateCapacityResource of UpdateCapacityResourceCmd

// Events
type CapacityResourceRegisteredEvt =
    { Id: PhysicalResourceId
      StandardResourceId: StandardResourceId
      ResourceGroupId: ResourceGroupId
      Name: string
      IsActive: bool
      EffectiveEfficiency: Percent
      EffectiveCostRate: Money option
      EffectiveCalendarId: CalendarId option
      Created: Timestamp }

type CapacityResourceUpdatedEvt =
    { Id: PhysicalResourceId
      Name: string
      IsActive: bool
      EffectiveEfficiency: Percent
      EffectiveCostRate: Money option
      EffectiveCalendarId: CalendarId option
      Modified: Timestamp }

type CapacityResourceEvent =
    | CapacityResourceRegistered of CapacityResourceRegisteredEvt
    | CapacityResourceUpdated of CapacityResourceUpdatedEvt

type DecideCapacityResource = Decide<CapacityResource, CapacityResourceCommand, CapacityResourceEvent>
type EvolveCapacityResource = Evolve<CapacityResource, CapacityResourceEvent>

let applyRegistered (evt: CapacityResourceRegisteredEvt) : CapacityResource =
    { Id = evt.Id
      StandardResourceId = evt.StandardResourceId
      ResourceGroupId = evt.ResourceGroupId
      Name = evt.Name
      IsActive = evt.IsActive
      EffectiveEfficiency = evt.EffectiveEfficiency
      EffectiveCostRate = evt.EffectiveCostRate
      EffectiveCalendarId = evt.EffectiveCalendarId
      Created = evt.Created
      Modified = evt.Created }

let applyUpdated (evt: CapacityResourceUpdatedEvt) (state: CapacityResource) : CapacityResource =
    { state with
        Name = evt.Name
        IsActive = evt.IsActive
        EffectiveEfficiency = evt.EffectiveEfficiency
        EffectiveCostRate = evt.EffectiveCostRate
        EffectiveCalendarId = evt.EffectiveCalendarId
        Modified = evt.Modified }

let evolve (state: CapacityResource option) (event: CapacityResourceEvent) : CapacityResource option =
    match event, state with
    | CapacityResourceRegistered e, None -> Some(applyRegistered e)
    | CapacityResourceUpdated e, Some s -> Some(applyUpdated e s)
    | CapacityResourceRegistered _, Some _ -> state
    | _, None -> None

let decide: DecideCapacityResource =
    fun cmd stateOpt ->
        match cmd, stateOpt with
        | RegisterCapacityResource cmd, None ->
            let evt =
                { Id = cmd.Id
                  StandardResourceId = cmd.StandardResourceId
                  ResourceGroupId = cmd.ResourceGroupId
                  Name = cmd.Name
                  IsActive = cmd.IsActive
                  EffectiveEfficiency = cmd.EffectiveEfficiency
                  EffectiveCostRate = cmd.EffectiveCostRate
                  EffectiveCalendarId = cmd.EffectiveCalendarId
                  Created = Timestamp.now }

            Ok(
                { NewState = applyRegistered evt
                  Events = [ CapacityResourceRegistered evt ] }
            )
        | RegisterCapacityResource _, Some _ -> Error(DomainError.invariant "Capacity resource already registered")
        | UpdateCapacityResource cmd, Some state ->
            let evt =
                { Id = cmd.Id
                  Name = cmd.Name
                  IsActive = cmd.IsActive
                  EffectiveEfficiency = cmd.EffectiveEfficiency
                  EffectiveCostRate = cmd.EffectiveCostRate
                  EffectiveCalendarId = cmd.EffectiveCalendarId
                  Modified = Timestamp.now }

            Ok(
                { NewState = applyUpdated evt state
                  Events = [ CapacityResourceUpdated evt ] }
            )
        | UpdateCapacityResource _, None -> Error(DomainError.invariant "Capacity resource not found")
