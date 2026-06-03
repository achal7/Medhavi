module Medhavi.Scheduler.Mrp.Domain.Algorithms.SafetyStock

open System
open Medhavi.SharedKernel

// ============================================================================
// Z-SCORE LOOKUP
// ============================================================================

/// Service level to z-score mapping (standard normal distribution)
/// Pure function — no side effects
let getZScore (serviceLevel: float) : float =
    match serviceLevel with
    | sl when sl <= 0.50 -> 0.0
    | 0.80 -> 0.84
    | 0.85 -> 1.04
    | 0.90 -> 1.28
    | 0.95 -> 1.65
    | 0.96 -> 1.75
    | 0.97 -> 1.88
    | 0.98 -> 2.05
    | 0.99 -> 2.33
    | 0.995 -> 2.58
    | sl when sl >= 0.999 -> 3.09
    | sl ->
        // Linear interpolation for intermediate values
        if sl < 0.95 then
            0.84 + (sl - 0.80) * (1.65 - 0.84) / (0.95 - 0.80)
        elif sl < 0.99 then
            1.65 + (sl - 0.95) * (2.33 - 1.65) / (0.99 - 0.95)
        else
            2.33
            + (sl - 0.99) * (3.09 - 2.33) / (0.999 - 0.99)

// ============================================================================
// DEMAND STATISTICS
// ============================================================================

/// Calculate standard deviation from historical demand data
/// Pure function — no side effects
let calculateDemandStdDev (demandHistory: decimal list) : decimal =
    if
        List.isEmpty demandHistory
        || demandHistory.Length < 2
    then
        0m
    else
        let mean =
            (List.sum demandHistory)
            / decimal (List.length demandHistory)

        let variance =
            demandHistory
            |> List.map (fun d -> (d - mean) * (d - mean))
            |> List.sum
            |> (fun sum -> sum / decimal (List.length demandHistory))

        let stdDev = decimal (sqrt (float variance))
        Math.Round(stdDev, 2)

/// Calculate average lead time in days from historical data
let calculateAverageLeadTime (leadTimes: TimeSpan list) : float =
    if List.isEmpty leadTimes then
        0.0
    else
        leadTimes
        |> List.map (fun lt -> lt.TotalDays)
        |> List.average

// ============================================================================
// SAFETY STOCK CALCULATION
// ============================================================================

/// Calculate safety stock using service level formula
/// Formula: safetyStock = zScore × demandStdDev × √leadTime
/// Pure function — no side effects
let calculateSafetyStock (serviceLevel: float) (demandStdDev: decimal) (leadTimeDays: float) : Quantity =
    if demandStdDev <= 0m || leadTimeDays <= 0.0 then
        Quantity.Zero
    else
        let zScore = getZScore serviceLevel
        let sqrtLeadTime = sqrt leadTimeDays
        let safetyStock = float demandStdDev * zScore * sqrtLeadTime
        let rounded = Math.Round(decimal safetyStock, 2)
        Quantity.clampToZero rounded

// ============================================================================
// PARAMETER-BASED CALCULATION
// ============================================================================

/// Safety stock calculation parameters
type SafetyStockParameters =
    { ServiceLevel: float option // e.g., 0.95 for 95%
      DemandStdDev: decimal option // Pre-calculated standard deviation
      LeadTimeDays: float option // Known average lead time
      DemandHistory: decimal list option // Historical demand for auto std dev calculation
      LeadTimeHistory: TimeSpan list option // Historical lead times for auto average
      StaticOverride: Quantity option } // Static safety stock (takes precedence)

module SafetyStockParameters =
    let empty =
        { ServiceLevel = None
          DemandStdDev = None
          LeadTimeDays = None
          DemandHistory = None
          LeadTimeHistory = None
          StaticOverride = None }

/// Calculate safety stock from parameters
/// Static override takes precedence over dynamic calculation
/// Pure function — no side effects
let calculateFromParameters (parameters: SafetyStockParameters) : Quantity =
    // Static override takes precedence
    match parameters.StaticOverride with
    | Some staticValue -> staticValue
    | None ->
        match parameters.ServiceLevel with
        | Some serviceLevel ->
            // Calculate demand std dev if not provided
            let demandStdDev =
                match parameters.DemandStdDev with
                | Some stdDev -> stdDev
                | None ->
                    match parameters.DemandHistory with
                    | Some history -> calculateDemandStdDev history
                    | None -> 0m

            // Calculate lead time if not provided
            let leadTimeDays =
                match parameters.LeadTimeDays with
                | Some days -> days
                | None ->
                    match parameters.LeadTimeHistory with
                    | Some leadTimes -> calculateAverageLeadTime leadTimes
                    | None -> 0.0

            if demandStdDev > 0m && leadTimeDays > 0.0 then
                calculateSafetyStock serviceLevel demandStdDev leadTimeDays
            else
                Quantity.Zero
        | None -> Quantity.Zero
