module Medhavi.Supply.Domain.SupplierAgg

open System
open Medhavi.Common
open Medhavi.SharedKernel

// Supplier classification/type
type SupplierType =
    | Strategic // Key strategic supplier
    | Preferred // Preferred supplier
    | Standard // Standard supplier

// Geographic region served by supplier
type RegionId = RegionId of string

type Status =
    | Active
    | InActive

// Core Supplier aggregate
type Supplier =
    { Id: SupplierId
      Code: string
      Name: string
      SupplierType: SupplierType
      Regions: RegionId list
      Status: Status
      Created: Timestamp
      Modified: Timestamp }

// =================================================================================================
// SUPPLIER COMMANDS
// =================================================================================================

// Create a new supplier
type DefineSupplierCmd =
    { Id: SupplierId
      Code: string
      Name: string
      SupplierType: SupplierType
      Regions: RegionId list }

// Update supplier basic information
type UpdateSupplierInfoCmd =
    { Id: SupplierId
      Name: string option
      SupplierType: SupplierType option
      Modified: Timestamp }

// Activate/Deactivate supplier
type ChangeSupplierStatusCmd =
    { Id: SupplierId
      IsActive: bool
      Reason: string option
      Modified: Timestamp }

// Add geographic region to supplier
type AddSupplierRegionCmd =
    { Id: SupplierId
      RegionId: RegionId
      Modified: Timestamp }

// Remove geographic region from supplier
type RemoveSupplierRegionCmd =
    { Id: SupplierId
      RegionId: RegionId
      Modified: Timestamp }

// Discriminated union of all supplier commands
type SupplierCommand =
    | DefineSupplier of DefineSupplierCmd
    | UpdateSupplierInfo of UpdateSupplierInfoCmd
    | ChangeSupplierStatus of ChangeSupplierStatusCmd
    | AddSupplierRegion of AddSupplierRegionCmd
    | RemoveSupplierRegion of RemoveSupplierRegionCmd

// =================================================================================================
// SUPPLIER EVENTS
// =================================================================================================

// Supplier created event
type SupplierCreatedEvt = { Result: Supplier }

// Supplier information updated
type SupplierInfoUpdatedEvt =
    { Id: SupplierId
      Name: string option
      SupplierType: SupplierType option
      Modified: Timestamp }

// Supplier status changed
type SupplierStatusChangedEvt =
    { Id: SupplierId
      IsActive: bool
      Reason: string option
      Modified: Timestamp }

// Supplier region added
type SupplierRegionAddedEvt =
    { Id: SupplierId
      RegionId: RegionId
      Modified: Timestamp }

// Supplier region removed
type SupplierRegionRemovedEvt =
    { Id: SupplierId
      RegionId: RegionId
      Modified: Timestamp }

// Discriminated union of all supplier events
type SupplierEvent =
    | SupplierCreated of SupplierCreatedEvt
    | SupplierInfoUpdated of SupplierInfoUpdatedEvt
    | SupplierStatusChanged of SupplierStatusChangedEvt
    | SupplierRegionAdded of SupplierRegionAddedEvt
    | SupplierRegionRemoved of SupplierRegionRemovedEvt

// =================================================================================================
// SUPPLIER DOMAIN LOGIC SIGNATURES
// =================================================================================================

// Decision function signature
type DecideSupplier = Decide<Supplier, SupplierCommand, SupplierEvent>

// Evolution function signature
type EvolveSupplier = Evolve<Supplier, SupplierEvent>

// =================================================================================================
// LEGACY SUPPLIER OFFER (TO BE KEPT FOR COMPATIBILITY)
// =================================================================================================

/// Supplier offer model (scaffold for Phase 7 – Supplier Management).
/// TODO: add events/commands/projection for supplier offers (MOQ, lot size, lead times, price tiers, reliability, incoterms).
type SupplierOffer =
    { SupplierId: string
      SkuId: SkuId
      StockingPointId: StockingPointId option
      Moq: decimal option
      LotSize: decimal option
      LeadTimeP50: TimeSpan option
      LeadTimeP95: TimeSpan option
      Price: decimal option
      Currency: string option
      Reliability: decimal option
      Incoterm: string option
      PriceTier: string option
      CreatedDate: Timestamp
      ModifiedDate: Timestamp }

// Validation functions (includes business rules)
let validateCreate (cmd: DefineSupplierCmd) : Result<unit, DomainError> =
    // Basic input validation
    if String.IsNullOrWhiteSpace cmd.Code then
        Error(DomainError.validation "Supplier code cannot be empty")
    elif String.IsNullOrWhiteSpace cmd.Name then
        Error(DomainError.validation "Supplier name cannot be empty")
    elif cmd.Regions.Length = 0 then
        Error(DomainError.validation "Supplier must have at least one region")
    else
        Ok()

let validateUpdateInfo (cmd: UpdateSupplierInfoCmd) : Result<unit, DomainError> =
    // Validate that at least one field is being updated
    match cmd.Name, cmd.SupplierType with
    | None, None -> Error(DomainError.validation "At least one field must be updated")
    | Some name, _ when String.IsNullOrWhiteSpace name -> Error(DomainError.validation "Supplier name cannot be empty")
    | _ -> Ok()

let validateChangeStatus (_cmd: ChangeSupplierStatusCmd) : Result<unit, DomainError> =
    // Status changes are always allowed
    Ok()

let validateAddRegion (_cmd: AddSupplierRegionCmd) : Result<unit, DomainError> =
    // Basic validation - region format could be added here if needed
    Ok()

let validateRemoveRegion (_cmd: RemoveSupplierRegionCmd) : Result<unit, DomainError> =
    // Basic validation - could check if supplier has at least one region remaining
    Ok()

// State evolution functions (pure state transitions)
// Note: ID is validated in Application layer before event creation
let applyCreated (evt: SupplierCreatedEvt) : Supplier =
    let result = evt.Result

    { Id = result.Id
      Code = result.Code
      Name = result.Name
      SupplierType = result.SupplierType
      Regions = result.Regions
      Status = Active // New suppliers start as active
      Created = result.Created
      Modified = result.Modified }

let applyInfoUpdated (evt: SupplierInfoUpdatedEvt) (state: Supplier) : Supplier =
    { state with
        Name = evt.Name |> Option.defaultValue state.Name
        SupplierType =
            evt.SupplierType
            |> Option.defaultValue state.SupplierType
        Modified = evt.Modified }

let applyStatusChanged (evt: SupplierStatusChangedEvt) (state: Supplier) : Supplier =
    { state with
        Status =
            match evt.IsActive with
            | true -> Active
            | false -> InActive
        Modified = evt.Modified }

let applyRegionAdded (evt: SupplierRegionAddedEvt) (state: Supplier) : Supplier =
    let newRegions = state.Regions @ [ evt.RegionId ]

    { state with
        Regions = newRegions
        Modified = evt.Modified }

let applyRegionRemoved (evt: SupplierRegionRemovedEvt) (state: Supplier) : Supplier =
    let newRegions =
        state.Regions
        |> List.filter (fun r -> r <> evt.RegionId)

    { state with
        Regions = newRegions
        Modified = evt.Modified }

let evolve (state: Supplier option) (event: SupplierEvent) : Supplier option =
    match event, state with
    | SupplierCreated e, None -> Some(applyCreated e)
    | SupplierInfoUpdated e, Some s -> Some(applyInfoUpdated e s)
    | SupplierStatusChanged e, Some s -> Some(applyStatusChanged e s)
    | SupplierRegionAdded e, Some s -> Some(applyRegionAdded e s)
    | SupplierRegionRemoved e, Some s -> Some(applyRegionRemoved e s)
    | SupplierCreated _, Some _ -> state // Idempotent - supplier already exists
    | _, None -> None // Can't apply updates to non-existent supplier

let decide: DecideSupplier =
    fun cmd state ->
        match cmd with
        | DefineSupplier c ->
            match validateCreate c with
            | Error e -> Error e
            | Ok _ ->
                let supplier: Supplier =
                    { Id = c.Id
                      Code = c.Code
                      Name = c.Name
                      SupplierType = c.SupplierType
                      Regions = c.Regions
                      Status = Active
                      Created = Timestamp.now
                      Modified = Timestamp.now }

                let evt = SupplierCreated { Result = supplier }

                { NewState = supplier
                  Events = [ evt ] }
                |> Ok

        | UpdateSupplierInfo c ->
            match validateUpdateInfo c with
            | Error e -> Error e
            | Ok _ ->
                let evt =
                    SupplierInfoUpdated
                        { Id = c.Id
                          Name = c.Name
                          SupplierType = c.SupplierType
                          Modified = c.Modified }

                evolve state evt
                |> Option.map (fun supplier ->
                    { NewState = supplier
                      Events = [ evt ] }
                    |> Ok)
                |> Option.defaultWith (fun () -> Error(DomainError.validation "Supplier not updated"))
        | ChangeSupplierStatus c ->
            match validateChangeStatus c with
            | Error e -> Error e
            | Ok _ ->
                let evt =
                    SupplierStatusChanged
                        { Id = c.Id
                          IsActive = c.IsActive
                          Reason = c.Reason
                          Modified = c.Modified }

                evolve state evt
                |> Option.map (fun supplier ->
                    { NewState = supplier
                      Events = [ evt ] }
                    |> Ok)
                |> Option.defaultWith (fun () -> Error(DomainError.validation "Supplier not updated"))

        | AddSupplierRegion c ->
            match validateAddRegion c with
            | Error e -> Error e
            | Ok _ ->
                let evt =
                    SupplierRegionAdded
                        { Id = c.Id
                          RegionId = c.RegionId
                          Modified = c.Modified }

                evolve state evt
                |> Option.map (fun supplier ->
                    { NewState = supplier
                      Events = [ evt ] }
                    |> Ok)
                |> Option.defaultWith (fun () -> Error(DomainError.validation "Supplier region not added"))
        | RemoveSupplierRegion c ->
            match validateRemoveRegion c with
            | Error e -> Error e
            | Ok _ ->
                let evt =
                    SupplierRegionRemoved
                        { Id = c.Id
                          RegionId = c.RegionId
                          Modified = c.Modified }

                evolve state evt
                |> Option.map (fun supplier ->
                    { NewState = supplier
                      Events = [ evt ] }
                    |> Ok)
                |> Option.defaultWith (fun () -> Error(DomainError.validation "Supplier region not removed"))
