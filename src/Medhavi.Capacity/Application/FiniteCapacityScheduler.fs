namespace Medhavi.Capacity.Application

open System
open Medhavi.SharedKernel
open Medhavi.Capacity
open Medhavi.Capacity.Domain.CapacityAgg
open Medhavi.Capacity.Domain.CapacityResourceAgg

type CapacityOutcome =
    | FullyScheduled
    | PartiallyScheduled
    | Unscheduled

type CapacityViolation =
    | CapacityOverload of
        resourceId: string *
        bucketId: string option *
        date: DateOnly *
        requestedMinutes: decimal *
        availableMinutes: decimal
    | DueDateMiss of workOrderId: string * startDate: DateTimeOffset * now: DateTimeOffset
    | ResourceUnavailable of resourceId: string
    | CalendarViolation of calendarId: string * date: DateOnly

type CapacityError =
    | RoutingNotFound of productId: string
    | ResourceGroupNotFound of resourceGroupId: string
    | NoEligibleResource of stepId: string * target: LoadTarget

type ScheduledOperation =
    { StepId: string
      OperationCode: string
      SequenceNumber: int
      ResourceId: PhysicalResourceId
      Window: TimeWindow
      DurationMinutes: decimal }

type ProductionOrder =
    { WorkOrderId: string
      ProductId: string
      Quantity: decimal
      DueDate: DateTimeOffset
      Operations: ScheduledOperation list
      Violations: CapacityViolation list }

type CapacityResult =
    { ProductionOrder: ProductionOrder
      Reservations: CapacityReservation list
      Violations: CapacityViolation list
      Outcome: CapacityOutcome }

type CapacityPlanningRequest =
    { WorkOrderId: string
      ProductId: string
      Quantity: decimal
      DueDate: DateTimeOffset }

module Result =
    let get =
        function
        | Ok x -> x
        | Error e -> failwithf "Result was Error: %A" e

module FiniteCapacityScheduler =

    let scheduleWorkOrder
        (request: CapacityPlanningRequest)
        (routing: RoutingLoadProfile)
        (activeResources: Map<string, CapacityResource>)
        (bucketsState: Map<string, CapacityBucket>)
        (now: DateTimeOffset)
        (initialAllocations: Map<CapacityBucketId, decimal>)
        : Result<CapacityResult * Map<CapacityBucketId, decimal>, CapacityError> =

        let stepFlows = RoutingInterpreter.calculateStepFlows routing request.Quantity
        let steps = routing.StepLoads |> List.sortByDescending(fun s -> s.SequenceNumber)

        let rec scheduleSteps
            (remainingSteps: RoutingStepLoadProfile list)
            (currentEnd: DateTimeOffset)
            (allocations: Map<CapacityBucketId, decimal>)
            (violations: CapacityViolation list)
            (accOps: ScheduledOperation list)
            (accReservations: CapacityReservation list)
            : Result<
                  ScheduledOperation list *
                  CapacityReservation list *
                  Map<CapacityBucketId, decimal> *
                  CapacityViolation list,
                  CapacityError
               >
            =

            match remainingSteps with
            | [] -> Ok(accOps, accReservations, allocations, violations)
            | step :: rest ->
                match step.Loads |> List.tryHead with
                | None ->
                    // Step with no loads: skip resource reservation and keep end time
                    scheduleSteps rest currentEnd allocations violations accOps accReservations
                | Some load ->
                    // Resolve physical resource
                    let resourceOpt =
                        match load.Target with
                        | LoadTarget.WorkCenter(resId, _) ->
                            match Map.tryFind resId activeResources with
                            | Some res when res.IsActive -> Some res
                            | _ -> None
                        | LoadTarget.Resource(rgId, _) ->
                            let candidates =
                                activeResources
                                |> Map.toList
                                |> List.map snd
                                |> List.filter(fun r -> ResourceGroupId.value r.ResourceGroupId = rgId && r.IsActive)
                                |> List.sortByDescending(fun r -> Percent.value r.EffectiveEfficiency)

                            candidates |> List.tryHead

                    match resourceOpt with
                    | None -> Error(NoEligibleResource(step.RoutingStepId, load.Target))
                    | Some res ->
                        // Calculate duration
                        let setup = load.SetupLoadMinutes |> Option.defaultValue 0.0m
                        let teardown = load.TeardownLoadMinutes |> Option.defaultValue 0.0m
                        let baseQty = if routing.BaseQuantity <= 0.0m then 1.0m else routing.BaseQuantity
                        let stepQty = Map.tryFind step.RoutingStepId stepFlows |> Option.defaultValue request.Quantity
                        let runTime = load.RunLoadPerBaseQuantityMinutes * (stepQty / baseQty)
                        let totalLoadMins = setup + runTime + teardown

                        let eff = Percent.value res.EffectiveEfficiency
                        let efficiencyFactor = if eff <= 0.0m then 1.0m else eff
                        let durationMinutes = totalLoadMins / efficiencyFactor

                        if durationMinutes < 0.0m then
                            // If duration is invalid, we return NoEligibleResource or raise error.
                            // Let's treat it as NoEligibleResource to satisfy error union
                            Error(NoEligibleResource(step.RoutingStepId, load.Target))
                        else
                            let duration = TimeSpan.FromMinutes(float durationMinutes)
                            let startTime = currentEnd.Subtract(duration)

                            // 1. DueDateMiss violation check
                            let dueViolations =
                                if startTime < now then
                                    DueDateMiss(request.WorkOrderId, startTime, now) :: violations
                                else
                                    violations

                            // 2. CapacityOverload violation check
                            let targetDate = DateOnly.FromDateTime(startTime.Date)

                            let bucketOpt =
                                bucketsState
                                |> Map.toList
                                |> List.map snd
                                |> List.tryFind(fun b ->
                                    b.ResourceId = res.Id
                                    && let startVal = Timestamp.value b.Window.Start in
                                       DateOnly.FromDateTime(startVal.Date) = targetDate)

                            let bucketId, freeMinutes, nextAllocations =
                                match bucketOpt with
                                | Some b ->
                                    let currentAlloc = Map.tryFind b.Id allocations |> Option.defaultValue 0.0m
                                    let free = DurationMinutes.value b.FreeMinutes
                                    let nextAlloc = currentAlloc + durationMinutes
                                    Some b.Id, free, Map.add b.Id nextAlloc allocations
                                | None ->
                                    // Generate a synthetic bucket ID for tracking allocations
                                    let dayStart =
                                        DateTimeOffset(
                                            startTime.Year,
                                            startTime.Month,
                                            startTime.Day,
                                            0,
                                            0,
                                            0,
                                            startTime.Offset
                                        )

                                    let dayEnd = dayStart.AddDays(1.0)
                                    let win = TimeWindow.createFromTime dayStart dayEnd |> Result.get
                                    let syntheticBucketId = CapacityBucketId.create res.Id win

                                    let currentAlloc =
                                        Map.tryFind syntheticBucketId allocations |> Option.defaultValue 0.0m

                                    let free = 480.0m // Standard fallback capacity
                                    let nextAlloc = currentAlloc + durationMinutes
                                    None, free, Map.add syntheticBucketId nextAlloc allocations

                            let capViolations =
                                let currentAlloc =
                                    match bucketId with
                                    | Some bid -> Map.tryFind bid allocations |> Option.defaultValue 0.0m
                                    | None ->
                                        let dayStart =
                                            DateTimeOffset(
                                                startTime.Year,
                                                startTime.Month,
                                                startTime.Day,
                                                0,
                                                0,
                                                0,
                                                startTime.Offset
                                            )

                                        let dayEnd = dayStart.AddDays(1.0)
                                        let win = TimeWindow.createFromTime dayStart dayEnd |> Result.get
                                        let syntheticBucketId = CapacityBucketId.create res.Id win
                                        Map.tryFind syntheticBucketId allocations |> Option.defaultValue 0.0m

                                let remainingFree = freeMinutes - currentAlloc

                                if remainingFree < durationMinutes then
                                    let bidStr = bucketId |> Option.map CapacityBucketId.value

                                    CapacityOverload(
                                        PhysicalResourceId.value res.Id,
                                        bidStr,
                                        targetDate,
                                        durationMinutes,
                                        remainingFree
                                    )
                                    :: dueViolations
                                else
                                    dueViolations

                            // Create scheduled operation window
                            match TimeWindow.createFromTime startTime currentEnd with
                            | Error _ -> Error(NoEligibleResource(step.RoutingStepId, load.Target))
                            | Ok win ->
                                let scheduledOp =
                                    { StepId = step.RoutingStepId
                                      OperationCode = step.OperationCode
                                      SequenceNumber = step.SequenceNumber
                                      ResourceId = res.Id
                                      Window = win
                                      DurationMinutes = durationMinutes }

                                let actualBucketId =
                                    match bucketId with
                                    | Some bid -> bid
                                    | None ->
                                        let dayStart =
                                            DateTimeOffset(
                                                startTime.Year,
                                                startTime.Month,
                                                startTime.Day,
                                                0,
                                                0,
                                                0,
                                                startTime.Offset
                                            )

                                        let dayEnd = dayStart.AddDays(1.0)
                                        let win = TimeWindow.createFromTime dayStart dayEnd |> Result.get
                                        CapacityBucketId.create res.Id win

                                let reservation =
                                    { Id = CapacityReservationId.create $"RES-{Guid.NewGuid().ToString()}" |> Result.get
                                      RequirementId =
                                        CapacityRequirementId.create $"REQ-{Guid.NewGuid().ToString()}" |> Result.get
                                      ResourceId = res.Id
                                      BucketId = actualBucketId
                                      Minutes =
                                        DurationMinutes.create durationMinutes
                                        |> Result.defaultValue DurationMinutes.zero
                                      Start = Some(Timestamp startTime)
                                      End = Some(Timestamp currentEnd)
                                      Status = CapacityReservationStatus.Planned
                                      Source = LoadSource.FromScheduler
                                      CreatedAt = Timestamp.now
                                      ModifiedAt = Timestamp.now }

                                scheduleSteps
                                    rest
                                    startTime
                                    nextAllocations
                                    capViolations
                                    (scheduledOp :: accOps)
                                    (reservation :: accReservations)

        if List.isEmpty routing.StepLoads then
            Error(RoutingNotFound request.ProductId)
        else
            match scheduleSteps steps request.DueDate initialAllocations [] [] [] with
            | Error err -> Error err
            | Ok(ops, reservations, finalAllocations, violations) ->
                let prodOrder =
                    { WorkOrderId = request.WorkOrderId
                      ProductId = request.ProductId
                      Quantity = request.Quantity
                      DueDate = request.DueDate
                      Operations = ops
                      Violations = violations }

                let outcome =
                    if ops.IsEmpty then Unscheduled
                    elif violations.IsEmpty then FullyScheduled
                    else PartiallyScheduled

                let result =
                    { ProductionOrder = prodOrder
                      Reservations = reservations
                      Violations = violations
                      Outcome = outcome }

                Ok(result, finalAllocations)
