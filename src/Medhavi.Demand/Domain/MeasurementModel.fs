module Medhavi.Demand.Domain.MeasurementModel

open System

type ForecastActual =
    { Period: string
      ForecastQuantity: decimal
      ActualQuantity: decimal }

type ForecastCycle =
    { CycleId: string
      GeneratedAt: DateTimeOffset
      Forecasts: Map<string, decimal> } // Period -> Quantity

type AccuracyThresholds =
    { ExcellentMax: decimal
      GoodMax: decimal
      AcceptableMax: decimal }

    static member Default() =
        { ExcellentMax = 5m
          GoodMax = 10m
          AcceptableMax = 20m }

    static member DefaultForMAPE() =
        { ExcellentMax = 10m
          GoodMax = 20m
          AcceptableMax = 30m }

type BiasThresholds =
    { ExcellentMaxPercent: decimal
      AcceptableMaxPercent: decimal }

    static member Default() =
        { ExcellentMaxPercent = 2m
          AcceptableMaxPercent = 5m }

// =============================================================================
// PI-DI-001: Demand Intelligence Effectiveness (reserved)
// =============================================================================

let demandIntelligenceEffectiveness () : unit = ()

// =============================================================================
// PI-DI-003: Weighted Absolute Percentage Error (WAPE)
// =============================================================================

let wape (data: ForecastActual seq) : decimal =
    let totalAbsoluteError = data |> Seq.sumBy(fun fa -> Math.Abs(fa.ForecastQuantity - fa.ActualQuantity))
    let totalActual = data |> Seq.sumBy(fun fa -> fa.ActualQuantity)

    if totalActual = 0m then
        failwith "[PI-DI-003] - Total actual demand is zero; WAPE undefined"

    (totalAbsoluteError / totalActual) * 100m

// =============================================================================
// PI-DI-002: Forecast Accuracy (100 − WAPE)
// =============================================================================

let forecastAccuracy (wapeValue: decimal) : decimal = 100m - wapeValue

// =============================================================================
// PI-DI-004: Mean Absolute Percentage Error (MAPE)
// =============================================================================

let mape (data: ForecastActual seq) : decimal =
    let percentageErrors =
        data
        |> Seq.choose(fun fa ->
            if fa.ActualQuantity = 0m then
                None
            else
                Some(Math.Abs(fa.ForecastQuantity - fa.ActualQuantity) / fa.ActualQuantity * 100m))

    let count = Seq.length percentageErrors

    if count = 0 then
        failwith "[PI-DI-004] - No non-zero actuals; MAPE undefined"

    (percentageErrors |> Seq.sum) / decimal count

// =============================================================================
// PI-DI-005: Forecast Bias (absolute and percentage)
// =============================================================================

let forecastBiasAbsolute (data: ForecastActual seq) : decimal =
    let totalError = data |> Seq.sumBy(fun fa -> fa.ForecastQuantity - fa.ActualQuantity)
    let count = Seq.length data
    if count = 0 then 0m else totalError / decimal count

let forecastBiasPercent (data: ForecastActual seq) : decimal =
    let biasAbs = forecastBiasAbsolute data
    let avgActual = data |> Seq.averageBy(fun fa -> fa.ActualQuantity)
    if avgActual = 0m then 0m else (biasAbs / avgActual) * 100m

// =============================================================================
// PI-DI-006: Forecast Value Added (FVA)
// =============================================================================

let fva (wapeNaive: decimal) (wapeProcess: decimal) : decimal = wapeNaive - wapeProcess

// =============================================================================
// PI-DI-007: Forecast Stability
// =============================================================================

let forecastStability (cycles: ForecastCycle list) : decimal =
    let pairs =
        cycles
        |> List.pairwise
        |> List.collect(fun (prev, curr) ->
            prev.Forecasts
            |> Map.toList
            |> List.choose(fun (period, prevQty) ->
                curr.Forecasts.TryFind period |> Option.map(fun currQty -> (prevQty, currQty))))

    match pairs with
    | [] -> 0m
    | _ ->
        let stabilityErrors =
            pairs
            |> List.map(fun (prevQty, currQty) ->
                if prevQty = 0m then 0m else Math.Abs(currQty - prevQty) / prevQty * 100m)

        (stabilityErrors |> List.sum) / decimal(List.length pairs)

// =============================================================================
// PI-DI-008: Forecast Value Realization
// =============================================================================

let forecastValueRealization (actualCompositeValue: decimal) (maxPotentialValue: decimal) : decimal =
    if maxPotentialValue = 0m then
        0m
    else
        (actualCompositeValue / maxPotentialValue) * 100m

// =============================================================================
// PI-DI-009: Demand Plan Adherence
// =============================================================================

let demandPlanAdherence (planEntries: (decimal * decimal) seq) (tolerancePercent: decimal) : decimal =
    let totalPlanned, totalExecuted =
        planEntries
        |> Seq.fold
            (fun (tp, te) (planned, executed) ->
                let withinTolerance =
                    if planned = 0m then
                        false
                    else
                        Math.Abs(executed - planned) / planned * 100m <= tolerancePercent

                if withinTolerance then (tp + planned, te + planned) else (tp + planned, te))
            (0m, 0m)

    if totalPlanned = 0m then 0m else (totalExecuted / totalPlanned) * 100m

// =============================================================================
// PI-DI-010: Service Level
// =============================================================================

let serviceLevel (fulfilledWithinWindow: decimal) (totalDemanded: decimal) : decimal =
    if totalDemanded = 0m then
        0m
    else
        (fulfilledWithinWindow / totalDemanded) * 100m

// =============================================================================
// PI-DI-011: Order Fill Rate
// =============================================================================

let orderFillRate (completelyFilledOrders: int) (totalOrders: int) : decimal =
    if totalOrders = 0 then
        0m
    else
        decimal completelyFilledOrders / decimal totalOrders * 100m

// =============================================================================
// PI-DI-012: On Time In Full (OTIF)
// =============================================================================

let otif (linesOTIF: int) (totalLines: int) : decimal =
    if totalLines = 0 then 0m else decimal linesOTIF / decimal totalLines * 100m

// =============================================================================
// PI-DI-013: Perfect Order Rate
// =============================================================================

let perfectOrderRate (perfectOrders: int) (totalOrders: int) : decimal =
    if totalOrders = 0 then
        0m
    else
        decimal perfectOrders / decimal totalOrders * 100m

// =============================================================================
// PI-DI-014: Customer Request Fulfilment Rate
// =============================================================================

let customerRequestFulfilmentRate (fulfilledRequests: int) (totalRequests: int) : decimal =
    if totalRequests = 0 then
        0m
    else
        decimal fulfilledRequests / decimal totalRequests * 100m

// =============================================================================
// PI-DI-015: Demand Satisfaction Rate
// =============================================================================

let demandSatisfactionRate (totalSatisfied: decimal) (totalDemanded: decimal) : decimal =
    if totalDemanded = 0m then 0m else (totalSatisfied / totalDemanded) * 100m
