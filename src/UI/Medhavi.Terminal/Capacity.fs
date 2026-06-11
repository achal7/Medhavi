module Medhavi.Terminal.Capacity

open System
open Medhavi.Terminal
open Medhavi.Capacity
open Medhavi.Capacity.Domain.CapacityResourceAgg
open Medhavi.Capacity.Domain.CapacityAgg
open Medhavi.Infrastructure.Projections
open Medhavi.SharedKernel
open Medhavi.Capacity.Application

let runCapacityCheckDemo capacityContext (masterDataContext: Medhavi.MasterData.MasterData) printer =
    printer.PrintLine Bold "\n--- [CTP CAPACITY CHECK DEMO] ---"

    let productId = "SKU-FRAME"
    let quantity = 10.0m
    let needDate = DateTimeOffset.UtcNow.AddDays(5.0)

    printfn
        "Running check for Product=%s, Qty=%M, NeedDate=%s"
        productId
        quantity
        (needDate.ToString("yyyy-MM-dd HH:mm"))

    let getRoutings productId =
        task {
            let! list = masterDataContext.Queries.Routing.GetAll()

            let filtered =
                list
                |> List.filter (fun r ->
                    match r.Details with
                    | Medhavi.Contracts.Domain.RoutingDetails.Work work -> work.ProductId = productId
                    | _ -> false)

            return Ok filtered
        }

    let resources =
        capacityContext.CapacityResourceAgent
            .GetStateAsync()
            .Result

    let calendars =
        capacityContext.CalendarAgent
            .GetStateAsync()
            .Result

    let buckets =
        capacityContext.CapacityAgent
            .GetStateAsync()
            .Result

    // 1. Run Infinite check
    let checkInfinite =
        SchedulerApp.checkCapacity
            productId
            quantity
            needDate
            CapacityPlanningMode.Infinite
            resources
            calendars
            buckets
            getRoutings
        |> Async.AwaitTask
        |> Async.RunSynchronously

    match checkInfinite with
    | Error err -> printer.PrintLine PrinterColor.Red (sprintf "Infinite capacity check failed: %A" err)
    | Ok res ->
        printer.PrintLine PrinterColor.Green "\n--- INFINITE CAPACITY CHECK RESULT ---"
        printfn "  Is Feasible: %b" res.IsFeasible
        printfn "  Suggested Date: %s" (res.SuggestedDate.ToString("yyyy-MM-dd HH:mm"))

        res.LatenessReason
        |> Option.iter (fun r -> printfn "  Reason: %s" r)

        printfn "  Required Loads:"

        for KeyValue(resId, dm) in res.RequiredLoads do
            printfn "    - %s: %Mm" resId (DurationMinutes.value dm)

    // 2. Run Finite check
    let checkFinite =
        SchedulerApp.checkCapacity
            productId
            quantity
            needDate
            CapacityPlanningMode.Finite
            resources
            calendars
            buckets
            getRoutings
        |> Async.AwaitTask
        |> Async.RunSynchronously

    match checkFinite with
    | Error err -> printer.PrintLine PrinterColor.Red (sprintf "Finite capacity check failed: %A" err)
    | Ok res ->
        printer.PrintLine PrinterColor.Green "\n--- FINITE CAPACITY CHECK RESULT ---"
        printfn "  Is Feasible: %b" res.IsFeasible
        printfn "  Suggested Date: %s" (res.SuggestedDate.ToString("yyyy-MM-dd HH:mm"))

        res.LatenessReason
        |> Option.iter (fun r -> printfn "  Reason: %s" r)

        printfn "  Bottleneck Resource: %A" res.BottleneckResourceId
        printfn "  Required Loads:"

        for KeyValue(resId, dm) in res.RequiredLoads do
            printfn "    - %s: %Mm" resId (DurationMinutes.value dm)

let showData capacityContext printer =
    let capResources: CapacityResource list =
        QueryServiceBase.getAll capacityContext.CapacityResourceAgent
        |> fun t -> t.Result

    let capResRows =
        capResources
        |> List.map (fun (r: CapacityResource) ->
            let costStr =
                r.EffectiveCostRate
                |> Option.map (fun c -> c.ToString())
                |> Option.defaultValue "-"

            let calStr =
                r.EffectiveCalendarId
                |> Option.map CalendarId.value
                |> Option.defaultValue "-"

            [| PhysicalResourceId.value r.Id
               StandardResourceId.value r.StandardResourceId
               ResourceGroupId.value r.ResourceGroupId
               r.Name
               (if r.IsActive then "Active" else "Inactive")
               (Percent.value r.EffectiveEfficiency).ToString()
               + "%"
               costStr
               calStr |])
        |> List.toArray

    Printer.printTable
        printer
        "CAPACITY RESOURCES (CLEAN BOUNDED VIEW WITH HIERARCHICAL FALLBACKS)"
        [| "RESOURCE ID"
           "STD RESOURCE ID"
           "GROUP ID"
           "NAME"
           "STATUS"
           "EFFICIENCY"
           "COST RATE"
           "CALENDAR ID" |]
        capResRows

    // 11.7 Capacity Buckets in Database
    let capBuckets: CapacityBucket list =
        QueryServiceBase.getAll capacityContext.CapacityAgent
        |> fun t -> t.Result

    let bucketRows =
        capBuckets
        |> List.map (fun (b: CapacityBucket) ->
            let startStr =
                (Timestamp.value b.Window.Start)
                    .ToString("yyyy-MM-dd HH:mm")

            let endStr =
                (Timestamp.value b.Window.End)
                    .ToString("yyyy-MM-dd HH:mm")

            [| CapacityBucketId.value b.Id
               PhysicalResourceId.value b.ResourceId
               $"{startStr} to {endStr}"
               (DurationMinutes.value b.AvailableMinutes)
                   .ToString()
               + "m"
               (DurationMinutes.value b.PlannedMinutes)
                   .ToString()
               + "m"
               (DurationMinutes.value b.FreeMinutes).ToString()
               + "m"
               b.Status.ToString() |])
        |> List.toArray

    Printer.printTable
        printer
        "CAPACITY BUCKETS IN DATABASE"
        [| "BUCKET ID"
           "RESOURCE ID"
           "WINDOW"
           "AVAILABLE"
           "PLANNED"
           "FREE"
           "STATUS" |]
        bucketRows
