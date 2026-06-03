/// Lot Sizing Algorithms — Pure functions for order quantity optimization
module Medhavi.Scheduler.Mrp.Domain.Algorithms.LotSizing

open System
open Medhavi.SharedKernel
open Medhavi.Scheduler.Mrp.Domain.Policies

// ============================================================================
// LOT-FOR-LOT
// ============================================================================

/// Order exactly the net requirement — minimizes inventory, maximizes orders
let lotForLot (netRequirement: Quantity) : Quantity = netRequirement

// ============================================================================
// FIXED LOT SIZE
// ============================================================================

/// Round up net requirement to fixed lot size multiples
/// Example: lotSize=100, netReq=250 → 300
let fixedLot (lotSize: Quantity) (netRequirement: Quantity) : Quantity =
    let lotVal = Quantity.value lotSize
    let reqVal = Quantity.value netRequirement

    if lotVal <= 0m then
        netRequirement
    else
        let multiples = Math.Ceiling(reqVal / lotVal)
        Quantity.clampToZero (multiples * lotVal)

// ============================================================================
// MINIMUM LOT SIZE
// ============================================================================

/// Ensure order is at least the minimum quantity
let minimumLot (minQty: Quantity) (netRequirement: Quantity) : Quantity = Quantity.maxOf minQty netRequirement

// ============================================================================
// ECONOMIC ORDER QUANTITY (EOQ)
// ============================================================================

/// Classic Wilson EOQ formula: sqrt(2 * D * S / H)
/// D = annual demand, S = ordering/setup cost per order, H = holding cost per unit per year
let eoq (annualDemand: Quantity) (orderingCost: PositiveDecimal) (holdingCost: PositiveDecimal) : Quantity =
    if
        annualDemand.IsZero
        || orderingCost.IsZero
        || holdingCost.IsZero
    then
        Quantity.Zero
    else
        let eoqValue =
            Math.Sqrt(
                float (
                    2m
                    * Quantity.value annualDemand
                    * PositiveDecimal.value orderingCost
                    / PositiveDecimal.value holdingCost
                )
            )

        let rounded = Math.Ceiling(decimal eoqValue)
        Quantity.clampToZero rounded

/// Apply EOQ: if net requirement < EOQ, order EOQ; otherwise order net requirement
let applyEoq
    (annualDemand: Quantity)
    (orderingCost: PositiveDecimal)
    (holdingCost: PositiveDecimal)
    (netRequirement: Quantity)
    : Quantity =
    let eoqQty = eoq annualDemand orderingCost holdingCost
    Quantity.maxOf eoqQty netRequirement

// ============================================================================
// SILVER-MEAL HEURISTIC
// ============================================================================

/// Silver-Meal: minimize average cost per period
/// Takes a list of per-period demands and returns the number of periods to combine
let silverMealPeriods (orderingCost: PositiveDecimal) (holdingCost: PositiveDecimal) (demands: Quantity list) : int =
    if
        List.isEmpty demands
        || orderingCost.IsZero
        || holdingCost.IsZero
    then
        1
    else
        let rec findOptimal periodCount prevAvgCost =
            if periodCount > List.length demands then
                periodCount - 1
            else
                // Calculate total cost for combining 'periodCount' periods
                let holdingTotal =
                    demands
                    |> List.take periodCount
                    |> List.mapi (fun i q -> Quantity.value q * holdingCost * decimal i)
                    |> List.sum

                let totalCost = orderingCost + holdingTotal
                let avgCost = totalCost / decimal periodCount

                if avgCost > prevAvgCost then
                    periodCount - 1 // Previous was better
                else
                    findOptimal (periodCount + 1) avgCost

        findOptimal 1 PositiveDecimal.MaxValue |> max 1

// ============================================================================
// PERIOD ORDER QUANTITY (POQ)
// ============================================================================

/// Combine demands for N periods into a single order
let periodOrderQuantity (periods: int) (demands: Quantity list) : Quantity =
    demands
    |> List.truncate (max 1 periods)
    |> List.fold (+) Quantity.Zero

// ============================================================================
// ROUNDING
// ============================================================================

/// Round quantity to nearest lot size multiple (up or down)
let roundToLot (lotSize: Quantity) (roundUp: bool) (quantity: Quantity) : Quantity =
    let lotVal = Quantity.value lotSize
    let qtyVal = Quantity.value quantity

    if lotVal <= 0m then
        quantity
    else
        let rounded =
            if roundUp then
                Math.Ceiling(qtyVal / lotVal) * lotVal
            else
                Math.Round(qtyVal / lotVal, MidpointRounding.AwayFromZero)
                * lotVal

        Quantity.clampToZero (max rounded lotVal) // At least one lot

// ============================================================================
// COMPOSITE POLICY APPLICATION
// ============================================================================

/// Apply the configured lot sizing policy to a net requirement
let applyPolicy (policy: LotSizingPolicy) (netRequirement: Quantity) (futureDemands: Quantity list) : Quantity =
    if Quantity.isZero netRequirement then
        Quantity.Zero
    else
        match policy with
        | LotForLot -> lotForLot netRequirement

        | FixedLot lotSize -> fixedLot lotSize netRequirement

        | MinimumLot minQty -> minimumLot minQty netRequirement

        | EOQ(annualDemand, orderingCost, holdingCost) -> applyEoq annualDemand orderingCost holdingCost netRequirement

        | SilverMeal(orderingCost, holdingCost) ->
            let periodsToOrder =
                silverMealPeriods orderingCost holdingCost (netRequirement :: futureDemands)

            periodOrderQuantity periodsToOrder (netRequirement :: futureDemands)

        | PeriodOrderQuantity periods -> periodOrderQuantity periods (netRequirement :: futureDemands)

        | RoundingLot(lotSize, roundUp) -> roundToLot lotSize roundUp netRequirement

/// Apply lot sizing with optional min/max constraints from netting policy
let applyWithConstraints
    (lotSizingPolicy: LotSizingPolicy option)
    (minQty: Quantity option)
    (maxQty: Quantity option)
    (netRequirement: Quantity)
    (futureDemands: Quantity list)
    : Quantity =

    // Step 1: Apply lot sizing
    let sized =
        match lotSizingPolicy with
        | Some policy -> applyPolicy policy netRequirement futureDemands
        | None -> lotForLot netRequirement

    // Step 2: Apply min constraint
    let withMin =
        match minQty with
        | Some min -> Quantity.maxOf min sized
        | None -> sized

    // Step 3: Apply max constraint
    match maxQty with
    | Some max -> Quantity.minOf max withMin
    | None -> withMin
