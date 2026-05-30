module Medhavi.Capacity.Domain.CalendarAgg

open System
open System.Text.Json.Serialization
open Medhavi.SharedKernel
open Medhavi.Common

[<JsonFSharpConverter>]
type CalendarType =
    | ResourceCalendar
    | PlantCalendar
    | StockingPointCalendar
    | NetworkCalendar

[<JsonFSharpConverter>]
type EventId = private EventId of string

[<JsonFSharpConverter>]
type EventType =
    | Holiday
    | Maintenance
    | Downtime
    | ExtraShift
    | Shift
    | Overtime
    | Campaign
    | Custom of string

type RecurrencePattern =
    | Daily of int
    | Weekly of int * string list // interval, weekdays
    | MonthlyDay of int // day of month
    | MonthlyWeek of string * string // week of month, day of week
    | YearlyDay of int * int // month, day
    | YearlyWeek of int * string * string // month, week of month, day of week

type Calendar =
    { Id: CalendarId
      CalendarType: CalendarType
      BaseDate: Timestamp
      WindowDays: int
      HistoryDays: int
      UpdateInterval: TimeSpan
      StartDateAsReal: decimal option
      Events: CalendarEvent list
      IsActive: bool
      Created: Timestamp
      Modified: Timestamp }

and CalendarEvent =
    { Id: EventId
      CalendarId: CalendarId
      Subject: string
      EventType: EventType
      CapacityFactor: Percent // 0.0–1.0
      IsDefault: bool
      Window: Window
      Recurrence: RecurrencePattern option
      Created: Timestamp
      Modified: Timestamp }

module EventId =
    open Medhavi.SharedKernel

    let create (value: string) : Result<EventId, DomainError> =
        if String.IsNullOrWhiteSpace value then
            Error(DomainError.validation "Event ID is required")
        else
            Ok(EventId value)

    let value (EventId v) = v

type EventSchedule =
    { EventId: EventId
      CalendarId: CalendarId
      Window: Window
      CapacityFactor: Percent }

type CalendarAvailability =
    { CalendarId: CalendarId
      Date: Timestamp
      CapacityFactor: Percent
      IsAvailable: bool }

// Commands
type CreateCalendarCmd =
    { Id: CalendarId
      CalendarType: CalendarType
      IsActive: bool
      Created: Timestamp }

type AddCalendarEventCmd =
    { CalendarId: CalendarId
      Event: CalendarEvent }

type RemoveCalendarEventCmd =
    { CalendarId: CalendarId
      EventId: EventId }

type ClearCalendarCmd =
    { CalendarId: CalendarId
      Modified: Timestamp }

type ActivateCalendarCmd =
    { CalendarId: CalendarId
      Modified: Timestamp }

type DeactivateCalendarCmd =
    { CalendarId: CalendarId
      Modified: Timestamp }

type CalendarCommand =
    | CreateCalendar of CreateCalendarCmd
    | AddCalendarEvent of AddCalendarEventCmd
    | RemoveCalendarEvent of RemoveCalendarEventCmd
    | ClearCalendar of ClearCalendarCmd
    | ActivateCalendar of ActivateCalendarCmd
    | DeactivateCalendar of DeactivateCalendarCmd

// Events
type CalendarCreatedEvt =
    { Id: CalendarId
      CalendarType: CalendarType
      IsActive: bool
      Created: Timestamp }

type CalendarEventAddedEvt =
    { CalendarId: CalendarId
      Event: CalendarEvent }

type CalendarEventRemovedEvt =
    { CalendarId: CalendarId
      EventId: EventId
      Modified: Timestamp }

type CalendarClearedEvt =
    { CalendarId: CalendarId
      Modified: Timestamp }

type CalendarActivatedEvt =
    { CalendarId: CalendarId
      Modified: Timestamp }

type CalendarDeactivatedEvt =
    { CalendarId: CalendarId
      Modified: Timestamp }

type CalendarsEvent =
    | CalendarCreated of CalendarCreatedEvt
    | CalendarEventAdded of CalendarEventAddedEvt
    | CalendarEventRemoved of CalendarEventRemovedEvt
    | CalendarCleared of CalendarClearedEvt
    | CalendarActivated of CalendarActivatedEvt
    | CalendarDeactivated of CalendarDeactivatedEvt

// Signatures
type DecideResourceCalendar = Decide<Calendar, CalendarCommand, CalendarsEvent>
type EvolveCalendar = Evolve<Calendar, CalendarsEvent>

// Validation functions
let validateCreate (cmd: CreateCalendarCmd) : Result<unit, DomainError> = Ok()

let validateAddEvent (cmd: AddCalendarEventCmd) : Result<unit, DomainError> =
    result {
        if String.IsNullOrWhiteSpace(cmd.Event.Subject) then
            return! Error(DomainError.validation "Calendar event subject is required")

        if cmd.Event.Window.Start >= cmd.Event.Window.End then
            return! Error(DomainError.validation "Calendar event start time must be before end time")

        return ()
    }

let validateRemoveEvent (cmd: RemoveCalendarEventCmd) : Result<unit, DomainError> = Ok()
let validateClear (_cmd: ClearCalendarCmd) : Result<unit, DomainError> = Ok()
let validateActivate (_cmd: ActivateCalendarCmd) : Result<unit, DomainError> = Ok()
let validateDeactivate (_cmd: DeactivateCalendarCmd) : Result<unit, DomainError> = Ok()

// State evolution functions
let applyCreated (evt: CalendarCreatedEvt) : Calendar =
    let now = Timestamp.now

    { Id = evt.Id
      CalendarType = evt.CalendarType
      BaseDate = now
      WindowDays = 30
      HistoryDays = 7
      UpdateInterval = TimeSpan.FromHours(1.0)
      StartDateAsReal = None
      Events = []
      IsActive = evt.IsActive
      Created = evt.Created
      Modified = evt.Created }

let applyEventAdded (evt: CalendarEventAddedEvt) (state: Calendar) : Calendar =
    let eventExists =
        state.Events
        |> List.exists (fun e -> e.Id = evt.Event.Id)

    if eventExists then
        state
    else
        { state with
            Events = evt.Event :: state.Events
            Modified = Timestamp.now }

let applyEventRemoved (evt: CalendarEventRemovedEvt) (state: Calendar) : Calendar =
    { state with
        Events =
            state.Events
            |> List.filter (fun e -> e.Id <> evt.EventId)
        Modified = evt.Modified }

let applyCleared (evt: CalendarClearedEvt) (state: Calendar) : Calendar =
    { state with
        Events = []
        Modified = evt.Modified }

let applyActivated (evt: CalendarActivatedEvt) (state: Calendar) : Calendar =
    { state with
        IsActive = true
        Modified = evt.Modified }

let applyDeactivated (evt: CalendarDeactivatedEvt) (state: Calendar) : Calendar =
    { state with
        IsActive = false
        Modified = evt.Modified }

let evolve (state: Calendar option) (event: CalendarsEvent) : Calendar option =
    match event, state with
    | CalendarCreated e, None -> Some(applyCreated e)
    | CalendarEventAdded e, Some s -> Some(applyEventAdded e s)
    | CalendarEventRemoved e, Some s -> Some(applyEventRemoved e s)
    | CalendarCleared e, Some s -> Some(applyCleared e s)
    | CalendarActivated e, Some s -> Some(applyActivated e s)
    | CalendarDeactivated e, Some s -> Some(applyDeactivated e s)
    | CalendarCreated _, Some _ -> state
    | _, None -> None

let decide: DecideResourceCalendar =
    fun cmd stateOpt ->
        match cmd, stateOpt with
        | CreateCalendar cmd, None ->
            validateCreate cmd
            |> Result.map (fun _ ->
                { NewState =
                    applyCreated
                        { Id = cmd.Id
                          CalendarType = cmd.CalendarType
                          IsActive = cmd.IsActive
                          Created = cmd.Created }
                  Events =
                    [ CalendarCreated
                          { Id = cmd.Id
                            CalendarType = cmd.CalendarType
                            IsActive = cmd.IsActive
                            Created = cmd.Created } ] })
        | CreateCalendar _, Some _ -> Error(DomainError.invariant "Calendar already exists")

        | AddCalendarEvent cmd, Some state ->
            validateAddEvent cmd
            |> Result.map (fun _ ->
                let evt =
                    { CalendarId = state.Id
                      Event = cmd.Event }

                let updatedState = applyEventAdded evt state

                { NewState = updatedState
                  Events = [ CalendarEventAdded evt ] })

        | RemoveCalendarEvent cmd, Some state ->
            validateRemoveEvent cmd
            |> Result.map (fun _ ->
                let evt =
                    { CalendarId = state.Id
                      EventId = cmd.EventId
                      Modified = Timestamp.now }

                let updatedState = applyEventRemoved evt state

                { NewState = updatedState
                  Events = [ CalendarEventRemoved evt ] })

        | ClearCalendar cmd, Some state ->
            validateClear cmd
            |> Result.map (fun _ ->
                let evt: CalendarClearedEvt =
                    { CalendarId = state.Id
                      Modified = cmd.Modified }

                let updatedState = applyCleared evt state

                { NewState = updatedState
                  Events = [ CalendarCleared evt ] })

        | ActivateCalendar cmd, Some state ->
            validateActivate cmd
            |> Result.map (fun _ ->
                let evt: CalendarActivatedEvt =
                    { CalendarId = state.Id
                      Modified = cmd.Modified }

                let updatedState = applyActivated evt state

                { NewState = updatedState
                  Events = [ CalendarActivated evt ] })

        | DeactivateCalendar cmd, Some state ->
            validateDeactivate cmd
            |> Result.map (fun _ ->
                let evt =
                    { CalendarId = state.Id
                      Modified = cmd.Modified }

                let updatedState = applyDeactivated evt state

                { NewState = updatedState
                  Events = [ CalendarDeactivated evt ] })

        | _, None -> Error(DomainError.validation "Calendar not found")
