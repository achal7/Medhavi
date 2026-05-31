module Medhavi.Capacity.Domain.CapacityAgg

open Medhavi.SharedKernel
open Medhavi.Capacity

type CapacityPlanningMode =
    | Infinite
    | Finite

type CapacityRequirementStatus =
    | Planned
    | Firm
    | Committed
    | Released
    | Cancelled
    | Completed

type CapacityReservationStatus =
    | Planned
    | Firm
    | Committed
    | Released
    | Cancelled
    | Completed

type CapacityBucketStatus =
    | Open
    | Frozen
    | Closed

type LoadSource =
    | FromRouting
    | FromManualOverride
    | FromScheduler
    | FromPromise
    | FromActualExecution

type CapacityRequirement =
    { Id: CapacityRequirementId
      ScenarioId: string option
      WorkOrderId: string option
      RoutingId: RoutingId
      RoutingRevision: Revision
      RoutingStepId: RoutingStepId option
      OperationCode: OperationId option
      ProductId: SkuId
      ResourceId: PhysicalResourceId option
      WorkCenterId: WorkCenterId option
      ResourceKind: CapacityResourceKind
      LoadBasis: CapacityLoadBasis
      RequiredQuantity: Quantity
      SetupMinutes: DurationMinutes option
      RunMinutesPerBaseQuantity: Quantity
      TeardownMinutes: DurationMinutes option
      RequiredMinutes: DurationMinutes
      StartWindow: DateRange option
      EndWindow: DateRange option
      Priority: int
      Status: CapacityRequirementStatus }

type CapacityReservation =
    { Id: CapacityReservationId
      RequirementId: CapacityRequirementId
      ResourceId: PhysicalResourceId
      BucketId: CapacityBucketId
      Minutes: DurationMinutes
      Start: Timestamp option
      End: Timestamp option
      Status: CapacityReservationStatus
      Source: LoadSource
      CreatedAt: Timestamp
      ModifiedAt: Timestamp }

type CapacityBucket =
    { Id: CapacityBucketId
      ResourceId: PhysicalResourceId
      Window: Window
      AvailableMinutes: DurationMinutes
      ReservedMinutes: DurationMinutes
      FirmMinutes: DurationMinutes
      PlannedMinutes: DurationMinutes
      ActualMinutes: DurationMinutes
      FreeMinutes: DurationMinutes
      Status: CapacityBucketStatus
      Reservations: CapacityReservation list }

type CapacityCommand =
    | DefineBucket of PhysicalResourceId * Window * DurationMinutes
    | ReserveCapacity of CapacityReservationId * CapacityRequirementId * DurationMinutes * start: Timestamp option * endVal: Timestamp option * LoadSource
    | CancelReservation of CapacityReservationId

type CapacityBucketCreatedEvt =
    { ResourceId: PhysicalResourceId
      Id: CapacityBucketId
      Window: Window
      TotalCapacity: DurationMinutes }

type CapacityReservedEvt =
    { Id: CapacityReservationId
      RequirementId: CapacityRequirementId
      ResourceId: PhysicalResourceId
      BucketId: CapacityBucketId
      Minutes: DurationMinutes
      Start: Timestamp option
      End: Timestamp option
      Source: LoadSource
      Created: Timestamp }

type ReservationCancelledEvt =
    { Id: CapacityReservationId
      BucketId: CapacityBucketId
      Minutes: DurationMinutes
      CancelledAt: Timestamp }

type CapacityEvent =
    | BucketCreated of CapacityBucketCreatedEvt
    | CapacityReserved of CapacityReservedEvt
    | ReservationCancelled of ReservationCancelledEvt

type DecideCapacity = Decide<CapacityBucket, CapacityCommand, CapacityEvent>
type EvolveCapacity = Evolve<CapacityBucket, CapacityEvent>

// Helper functions for DurationMinutes addition and subtraction
let addMinutes (d1: DurationMinutes) (d2: DurationMinutes) : DurationMinutes =
    let v = DurationMinutes.value d1 + DurationMinutes.value d2
    match DurationMinutes.create v with
    | Ok res -> res
    | Error e -> failwith e

let subMinutes (d1: DurationMinutes) (d2: DurationMinutes) : DurationMinutes =
    let v = max 0m (DurationMinutes.value d1 - DurationMinutes.value d2)
    match DurationMinutes.create v with
    | Ok res -> res
    | Error e -> failwith e

let applyBucketCreated (evt: CapacityBucketCreatedEvt) : CapacityBucket =
    { Id = evt.Id
      ResourceId = evt.ResourceId
      Window = evt.Window
      AvailableMinutes = evt.TotalCapacity
      ReservedMinutes = DurationMinutes.zero
      FirmMinutes = DurationMinutes.zero
      PlannedMinutes = DurationMinutes.zero
      ActualMinutes = DurationMinutes.zero
      FreeMinutes = evt.TotalCapacity
      Status = CapacityBucketStatus.Open
      Reservations = List.Empty }

let applyCapacityReserved (evt: CapacityReservedEvt) (state: CapacityBucket) : CapacityBucket =
    let res =
        { Id = evt.Id
          RequirementId = evt.RequirementId
          ResourceId = evt.ResourceId
          BucketId = evt.BucketId
          Minutes = evt.Minutes
          Start = evt.Start
          End = evt.End
          Status = CapacityReservationStatus.Planned
          Source = evt.Source
          CreatedAt = evt.Created
          ModifiedAt = evt.Created }
    { state with
        PlannedMinutes = addMinutes state.PlannedMinutes evt.Minutes
        FreeMinutes = subMinutes state.FreeMinutes evt.Minutes
        Reservations = res :: state.Reservations }

let applyReservationCancelled (evt: ReservationCancelledEvt) (state: CapacityBucket) : CapacityBucket =
    let remaining = state.Reservations |> List.filter (fun r -> r.Id <> evt.Id)
    { state with
        PlannedMinutes = subMinutes state.PlannedMinutes evt.Minutes
        FreeMinutes = addMinutes state.FreeMinutes evt.Minutes
        Reservations = remaining }

let evolve: EvolveCapacity =
    fun event stateOpt ->
        match event, stateOpt with
        | BucketCreated e, None -> Some(applyBucketCreated e)
        | CapacityReserved e, Some s -> Some(applyCapacityReserved e s)
        | ReservationCancelled e, Some s -> Some(applyReservationCancelled e s)
        | BucketCreated _, Some s -> Some s
        | _, _ -> stateOpt

let decide: DecideCapacity =
    fun command stateOpt ->
        match command, stateOpt with
        | DefineBucket(resId, window, total), None ->
            let id = CapacityBucketId.create resId window
            let evt =
                { ResourceId = resId
                  Window = window
                  Id = id
                  TotalCapacity = total }
            Ok
                { NewState = applyBucketCreated evt
                  Events = [ BucketCreated evt ] }

        | DefineBucket _, Some _ -> Error(DomainError.invariant "Capacity bucket already exists")

        | ReserveCapacity(resId, reqId, minutes, startOpt, endOpt, source), Some state ->
            if DurationMinutes.value state.FreeMinutes < DurationMinutes.value minutes then
                Error(DomainError.invariant "CapacityExceeded")
            else
                let evt =
                    { Id = resId
                      RequirementId = reqId
                      ResourceId = state.ResourceId
                      BucketId = state.Id
                      Minutes = minutes
                      Start = startOpt
                      End = endOpt
                      Source = source
                      Created = Timestamp.now }
                Ok
                    { NewState = applyCapacityReserved evt state
                      Events = [ CapacityReserved evt ] }

        | CancelReservation resId, Some state ->
            match state.Reservations |> List.tryFind (fun r -> r.Id = resId) with
            | None -> Error(DomainError.validation "Reservation NotFound")
            | Some res ->
                let evt =
                    { Id = res.Id
                      BucketId = state.Id
                      Minutes = res.Minutes
                      CancelledAt = Timestamp.now }
                Ok
                    { NewState = applyReservationCancelled evt state
                      Events = [ ReservationCancelled evt ] }

        | _, None -> Error(DomainError.validation "Capacity bucket not found")
