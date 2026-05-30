namespace Medhavi.Capacity.Application

open System
open Medhavi.Common.Patterns
open Medhavi.Infrastructure
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Aggregate
open Medhavi.Infrastructure.Projections

// ----------------------------------------------------
// 1. Calendar Application Capabilities & Projection
// ----------------------------------------------------
module CalendarApp =
    open Medhavi.Capacity.Domain.CalendarAgg

    module ACL =
        let toCreateCommand (cmd: CreateCalendarCmd) = Ok cmd
        let toAddEventCommand (cmd: AddCalendarEventCmd) = Ok cmd
        let toRemoveEventCommand (cmd: RemoveCalendarEventCmd) = Ok cmd
        let toClearCommand (cmd: ClearCalendarCmd) = Ok cmd
        let toActivateCommand (cmd: ActivateCalendarCmd) = Ok cmd
        let toDeactivateCommand (cmd: DeactivateCalendarCmd) = Ok cmd

    type Decision = Decision<Calendar, CalendarsEvent>

    type CalendarCapabilities =
        { Create: CreateCalendarCmd -> TaskResult<Decision, ApplicationError>
          AddEvent: AddCalendarEventCmd -> TaskResult<Decision, ApplicationError>
          RemoveEvent: RemoveCalendarEventCmd -> TaskResult<Decision, ApplicationError>
          Clear: ClearCalendarCmd -> TaskResult<Decision, ApplicationError>
          Activate: ActivateCalendarCmd -> TaskResult<Decision, ApplicationError>
          Deactivate: DeactivateCalendarCmd -> TaskResult<Decision, ApplicationError> }

    let createCapabilities (repo: Repository<Calendar, string, CalendarsEvent>) : CalendarCapabilities =
        { Create =
            liftCmdResult ACL.toCreateCommand
            >=> handleCommand (fun c -> CalendarId.value c.Id) repo CreateCalendar decide
          AddEvent =
            liftCmdResult ACL.toAddEventCommand
            >=> handleCommand (fun c -> CalendarId.value c.CalendarId) repo AddCalendarEvent decide
          RemoveEvent =
            liftCmdResult ACL.toRemoveEventCommand
            >=> handleCommand (fun c -> CalendarId.value c.CalendarId) repo RemoveCalendarEvent decide
          Clear =
            liftCmdResult ACL.toClearCommand
            >=> handleCommand (fun c -> CalendarId.value c.CalendarId) repo ClearCalendar decide
          Activate =
            liftCmdResult ACL.toActivateCommand
            >=> handleCommand (fun c -> CalendarId.value c.CalendarId) repo ActivateCalendar decide
          Deactivate =
            liftCmdResult ACL.toDeactivateCommand
            >=> handleCommand (fun c -> CalendarId.value c.CalendarId) repo DeactivateCalendar decide }

    let evolveProjection (state: Map<string, Calendar>) (evt: CalendarsEvent) =
        match evt with
        | CalendarCreated e ->
            let cid =
                match e.Id with
                | CalendarId id -> id

            match evolve None (CalendarCreated e) with
            | Some s -> Map.add cid s state
            | None -> state
        | CalendarEventAdded e ->
            let cid =
                match e.CalendarId with
                | CalendarId id -> id

            match Map.tryFind cid state with
            | Some existing ->
                match evolve (Some existing) (CalendarEventAdded e) with
                | Some updated -> Map.add cid updated state
                | None -> state
            | None -> state
        | CalendarEventRemoved e ->
            let cid =
                match e.CalendarId with
                | CalendarId id -> id

            match Map.tryFind cid state with
            | Some existing ->
                match evolve (Some existing) (CalendarEventRemoved e) with
                | Some updated -> Map.add cid updated state
                | None -> state
            | None -> state
        | CalendarCleared e ->
            let cid =
                match e.CalendarId with
                | CalendarId id -> id

            match Map.tryFind cid state with
            | Some existing ->
                match evolve (Some existing) (CalendarCleared e) with
                | Some updated -> Map.add cid updated state
                | None -> state
            | None -> state
        | CalendarActivated e ->
            let cid =
                match e.CalendarId with
                | CalendarId id -> id

            match Map.tryFind cid state with
            | Some existing ->
                match evolve (Some existing) (CalendarActivated e) with
                | Some updated -> Map.add cid updated state
                | None -> state
            | None -> state
        | CalendarDeactivated e ->
            let cid =
                match e.CalendarId with
                | CalendarId id -> id

            match Map.tryFind cid state with
            | Some existing ->
                match evolve (Some existing) (CalendarDeactivated e) with
                | Some updated -> Map.add cid updated state
                | None -> state
            | None -> state

    let createProjectionAgent () =
        ProjectionAgent<Map<string, Calendar>, CalendarsEvent>(evolveProjection, Map.empty, "CalendarReadModel")

// ----------------------------------------------------
// 2. CapacityBucket Application Capabilities & Projection
// ----------------------------------------------------
module CapacityApp =
    open Medhavi.Capacity.Domain.CapacityAgg

    type DefineBucketReq = PhysicalResourceId * Window * DurationMinutes

    type ReserveReq =
        CapacityReservationId *
        CapacityRequirementId *
        DurationMinutes *
        Timestamp option *
        Timestamp option *
        LoadSource *
        CapacityBucketId

    type CancelReq = CapacityReservationId * CapacityBucketId

    type CapacityEnvelope<'Cmd> =
        { BucketId: CapacityBucketId
          Command: 'Cmd }

    module ACL =
        let toDefineBucketCommand
            ((resId, window, total): DefineBucketReq)
            : Result<CapacityEnvelope<CapacityCommand>, DomainError> =
            Ok
                { BucketId = CapacityBucketId.create resId window
                  Command = DefineBucket(resId, window, total) }

        let toReserveCommand
            ((resId, reqId, minutes, startOpt, endOpt, source, bucketId): ReserveReq)
            : Result<CapacityEnvelope<CapacityCommand>, DomainError> =
            Ok
                { BucketId = bucketId
                  Command = ReserveCapacity(resId, reqId, minutes, startOpt, endOpt, source) }

        let toCancelCommand ((resId, bucketId): CancelReq) : Result<CapacityEnvelope<CapacityCommand>, DomainError> =
            Ok
                { BucketId = bucketId
                  Command = CancelReservation resId }

    type Decision = Decision<CapacityBucket, CapacityEvent>

    type CapacityCapabilities =
        { DefineBucket: DefineBucketReq -> TaskResult<Decision, ApplicationError>
          Reserve: ReserveReq -> TaskResult<Decision, ApplicationError>
          Cancel: CancelReq -> TaskResult<Decision, ApplicationError> }

    let createCapabilities (repo: Repository<CapacityBucket, string, CapacityEvent>) : CapacityCapabilities =
        { DefineBucket =
            liftCmdResult ACL.toDefineBucketCommand
            >=> handleCommand (fun env -> CapacityBucketId.value env.BucketId) repo (fun env -> env.Command) decide
          Reserve =
            liftCmdResult ACL.toReserveCommand
            >=> handleCommand (fun env -> CapacityBucketId.value env.BucketId) repo (fun env -> env.Command) decide
          Cancel =
            liftCmdResult ACL.toCancelCommand
            >=> handleCommand (fun env -> CapacityBucketId.value env.BucketId) repo (fun env -> env.Command) decide }

    let evolveProjection (state: Map<string, CapacityBucket>) (evt: CapacityEvent) =
        match evt with
        | BucketCreated e ->
            let bid = CapacityBucketId.value e.Id

            match evolve (BucketCreated e) None with
            | Some s -> Map.add bid s state
            | None -> state
        | CapacityReserved e ->
            let bid = CapacityBucketId.value e.BucketId

            match Map.tryFind bid state with
            | Some existing ->
                match evolve (CapacityReserved e) (Some existing) with
                | Some updated -> Map.add bid updated state
                | None -> state
            | None -> state
        | ReservationCancelled e ->
            let bid = CapacityBucketId.value e.BucketId

            match Map.tryFind bid state with
            | Some existing ->
                match evolve (ReservationCancelled e) (Some existing) with
                | Some updated -> Map.add bid updated state
                | None -> state
            | None -> state

    let createProjectionAgent () =
        ProjectionAgent<Map<string, CapacityBucket>, CapacityEvent>(
            evolveProjection,
            Map.empty,
            "CapacityBucketReadModel"
        )

// ----------------------------------------------------
// 3. Operation Application Capabilities & Projection
// ----------------------------------------------------
module OperationApp =
    open Medhavi.Capacity.Domain.OperationAgg

    module ACL =
        let toScheduleCommand (cmd: ScheduleOperationCmd) = Ok cmd
        let toStartCommand (cmd: StartOperationCmd) = Ok cmd
        let toCompleteCommand (cmd: CompleteOperationCmd) = Ok cmd
        let toCancelCommand (cmd: CancelOperationCmd) = Ok cmd

    type Decision = Decision<Operation, OperationEvent>

    type OperationCapabilities =
        { Schedule: ScheduleOperationCmd -> TaskResult<Decision, ApplicationError>
          Start: StartOperationCmd -> TaskResult<Decision, ApplicationError>
          Complete: CompleteOperationCmd -> TaskResult<Decision, ApplicationError>
          Cancel: CancelOperationCmd -> TaskResult<Decision, ApplicationError> }

    let createCapabilities (repo: Repository<Operation, string, OperationEvent>) : OperationCapabilities =
        { Schedule =
            liftCmdResult ACL.toScheduleCommand
            >=> handleCommand (fun c -> OperationId.value c.Id) repo ScheduleOperation decide
          Start =
            liftCmdResult ACL.toStartCommand
            >=> handleCommand (fun c -> OperationId.value c.Id) repo StartOperation decide
          Complete =
            liftCmdResult ACL.toCompleteCommand
            >=> handleCommand (fun c -> OperationId.value c.Id) repo CompleteOperation decide
          Cancel =
            liftCmdResult ACL.toCancelCommand
            >=> handleCommand (fun c -> OperationId.value c.Id) repo CancelOperation decide }

    let evolveProjection (state: Map<string, Operation>) (evt: OperationEvent) =
        match evt with
        | OperationScheduled e ->
            let oid = OperationId.value e.Id

            match evolve (OperationScheduled e) None with
            | Some s -> Map.add oid s state
            | None -> state
        | OperationStarted e ->
            let oid = OperationId.value e.Id

            match Map.tryFind oid state with
            | Some existing ->
                match evolve (OperationStarted e) (Some existing) with
                | Some updated -> Map.add oid updated state
                | None -> state
            | None -> state
        | OperationCompleted e ->
            let oid = OperationId.value e.Id

            match Map.tryFind oid state with
            | Some existing ->
                match evolve (OperationCompleted e) (Some existing) with
                | Some updated -> Map.add oid updated state
                | None -> state
            | None -> state
        | OperationCancelled e ->
            let oid = OperationId.value e.Id

            match Map.tryFind oid state with
            | Some existing ->
                match evolve (OperationCancelled e) (Some existing) with
                | Some updated -> Map.add oid updated state
                | None -> state
            | None -> state

    let createProjectionAgent () =
        ProjectionAgent<Map<string, Operation>, OperationEvent>(evolveProjection, Map.empty, "OperationReadModel")
