module Medhavi.DecisionCore.Feasibility

open Medhavi.SharedKernel
open System

// ----- Input types -----

type SupplySnapshot = {
    OnHand: Map<string, PositiveDecimal>          // SkuId -> quantity on hand
    Inbound: Map<string, (PositiveDecimal * Timestamp) list> // SkuId -> list of (qty, expected date)
}

type FeasibilityInput = {
    DemandQty: PositiveDecimal
    RequestedDate: Timestamp
    SkuId: string
    SupplySnapshot: SupplySnapshot
    ActiveReservations: Reservation list
    TimeWindow: Timestamp
}

type LimiterDomain =
    | Material
    | Capacity
    | Transport
    | Supplier
    | Policy
    | Other of string

type LimiterSeverity =
    | Hard   // cannot be violated
    | Soft   // can be violated with penalty

type Limiter = {
    Domain: LimiterDomain
    Severity: LimiterSeverity
    Code: string
    Message: string
}

type FeasibilityResult =
    | Feasible of earliestDate: Timestamp * confidence: float
    | PartiallyFeasible of quantity: PositiveDecimal * date: Timestamp
    | Infeasible of reason: Limiter list

type ATPResult = {
    Feasible: bool
    EarliestDate: Timestamp option
    AvailableQuantity: PositiveDecimal
    Sources: (string * PositiveDecimal) list   // (source description, quantity)
    Limiters: Limiter list
    Confidence: float
}

type CTPResult = {
    Feasible: bool
    EarliestProductionDate: Timestamp option
    EarliestDeliveryDate: Timestamp option
    RequiredMaterials: (string * PositiveDecimal) list
    Limiters: Limiter list
    Confidence: float
}

module Feasibility =

    let checkATP (input: FeasibilityInput) : FeasibilityResult =
        let onHand =
            input.SupplySnapshot.OnHand
            |> Map.tryFind input.SkuId
            |> Option.defaultValue PositiveDecimal.Zero

        let inboundQty =
            input.SupplySnapshot.Inbound
            |> Map.tryFind input.SkuId
            |> Option.defaultValue []
            |> List.filter (fun (_, d) -> d <= input.RequestedDate)
            |> List.sumBy fst

        let reservedQty =
            input.ActiveReservations
            |> List.filter (fun r -> r.SkuId = input.SkuId && r.Status <> ReservationStatus.Released && r.Status <> ReservationStatus.Expired)
            |> List.sumBy (fun r -> r.Quantity)

        let netAvailable = onHand + inboundQty - reservedQty

        if netAvailable >= input.DemandQty then
            Feasible (input.RequestedDate, 1.0)
        elif netAvailable > PositiveDecimal.Zero then
            PartiallyFeasible (netAvailable, input.RequestedDate)
        else
            Infeasible [
                { Domain = Material
                  Severity = Hard
                  Code = "ATP_MATERIAL_SHORTAGE"
                  Message = $"Insufficient material for SKU {input.SkuId}" }
            ]

    let checkCTP (input: FeasibilityInput) : FeasibilityResult =
        // Placeholder: real CTP checks capacity and material availability.
        // Here we just use a simple horizon check.
        let productionHorizon = Timestamp.create (DateTimeOffset.UtcNow.AddDays(14.0))
        let earliestProduction = Timestamp.create (DateTimeOffset.UtcNow.AddDays(3.0))

        if input.RequestedDate >= productionHorizon then
            Infeasible [
                { Domain = Capacity
                  Severity = Hard
                  Code = "CTP_CAPACITY_EXCEEDED"
                  Message = "Requested date beyond CTP horizon" }
            ]
        elif input.RequestedDate >= earliestProduction then
            Feasible (input.RequestedDate, 0.9)
        else
            PartiallyFeasible (input.DemandQty, earliestProduction)

    let composeFeasibility (results: FeasibilityResult list) : FeasibilityResult =
        let folder acc next =
            match acc, next with
            | Infeasible _, _ -> acc
            | _, Infeasible _ -> next
            | PartiallyFeasible (q1, d1), PartiallyFeasible (q2, d2) ->
                PartiallyFeasible (min q1 q2, max d1 d2)
            | Feasible (d1, c1), Feasible (d2, c2) ->
                Feasible (max d1 d2, min c1 c2)
            | Feasible _, PartiallyFeasible _ -> next
            | PartiallyFeasible _, Feasible _ -> acc

        match results with
        | [] -> Feasible (Timestamp.minValue, 1.0)
        | h :: t -> List.fold folder h t

    let determineAcceptability (result: FeasibilityResult) (minConfidence: float) (maxDate: Timestamp) =
        match result with
        | Feasible (date, confidence) -> confidence >= minConfidence && date <= maxDate
        | _ -> false
