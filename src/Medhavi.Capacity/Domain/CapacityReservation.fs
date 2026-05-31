namespace Medhavi.Capacity.Domain.CapacityReservationAgg

open System
open Medhavi.SharedKernel
open Medhavi.Capacity.Domain.CapacityAgg

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

type CreateReservationCmd =
    { Id: CapacityReservationId
      RequirementId: CapacityRequirementId
      ResourceId: PhysicalResourceId
      BucketId: CapacityBucketId
      Minutes: DurationMinutes
      Start: Timestamp option
      End: Timestamp option
      Source: LoadSource
      Created: Timestamp }

type ReleaseReservationCmd =
    { Id: CapacityReservationId
      ReleasedAt: Timestamp }

type CapacityReservationCommand =
    | CreateReservation of CreateReservationCmd
    | ReleaseReservation of ReleaseReservationCmd

type CapacityReservationCreatedEvt =
    { Id: CapacityReservationId
      RequirementId: CapacityRequirementId
      ResourceId: PhysicalResourceId
      BucketId: CapacityBucketId
      Minutes: DurationMinutes
      Start: Timestamp option
      End: Timestamp option
      Source: LoadSource
      Created: Timestamp }

type CapacityReservationReleasedEvt =
    { Id: CapacityReservationId
      ReleasedAt: Timestamp }

type CapacityReservationEvent =
    | CapacityReservationCreated of CapacityReservationCreatedEvt
    | CapacityReservationReleased of CapacityReservationReleasedEvt

type DecideReservation = Decide<CapacityReservation, CapacityReservationCommand, CapacityReservationEvent>
type EvolveReservation = Evolve<CapacityReservation, CapacityReservationEvent>

module Reservation =
    let applyCreated (evt: CapacityReservationCreatedEvt) : CapacityReservation =
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

    let applyReleased (evt: CapacityReservationReleasedEvt) (state: CapacityReservation) : CapacityReservation =
        { state with
            Status = CapacityReservationStatus.Released
            ModifiedAt = evt.ReleasedAt }

    let evolve: EvolveReservation =
        fun event stateOpt ->
            match event, stateOpt with
            | CapacityReservationCreated e, None -> Some(applyCreated e)
            | CapacityReservationReleased e, Some s -> Some(applyReleased e s)
            | CapacityReservationCreated _, Some s -> Some s
            | _, _ -> stateOpt

    let decide: DecideReservation =
        fun command stateOpt ->
            match command, stateOpt with
            | CreateReservation cmd, None ->
                if DurationMinutes.value cmd.Minutes <= 0.0m then
                    Error (DomainError.validation "Reservation minutes must be greater than zero")
                else
                    let evt =
                        { Id = cmd.Id
                          RequirementId = cmd.RequirementId
                          ResourceId = cmd.ResourceId
                          BucketId = cmd.BucketId
                          Minutes = cmd.Minutes
                          Start = cmd.Start
                          End = cmd.End
                          Source = cmd.Source
                          Created = cmd.Created }
                    Ok { NewState = applyCreated evt
                         Events = [ CapacityReservationCreated evt ] }
            | CreateReservation _, Some _ ->
                Error (DomainError.invariant "Reservation already exists")

            | ReleaseReservation cmd, Some state ->
                if state.Status = CapacityReservationStatus.Released then
                    Error (DomainError.invariant "Reservation is already released")
                else
                    let evt =
                        { Id = cmd.Id
                          ReleasedAt = cmd.ReleasedAt }
                    Ok { NewState = applyReleased evt state
                         Events = [ CapacityReservationReleased evt ] }
            | ReleaseReservation _, None ->
                Error (DomainError.validation "Reservation not found")
