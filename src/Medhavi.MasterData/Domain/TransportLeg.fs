module Medhavi.Domain.Transport

open System
open System.Text.Json.Serialization
open Medhavi.Common
open Medhavi.SharedKernel
open Medhavi.MasterData.Domain.UomAgg

/// Transport mode (Air, Road, Rail, Sea, etc.)
[<JsonFSharpConverter>]
type TransportMode =
    | Air
    | Road
    | Rail
    | Sea
    | Pipeline
    | Other of string

/// Transport schedule pattern
[<JsonFSharpConverter>]
type TransportSchedule =
    | Daily
    | Weekly of int // day of week (0=Sunday, 6=Saturday)
    | Monthly of int // day of month
    | OnDemand
    | Custom of string

/// Regulatory/hazmat constraint
[<JsonFSharpConverter>]
type TransportConstraint =
    | Hazmat
    | TemperatureControlled
    | Refrigerated
    | Fragile
    | Oversized
    | Regulatory of string
    | Custom of string

[<JsonFSharpConverter>]
type TransportLegPurpose =
    | Primary
    | Alternate
    | Emergency

type TransportCostDetail =
    { FixedCost: PositiveDecimal // Base cost per leg (regardless of quantity)
      VariableCostPerUnit: PositiveDecimal option // Cost per unit (kg, m3, etc.)
      MinFillThreshold: float option // Minimum fill percentage (0.0-1.0)
      MinFillPenalty: PositiveDecimal option // Penalty if utilization < MinFillThreshold
      LanePreference: PositiveDecimal option } // Preference multiplier (1.0 = neutral, <1.0 = preferred, >1.0 = penalized)

[<JsonFSharpConverter>]
type TransportLegId = private TransportLegId of string

type TransportCalendarId = TransportCalendarId of string

type CapacityProfile =
    | StaticCapacity of PositiveDecimal
    | ByDate of Map<DateTime, PositiveDecimal>

type TransportCalendarStatus =
    | Active
    | Retired

type TransportCalendar =
    { Id: TransportCalendarId
      Name: string
      TimeZoneId: string // IANA or Windows Id.
      Pattern: TransportSchedule
      PatternTimeOfDay: TimeSpan option // local time-of-day for departures
      ExceptionalDates: Timestamp list
      AdditionalDeparturesLocal: Timestamp list // local date-times
      WindowStartOffset: TimeSpan option
      WindowEndOffset: TimeSpan option
      CapacityProfile: CapacityProfile option
      EffectiveStart: Timestamp
      EffectiveEnd: Timestamp option
      Status: TransportCalendarStatus
      Created: Timestamp
      Modified: Timestamp }

// Add TransportCalendar commands
type DefineTransportCalendarCmd =
    { Id: TransportCalendarId
      Name: string
      TimeZoneId: string
      Pattern: TransportSchedule
      PatternTimeOfDay: TimeSpan option
      ExceptionalDates: Timestamp list
      AdditionalDeparturesLocal: Timestamp list
      WindowStartOffset: TimeSpan option
      WindowEndOffset: TimeSpan option
      CapacityProfile: CapacityProfile option
      EffectiveStart: Timestamp
      EffectiveEnd: Timestamp option
      Created: Timestamp }

type UpdateTransportCalendarCmd =
    { Id: TransportCalendarId
      Name: string option
      TimeZoneId: string option
      // ... other optional fields
      Modified: Timestamp }

type TransportCalendarDefinedEvt =
    { Id: TransportCalendarId
      Name: string
      TimeZoneId: string
      Pattern: TransportSchedule
      PatternTimeOfDay: TimeSpan option
      ExceptionalDates: Timestamp list
      AdditionalDeparturesLocal: Timestamp list
      WindowStartOffset: TimeSpan option
      WindowEndOffset: TimeSpan option
      CapacityProfile: CapacityProfile option
      EffectiveStart: Timestamp
      EffectiveEnd: Timestamp option
      Created: Timestamp }

type TransportCalendarUpdatedEvt =
    { Id: TransportCalendarId
      Name: string option
      TimeZoneId: string option
      // ... other optional fields
      Modified: Timestamp }

type TransportCalendarDeactivatedEvt =
    { Id: TransportCalendarId
      DeactivatedAt: Timestamp }

type TransportCalendarEvent =
    | CalendarDefined of TransportCalendarDefinedEvt
    | CalendarUpdated of TransportCalendarUpdatedEvt
    | CalendarDeactivated of TransportCalendarDeactivatedEvt

// Add evolve function for TransportCalendar
type EvolveTransportCalendar = Evolve<TransportCalendar, TransportCalendarEvent>

let evolveTransportCalendar: EvolveTransportCalendar =
    fun evt state ->
        match evt, state with
        | CalendarDefined e, None ->
            { Id = e.Id
              Name = e.Name
              TimeZoneId = e.TimeZoneId
              Pattern = e.Pattern
              PatternTimeOfDay = e.PatternTimeOfDay
              ExceptionalDates = e.ExceptionalDates
              AdditionalDeparturesLocal = e.AdditionalDeparturesLocal
              WindowStartOffset = e.WindowStartOffset
              WindowEndOffset = e.WindowEndOffset
              CapacityProfile = e.CapacityProfile
              EffectiveStart = e.EffectiveStart
              EffectiveEnd = e.EffectiveEnd
              Status = Active
              Created = e.Created
              Modified = e.Created }
            |> Some
        | CalendarUpdated e, Some s ->
            { s with
                Name = e.Name |> Option.defaultValue s.Name
                TimeZoneId = e.TimeZoneId |> Option.defaultValue s.TimeZoneId
                // ... update other fields
                Modified = e.Modified }
            |> Some
        | CalendarDeactivated e, Some s ->
            { s with
                Status = Retired
                Modified = e.DeactivatedAt }
            |> Some
        | _ -> failwith "Invalid state/event combination"

type TransportLegStatus =
    | Active
    | Retired

/// Transport Leg aggregate
/// Represents a scheduled transport leg with mode, schedule, capacity, cutoff, constraints, reliability, and CO2
type TransportLeg =
    { Id: TransportLegId
      Origin: StockingPointId
      Destination: StockingPointId
      Mode: TransportMode
      Purpose: TransportLegPurpose
      Schedule: TransportSchedule
      CalendarId: TransportCalendarId option
      LeadTime: TimeSpan // Duration of the transport leg
      Capacity: PositiveDecimal option // Capacity in weight/volume units
      CapacityUnit: UomId option
      CostDetail: TransportCostDetail
      Cutoff: TimeSpan option // Cutoff time before departure
      Constraints: TransportConstraint list
      Reliability: Percent option // 0.0-1.0 reliability factor
      CO2PerUnit: PositiveDecimal option // CO2 emissions per unit (kg/kg or kg/m3)
      EffectiveStart: Timestamp
      EffectiveEnd: Timestamp option
      Status: TransportLegStatus
      Created: Timestamp
      Modified: Timestamp }

// Commands
type DefineTransportLegCmd =
    { Id: TransportLegId
      Origin: StockingPointId
      Destination: StockingPointId
      Mode: TransportMode
      Schedule: TransportSchedule
      LeadTime: TimeSpan // Duration of the transport leg
      Capacity: PositiveDecimal option
      CapacityUnit: UomId option
      Cutoff: TimeSpan option
      Constraints: TransportConstraint list
      Reliability: Percent option
      CO2PerUnit: PositiveDecimal option
      EffectiveStart: Timestamp
      EffectiveEnd: Timestamp option
      Created: Timestamp }

type UpdateTransportLegCmd =
    { Id: TransportLegId
      Mode: TransportMode option
      Schedule: TransportSchedule option
      LeadTime: TimeSpan option // Duration of the transport leg
      Capacity: decimal option
      CapacityUnit: UomId option
      Cutoff: TimeSpan option
      Constraints: TransportConstraint list option
      Reliability: float option
      CO2PerUnit: decimal option
      EffectiveEnd: Timestamp option
      Modified: Timestamp }

type DeactivateTransportLegCmd =
    { Id: TransportLegId
      DeactivatedAt: Timestamp }

type TransportLegCommand =
    | DefineTransportLeg of DefineTransportLegCmd
    | UpdateTransportLeg of UpdateTransportLegCmd
    | DeactivateTransportLeg of DeactivateTransportLegCmd

// Events
type TransportLegDefinedEvt =
    { Id: TransportLegId
      Origin: StockingPointId
      Destination: StockingPointId
      Mode: TransportMode
      Schedule: TransportSchedule
      LeadTime: TimeSpan // Duration of the transport leg
      Capacity: PositiveDecimal option
      CapacityUnit: UomId option
      Cutoff: TimeSpan option
      Constraints: TransportConstraint list
      Reliability: Percent option
      CO2PerUnit: PositiveDecimal option
      EffectiveStart: Timestamp
      EffectiveEnd: Timestamp option
      Created: Timestamp
      CostDetail: TransportCostDetail }

type TransportLegUpdatedEvt =
    { Id: TransportLegId
      Mode: TransportMode option
      Schedule: TransportSchedule option
      LeadTime: TimeSpan option // Duration of the transport leg
      Capacity: PositiveDecimal option
      CapacityUnit: UomId option
      Cutoff: TimeSpan option
      Constraints: TransportConstraint list option
      Reliability: Percent option
      CO2PerUnit: PositiveDecimal option
      EffectiveEnd: Timestamp option
      Modified: Timestamp
      Cost: TransportCostDetail }

type TransportLegDeactivatedEvt =
    { Id: TransportLegId
      DeactivatedAt: Timestamp }

type TransportLegEvent =
    | TransportLegDefined of TransportLegDefinedEvt
    | TransportLegUpdated of TransportLegUpdatedEvt
    | TransportLegDeactivated of TransportLegDeactivatedEvt

// Signatures
type DecideTransportLeg = Decide<TransportLeg, TransportLegCommand, TransportLegEvent>

type EvolveTransportLeg = Evolve<TransportLeg, TransportLegEvent>

let tryParseInt (s: string) =
    match System.Int32.TryParse s with
    | true, v -> Some v
    | false, _ -> None

let parseTransportSchedule (input: string) : Result<TransportSchedule, string> =
    let s = input.Trim()

    match s.ToLowerInvariant() with
    | "daily" -> Ok Daily
    | "ondemand"
    | "on-demand" -> Ok OnDemand

    | _ when s.StartsWith("weekly:", StringComparison.OrdinalIgnoreCase) ->
        let value = s.Substring("weekly:".Length)

        match tryParseInt value with
        | Some d when d >= 0 && d <= 6 -> Ok(Weekly d)
        | _ -> Error "Weekly schedule requires day-of-week (0=Sunday .. 6=Saturday)"

    | _ when s.StartsWith("monthly:", StringComparison.OrdinalIgnoreCase) ->
        let value = s.Substring("monthly:".Length)

        match tryParseInt value with
        | Some d when d >= 1 && d <= 31 -> Ok(Monthly d)
        | _ -> Error "Monthly schedule requires day-of-month (1 .. 31)"

    | _ when s.StartsWith("custom:", StringComparison.OrdinalIgnoreCase) ->
        Ok(TransportSchedule.Custom(s.Substring("custom:".Length)))

    | _ -> Error $"Unknown transport schedule: '{input}'"

let transportScheduleToString =
    function
    | Daily -> "Daily"
    | Weekly d -> $"Weekly:{d}"
    | Monthly d -> $"Monthly:{d}"
    | OnDemand -> "OnDemand"
    | TransportSchedule.Custom s -> $"Custom:{s}"

let parseTransportConstraint (input: string) =
    match input.Trim().ToLowerInvariant() with
    | "hazmat" -> Ok Hazmat
    | "temperaturecontrolled"
    | "temperature_controlled"
    | "temperature-controlled" -> Ok TemperatureControlled
    | "refrigerated" -> Ok Refrigerated
    | "fragile" -> Ok Fragile
    | "oversized" -> Ok Oversized
    | s when s.StartsWith("regulatory:") -> Ok(Regulatory(s.Substring("regulatory:".Length)))
    | s when s.StartsWith("custom:") -> Ok(Custom(s.Substring("custom:".Length)))
    | _ -> Error $"Unknown transport constraint: '{input}'"

let parseTransportMode (input: string) : Result<TransportMode, string> =
    match input.Trim().ToLowerInvariant() with
    | "air" -> Ok Air
    | "road" -> Ok Road
    | "rail" -> Ok Rail
    | "sea" -> Ok Sea
    | "pipeline" -> Ok Pipeline
    | "" -> Error "Transport mode cannot be empty"
    | s -> Ok(Other s)

let validateDefine (cmd: DefineTransportLegCmd) : Result<unit, DomainError> =
    result {

        // Validate that From and To are different
        let! _ =
            if cmd.Origin = cmd.Destination then
                Error(DomainError.validation "Transport leg origin and destination must be different")
            else
                Ok()

        // Validate lead time
        let! _ =
            if cmd.LeadTime.TotalDays < 0.0 then
                Error(DomainError.validation "Lead time days must be non-negative")
            else
                Ok()

        // Validate capacity if provided
        let! _ =
            match cmd.Capacity with
            | Some cap when (PositiveDecimal.value cap) <= 0M ->
                Error(DomainError.validation "Capacity must be greater than zero if provided")
            | _ -> Ok()

        return ()
    }

let validateCapacity (capOpt: decimal option) : Result<PositiveDecimal option, DomainError> =
    match capOpt with
    | None -> Ok None
    | Some v -> PositiveDecimal.create v |> Result.map Some

let validateReliability (relOpt: float option) : Result<Percent option, DomainError> =
    match relOpt with
    | None -> Ok None
    | Some v -> Percent.create (decimal v) |> Result.map Some

let validateCO2PerUnit (co2Opt: decimal option) : Result<PositiveDecimal option, DomainError> =
    match co2Opt with
    | None -> Ok None
    | Some v -> PositiveDecimal.create v |> Result.map Some

let validateUpdate (cmd: UpdateTransportLegCmd) : Result<PositiveDecimal option * Percent option * PositiveDecimal option, DomainError> =
    result {
        let! cap = validateCapacity cmd.Capacity
        let! rel = validateReliability cmd.Reliability
        let! co2 = validateCO2PerUnit cmd.CO2PerUnit
        return (cap, rel, co2)
    }

let decide: DecideTransportLeg =
    fun command stateOpt ->
        match command, stateOpt with
        | DefineTransportLeg cmd, None ->
            match validateDefine cmd with
            | Error e -> Error e
            | Ok () ->
                let costDetail =
                    { FixedCost = PositiveDecimal.Zero
                      VariableCostPerUnit = None
                      MinFillThreshold = None
                      MinFillPenalty = None
                      LanePreference = None }

                let evt =
                    { Id = cmd.Id
                      Origin = cmd.Origin
                      Destination = cmd.Destination
                      Mode = cmd.Mode
                      Schedule = cmd.Schedule
                      LeadTime = cmd.LeadTime
                      Capacity = cmd.Capacity
                      CapacityUnit = cmd.CapacityUnit
                      Cutoff = cmd.Cutoff
                      Constraints = cmd.Constraints
                      Reliability = cmd.Reliability
                      CO2PerUnit = cmd.CO2PerUnit
                      EffectiveStart = cmd.EffectiveStart
                      EffectiveEnd = cmd.EffectiveEnd
                      Created = cmd.Created
                      CostDetail = costDetail }

                let newState =
                    { Id = cmd.Id
                      Origin = cmd.Origin
                      Destination = cmd.Destination
                      CalendarId = None
                      Mode = cmd.Mode
                      Purpose = TransportLegPurpose.Primary
                      Schedule = cmd.Schedule
                      LeadTime = cmd.LeadTime
                      Capacity = cmd.Capacity
                      CapacityUnit = cmd.CapacityUnit
                      Cutoff = cmd.Cutoff
                      CostDetail = costDetail
                      Constraints = cmd.Constraints
                      Reliability = cmd.Reliability
                      CO2PerUnit = cmd.CO2PerUnit
                      EffectiveStart = cmd.EffectiveStart
                      EffectiveEnd = cmd.EffectiveEnd
                      Status = Active
                      Created = cmd.Created
                      Modified = cmd.Created }

                Ok { NewState = newState; Events = [ TransportLegDefined evt ] }

        | DefineTransportLeg _, Some _ -> Error(DomainError.validation "TransportLeg already exists")

        | UpdateTransportLeg cmd, Some state when state.Id = cmd.Id ->
            match state.Status with
            | Retired -> Error(DomainError.invariant "Cannot update an inactive TransportLeg")
            | Active ->
                match validateUpdate cmd with
                | Error e -> Error e
                | Ok (cap, rel, co2) ->
                    let evt =
                        { Id = cmd.Id
                          Mode = cmd.Mode
                          Schedule = cmd.Schedule
                          LeadTime = cmd.LeadTime
                          Capacity = cap
                          CapacityUnit = cmd.CapacityUnit
                          Cutoff = cmd.Cutoff
                          Constraints = cmd.Constraints
                          Reliability = rel
                          CO2PerUnit = co2
                          EffectiveEnd = cmd.EffectiveEnd
                          Modified = cmd.Modified
                          Cost = state.CostDetail }

                    let newState =
                        { state with
                            Mode = cmd.Mode |> Option.defaultValue state.Mode
                            Schedule = cmd.Schedule |> Option.defaultValue state.Schedule
                            LeadTime = cmd.LeadTime |> Option.defaultValue state.LeadTime
                            Capacity = cap
                            CapacityUnit = cmd.CapacityUnit
                            Cutoff = cmd.Cutoff
                            Constraints = cmd.Constraints |> Option.defaultValue state.Constraints
                            Reliability = rel
                            CO2PerUnit = co2
                            EffectiveEnd = cmd.EffectiveEnd
                            Modified = cmd.Modified }

                    Ok { NewState = newState; Events = [ TransportLegUpdated evt ] }

        | UpdateTransportLeg _, Some _ -> Error(DomainError.validation "TransportLeg not found")

        | DeactivateTransportLeg cmd, Some state when state.Id = cmd.Id ->
            match state.Status with
            | Retired -> Error(DomainError.invariant "TransportLeg is already inactive")
            | Active ->
                let updated =
                    { state with
                        Status = Retired
                        Modified = cmd.DeactivatedAt }

                let evt =
                    { Id = cmd.Id
                      DeactivatedAt = cmd.DeactivatedAt }

                Ok { NewState = updated; Events = [ TransportLegDeactivated evt ] }

        | DeactivateTransportLeg _, Some _ -> Error(DomainError.validation "TransportLeg not found")

        | _, None -> Error(DomainError.validation "TransportLeg not found")
        | _, _ -> Error(DomainError.validation "Invalid command/state combination")

// Implement the Evolve function
let evolveTransportLeg: EvolveTransportLeg =
    fun evt state ->
        match evt, state with
        | TransportLegDefined e, None ->
            { Id = e.Id
              Origin = e.Origin
              Destination = e.Destination
              CalendarId = None
              Mode = e.Mode
              Purpose = TransportLegPurpose.Primary
              Schedule = e.Schedule
              LeadTime = e.LeadTime
              Capacity = e.Capacity
              CapacityUnit = e.CapacityUnit
              Cutoff = e.Cutoff
              CostDetail = e.CostDetail
              Constraints = e.Constraints
              Reliability = e.Reliability
              CO2PerUnit = e.CO2PerUnit
              EffectiveStart = e.EffectiveStart
              EffectiveEnd = e.EffectiveEnd
              Status = Active
              Created = e.Created
              Modified = e.Created }
            |> Some
        | TransportLegUpdated e, Some s ->
            { s with
                Mode = e.Mode |> Option.defaultValue s.Mode
                Schedule = e.Schedule |> Option.defaultValue s.Schedule
                LeadTime = e.LeadTime |> Option.defaultValue s.LeadTime
                Capacity = e.Capacity
                CapacityUnit = e.CapacityUnit
                Cutoff = e.Cutoff
                Constraints = e.Constraints |> Option.defaultValue s.Constraints
                Reliability = e.Reliability
                CO2PerUnit = e.CO2PerUnit
                EffectiveEnd = e.EffectiveEnd
                CostDetail = e.Cost
                Modified = e.Modified }
            |> Some
        | TransportLegDeactivated e, Some s ->
            { s with
                Status = Retired
                Modified = e.DeactivatedAt }
            |> Some
        | _ -> failwith "Invalid state/event combination"
