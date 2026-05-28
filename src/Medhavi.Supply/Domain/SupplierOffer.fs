module Medhavi.Supply.Domain.SupplierOfferAgg

open System
open System.Text.Json.Serialization
open Medhavi.Common
open Medhavi.SharedKernel

/// Price tier for supplier offers
[<JsonFSharpConverter>]
type PriceTier =
    { TierNumber: int
      MinQuantity: Quantity
      MaxQuantity: Quantity option
      PricePerUnit: decimal
      Currency: string }

/// Incoterms for supplier offers
[<JsonFSharpConverter>]
type Incoterm =
    | FOB // Free On Board
    | CIF // Cost, Insurance, Freight
    | EXW // Ex Works
    | DDP // Delivered Duty Paid
    | Other of string

    member this.displayString: string =
        match this with
        | FOB -> "FOB"
        | CIF -> "CIF"
        | EXW -> "EXW"
        | DDP -> "DDP"
        | Other s -> s

    static member parse(input: string) =
        let normalized = input.Trim().ToUpperInvariant()

        match normalized with
        | "FOB" -> FOB
        | "CIF" -> CIF
        | "EXW" -> EXW
        | "DDP" -> DDP
        | _ -> Other input

/// Supplier capacity window
[<JsonFSharpConverter>]
type SupplierCapacityWindow =
    { WindowId: string
      StartDate: Timestamp
      EndDate: Timestamp
      MaxQuantity: Quantity
      AvailableQuantity: Quantity }

[<JsonFSharpConverter>]
type SupplierOfferId = private SupplierOfferId of string

module SupplierOfferId =
    let create = IdsFactory.createExplicitId SupplierOfferId "SupplierOfferId"
    let value (SupplierOfferId id) = id

/// Supplier Offer aggregate
/// Represents a supplier's offer for a specific product/stocking point combination
type SupplierOffer =
    { Id: SupplierOfferId
      SupplierId: SupplierId
      SkuId: SkuId
      StockingPointId: StockingPointId option
      Moq: decimal option // Minimum Order Quantity
      LotSize: Quantity option
      LeadTimeP50: TimeSpan option // Median lead time
      LeadTimeP95: TimeSpan option // 95th percentile lead time
      PriceTiers: PriceTier list
      Reliability: Percent option // 0.0-1.0
      Incoterm: Incoterm option
      CapacityWindows: SupplierCapacityWindow list
      IsActive: bool
      CreatedDate: Timestamp
      ModifiedDate: Timestamp }

// =================================================================================================
// SUPPLIER OFFER COMMANDS
// =================================================================================================

type PriceTierCmd =
    { MinQuantity: decimal
      MaxQuantity: decimal option
      PricePerUnit: decimal
      Currency: string }

type CapacityWindowCmd =
    { WindowId: string
      StartDate: Timestamp
      EndDate: Timestamp
      MaxQuantity: decimal
      AvailableQuantity: decimal }

/// Create a new supplier offer
type DefineSupplierOfferCmd =
    { Id: string
      SupplierId: SupplierId
      SkuId: SkuId
      StockingPointId: StockingPointId option
      Moq: decimal option
      LotSize: decimal option
      LeadTimeP50: TimeSpan option
      LeadTimeP95: TimeSpan option
      PriceTiers: PriceTierCmd list
      Reliability: decimal option
      Incoterm: Incoterm option
      CapacityWindows: CapacityWindowCmd list
      CreatedDate: Timestamp }

/// Update supplier offer
type UpdateSupplierOfferCmd =
    { Id: SupplierOfferId
      Moq: decimal option
      LotSize: decimal option
      LeadTimeP50: TimeSpan option
      LeadTimeP95: TimeSpan option
      PriceTiers: PriceTierCmd list option
      Reliability: decimal option
      Incoterm: Incoterm option
      CapacityWindows: CapacityWindowCmd list option
      ModifiedDate: Timestamp }

/// Delete supplier offer
type RevokeSupplierOfferCmd =
    { Id: SupplierOfferId
      DeletedDate: Timestamp }

/// Activate/Deactivate supplier offer
type ChangeSupplierOfferStatusCmd =
    { Id: SupplierOfferId
      IsActive: bool
      ModifiedDate: Timestamp }

/// Discriminated union of all supplier offer commands
type SupplierOfferCommand =
    | DefineSupplierOffer of DefineSupplierOfferCmd
    | UpdateSupplierOffer of UpdateSupplierOfferCmd
    | RevokeSupplierOffer of RevokeSupplierOfferCmd
    | ChangeSupplierOfferStatus of ChangeSupplierOfferStatusCmd

// =================================================================================================
// SUPPLIER OFFER EVENTS
// =================================================================================================

/// Supplier offer created event
type SupplierOfferDefinedEvt =
    { Id: SupplierOfferId
      SupplierId: SupplierId
      SkuId: SkuId
      StockingPointId: StockingPointId option
      Moq: decimal option
      LotSize: Quantity option
      LeadTimeP50: TimeSpan option
      LeadTimeP95: TimeSpan option
      PriceTiers: PriceTier list
      Reliability: Percent option
      Incoterm: Incoterm option
      CapacityWindows: SupplierCapacityWindow list
      CreatedDate: Timestamp }

/// Supplier offer updated event
type SupplierOfferUpdatedEvt =
    { Id: SupplierOfferId
      Moq: decimal option
      LotSize: Quantity option
      LeadTimeP50: TimeSpan option
      LeadTimeP95: TimeSpan option
      PriceTiers: PriceTier list option
      Reliability: Percent option
      Incoterm: Incoterm option
      CapacityWindows: SupplierCapacityWindow list option
      ModifiedDate: Timestamp }

/// Supplier offer deleted event
type SupplierOfferRevokedEvt =
    { Id: SupplierOfferId
      DeletedDate: Timestamp }

/// Supplier offer status changed event
type SupplierOfferStatusChangedEvt =
    { Id: SupplierOfferId
      IsActive: bool
      ModifiedDate: Timestamp }

/// Discriminated union of all supplier offer events
type SupplierOfferEvent =
    | SupplierOfferDefined of SupplierOfferDefinedEvt
    | SupplierOfferUpdated of SupplierOfferUpdatedEvt
    | SupplierOfferRevoked of SupplierOfferRevokedEvt
    | SupplierOfferStatusChanged of SupplierOfferStatusChangedEvt

// =================================================================================================
// SUPPLIER OFFER DOMAIN LOGIC SIGNATURES
// =================================================================================================

/// Decision function signature
type DecideSupplierOffer = Decide<SupplierOffer, SupplierOfferCommand, SupplierOfferEvent>

/// Evolution function signature
type EvolveSupplierOffer = Evolve<SupplierOffer, SupplierOfferEvent>

// =================================================================================================
// VALIDATION FUNCTIONS
// =================================================================================================

/// Validate create supplier offer command
let validateDefine (cmd: DefineSupplierOfferCmd) : Result<unit, DomainError> =
    // Basic input validation
    if String.IsNullOrWhiteSpace cmd.Id then
        Error(DomainError.validation "SupplierOffer ID cannot be empty")
    elif cmd.PriceTiers.IsEmpty then
        Error(DomainError.validation "SupplierOffer must have at least one price tier")
    elif
        cmd.PriceTiers
        |> List.exists (fun tier -> tier.MinQuantity < 0m)
    then
        Error(DomainError.validation "Price tier MinQuantity cannot be negative")
    elif
        cmd.PriceTiers
        |> List.exists (fun tier -> tier.PricePerUnit < 0m)
    then
        Error(DomainError.validation "Price tier PricePerUnit cannot be negative")
    elif
        cmd.Reliability.IsSome
        && PositiveDecimal.create cmd.Reliability.Value
           |> Result.isError
    then
        Error(DomainError.validation "Reliability must be between 0.0 and 1.0")
    else
        Ok()

/// Validate update supplier offer command
let validateUpdate (cmd: UpdateSupplierOfferCmd) : Result<unit, DomainError> =
    // Validate price tiers if provided
    match cmd.PriceTiers with
    | Some tiers when tiers.IsEmpty -> Error(DomainError.validation "Price tiers cannot be empty")
    | Some tiers when
        tiers
        |> List.exists (fun tier -> tier.MinQuantity < 0m)
        ->
        Error(DomainError.validation "Price tier MinQuantity cannot be negative")
    | Some tiers when
        tiers
        |> List.exists (fun tier -> tier.PricePerUnit < 0m)
        ->
        Error(DomainError.validation "Price tier PricePerUnit cannot be negative")
    | _ -> Ok()

/// Validate change status command
let validateChangeStatus (_cmd: ChangeSupplierOfferStatusCmd) : Result<unit, DomainError> =
    // Status changes are always allowed
    Ok()

// =================================================================================================
// STATE EVOLUTION FUNCTIONS
// =================================================================================================

/// Apply supplier offer created event
let applyDefined (evt: SupplierOfferDefinedEvt) : SupplierOffer =
    { Id = evt.Id
      SupplierId = evt.SupplierId
      SkuId = evt.SkuId
      StockingPointId = evt.StockingPointId
      Moq = evt.Moq
      LotSize = evt.LotSize
      LeadTimeP50 = evt.LeadTimeP50
      LeadTimeP95 = evt.LeadTimeP95
      PriceTiers = evt.PriceTiers
      Reliability = evt.Reliability
      Incoterm = evt.Incoterm
      CapacityWindows = evt.CapacityWindows
      IsActive = true // New offers start as active
      CreatedDate = evt.CreatedDate
      ModifiedDate = evt.CreatedDate }

/// Apply supplier offer updated event
let applyUpdated (evt: SupplierOfferUpdatedEvt) (state: SupplierOffer) : SupplierOffer =
    { state with
        Moq = evt.Moq |> Option.orElse state.Moq
        LotSize = evt.LotSize |> Option.orElse state.LotSize
        LeadTimeP50 = evt.LeadTimeP50 |> Option.orElse state.LeadTimeP50
        LeadTimeP95 = evt.LeadTimeP95 |> Option.orElse state.LeadTimeP95
        PriceTiers =
            evt.PriceTiers
            |> Option.defaultValue state.PriceTiers
        Reliability = evt.Reliability |> Option.orElse state.Reliability
        Incoterm = evt.Incoterm |> Option.orElse state.Incoterm
        CapacityWindows =
            evt.CapacityWindows
            |> Option.defaultValue state.CapacityWindows
        ModifiedDate = evt.ModifiedDate }

/// Apply supplier offer deleted event
/// Note: In event sourcing, we typically don't delete but mark as deleted
let applyDeleted (_evt: SupplierOfferRevokedEvt) (state: SupplierOffer) : SupplierOffer =
    { state with
        IsActive = false
        ModifiedDate = _evt.DeletedDate }

/// Apply supplier offer status changed event
let applyStatusChanged (evt: SupplierOfferStatusChangedEvt) (state: SupplierOffer) : SupplierOffer =
    { state with
        IsActive = evt.IsActive
        ModifiedDate = evt.ModifiedDate }

/// Evolve supplier offer state
let evolve (state: SupplierOffer option) (event: SupplierOfferEvent) : SupplierOffer option =
    match event, state with
    | SupplierOfferDefined e, None -> Some(applyDefined e)
    | SupplierOfferUpdated e, Some s -> Some(applyUpdated e s)
    | SupplierOfferRevoked e, Some s -> Some(applyDeleted e s)
    | SupplierOfferStatusChanged e, Some s -> Some(applyStatusChanged e s)
    | SupplierOfferDefined _, Some _ -> state // Idempotent - offer already exists
    | _, None -> None // Can't apply updates to non-existent offer

let decide: DecideSupplierOffer =
    fun command stateOpt ->
        match command, stateOpt with
        | DefineSupplierOffer cmd, None ->
            match validateDefine cmd with
            | Error e -> Error e
            | Ok() ->
                match SupplierOfferId.create cmd.Id with
                | Error e -> Error e
                | Ok offerId ->
                    let priceTiers =
                        cmd.PriceTiers
                        |> List.map (fun (t: PriceTierCmd) ->
                            // TODO - Auto generated number
                            { TierNumber = 1
                              MinQuantity = Quantity.clampToZero t.MinQuantity
                              MaxQuantity = t.MaxQuantity |> Option.map Quantity.clampToZero
                              PricePerUnit = t.PricePerUnit
                              Currency = t.Currency })

                    let capacityWindows =
                        cmd.CapacityWindows
                        |> List.map (fun w ->
                            { WindowId = w.WindowId
                              StartDate = w.StartDate
                              EndDate = w.EndDate
                              MaxQuantity = Quantity.clampToZero w.MaxQuantity
                              AvailableQuantity = Quantity.clampToZero w.AvailableQuantity }
                            : SupplierCapacityWindow)

                    let lotSizeVal = cmd.LotSize |> Option.map Quantity.clampToZero

                    let reliabilityVal =
                        cmd.Reliability
                        |> Option.map (fun r ->
                            Percent.create r
                            |> function
                                | Ok x -> x
                                | Error _ -> failwith "invalid")

                    let offer =
                        { Id = offerId
                          SupplierId = cmd.SupplierId
                          SkuId = cmd.SkuId
                          StockingPointId = cmd.StockingPointId
                          Moq = cmd.Moq
                          LotSize = lotSizeVal
                          LeadTimeP50 = cmd.LeadTimeP50
                          LeadTimeP95 = cmd.LeadTimeP95
                          PriceTiers = priceTiers
                          Reliability = reliabilityVal
                          Incoterm = cmd.Incoterm
                          CapacityWindows = capacityWindows
                          IsActive = true
                          CreatedDate = cmd.CreatedDate
                          ModifiedDate = cmd.CreatedDate }

                    let evt =
                        { Id = offerId
                          SupplierId = cmd.SupplierId
                          SkuId = cmd.SkuId
                          StockingPointId = cmd.StockingPointId
                          Moq = cmd.Moq
                          LotSize = lotSizeVal
                          LeadTimeP50 = cmd.LeadTimeP50
                          LeadTimeP95 = cmd.LeadTimeP95
                          PriceTiers = priceTiers
                          Reliability = reliabilityVal
                          Incoterm = cmd.Incoterm
                          CapacityWindows = capacityWindows
                          CreatedDate = cmd.CreatedDate }

                    Ok
                        { NewState = offer
                          Events = [ SupplierOfferDefined evt ] }

        | DefineSupplierOffer _, Some _ -> Error(DomainError.validation "SupplierOffer already exists")

        | UpdateSupplierOffer cmd, Some state when state.Id = cmd.Id ->
            match validateUpdate cmd with
            | Error e -> Error e
            | Ok() ->
                let priceTiersOpt =
                    cmd.PriceTiers
                    |> Option.map (
                        List.map (fun t ->
                            { TierNumber = 1
                              MinQuantity = Quantity.clampToZero t.MinQuantity
                              MaxQuantity = t.MaxQuantity |> Option.map Quantity.clampToZero
                              PricePerUnit = t.PricePerUnit
                              Currency = t.Currency }
                            : PriceTier)
                    )

                let capacityWindowsOpt =
                    cmd.CapacityWindows
                    |> Option.map (
                        List.map (fun w ->
                            { WindowId = w.WindowId
                              StartDate = w.StartDate
                              EndDate = w.EndDate
                              MaxQuantity = Quantity.clampToZero w.MaxQuantity
                              AvailableQuantity = Quantity.clampToZero w.AvailableQuantity }
                            : SupplierCapacityWindow)
                    )

                let lotSizeVal = cmd.LotSize |> Option.map Quantity.clampToZero

                let reliabilityVal =
                    cmd.Reliability
                    |> Option.map (fun r ->
                        Percent.create r
                        |> function
                            | Ok x -> x
                            | Error _ -> failwith "invalid")

                let evt =
                    { Id = cmd.Id
                      Moq = cmd.Moq
                      LotSize = lotSizeVal
                      LeadTimeP50 = cmd.LeadTimeP50
                      LeadTimeP95 = cmd.LeadTimeP95
                      PriceTiers = priceTiersOpt
                      Reliability = reliabilityVal
                      Incoterm = cmd.Incoterm
                      CapacityWindows = capacityWindowsOpt
                      ModifiedDate = cmd.ModifiedDate }

                let newState = applyUpdated evt state

                Ok
                    { NewState = newState
                      Events = [ SupplierOfferUpdated evt ] }

        | UpdateSupplierOffer _, Some _ -> Error(DomainError.validation "SupplierOffer not found")

        | RevokeSupplierOffer cmd, Some state when state.Id = cmd.Id ->
            let evt =
                { Id = cmd.Id
                  DeletedDate = cmd.DeletedDate }

            let newState = applyDeleted evt state

            Ok
                { NewState = newState
                  Events = [ SupplierOfferRevoked evt ] }

        | RevokeSupplierOffer _, Some _ -> Error(DomainError.validation "SupplierOffer not found")

        | ChangeSupplierOfferStatus cmd, Some state when state.Id = cmd.Id ->
            let evt =
                { Id = cmd.Id
                  IsActive = cmd.IsActive
                  ModifiedDate = cmd.ModifiedDate }

            let newState = applyStatusChanged evt state

            Ok
                { NewState = newState
                  Events = [ SupplierOfferStatusChanged evt ] }

        | ChangeSupplierOfferStatus _, Some _ -> Error(DomainError.validation "SupplierOffer not found")
        | _, None -> Error(DomainError.validation "SupplierOffer not found")
