module Medhavi.Capacity.Domain.OperationAgg

open System
open Medhavi.SharedKernel

type OperationState =
    | Scheduled
    | InProgress
    | Completed
    | Cancelled

type Operation =
    { Id: OperationId
      SequenceNumber: int
      RoutingStepId: RoutingStepId
      State: OperationState
      AddedLeadTime: TimeSpan
      IsFixed: bool
      //CampaignTypeAssignment: CampaignTypeAssignmentId option
      // Capacity-related fields (mandatory when Scheduled/InProgress)
      Window: TimeWindow
      ResourceId: PhysicalResourceId option // Mandatory when State = Scheduled or InProgress
      Duration: TimeSpan option // Mandatory when State = Scheduled or InProgress
      CreatedDate: Timestamp
      ModifiedDate: Timestamp }

// Commands
type ScheduleOperationCmd =
    { Id: OperationId
      SequenceNumber: int
      Window: TimeWindow // Mandatory: when operation is scheduled, it must have a time window
      RoutingStepId: RoutingStepId
      ResourceId: PhysicalResourceId // Mandatory: which resource will perform this operation
      Duration: TimeSpan // Mandatory: how long the operation will take
      IsFixed: bool }

type StartOperationCmd =
    { Id: OperationId
      StartedDate: Timestamp }

type CompleteOperationCmd =
    { Id: OperationId
      CompletedDate: Timestamp }

type CancelOperationCmd =
    { Id: OperationId
      CancelledDate: Timestamp }

type OperationCommand =
    | ScheduleOperation of ScheduleOperationCmd
    | StartOperation of StartOperationCmd
    | CompleteOperation of CompleteOperationCmd
    | CancelOperation of CancelOperationCmd

// Events
type OperationScheduledEvt =
    { Id: OperationId
      SequenceNumber: int
      RoutingStepId: RoutingStepId
      Window: TimeWindow // Time window when operation is scheduled
      ResourceId: PhysicalResourceId // Resource that will perform the operation
      Duration: TimeSpan } // Duration of the operation

type OperationStartedEvt =
    { Id: OperationId
      StartedDate: Timestamp }

type OperationCompletedEvt =
    { Id: OperationId
      CompletedDate: Timestamp }

type OperationCancelledEvt =
    { Id: OperationId
      CancelledDate: Timestamp }

type OperationEvent =
    | OperationScheduled of OperationScheduledEvt
    | OperationStarted of OperationStartedEvt
    | OperationCompleted of OperationCompletedEvt
    | OperationCancelled of OperationCancelledEvt

// Signatures
type DecideOperation = Decide<Operation, OperationCommand, OperationEvent>
type EvolveOperation = Evolve<Operation, OperationEvent>

let applyScheduled (evt: OperationScheduledEvt) : Operation =
    { Id = evt.Id
      SequenceNumber = evt.SequenceNumber
      RoutingStepId = evt.RoutingStepId
      State = Scheduled
      AddedLeadTime = TimeSpan.Zero
      IsFixed = false
      Window = evt.Window
      ResourceId = Some evt.ResourceId
      Duration = Some evt.Duration
      CreatedDate = Timestamp.now
      ModifiedDate = Timestamp.now }

let applyStarted (evt: OperationStartedEvt) (state: Operation) : Operation =
    { state with
        State = InProgress
        ModifiedDate = evt.StartedDate }

let applyCompleted (evt: OperationCompletedEvt) (state: Operation) : Operation =
    { state with
        State = Completed
        ModifiedDate = evt.CompletedDate }

let applyCancelled (evt: OperationCancelledEvt) (state: Operation) : Operation =
    { state with
        State = Cancelled
        ModifiedDate = evt.CancelledDate }

let evolve: EvolveOperation =
    fun event stateOpt ->
        match event, stateOpt with
        | OperationScheduled e, None -> Some(applyScheduled e)
        | OperationStarted e, Some s -> Some(applyStarted e s)
        | OperationCompleted e, Some s -> Some(applyCompleted e s)
        | OperationCancelled e, Some s -> Some(applyCancelled e s)
        | OperationScheduled _, Some state -> Some state // Idempotent
        | _, _ -> stateOpt

let decide: DecideOperation =
    fun cmd stateOpt ->
        match cmd, stateOpt with
        | ScheduleOperation cmd, None ->
            let operation: Operation =
                { Id = cmd.Id
                  AddedLeadTime = TimeSpan.Zero
                  CreatedDate = Timestamp.now
                  Duration = Some cmd.Duration
                  IsFixed = cmd.IsFixed
                  ModifiedDate = Timestamp.minValue
                  ResourceId = Some cmd.ResourceId
                  RoutingStepId = cmd.RoutingStepId
                  SequenceNumber = cmd.SequenceNumber
                  State = Scheduled
                  Window = cmd.Window }

            let evt =
                { Id = cmd.Id
                  SequenceNumber = cmd.SequenceNumber
                  RoutingStepId = cmd.RoutingStepId
                  Window = cmd.Window
                  ResourceId = cmd.ResourceId
                  Duration = cmd.Duration }

            Ok(
                { NewState = operation
                  Events = [ OperationScheduled evt ] }
            )

        | ScheduleOperation _, Some _ -> Error(DomainError.invariant "Operation already scheduled")

        | StartOperation cmd, Some state ->
            match state.State with
            | Scheduled ->
                { Start = cmd.StartedDate
                  End = Timestamp.minValue }
                |> fun win ->
                    { state with
                        State = InProgress
                        Window = win
                        ModifiedDate = Timestamp.now }
                |> fun operation ->
                    Ok(
                        { NewState = operation
                          Events =
                            [ OperationStarted
                                  { Id = operation.Id
                                    StartedDate = operation.Window.Start } ] }
                    )
            // TODO: Implement in progress state. The window must gets adjusted because of the ETA given
            //| InProgress -> Ok([]) // Idempotent
            | _ -> Error(DomainError.invariant "Operation must be Scheduled to start")

        | CompleteOperation cmd, Some state ->
            match state.State with
            | InProgress ->
                let operation: Operation =
                    { state with
                        State = Completed
                        ModifiedDate = cmd.CompletedDate }

                Ok(
                    { NewState = operation
                      Events =
                        [ OperationCompleted
                              { Id = state.Id
                                CompletedDate = cmd.CompletedDate } ] }
                )
            | _ -> Error(DomainError.invariant "Operation must be InProgress to complete")

        | CancelOperation cmd, Some state ->
            match state.State with
            | Cancelled -> Error(DomainError.invariant "Operation already cancelled")
            | Completed -> Error(DomainError.invariant "Cannot cancel completed operation")
            | _ ->
                let updatedOperation = { state with State = Cancelled }

                Ok(
                    { NewState = updatedOperation
                      Events =
                        [ OperationCancelled
                              { Id = state.Id
                                CancelledDate = cmd.CancelledDate } ] }
                )

        | _, None -> Error(DomainError.validation "Operation not found")
