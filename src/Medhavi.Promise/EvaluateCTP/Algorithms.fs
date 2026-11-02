/// CA-P-005 Evaluate Capable-to-Promise Algorithms
/// Traces to: BA-P-003 Capable-to-Promise Manufacturing Scheduling
module Medhavi.Promise.EvaluateCTP.Algorithms

open System
open Medhavi.SemanticModel
open Medhavi.Promise
open Model
open Policies

let calculateCtpSchedule
    (policy: CapableToPromisePolicy)
    (bomComponents: BomComponentFact list)
    (capacitySlots: CapacitySlotFact list)
    (requestedQty: Quantity)
    (evaluationDate: Timestamp)
    : bool * Timestamp * int * string list * string option =

    let qtyVal = Quantity.value requestedQty

    // 1. Critical-path BOM component lead time
    let maxComponentLeadDays =
        if bomComponents.IsEmpty then 0
        else bomComponents |> List.map (fun b -> b.LeadTimeDays) |> List.max

    let componentStockouts =
        bomComponents
        |> List.filter (fun b -> b.LeadTimeDays > 14)
        |> List.map (fun b -> $"LongLeadTimeComponent: {ItemId.value b.ComponentItem} ({b.LeadTimeDays} days)")

    // 2. Required assembly hours
    let requiredHours =
        let baseHours = qtyVal * policy.StandardAssemblyHoursPerUnit
        let withBuffer = baseHours * (1.0m + (policy.CapacityBufferPct / 100.0m))
        Math.Max(policy.MinShiftHoursPerJob, Math.Round(withBuffer, 2))

    // 3. Find earliest available capacity slot after component arrival
    let compArrivalDate =
        let arr = (Timestamp.value evaluationDate).AddDays(float maxComponentLeadDays)
        Timestamp.create arr |> Result.defaultWith (fun _ -> evaluationDate)

    let eligibleSlots =
        capacitySlots
        |> List.filter (fun s ->
            Timestamp.value s.ShiftDate >= Timestamp.value compArrivalDate
            && s.AvailableCapacityHours >= requiredHours)
        |> List.sortBy (fun s -> Timestamp.value s.ShiftDate)

    match eligibleSlots with
    | firstSlot :: _ ->
        let finishDate =
            let fin = (Timestamp.value firstSlot.ShiftDate).AddDays(1.0)
            Timestamp.create fin |> Result.defaultWith (fun _ -> firstSlot.ShiftDate)
        let totalLeadDays = maxComponentLeadDays + int (Math.Ceiling((Timestamp.value finishDate - Timestamp.value evaluationDate).TotalDays))
        true, finishDate, totalLeadDays, componentStockouts, None

    | [] ->
        let bottleneck =
            if capacitySlots.IsEmpty then Some "NoPlannedShifts"
            else
                let minAvail = capacitySlots |> List.minBy (fun s -> s.AvailableCapacityHours)
                Some (PhysicalResourceId.value minAvail.Resource)
        let fallbackFinish =
            let fin = (Timestamp.value compArrivalDate).AddDays(14.0)
            Timestamp.create fin |> Result.defaultWith (fun _ -> compArrivalDate)
        false, fallbackFinish, maxComponentLeadDays + 14, ("CapacityDeficit" :: componentStockouts), bottleneck

