namespace Medhavi.Capacity.Application

open System
open System.Threading.Tasks
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

// ----------------------------------------------------
// 4. CapacityReservation Application Capabilities & Projection
// ----------------------------------------------------
module CapacityReservationApp =
    open Medhavi.Capacity.Domain.CapacityReservationAgg

    module ACL =
        let toCreateCommand (cmd: CreateReservationCmd) = Ok cmd
        let toReleaseCommand (cmd: ReleaseReservationCmd) = Ok cmd

    type Decision = Decision<CapacityReservation, CapacityReservationEvent>

    type CapacityReservationCapabilities =
        { Create: CreateReservationCmd -> TaskResult<Decision, ApplicationError>
          Release: ReleaseReservationCmd -> TaskResult<Decision, ApplicationError> }

    let createCapabilities (repo: Repository<CapacityReservation, string, CapacityReservationEvent>) : CapacityReservationCapabilities =
        { Create =
            liftCmdResult ACL.toCreateCommand
            >=> handleCommand (fun c -> CapacityReservationId.value c.Id) repo CreateReservation Reservation.decide
          Release =
            liftCmdResult ACL.toReleaseCommand
            >=> handleCommand (fun c -> CapacityReservationId.value c.Id) repo ReleaseReservation Reservation.decide }

    let evolveProjection (state: Map<string, CapacityReservation>) (evt: CapacityReservationEvent) =
        match evt with
        | CapacityReservationCreated e ->
            let rid = CapacityReservationId.value e.Id
            match Reservation.evolve (CapacityReservationCreated e) None with
            | Some s -> Map.add rid s state
            | None -> state
        | CapacityReservationReleased e ->
            let rid = CapacityReservationId.value e.Id
            match Map.tryFind rid state with
            | Some existing ->
                match Reservation.evolve (CapacityReservationReleased e) (Some existing) with
                | Some updated -> Map.add rid updated state
                | None -> state
            | None -> state

    let createProjectionAgent () =
        ProjectionAgent<Map<string, CapacityReservation>, CapacityReservationEvent>(
            evolveProjection,
            Map.empty,
            "CapacityReservationReadModel"
        )

// ----------------------------------------------------
// 5. CTP Scheduler & CheckCapacity Logic
// ----------------------------------------------------
module SchedulerApp =
    open Medhavi.Capacity
    open Medhavi.Capacity.Domain.CapacityAgg
    open Medhavi.Capacity.Domain.CalendarAgg
    open Medhavi.Capacity.Domain.CapacityResourceAgg
    open Medhavi.Capacity.RoutingInterpreter

    type CheckCapacityResult =
        { IsFeasible: bool
          SuggestedDate: DateTimeOffset
          RequiredLoads: Map<string, DurationMinutes>
          BottleneckResourceId: string option
          LatenessReason: string option }

    let checkCapacity
        (productId: string)
        (quantity: decimal)
        (needDate: DateTimeOffset)
        (planningMode: CapacityPlanningMode)
        (resourcesState: Map<string, CapacityResource>)
        (calendarState: Map<string, Calendar>)
        (bucketsState: Map<string, CapacityBucket>)
        (getRoutingsForProduct: string -> Task<Result<Medhavi.Contracts.Domain.Routing list, ApplicationError>>)
        : Task<Result<CheckCapacityResult, ApplicationError>> =
        task {
            let! routingsRes = getRoutingsForProduct productId
            match routingsRes with
            | Error err -> return Error err
            | Ok [] -> return Error (ApplicationError.Domain (DomainError.validation $"No routing found for product {productId}"))
            | Ok rawRoutings ->
                let loadProfiles = rawRoutings |> List.map RoutingAcl.translate
                let preferredOpt = 
                    loadProfiles 
                    |> List.filter (fun (r: RoutingLoadProfile) -> 
                        let raw = rawRoutings |> List.find (fun (rr: Medhavi.Contracts.Domain.Routing) -> rr.Id = r.RoutingId)
                        raw.Preference.IsPreferred)
                    |> List.tryHead
                    |> Option.orElse (loadProfiles |> List.sortBy (fun (r: RoutingLoadProfile) -> r.PreferencePriority) |> List.tryHead)
                
                match preferredOpt with
                | None -> return Error (ApplicationError.Domain (DomainError.validation $"No valid routing load profile found for product {productId}"))
                | Some (routing: RoutingLoadProfile) ->
                    let stepFlows = RoutingInterpreter.calculateStepFlows routing quantity
                    let steps = routing.StepLoads |> List.sortBy (fun (s: RoutingStepLoadProfile) -> s.SequenceNumber)
                    
                    let stepResourceMappings = 
                        steps
                        |> List.choose (fun (s: RoutingStepLoadProfile) ->
                            let firstLoadOpt = s.Loads |> List.tryHead
                            match firstLoadOpt with
                            | None -> None
                            | Some (load: CapacityRoutingLoad) ->
                                match load.Target with
                                | LoadTarget.Resource(rgId, _) ->
                                    let candidateRes = 
                                        resourcesState 
                                        |> Map.toList 
                                        |> List.map snd
                                        |> List.filter (fun (r: CapacityResource) -> ResourceGroupId.value r.ResourceGroupId = rgId && r.IsActive)
                                        |> List.sortByDescending (fun (r: CapacityResource) -> Percent.value r.EffectiveEfficiency)
                                        |> List.tryHead
                                    
                                    match candidateRes with
                                    | Some res -> 
                                        let setup = load.SetupLoadMinutes |> Option.defaultValue 0.0m
                                        let teardown = load.TeardownLoadMinutes |> Option.defaultValue 0.0m
                                        let baseQty = if routing.BaseQuantity <= 0.0m then 1.0m else routing.BaseQuantity
                                        let stepQty = Map.tryFind s.RoutingStepId stepFlows |> Option.defaultValue quantity
                                        let runTime = load.RunLoadPerBaseQuantityMinutes * (stepQty / baseQty)
                                        let totalLoadMins = setup + runTime + teardown
                                        
                                        let eff = Percent.value res.EffectiveEfficiency / 100.0m
                                        let totalLoadMinsEff = totalLoadMins / (if eff <= 0.0m then 1.0m else eff)
                                        
                                        Some (s.RoutingStepId, (PhysicalResourceId.value res.Id, totalLoadMinsEff))
                                    | None -> None
                                | LoadTarget.WorkCenter(resId, _) ->
                                    match Map.tryFind resId resourcesState with
                                    | Some (res: CapacityResource) when res.IsActive ->
                                        let setup = load.SetupLoadMinutes |> Option.defaultValue 0.0m
                                        let teardown = load.TeardownLoadMinutes |> Option.defaultValue 0.0m
                                        let baseQty = if routing.BaseQuantity <= 0.0m then 1.0m else routing.BaseQuantity
                                        let stepQty = Map.tryFind s.RoutingStepId stepFlows |> Option.defaultValue quantity
                                        let runTime = load.RunLoadPerBaseQuantityMinutes * (stepQty / baseQty)
                                        let totalLoadMins = setup + runTime + teardown
                                        
                                        let eff = Percent.value res.EffectiveEfficiency / 100.0m
                                        let totalLoadMinsEff = totalLoadMins / (if eff <= 0.0m then 1.0m else eff)
                                        Some (s.RoutingStepId, (PhysicalResourceId.value res.Id, totalLoadMinsEff))
                                    | _ -> None)
                        |> Map.ofList
                    
                    if stepResourceMappings.Count < steps.Length then
                        return Error (ApplicationError.Domain (DomainError.validation "Could not map all steps to active physical resources"))
                    else
                        let reqLoads = 
                            stepResourceMappings 
                            |> Map.toList 
                            |> List.map snd
                            |> List.groupBy fst
                            |> List.map (fun (resId, pairs) -> 
                                let totalMins = pairs |> List.sumBy snd
                                resId, DurationMinutes.create totalMins |> Result.defaultValue DurationMinutes.zero)
                            |> Map.ofList

                        match planningMode with
                        | CapacityPlanningMode.Infinite ->
                            let rec scheduleInfinite (idx: int) (currentEnd: DateTimeOffset) (acc: (string * DateTimeOffset * DateTimeOffset) list) =
                                if idx < 0 then Ok acc
                                else
                                    let step: RoutingStepLoadProfile = steps.[idx]
                                    let (resId, mins) = Map.find step.RoutingStepId stepResourceMappings
                                    let duration = TimeSpan.FromMinutes(float mins)
                                    let start = currentEnd.Subtract(duration)
                                    scheduleInfinite (idx - 1) start ((step.RoutingStepId, start, currentEnd) :: acc)
                            
                            match scheduleInfinite (steps.Length - 1) needDate [] with
                            | Error _ -> return Error (ApplicationError.Domain (DomainError.invariant "Failed to schedule infinite"))
                            | Ok scheduledSteps ->
                                let firstStart = scheduledSteps |> List.map (fun (_, s, _) -> s) |> List.min
                                let now = DateTimeOffset.UtcNow
                                if firstStart >= now then
                                    return Ok { IsFeasible = true
                                                SuggestedDate = needDate
                                                RequiredLoads = reqLoads
                                                BottleneckResourceId = None
                                                LatenessReason = None }
                                else
                                    let shift = now - firstStart
                                    let suggestedDate = needDate.Add(shift)
                                    return Ok { IsFeasible = false
                                                SuggestedDate = suggestedDate
                                                RequiredLoads = reqLoads
                                                BottleneckResourceId = None
                                                LatenessReason = Some ("Scheduling requires starting in the past (" + firstStart.ToString("yyyy-MM-dd HH:mm") + "). Shifted forward.") }
                        
                        | CapacityPlanningMode.Finite ->
                            let rec scheduleFiniteBackward (idx: int) (currentEnd: DateTimeOffset) (acc: (string * DateTimeOffset * DateTimeOffset) list) =
                                if idx < 0 then Some acc
                                else
                                    let step: RoutingStepLoadProfile = steps.[idx]
                                    let (resId, mins) = Map.find step.RoutingStepId stepResourceMappings
                                    
                                    let rec findCapacity (targetEnd: DateTimeOffset) (remainingMins: decimal) (accDays: (DateTimeOffset * DateTimeOffset) list) =
                                        let now = DateTimeOffset.UtcNow
                                        if targetEnd < now then None
                                        elif remainingMins <= 0.0m then
                                            let start = accDays |> List.map fst |> List.min
                                            let endVal = accDays |> List.map snd |> List.max
                                            Some (start, endVal)
                                        else
                                            let dayStart = DateTimeOffset(targetEnd.Year, targetEnd.Month, targetEnd.Day, 0, 0, 0, targetEnd.Offset)
                                            let bucketKey = resId + ":" + dayStart.ToString("yyyy-MM-dd")
                                            let freeMins = 
                                                match Map.tryFind bucketKey bucketsState with
                                                | Some b -> DurationMinutes.value b.FreeMinutes
                                                | None -> 480.0m
                                            
                                            if freeMins <= 0.0m then
                                                findCapacity (dayStart.AddDays(-1.0)) remainingMins accDays
                                            else
                                                let allocated = min remainingMins freeMins
                                                let dayEnd = dayStart.AddDays(1.0)
                                                findCapacity (dayStart.AddDays(-1.0)) (remainingMins - allocated) ((dayStart, dayEnd) :: accDays)
                                    
                                    match findCapacity currentEnd mins [] with
                                    | None -> None
                                    | Some (start, endVal) ->
                                        scheduleFiniteBackward (idx - 1) start ((step.RoutingStepId, start, endVal) :: acc)
                            
                            match scheduleFiniteBackward (steps.Length - 1) needDate [] with
                            | Some scheduledSteps ->
                                return Ok { IsFeasible = true
                                            SuggestedDate = needDate
                                            RequiredLoads = reqLoads
                                            BottleneckResourceId = None
                                            LatenessReason = None }
                            | None ->
                                let rec scheduleFiniteForward (idx: int) (currentStart: DateTimeOffset) (acc: (string * DateTimeOffset * DateTimeOffset) list) =
                                    if idx >= steps.Length then Some acc
                                    else
                                        let step: RoutingStepLoadProfile = steps.[idx]
                                        let (resId, mins) = Map.find step.RoutingStepId stepResourceMappings
                                        
                                        let rec findCapacityForward (targetStart: DateTimeOffset) (remainingMins: decimal) (accDays: (DateTimeOffset * DateTimeOffset) list) =
                                            if remainingMins <= 0.0m then
                                                let start = accDays |> List.map fst |> List.min
                                                let endVal = accDays |> List.map snd |> List.max
                                                Some (start, endVal)
                                            else
                                                let dayStart = DateTimeOffset(targetStart.Year, targetStart.Month, targetStart.Day, 0, 0, 0, targetStart.Offset)
                                                let bucketKey = resId + ":" + dayStart.ToString("yyyy-MM-dd")
                                                let freeMins = 
                                                    match Map.tryFind bucketKey bucketsState with
                                                    | Some b -> DurationMinutes.value b.FreeMinutes
                                                    | None -> 480.0m
                                                
                                                if freeMins <= 0.0m then
                                                    findCapacityForward (dayStart.AddDays(1.0)) remainingMins accDays
                                                else
                                                    let allocated = min remainingMins freeMins
                                                    let dayEnd = dayStart.AddDays(1.0)
                                                    findCapacityForward (dayStart.AddDays(1.0)) (remainingMins - allocated) ((dayStart, dayEnd) :: accDays)
                                        
                                        match findCapacityForward currentStart mins [] with
                                        | None -> None
                                        | Some (start, endVal) ->
                                            scheduleFiniteForward (idx + 1) endVal ((step.RoutingStepId, start, endVal) :: acc)
                                
                                let now = DateTimeOffset.UtcNow
                                match scheduleFiniteForward 0 now [] with
                                | None ->
                                    return Error (ApplicationError.Domain (DomainError.invariant "Could not find sufficient capacity forward in 30 days window"))
                                | Some scheduledSteps ->
                                    let suggestedEnd = scheduledSteps |> List.map (fun (_, _, e) -> e) |> List.max
                                    let bottleneck = 
                                        stepResourceMappings 
                                        |> Map.toList 
                                        |> List.map (fun (_, (resId, _)) -> resId)
                                        |> List.tryHead
                                    
                                    return Ok { IsFeasible = false
                                                SuggestedDate = suggestedEnd
                                                RequiredLoads = reqLoads
                                                BottleneckResourceId = bottleneck
                                                LatenessReason = Some ("Insufficient capacity. Earliest completion: " + suggestedEnd.ToString("yyyy-MM-dd HH:mm") + ".") }
        }
